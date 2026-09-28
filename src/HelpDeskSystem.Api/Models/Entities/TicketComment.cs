namespace HelpDeskSystem.Api.Models.Entities;

public class TicketComment
{
    public int CommentId { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
