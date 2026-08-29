namespace Plugin.Maui.Printing;

/// <summary>
/// Event, parking, or boarding ticket.
/// </summary>
public sealed class TicketDocument
{
    /// <summary>
    /// Gets the event or route name.
    /// </summary>
    public required string EventName { get; init; }

    /// <summary>
    /// Gets the ticket number.
    /// </summary>
    public string? TicketNumber { get; init; }

    /// <summary>
    /// Gets when the event starts.
    /// </summary>
    public DateTimeOffset? StartsAt { get; init; }

    /// <summary>
    /// Gets the seat, gate, or bay.
    /// </summary>
    public string? Seat { get; init; }

    /// <summary>
    /// Gets the holder name.
    /// </summary>
    public string? HolderName { get; init; }

    /// <summary>
    /// Gets a verification QR payload.
    /// </summary>
    public string? QrPayload { get; init; }

    /// <summary>
    /// Gets extra rows.
    /// </summary>
    public IReadOnlyDictionary<string, string> Fields { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Gets the job name.
    /// </summary>
    public string JobName => string.IsNullOrWhiteSpace(TicketNumber)
        ? EventName
        : $"Ticket {TicketNumber}";
}
