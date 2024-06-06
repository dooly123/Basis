using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
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
        
        SteamVR_Events.DeviceConnected.Listen(UpdateDeviceList);
    }
    private static void StopXRSDK()
    {
        if (XRInstance != null && XRInstance.activeLoader != null)
        {
            XRInstance.StopSubsystems();
        }
    }
    private static void UpdateDeviceList(int deviceIndex, bool connected)
    {
        ETrackedDeviceClass deviceType = OpenVR.System.GetTrackedDeviceClass((uint)deviceIndex);
        Debug.Log("Device " + deviceIndex + " connected: " + connected);
        Debug.Log("Device Type: " + deviceType);

        if (deviceType == ETrackedDeviceClass.TrackingReference)
        {
            return;
        }
        
        string ID = GenerateID(deviceIndex);
        if (connected)
        {
            if (TypicalDevices.ContainsKey(ID) == false)
            {
                OpenVRDevice openVRDevice = new OpenVRDevice
                {
                    deviceIndex = deviceIndex,
                    deviceName = ID,
                    deviceType = deviceType
                };
                CreatePhysicalTrackedDevice(openVRDevice, ID);
                TypicalDevices.Add(ID, openVRDevice);
                
                Debug.Log("Creating device: " + ID);
            }
        }
        else
        {
            DestroyPhysicalTrackedDevice(ID);
        }
    }
    public static string GenerateID(int device)
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
   }
   public static void DestroyInputDevice(string ID)
   {
       TypicalDevices.Remove(ID);
   }
   public static void DestroyOpenVRInput(string ID)
   {
       TrackedOpenVRInputDevices.RemoveAll(x => x.ID == ID);
   }
}