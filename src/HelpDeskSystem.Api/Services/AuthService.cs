using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Repositories;
using HelpDeskSystem.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskSystem.Api.Services;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginRequestDto dto);
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IUnitOfWork uow, IJwtTokenService jwtTokenService)
    {
        _uow = uow;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto dto)
    {
        var employee = await _uow.Employees.Query()
            .Include(e => e.Role)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Email == dto.Email && e.IsActive);

        if (employee is null || !BCrypt.Net.BCrypt.Verify(dto.Password, employee.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _jwtTokenService.GenerateToken(employee);

        return new AuthResultDto
        {
            Token = token,
            FullName = employee.FullName,
            Role = employee.Role?.RoleName ?? string.Empty,
            Department = employee.Department?.DepartmentName
        };
    }
}
