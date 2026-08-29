namespace Plugin.Maui.Printing;

/// <summary>
/// Text emphasis for a receipt line. Flags may be combined.
/// </summary>
[Flags]
public enum ReceiptTextStyle
{
    /// <summary>Normal size, not bold.</summary>
    None = 0,

    /// <summary>Emphasized (ESC E).</summary>
    Bold = 1,

    /// <summary>Underline.</summary>
    Underline = 2,

    /// <summary>Double width and height.</summary>
    DoubleSize = 4
}
