#if IOS
using CoreBluetooth;
using Foundation;
using UIKit;

namespace Plugin.Maui.Printing;

sealed class IosPrintPlatform : IPrintPlatform
{
    public bool IsSupported => true;

    public PrintAvailability GetAvailability()
    {
        var state = CBCentralManager.Authorization;
        return new PrintAvailability
        {
            IsSupported = true,
            CanUseSystemPrinter = true,
            CanUseBluetooth = true,
            BluetoothEnabled = state is CBManagerAuthorization.AllowedAlways,
            Platform = "iOS AirPrint + BLE"
        };
    }

    public Task<PrinterPermissionStatus> CheckPermissionsAsync() =>
        Task.FromResult(Map(CBCentralManager.Authorization));

    public async Task<PrinterPermissionStatus> RequestPermissionsAsync()
    {
        if (CBCentralManager.Authorization is CBManagerAuthorization.AllowedAlways)
            return PrinterPermissionStatus.Granted;

        using var probe = new IosBleProbe();
        await probe.WaitForReadyAsync(TimeSpan.FromSeconds(8)).ConfigureAwait(false);
        return Map(CBCentralManager.Authorization);
    }

    public async Task<IReadOnlyList<PrinterInfo>> DiscoverPrintersAsync(PrinterDiscoveryOptions options, CancellationToken cancellationToken)
    {
        var printers = new List<PrinterInfo>
        {
            new()
            {
                Id = "system",
                Name = "AirPrint",
                Kind = PrinterKind.System,
                SupportsPdf = true,
                SupportsEscPos = false
            }
        };

        if (options.Kind is PrinterKind.System || !options.ScanBluetooth)
            return printers;

        using var session = new IosBleThermalSession();
        var found = await session.ScanAsync(options.ScanTimeout, cancellationToken).ConfigureAwait(false);
        printers.AddRange(found);
        return printers;
    }

    public Task<PrintResult> PrintSystemAsync(NormalizedPrintJob job, CancellationToken cancellationToken) =>
        MainThread.InvokeOnMainThreadAsync(() => PresentSystemPrint(job));

    public async Task<PrintResult> PrintBluetoothAsync(NormalizedPrintJob job, byte[] payload, CancellationToken cancellationToken)
    {
        var permission = await RequestPermissionsAsync().ConfigureAwait(false);
        if (permission is not PrinterPermissionStatus.Granted)
        {
            return PrintResult.Unavailable(
                job.Document.ContentKind,
                job.RequestedTarget,
                job.ResolvedTarget,
                job.Document.JobKind,
                "Bluetooth permission was denied. Add NSBluetoothAlwaysUsageDescription.");
        }

        using var session = new IosBleThermalSession();
        try
        {
            await session.WriteAsync(job.Options, payload, cancellationToken).ConfigureAwait(false);
            return PrintResult.Success(
                job.Document.ContentKind,
                job.RequestedTarget,
                job.ResolvedTarget,
                job.Document.JobKind,
                job.Options.BluetoothAddress ?? job.Options.PrinterId,
                job.Options.BluetoothAddress,
                "Sent to BLE printer");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (PrinterException ex) when (ex.Error == PrinterError.PrinterUnavailable)
        {
            return PrintResult.Unavailable(
                job.Document.ContentKind,
                job.RequestedTarget,
                job.ResolvedTarget,
                job.Document.JobKind,
                ex.Message);
        }
        catch (Exception ex)
        {
            return PrintResult.Fail(
                job.Document.ContentKind,
                job.RequestedTarget,
                job.ResolvedTarget,
                ex.Message,
                job.Document.JobKind);
        }
    }

    static Task<PrintResult> PresentSystemPrint(NormalizedPrintJob job)
    {
        var tcs = new TaskCompletionSource<PrintResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var printInfo = UIPrintInfo.PrintInfo;
        printInfo.JobName = job.JobName;
        printInfo.Orientation = job.Options.Orientation == PrintOrientation.Landscape
            ? UIPrintInfoOrientation.Landscape
            : UIPrintInfoOrientation.Portrait;
        printInfo.OutputType = job.Document.ContentKind == PrintContentKind.Image
            ? UIPrintInfoOutputType.Photo
            : UIPrintInfoOutputType.General;

        var controller = UIPrintInteractionController.SharedPrintController;
        controller.PrintInfo = printInfo;
        controller.ShowsNumberOfCopies = true;

        if (job.Document.ContentKind == PrintContentKind.Pdf)
        {
            controller.PrintingItem = job.FilePath is { Length: > 0 }
                ? NSUrl.FromFilename(job.FilePath)
                : NSData.FromArray(job.Bytes ?? []);
        }
        else if (job.Document.ContentKind == PrintContentKind.Image)
        {
            UIImage? image = job.FilePath is { Length: > 0 }
                ? UIImage.FromFile(job.FilePath)
                : job.Bytes is { Length: > 0 }
                    ? UIImage.LoadFromData(NSData.FromArray(job.Bytes))
                    : null;

            if (image is null)
            {
                tcs.TrySetResult(PrintResult.Fail(job.Document.ContentKind, job.RequestedTarget, job.ResolvedTarget, "The image could not be decoded.", job.Document.JobKind));
                return tcs.Task;
            }

            controller.PrintingItem = image;
        }
        else
        {
            controller.PrintFormatter = new UISimpleTextPrintFormatter(job.Text ?? string.Empty);
        }

        controller.Present(true, (_, completed, error) =>
        {
            if (error is not null)
            {
                tcs.TrySetResult(PrintResult.Fail(job.Document.ContentKind, job.RequestedTarget, PrinterKind.System, error.LocalizedDescription, job.Document.JobKind));
                return;
            }

            tcs.TrySetResult(completed
                ? PrintResult.Success(job.Document.ContentKind, job.RequestedTarget, PrinterKind.System, job.Document.JobKind, "system", "AirPrint")
                : PrintResult.Cancel(job.Document.ContentKind, job.RequestedTarget, PrinterKind.System, job.Document.JobKind));
        });

        return tcs.Task;
    }

    static PrinterPermissionStatus Map(CBManagerAuthorization authorization) => authorization switch
    {
        CBManagerAuthorization.AllowedAlways => PrinterPermissionStatus.Granted,
        CBManagerAuthorization.Denied => PrinterPermissionStatus.Denied,
        CBManagerAuthorization.Restricted => PrinterPermissionStatus.Restricted,
        _ => PrinterPermissionStatus.Unknown
    };
}

sealed class IosBleProbe : NSObject, ICBCentralManagerDelegate
{
    readonly CBCentralManager _manager;
    readonly TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IosBleProbe()
    {
        _manager = new CBCentralManager(this, null);
    }

