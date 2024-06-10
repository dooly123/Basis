using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

public class BasisDeviceManagement : MonoBehaviour
{
    public BasisBootedMode CurrentMode = BasisBootedMode.Desktop;
    public static BasisDeviceManagement Instance;
    public BasisAvatarEyeInput BasisAvatarEyeInput;
    public BasisOpusSettings BasisOpusSettings;
    public event Action<BasisBootedMode> OnBootModeChanged;
    public event Action<BasisBootedMode> OnBootModeStopped;
    [SerializeField]
    public BasisXRManagement BasisXRManagement = new BasisXRManagement();
    [SerializeField]
    public BasisOpenVRManagement BasisOpenVRManagement;
    [SerializeField]
    public BasisOpenXRManagement BasisOpenXRManagement;
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
        SwitchMode(CurrentMode,true);
        await LoadGameobject("NetworkManagement", new InstantiationParameters());
        BasisXRManagement.CheckForPass += CheckForPass;
        BasisXRManagement.Initalize();
    }
    private void CheckForPass(BasisXRManagement.LoaderType type)
    {
        Debug.Log("Loading " + type);
        switch (type)
        {
            case BasisXRManagement.LoaderType.OpenVRLoader:
                BasisOpenVRManagement ??= new BasisOpenVRManagement();
                BasisOpenVRManagement.StartXRSDK();
                SetCameraRenderState(true);
                break;
            case BasisXRManagement.LoaderType.OpenXRLoader:
                BasisOpenXRManagement ??= new BasisOpenXRManagement();
                BasisOpenXRManagement.StartXRSDK();
                SetCameraRenderState(true);
                break;
            case BasisXRManagement.LoaderType.NotVR:
                UseFallBack();
                break;
            default:
                Debug.LogError("This should not occur (default)");
                UseFallBack();
                break;
        }
    }
    public void SwitchMode(BasisBootedMode newMode,bool ForceRefresh = false)
    {
        if (CurrentMode != newMode || ForceRefresh)
        {
            Debug.Log("killing off" +  CurrentMode);
            if (newMode != BasisBootedMode.Desktop)
            {
                StopDesktop();
            }
            else
            {
                ShutDownXR();
            }
            CurrentMode = newMode;
            OnBootModeChanged?.Invoke(CurrentMode); // Trigger the mode change event

            Debug.Log("Loading " + CurrentMode);
            switch (CurrentMode)
            {
                case BasisBootedMode.OpenVR:
                    BasisXRManagement.BeginLoad(BasisXRManagement.LoaderType.OpenVRLoader);
                    break;
                case BasisBootedMode.OpenXR:
                    BasisXRManagement.BeginLoad(BasisXRManagement.LoaderType.OpenXRLoader);
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
        else
        {
            Debug.Log("Skipping Switch As Already Initalized");
        }
    }

    public void SetCameraRenderState(bool state)
    {
        BasisLocalCameraDriver.Instance.CameraData.allowXRRendering = state;
    }

    public void UseFallBack()
    {
        Debug.Log("Booting Desktop");
        SetCameraRenderState(false);
        BasisAvatarEyeInput = BasisHelpers.GetOrAddComponent<BasisAvatarEyeInput>(this.gameObject);
        CurrentMode = BasisBootedMode.Desktop;
    }

    public void OnDestroy()
    {
        ShutDownXR();
        StopDesktop();
    }

    public void ShutDownXR()
    {
        OnBootModeStopped?.Invoke(CurrentMode); // Trigger the mode stop event
        if (BasisOpenVRManagement != null)
        {
            BasisOpenVRManagement.StopXRSDK();
            BasisOpenVRManagement = null;
        }
        if (BasisOpenXRManagement != null)
        {
            BasisOpenXRManagement.StopXR();
            BasisOpenXRManagement = null;
        }
        BasisXRManagement.StopXR();
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
#if UNITY_EDITOR
    [MenuItem("Basis/ForceSetOpenXR")]
    public static void ForceSetOpenXR()
    {
        if (Instance != null) Instance.SwitchMode(BasisBootedMode.OpenXR);
    }

    [MenuItem("Basis/ForceSetOpenVR")]
    public static void ForceSetOpenVR()
    {
        if (Instance != null) Instance.SwitchMode(BasisBootedMode.OpenVR);
    }

    [MenuItem("Basis/ForceSetDesktop")]
    public static void ForceSetDesktop()
    {
        if (Instance != null) Instance.SwitchMode(BasisBootedMode.Desktop);
    }
#else
    public static void ForceSetOpenXR()
    {
        if (Instance != null) Instance.SwitchMode(BasisBootedMode.OpenXR);
    }

    public static void ForceSetOpenVR()
    {
        if (Instance != null) Instance.SwitchMode(BasisBootedMode.OpenVR);
    }

    public static void ForceSetDesktop()
    {
        if (Instance != null) Instance.SwitchMode(BasisBootedMode.Desktop);
    }
#endif
}