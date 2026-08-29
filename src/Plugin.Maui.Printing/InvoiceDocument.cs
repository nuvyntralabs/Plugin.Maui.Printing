namespace Plugin.Maui.Printing;

/// <summary>
/// Sales invoice rendered as a thermal receipt or formatted system text.
/// </summary>
public sealed class InvoiceDocument
{
    /// <summary>
    /// Gets the seller / shop name.
    /// </summary>
    public required string SellerName { get; init; }

    /// <summary>
    /// Gets an optional seller address.
    /// </summary>
    public string? SellerAddress { get; init; }

    /// <summary>
    /// Gets the invoice number.
    /// </summary>
    public required string InvoiceNumber { get; init; }

    /// <summary>
    /// Gets when the invoice was issued.
    /// </summary>
    public DateTimeOffset IssuedAt { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// Gets the buyer name.
    /// </summary>
    public string? BuyerName { get; init; }

    /// <summary>
    /// Gets line items.
    /// </summary>
    public IReadOnlyList<DocumentLine> Lines { get; init; } = [];

    /// <summary>
    /// Gets the subtotal. When omitted, line amounts are summed.
    /// </summary>
    public decimal? Subtotal { get; init; }

    /// <summary>
    /// Gets tax.
    /// </summary>
    public decimal Tax { get; init; }

    /// <summary>
    /// Gets the grand total. When omitted, subtotal + tax is used.
    /// </summary>
    public decimal? Total { get; init; }

    /// <summary>
    /// Gets footer notes.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Gets an optional payment or verification QR payload.
    /// </summary>
    public string? QrPayload { get; init; }

    /// <summary>
    /// Gets the job name used on the system print sheet.
    /// </summary>
    public string JobName => $"Invoice {InvoiceNumber}";

    internal decimal ResolvedSubtotal => Subtotal ?? Lines.Sum(line => line.ResolvedAmount);

    internal decimal ResolvedTotal => Total ?? ResolvedSubtotal + Tax;
}
