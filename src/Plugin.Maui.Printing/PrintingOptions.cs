namespace Plugin.Maui.Printing;

/// <summary>
/// Process-wide defaults for <see cref="IPrinter"/>.
/// </summary>
public sealed class PrintingOptions
{
    /// <summary>
    /// Job name used when a document omits one.
    /// </summary>
    public string DefaultJobName { get; set; } = "Print";

    /// <summary>
    /// Destination used when a request omits <see cref="PrintOptions.Target"/>.
    /// Default is <see cref="PrinterKind.Auto"/>.
    /// </summary>
    public PrinterKind DefaultTarget { get; set; } = PrinterKind.Auto;

    /// <summary>
    /// Thermal paper width for new jobs. Default is 80 mm.
    /// </summary>
    public ThermalPaperWidth DefaultThermalWidth { get; set; } = ThermalPaperWidth.Mm80;

    /// <summary>
    /// Last-used Bluetooth address, applied when a job omits one.
    /// </summary>
    public string? DefaultBluetoothAddress { get; set; }

    /// <summary>
    /// Default BLE service for iOS thermal writes.
    /// Common portable printers use <c>000018f0-0000-1000-8000-00805f9b34fb</c>.
    /// </summary>
    public Guid DefaultBleServiceId { get; set; } = Guid.Parse("000018f0-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// Default BLE write characteristic for iOS thermal writes.
    /// Common portable printers use <c>00002af1-0000-1000-8000-00805f9b34fb</c>.
    /// </summary>
    public Guid DefaultBleCharacteristicId { get; set; } = Guid.Parse("00002af1-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// When <c>true</c>, a missing printer throws <see cref="PrinterException"/>
    /// instead of returning <see cref="PrintStatus.PrinterUnavailable"/>. Default is <c>false</c>.
    /// </summary>
    public bool ThrowWhenPrinterUnavailable { get; set; }

    /// <summary>
    /// When <c>true</c>, print attempts raise <see cref="IPrinter.PrintCompleted"/>. Default is <c>true</c>.
    /// </summary>
    public bool RaisePrintCompletedEvent { get; set; } = true;
}
