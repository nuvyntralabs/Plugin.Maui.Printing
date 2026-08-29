namespace Plugin.Maui.Printing;

/// <summary>
/// Raised while a Bluetooth write is in progress.
/// </summary>
public sealed class PrintProgressEventArgs : EventArgs
{
    /// <summary>
    /// Initializes progress args.
    /// </summary>
    public PrintProgressEventArgs(int bytesWritten, int totalBytes, string? stage = null)
    {
        BytesWritten = bytesWritten;
        TotalBytes = totalBytes;
        Stage = stage;
    }

    /// <summary>
    /// Gets how many bytes have been sent.
    /// </summary>
    public int BytesWritten { get; }

    /// <summary>
    /// Gets the payload size.
    /// </summary>
    public int TotalBytes { get; }

    /// <summary>
    /// Gets a short stage label (connecting, writing, cutting).
    /// </summary>
    public string? Stage { get; }

    /// <summary>
    /// Gets a 0–1 progress value.
    /// </summary>
    public double Progress => TotalBytes <= 0 ? 0 : (double)BytesWritten / TotalBytes;
}
