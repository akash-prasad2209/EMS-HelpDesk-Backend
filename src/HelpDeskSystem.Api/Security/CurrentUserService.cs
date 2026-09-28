using System.Security.Claims;

namespace HelpDeskSystem.Api.Security;

public interface ICurrentUserService
{
    int EmployeeId { get; }
    string Role { get; }
    int? DepartmentId { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int EmployeeId =>
        int.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public string Role => User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public int? DepartmentId =>
        int.TryParse(User?.FindFirstValue("departmentId"), out var id) ? id : null;
}
