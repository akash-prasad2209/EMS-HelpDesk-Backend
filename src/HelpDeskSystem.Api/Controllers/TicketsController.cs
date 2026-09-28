using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Security;
using HelpDeskSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Api.Controllers;

// No explicit [Route] here - ApiControllerBase already supplies "api/v1/[controller]",
// which resolves to "api/v1/Tickets" and matches "/tickets" case-insensitively.
// (A second explicit [Route("api/v1/tickets")] here previously caused every action to be
// reachable via two near-identical templates, which throws AmbiguousMatchException at
// request time - do not re-add a class-level [Route] to this controller.)
public class TicketsController : ApiControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ICurrentUserService _currentUser;

    public TicketsController(ITicketService ticketService, ICurrentUserService currentUser)
    {
        _ticketService = ticketService;
        _currentUser = currentUser;
    }

    // Any authenticated role may raise a ticket for themselves.
    [HttpPost]
    public async Task<IActionResult> Raise(CreateTicketDto dto)
    {
        var result = await _ticketService.RaiseTicketAsync(_currentUser, dto);
        return FromResult(ApiResponse<TicketDto>.Ok(result, "Ticket raised", 201));
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await _ticketService.GetMyTicketsAsync(_currentUser);
        return FromResult(ApiResponse<IEnumerable<TicketDto>>.Ok(result));
    }

    [HttpGet("queue")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Hr},{RoleNames.It}")]
    public async Task<IActionResult> GetQueue()
    {
        var result = await _ticketService.GetQueueAsync(_currentUser);
        return FromResult(ApiResponse<IEnumerable<TicketDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _ticketService.GetByIdAsync(id, _currentUser);
        return FromResult(ApiResponse<TicketDetailDto>.Ok(result));
    }

    [HttpPut("{id:int}/assign")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Hr},{RoleNames.It}")]
    public async Task<IActionResult> Assign(int id, AssignTicketDto dto)
    {
        await _ticketService.AssignTicketAsync(id, dto, _currentUser);
        return FromResult(ApiResponse<object?>.Ok(null, "Ticket assigned"));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Hr},{RoleNames.It}")]
    public async Task<IActionResult> ChangeStatus(int id, ChangeTicketStatusDto dto)
    {
        await _ticketService.ChangeStatusAsync(id, dto, _currentUser);
        return FromResult(ApiResponse<object?>.Ok(null, "Ticket status updated"));
    }

    [HttpPut("{id:int}/reopen")]
    public async Task<IActionResult> Reopen(int id)
    {
        await _ticketService.ReopenAsync(id, _currentUser);
        return FromResult(ApiResponse<object?>.Ok(null, "Ticket reopened"));
    }

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, CreateCommentDto dto)
    {
        var result = await _ticketService.AddCommentAsync(id, dto, _currentUser);
        return FromResult(ApiResponse<CommentDto>.Ok(result, "Comment added", 201));
    }

    [HttpGet("{id:int}/comments")]
    public async Task<IActionResult> GetComments(int id)
    {
        var result = await _ticketService.GetCommentsAsync(id, _currentUser);
        return FromResult(ApiResponse<IEnumerable<CommentDto>>.Ok(result));
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var result = await _ticketService.GetHistoryAsync(id, _currentUser);
        return FromResult(ApiResponse<IEnumerable<TicketHistoryDto>>.Ok(result));
    }

    [HttpPost("{id:int}/attachments")]
    public async Task<IActionResult> AddAttachment(int id, IFormFile file)
    {
        var result = await _ticketService.AddAttachmentAsync(id, file, _currentUser);
        return FromResult(ApiResponse<AttachmentDto>.Ok(result, "Attachment uploaded", 201));
    }
}
