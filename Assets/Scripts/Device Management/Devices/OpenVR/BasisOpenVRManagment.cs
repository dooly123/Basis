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
    public static string SteamVRBehaviour = "SteamVR_Behaviour";
    public void StartXRSDK()
    {
        if (IsInUse)
        {
            return;
        }

        Debug.Log("Starting SteamVR Instance...");
        SteamVR = SteamVR.instance;

        if (SteamVR_BehaviourGameobject == null)
        {
            SteamVR_BehaviourGameobject = new GameObject(SteamVRBehaviour);
        }

        SteamVR_Behaviour = BasisHelpers.GetOrAddComponent<SteamVR_Behaviour>(SteamVR_BehaviourGameobject);
        SteamVR_Behaviour.Initialize();
        SteamVR_Render = BasisHelpers.GetOrAddComponent<SteamVR_Render>(SteamVR_BehaviourGameobject);

        SteamVR_Behaviour.initializeSteamVROnAwake = false;
        SteamVR_Behaviour.doNotDestroy = false;

        SteamVR_Render.StartCoroutine(CheckState());
        IsInUse = true;
    }

    public void StopXRSDK()
    {
        if (SteamVR_BehaviourGameobject != null)
        {
            GameObject.Destroy(SteamVR_BehaviourGameobject);
        }

        foreach (var device in TypicalDevices.Keys.ToList())
        {
            DestroyPhysicalTrackedDevice(device);
        }

        SteamVR_Behaviour = null;
        SteamVR_Render = null;
        IsInUse = false;
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
        SteamVR_Events.System(EVREventType.VREvent_TrackedDeviceRoleChanged).Listen(OnTrackedDeviceRoleChanged); //gets called when a role is changed after the device is already connected.
    }
    private void OnDeviceConnected(int arg0, bool arg1)
    {
        OnDeviceConnected((uint)arg0, arg1);
    }
    private void OnTrackedDeviceRoleChanged(VREvent_t arg0)
    {
        OnDeviceConnected(arg0.trackedDeviceIndex, true);
    }

    private void OnDeviceConnected(uint deviceIndex, bool deviceConnected)
    {
        Debug.Log($"Device index {deviceIndex} is connected: {deviceConnected}");
        var error = new ETrackedPropertyError();
        var id = new StringBuilder(64);
        OpenVR.System.GetStringTrackedDeviceProperty(deviceIndex, ETrackedDeviceProperty.Prop_RenderModelName_String, id, 64, ref error);
        ETrackedDeviceClass deviceClass = OpenVR.System.GetTrackedDeviceClass(deviceIndex);
        string uniqueID = $"{deviceIndex}|{id}";
        string notUnique = id.ToString();

        if (deviceConnected)
        {
            CreateTrackerDevice(deviceIndex, deviceClass, uniqueID, notUnique);
        }
        else
        {
            DestroyPhysicalTrackedDevice(uniqueID);
        }
    }
    private void CreateTrackerDevice(uint deviceIndex, ETrackedDeviceClass deviceClass, string uniqueID, string unUniqueID)
    {
        var openVRDevice = new OpenVRDevice
        {
            deviceClass = deviceClass,
            deviceIndex = deviceIndex,
            deviceName = uniqueID,
        };
        var gameObject = new GameObject(uniqueID)
        {
            transform = { parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform }
        };

        switch (openVRDevice.deviceClass)
        {
            case ETrackedDeviceClass.HMD:
                CreateHMD(gameObject, openVRDevice, uniqueID, unUniqueID);
                break;
            case ETrackedDeviceClass.Controller:
                CreateController(gameObject, openVRDevice, uniqueID, unUniqueID);
                break;
            case ETrackedDeviceClass.TrackingReference:
                Debug.Log("Was Tracked Reference Returning (lighthouse)");
                break;
            default:
                CreateTracker(gameObject, openVRDevice, uniqueID, unUniqueID, false, BasisBoneTrackedRole.CenterEye);
                break;
        }
    }
    /// <summary>
    ///         TypicalDevices[uniqueID] = openVRDevice;
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="device"></param>
    /// <param name="uniqueID"></param>
    /// <param name="unUniqueID"></param>

    private void CreateHMD(GameObject gameObject, OpenVRDevice device, string uniqueID, string unUniqueID)
    {
        if (TypicalDevices.ContainsKey(uniqueID) == false)
        {
            var basisOpenVRInputSpatial = gameObject.AddComponent<BasisOpenVRInputSpatial>();
            basisOpenVRInputSpatial.ClassName = nameof(BasisOpenVRInputSpatial);
            bool foundRole = TryAssignRole(device.deviceClass, device.deviceIndex, out BasisBoneTrackedRole role, out SteamVR_Input_Sources source);
            basisOpenVRInputSpatial.Initialize(UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.Center, uniqueID, unUniqueID, nameof(BasisOpenVRManagement), foundRole, role, source);
            BasisDeviceManagement.Instance.TryAdd(basisOpenVRInputSpatial);
            TypicalDevices.TryAdd(uniqueID, device);
        }
        else
        {
            foreach (BasisInput Input in BasisDeviceManagement.Instance.AllInputDevices)
            {
                if(Input.UniqueDeviceIdentifier == uniqueID && Input.SubSystemIdentifier == uniqueID)
                {
                    if(Input.ClassName == nameof(BasisOpenVRInputSpatial))
                    {
                        BasisOpenVRInputSpatial Spatial = (BasisOpenVRInputSpatial)Input;
                        bool foundRole = TryAssignRole(device.deviceClass, device.deviceIndex, out BasisBoneTrackedRole role, out SteamVR_Input_Sources source);
                        Spatial.Initialize(UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.Center, uniqueID, unUniqueID,nameof(BasisOpenVRManagement), foundRole, role, source);
                    }
                    else
                    {
                        DestroyPhysicalTrackedDevice(uniqueID);
                        OnDeviceConnected(device.deviceIndex, true);
                    }
                }
            }
        }
    }
    public void CreateController(GameObject gameObject, OpenVRDevice device, string uniqueID, string unUniqueID)
    {
        if (TypicalDevices.ContainsKey(uniqueID) == false)
        {
            var basisOpenVRInputController = gameObject.AddComponent<BasisOpenVRInputController>();
            basisOpenVRInputController.ClassName = nameof(BasisOpenVRInputController);
            bool foundRole = TryAssignRole(device.deviceClass, device.deviceIndex, out BasisBoneTrackedRole role, out SteamVR_Input_Sources source);
            basisOpenVRInputController.Initialize(device, uniqueID, unUniqueID, nameof(BasisOpenVRManagement), foundRole, role, source);
            BasisDeviceManagement.Instance.TryAdd(basisOpenVRInputController);
            TypicalDevices.TryAdd(uniqueID, device);
        }
        else
        {
            foreach (BasisInput Input in BasisDeviceManagement.Instance.AllInputDevices)
            {
                if (Input.UniqueDeviceIdentifier == uniqueID && Input.SubSystemIdentifier == uniqueID)
                {
                    if (Input.ClassName == nameof(BasisOpenVRInputController))
                    {
                        BasisOpenVRInputController BasisOpenVRInputController = (BasisOpenVRInputController)Input;
                        bool foundRole = TryAssignRole(device.deviceClass, device.deviceIndex, out BasisBoneTrackedRole role, out SteamVR_Input_Sources source);
                        BasisOpenVRInputController.Initialize(device, uniqueID, unUniqueID, nameof(BasisOpenVRManagement), foundRole, role, source);
                    }
                    else
                    {
                        DestroyPhysicalTrackedDevice(uniqueID);
                        OnDeviceConnected(device.deviceIndex, true);
                    }
                }
            }
        }
    }

    public void CreateTracker(GameObject gameObject, OpenVRDevice device, string uniqueID, string unUniqueID, bool autoAssignRole, BasisBoneTrackedRole role)
    {
        if (TypicalDevices.ContainsKey(uniqueID) == false)
        {
            var basisOpenVRInput = gameObject.AddComponent<BasisOpenVRInput>();
            basisOpenVRInput.ClassName = nameof(BasisOpenVRInput);
            basisOpenVRInput.Initialize(device, uniqueID, unUniqueID, nameof(BasisOpenVRManagement), autoAssignRole, role);
            BasisDeviceManagement.Instance.TryAdd(basisOpenVRInput);
            TypicalDevices.TryAdd(uniqueID, device);
        }
        else
        {
            foreach (BasisInput Input in BasisDeviceManagement.Instance.AllInputDevices)
            {
                if (Input.UniqueDeviceIdentifier == uniqueID && Input.SubSystemIdentifier == uniqueID)
                {
                    if (Input.ClassName == nameof(BasisOpenVRInput))
                    {
                        BasisOpenVRInput basisOpenVRInput = (BasisOpenVRInput)Input;
                        basisOpenVRInput.Initialize(device, uniqueID, unUniqueID, nameof(BasisOpenVRManagement), autoAssignRole, role);
                    }
                    else
                    {
                        DestroyPhysicalTrackedDevice(uniqueID);
                        OnDeviceConnected(device.deviceIndex, true);
                    }
                }
            }
        }
    }
    public bool TryAssignRole(ETrackedDeviceClass deviceClass, uint deviceIndex, out BasisBoneTrackedRole role, out SteamVR_Input_Sources source)
    {
        source = SteamVR_Input_Sources.Any;
        role = BasisBoneTrackedRole.CenterEye;

        if (deviceClass == ETrackedDeviceClass.HMD)
        {
            role = BasisBoneTrackedRole.CenterEye;
            source = SteamVR_Input_Sources.Head;
            return true;
        }

        if (deviceClass == ETrackedDeviceClass.Controller)
        {
            var controllerRole = SteamVR.instance.hmd.GetControllerRoleForTrackedDeviceIndex(deviceIndex);
            if (controllerRole == ETrackedControllerRole.LeftHand)
            {
                role = BasisBoneTrackedRole.LeftHand;
                source = SteamVR_Input_Sources.LeftHand;
                return true;
            }

            if (controllerRole == ETrackedControllerRole.RightHand)
            {
                role = BasisBoneTrackedRole.RightHand;
                source = SteamVR_Input_Sources.RightHand;
                return true;
            }

            role = BasisBoneTrackedRole.LeftHand;
            source = SteamVR_Input_Sources.LeftHand;
            Debug.LogError("Device unknown");
        }

        return false;
    }
    public void DestroyPhysicalTrackedDevice(string id)
    {
        TypicalDevices.Remove(id);
        BasisDeviceManagement.Instance.RemoveDevicesFrom("BasisOpenVRManagement", id);
    }
}