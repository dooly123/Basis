using Basis.Scripts.Addressable_Driver.Resource;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.Command_Line_Args;
using Basis.Scripts.Device_Management.Devices;
using Basis.Scripts.Drivers;
using Basis.Scripts.Player;
using Basis.Scripts.TransformBinders;
using Basis.Scripts.TransformBinders.BoneControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Basis.Scripts.Device_Management
{
    public partial class BasisDeviceManagement : MonoBehaviour
    {
        public bool FireOffNetwork = true;
        public bool HasEvents = false;
        public const string InvalidConst = "Invalid";
        public string[] BakedInCommandLineArgs = new string[] { };
        public static string NetworkManagement = "NetworkManagement";
        public string CurrentMode = "None";
        [SerializeField]
        public const string Desktop = "Desktop";
        public string DefaultMode()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return "OpenXRLoader";
            }
            else
            {
                return Desktop;
            }
        }
        /// <summary>
        /// checks to see if we are in desktop
        /// this being false does not mean its vr.
        ///
        /// </summary>
        /// <returns></returns>
        public static bool IsUserInDesktop()
        {
            if(BasisDeviceManagement.Instance == null)
            {
                return false;
            }
            if (Desktop == BasisDeviceManagement.Instance.CurrentMode)
            {
                return true;
            }
            return false;
        }
        public static BasisDeviceManagement Instance;
        public BasisOpusSettings BasisOpusSettings;
        public event Action<string> OnBootModeChanged;
        public event Action<string> OnBootModeStopped;
        public delegate Task InitializationCompletedHandler();
        public event InitializationCompletedHandler OnInitializationCompleted;
        public BasisDeviceNameMatcher BasisDeviceNameMatcher;
        [SerializeField]
        public BasisObservableList<BasisInput> AllInputDevices = new BasisObservableList<BasisInput>();
        [SerializeField]
        public BasisXRManagement BasisXRManagement = new BasisXRManagement();
        [SerializeField]
        public List<BasisBaseTypeManagement> BaseTypes = new List<BasisBaseTypeManagement>();
        [SerializeField]
        public List<BasisLockToInput> BasisLockToInputs = new List<BasisLockToInput>();
        [SerializeField]
        public List<StoredPreviousDevice> PreviouslyConnectedDevices = new List<StoredPreviousDevice>();
        [SerializeField]
        public List<BasisDeviceMatchSettings> UseAbleDeviceConfigs = new List<BasisDeviceMatchSettings>();
        async void Start()
        {
            if (BasisHelpers.CheckInstance<BasisDeviceManagement>(Instance))
            {
                Instance = this;
            }
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            await Initialize();
        }
        void OnDestroy()
        {
            ShutDownXR(true);
            if (TryFindBasisBaseTypeManagement(Desktop, out List<BasisBaseTypeManagement> Matched))
            {
                foreach (var m in Matched)
                {
                    m.StopSDK();
                }
            }
            if (TryFindBasisBaseTypeManagement("SimulateXR", out Matched))
            {
                foreach (var m in Matched)
                {
                    m.StopSDK();
                }
            }
            if (HasEvents)
            {
                BasisXRManagement.CheckForPass -= CheckForPass;

                OnInitializationCompleted -= RunAfterInitialized;
            }
        }
        public static void UnassignFBTrackers()
        {
            foreach (BasisInput Input in BasisDeviceManagement.Instance.AllInputDevices)
            {
                Input.UnAssignFBTracker();
            }
        }
        public bool TryFindBasisBaseTypeManagement(string Name, out List<BasisBaseTypeManagement> Match)
        {
            Match = new List<BasisBaseTypeManagement>();
            foreach (BasisBaseTypeManagement Type in BaseTypes)
            {
                if (Type.Type() == Name)
                {
                    Match.Add(Type);
                }
            }
            if (Match.Count == 0)
            {
                return false;
            }
            return true;
        }
        public async Task Initialize()
        {
            CommandLineArgs.Initialize(BakedInCommandLineArgs,out string ForcedDevicemanager);
            LoadAndOrSaveDefaultDeviceConfigs();
            InstantiationParameters parameters = new InstantiationParameters();
            await BasisPlayerFactory.CreateLocalPlayer(parameters);

            if (string.IsNullOrEmpty(ForcedDevicemanager))
            {
                SwitchMode(DefaultMode());
            }
            else
            {
                SwitchMode(ForcedDevicemanager);
            }
            if (HasEvents == false)
            {
                BasisXRManagement.CheckForPass += CheckForPass;

                OnInitializationCompleted += RunAfterInitialized;
                HasEvents = true;
            }
            await OnInitializationCompleted?.Invoke();
        }
        public void LoadAndOrSaveDefaultDeviceConfigs()
        {
            LoadAndOrSaveDefaultDeviceConfigs(Application.persistentDataPath + "/Devices");
        }
        public async Task RunAfterInitialized()
        {
            if (FireOffNetwork)
            {
                await LoadGameobject(NetworkManagement, new InstantiationParameters());
            }
        }
        public void SwitchMode(string newMode)
        {
            if (CurrentMode != "None")
            {
                BasisDebug.Log("killing off " + CurrentMode, BasisDebug.LogTag.Device);
                if (newMode == "Desktop")
                {
                    ShutDownXR();
                }
                else
                {
                    foreach (BasisBaseTypeManagement Type in BaseTypes)
                    {
                        if (Type.Type() == Desktop)
                        {
                            Type.StopSDK();
                        }
                    }
                }
            }

            CurrentMode = newMode;
            if (newMode != "Desktop" && newMode != "Exiting")
            {
                BasisCursorManagement.UnlockCursorBypassChecks("Forceful Unlock From Device Management");
            }
            OnBootModeChanged?.Invoke(CurrentMode);

            BasisDebug.Log("Loading " + CurrentMode, BasisDebug.LogTag.Device);

            switch (CurrentMode)
            {
                case "OpenVRLoader":
                case "OpenXRLoader":
                    BasisXRManagement.BeginLoad();
                    break;
                case "Desktop":
                    if (TryFindBasisBaseTypeManagement(Desktop, out List<BasisBaseTypeManagement> Matched))
                    {
                        foreach (var m in Matched)
                        {
                            m.BeginLoadSDK();
                        }
                    }
                    break;
                case "Exiting":
                    break;
                default:
                    BasisDebug.LogError("This should not occur (default)");
                    if (TryFindBasisBaseTypeManagement("Desktop", out Matched))
                    {
                        foreach (var m in Matched)
                        {
                            m.BeginLoadSDK();
                        }
                    }
                    break;
            }
        }
        public void ShutDownXR(bool isExiting = false)
        {
            if (TryFindBasisBaseTypeManagement("OpenVRLoader", out List<BasisBaseTypeManagement> Matched))
            {
                foreach (var m in Matched)
                {
                    m.StopSDK();
                }
            }
            if (TryFindBasisBaseTypeManagement("OpenXRLoader", out Matched))
            {
                foreach (var m in Matched)
                {
                    m.StopSDK();
                }
            }
            if (TryFindBasisBaseTypeManagement("SimulateXR", out Matched))
            {
                foreach (var m in Matched)
                {
                    m.StopSDK();
                }
            }
            BasisXRManagement.StopXR(isExiting);
            AllInputDevices.RemoveAll(item => item == null);

            OnBootModeStopped?.Invoke(CurrentMode);
        }
        public static async Task LoadGameobject(string playerAddressableID, InstantiationParameters instantiationParameters)
        {
            ChecksRequired Required = new ChecksRequired();
            Required.UseContentRemoval = false;
            (List<GameObject>, Addressable_Driver.AddressableGenericResource) data = await AddressableResourceProcess.LoadAsGameObjectsAsync(playerAddressableID, instantiationParameters, Required);
            List<GameObject> gameObjects = data.Item1;

            if (gameObjects.Count == 0)
            {
                BasisDebug.LogError("Missing ");
            }
        }
        public static void ForceLoadXR()
        {
            SwitchSetMode("OpenVRLoader");
        }
        public static void ForceSetDesktop()
        {
            SwitchSetMode("Desktop");
        }
        public static void SwitchSetMode(string Mode)
        {
            if (Instance != null && Mode != Instance.CurrentMode)
            {
               Instance.SwitchMode(Mode);
            }
        }
        public static void ShowTrackers()
        {
             ShowTrackersAsync();
        }
        public void SetCameraRenderState(bool state)
        {
            BasisLocalCameraDriver.Instance.CameraData.allowXRRendering = state;
        }
        public static void ShowTrackersAsync()
        {
            var inputDevices = Instance.AllInputDevices;
            for (int Index = 0; Index < inputDevices.Count; Index++)
            {
                inputDevices[Index].ShowTrackedVisual();
            }
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
                    if (device.SubSystemIdentifier == SubSystem && device.UniqueDeviceIdentifier == id)
                    {
                        CacheDevice(device);
                        AllInputDevices[Index] = null;
                        GameObject.Destroy(device.gameObject);
                    }
                }
            }

            AllInputDevices.RemoveAll(item => item == null);
        }
        private void CheckForPass(string type)
        {
            BasisDebug.Log("Loading " + type, BasisDebug.LogTag.Device);
            if (TryFindBasisBaseTypeManagement("SimulateXR", out List<BasisBaseTypeManagement> Matched))
            {
                foreach (var m in Matched)
                {
                    m.StartSDK();
                }
            }
            if (TryFindBasisBaseTypeManagement(type, out Matched))
            {
                foreach (var m in Matched)
                {
                    m.StartSDK();
                }
            }
        }
        public bool TryAdd(BasisInput basisXRInput)
        {
            if (AllInputDevices.Contains(basisXRInput) == false)
            {
                AllInputDevices.Add(basisXRInput);
                if (RestoreDevice(basisXRInput.SubSystemIdentifier, basisXRInput.UniqueDeviceIdentifier, out StoredPreviousDevice PreviousDevice))
                {
                    if (CheckBeforeOverride(PreviousDevice))
                    {
                        StartCoroutine(RestoreInversetOffsets(basisXRInput, PreviousDevice));
                    }
                    else
                    {
                        BasisDebug.Log("bailing out of restore already has a replacement", BasisDebug.LogTag.Device);
                    }
                }
                return true;
            }
            else
            {
                BasisDebug.LogError("already added a Input Device thats identical!", BasisDebug.LogTag.Device);
            }
            return false;
        }
        IEnumerator RestoreInversetOffsets(BasisInput basisXRInput, StoredPreviousDevice PreviousDevice)
        {
            yield return new WaitForEndOfFrame();
            if (basisXRInput != null && basisXRInput.Control != null)
            {
                if (CheckBeforeOverride(PreviousDevice))
                {
                    BasisDebug.Log("device is restored " + PreviousDevice.trackedRole, BasisDebug.LogTag.Device);
                    basisXRInput.ApplyTrackerCalibration(PreviousDevice.trackedRole);
                    basisXRInput.Control.InverseOffsetFromBone = PreviousDevice.InverseOffsetFromBone;
                }
            }

        }
        public bool CheckBeforeOverride(StoredPreviousDevice Stored)
        {
            foreach (var device in AllInputDevices)
            {
                if (device.TryGetRole(out BasisBoneTrackedRole Role))
                {
                    if (Role == Stored.trackedRole)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool FindDevice(out BasisInput FindDevice, BasisBoneTrackedRole FindRole)
        {
            foreach (var device in AllInputDevices)
            {
                if (device != null && device.Control != null)
                {
                    if (device.Control.HasBone)
                    {
                        if (device.TryGetRole(out BasisBoneTrackedRole Role))
                        {
                            if (Role == FindRole)
                            {
                                FindDevice = device;
                                return true;

                            }
                        }
                    }
                }
            }
            FindDevice = null;
            return false;
        }
        public void CacheDevice(BasisInput DevicesThatsGettingPurged)
        {
            if (DevicesThatsGettingPurged.TryGetRole(out BasisBoneTrackedRole Role) && DevicesThatsGettingPurged.Control != null)
            {
                StoredPreviousDevice StoredPreviousDevice = new StoredPreviousDevice
                { InverseOffsetFromBone = DevicesThatsGettingPurged.Control.InverseOffsetFromBone }; ;

                StoredPreviousDevice.trackedRole = Role;
                StoredPreviousDevice.hasRoleAssigned = DevicesThatsGettingPurged.hasRoleAssigned;
                StoredPreviousDevice.SubSystem = DevicesThatsGettingPurged.SubSystemIdentifier;
                StoredPreviousDevice.UniqueID = DevicesThatsGettingPurged.UniqueDeviceIdentifier;
                PreviouslyConnectedDevices.Add(StoredPreviousDevice);
            }
        }
        public bool RestoreDevice(string SubSystem, string id, out StoredPreviousDevice StoredPreviousDevice)
        {
            foreach (StoredPreviousDevice Device in PreviouslyConnectedDevices)
            {
                if (Device.UniqueID == id && Device.SubSystem == SubSystem)
                {
                    BasisDebug.Log("this device is restoreable restoring..", BasisDebug.LogTag.Device);
                    PreviouslyConnectedDevices.Remove(Device);
                    StoredPreviousDevice = Device;
                    return true;
                }
            }
            StoredPreviousDevice = null;
            return false;
        }
        public void LoadAndOrSaveDefaultDeviceConfigs(string directoryPath)
        {
            var builtInDevices = BasisDeviceNameMatcher.BasisDevice;
            //save to disc any that do not exist
            BasisDeviceLoaderAndSaver.SaveDevices(directoryPath, builtInDevices);
            //now lets load them all and override versions that are outdated.
            List<BasisDeviceMatchSettings> loadedDevices = BasisDeviceLoaderAndSaver.LoadDeviceAsync(directoryPath);

            // Dictionary to store devices by DeviceID for quick lookup
            var deviceDictionary = builtInDevices.ToDictionary(
                device => string.IsNullOrEmpty(device.DeviceID) ? InvalidConst : device.DeviceID,
                device => device
            );

            foreach (var loadedDevice in loadedDevices)
            {
                var loadedDeviceID = string.IsNullOrEmpty(loadedDevice.DeviceID) ? InvalidConst : loadedDevice.DeviceID;

                if (deviceDictionary.TryGetValue(loadedDeviceID, out var existingDevice))
                {
                    // Replace the built-in device if the loaded one has a higher version number
                    if (loadedDevice.VersionNumber > existingDevice.VersionNumber)
                    {
                        deviceDictionary[loadedDeviceID] = loadedDevice;
                    }
                }
                else
                {
                    // Add the new loaded device
                    deviceDictionary[loadedDeviceID] = loadedDevice;
                }
            }

            UseAbleDeviceConfigs = deviceDictionary.Values.ToList();
        }
    }
}
