using HelpDeskSystem.Api.Models.Entities;

namespace HelpDeskSystem.Api.Patterns.Strategy;

public interface IAssignmentStrategy
{
    string Name { get; }
    Employee Assign(Ticket ticket, IReadOnlyList<Employee> departmentStaff);
}
