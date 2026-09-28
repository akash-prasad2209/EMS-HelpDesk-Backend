using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Repositories;
using HelpDeskSystem.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskSystem.Api.Services;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(ICurrentUserService user);
    Task MarkAsReadAsync(int notificationId, ICurrentUserService user);
}

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    public NotificationService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(ICurrentUserService user)
    {
        var notifications = await _uow.Notifications.Query()
            .Where(n => n.EmployeeId == user.EmployeeId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(n => new NotificationDto
        {
            NotificationId = n.NotificationId,
            TicketId = n.TicketId,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task MarkAsReadAsync(int notificationId, ICurrentUserService user)
    {
        var notification = await _uow.Notifications.GetByIdAsync(notificationId)
            ?? throw new NotFoundException($"Notification {notificationId} was not found.");

        if (notification.EmployeeId != user.EmployeeId)
            throw new NotFoundException($"Notification {notificationId} was not found.");

        notification.IsRead = true;
        await _uow.SaveChangesAsync();
    }
}
