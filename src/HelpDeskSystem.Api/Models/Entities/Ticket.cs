using HelpDeskSystem.Api.Common;

namespace HelpDeskSystem.Api.Models.Entities;

public class Ticket
{
    public int TicketId { get; private set; }
    public string TicketNumber { get; private set; } = string.Empty;
    public string ShortDescription { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public TicketImpact Impact { get; private set; }
    public TicketUrgency Urgency { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketStatus Status { get; private set; }

    public int CreatedByEmployeeId { get; private set; }
    public Employee? CreatedByEmployee { get; set; }

    public int? AssignedToEmployeeId { get; private set; }
    public Employee? AssignedToEmployee { get; set; }

    public int CategoryId { get; private set; }
    public Category? Category { get; set; }

    public int RequestTypeId { get; private set; }
    public RequestType? RequestType { get; set; }

    public int? AssetId { get; private set; }
    public Asset? Asset { get; set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    private Ticket() { } // EF Core

    // ---- FACTORY METHOD ----
    // The only way to construct a Ticket. Guarantees a ticket can never exist
    // half-initialized: TicketNumber, Status = New and Priority are always set together.
    public static Ticket Raise(
        int createdByEmployeeId,
        int categoryId,
        int requestTypeId,
        string shortDescription,
        string? description,
        TicketImpact impact,
        TicketUrgency urgency,
        int? assetId = null)
    {
        Guard.AgainstNullOrWhiteSpace(shortDescription, nameof(shortDescription));

        return new Ticket
        {
            ShortDescription = shortDescription,
            Description = description,
            Impact = impact,
            Urgency = urgency,
            Priority = DerivePriority(impact, urgency),
            Status = TicketStatus.New,
            CreatedByEmployeeId = createdByEmployeeId,
            CategoryId = categoryId,
            RequestTypeId = requestTypeId,
            AssetId = assetId,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static TicketPriority DerivePriority(TicketImpact impact, TicketUrgency urgency)
    {
        var score = (int)impact + (int)urgency;
        return score switch
        {
            <= 2 => TicketPriority.Low,
            3 => TicketPriority.Medium,
            4 => TicketPriority.High,
            _ => TicketPriority.Critical
        };
    }

    // Called once, right after the first SaveChanges, when TicketId is known.
    public void AssignTicketNumber()
    {
        if (!string.IsNullOrEmpty(TicketNumber)) return;
        TicketNumber = $"TCK-{TicketId:D5}";
    }

    // ---- Internal state-transition methods ----
    // Only the ITicketState implementations (Patterns/State) call these, so the
    // rules about which transition is legal from which status live in one place.
    internal void MarkAssigned(int employeeId)
    {
        AssignedToEmployeeId = employeeId;
        Status = TicketStatus.Assigned;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void MarkInProgress()
    {
        Status = TicketStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void MarkResolved()
    {
        Status = TicketStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void MarkClosed()
    {
        Status = TicketStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void MarkReopened()
    {
        Status = TicketStatus.Reopened;
        ClosedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
