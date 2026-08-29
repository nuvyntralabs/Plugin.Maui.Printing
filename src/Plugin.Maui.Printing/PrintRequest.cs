namespace Plugin.Maui.Printing;

/// <summary>
/// A document plus per-job options.
/// </summary>
public sealed class PrintRequest
{
    /// <summary>
    /// Gets the document to print.
    /// </summary>
    public required PrintDocument Document { get; init; }

    /// <summary>
    /// Gets per-job options. When omitted, <see cref="PrintingOptions"/> defaults apply.
    /// </summary>
    public PrintOptions? Options { get; init; }
}
