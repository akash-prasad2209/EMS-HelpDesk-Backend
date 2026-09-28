using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Repositories;

namespace HelpDeskSystem.Api.Patterns.Observer;

public class TicketHistoryObserver : ITicketObserver
{
    private readonly IUnitOfWork _uow;

    public TicketHistoryObserver(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task OnTicketChangedAsync(TicketChangedEventArgs e)
    {
        await _uow.TicketHistories.AddAsync(new TicketHistory
        {
            TicketId = e.TicketId,
            ChangedByEmployeeId = e.ChangedByEmployeeId,
            ActionType = e.ActionType,
            FieldName = e.FieldName,
            OldValue = e.OldValue,
            NewValue = e.NewValue,
            ChangedAt = DateTime.UtcNow
        });
    }
}

public class NotificationObserver : ITicketObserver
{
    private readonly IUnitOfWork _uow;

    public NotificationObserver(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task OnTicketChangedAsync(TicketChangedEventArgs e)
    {
        if (e.RecipientEmployeeId is null) return;

        await _uow.Notifications.AddAsync(new Notification
        {
            TicketId = e.TicketId,
            EmployeeId = e.RecipientEmployeeId.Value,
            Title = "Ticket Updated",
            Message = $"{e.ActionType}" + (e.NewValue is not null ? $": {e.OldValue} -> {e.NewValue}" : string.Empty),
            CreatedAt = DateTime.UtcNow
        });
    }
}
