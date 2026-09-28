using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Security;

namespace HelpDeskSystem.Api.Patterns.Policy;

public class TicketAccessPolicy : ITicketAccessPolicy
{
    public IQueryable<Ticket> ApplyScope(IQueryable<Ticket> query, ICurrentUserService user)
    {
        return user.Role switch
        {
            RoleNames.Admin => query,
            RoleNames.Hr or RoleNames.It => query.Where(t => t.Category!.DepartmentId == user.DepartmentId),
            _ => query.Where(t => t.CreatedByEmployeeId == user.EmployeeId)
        };
    }

    public bool CanManage(Ticket ticket, ICurrentUserService user)
    {
        if (user.Role == RoleNames.Admin) return true;
        if (user.Role is RoleNames.Hr or RoleNames.It)
            return ticket.Category?.DepartmentId == user.DepartmentId;
        return false; // a plain Employee never manages (assigns/resolves) a ticket
    }

    public bool CanView(Ticket ticket, ICurrentUserService user)
    {
        if (user.Role == RoleNames.Admin) return true;
        if (user.Role is RoleNames.Hr or RoleNames.It)
            return ticket.Category?.DepartmentId == user.DepartmentId;
        return ticket.CreatedByEmployeeId == user.EmployeeId
            || ticket.AssignedToEmployeeId == user.EmployeeId;
    }
}
