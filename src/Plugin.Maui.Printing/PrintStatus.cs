namespace Plugin.Maui.Printing;

/// <summary>
/// Outcome of a print attempt.
/// </summary>
public enum PrintStatus
{
    /// <summary>The job was submitted or the printer accepted the payload.</summary>
    Completed = 0,

    /// <summary>The user dismissed the system print UI.</summary>
    Cancelled = 1,

    /// <summary>No matching printer was found or Bluetooth is unavailable.</summary>
    PrinterUnavailable = 2,

    /// <summary>The job failed after it started.</summary>
    Failed = 3
}
