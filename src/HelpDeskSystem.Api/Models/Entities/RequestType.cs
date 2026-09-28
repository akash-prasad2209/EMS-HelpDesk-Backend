namespace HelpDeskSystem.Api.Models.Entities;

public class RequestType
{
    public int RequestTypeId { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
