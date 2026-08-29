namespace Plugin.Maui.Printing;

/// <summary>
/// A priced or counted line on an invoice, challan, or similar document.
/// </summary>
public sealed class DocumentLine
{
    /// <summary>
    /// Gets the item description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the quantity. Default is 1.
    /// </summary>
    public decimal Quantity { get; init; } = 1;

    /// <summary>
    /// Gets the unit price when the line is priced.
    /// </summary>
    public decimal? UnitPrice { get; init; }

    /// <summary>
    /// Gets an optional unit (pcs, kg, hrs).
    /// </summary>
    public string? Unit { get; init; }

    /// <summary>
    /// Gets the line total. When omitted, quantity × unit price is used.
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    /// Resolves the printed amount.
    /// </summary>
    public decimal ResolvedAmount => Amount ?? Quantity * (UnitPrice ?? 0);
}
