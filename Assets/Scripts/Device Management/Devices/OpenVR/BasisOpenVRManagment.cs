using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using UnityEngine;
using Valve.VR;
[System.Serializable]
public class BasisOpenVRManagement
{
    public GameObject SteamVR_BehaviourGameobject;
    public SteamVR_Behaviour SteamVR_Behaviour;
    public SteamVR_Render SteamVR_Render;
    public SteamVR SteamVR;
    /// <summary>
    /// generated at runtime
    /// </summary>
    public List<BasisOpenVRInput> TrackedOpenVRInputDevices = new List<BasisOpenVRInput>();
    /// <summary>
    /// keeps track of generated IDs and match InputDevice
    /// </summary>
    public Dictionary<string, OpenVRDevice> TypicalDevices = new Dictionary<string, OpenVRDevice>();
    public void StartXRSDK()
    {
        Debug.Log("Starting SteamVR Instance...");
        SteamVR = SteamVR.instance;
        if (SteamVR_BehaviourGameobject == null)
        {
            SteamVR_BehaviourGameobject = new GameObject("SteamVR");
        }
        SteamVR_BehaviourGameobject.name = "SteamVR_Behaviour";
        SteamVR_Behaviour = BasisHelpers.GetOrAddComponent<SteamVR_Behaviour>(SteamVR_BehaviourGameobject);
        SteamVR_Behaviour.Initialize();
        SteamVR_Render = BasisHelpers.GetOrAddComponent<SteamVR_Render>(SteamVR_BehaviourGameobject);
        SteamVR_Behaviour.initializeSteamVROnAwake = false;
        SteamVR_Behaviour.doNotDestroy = false;
        
        SteamVR_Render.StartCoroutine(CheckState());
    }
    
    public IEnumerator CheckState()
    {
        // Loop until SteamVR is either successfully initialized or failed to initialize
        while (SteamVR.initializedState == SteamVR.InitializedStates.None)
        {
            yield return null; // Wait for the next frame
        }

        // Check if initialization failed
        if (SteamVR.initializedState == SteamVR.InitializedStates.InitializeFailure)
        {
            Debug.LogError("SteamVR failed to initialize");
            yield break; // Exit the coroutine
        }
        
        // Now proceed to bind events
        SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
        
        foreach (SteamVR_Action_Pose poseAction in SteamVR_Input.actionsPose)
        {
            foreach (SteamVR_Input_Sources inputSource in (SteamVR_Input_Sources[])System.Enum.GetValues(typeof(SteamVR_Input_Sources)))
            {
                poseAction[inputSource].onDeviceConnectedChanged += UpdateOnConnectORDisconnect;
            }
        }
    }

    private void OnDeviceConnected(int deviceIndex, bool deviceConnected)
    {
        Debug.Log("Device index " + deviceIndex + " IS Connected " + deviceConnected);
        GenerateID((uint)deviceIndex, out string UniqueID,out string UnUniqueID);

        ETrackedDeviceClass deviceClass = OpenVR.System.GetTrackedDeviceClass((uint)deviceIndex);
        if (deviceClass == ETrackedDeviceClass.TrackingReference)
        {
            return;
        }
        
        if (deviceConnected)
        {
            CreateDevice((uint)deviceIndex, deviceClass, UniqueID, UnUniqueID);
        }
        else
        {
            foreach (KeyValuePair<string, OpenVRDevice> deviceData in TypicalDevices)
            {
                if (deviceData.Value.deviceName == UniqueID)
                {
                    DestroyPhysicalTrackedDevice(UniqueID);
                    return;
                }
            }
        }
    }

