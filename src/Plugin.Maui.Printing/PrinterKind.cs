namespace Plugin.Maui.Printing;

/// <summary>
/// Where a print job should go.
/// </summary>
public enum PrinterKind
{
    /// <summary>
    /// PDF, images, and plain text use the system printer.
    /// Receipts and structured business documents use a thermal / Bluetooth printer.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Android <c>PrintManager</c> or iOS AirPrint.
    /// </summary>
    System = 1,

    /// <summary>
    /// A paired Bluetooth printer (Classic SPP on Android, BLE on iOS).
    /// </summary>
    Bluetooth = 2,

    /// <summary>
    /// ESC/POS thermal output over Bluetooth.
    /// </summary>
    Thermal = 3
}
