using Basis.Scripts.Device_Management;
using System;
using UnityEngine;

namespace Basis.Scripts.Command_Line_Args
{
public static class CommandLineArgs
{
    public static void Initialize()
    {
        string[] args = Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if (arg.Equals("--disable-openxr", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("Disabling OpenXR");
                BasisDeviceManagement.Instance.BasisXRManagement.ForceDisableXRSolution(BasisBootedMode.OpenXRLoader);
            }
            else
            {
                if (arg.Equals("--disable-openvr", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log("Disabling OpenVR");
                    BasisDeviceManagement.Instance.BasisXRManagement.ForceDisableXRSolution(BasisBootedMode.OpenVRLoader);
                }
            }
        }
    }
}
}