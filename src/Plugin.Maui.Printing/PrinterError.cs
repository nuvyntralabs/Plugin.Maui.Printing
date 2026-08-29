namespace Plugin.Maui.Printing;

/// <summary>
/// Classifies a <see cref="PrinterException"/>.
/// </summary>
public enum PrinterError
{
    /// <summary>The request is missing required fields.</summary>
    InvalidRequest = 0,

    /// <summary>A file path does not exist.</summary>
    FileNotFound = 1,

    /// <summary>The current platform cannot print.</summary>
    NotSupported = 2,

    /// <summary>Bluetooth or print permission was denied.</summary>
    PermissionDenied = 3,

    /// <summary>No system, Bluetooth, or thermal printer is available.</summary>
    PrinterUnavailable = 4,

    /// <summary>Connecting to or writing the printer failed.</summary>
    IoFailure = 5,

    /// <summary>The payload cannot be printed on the chosen target.</summary>
    UnsupportedContent = 6
}
