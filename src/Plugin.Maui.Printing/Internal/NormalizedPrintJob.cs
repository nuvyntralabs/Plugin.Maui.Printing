namespace Plugin.Maui.Printing;

sealed class NormalizedPrintJob
{
    public required PrintDocument Document { get; init; }

    public required PrintOptions Options { get; init; }

    public required PrinterKind RequestedTarget { get; init; }

    public required PrinterKind ResolvedTarget { get; init; }

    public required string JobName { get; init; }

    public string? FilePath { get; init; }

    public byte[]? Bytes { get; init; }

    public string? Text { get; init; }
}
