#if ANDROID
using Android.Bluetooth;
using Android.Content;
using Android.Graphics.Pdf;
using Android.OS;
using Android.Print;
using Java.Util;
using Application = Android.App.Application;
using IOPath = System.IO.Path;
using AndroidBitmap = Android.Graphics.Bitmap;
using AndroidBitmapFactory = Android.Graphics.BitmapFactory;
using AndroidPaint = Android.Graphics.Paint;
using AndroidRect = Android.Graphics.Rect;

namespace Plugin.Maui.Printing;

sealed class AndroidPrintPlatform : IPrintPlatform
{
    static readonly UUID SppUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB")!;

    public bool IsSupported => true;

    public PrintAvailability GetAvailability()
    {
        var adapter = GetAdapter();
        return new PrintAvailability
        {
            IsSupported = true,
            CanUseSystemPrinter = true,
            CanUseBluetooth = adapter is not null,
            BluetoothEnabled = adapter?.IsEnabled == true,
            Platform = "Android PrintManager + Bluetooth SPP"
        };
    }

    public async Task<PrinterPermissionStatus> CheckPermissionsAsync()
    {
        var status = await Permissions.CheckStatusAsync<BluetoothPrintPermission>().ConfigureAwait(false);
        return Map(status);
    }

    public async Task<PrinterPermissionStatus> RequestPermissionsAsync()
    {
        var status = await Permissions.RequestAsync<BluetoothPrintPermission>().ConfigureAwait(false);
        return Map(status);
    }

    public async Task<IReadOnlyList<PrinterInfo>> DiscoverPrintersAsync(PrinterDiscoveryOptions options, CancellationToken cancellationToken)
    {
        var printers = new List<PrinterInfo>
        {
            new()
            {
                Id = "system",
                Name = "System printer",
                Kind = PrinterKind.System,
                SupportsPdf = true,
                SupportsEscPos = false
            }
        };

        if (options.Kind is PrinterKind.System)
            return printers;

        var permission = await CheckPermissionsAsync().ConfigureAwait(false);
        if (permission is not PrinterPermissionStatus.Granted)
            return printers;

        var adapter = GetAdapter();
        if (adapter is null || !options.IncludePaired)
            return printers;

        foreach (var device in adapter.BondedDevices ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            var address = device.Address ?? device.Name ?? Guid.NewGuid().ToString("N");
            printers.Add(new PrinterInfo
            {
                Id = address,
                Name = device.Name ?? address,
                Kind = LooksThermal(device) ? PrinterKind.Thermal : PrinterKind.Bluetooth,
                Address = device.Address,
                IsPaired = true,
                SupportsPdf = false,
                SupportsEscPos = true
            });
        }

        return printers;
    }

