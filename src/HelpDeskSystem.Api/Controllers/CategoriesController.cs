using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Api.Controllers;

public class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _service;
    public CategoriesController(ICategoryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return FromResult(ApiResponse<IEnumerable<CategoryDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return FromResult(ApiResponse<CategoryDto>.Ok(result, "Category created", 201));
    }
}
