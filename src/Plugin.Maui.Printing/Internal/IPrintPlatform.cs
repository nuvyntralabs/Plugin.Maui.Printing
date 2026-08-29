namespace Plugin.Maui.Printing;

interface IPrintPlatform
{
    bool IsSupported { get; }

    PrintAvailability GetAvailability();

    Task<PrinterPermissionStatus> CheckPermissionsAsync();

    Task<PrinterPermissionStatus> RequestPermissionsAsync();

    Task<IReadOnlyList<PrinterInfo>> DiscoverPrintersAsync(PrinterDiscoveryOptions options, CancellationToken cancellationToken);

    Task<PrintResult> PrintSystemAsync(NormalizedPrintJob job, CancellationToken cancellationToken);

    Task<PrintResult> PrintBluetoothAsync(NormalizedPrintJob job, byte[] payload, CancellationToken cancellationToken);
}