    public Task<PrintResult> PrintSystemAsync(NormalizedPrintJob job, CancellationToken cancellationToken) =>
        MainThread.InvokeOnMainThreadAsync(() => PrintSystem(job));

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
                "Bluetooth permission was denied.");
        }

        var adapter = GetAdapter();
        if (adapter is null || !adapter.IsEnabled)
        {
            return PrintResult.Unavailable(
                job.Document.ContentKind,
                job.RequestedTarget,
                job.ResolvedTarget,
                job.Document.JobKind,
                "Bluetooth is not available or is turned off.");
        }

        var device = ResolveDevice(adapter, job.Options);
        if (device is null)
        {
            return PrintResult.Unavailable(
                job.Document.ContentKind,
                job.RequestedTarget,
                job.ResolvedTarget,
                job.Document.JobKind,
                "No paired Bluetooth printer was found. Pair the printer in Android Settings or set BluetoothAddress.");
        }

        try
        {
            await Task.Run(() => WriteSpp(adapter, device, payload, cancellationToken), cancellationToken).ConfigureAwait(false);
            return PrintResult.Success(
                job.Document.ContentKind,
                job.RequestedTarget,
                job.ResolvedTarget,
                job.Document.JobKind,
                device.Address,
                device.Name,
                "Sent to Bluetooth printer");
        }
        catch (System.OperationCanceledException)
        {
            throw;
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

    PrintResult PrintSystem(NormalizedPrintJob job)
    {
        var activity = Platform.CurrentActivity ?? throw new PrinterException(PrinterError.NotSupported, "No current Android activity.");
        var printManager = activity.GetSystemService(Context.PrintService) as PrintManager
            ?? throw new PrinterException(PrinterError.NotSupported, "PrintManager is not available.");

        var attributes = BuildAttributes(job.Options);

        string pdfPath;
        try
        {
            pdfPath = job.Document.ContentKind switch
            {
                PrintContentKind.Pdf => EnsureFile(job, ".pdf"),
                PrintContentKind.Image => WriteImagePdf(job, job.Options.Paper),
                _ => WriteTextPdf(job.Text ?? string.Empty, job.Options.Paper)
            };
        }
        catch (PrinterException ex)
        {
            return PrintResult.Fail(job.Document.ContentKind, job.RequestedTarget, job.ResolvedTarget, ex.Message, job.Document.JobKind);
        }

        printManager.Print(job.JobName, new StreamPrintAdapter(job.JobName, pdfPath), attributes);
        return PrintResult.Success(job.Document.ContentKind, job.RequestedTarget, PrinterKind.System, job.Document.JobKind, "system", "System printer");
    }

    static PrintAttributes BuildAttributes(PrintOptions options)
    {
        var media = options.Paper switch
        {
            PaperSize.Letter => PrintAttributes.MediaSize.NaLetter,
            PaperSize.A5 => PrintAttributes.MediaSize.IsoA5,
            _ => PrintAttributes.MediaSize.IsoA4
        } ?? PrintAttributes.MediaSize.IsoA4;

        if (options.Orientation == PrintOrientation.Landscape)
            media = media.AsLandscape() ?? media;

        return new PrintAttributes.Builder()
            .SetMediaSize(media)
            .SetMinMargins(PrintAttributes.Margins.NoMargins ?? new PrintAttributes.Margins(0, 0, 0, 0))
            .Build();
    }

    static BluetoothDevice? ResolveDevice(BluetoothAdapter adapter, PrintOptions options)
    {
        var id = options.BluetoothAddress ?? options.PrinterId;
        if (!string.IsNullOrWhiteSpace(id) && !string.Equals(id, "system", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return adapter.GetRemoteDevice(id);
            }
            catch (Java.Lang.IllegalArgumentException)
            {
                return adapter.BondedDevices?.FirstOrDefault(device =>
                    string.Equals(device.Address, id, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(device.Name, id, StringComparison.OrdinalIgnoreCase));
            }
        }

        return adapter.BondedDevices?.FirstOrDefault(LooksThermal)
            ?? adapter.BondedDevices?.FirstOrDefault();
    }

    static void WriteSpp(BluetoothAdapter adapter, BluetoothDevice device, byte[] payload, CancellationToken cancellationToken)
    {
        adapter.CancelDiscovery();
        BluetoothSocket? socket = null;
        try
        {
            socket = Connect(device) ?? throw new IOException("Could not open a Bluetooth socket.");
            using var output = socket.OutputStream ?? throw new IOException("Bluetooth output stream is null.");
            const int chunk = 1024;
            for (var offset = 0; offset < payload.Length; offset += chunk)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var length = Math.Min(chunk, payload.Length - offset);
                output.Write(payload, offset, length);
            }

            output.Flush();
        }
        finally
        {
            try
            {
                socket?.Close();
            }
            catch
            {
                // ignored
            }
        }
    }

    static BluetoothSocket Connect(BluetoothDevice device)
    {
        try
        {
            var socket = device.CreateRfcommSocketToServiceRecord(SppUuid);
            socket.Connect();
            return socket;
        }
        catch (Exception)
        {
            try
            {
                var insecure = device.CreateInsecureRfcommSocketToServiceRecord(SppUuid);
                insecure.Connect();
                return insecure;
            }
            catch (Exception)
            {
                var integerClass = Java.Lang.Integer.Type;
                if (integerClass is null)
                    throw;

                var method = device.Class.GetMethod("createRfcommSocket", integerClass);
                var fallback = (BluetoothSocket?)method?.Invoke(device, 1)
                    ?? throw new IOException("RFCOMM fallback failed.");
                fallback.Connect();
                return fallback;
            }
        }
    }

    static BluetoothAdapter? GetAdapter()
    {
        var manager = Application.Context.GetSystemService(Context.BluetoothService) as BluetoothManager;
        return manager?.Adapter;
    }

    static bool LooksThermal(BluetoothDevice device)
    {
        if (device.BluetoothClass?.MajorDeviceClass == MajorDeviceClass.Imaging)
            return true;

        var name = device.Name ?? string.Empty;
        return name.Contains("print", StringComparison.OrdinalIgnoreCase)
            || name.Contains("POS", StringComparison.OrdinalIgnoreCase)
            || name.Contains("RPP", StringComparison.OrdinalIgnoreCase)
            || name.Contains("MTP", StringComparison.OrdinalIgnoreCase)
            || name.Contains("TSP", StringComparison.OrdinalIgnoreCase)
            || name.Contains("thermal", StringComparison.OrdinalIgnoreCase);
    }

    static string EnsureFile(NormalizedPrintJob job, string extension)
    {
        if (!string.IsNullOrWhiteSpace(job.FilePath))
            return job.FilePath;

        var folder = IOPath.Combine(FileSystem.CacheDirectory, "maui-printing");
        Directory.CreateDirectory(folder);
        var path = IOPath.Combine(folder, $"{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(path, job.Bytes ?? []);
        return path;
    }

    static string WriteImagePdf(NormalizedPrintJob job, PaperSize paper)
    {
        AndroidBitmap? bitmap = null;
        if (!string.IsNullOrWhiteSpace(job.FilePath))
            bitmap = AndroidBitmapFactory.DecodeFile(job.FilePath);
        if (bitmap is null && job.Bytes is { Length: > 0 })
            bitmap = AndroidBitmapFactory.DecodeByteArray(job.Bytes, 0, job.Bytes.Length);
        if (bitmap is null)
            throw new PrinterException(PrinterError.UnsupportedContent, "The image could not be decoded.");

        var (width, height) = PageSize(paper);
        var document = new PdfDocument();
        try
        {
            var page = document.StartPage(new PdfDocument.PageInfo.Builder(width, height, 1).Create())
                ?? throw new PrinterException(PrinterError.IoFailure, "Could not start a PDF page.");
            var dest = new AndroidRect(36, 36, width - 36, height - 36);
            page.Canvas?.DrawBitmap(bitmap, null, dest, null);
            document.FinishPage(page);

            var folder = IOPath.Combine(FileSystem.CacheDirectory, "maui-printing");
            Directory.CreateDirectory(folder);
            var path = IOPath.Combine(folder, $"{Guid.NewGuid():N}.pdf");
            using var stream = File.Create(path);
            document.WriteTo(stream);
            return path;
        }
        finally
        {
            document.Close();
            bitmap.Recycle();
        }
    }

    static string WriteTextPdf(string text, PaperSize paper)
    {
        var (width, height) = PageSize(paper);

        var document = new PdfDocument();
        try
        {
            var paint = new AndroidPaint { TextSize = 12, AntiAlias = true, Color = Android.Graphics.Color.Black };
            var lines = Wrap(text, 90);
            var index = 1;
            var y = 0f;
            PdfDocument.Page? page = null;

            void NewPage()
            {
                if (page is not null)
                    document.FinishPage(page);
                page = document.StartPage(new PdfDocument.PageInfo.Builder(width, height, index++).Create());
                y = 54;
            }

            NewPage();
            foreach (var line in lines)
            {
                if (y > height - 54)
                    NewPage();
                page!.Canvas.DrawText(line, 54, y, paint);
                y += 16;
            }

            if (page is not null)
                document.FinishPage(page);

            var folder = IOPath.Combine(FileSystem.CacheDirectory, "maui-printing");
            Directory.CreateDirectory(folder);
            var path = IOPath.Combine(folder, $"{Guid.NewGuid():N}.pdf");
            using var stream = File.Create(path);
            document.WriteTo(stream);
            return path;
        }
        finally
        {
            document.Close();
        }
    }

    static (int Width, int Height) PageSize(PaperSize paper) => paper switch
    {
        PaperSize.Letter => (612, 792),
        PaperSize.A5 => (420, 595),
        _ => (595, 842)
    };

    static IEnumerable<string> Wrap(string text, int width)
    {
        using var reader = new StringReader(text);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (line.Length <= width)
            {
                yield return line;
                continue;
            }

            for (var i = 0; i < line.Length; i += width)
                yield return line.Substring(i, Math.Min(width, line.Length - i));
        }
    }

    static PrinterPermissionStatus Map(PermissionStatus status) => status switch
    {
        PermissionStatus.Granted => PrinterPermissionStatus.Granted,
        PermissionStatus.Denied => PrinterPermissionStatus.Denied,
        PermissionStatus.Disabled => PrinterPermissionStatus.Restricted,
        PermissionStatus.Restricted => PrinterPermissionStatus.Restricted,
        _ => PrinterPermissionStatus.Unknown
    };
}

