using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Api.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class EmployeesController : ApiControllerBase
{
    private readonly IEmployeeService _service;
    public EmployeesController(IEmployeeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return FromResult(ApiResponse<IEnumerable<EmployeeDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return FromResult(ApiResponse<EmployeeDto>.Ok(result, "Employee created", 201));
    }
}
