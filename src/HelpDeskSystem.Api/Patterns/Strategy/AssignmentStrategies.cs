using HelpDeskSystem.Api.Models.Entities;

namespace HelpDeskSystem.Api.Patterns.Strategy;

// Least-loaded needs an open-ticket count per employee; we approximate it by
// counting currently-assigned, not-yet-closed tickets passed in by the caller.
// To keep the strategy pure/stateless it accepts staff pre-ordered by load,
// so TicketService is responsible for the query, the strategy just picks.

public class RoundRobinStrategy : IAssignmentStrategy
{
    private int _lastIndex = -1;
    public string Name => "RoundRobin";

    public Employee Assign(Ticket ticket, IReadOnlyList<Employee> departmentStaff)
    {
        if (departmentStaff.Count == 0)
            throw new InvalidOperationException("No staff available in this department to assign the ticket to.");

        _lastIndex = (_lastIndex + 1) % departmentStaff.Count;
        return departmentStaff[_lastIndex];
    }
}

public class WorkloadStrategy : IAssignmentStrategy
{
    public string Name => "LeastLoaded";

    // Expects departmentStaff already ordered by current open-ticket count (ascending)
    // by the caller (TicketService), since that count lives in the Ticket table, not on Employee.
    public Employee Assign(Ticket ticket, IReadOnlyList<Employee> departmentStaff)
    {
        if (departmentStaff.Count == 0)
            throw new InvalidOperationException("No staff available in this department to assign the ticket to.");

        return departmentStaff[0];
    }
}
