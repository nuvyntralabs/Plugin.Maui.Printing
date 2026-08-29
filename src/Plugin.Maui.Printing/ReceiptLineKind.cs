namespace Plugin.Maui.Printing;

/// <summary>
/// A single instruction in a <see cref="ReceiptDocument"/>.
/// </summary>
public enum ReceiptLineKind
{
    /// <summary>Print text and advance one line.</summary>
    Text = 0,

    /// <summary>Left and right columns on one line.</summary>
    Columns = 1,

    /// <summary>A full-width rule.</summary>
    Separator = 2,

    /// <summary>Advance the paper without printing.</summary>
    Feed = 3,

    /// <summary>QR code (Model 2).</summary>
    QrCode = 4,

    /// <summary>1D barcode.</summary>
    Barcode = 5,

    /// <summary>1-bit raster or platform-decoded image.</summary>
    Image = 6,

    /// <summary>Partial or full cut.</summary>
    Cut = 7,

    /// <summary>Cash-drawer pulse.</summary>
    CashDrawer = 8
}
