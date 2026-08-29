namespace Plugin.Maui.Printing;

/// <summary>
/// A discovered system, Bluetooth, or thermal printer.
/// </summary>
public sealed class PrinterInfo
{
    /// <summary>
    /// Gets a stable id (Android MAC, iOS peripheral UUID, or <c>system</c>).
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the transport.
    /// </summary>
    public required PrinterKind Kind { get; init; }

    /// <summary>
    /// Gets the Bluetooth address when the printer is wireless.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Gets whether the device is already paired / bonded.
    /// </summary>
    public bool IsPaired { get; init; }

    /// <summary>
    /// Gets whether the printer can accept PDF through the system print stack.
    /// </summary>
    public bool SupportsPdf { get; init; }

    /// <summary>
    /// Gets whether the printer can accept ESC/POS.
    /// </summary>
    public bool SupportsEscPos { get; init; }
}
