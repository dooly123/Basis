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
            BasisAvatarEyeInput.SubSystem = nameof(BasisDesktopManagement);
            BasisAvatarEyeInput.Initalize("Desktop Eye");

            if (BasisDeviceManagement.Instance.AllInputDevices.Contains(BasisAvatarEyeInput) == false)
            {
                BasisDeviceManagement.Instance.AllInputDevices.Add(BasisAvatarEyeInput);
            }
            else
            {
                Debug.LogError("already added a Input Device thats identical!");
            }
        }
    }
    public void StopDesktop()
    {
        BasisDeviceManagement.Instance.BasisSimulateXR.StopXR();
        BasisDeviceManagement.Instance.RemoveDevicesFrom(nameof(BasisDesktopManagement), "Desktop Eye");
    }
}