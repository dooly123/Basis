using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR.Management;
using Valve.VR;
public static class BasisOpenVRManagement
{
    public static XRManagerSettings XRInstance;
    public static List<OpenVRDevice> inputDevices = new List<OpenVRDevice>();
    /// <summary>
    /// generated at runtime
    /// </summary>
    public static List<BasisOpenVRInput> TrackedOpenVRInputDevices = new List<BasisOpenVRInput>();
    /// <summary>
    /// keeps track of generated IDs and match InputDevice
    /// </summary>
    public static Dictionary<string, OpenVRDevice> TypicalDevices = new Dictionary<string, OpenVRDevice>();
    public static bool TryStartOpenVR()
    {
        //finds the first working vr loader
        XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
        //start its subsystems
        XRInstance = XRGeneralSettings.Instance.Manager;
        StartXRSDK();
        if (XRInstance.activeLoader == null)
        {
            return false;
        }
        return true;
    }
    private static void StartXRSDK()
    {
        if (XRInstance != null && XRInstance.activeLoader != null)
        {
            XRInstance.StartSubsystems();
        }

        SteamVR instance = SteamVR.instance;
        Debug.Log("OpenVR Headset HMD Type: " + instance.hmd_Type);

        GameObject go = new GameObject();
        go.name = "SteamVR_Behaviour";
        SteamVR_Behaviour steamVR_Behaviour = go.AddComponent<SteamVR_Behaviour>();
        steamVR_Behaviour.initializeSteamVROnAwake = true;
        steamVR_Behaviour.doNotDestroy = true;
        SteamVR_Render steamVR_Render = go.AddComponent<SteamVR_Render>();
        SteamVR_Action_Pose[] poses = SteamVR_Input.actionsPose;
        foreach (SteamVR_Action_Pose Pose in poses)
        {
            Pose.onDeviceConnectedChanged += UpdateDeviceList;
            UpdateDeviceList(Pose);
        }
    }

    private static void UpdateDeviceList(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource, bool deviceConnected)
    {
        UpdateDeviceList(fromAction);
        if (deviceConnected == false)
        {
            foreach (KeyValuePair<string, OpenVRDevice> deviceData in TypicalDevices)
            {
                if (deviceData.Value.SteamVR_Input_Sources == fromSource)
                {
                    string ID = deviceData.Key;
                    DestroyPhysicalTrackedDevice(ID);
                    return;
                }
            }
        }
    }

    private static void UpdateDeviceList(SteamVR_Action_Pose Pose)
    {
        foreach (SteamVR_Input_Sources inputSource in (SteamVR_Input_Sources[])System.Enum.GetValues(typeof(SteamVR_Input_Sources)))
        {
            if (inputSource != SteamVR_Input_Sources.Any)
            {
                SteamVR_Action_Pose_Source poseSource = Pose[inputSource];
                if (poseSource != null && poseSource.poseIsValid)
                {
                    UpdateDevice(poseSource);
                }
            }
        }
    }

    private static void UpdateDevice(SteamVR_Action_Pose_Source fromAction)
    {
        uint deviceIndex = fromAction.trackedDeviceIndex;
        Debug.Log("Device " + deviceIndex);
        string ID = GenerateID(deviceIndex);
        if (TypicalDevices.ContainsKey(ID) == false)
        {
            OpenVRDevice openVRDevice = new OpenVRDevice
            {
                SteamVR_Input_Sources = fromAction.inputSource,
                deviceName = ID,
                deviceIndex = deviceIndex,
                Pose = fromAction
            };
            CreatePhysicalTrackedDevice(openVRDevice, ID);
            TypicalDevices.Add(ID, openVRDevice);
            Debug.Log("Creating device: " + ID);
        }
    }
    public static void StopXRSDK()
    {
        if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
        {
            if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
        }
        StopXR();
    }
    public static void StopXR()
    {
        if (XRInstance != null && XRInstance.activeLoader != null)
        {
            XRInstance.StopSubsystems();
        }
        foreach (BasisOpenVRInput BasisOpenVRInput in TrackedOpenVRInputDevices)
        {
            if (BasisOpenVRInput != null)
            {
                Object.Destroy(BasisOpenVRInput.gameObject);
            }
        }
    }
    public static string GenerateID(uint device)
    {
        ETrackedPropertyError error = new ETrackedPropertyError();
        StringBuilder id = new System.Text.StringBuilder(64);
        OpenVR.System.GetStringTrackedDeviceProperty((uint)device, ETrackedDeviceProperty.Prop_RenderModelName_String, id, 64, ref error);
        string ID = device + "|" + id;
        return ID;
    }
    public static void CreatePhysicalTrackedDevice(OpenVRDevice device, string ID)
    {
        GameObject gameObject = new GameObject(ID);
        gameObject.transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
        BasisOpenVRInput BasisOpenVRInput = gameObject.AddComponent<BasisOpenVRInput>();
        BasisOpenVRInput.Initialize(device, ID);
        TrackedOpenVRInputDevices.Add(BasisOpenVRInput);
    }
    public static void DestroyPhysicalTrackedDevice(string ID)
    {
        DestroyInputDevice(ID);
        DestroyOpenVRInput(ID);
        Debug.Log("Destroying device: " + ID);
    }
    public static void DestroyInputDevice(string ID)
    {
        foreach (var device in TypicalDevices)
        {
            if (device.Key == ID)
            {
                TypicalDevices.Remove(ID);
                break;
            }
        }
    }
    public static void DestroyOpenVRInput(string ID)
    {
        foreach (var device in TrackedOpenVRInputDevices)
        {
            if (device.ID == ID)
            {
                // I think they already should be destroyed, IDK. Doing this seems to work fine.
                Object.Destroy(device.gameObject);
                TrackedOpenVRInputDevices.Remove(device);
                break;
            }
        }
    }
}