    public async Task WaitForReadyAsync(TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await _ready.Task.WaitAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // The authorization prompt may still be showing.
        }
    }

    [Export("centralManagerDidUpdateState:")]
    public void UpdatedState(CBCentralManager central) => _ready.TrySetResult(true);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _manager.Dispose();
        base.Dispose(disposing);
    }
}

sealed class IosBleThermalSession : NSObject, ICBCentralManagerDelegate, ICBPeripheralDelegate
{
    static readonly CBUUID[] CommonWriteCharacteristics =
    [
        CBUUID.FromString("2AF1"),
        CBUUID.FromString("FFE1"),
        CBUUID.FromString("FF02"),
        CBUUID.FromString("18F0")
    ];

    readonly CBCentralManager _manager;
    readonly List<PrinterInfo> _found = [];
    readonly TaskCompletionSource<bool> _powered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    TaskCompletionSource<bool>? _connected;
    TaskCompletionSource<bool>? _readyToWrite;
    TaskCompletionSource<bool>? _write;
    CBPeripheral? _peripheral;
    CBCharacteristic? _characteristic;
    Guid? _serviceId;
    Guid? _characteristicId;
    byte[]? _payload;
    int _offset;

    public IosBleThermalSession()
    {
        _manager = new CBCentralManager(this, null);
    }

    public async Task<IReadOnlyList<PrinterInfo>> ScanAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        await WaitPoweredAsync(timeout, cancellationToken).ConfigureAwait(false);
        _manager.ScanForPeripherals((CBUUID[]?)null);

        try
        {
            await Task.Delay(timeout, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _manager.StopScan();
        }

        return _found;
    }

    public async Task WriteAsync(PrintOptions options, byte[] payload, CancellationToken cancellationToken)
    {
        _payload = payload;
        _serviceId = options.BleServiceId;
        _characteristicId = options.BleCharacteristicId;
        await WaitPoweredAsync(options.Timeout, cancellationToken).ConfigureAwait(false);

        _connected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _readyToWrite = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var id = options.BluetoothAddress ?? options.PrinterId;
        if (!string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out var uuid))
        {
            var known = _manager.RetrievePeripheralsWithIdentifiers(new NSUuid(uuid.ToString()));
            if (known.Length > 0)
            {
                _peripheral = known[0];
                _peripheral.Delegate = this;
                _manager.ConnectPeripheral(_peripheral);
            }
        }

        if (_peripheral is null)
        {
            _manager.ScanForPeripherals((CBUUID[]?)null);
            using var scanCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            scanCts.CancelAfter(options.Timeout);
            try
            {
                await _connected.Task.WaitAsync(scanCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _manager.StopScan();
                throw new PrinterException(PrinterError.PrinterUnavailable, "No BLE printer was found. Pass PrinterId or BluetoothAddress.");
            }
        }

        _manager.StopScan();
        await _readyToWrite.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        await WriteChunksAsync(cancellationToken).ConfigureAwait(false);
        if (_peripheral is not null)
            _manager.CancelPeripheralConnection(_peripheral);
    }

