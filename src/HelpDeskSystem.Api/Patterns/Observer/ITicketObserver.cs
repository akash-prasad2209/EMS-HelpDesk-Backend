namespace HelpDeskSystem.Api.Patterns.Observer;

public class TicketChangedEventArgs
{
    public int TicketId { get; init; }
    public int ChangedByEmployeeId { get; init; }
    public int? RecipientEmployeeId { get; init; } // who should be notified (usually the ticket creator)
    public string ActionType { get; init; } = string.Empty;
    public string? FieldName { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}

public interface ITicketObserver
{
    Task OnTicketChangedAsync(TicketChangedEventArgs e);
}
