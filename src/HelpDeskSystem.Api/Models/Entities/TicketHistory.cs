namespace HelpDeskSystem.Api.Models.Entities;

public class TicketHistory
{
    public int TicketHistoryId { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public int ChangedByEmployeeId { get; set; }
    public Employee? ChangedByEmployee { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
