namespace Plugin.Maui.Printing;

/// <summary>
/// Raised after a print attempt finishes.
/// </summary>
public sealed class PrintCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes event args with the print result.
    /// </summary>
    public PrintCompletedEventArgs(PrintResult result)
    {
        Result = result;
    }

    /// <summary>
    /// Gets the print result.
    /// </summary>
    public PrintResult Result { get; }
}
