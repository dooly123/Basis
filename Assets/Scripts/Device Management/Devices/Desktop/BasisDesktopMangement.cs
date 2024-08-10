using Basis.Scripts.BasisSdk.Players;
using System;
using UnityEngine;

namespace Basis.Scripts.Device_Management.Devices.Desktop
{
    [Serializable]
    public class BasisDesktopManagement : BasisBaseTypeManagement
    {
        public BasisAvatarEyeInput BasisAvatarEyeInput;
        public override void BeginLoadSDK()
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

        public override void StartSDK()
        {
        }
        public override void StopSDK()
        {
            BasisDeviceManagement.Instance.BasisSimulateXR.StopXR();
            BasisDeviceManagement.Instance.RemoveDevicesFrom(nameof(BasisDesktopManagement), "Desktop Eye");
        }

        public override string Type()
        {
            return "Desktop";
        }
    }
}