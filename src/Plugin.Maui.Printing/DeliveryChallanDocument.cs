namespace Plugin.Maui.Printing;

/// <summary>
/// Goods delivery challan.
/// </summary>
public sealed class DeliveryChallanDocument
{
    /// <summary>
    /// Gets the challan number.
    /// </summary>
    public required string ChallanNumber { get; init; }

    /// <summary>
    /// Gets when the challan was issued.
    /// </summary>
    public DateTimeOffset IssuedAt { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// Gets the consignor / sender.
    /// </summary>
    public required string Consignor { get; init; }

    /// <summary>
    /// Gets the consignee / receiver.
    /// </summary>
    public required string Consignee { get; init; }

    /// <summary>
    /// Gets an optional vehicle registration number.
    /// </summary>
    public string? VehicleNumber { get; init; }

    /// <summary>
    /// Gets shipped items.
    /// </summary>
    public IReadOnlyList<DocumentLine> Lines { get; init; } = [];

    /// <summary>
    /// Gets footer notes.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Gets the job name.
    /// </summary>
    public string JobName => $"Challan {ChallanNumber}";
}
