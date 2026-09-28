namespace HelpDeskSystem.Api.Models.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // The department a ticket in this category is routed to (Section 6.1 in the LLD).
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? Description { get; set; }
}
