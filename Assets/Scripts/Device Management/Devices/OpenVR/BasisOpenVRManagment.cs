using System;
using System.Collections;
using System.Collections.Generic;
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

    public List<BasisOpenVRInput> TrackedOpenVRInputDevices = new List<BasisOpenVRInput>();
    public Dictionary<string, OpenVRDevice> TypicalDevices = new Dictionary<string, OpenVRDevice>();

    public void StartXRSDK()
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
    }

    private IEnumerator CheckState()
    {
        while (SteamVR.initializedState == SteamVR.InitializedStates.None)
        {
            yield return null;
        }

        if (SteamVR.initializedState == SteamVR.InitializedStates.InitializeFailure)
        {
            Debug.LogError("SteamVR failed to initialize");
            yield break;
        }

        SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);

        foreach (var poseAction in SteamVR_Input.actionsPose)
        {
            foreach (SteamVR_Input_Sources inputSource in Enum.GetValues(typeof(SteamVR_Input_Sources)))
            {
                poseAction[inputSource].onDeviceConnectedChanged += UpdateOnConnectOrDisconnect;
            }
        }
    }

    private void OnDeviceConnected(int deviceIndex, bool deviceConnected)
    {
        Debug.Log($"Device index {deviceIndex} is connected: {deviceConnected}");
        GenerateID((uint)deviceIndex, out string uniqueID, out string unUniqueID);

        var deviceClass = OpenVR.System.GetTrackedDeviceClass((uint)deviceIndex);
        if (deviceClass == ETrackedDeviceClass.TrackingReference)
        {
            return;
        }

        if (deviceConnected)
        {
            CreateDevice((uint)deviceIndex, deviceClass, uniqueID, unUniqueID);
        }
        else
        {
            DestroyPhysicalTrackedDevice(uniqueID);
        }
    }

    private void UpdateOnConnectOrDisconnect(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource, bool deviceConnected)
    {
        if (deviceConnected)
        {
            var deviceIndex = GetDeviceIndex(fromSource);
            foreach (var device in TrackedOpenVRInputDevices)
            {
                if (device.Device.deviceIndex == deviceIndex)
                {
                    device.TrackedRole = fromSource == SteamVR_Input_Sources.LeftHand ? BasisBoneTrackedRole.LeftHand : BasisBoneTrackedRole.RightHand;
                    device.inputSource = fromSource;
                }
            }
        }
    }

    private void CreateDevice(uint deviceIndex, ETrackedDeviceClass deviceClass, string uniqueID, string unUniqueID)
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

        foreach (var device in TrackedOpenVRInputDevices)
        {
            if (device != null)
            {
                GameObject.Destroy(device.gameObject);
            }
        }

        TrackedOpenVRInputDevices.Clear();
        SteamVR_Behaviour = null;
        SteamVR_Render = null;
    }

    public void GenerateID(uint device, out string uniqueID, out string notUnique)
    {
        var error = new ETrackedPropertyError();
        var id = new StringBuilder(64);
        OpenVR.System.GetStringTrackedDeviceProperty(device, ETrackedDeviceProperty.Prop_RenderModelName_String, id, 64, ref error);
        uniqueID = $"{device}|{id}";
        notUnique = id.ToString();
    }

    private uint GetDeviceIndex(SteamVR_Input_Sources source)
    {
        var poseAction = SteamVR_Actions.default_Pose;
        return poseAction[source].trackedDeviceIndex;
    }

    private void CreatePhysicalTrackedDevice(OpenVRDevice device, string uniqueID, string unUniqueID)
    {
        var gameObject = new GameObject(uniqueID)
        {
            transform = { parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform }
        };

        var basisOpenVRInput = gameObject.AddComponent<BasisOpenVRInput>();
        basisOpenVRInput.Initialize(device, uniqueID, unUniqueID);
        TrackedOpenVRInputDevices.Add(basisOpenVRInput);
        BasisDeviceManagement.Instance.AllInputDevices.Add(basisOpenVRInput);
    }

    public void DestroyPhysicalTrackedDevice(string id)
    {
        DestroyInputDevice(id);
        DestroyOpenVRInput(id);
        Debug.Log($"Destroying device: {id}");
    }

    private void DestroyInputDevice(string id)
    {
        TypicalDevices.Remove(id);
    }

    private void DestroyOpenVRInput(string id)
    {
        var devicesToRemove = new List<BasisOpenVRInput>();

        foreach (var device in TrackedOpenVRInputDevices)
        {
            if (device.UniqueID == id)
            {
                devicesToRemove.Add(device);
                GameObject.Destroy(device.gameObject);
            }
        }

        foreach (var device in devicesToRemove)
        {
            TrackedOpenVRInputDevices.Remove(device);
        }

        BasisDeviceManagement.Instance.AllInputDevices.RemoveAll(item => item == null);
    }
}