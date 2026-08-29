namespace Plugin.Maui.Printing;

/// <summary>
/// Per-job print settings.
/// </summary>
public sealed class PrintOptions
{
    /// <summary>
    /// Gets or sets the destination. Default is <see cref="PrinterKind.Auto"/>.
    /// </summary>
    public PrinterKind Target { get; set; } = PrinterKind.Auto;

    /// <summary>
    /// Gets or sets a printer id from <see cref="IPrinter.DiscoverPrintersAsync"/>.
    /// </summary>
    public string? PrinterId { get; set; }

    /// <summary>
    /// Gets or sets a Bluetooth MAC (Android) or peripheral UUID (iOS).
    /// </summary>
    public string? BluetoothAddress { get; set; }

    /// <summary>
    /// Gets or sets the BLE service used for thermal writes on iOS.
    /// </summary>
    public Guid? BleServiceId { get; set; }

    /// <summary>
    /// Gets or sets the BLE characteristic used for thermal writes on iOS.
    /// </summary>
    public Guid? BleCharacteristicId { get; set; }

    /// <summary>
    /// Gets or sets system-printer paper. Default is A4.
    /// </summary>
    public PaperSize Paper { get; set; } = PaperSize.A4;

    /// <summary>
    /// Gets or sets thermal paper width. Default is 80 mm.
    /// </summary>
    public ThermalPaperWidth ThermalWidth { get; set; } = ThermalPaperWidth.Mm80;

    /// <summary>
    /// Gets or sets system-printer orientation. Default is portrait.
    /// </summary>
    public PrintOrientation Orientation { get; set; } = PrintOrientation.Portrait;

    /// <summary>
    /// Gets or sets how many copies the system printer should produce. Default is 1.
    /// </summary>
    public int Copies { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to show the system print dialog. Default is <c>true</c>.
    /// </summary>
    public bool ShowSystemDialog { get; set; } = true;

    /// <summary>
    /// Gets or sets whether thermal jobs cut the paper after printing. Default is <c>true</c>.
    /// </summary>
    public bool CutPaper { get; set; } = true;

    /// <summary>
    /// Gets or sets whether thermal jobs pulse the cash drawer.
    /// </summary>
    public bool OpenCashDrawer { get; set; }

    /// <summary>
    /// Gets or sets an override for <see cref="PrintDocument.JobName"/>.
    /// </summary>
    public string? JobName { get; set; }

    /// <summary>
    /// Gets or sets the Bluetooth connect / write timeout. Default is 45 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(45);
}
