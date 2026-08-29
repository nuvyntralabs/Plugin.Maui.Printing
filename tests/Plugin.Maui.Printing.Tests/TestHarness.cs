namespace Plugin.Maui.Printing.Tests;

sealed class FakePrintPlatform : IPrintPlatform
{
    public bool IsSupported { get; set; } = true;

    public List<PrinterInfo> Printers { get; } =
    [
        new()
        {
            Id = "system",
            Name = "System printer",
            Kind = PrinterKind.System,
            SupportsPdf = true
        },
        new()
        {
            Id = "00:11:22:33:44:55",
            Name = "POS-80",
            Kind = PrinterKind.Thermal,
            Address = "00:11:22:33:44:55",
            IsPaired = true,
            SupportsEscPos = true
        }
    ];

    public NormalizedPrintJob? LastJob { get; private set; }

    public byte[]? LastPayload { get; private set; }

    public string? LastMode { get; private set; }

    public PrintResult NextResult { get; set; } =
        PrintResult.Success(PrintContentKind.Text, PrinterKind.System, PrinterKind.System);

    public PrintAvailability GetAvailability() =>
        new()
        {
            IsSupported = IsSupported,
            CanUseSystemPrinter = true,
            CanUseBluetooth = true,
            BluetoothEnabled = true,
            Platform = "fake"
        };

    public Task<PrinterPermissionStatus> CheckPermissionsAsync() =>
        Task.FromResult(PrinterPermissionStatus.Granted);

    public Task<PrinterPermissionStatus> RequestPermissionsAsync() =>
        Task.FromResult(PrinterPermissionStatus.Granted);

    public Task<IReadOnlyList<PrinterInfo>> DiscoverPrintersAsync(PrinterDiscoveryOptions options, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PrinterInfo>>(Printers);

    public Task<PrintResult> PrintSystemAsync(NormalizedPrintJob job, CancellationToken cancellationToken)
    {
        LastJob = job;
        LastPayload = null;
        LastMode = "system";
        return Task.FromResult(NextResult with
        {
            ContentKind = job.Document.ContentKind,
            JobKind = job.Document.JobKind,
            RequestedTarget = job.RequestedTarget,
            ResolvedTarget = job.ResolvedTarget
        });
    }

    public Task<PrintResult> PrintBluetoothAsync(NormalizedPrintJob job, byte[] payload, CancellationToken cancellationToken)
    {
        LastJob = job;
        LastPayload = payload;
        LastMode = "bluetooth";
        return Task.FromResult(NextResult with
        {
            ContentKind = job.Document.ContentKind,
            JobKind = job.Document.JobKind,
            RequestedTarget = job.RequestedTarget,
            ResolvedTarget = job.ResolvedTarget,
            PrinterId = job.Options.BluetoothAddress ?? "00:11:22:33:44:55"
        });
    }
}

static class Harness
{
    public static (PrinterImplementation Printer, FakePrintPlatform Platform) Create(Action<PrintingOptions>? configure = null)
    {
        var options = new PrintingOptions
        {
            DefaultJobName = "Printing",
            RaisePrintCompletedEvent = true
        };
        configure?.Invoke(options);
        var platform = new FakePrintPlatform();
        var printer = Printer.Create(options, platform);
        return (printer, platform);
    }

    public static string WriteTempFile(string name = "invoice.pdf", string contents = "%PDF-1.1")
    {
        var folder = Path.Combine(Path.GetTempPath(), "printing-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, name);
        File.WriteAllText(path, contents);
        return path;
    }
}
