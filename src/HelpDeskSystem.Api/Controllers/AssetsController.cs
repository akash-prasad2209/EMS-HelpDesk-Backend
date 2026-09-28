using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Api.Controllers;

public class AssetsController : ApiControllerBase
{
    private readonly IAssetService _service;
    public AssetsController(IAssetService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return FromResult(ApiResponse<IEnumerable<AssetDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.It}")]
    public async Task<IActionResult> Create(CreateAssetDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return FromResult(ApiResponse<AssetDto>.Ok(result, "Asset created", 201));
    }
}
