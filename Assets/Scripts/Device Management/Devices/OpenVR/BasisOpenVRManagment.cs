using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.VR;
[Serializable]
public class BasisOpenVRManagement
{
    public GameObject SteamVR_BehaviourGameobject;
    public SteamVR_Behaviour SteamVR_Behaviour;
    public SteamVR_Render SteamVR_Render;
    public SteamVR SteamVR;
    public Dictionary<string, OpenVRDevice> TypicalDevices = new Dictionary<string, OpenVRDevice>();
    public bool IsInUse = false;
    public void StartXRSDK()
    {
        if (!IsInUse)
        {
            Debug.Log("Starting SteamVR Instance...");
            SteamVR = SteamVR.instance;

            if (SteamVR_BehaviourGameobject == null)
            {
                SteamVR_BehaviourGameobject = new GameObject("SteamVR_Behaviour");
            }

            SteamVR_Behaviour = BasisHelpers.GetOrAddComponent<SteamVR_Behaviour>(SteamVR_BehaviourGameobject);
            SteamVR_Behaviour.Initialize();
            SteamVR_Render = BasisHelpers.GetOrAddComponent<SteamVR_Render>(SteamVR_BehaviourGameobject);

            SteamVR_Behaviour.initializeSteamVROnAwake = false;
            SteamVR_Behaviour.doNotDestroy = false;

            SteamVR_Render.StartCoroutine(CheckState());
            IsInUse = true;
        }
    }

    private IEnumerator CheckState()
    {
        while (SteamVR.initializedState == SteamVR.InitializedStates.None)
        {
            Debug.LogError("SteamVR initializedState failed");
            yield return null;
        }

        if (SteamVR.initializedState == SteamVR.InitializedStates.InitializeFailure)
        {
            Debug.LogError("SteamVR failed to initialize");
            yield break;
        }

        SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
    }

    private void OnDeviceConnected(int deviceIndex, bool deviceConnected)
    {
        uint DeviceID = (uint)deviceIndex;
        Debug.Log($"Device index {deviceIndex} is connected: {deviceConnected}");
        GenerateID(DeviceID, out ETrackedDeviceClass ETrackedDeviceClass, out string uniqueID, out string unUniqueID);
        if (ETrackedDeviceClass == ETrackedDeviceClass.TrackingReference)
        {
            //we dont want lighthouses
            return;
        }

        if (deviceConnected)
        {
            CreateTrackerDevice(DeviceID, ETrackedDeviceClass, uniqueID, unUniqueID);
        }
        else
        {
            DestroyPhysicalTrackedDevice(uniqueID);
        }
    }
    private void CreateTrackerDevice(uint deviceIndex, ETrackedDeviceClass deviceClass, string uniqueID, string unUniqueID)
    {
        if (!TypicalDevices.ContainsKey(uniqueID))
        {
            var openVRDevice = new OpenVRDevice
            {
                deviceClass = deviceClass,
                deviceIndex = deviceIndex,
                deviceName = uniqueID,
            };

            CreatePhysicalTrackedDevice(openVRDevice, uniqueID, unUniqueID);
            TypicalDevices[uniqueID] = openVRDevice;
            Debug.Log($"Creating device: {uniqueID}");
        }
        else
        {
            Debug.LogError("Device was already added");
        }
    }
    public void StopXRSDK()
    {
        if (SteamVR_BehaviourGameobject != null)
        {
            GameObject.Destroy(SteamVR_BehaviourGameobject);
        }
        List<string> Devices = TypicalDevices.Keys.ToList();
        foreach (string device in Devices)
        {
            DestroyPhysicalTrackedDevice(device);
        }
        SteamVR_Behaviour = null;
        SteamVR_Render = null;
        IsInUse = false;
    }
    public void GenerateID(uint device, out ETrackedDeviceClass ETrackedDeviceClass, out string uniqueID, out string notUnique)
    {
        var error = new ETrackedPropertyError();
        var id = new StringBuilder(64);
        OpenVR.System.GetStringTrackedDeviceProperty(device, ETrackedDeviceProperty.Prop_RenderModelName_String, id, 64, ref error);
       ETrackedDeviceClass = OpenVR.System.GetTrackedDeviceClass(device);
        uniqueID = $"{device}|{id}";
        notUnique = id.ToString();
    }
    private void CreatePhysicalTrackedDevice(OpenVRDevice device, string uniqueID, string unUniqueID)
    {
        var gameObject = new GameObject(uniqueID)
        {
            transform = { parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform }
        };

        switch (device.deviceClass)
        {
            case ETrackedDeviceClass.HMD:
                CreateHMD(gameObject, device, uniqueID, unUniqueID);
                break;
            case ETrackedDeviceClass.Controller:
                CreateController(gameObject, device, uniqueID, unUniqueID);
                break;

            case ETrackedDeviceClass.TrackingReference:
                Debug.Log("was Tracked Reference Returning (lighthouse)");
                break;
            case ETrackedDeviceClass.GenericTracker:
            case ETrackedDeviceClass.DisplayRedirect:
            default:
                CreateTracker(gameObject, device, uniqueID, unUniqueID,false, BasisBoneTrackedRole.CenterEye);
                break;
        }
    }

