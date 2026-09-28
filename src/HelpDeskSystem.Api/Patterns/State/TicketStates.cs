using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Entities;

namespace HelpDeskSystem.Api.Patterns.State;

public class NewState : ITicketState
{
    public TicketStatus Status => TicketStatus.New;
    public void Assign(Ticket ticket, int employeeId) => ticket.MarkAssigned(employeeId);
    public void Start(Ticket ticket) => throw new BusinessRuleException("Assign the ticket before starting work on it.");
    public void Resolve(Ticket ticket) => throw new BusinessRuleException("Cannot resolve an unassigned ticket.");
    public void Close(Ticket ticket) => throw new BusinessRuleException("Cannot close a new ticket.");
    public void Reopen(Ticket ticket) => throw new BusinessRuleException("Ticket is already open.");
}

public class AssignedState : ITicketState
{
    public TicketStatus Status => TicketStatus.Assigned;
    public void Assign(Ticket ticket, int employeeId) => ticket.MarkAssigned(employeeId); // reassign
    public void Start(Ticket ticket) => ticket.MarkInProgress();
    public void Resolve(Ticket ticket) => throw new BusinessRuleException("Start work on the ticket before resolving it.");
    public void Close(Ticket ticket) => throw new BusinessRuleException("Resolve the ticket before closing it.");
    public void Reopen(Ticket ticket) => throw new BusinessRuleException("Ticket is already open.");
}

public class InProgressState : ITicketState
{
    public TicketStatus Status => TicketStatus.InProgress;
    public void Assign(Ticket ticket, int employeeId) => ticket.MarkAssigned(employeeId); // reassign
    public void Start(Ticket ticket) { /* already in progress */ }
    public void Resolve(Ticket ticket) => ticket.MarkResolved();
    public void Close(Ticket ticket) => throw new BusinessRuleException("Resolve the ticket before closing it.");
    public void Reopen(Ticket ticket) => throw new BusinessRuleException("Ticket is already open.");
}

public class ResolvedState : ITicketState
{
    public TicketStatus Status => TicketStatus.Resolved;
    public void Assign(Ticket ticket, int employeeId) => ticket.MarkAssigned(employeeId); // reopens implicitly to Assigned
    public void Start(Ticket ticket) => ticket.MarkInProgress();
    public void Resolve(Ticket ticket) { /* already resolved */ }
    public void Close(Ticket ticket) => ticket.MarkClosed();
    public void Reopen(Ticket ticket) => ticket.MarkReopened();
}

public class ClosedState : ITicketState
{
    public TicketStatus Status => TicketStatus.Closed;
    public void Assign(Ticket ticket, int employeeId) => throw new BusinessRuleException("Ticket is closed. Reopen it first.");
    public void Start(Ticket ticket) => throw new BusinessRuleException("Ticket is closed. Reopen it first.");
    public void Resolve(Ticket ticket) => throw new BusinessRuleException("Ticket is closed.");
    public void Close(Ticket ticket) { /* already closed */ }
    public void Reopen(Ticket ticket) => ticket.MarkReopened();
}

public class ReopenedState : ITicketState
{
    public TicketStatus Status => TicketStatus.Reopened;
    public void Assign(Ticket ticket, int employeeId) => ticket.MarkAssigned(employeeId);
    public void Start(Ticket ticket) => ticket.MarkInProgress();
    public void Resolve(Ticket ticket) => throw new BusinessRuleException("Assign and start work before resolving.");
    public void Close(Ticket ticket) => throw new BusinessRuleException("Resolve the ticket before closing it.");
    public void Reopen(Ticket ticket) { /* already reopened */ }
}

public static class TicketStateFactory
{
    public static ITicketState For(TicketStatus status) => status switch
    {
        TicketStatus.New => new NewState(),
        TicketStatus.Assigned => new AssignedState(),
        TicketStatus.InProgress => new InProgressState(),
        TicketStatus.Resolved => new ResolvedState(),
        TicketStatus.Closed => new ClosedState(),
        TicketStatus.Reopened => new ReopenedState(),
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };
}
