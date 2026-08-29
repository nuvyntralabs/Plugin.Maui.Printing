namespace Plugin.Maui.Printing;

/// <summary>
/// Print PDF, images, text, and thermal documents on Android and iOS.
/// </summary>
public interface IPrinter : IDisposable
{
    /// <summary>
    /// Always <c>true</c> on Android and iOS. <c>false</c> on the <c>net10.0</c>
    /// reference assembly.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets a point-in-time capability snapshot.
    /// </summary>
    PrintAvailability Availability { get; }

    /// <summary>
    /// Raised after a print attempt finishes.
    /// </summary>
    event EventHandler<PrintCompletedEventArgs>? PrintCompleted;

    /// <summary>
    /// Raised while a Bluetooth payload is written.
    /// </summary>
    event EventHandler<PrintProgressEventArgs>? PrintProgress;

    /// <summary>
    /// No-op reserved for host startup. Safe to call more than once.
    /// </summary>
    void Start();

    /// <summary>
    /// Checks Bluetooth permission without prompting. System printing does not need it.
    /// </summary>
    Task<PrinterPermissionStatus> CheckPermissionsAsync();

    /// <summary>
    /// Requests Bluetooth permission used for thermal / SPP / BLE printers.
    /// </summary>
    Task<PrinterPermissionStatus> RequestPermissionsAsync();

    /// <summary>
    /// Lists system and paired or nearby Bluetooth printers.
    /// </summary>
    Task<IReadOnlyList<PrinterInfo>> DiscoverPrintersAsync(PrinterDiscoveryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prints a document.
    /// </summary>
    Task<PrintResult> PrintAsync(PrintDocument document, PrintOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prints a thermal receipt.
    /// </summary>
    Task<PrintResult> PrintAsync(ReceiptDocument receipt, PrintOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prints using a request object.
    /// </summary>
    Task<PrintResult> PrintAsync(PrintRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prints a local PDF file.
    /// </summary>
    Task<PrintResult> PrintPdfAsync(string filePath, PrintOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prints a local image file.
    /// </summary>
    Task<PrintResult> PrintImageAsync(string filePath, PrintOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prints plain text.
    /// </summary>
    Task<PrintResult> PrintTextAsync(string text, PrintOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prints a thermal receipt.
    /// </summary>
    Task<PrintResult> PrintReceiptAsync(ReceiptDocument receipt, PrintOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Encodes a receipt as ESC/POS without sending it to a printer.
    /// </summary>
    byte[] EncodeEscPos(ReceiptDocument receipt, ThermalPaperWidth? width = null);
}
