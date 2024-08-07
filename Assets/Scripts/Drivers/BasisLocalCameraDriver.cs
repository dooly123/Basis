using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management;
using Basis.Scripts.TransformBinders;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Basis.Scripts.Drivers
{
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
    public bool HasEvents = false;
    public Canvas MicrophoneCanvas;
    public RawImage MicrophoneMutedIcon;
    public RawImage MicrophoneUnMutedIcon;

    public Vector3 DesktopMicrophoneOffset = new Vector3(-0.001f, -0.0015f, 2f); // Adjust as needed for canvas position and depth
    public Vector3 VRMicrophoneOffset = new Vector3(-0.0004f, -0.0015f, 2f);

    public AudioClip MuteSound;
    public AudioClip UnMuteSound;
    public AudioSource AudioSource;
    public float NearClip = 0.001f;
    public void OnEnable()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        LocalPlayer = BasisLocalPlayer.Instance;
        Camera.nearClipPlane = NearClip;
        Camera.farClipPlane = 1500;
        QualitySettings.maxQueuedFrames = -1;
        CameraInstanceID = Camera.GetInstanceID();
        //fire static event that says the instance exists
        OnHeightChanged();
        if (HasEvents == false)
        {
            MicrophoneRecorder.OnPausedAction += OnPausedEvent;
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            BasisDeviceManagement.Instance.OnBootModeChanged += OnModeSwitch;
            BasisLocalPlayer.Instance.OnPlayersHeightChanged += OnHeightChanged;
            InstanceExists?.Invoke();
            HasEvents = true;
        }
        OnPausedEvent(MicrophoneRecorder.isPaused);
    }

    private void OnPausedEvent(bool IsMuted)
    {
        if (IsMuted)
        {
            MicrophoneMutedIcon.gameObject.SetActive(true);
            MicrophoneUnMutedIcon.gameObject.SetActive(false);
            AudioSource.PlayOneShot(MuteSound);
        }
        else
        {
            MicrophoneMutedIcon.gameObject.SetActive(false);
            MicrophoneUnMutedIcon.gameObject.SetActive(true);
            AudioSource.PlayOneShot(UnMuteSound);
        }
    }

    public void OnDestroy()
    {
        MicrophoneRecorder.OnPausedAction -= OnPausedEvent;
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        BasisDeviceManagement.Instance.OnBootModeChanged -= OnModeSwitch;
        BasisLocalPlayer.Instance.OnPlayersHeightChanged -= OnHeightChanged;
        HasEvents = false;
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
        if (LocalPlayer.AvatarDriver && LocalPlayer.AvatarDriver.References != null && LocalPlayer.AvatarDriver.References.head != null)
        {
            LocalPlayer.AvatarDriver.References.head.localScale = LocalPlayer.AvatarDriver.HeadScale;
        }
        if (HasEvents)
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            BasisDeviceManagement.Instance.OnBootModeChanged -= OnModeSwitch;
            HasEvents = false;
        }
    }
    public void BeginCameraRendering(ScriptableRenderContext context, Camera Camera)
    {
        if (LocalPlayer.HasAvatarDriver && LocalPlayer.AvatarDriver.References.Hashead)
        {
            if (Camera.GetInstanceID() == CameraInstanceID)
            {
                if (LocalPlayer.AvatarDriver.References.head.localScale != LocalPlayer.AvatarDriver.HeadScaledDown)
                {
                    LocalPlayer.AvatarDriver.References.head.localScale = LocalPlayer.AvatarDriver.HeadScaledDown;
                }
                if (CameraData.allowXRRendering)
                {
                    MicrophoneCanvas.transform.localPosition = Camera.ViewportToScreenPoint(VRMicrophoneOffset);
                }
                else
                {
                    MicrophoneCanvas.transform.localPosition = Camera.ViewportToScreenPoint(DesktopMicrophoneOffset);
                }
            }
            else
            {
                if (LocalPlayer.AvatarDriver.References.head.localScale != LocalPlayer.AvatarDriver.HeadScale)
                {
                    LocalPlayer.AvatarDriver.References.head.localScale = LocalPlayer.AvatarDriver.HeadScale;
                }
            }
        }
    }
}
}