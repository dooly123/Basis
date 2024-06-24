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
    public BasisLocalPlayer LocalPlayer;
    public int DefaultCameraFov = 90;
    // Static event to notify when the instance exists
    public static event System.Action InstanceExists;
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
        RenderPipelineManager.beginCameraRendering += beginCameraRendering;
        BasisDeviceManagement.Instance.OnBootModeChanged += OnModeSwitch;
        //fire static event that says the instance exists
        InstanceExists?.Invoke();
    }
    private void OnModeSwitch(BasisBootedMode mode)
    {
        if(mode == BasisBootedMode.Desktop)
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
