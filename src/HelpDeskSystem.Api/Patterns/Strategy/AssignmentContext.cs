using HelpDeskSystem.Api.Models.Entities;

namespace HelpDeskSystem.Api.Patterns.Strategy;

// Holds whichever IAssignmentStrategy is active and delegates to it.
// Registered as a Singleton in DI; swap the default via SetStrategy without
// touching TicketService or any controller.
public class AssignmentContext
{
    private IAssignmentStrategy _strategy;

    public AssignmentContext(IAssignmentStrategy initialStrategy)
    {
        _strategy = initialStrategy;
    }

    public string CurrentStrategyName => _strategy.Name;

    public void SetStrategy(IAssignmentStrategy strategy) => _strategy = strategy;

    public Employee PickAssignee(Ticket ticket, IReadOnlyList<Employee> departmentStaff) =>
        _strategy.Assign(ticket, departmentStaff);
}
