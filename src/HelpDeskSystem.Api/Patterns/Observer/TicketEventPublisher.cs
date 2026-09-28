namespace HelpDeskSystem.Api.Patterns.Observer;

// Registered as Scoped in DI with both observers injected, so publishing one
// event writes both the TicketHistory row and the Notification row without
// TicketService knowing either table exists.
public class TicketEventPublisher
{
    private readonly IEnumerable<ITicketObserver> _observers;

    public TicketEventPublisher(IEnumerable<ITicketObserver> observers)
    {
        _observers = observers;
    }

    public async Task PublishAsync(TicketChangedEventArgs e)
    {
        foreach (var observer in _observers)
            await observer.OnTicketChangedAsync(e);
    }
}
