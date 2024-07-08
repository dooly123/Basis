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
    public SteamAudio.SteamAudioListener SteamAudioListener;
    public BasisLocalPlayer LocalPlayer;
    public int DefaultCameraFov = 90;
    // Static event to notify when the instance exists
    public static event System.Action InstanceExists;
    public BasisLockToInput BasisLockToInput;
    public void OnEnable()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        LocalPlayer = BasisLocalPlayer.Instance;
        Camera.nearClipPlane = 0.01f;
        Camera.farClipPlane = 1500;
        QualitySettings.maxQueuedFrames = -1;
        CameraInstanceID = Camera.GetInstanceID();
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        BasisDeviceManagement.Instance.OnBootModeChanged += OnModeSwitch;
        BasisLocalPlayer.Instance.OnPlayersHeightChanged += OnHeightChanged;
        //fire static event that says the instance exists
        OnHeightChanged();
        InstanceExists?.Invoke();
    }
    private void OnModeSwitch(BasisBootedMode mode)
    {
        if (mode == BasisBootedMode.Desktop)
        {
            Camera.fieldOfView = DefaultCameraFov;
        }
        OnHeightChanged();
    }
    public void OnHeightChanged()
    {
        this.gameObject.transform.localScale = Vector3.one * LocalPlayer.RatioPlayerToAvatarScale;
    }
    public void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        if (LocalPlayer.AvatarDriver && LocalPlayer.AvatarDriver.References != null && LocalPlayer.AvatarDriver.References.head != null)
        {
            LocalPlayer.AvatarDriver.References.head.localScale = LocalPlayer.AvatarDriver.HeadScale;
        }
        BasisDeviceManagement.Instance.OnBootModeChanged -= OnModeSwitch;
    }
    public void BeginCameraRendering(ScriptableRenderContext context, Camera Camera)
    {
        if (LocalPlayer.HasAvatarDriver && LocalPlayer.AvatarDriver.References.Hashead)
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