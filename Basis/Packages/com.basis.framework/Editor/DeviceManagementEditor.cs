using UnityEditor;

namespace Basis.Scripts.Device_Management.Editor
{
public static class DeviceManagementEditor
{
    [MenuItem("Basis/ForceLoadXR")]
    public static void ForceLoadXR()
    {
        BasisDeviceManagement.ForceLoadXR();
    }
    [MenuItem("Basis/ForceSetDesktop")]
    public static void ForceSetDesktop()
    {
        BasisDeviceManagement.ForceSetDesktop();
    }
}
}