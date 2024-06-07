using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

public partial class BasisDeviceManagement : MonoBehaviour
{
    public BasisBootedMode CurrentMode = BasisBootedMode.Desktop;
    public static BasisDeviceManagement Instance;
    public BasisAvatarEyeInput BasisAvatarEyeInput;
    public BasisOpusSettings BasisOpusSettings;
    // Define the events
    public event Action<BasisBootedMode> OnBootModeChanged;
    public event Action<BasisBootedMode> OnBootModeStopped;

    void Start()
    {
        if (BasisHelpers.CheckInstance<BasisDeviceManagement>(Instance))
        {
            Instance = this;
        }
        Initialize();
    }

    public async void Initialize()
    {
        InstantiationParameters Parameters = new InstantiationParameters();
        await BasisPlayerFactory.CreateLocalPlayer(Parameters);
        BasisOverrideRotations BasisXRHeadToBodyOverride = BasisHelpers.GetOrAddComponent<BasisOverrideRotations>(this.gameObject);
        BasisXRHeadToBodyOverride.Initialize();
        SwitchMode(CurrentMode);
        await LoadGameobject("NetworkManagement", new InstantiationParameters());
    }

#if UNITY_EDITOR
    [MenuItem("Basis/ForceSetOpenXR")]
    public static void ForceSetOpenXR()
    {
        Instance.SwitchMode(BasisBootedMode.OpenXR);
    }

    [MenuItem("Basis/ForceSetOpenVR")]
    public static void ForceSetOpenVR()
    {
        Instance.SwitchMode(BasisBootedMode.OpenVR);
    }

    [MenuItem("Basis/ForceSetDesktop")]
    public static void ForceSetDesktop()
    {
        Instance.SwitchMode(BasisBootedMode.Desktop);
    }
#else
    public static void ForceSetOpenXR()
    {
        Instance.SwitchMode(BasisBootedMode.OpenXR);
    }

    public static void ForceSetOpenVR()
    {
        Instance.SwitchMode(BasisBootedMode.OpenVR);
    }

    public static void ForceSetDesktop()
    {
        Instance.SwitchMode(BasisBootedMode.Desktop);
    }
#endif

    public void SwitchMode(BasisBootedMode newMode)
    {
        if (CurrentMode != newMode)
        {
            ShutDownLastMode();
            CurrentMode = newMode;
            OnBootModeChanged?.Invoke(CurrentMode); // Trigger the mode change event
        }

        switch (CurrentMode)
        {
            case BasisBootedMode.OpenVR:
                if (BasisOpenVRManagement.TryStartOpenVR())
                {
                    SetCameraRenderState(true);
                }
                else
                {
                    UseFallBack();
                }
                break;
            case BasisBootedMode.OpenXR:
                if (BasisOpenXRManagement.TryStartOpenXR())
                {
                    SetCameraRenderState(true);
                }
                else
                {
                    UseFallBack();
                }
                break;
            case BasisBootedMode.Desktop:
                UseFallBack();
                break;
            default:
                Debug.LogError("This should not occur (default)");
                UseFallBack();
                break;
        }
    }

    public void SetCameraRenderState(bool state)
    {
        BasisLocalCameraDriver.Instance.CameraData.allowXRRendering = state;
    }

    public void UseFallBack()
    {
        SetCameraRenderState(false);
        BasisAvatarEyeInput = BasisHelpers.GetOrAddComponent<BasisAvatarEyeInput>(this.gameObject);
        CurrentMode = BasisBootedMode.Desktop;
    }

    public void OnDestroy()
    {
        ShutDownLastMode();
    }

    public void ShutDownLastMode()
    {
        OnBootModeStopped?.Invoke(CurrentMode); // Trigger the mode stop event

        BasisOpenVRManagement.StopXRSDK();
        BasisOpenXRManagement.StopXR();
        StopDesktop();

        switch (CurrentMode)
        {
            case BasisBootedMode.OpenVR:
                break;
            case BasisBootedMode.OpenXR:
                break;
            case BasisBootedMode.Desktop:
                break;
        }
    }

    public void StopDesktop()
    {
        if (BasisAvatarEyeInput != null)
        {
            GameObject.Destroy(BasisAvatarEyeInput);
        }
    }

    public static async Task<BasisPlayer> LoadGameobject(string PlayerAddressableID, InstantiationParameters InstantiationParameters)
    {
        List<GameObject> Gameobjects = await AddressableResourceProcess.LoadAsGameObjectsAsync(PlayerAddressableID, InstantiationParameters);
        if (Gameobjects.Count != 0)
        {
        }
        else
        {
            Debug.LogError("Missing ");
        }
        return null;
    }
}