using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Security;
using HelpDeskSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Api.Controllers;

public class NotificationsController : ApiControllerBase
{
    private readonly INotificationService _service;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(INotificationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await _service.GetMyNotificationsAsync(_currentUser);
        return FromResult(ApiResponse<IEnumerable<NotificationDto>>.Ok(result));
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _service.MarkAsReadAsync(id, _currentUser);
        return FromResult(ApiResponse<object?>.Ok(null, "Marked as read"));
    }
}
