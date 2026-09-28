using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Api.Common;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult<T>(ApiResponse<T> response) => StatusCode(response.StatusCode, response);
}