    private void UpdateOnConnectORDisconnect(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource, bool deviceConnected)
    {
        if (deviceConnected)
        {
            foreach (var device in TrackedOpenVRInputDevices)
            {
                if (device.TrackedRole == BasisBoneTrackedRole.LeftHand)
                {
                    if (device.Device.deviceIndex == GetDeviceIndex(fromSource))
                    {
                        device.TrackedRole = BasisBoneTrackedRole.LeftHand;
                        device.inputSource = fromSource;
                    }
                }
                if (device.TrackedRole == BasisBoneTrackedRole.RightHand)
                {
                    if (device.Device.deviceIndex == GetDeviceIndex(fromSource))
                    {
                        device.TrackedRole = BasisBoneTrackedRole.RightHand;
                        device.inputSource = fromSource;
                    }
                }
            }
        }
    }
    private void CreateDevice(uint deviceIndex, ETrackedDeviceClass deviceClass, string UniqueID, string UnUniqueID)
    {
        if (TypicalDevices.ContainsKey(UniqueID) == false)
        {
            OpenVRDevice openVRDevice = new OpenVRDevice
            {
                deviceClass = deviceClass,
                deviceIndex = deviceIndex,
                deviceName = UniqueID,
            };
            CreatePhysicalTrackedDevice(openVRDevice, UniqueID, UnUniqueID);
            if (TypicalDevices.TryAdd(UniqueID, openVRDevice))
            {
                Debug.Log("Creating device: " + UniqueID);
            }
            else
            {
                Debug.LogError("Device was already added");
            }
        }
    }
    public void StopXRSDK()
    {
        if (SteamVR_BehaviourGameobject != null)
        {
            GameObject.Destroy(SteamVR_BehaviourGameobject);
        }
        foreach (BasisOpenVRInput BasisOpenVRInput in TrackedOpenVRInputDevices)
        {
            if (BasisOpenVRInput != null)
            {
                Object.Destroy(BasisOpenVRInput.gameObject);
            }
        }
        Object.Destroy(SteamVR_Behaviour);
    }
    public void GenerateID(uint device,out string UniqueID,out string NotUnique)
    {
        ETrackedPropertyError error = new ETrackedPropertyError();
        StringBuilder id = new StringBuilder(64);
        OpenVR.System.GetStringTrackedDeviceProperty(device, ETrackedDeviceProperty.Prop_RenderModelName_String, id, 64, ref error);
        UniqueID = device + "|" + id;
        NotUnique = id.ToString();
    }
    uint GetDeviceIndex(SteamVR_Input_Sources source)
    {
        // Get the device index from the input source
        SteamVR_Action_Pose poseAction = SteamVR_Actions.default_Pose;

        // Get the input device index associated with the action and input source
        uint inputDevice = poseAction[source].trackedDeviceIndex;
        return inputDevice;

    }
    public void CreatePhysicalTrackedDevice(OpenVRDevice device, string UniqueID, string UnUniqueID)
    {
        GameObject gameObject = new GameObject(UniqueID);
        gameObject.transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
        BasisOpenVRInput BasisOpenVRInput = gameObject.AddComponent<BasisOpenVRInput>();
        BasisOpenVRInput.Initialize(device, UniqueID, UnUniqueID);
        TrackedOpenVRInputDevices.Add(BasisOpenVRInput);
    }
    public void DestroyPhysicalTrackedDevice(string ID)
    {
        DestroyInputDevice(ID);
        DestroyOpenVRInput(ID);
        Debug.Log("Destroying device: " + ID);
    }
    public void DestroyInputDevice(string ID)
    {
        foreach (KeyValuePair<string, OpenVRDevice> device in TypicalDevices)
        {
            if (device.Key == ID)
            {
                TypicalDevices.Remove(ID);
                break;
            }
        }
    }
    public void DestroyOpenVRInput(string ID)
    {
        for (int Index = 0; Index < TrackedOpenVRInputDevices.Count; Index++)
        {
            BasisOpenVRInput device = TrackedOpenVRInputDevices[Index];
            if (device.UniqueID == ID)
            {
                // I think they already should be destroyed, IDK. Doing this seems to work fine.
                Object.Destroy(device.gameObject);
                TrackedOpenVRInputDevices.Remove(device);
                break;
            }
        }
    }
}