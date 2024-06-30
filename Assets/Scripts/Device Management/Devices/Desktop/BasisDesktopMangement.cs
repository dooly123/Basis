using System;
using UnityEngine;
[Serializable]
public class BasisDesktopManagement
{
    public BasisAvatarEyeInput BasisAvatarEyeInput;
    public void BeginDesktop()
    {
        if (BasisAvatarEyeInput == null)
        {
            BasisDeviceManagement.Instance.SetCameraRenderState(false);
            BasisDeviceManagement.Instance.CurrentMode = BasisBootedMode.Desktop;
            GameObject gameObject = new GameObject("Desktop Eye");
            if (BasisLocalPlayer.Instance != null)
            {
                gameObject.transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
            }
            BasisAvatarEyeInput = gameObject.AddComponent<BasisAvatarEyeInput>();
            BasisAvatarEyeInput.Initalize("Desktop Eye", nameof(BasisDesktopManagement));
            BasisDeviceManagement.Instance.TryAdd(BasisAvatarEyeInput);
        }
    }
    public void StopDesktop()
    {
        BasisDeviceManagement.Instance.BasisSimulateXR.StopXR();
        BasisDeviceManagement.Instance.RemoveDevicesFrom(nameof(BasisDesktopManagement), "Desktop Eye");
    }
}