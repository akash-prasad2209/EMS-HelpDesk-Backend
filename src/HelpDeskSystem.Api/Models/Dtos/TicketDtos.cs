using HelpDeskSystem.Api.Models.Entities;

namespace HelpDeskSystem.Api.Models.Dtos;

public class CreateTicketDto
{
    public int CategoryId { get; set; }
    public int RequestTypeId { get; set; }
    public string ShortDescription { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketImpact Impact { get; set; }
    public TicketUrgency Urgency { get; set; }
    public int? AssetId { get; set; }
}

public class AssignTicketDto
{
    // Optional: if omitted, the current assignment strategy picks the assignee
    // from the ticket's owning department.
    public int? EmployeeId { get; set; }
}

public class ChangeTicketStatusDto
{
    public TicketStatus NewStatus { get; set; }
}

public class TicketDto
{
    public int TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TicketDetailDto : TicketDto
{
    public string? Description { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
