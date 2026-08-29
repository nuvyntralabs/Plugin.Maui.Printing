namespace Plugin.Maui.Printing;

/// <summary>
/// Shipping, inventory, or product label.
/// </summary>
public sealed class LabelDocument
{
    /// <summary>
    /// Gets the title printed at the top.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets barcode data.
    /// </summary>
    public string? Barcode { get; init; }

    /// <summary>
    /// Gets the barcode format. Default is Code 128.
    /// </summary>
    public BarcodeSymbology BarcodeSymbology { get; init; } = BarcodeSymbology.Code128;

    /// <summary>
    /// Gets extra key/value rows (SKU, bin, destination).
    /// </summary>
    public IReadOnlyDictionary<string, string> Fields { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Gets an optional QR payload.
    /// </summary>
    public string? QrPayload { get; init; }

    /// <summary>
    /// Gets the job name.
    /// </summary>
    public string JobName => $"Label {Title}";
}
