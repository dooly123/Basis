using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public BasisOpenVRManagement BasisOpenVRManagement = new BasisOpenVRManagement();
    [SerializeField]
    public BasisOpenXRManagement BasisOpenXRManagement = new BasisOpenXRManagement();
    [SerializeField]
    public BasisDeviceNameMatcher BasisDeviceNameMatcher;
    // Define the delegate
    public delegate Task InitializationCompletedHandler();

    // Define the event based on the delegate
    public event InitializationCompletedHandler OnInitializationCompleted;
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
        BasisXRManagement.CheckForPass += CheckForPass;
        BasisXRManagement.Initalize();
        OnInitializationCompleted += RunAfterInitialized;
        await OnInitializationCompleted?.Invoke();
    }
    public async Task RunAfterInitialized()
    {
      //  await LoadGameobject("NetworkManagement", new InstantiationParameters());
    }

    private void CheckForPass(BasisBootedMode type)
    {
        Debug.Log("Loading " + type);
        switch (type)
        {
            case BasisBootedMode.OpenVRLoader:
                BasisOpenVRManagement ??= new BasisOpenVRManagement();
                BasisOpenVRManagement.StartXRSDK();
                SetCameraRenderState(true);
                break;
            case BasisBootedMode.OpenXRLoader:
                BasisOpenXRManagement ??= new BasisOpenXRManagement();
                BasisOpenXRManagement.StartXRSDK();
                SetCameraRenderState(true);
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
    public void SwitchMode(BasisBootedMode newMode)
    {
        Debug.Log("killing off" + CurrentMode);
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
            case BasisBootedMode.OpenVRLoader:
                BasisXRManagement.BeginLoad();
                break;
            case BasisBootedMode.OpenXRLoader:
                BasisXRManagement.BeginLoad();
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
        if (BasisOpenXRManagement != null)
        {
            BasisOpenXRManagement.StopXR();
            BasisOpenXRManagement = null;
        }
        if (BasisOpenVRManagement != null)
        {
            BasisOpenVRManagement.StopXRSDK();
            BasisOpenVRManagement = null;
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
       var data = await AddressableResourceProcess.LoadAsGameObjectsAsync(PlayerAddressableID, InstantiationParameters);
        List<GameObject> Gameobjects = data.Item1;
        if (Gameobjects.Count != 0)
        {
        }
        else
        {
            Debug.LogError("Missing ");
        }
        return null;
    }
    public static void ForceLoadXR()
    {
        if (Instance != null)
        {
            Instance.SwitchMode(BasisBootedMode.OpenVRLoader);
        }
    }
    public static void ForceSetDesktop()
    {
        if (Instance != null)
        {
            Instance.SwitchMode(BasisBootedMode.Desktop);
        }
    }
}