using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskSystem.Api.Services;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
}

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _uow;
    public DepartmentService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync() =>
        (await _uow.Departments.Query().ToListAsync())
            .Select(d => new DepartmentDto { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName, Description = d.Description });

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var department = new Department { DepartmentName = dto.DepartmentName, Description = dto.Description };
        await _uow.Departments.AddAsync(department);
        await _uow.SaveChangesAsync();
        return new DepartmentDto { DepartmentId = department.DepartmentId, DepartmentName = department.DepartmentName, Description = department.Description };
    }
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
}

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    public CategoryService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<CategoryDto>> GetAllAsync() =>
        (await _uow.Categories.Query().Include(c => c.Department).ToListAsync())
            .Select(ToDto);

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        _ = await _uow.Departments.GetByIdAsync(dto.DepartmentId)
            ?? throw new NotFoundException($"Department {dto.DepartmentId} was not found.");

        var category = new Category { CategoryName = dto.CategoryName, DepartmentId = dto.DepartmentId, Description = dto.Description };
        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();

        var saved = await _uow.Categories.Query().Include(c => c.Department).FirstAsync(c => c.CategoryId == category.CategoryId);
        return ToDto(saved);
    }

    private static CategoryDto ToDto(Category c) => new()
    {
        CategoryId = c.CategoryId,
        CategoryName = c.CategoryName,
        DepartmentId = c.DepartmentId,
        DepartmentName = c.Department?.DepartmentName ?? string.Empty,
        Description = c.Description
    };
}

public interface IRequestTypeService
{
    Task<IEnumerable<RequestTypeDto>> GetAllAsync();
    Task<RequestTypeDto> CreateAsync(CreateRequestTypeDto dto);
}

public class RequestTypeService : IRequestTypeService
{
    private readonly IUnitOfWork _uow;
    public RequestTypeService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<RequestTypeDto>> GetAllAsync() =>
        (await _uow.RequestTypes.Query().ToListAsync())
            .Select(r => new RequestTypeDto { RequestTypeId = r.RequestTypeId, RequestTypeName = r.RequestTypeName, Description = r.Description });

    public async Task<RequestTypeDto> CreateAsync(CreateRequestTypeDto dto)
    {
        var requestType = new RequestType { RequestTypeName = dto.RequestTypeName, Description = dto.Description };
        await _uow.RequestTypes.AddAsync(requestType);
        await _uow.SaveChangesAsync();
        return new RequestTypeDto { RequestTypeId = requestType.RequestTypeId, RequestTypeName = requestType.RequestTypeName, Description = requestType.Description };
    }
}

public interface IAssetService
{
    Task<IEnumerable<AssetDto>> GetAllAsync();
    Task<AssetDto> CreateAsync(CreateAssetDto dto);
}

public class AssetService : IAssetService
{
    private readonly IUnitOfWork _uow;
    public AssetService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<AssetDto>> GetAllAsync() =>
        (await _uow.Assets.Query().ToListAsync()).Select(ToDto);

    public async Task<AssetDto> CreateAsync(CreateAssetDto dto)
    {
        var asset = new Asset
        {
            AssetNo = dto.AssetNo,
            AssetName = dto.AssetName,
            AssetType = dto.AssetType,
            IPAddress = dto.IPAddress,
            Location = dto.Location,
            Status = dto.Status
        };
        await _uow.Assets.AddAsync(asset);
        await _uow.SaveChangesAsync();
        return ToDto(asset);
    }

    private static AssetDto ToDto(Asset a) => new()
    {
        AssetId = a.AssetId,
        AssetNo = a.AssetNo,
        AssetName = a.AssetName,
        AssetType = a.AssetType,
        Location = a.Location,
        Status = a.Status
    };
}
