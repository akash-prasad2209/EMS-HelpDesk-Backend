using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Patterns.Factory;
using HelpDeskSystem.Api.Patterns.Observer;
using HelpDeskSystem.Api.Patterns.Policy;
using HelpDeskSystem.Api.Patterns.State;
using HelpDeskSystem.Api.Patterns.Strategy;
using HelpDeskSystem.Api.Repositories;
using HelpDeskSystem.Api.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskSystem.Api.Services;

public interface ITicketService
{
    Task<TicketDto> RaiseTicketAsync(ICurrentUserService user, CreateTicketDto dto);
    Task<IEnumerable<TicketDto>> GetMyTicketsAsync(ICurrentUserService user);
    Task<IEnumerable<TicketDto>> GetQueueAsync(ICurrentUserService user);
    Task<TicketDetailDto> GetByIdAsync(int ticketId, ICurrentUserService user);
    Task AssignTicketAsync(int ticketId, AssignTicketDto dto, ICurrentUserService user);
    Task ChangeStatusAsync(int ticketId, ChangeTicketStatusDto dto, ICurrentUserService user);
    Task ReopenAsync(int ticketId, ICurrentUserService user);
    Task<CommentDto> AddCommentAsync(int ticketId, CreateCommentDto dto, ICurrentUserService user);
    Task<IEnumerable<CommentDto>> GetCommentsAsync(int ticketId, ICurrentUserService user);
    Task<IEnumerable<TicketHistoryDto>> GetHistoryAsync(int ticketId, ICurrentUserService user);
    Task<AttachmentDto> AddAttachmentAsync(int ticketId, IFormFile file, ICurrentUserService user);
}

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _uow;
    private readonly TicketFactory _ticketFactory;
    private readonly AssignmentContext _assignmentContext;
    private readonly TicketEventPublisher _eventPublisher;
    private readonly ITicketAccessPolicy _accessPolicy;
    private readonly IWebHostEnvironment _env;

    public TicketService(
        IUnitOfWork uow,
        TicketFactory ticketFactory,
        AssignmentContext assignmentContext,
        TicketEventPublisher eventPublisher,
        ITicketAccessPolicy accessPolicy,
        IWebHostEnvironment env)
    {
        _uow = uow;
        _ticketFactory = ticketFactory;
        _assignmentContext = assignmentContext;
        _eventPublisher = eventPublisher;
        _accessPolicy = accessPolicy;
        _env = env;
    }

    public async Task<TicketDto> RaiseTicketAsync(ICurrentUserService user, CreateTicketDto dto)
    {
        // FACTORY
        var ticket = await _ticketFactory.CreateAsync(user.EmployeeId, dto);
        await _uow.Tickets.AddAsync(ticket);
        await _uow.SaveChangesAsync(); // TicketId is now known
        ticket.AssignTicketNumber();

        // OBSERVER - audit trail entry for creation. Saved together with the
        // TicketNumber update above in one SaveChangesAsync call (EF Core commits
        // every tracked change from a single call in one transaction).
        await _eventPublisher.PublishAsync(new TicketChangedEventArgs
        {
            TicketId = ticket.TicketId,
            ChangedByEmployeeId = user.EmployeeId,
            ActionType = "Created",
            NewValue = ticket.Status.ToString()
        });
        await _uow.SaveChangesAsync();

        var withDetails = await _uow.Tickets.GetWithDetailsAsync(ticket.TicketId);
        return ToDto(withDetails!);
    }

    public async Task<IEnumerable<TicketDto>> GetMyTicketsAsync(ICurrentUserService user)
    {
        var tickets = await _uow.Tickets.QueryWithDetails()
            .Where(t => t.CreatedByEmployeeId == user.EmployeeId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(ToDto);
    }

    public async Task<IEnumerable<TicketDto>> GetQueueAsync(ICurrentUserService user)
    {
        // POLICY - department-scoped queue (Admin sees all, HR/IT see their own department)
        var query = _accessPolicy.ApplyScope(_uow.Tickets.QueryWithDetails(), user);
        var tickets = await query.OrderBy(t => t.CreatedAt).ToListAsync();
        return tickets.Select(ToDto);
    }

    public async Task<TicketDetailDto> GetByIdAsync(int ticketId, ICurrentUserService user)
    {
        var ticket = await _uow.Tickets.GetWithDetailsAsync(ticketId)
            ?? throw new NotFoundException($"Ticket {ticketId} was not found.");

        if (!_accessPolicy.CanView(ticket, user))
            throw new NotFoundException($"Ticket {ticketId} was not found."); // 404, not 403 - don't leak existence

        return ToDetailDto(ticket);
    }

    public async Task AssignTicketAsync(int ticketId, AssignTicketDto dto, ICurrentUserService user)
    {
        var ticket = await _uow.Tickets.GetWithDetailsAsync(ticketId)
            ?? throw new NotFoundException($"Ticket {ticketId} was not found.");

        if (!_accessPolicy.CanManage(ticket, user))
            throw new NotFoundException($"Ticket {ticketId} was not found.");

        int assigneeId;
        if (dto.EmployeeId.HasValue)
        {
            assigneeId = dto.EmployeeId.Value;
        }
        else
        {
            // STRATEGY - auto-pick from the ticket's own department staff.
            // Pre-order by current open-ticket load so WorkloadStrategy (least-loaded)
            // and RoundRobinStrategy both receive a stable, meaningful ordering.
            var openStatuses = new[] { TicketStatus.Assigned, TicketStatus.InProgress, TicketStatus.Reopened };

            var departmentStaff = await _uow.Employees.Query()
                .Where(e => e.DepartmentId == ticket.Category!.DepartmentId && e.IsActive)
                .Select(e => new
                {
                    Employee = e,
                    OpenCount = _uow.Tickets.Query().Count(t => t.AssignedToEmployeeId == e.EmployeeId && openStatuses.Contains(t.Status))
                })
                .OrderBy(x => x.OpenCount)
                .Select(x => x.Employee)
                .ToListAsync();

            var assignee = _assignmentContext.PickAssignee(ticket, departmentStaff);
            assigneeId = assignee.EmployeeId;
        }

        var oldStatus = ticket.Status.ToString();

        // STATE - only the current state decides whether "assign" is legal right now
        var state = TicketStateFactory.For(ticket.Status);
        state.Assign(ticket, assigneeId);

        // OBSERVER
        await _eventPublisher.PublishAsync(new TicketChangedEventArgs
        {
            TicketId = ticket.TicketId,
            ChangedByEmployeeId = user.EmployeeId,
            RecipientEmployeeId = ticket.CreatedByEmployeeId,
            ActionType = "Assigned",
            FieldName = "Status",
            OldValue = oldStatus,
            NewValue = ticket.Status.ToString()
        });

        await _uow.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(int ticketId, ChangeTicketStatusDto dto, ICurrentUserService user)
    {
        var ticket = await _uow.Tickets.GetWithDetailsAsync(ticketId)
            ?? throw new NotFoundException($"Ticket {ticketId} was not found.");

        if (!_accessPolicy.CanManage(ticket, user))
            throw new NotFoundException($"Ticket {ticketId} was not found.");

        var oldStatus = ticket.Status.ToString();
        var state = TicketStateFactory.For(ticket.Status);

        // STATE - the transition method itself enforces which moves are legal
        switch (dto.NewStatus)
        {
            case TicketStatus.InProgress: state.Start(ticket); break;
            case TicketStatus.Resolved: state.Resolve(ticket); break;
            case TicketStatus.Closed: state.Close(ticket); break;
            default: throw new BusinessRuleException($"Cannot transition directly to {dto.NewStatus}.");
        }

        await _eventPublisher.PublishAsync(new TicketChangedEventArgs
        {
            TicketId = ticket.TicketId,
            ChangedByEmployeeId = user.EmployeeId,
            RecipientEmployeeId = ticket.CreatedByEmployeeId,
            ActionType = "StatusChanged",
            FieldName = "Status",
            OldValue = oldStatus,
            NewValue = ticket.Status.ToString()
        });

        await _uow.SaveChangesAsync();
    }

    public async Task ReopenAsync(int ticketId, ICurrentUserService user)
    {
        var ticket = await _uow.Tickets.GetWithDetailsAsync(ticketId)
            ?? throw new NotFoundException($"Ticket {ticketId} was not found.");

        // Only the creator may reopen their own ticket (Section 4 access matrix)
        if (ticket.CreatedByEmployeeId != user.EmployeeId)
            throw new NotFoundException($"Ticket {ticketId} was not found.");

        var oldStatus = ticket.Status.ToString();
        var state = TicketStateFactory.For(ticket.Status);
        state.Reopen(ticket);

        await _eventPublisher.PublishAsync(new TicketChangedEventArgs
        {
            TicketId = ticket.TicketId,
            ChangedByEmployeeId = user.EmployeeId,
            RecipientEmployeeId = ticket.AssignedToEmployeeId,
            ActionType = "Reopened",
            FieldName = "Status",
            OldValue = oldStatus,
            NewValue = ticket.Status.ToString()
        });

        await _uow.SaveChangesAsync();
    }

    public async Task<CommentDto> AddCommentAsync(int ticketId, CreateCommentDto dto, ICurrentUserService user)
    {
        var ticket = await _uow.Tickets.GetWithDetailsAsync(ticketId)
            ?? throw new NotFoundException($"Ticket {ticketId} was not found.");

        if (!_accessPolicy.CanView(ticket, user))
            throw new NotFoundException($"Ticket {ticketId} was not found.");

        var comment = new TicketComment
        {
            TicketId = ticketId,
            EmployeeId = user.EmployeeId,
            CommentText = dto.CommentText,
            CreatedAt = DateTime.UtcNow
        };
        await _uow.TicketComments.AddAsync(comment);

        var recipient = user.EmployeeId == ticket.CreatedByEmployeeId
            ? ticket.AssignedToEmployeeId
            : ticket.CreatedByEmployeeId;

        await _eventPublisher.PublishAsync(new TicketChangedEventArgs
        {
            TicketId = ticketId,
            ChangedByEmployeeId = user.EmployeeId,
            RecipientEmployeeId = recipient,
            ActionType = "CommentAdded"
        });

        await _uow.SaveChangesAsync();

        var employee = await _uow.Employees.GetByIdAsync(user.EmployeeId);
        return new CommentDto
        {
            CommentId = comment.CommentId,
            EmployeeName = employee?.FullName ?? string.Empty,
            CommentText = comment.CommentText,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsAsync(int ticketId, ICurrentUserService user)
    {
        var ticket = await _uow.Tickets.GetWithDetailsAsync(ticketId)
            ?? throw new NotFoundException($"Ticket {ticketId} was not found.");

        if (!_accessPolicy.CanView(ticket, user))
            throw new NotFoundException($"Ticket {ticketId} was not found.");

        var comments = await _uow.TicketComments.Query()
            .Include(c => c.Employee)
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => new CommentDto
        {
            CommentId = c.CommentId,
            EmployeeName = c.Employee?.FullName ?? string.Empty,
            CommentText = c.CommentText,
            CreatedAt = c.CreatedAt
        });
    }

    public async Task<IEnumerable<TicketHistoryDto>> GetHistoryAsync(int ticketId, ICurrentUserService user)
    {
        var ticket = await _uow.Tickets.GetWithDetailsAsync(ticketId)
            ?? throw new NotFoundException($"Ticket {ticketId} was not found.");

        if (!_accessPolicy.CanView(ticket, user))
            throw new NotFoundException($"Ticket {ticketId} was not found.");

        var history = await _uow.TicketHistories.Query()
            .Include(h => h.ChangedByEmployee)
            .Where(h => h.TicketId == ticketId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync();

        return history.Select(h => new TicketHistoryDto
        {
            ActionType = h.ActionType,
            FieldName = h.FieldName,
            OldValue = h.OldValue,
            NewValue = h.NewValue,
            ChangedByName = h.ChangedByEmployee?.FullName ?? string.Empty,
            ChangedAt = h.ChangedAt
        });
    }

    public async Task<AttachmentDto> AddAttachmentAsync(int ticketId, IFormFile file, ICurrentUserService user)
    {
        var ticket = await _uow.Tickets.GetWithDetailsAsync(ticketId)
            ?? throw new NotFoundException($"Ticket {ticketId} was not found.");

        if (!_accessPolicy.CanView(ticket, user))
            throw new NotFoundException($"Ticket {ticketId} was not found.");

        var uploadsRoot = Path.Combine(_env.ContentRootPath, "Uploads", ticketId.ToString());
        Directory.CreateDirectory(uploadsRoot);
        var storedFileName = $"{Guid.NewGuid():N}_{file.FileName}";
        var fullPath = Path.Combine(uploadsRoot, storedFileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var attachment = new TicketAttachment
        {
            TicketId = ticketId,
            FileName = file.FileName,
            FilePath = fullPath,
            FileType = file.ContentType,
            FileSize = file.Length,
            UploadedByEmployeeId = user.EmployeeId,
            UploadedAt = DateTime.UtcNow
        };
        await _uow.TicketAttachments.AddAsync(attachment);

        await _eventPublisher.PublishAsync(new TicketChangedEventArgs
        {
            TicketId = ticketId,
            ChangedByEmployeeId = user.EmployeeId,
            RecipientEmployeeId = user.EmployeeId == ticket.CreatedByEmployeeId
                ? ticket.AssignedToEmployeeId
                : ticket.CreatedByEmployeeId,
            ActionType = "AttachmentAdded",
            NewValue = file.FileName
        });

        await _uow.SaveChangesAsync();

        return new AttachmentDto
        {
            AttachmentId = attachment.AttachmentId,
            FileName = attachment.FileName,
            FileType = attachment.FileType,
            FileSize = attachment.FileSize,
            UploadedAt = attachment.UploadedAt
        };
    }

    private static TicketDto ToDto(Ticket t) => new()
    {
        TicketId = t.TicketId,
        TicketNumber = t.TicketNumber,
        ShortDescription = t.ShortDescription,
        Priority = t.Priority.ToString(),
        Status = t.Status.ToString(),
        CategoryName = t.Category?.CategoryName ?? string.Empty,
        AssignedToName = t.AssignedToEmployee?.FullName,
        CreatedAt = t.CreatedAt
    };

    private static TicketDetailDto ToDetailDto(Ticket t) => new()
    {
        TicketId = t.TicketId,
        TicketNumber = t.TicketNumber,
        ShortDescription = t.ShortDescription,
        Description = t.Description,
        Priority = t.Priority.ToString(),
        Status = t.Status.ToString(),
        CategoryName = t.Category?.CategoryName ?? string.Empty,
        RequestTypeName = t.RequestType?.RequestTypeName ?? string.Empty,
        AssignedToName = t.AssignedToEmployee?.FullName,
        CreatedByName = t.CreatedByEmployee?.FullName ?? string.Empty,
        CreatedAt = t.CreatedAt,
        ResolvedAt = t.ResolvedAt,
        ClosedAt = t.ClosedAt
    };
}
