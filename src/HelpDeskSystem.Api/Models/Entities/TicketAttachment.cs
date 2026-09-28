namespace HelpDeskSystem.Api.Models.Entities;

public class TicketAttachment
{
    public int AttachmentId { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public int UploadedByEmployeeId { get; set; }
    public Employee? UploadedByEmployee { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
