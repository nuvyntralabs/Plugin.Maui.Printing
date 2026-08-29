namespace Plugin.Maui.Printing;

/// <summary>
/// Outcome of <see cref="IPrinter.PrintAsync(PrintDocument, PrintOptions?, CancellationToken)"/>.
/// </summary>
public sealed record PrintResult
{
    /// <summary>
    /// Gets the classified status.
    /// </summary>
    public required PrintStatus Status { get; init; }

    /// <summary>
    /// Gets the payload type.
    /// </summary>
    public required PrintContentKind ContentKind { get; init; }

    /// <summary>
    /// Gets the business document kind.
    /// </summary>
    public PrintJobKind JobKind { get; init; }

    /// <summary>
    /// Gets the destination that was requested.
    /// </summary>
    public PrinterKind RequestedTarget { get; init; }

    /// <summary>
    /// Gets the destination that was actually used.
    /// </summary>
    public PrinterKind ResolvedTarget { get; init; }

    /// <summary>
    /// Gets the printer id when one was selected.
    /// </summary>
    public string? PrinterId { get; init; }

    /// <summary>
    /// Gets the printer display name when known.
    /// </summary>
    public string? PrinterName { get; init; }

    /// <summary>
    /// Gets a human-readable status message.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Whether the job was submitted or the printer accepted the payload.
    /// </summary>
    public bool Completed => Status == PrintStatus.Completed;

    /// <summary>
    /// Creates a completed result.
    /// </summary>
    public static PrintResult Success(PrintContentKind content, PrinterKind requested, PrinterKind resolved, PrintJobKind jobKind = PrintJobKind.Generic, string? printerId = null, string? printerName = null, string? message = null) =>
        new()
        {
            Status = PrintStatus.Completed,
            ContentKind = content,
            JobKind = jobKind,
            RequestedTarget = requested,
            ResolvedTarget = resolved,
            PrinterId = printerId,
            PrinterName = printerName,
            Message = message ?? "Printed"
        };

    /// <summary>
    /// Creates a cancelled result.
    /// </summary>
    public static PrintResult Cancel(PrintContentKind content, PrinterKind requested, PrinterKind resolved, PrintJobKind jobKind = PrintJobKind.Generic, string? message = null) =>
        new()
        {
            Status = PrintStatus.Cancelled,
            ContentKind = content,
            JobKind = jobKind,
            RequestedTarget = requested,
            ResolvedTarget = resolved,
            Message = message ?? "Cancelled"
        };

    /// <summary>
    /// Creates a printer-unavailable result.
    /// </summary>
    public static PrintResult Unavailable(PrintContentKind content, PrinterKind requested, PrinterKind resolved, PrintJobKind jobKind = PrintJobKind.Generic, string? message = null) =>
        new()
        {
            Status = PrintStatus.PrinterUnavailable,
            ContentKind = content,
            JobKind = jobKind,
            RequestedTarget = requested,
            ResolvedTarget = resolved,
            Message = message ?? "No matching printer is available."
        };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static PrintResult Fail(PrintContentKind content, PrinterKind requested, PrinterKind resolved, string message, PrintJobKind jobKind = PrintJobKind.Generic) =>
        new()
        {
            Status = PrintStatus.Failed,
            ContentKind = content,
            JobKind = jobKind,
            RequestedTarget = requested,
            ResolvedTarget = resolved,
            Message = message
        };
}
