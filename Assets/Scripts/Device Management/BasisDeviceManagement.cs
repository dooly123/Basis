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
    public BasisBootedMode CurrentMode = BasisBootedMode.None;
    public BasisBootedMode DefaultMode = BasisBootedMode.Desktop;
    public static BasisDeviceManagement Instance;
    public BasisOpusSettings BasisOpusSettings;
    public event Action<BasisBootedMode> OnBootModeChanged;
    public event Action<BasisBootedMode> OnBootModeStopped;
    [SerializeField] public BasisObservableList<BasisInput> AllInputDevices = new BasisObservableList<BasisInput>();
    [SerializeField] public BasisXRManagement BasisXRManagement = new BasisXRManagement();
    [SerializeField] public BasisOpenVRManagement BasisOpenVRManagement = new BasisOpenVRManagement();
    [SerializeField] public BasisOpenXRManagement BasisOpenXRManagement = new BasisOpenXRManagement();
    [SerializeField] public BasisDesktopManagement BasisDesktopManagement = new BasisDesktopManagement();
    [SerializeField] public BasisSimulateXR BasisSimulateXR = new BasisSimulateXR();
    [SerializeField] public BasisDeviceNameMatcher BasisDeviceNameMatcher;
    [SerializeField] public List<BasisLockToInput> BasisLockToInputs = new List<BasisLockToInput>();
    public delegate Task InitializationCompletedHandler();
    public event InitializationCompletedHandler OnInitializationCompleted;
    public bool FireOffNetwork = true;
    void Start()
    {
        if (BasisHelpers.CheckInstance<BasisDeviceManagement>(Instance))
        {
            Instance = this;
        }
        Initialize();
    }
    void OnDestroy()
    {
        ShutDownXR(true);
        BasisDesktopManagement.StopDesktop();
        BasisSimulateXR.StopXR();
    }
    public async void Initialize()
    {
        InstantiationParameters parameters = new InstantiationParameters();
        await BasisPlayerFactory.CreateLocalPlayer(parameters);

        BasisOverrideRotations basisXRHeadToBodyOverride = BasisHelpers.GetOrAddComponent<BasisOverrideRotations>(this.gameObject);
        basisXRHeadToBodyOverride.Initialize();

        SwitchMode(DefaultMode);

        BasisXRManagement.CheckForPass += CheckForPass;

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
    public void SwitchMode(BasisBootedMode newMode)
    {
        if (CurrentMode != BasisBootedMode.None)
        {
            Debug.Log("killing off " + CurrentMode);
            if (newMode == BasisBootedMode.Desktop)
            {
                ShutDownXR();
            }
            else
            {
                BasisDesktopManagement.StopDesktop();
            }
        }

        CurrentMode = newMode;
        OnBootModeChanged?.Invoke(CurrentMode);

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
                BasisDesktopManagement.BeginDesktop();
                break;
            case BasisBootedMode.Exiting:
                break;
            default:
                Debug.LogError("This should not occur (default)");
                BasisDesktopManagement.BeginDesktop();
                break;
        }
    }
    public void SetCameraRenderState(bool state)
    {
        BasisLocalCameraDriver.Instance.CameraData.allowXRRendering = state;
    }
    public void ShutDownXR(bool isExiting = false)
    {
        BasisSimulateXR.StopXR();
        BasisOpenXRManagement.StopXRSDK();
        BasisOpenVRManagement.StopXRSDK();

        BasisXRManagement.StopXR(isExiting);
        AllInputDevices.RemoveAll(item => item == null);

        OnBootModeStopped?.Invoke(CurrentMode);
    }
    public static async Task<BasisPlayer> LoadGameobject(string playerAddressableID, InstantiationParameters instantiationParameters)
    {
        var data = await AddressableResourceProcess.LoadAsGameObjectsAsync(playerAddressableID, instantiationParameters);
        var gameObjects = data.Item1;

        if (gameObjects.Count == 0)
        {
            Debug.LogError("Missing ");
        }

        return null;
    }
    public static void ForceLoadXR()
    {
        SwitchSetMode(BasisBootedMode.OpenVRLoader);
    }
    public static void ForceSetDesktop()
    {
        SwitchSetMode(BasisBootedMode.Desktop);
    }
    public static void SwitchSetMode(BasisBootedMode Mode)
    {
        if (Instance != null && Mode != Instance.CurrentMode)
        {
            Instance.SwitchMode(Mode);
        }
    }
    public static async void ShowTrackers()
    {
        await ShowTrackersAsync();
    }
    public static async Task ShowTrackersAsync()
    {
        var inputDevices = Instance.AllInputDevices;
        var showTrackedVisualTasks = new List<Task>();

        foreach (var input in inputDevices)
        {
            showTrackedVisualTasks.Add(input.ShowTrackedVisual());
        }

        await Task.WhenAll(showTrackedVisualTasks);
    }
    public static void HideTrackers()
    {
        for (int Index = 0; Index < Instance.AllInputDevices.Count; Index++)
        {
            Instance.AllInputDevices[Index].HideTrackedVisual();
        }
    }
    public void RemoveDevicesFrom(string SubSystem, string id)
    {
        for (int Index = 0; Index < AllInputDevices.Count; Index++)
        {
            BasisInput device = AllInputDevices[Index];
            if (device != null)
            {
                if (device.SubSystem == SubSystem && device.UniqueID == id)
                {
                    AllInputDevices[Index] = null;
                    GameObject.Destroy(device.gameObject);
                }
            }
        }

        AllInputDevices.RemoveAll(item => item == null);
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
                BasisDesktopManagement.BeginDesktop();
                break;
            case BasisBootedMode.Exiting:
                break;
            default:
                Debug.LogError("This should not occur (default)");
                BasisDesktopManagement.BeginDesktop();
                break;
        }
    }
    public bool TryAdd(BasisInput basisXRInput)
    {
        if (AllInputDevices.Contains(basisXRInput) == false)
        {
            AllInputDevices.Add(basisXRInput);
            return true;
        }
        else
        {
            Debug.LogError("already added a Input Device thats identical!");
        }
         return false;
    }
    public bool FindDevice(out BasisInput FindDevice, BasisBoneTrackedRole FindRole)
    {
        foreach (var device in AllInputDevices)
        {
            if (device != null && device.Control != null)
            {
                if (device.Control.HasBone)
                {
                    if (BasisLocalPlayer.Instance.LocalBoneDriver.FindTrackedRole(device.Control, out BasisBoneTrackedRole role))
                    {
                        if (FindRole == role)
                        {
                            FindDevice = device;
                        }
                    }
                }
            }
        }
        FindDevice = null;
        return false;
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