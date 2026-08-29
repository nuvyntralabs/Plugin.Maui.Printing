namespace Plugin.Maui.Printing;

/// <summary>
/// Bluetooth / print permission state.
/// </summary>
public enum PrinterPermissionStatus
{
    /// <summary>Not yet checked.</summary>
    Unknown = 0,

    /// <summary>The app may talk to printers that need permission.</summary>
    Granted = 1,

    /// <summary>The user denied Bluetooth or nearby-device permission.</summary>
    Denied = 2,

    /// <summary>The OS restricted the permission.</summary>
    Restricted = 3,

    /// <summary>System printing does not need a runtime permission.</summary>
    NotRequired = 4
}
