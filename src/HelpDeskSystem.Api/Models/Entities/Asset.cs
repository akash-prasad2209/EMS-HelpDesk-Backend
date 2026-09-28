namespace HelpDeskSystem.Api.Models.Entities;

public class Asset
{
    public int AssetId { get; set; }
    public string AssetNo { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? AssetType { get; set; }
    public string? IPAddress { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
}