    private void CreateHMD(GameObject gameObject, OpenVRDevice device, string uniqueID, string unUniqueID)
    {
        // Implement HMD specific initialization if needed
        Debug.Log($"Creating HMD: {uniqueID}");
        // For now, we'll treat it as a generic tracker
        CreateTracker(gameObject, device, uniqueID, unUniqueID, true, BasisBoneTrackedRole.CenterEye);
    }
    public void CreateController(GameObject GameObject, OpenVRDevice device, string uniqueID, string unUniqueID)
    {
        BasisOpenVRInputController BasisOpenVRInputController = GameObject.AddComponent<BasisOpenVRInputController>();
        bool FoundRole = TryAssignRole(device.deviceClass, device.deviceIndex, out BasisBoneTrackedRole Role, out SteamVR_Input_Sources Source);
        BasisOpenVRInputController.Initialize(device, uniqueID, unUniqueID, "BasisOpenVRManagement",FoundRole, Role, Source);
        BasisDeviceManagement.Instance.TryAdd(BasisOpenVRInputController);
    }
    public void CreateTracker(GameObject GameObject, OpenVRDevice device, string uniqueID, string unUniqueID,bool AutoAssignRole, BasisBoneTrackedRole role)
    {
        BasisOpenVRInput basisOpenVRInput = GameObject.AddComponent<BasisOpenVRInput>();
        basisOpenVRInput.Initialize(device, uniqueID, unUniqueID, "BasisOpenVRManagement", AutoAssignRole, role);
        BasisDeviceManagement.Instance.TryAdd(basisOpenVRInput);
    }
    public bool TryAssignRole(ETrackedDeviceClass deviceClass,uint deviceIndex, out BasisBoneTrackedRole Role,out SteamVR_Input_Sources Source )
    {
        Source = SteamVR_Input_Sources.Any;
        Role = BasisBoneTrackedRole.CenterEye;

        if (deviceClass == ETrackedDeviceClass.HMD)
        {
            Role = BasisBoneTrackedRole.CenterEye;
            Source = SteamVR_Input_Sources.Head;
            return true;
        }
        else
        {
            if (deviceClass == ETrackedDeviceClass.Controller)
            {
                bool isLeftHand = SteamVR.instance.hmd.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand) == deviceIndex;
                if (isLeftHand)
                {
                    Role = BasisBoneTrackedRole.LeftHand;
                    Source = SteamVR_Input_Sources.LeftHand;
                    return true;
                }
                bool isRightHand = SteamVR.instance.hmd.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand) == deviceIndex;
                if (isRightHand)
                {
                    Role = BasisBoneTrackedRole.RightHand;
                    Source = SteamVR_Input_Sources.RightHand;
                    return true;
                }
            }
        }
        return false;
    }
    public void DestroyPhysicalTrackedDevice(string id)
    {
        TypicalDevices.Remove(id);
        BasisDeviceManagement.Instance.RemoveDevicesFrom("BasisOpenVRManagement", id);
    }
}