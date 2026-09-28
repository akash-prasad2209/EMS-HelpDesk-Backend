using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Security;

namespace HelpDeskSystem.Api.Patterns.Policy;

public interface ITicketAccessPolicy
{
    // Filters an already-built IQueryable<Ticket> down to what this user may see.
    // Every ticket query (queue list, ticket detail, comments, history) goes
    // through this one method - Section 4 of the LLD, implemented once.
    IQueryable<Ticket> ApplyScope(IQueryable<Ticket> query, ICurrentUserService user);

    // Whether this user may act on (assign/change status of) this specific ticket.
    bool CanManage(Ticket ticket, ICurrentUserService user);

    // Whether this user may view this specific ticket (comments/history/detail).
    bool CanView(Ticket ticket, ICurrentUserService user);
}
