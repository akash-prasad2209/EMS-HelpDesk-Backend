using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Api.Controllers;

public class RequestTypesController : ApiControllerBase
{
    private readonly IRequestTypeService _service;
    public RequestTypesController(IRequestTypeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return FromResult(ApiResponse<IEnumerable<RequestTypeDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Create(CreateRequestTypeDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return FromResult(ApiResponse<RequestTypeDto>.Ok(result, "Request type created", 201));
    }
}
