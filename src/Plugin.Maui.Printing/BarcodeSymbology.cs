namespace Plugin.Maui.Printing;

/// <summary>
/// Barcode formats the ESC/POS encoder can emit.
/// </summary>
public enum BarcodeSymbology
{
    /// <summary>Code 128 (receipt, ticket, challan numbers).</summary>
    Code128 = 0,

    /// <summary>Code 39.</summary>
    Code39 = 1,

    /// <summary>EAN-13 (12 digits + check digit, or 13 digits).</summary>
    Ean13 = 2
}