    async Task WaitPoweredAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        await _powered.Task.WaitAsync(cts.Token).ConfigureAwait(false);
        if (_manager.State != CBManagerState.PoweredOn)
            throw new PrinterException(PrinterError.PrinterUnavailable, $"Bluetooth is not ready ({_manager.State}).");
    }

    async Task WriteChunksAsync(CancellationToken cancellationToken)
    {
        if (_peripheral is null || _characteristic is null || _payload is null)
            throw new PrinterException(PrinterError.IoFailure, "BLE write characteristic was not discovered.");

        const int chunk = 182;
        while (_offset < _payload.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _write = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var length = Math.Min(chunk, _payload.Length - _offset);
            var slice = new byte[length];
            Buffer.BlockCopy(_payload, _offset, slice, 0, length);
            var type = _characteristic.Properties.HasFlag(CBCharacteristicProperties.WriteWithoutResponse)
                ? CBCharacteristicWriteType.WithoutResponse
                : CBCharacteristicWriteType.WithResponse;
            _peripheral.WriteValue(NSData.FromArray(slice), _characteristic, type);
            if (type == CBCharacteristicWriteType.WithResponse)
                await _write.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(20, cancellationToken).ConfigureAwait(false);
            _offset += length;
        }
    }

    [Export("centralManagerDidUpdateState:")]
    public void UpdatedState(CBCentralManager central)
    {
        if (central.State == CBManagerState.PoweredOn)
            _powered.TrySetResult(true);
    }

    [Export("centralManager:didDiscoverPeripheral:advertisementData:RSSI:")]
    public void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber rssi)
    {
        var name = peripheral.Name ?? "BLE printer";
        var id = peripheral.Identifier.AsString();
        if (_found.All(item => item.Id != id))
        {
            _found.Add(new PrinterInfo
            {
                Id = id,
                Name = name,
                Kind = PrinterKind.Thermal,
                Address = id,
                IsPaired = false,
                SupportsPdf = false,
                SupportsEscPos = true
            });
        }

        if (_connected is null || _peripheral is not null)
            return;

        _peripheral = peripheral;
        _peripheral.Delegate = this;
        _manager.ConnectPeripheral(_peripheral);
        _connected.TrySetResult(true);
    }

    [Export("centralManager:didConnectPeripheral:")]
    public void ConnectedPeripheral(CBCentralManager central, CBPeripheral peripheral)
    {
        _connected?.TrySetResult(true);
        var services = _serviceId is { } service
            ? new[] { CBUUID.FromString(service.ToString()) }
            : null;
        peripheral.DiscoverServices(services);
    }

    [Export("centralManager:didFailToConnectPeripheral:error:")]
    public void FailedToConnectPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError? error)
    {
        _connected?.TrySetException(new PrinterException(PrinterError.IoFailure, error?.LocalizedDescription ?? "BLE connect failed."));
    }

    [Export("peripheral:didDiscoverServices:")]
    public void DiscoveredService(CBPeripheral peripheral, NSError? error)
    {
        if (error is not null)
        {
            _readyToWrite?.TrySetException(new PrinterException(PrinterError.IoFailure, error.LocalizedDescription));
            return;
        }

        foreach (var service in peripheral.Services ?? [])
            peripheral.DiscoverCharacteristics((CBUUID[]?)null, service);
    }

    [Export("peripheral:didDiscoverCharacteristicsForService:error:")]
    public void DiscoveredCharacteristics(CBPeripheral peripheral, CBService service, NSError? error)
    {
        if (error is not null || _characteristic is not null)
            return;

        var preferred = _characteristicId is { } id ? CBUUID.FromString(id.ToString()) : null;
        _characteristic = service.Characteristics?.FirstOrDefault(item => preferred is not null && item.UUID.Equals(preferred))
            ?? service.Characteristics?.FirstOrDefault(item => CommonWriteCharacteristics.Any(uuid => item.UUID.Equals(uuid)))
            ?? service.Characteristics?.FirstOrDefault(item =>
                item.Properties.HasFlag(CBCharacteristicProperties.Write)
                || item.Properties.HasFlag(CBCharacteristicProperties.WriteWithoutResponse));

        if (_characteristic is not null)
            _readyToWrite?.TrySetResult(true);
    }

    [Export("peripheral:didWriteValueForCharacteristic:error:")]
    public void WroteCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic, NSError? error)
    {
        if (error is not null)
            _write?.TrySetException(new PrinterException(PrinterError.IoFailure, error.LocalizedDescription));
        else
            _write?.TrySetResult(true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _manager.StopScan();
            if (_peripheral is not null)
                _manager.CancelPeripheralConnection(_peripheral);
            _manager.Dispose();
        }

        base.Dispose(disposing);
    }
}
#endif
