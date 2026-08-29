namespace Plugin.Maui.Printing;

sealed class PrinterImplementation : IPrinter
{
    readonly PrintingOptions _options;
    readonly IPrintPlatform _platform;
    readonly object _gate = new();
    bool _started;
    bool _disposed;

    public PrinterImplementation(PrintingOptions options, IPrintPlatform platform)
    {
        _options = options;
        _platform = platform;
    }

    public bool IsSupported => _platform.IsSupported;

    public PrintAvailability Availability => _platform.GetAvailability();

    public event EventHandler<PrintCompletedEventArgs>? PrintCompleted;

    public event EventHandler<PrintProgressEventArgs>? PrintProgress;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _started = true;
    }

    public Task<PrinterPermissionStatus> CheckPermissionsAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _platform.CheckPermissionsAsync();
    }

    public Task<PrinterPermissionStatus> RequestPermissionsAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _platform.RequestPermissionsAsync();
    }

    public async Task<IReadOnlyList<PrinterInfo>> DiscoverPrintersAsync(PrinterDiscoveryOptions? options = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        EnsureSupported();
        return await _platform.DiscoverPrintersAsync(options ?? new PrinterDiscoveryOptions(), cancellationToken).ConfigureAwait(false);
    }

    public Task<PrintResult> PrintPdfAsync(string filePath, PrintOptions? options = null, CancellationToken cancellationToken = default) =>
        PrintAsync(PrintDocument.Pdf(filePath), options, cancellationToken);

    public Task<PrintResult> PrintImageAsync(string filePath, PrintOptions? options = null, CancellationToken cancellationToken = default) =>
        PrintAsync(PrintDocument.Image(filePath), options, cancellationToken);

    public Task<PrintResult> PrintTextAsync(string text, PrintOptions? options = null, CancellationToken cancellationToken = default) =>
        PrintAsync(PrintDocument.FromText(text), options, cancellationToken);

    public Task<PrintResult> PrintReceiptAsync(ReceiptDocument receipt, PrintOptions? options = null, CancellationToken cancellationToken = default) =>
        PrintAsync(receipt, options, cancellationToken);

    public Task<PrintResult> PrintAsync(ReceiptDocument receipt, PrintOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        return PrintAsync(PrintDocument.FromReceipt(receipt), options, cancellationToken);
    }

    public Task<PrintResult> PrintAsync(PrintRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return PrintAsync(request.Document, request.Options, cancellationToken);
    }

    public async Task<PrintResult> PrintAsync(PrintDocument document, PrintOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        EnsureSupported();

        var jobOptions = NormalizeOptions(options);
        Validate(document);
        var requested = jobOptions.Target;
        var resolved = ResolveTarget(document, requested);
        var job = Normalize(document, jobOptions, requested, resolved);

        PrintProgress?.Invoke(this, new PrintProgressEventArgs(0, 0, "starting"));

        PrintResult result;
        if (resolved is PrinterKind.Bluetooth or PrinterKind.Thermal)
        {
            var payload = BuildThermalPayload(document, jobOptions);
            result = await _platform.PrintBluetoothAsync(job, payload, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            result = await _platform.PrintSystemAsync(job, cancellationToken).ConfigureAwait(false);
        }

        return Handle(result);
    }

    public byte[] EncodeEscPos(ReceiptDocument receipt, ThermalPaperWidth? width = null)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ObjectDisposedException.ThrowIf(_disposed, this);
        return ReceiptRenderer.Encode(receipt, width ?? _options.DefaultThermalWidth, cutPaper: true, openCashDrawer: false);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;
            _disposed = true;
        }

        _ = _started;
    }

    void EnsureSupported()
    {
        if (_platform.IsSupported)
            return;

        throw new PrinterException(PrinterError.NotSupported, "Printing requires Android or iOS.");
    }

    PrintOptions NormalizeOptions(PrintOptions? options)
    {
        options ??= new PrintOptions();
        if (options.Target == PrinterKind.Auto)
            options.Target = _options.DefaultTarget;
        if (options.Copies < 1)
            options.Copies = 1;
        if (string.IsNullOrWhiteSpace(options.BluetoothAddress))
            options.BluetoothAddress = _options.DefaultBluetoothAddress;
        options.BleServiceId ??= _options.DefaultBleServiceId;
        options.BleCharacteristicId ??= _options.DefaultBleCharacteristicId;
        return options;
    }

    static PrinterKind ResolveTarget(PrintDocument document, PrinterKind requested)
    {
        if (requested != PrinterKind.Auto)
            return requested is PrinterKind.Bluetooth ? PrinterKind.Thermal : requested;

        return document.ContentKind is PrintContentKind.Receipt or PrintContentKind.RawEscPos
            ? PrinterKind.Thermal
            : PrinterKind.System;
    }

    static void Validate(PrintDocument document)
    {
        switch (document.ContentKind)
        {
            case PrintContentKind.Pdf:
            case PrintContentKind.Image:
                if (string.IsNullOrWhiteSpace(document.FilePath) && document.Bytes is not { Length: > 0 })
                    throw new PrinterException(PrinterError.InvalidRequest, "A file path or byte payload is required.");
                if (!string.IsNullOrWhiteSpace(document.FilePath) && !File.Exists(document.FilePath))
                    throw new PrinterException(PrinterError.FileNotFound, $"File not found: {document.FilePath}");
                break;
            case PrintContentKind.Text:
                if (string.IsNullOrWhiteSpace(document.Text))
                    throw new PrinterException(PrinterError.InvalidRequest, "Text is required.");
                break;
            case PrintContentKind.Receipt:
                if (document.Receipt is null || document.Receipt.Lines.Count == 0)
                    throw new PrinterException(PrinterError.InvalidRequest, "Receipt lines are required.");
                break;
            case PrintContentKind.RawEscPos:
                if (document.Bytes is not { Length: > 0 })
                    throw new PrinterException(PrinterError.InvalidRequest, "ESC/POS bytes are required.");
                break;
            default:
                throw new PrinterException(PrinterError.UnsupportedContent, $"Unsupported content: {document.ContentKind}");
        }
    }

    NormalizedPrintJob Normalize(PrintDocument document, PrintOptions options, PrinterKind requested, PrinterKind resolved)
    {
        var jobName = FirstNonEmpty(options.JobName, document.JobName, document.Receipt?.JobName, _options.DefaultJobName) ?? "Print";
        return new NormalizedPrintJob
        {
            Document = document,
            Options = options,
            RequestedTarget = requested,
            ResolvedTarget = resolved,
            JobName = jobName,
            FilePath = document.FilePath,
            Bytes = document.Bytes,
            Text = document.ContentKind == PrintContentKind.Receipt && document.Receipt is not null
                ? ReceiptRenderer.ToPlainText(document.Receipt, options.ThermalWidth)
                : document.Text
        };
    }

    byte[] BuildThermalPayload(PrintDocument document, PrintOptions options)
    {
        if (document.ContentKind == PrintContentKind.RawEscPos)
            return document.Bytes ?? [];

        if (document.ContentKind == PrintContentKind.Receipt && document.Receipt is not null)
            return ReceiptRenderer.Encode(document.Receipt, options.ThermalWidth, options.CutPaper, options.OpenCashDrawer);

        if (document.ContentKind == PrintContentKind.Text)
        {
            var receipt = new ReceiptDocument { JobName = document.JobName }.Text(document.Text ?? string.Empty);
            return ReceiptRenderer.Encode(receipt, options.ThermalWidth, options.CutPaper, options.OpenCashDrawer);
        }

        throw new PrinterException(
            PrinterError.UnsupportedContent,
            $"{document.ContentKind} cannot be sent as ESC/POS. Use a system printer or provide a receipt / raw payload.");
    }

    PrintResult Handle(PrintResult result)
    {
        if (result.Status == PrintStatus.PrinterUnavailable && _options.ThrowWhenPrinterUnavailable)
            throw new PrinterException(PrinterError.PrinterUnavailable, result.Message ?? "No matching printer is available.");

        return Raise(result);
    }

    PrintResult Raise(PrintResult result)
    {
        if (_options.RaisePrintCompletedEvent)
            PrintCompleted?.Invoke(this, new PrintCompletedEventArgs(result));
        return result;
    }

    static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }
}
