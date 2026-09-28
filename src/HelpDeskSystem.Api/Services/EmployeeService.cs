using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskSystem.Api.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
}

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _uow;
    public EmployeeService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        var employees = await _uow.Employees.Query()
            .Include(e => e.Role)
            .Include(e => e.Department)
            .ToListAsync();

        return employees.Select(ToDto);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        if (await _uow.Employees.Query().AnyAsync(e => e.Email == dto.Email))
            throw new BusinessRuleException($"Email {dto.Email} is already in use.");
        if (await _uow.Employees.Query().AnyAsync(e => e.EmployeeCode == dto.EmployeeCode))
            throw new BusinessRuleException($"Employee code {dto.EmployeeCode} is already in use.");

        var employee = new Employee
        {
            EmployeeCode = dto.EmployeeCode,
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = dto.RoleId,
            DepartmentId = dto.DepartmentId,
            IsActive = true
        };
        await _uow.Employees.AddAsync(employee);
        await _uow.SaveChangesAsync();

        var saved = await _uow.Employees.Query()
            .Include(e => e.Role).Include(e => e.Department)
            .FirstAsync(e => e.EmployeeId == employee.EmployeeId);
        return ToDto(saved);
    }

    private static EmployeeDto ToDto(Employee e) => new()
    {
        EmployeeId = e.EmployeeId,
        EmployeeCode = e.EmployeeCode,
        FullName = e.FullName,
        Email = e.Email,
        RoleName = e.Role?.RoleName ?? string.Empty,
        DepartmentName = e.Department?.DepartmentName,
        IsActive = e.IsActive
    };
}
