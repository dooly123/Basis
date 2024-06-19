using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

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
    public event Action<BasisBootedMode> OnBootModeChanged;
    public event Action<BasisBootedMode> OnBootModeStopped;
    [SerializeField]
    public BasisObservableList<BasisInput> AllInputDevices = new BasisObservableList<BasisInput>();
    [SerializeField]
    public BasisXRManagement BasisXRManagement = new BasisXRManagement();
    [SerializeField]
    public BasisOpenVRManagement BasisOpenVRManagement = new BasisOpenVRManagement();
    [SerializeField]
    public BasisOpenXRManagement BasisOpenXRManagement = new BasisOpenXRManagement();
    [SerializeField]
    public BasisSimulateXR BasisSimulateXR = new BasisSimulateXR();
    [SerializeField]
    public BasisDeviceNameMatcher BasisDeviceNameMatcher;
    [SerializeField]
    public List<BasisLockToInput> basisLockToInputs = new List<BasisLockToInput>();
    // Define the delegate
    public delegate Task InitializationCompletedHandler();

    // Define the event based on the delegate
    public event InitializationCompletedHandler OnInitializationCompleted;

    public bool FireOffNetwork = false;
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
        if (FireOffNetwork)
        {
            await LoadGameobject("NetworkManagement", new InstantiationParameters());
        }
    }

    private void CheckForPass(BasisBootedMode type)
    {
        Debug.Log("Loading " + type);
        BasisSimulateXR.StartSimulation();
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
            case BasisBootedMode.Exiting:
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
        CurrentMode = BasisBootedMode.Desktop;
        GameObject gameObject = new GameObject("Desktop Eye");
        if(BasisLocalPlayer.Instance != null)
        {
            gameObject.transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
        }
        BasisAvatarEyeInput = gameObject.AddComponent<BasisAvatarEyeInput>();
        BasisAvatarEyeInput.Initalize();
        Instance.AllInputDevices.Add(BasisAvatarEyeInput);
    }

    public void OnDestroy()
    {
        ShutDownXR(true);
        StopDesktop();
        BasisSimulateXR.StopXR();
    }
    public void ShutDownXR(bool IsExiting = false)
    {
        BasisSimulateXR.StopXR();
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
        BasisXRManagement.StopXR(IsExiting);
    }

    public void StopDesktop()
    {
        BasisSimulateXR.StopXR();
        if (BasisAvatarEyeInput != null)
        {
            AllInputDevices.Remove(BasisAvatarEyeInput);
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
    public static async void ShowTrackers()
    {
        await ShowTrackersAsync();
    }
    public async static Task ShowTrackersAsync()
    {
        var inputDevices = BasisDeviceManagement.Instance.AllInputDevices;
        var showTrackedVisualTasks = new List<Task>();

        foreach (var input in inputDevices)
        {
            showTrackedVisualTasks.Add(input.ShowTrackedVisual());
        }

        await Task.WhenAll(showTrackedVisualTasks);
    }
    public static void HideTrackers()
    {
        foreach (var input in BasisDeviceManagement.Instance.AllInputDevices)
        {
            input.HideTrackedVisual();
        }
    }
#if UNITY_EDITOR
    [MenuItem("Basis/Hide Trackers")]
    public static void HideTrackersEditor()
    {
        HideTrackers();
    }
    [MenuItem("Basis/Show Trackers")]
    public static void ShowTrackersEditor()
    {
        ShowTrackers();
    }

#endif
}