namespace HelpDeskSystem.Api.Models.Entities;

public class Notification
{
    public int NotificationId { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public int EmployeeId { get; set; } // recipient
    public Employee? Employee { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
