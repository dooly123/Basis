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
                BasisBoneTrackedRole role = GetBasisBoneTrackedRole(deviceType, (uint)deviceIndex);

                OpenVRDevice openVRDevice = new OpenVRDevice
                {
                    deviceIndex = deviceIndex,
                    deviceName = ID,
                    deviceType = role
                };
                CreatePhysicalTrackedDevice(openVRDevice, ID);
                TypicalDevices.Add(ID, openVRDevice);

                // If a controller is connecting, check both controllers and make sure to update the roles so they are not the
                // same or swapped. Sometimes when OpenVR has a new controller connecting, it will swap left and right roles.
                UpdateControllerRoles(deviceType, role, ID);
                
                Debug.Log("Creating device: " + ID);
            }
        }
        else
        {
            DestroyPhysicalTrackedDevice(ID);
        }
    }

    private static BasisBoneTrackedRole GetBasisBoneTrackedRole(ETrackedDeviceClass deviceType, uint deviceIndex)
    {
        BasisBoneTrackedRole role = BasisBoneTrackedRole.Hips;
        switch (deviceType)
        {
            case ETrackedDeviceClass.Controller:
                bool isLeftHand = OpenVR.System.GetControllerRoleForTrackedDeviceIndex(deviceIndex) == ETrackedControllerRole.LeftHand;
                role = isLeftHand ? BasisBoneTrackedRole.LeftHand : BasisBoneTrackedRole.RightHand;
                break;
            case ETrackedDeviceClass.GenericTracker:
                //role = BasisBoneTrackedRole.None;
                break;
            case ETrackedDeviceClass.HMD:
                role = BasisBoneTrackedRole.CenterEye;
                break;
            case ETrackedDeviceClass.Invalid:
                //role = BasisBoneTrackedRole.None;
                break;
            case ETrackedDeviceClass.TrackingReference:
                //role = BasisBoneTrackedRole.None;
                break;
        }
        return role;
    }

    private static void UpdateControllerRoles(ETrackedDeviceClass deviceType, BasisBoneTrackedRole role, string ID)
    {
        if (deviceType == ETrackedDeviceClass.Controller)
        {
            foreach (var device in TrackedOpenVRInputDevices)
            {
                if (device.ID == ID) continue;
                if (device.Device.deviceType == BasisBoneTrackedRole.LeftHand || device.Device.deviceType == BasisBoneTrackedRole.RightHand)
                {
                    BasisBoneTrackedRole newRole = role == BasisBoneTrackedRole.LeftHand ? BasisBoneTrackedRole.RightHand : BasisBoneTrackedRole.LeftHand;
                    device.Device.deviceType = newRole;
                    device.TrackedRole = newRole;
                    break;
                }
            }
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