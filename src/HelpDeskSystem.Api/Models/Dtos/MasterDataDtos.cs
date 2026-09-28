namespace HelpDeskSystem.Api.Models.Dtos;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateDepartmentDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string? Description { get; set; }
}

public class RequestTypeDto
{
    public int RequestTypeId { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateRequestTypeDto
{
    public string RequestTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AssetDto
{
    public int AssetId { get; set; }
    public string AssetNo { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? AssetType { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
}

public class CreateAssetDto
{
    public string AssetNo { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? AssetType { get; set; }
    public string? IPAddress { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
}
