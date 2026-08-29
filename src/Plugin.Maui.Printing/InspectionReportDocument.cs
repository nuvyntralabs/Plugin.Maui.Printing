namespace Plugin.Maui.Printing;

/// <summary>
/// Vehicle inspection report (checklist + VIN / registration).
/// </summary>
public sealed class InspectionReportDocument
{
    /// <summary>
    /// Gets the report number.
    /// </summary>
    public required string ReportNumber { get; init; }

    /// <summary>
    /// Gets when the inspection happened.
    /// </summary>
    public DateTimeOffset InspectedAt { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// Gets the VIN or registration number.
    /// </summary>
    public required string VehicleIdentifier { get; init; }

    /// <summary>
    /// Gets the inspector name.
    /// </summary>
    public string? Inspector { get; init; }

    /// <summary>
    /// Gets checklist rows.
    /// </summary>
    public IReadOnlyList<InspectionItem> Items { get; init; } = [];

    /// <summary>
    /// Gets an overall summary.
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// Gets the job name.
    /// </summary>
    public string JobName => $"Inspection {ReportNumber}";
}
