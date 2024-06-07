using SteamAudio;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BasisLocalCameraDriver : MonoBehaviour
{
    public static BasisLocalCameraDriver Instance;
    public Camera Camera;
    public int CameraInstanceID;
    public AudioListener Listener;
    public UniversalAdditionalCameraData CameraData;
    public SteamAudioListener SteamAudioListener;
    public BasisLockToPositionBinder CamerasLockToPosition;
    public BasisLocalPlayer LocalPlayer;
    public int DefaultCameraFov = 90;
    public void OnEnable()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        LocalPlayer = BasisLocalPlayer.Instance;
        Camera.nearClipPlane = 0.01f;
        Camera.farClipPlane = 1500;
        CameraInstanceID = Camera.GetInstanceID();
        CamerasLockToPosition.CharacterTransformDriver = LocalPlayer.LocalBoneDriver;
        if (CamerasLockToPosition.CharacterTransformDriver == null)
        {
            Debug.LogError("missing Character Tranform Driver");
        }
        CamerasLockToPosition.Initialize(LocalPlayer);
        RenderPipelineManager.beginCameraRendering += beginCameraRendering;
        BasisDeviceManagement.Instance.OnBootModeChanged += OnModeSwitch;
    }

    private void OnModeSwitch(BasisDeviceManagement.BasisBootedMode mode)
    {
        if(mode == BasisDeviceManagement.BasisBootedMode.Desktop)
        {
            Camera.fieldOfView = DefaultCameraFov;
        }
    }
    public void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= beginCameraRendering;
        if (LocalPlayer.AvatarDriver && LocalPlayer.AvatarDriver.References != null && LocalPlayer.AvatarDriver.References.head != null)
        {
            LocalPlayer.AvatarDriver.References.head.localScale = LocalPlayer.AvatarDriver.HeadScale;
        }
    }
    public void beginCameraRendering(ScriptableRenderContext context, Camera Camera)
    {
        if (LocalPlayer.HasAvatarDriver)
        {
            if (Camera.GetInstanceID() == CameraInstanceID)
            {
                LocalPlayer.AvatarDriver.References.head.localScale = LocalPlayer.AvatarDriver.HeadScaledDown;
            }
            else
            {
                LocalPlayer.AvatarDriver.References.head.localScale = LocalPlayer.AvatarDriver.HeadScale;
            }
        }
    }
}
