#if ANDROID
using Android;

namespace Plugin.Maui.Printing;

sealed class BluetoothPrintPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
    {
        get
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                return
                [
                    (Manifest.Permission.BluetoothConnect, true),
                    (Manifest.Permission.BluetoothScan, true)
                ];
            }

            return
            [
                (Manifest.Permission.Bluetooth, true),
                (Manifest.Permission.AccessFineLocation, true)
            ];
        }
    }
}
#endif
