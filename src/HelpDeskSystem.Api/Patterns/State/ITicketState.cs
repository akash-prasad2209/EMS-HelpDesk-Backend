using HelpDeskSystem.Api.Models.Entities;

namespace HelpDeskSystem.Api.Patterns.State;

public interface ITicketState
{
    TicketStatus Status { get; }
    void Assign(Ticket ticket, int employeeId);
    void Start(Ticket ticket);
    void Resolve(Ticket ticket);
    void Close(Ticket ticket);
    void Reopen(Ticket ticket);
}
