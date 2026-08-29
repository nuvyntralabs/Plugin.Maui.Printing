namespace Plugin.Maui.Printing;

/// <summary>
/// One ESC/POS or formatted-text instruction.
/// </summary>
public sealed class ReceiptLine
{
    /// <summary>
    /// Gets the instruction kind.
    /// </summary>
    public required ReceiptLineKind Kind { get; init; }

    /// <summary>
    /// Gets the primary text, QR payload, or barcode data.
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Gets the right-hand column for <see cref="ReceiptLineKind.Columns"/>.
    /// </summary>
    public string? RightText { get; init; }

    /// <summary>
    /// Gets the alignment. Default is left.
    /// </summary>
    public ReceiptAlign Align { get; init; } = ReceiptAlign.Left;

    /// <summary>
    /// Gets text emphasis.
    /// </summary>
    public ReceiptTextStyle Style { get; init; } = ReceiptTextStyle.None;

    /// <summary>
    /// Gets the barcode format when <see cref="Kind"/> is <see cref="ReceiptLineKind.Barcode"/>.
    /// </summary>
    public BarcodeSymbology BarcodeSymbology { get; init; } = BarcodeSymbology.Code128;

    /// <summary>
    /// Gets optional 1-bit raster (MSB first, row padded to 8 pixels) or PNG/JPEG bytes for images.
    /// </summary>
    public byte[]? ImageBytes { get; init; }

    /// <summary>
    /// Gets raster width in pixels when <see cref="ImageBytes"/> is already 1-bit.
    /// </summary>
    public int ImageWidth { get; init; }

    /// <summary>
    /// Gets raster height in pixels when <see cref="ImageBytes"/> is already 1-bit.
    /// </summary>
    public int ImageHeight { get; init; }

    /// <summary>
    /// Gets how many lines to feed when <see cref="Kind"/> is <see cref="ReceiptLineKind.Feed"/>.
    /// </summary>
    public int FeedLines { get; init; } = 1;

    /// <summary>
    /// Gets the rule character for separators. Default is <c>-</c>.
    /// </summary>
    public char SeparatorChar { get; init; } = '-';

    /// <summary>
    /// Gets whether a cut is partial. Default is <c>true</c>.
    /// </summary>
    public bool PartialCut { get; init; } = true;
}
