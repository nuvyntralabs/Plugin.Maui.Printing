namespace Plugin.Maui.Printing;

/// <summary>
/// One check on a vehicle inspection report.
/// </summary>
public sealed class InspectionItem
{
    /// <summary>
    /// Gets the checkpoint name (tires, lights, brakes).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the result, for example Pass, Fail, or N/A.
    /// </summary>
    public required string Result { get; init; }

    /// <summary>
    /// Gets an optional note.
    /// </summary>
    public string? Notes { get; init; }
}
