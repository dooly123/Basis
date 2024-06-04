using SteamAudio;
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
    }
    public void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= beginCameraRendering;
        if (LocalPlayer.LocalAvatarDriver && LocalPlayer.LocalAvatarDriver.References != null && LocalPlayer.LocalAvatarDriver.References.head != null)
        {
            LocalPlayer.LocalAvatarDriver.References.head.localScale = LocalPlayer.LocalAvatarDriver.HeadScale;
        }
    }
    public void beginCameraRendering(ScriptableRenderContext context, Camera Camera)
    {
        if (LocalPlayer.HasAvatarDriver)
        {
            if (Camera.GetInstanceID() == CameraInstanceID)
            {
                LocalPlayer.LocalAvatarDriver.References.head.localScale = LocalPlayer.LocalAvatarDriver.HeadScaledDown;
            }
            else
            {
                LocalPlayer.LocalAvatarDriver.References.head.localScale = LocalPlayer.LocalAvatarDriver.HeadScale;
            }
        }
    }
}