sealed class StreamPrintAdapter : PrintDocumentAdapter
{
    readonly string _jobName;
    readonly string _path;

    public StreamPrintAdapter(string jobName, string path)
    {
        _jobName = jobName;
        _path = path;
    }

    public override void OnLayout(PrintAttributes? oldAttributes, PrintAttributes? newAttributes, CancellationSignal? cancellationSignal, LayoutResultCallback? callback, Bundle? extras)
    {
        if (cancellationSignal?.IsCanceled == true)
        {
            callback?.OnLayoutCancelled();
            return;
        }

        var info = new PrintDocumentInfo.Builder(_jobName)
            .SetContentType(PrintContentType.Document)
            .SetPageCount(PrintDocumentInfo.PageCountUnknown)
            .Build();
        callback?.OnLayoutFinished(info, true);
    }

    public override void OnWrite(PageRange[]? pages, ParcelFileDescriptor? destination, CancellationSignal? cancellationSignal, WriteResultCallback? callback)
    {
        if (destination is null)
        {
            callback?.OnWriteFailed(new Java.Lang.String("No destination."));
            return;
        }

        try
        {
            using var input = File.OpenRead(_path);
            using var output = new Java.IO.FileOutputStream(destination.FileDescriptor);
            var buffer = new byte[8192];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, read);
            callback?.OnWriteFinished([PageRange.AllPages]);
        }
        catch (Exception ex)
        {
            callback?.OnWriteFailed(new Java.Lang.String(ex.Message));
        }
    }
}
#endif
