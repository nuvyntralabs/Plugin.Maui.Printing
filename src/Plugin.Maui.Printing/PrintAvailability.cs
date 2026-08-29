namespace Plugin.Maui.Printing;

/// <summary>
/// Point-in-time print capability.
/// </summary>
public sealed class PrintAvailability
{
    /// <summary>
    /// Gets whether native print APIs exist on this target.
    /// </summary>
    public required bool IsSupported { get; init; }

    /// <summary>
    /// Gets whether the system print UI can be shown.
    /// </summary>
    public bool CanUseSystemPrinter { get; init; }

    /// <summary>
    /// Gets whether Bluetooth Classic or BLE is present.
    /// </summary>
    public bool CanUseBluetooth { get; init; }

    /// <summary>
    /// Gets whether Bluetooth is powered on.
    /// </summary>
    public bool BluetoothEnabled { get; init; }

    /// <summary>
    /// Gets a short description of the native stack.
    /// </summary>
    public string? Platform { get; init; }
}
