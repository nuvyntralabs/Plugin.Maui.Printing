namespace Plugin.Maui.Printing;

/// <summary>
/// Thrown when a print request cannot be started or the printer reports failure.
/// </summary>
public sealed class PrinterException : Exception
{
    /// <summary>
    /// Initializes a new exception with an error code and message.
    /// </summary>
    public PrinterException(PrinterError error, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Error = error;
    }

    /// <summary>
    /// Gets the classified error.
    /// </summary>
    public PrinterError Error { get; }
}
