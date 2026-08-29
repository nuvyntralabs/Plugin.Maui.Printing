namespace Plugin.Maui.Printing;

/// <summary>
/// Filters for <see cref="IPrinter.DiscoverPrintersAsync"/>.
/// </summary>
public sealed class PrinterDiscoveryOptions
{
    /// <summary>
    /// Gets or sets which transports to include. Default is <see cref="PrinterKind.Auto"/> (all).
    /// </summary>
    public PrinterKind Kind { get; set; } = PrinterKind.Auto;

    /// <summary>
    /// Gets or sets whether to include already-paired Bluetooth printers. Default is <c>true</c>.
    /// </summary>
    public bool IncludePaired { get; set; } = true;

    /// <summary>
    /// Gets or sets whether iOS should scan for BLE printers. Default is <c>true</c>.
    /// </summary>
    public bool ScanBluetooth { get; set; } = true;

    /// <summary>
    /// Gets or sets how long a BLE scan may run. Default is 8 seconds.
    /// </summary>
    public TimeSpan ScanTimeout { get; set; } = TimeSpan.FromSeconds(8);
}
