namespace Plugin.Maui.Printing;

sealed class UnsupportedPrintPlatform : IPrintPlatform
{
    public bool IsSupported => false;

    public PrintAvailability GetAvailability() =>
        new()
        {
            IsSupported = false,
            CanUseSystemPrinter = false,
            CanUseBluetooth = false,
            BluetoothEnabled = false,
            Platform = "net10.0"
        };

    public Task<PrinterPermissionStatus> CheckPermissionsAsync() =>
        Task.FromResult(PrinterPermissionStatus.NotRequired);

    public Task<PrinterPermissionStatus> RequestPermissionsAsync() =>
        Task.FromResult(PrinterPermissionStatus.NotRequired);

    public Task<IReadOnlyList<PrinterInfo>> DiscoverPrintersAsync(PrinterDiscoveryOptions options, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PrinterInfo>>([]);

    public Task<PrintResult> PrintSystemAsync(NormalizedPrintJob job, CancellationToken cancellationToken) =>
        throw new PrinterException(PrinterError.NotSupported, "Printing requires Android or iOS.");

    public Task<PrintResult> PrintBluetoothAsync(NormalizedPrintJob job, byte[] payload, CancellationToken cancellationToken) =>
        throw new PrinterException(PrinterError.NotSupported, "Printing requires Android or iOS.");
}
