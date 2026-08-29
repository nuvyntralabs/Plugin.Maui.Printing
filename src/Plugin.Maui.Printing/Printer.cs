namespace Plugin.Maui.Printing;

/// <summary>
/// Entry point for printing when dependency injection is not used.
/// </summary>
public static class Printer
{
    static IPrinter? _current;

    /// <summary>
    /// Gets the shared <see cref="IPrinter"/> instance.
    /// </summary>
    public static IPrinter Current => _current ??= Create(new PrintingOptions());

    /// <summary>
    /// Always <c>true</c> on Android and iOS.
    /// </summary>
    public static bool IsSupported => Current.IsSupported;

    /// <summary>
    /// Raised after a print attempt finishes.
    /// </summary>
    public static event EventHandler<PrintCompletedEventArgs>? PrintCompleted
    {
        add => Current.PrintCompleted += value;
        remove => Current.PrintCompleted -= value;
    }

    /// <summary>
    /// Prints a document.
    /// </summary>
    /// <example>
    /// <code>
    /// await Printer.PrintAsync(PrintDocument.Pdf(invoicePath));
    /// await Printer.PrintAsync(receipt);
    /// </code>
    /// </example>
    public static Task<PrintResult> PrintAsync(PrintDocument document, PrintOptions? options = null, CancellationToken cancellationToken = default) =>
        Current.PrintAsync(document, options, cancellationToken);

    /// <summary>
    /// Prints a receipt document.
    /// </summary>
    public static Task<PrintResult> PrintAsync(ReceiptDocument receipt, PrintOptions? options = null, CancellationToken cancellationToken = default) =>
        Current.PrintReceiptAsync(receipt, options, cancellationToken);

    /// <summary>
    /// Prints using a request object.
    /// </summary>
    public static Task<PrintResult> PrintAsync(PrintRequest request, CancellationToken cancellationToken = default) =>
        Current.PrintAsync(request, cancellationToken);

    /// <summary>
    /// Prints a local PDF file.
    /// </summary>
    public static Task<PrintResult> PrintPdfAsync(string filePath, PrintOptions? options = null, CancellationToken cancellationToken = default) =>
        Current.PrintPdfAsync(filePath, options, cancellationToken);

    /// <summary>
    /// Prints a local image file.
    /// </summary>
    public static Task<PrintResult> PrintImageAsync(string filePath, PrintOptions? options = null, CancellationToken cancellationToken = default) =>
        Current.PrintImageAsync(filePath, options, cancellationToken);

    /// <summary>
    /// Prints plain text.
    /// </summary>
    public static Task<PrintResult> PrintTextAsync(string text, PrintOptions? options = null, CancellationToken cancellationToken = default) =>
        Current.PrintTextAsync(text, options, cancellationToken);

    /// <summary>
    /// Prints a thermal receipt.
    /// </summary>
    public static Task<PrintResult> PrintReceiptAsync(ReceiptDocument receipt, PrintOptions? options = null, CancellationToken cancellationToken = default) =>
        Current.PrintReceiptAsync(receipt, options, cancellationToken);

    /// <summary>
    /// Lists system and Bluetooth printers.
    /// </summary>
    public static Task<IReadOnlyList<PrinterInfo>> DiscoverPrintersAsync(PrinterDiscoveryOptions? options = null, CancellationToken cancellationToken = default) =>
        Current.DiscoverPrintersAsync(options, cancellationToken);

    /// <summary>
    /// Encodes a receipt as ESC/POS without sending it to a printer.
    /// </summary>
    public static byte[] EncodeEscPos(ReceiptDocument receipt, ThermalPaperWidth? width = null) =>
        Current.EncodeEscPos(receipt, width);

    /// <summary>
    /// Creates a printer client for the current platform.
    /// </summary>
    public static IPrinter Create(PrintingOptions? options = null)
    {
        options ??= new PrintingOptions();
        return new PrinterImplementation(options, CreatePlatform());
    }

    /// <summary>
    /// Replaces the shared instance. Intended for tests and custom implementations.
    /// </summary>
    public static void SetDefault(IPrinter implementation) =>
        _current = implementation ?? throw new ArgumentNullException(nameof(implementation));

    internal static PrinterImplementation Create(PrintingOptions options, IPrintPlatform platform) =>
        new(options, platform);

    static IPrintPlatform CreatePlatform()
    {
#if ANDROID
        return new AndroidPrintPlatform();
#elif IOS
        return new IosPrintPlatform();
#else
        return new UnsupportedPrintPlatform();
#endif
    }
}
