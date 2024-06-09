using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Valve.VR;
[System.Serializable]
public class BasisOpenVRManagement
{
    public GameObject SteamVR_BehaviourGameobject;
    public SteamVR_Behaviour SteamVR_Behaviour;
    public SteamVR_Render SteamVR_Render;
    public SteamVR SteamVR;
    public List<OpenVRDevice> inputDevices = new List<OpenVRDevice>();
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
        SteamVR = SteamVR.instance;
        Debug.Log("OpenVR Headset HMD Type: " + SteamVR.hmd_Type);
        if (SteamVR_BehaviourGameobject == null)
        {
            SteamVR_BehaviourGameobject = new GameObject("SteamVR");
        }
        SteamVR_BehaviourGameobject.name = "SteamVR_Behaviour";
        SteamVR_Behaviour = BasisHelpers.GetOrAddComponent<SteamVR_Behaviour>(SteamVR_BehaviourGameobject);
        SteamVR_Render = BasisHelpers.GetOrAddComponent<SteamVR_Render>(SteamVR_BehaviourGameobject);
        SteamVR_Behaviour.initializeSteamVROnAwake = false;
        SteamVR_Behaviour.doNotDestroy = false;
        SteamVR.Initialize();
        SteamVR_Render.StartCoroutine(CheckState());
    }
    public IEnumerator CheckState()
    {
        if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess || SteamVR.initializedState != SteamVR.InitializedStates.InitializeFailure || SteamVR.initializedState != SteamVR.InitializedStates.None)
        {
            Debug.Log("Waiting");
            yield return new WaitForEndOfFrame();
        }
        foreach (SteamVR_Action_Pose poseAction in SteamVR_Input.actionsPose)
        {
            foreach (SteamVR_Input_Sources inputSource in (SteamVR_Input_Sources[])System.Enum.GetValues(typeof(SteamVR_Input_Sources)))
            {
                if (inputSource != SteamVR_Input_Sources.Any)
                {
                    // poseAction[inputSource].onUpdate += SteamVR_Behaviour_Pose_OnUpdate;
                    poseAction[inputSource].onDeviceConnectedChanged += UpdateOnConnectORDisconnect;
                    UpdateOnConnectORDisconnect(poseAction, poseAction[inputSource].inputSource, poseAction[inputSource].deviceIsConnected);
                    //  poseAction[inputSource].onTrackingChanged += OnTrackingChanged;
                    // poseAction[inputSource].onChange += SteamVR_Behaviour_Pose_OnChange;
                }
            }
        }
    }
    private void UpdateOnConnectORDisconnect(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource, bool deviceConnected)
    {
        Debug.Log("Action " + fromAction.fullPath + " Input Source " + fromSource + " IS Connected " + deviceConnected);
        if (deviceConnected)
        {
            CreateDevice(fromAction, fromSource);
        }
        else
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
    private void CreateDevice(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
    {
        string ID = GenerateID((uint)fromSource);
        if (TypicalDevices.ContainsKey(ID) == false)
        {
            OpenVRDevice openVRDevice = new OpenVRDevice
            {
                SteamVR_Input_Sources = fromSource,
                deviceName = ID,
                SteamVR_Action_Pose = fromAction
            };
            CreatePhysicalTrackedDevice(openVRDevice, ID);
            if (TypicalDevices.TryAdd(ID, openVRDevice))
            {
                Debug.Log("Creating device: " + ID);
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
    public string GenerateID(uint device)
    {
        ETrackedPropertyError error = new ETrackedPropertyError();
        StringBuilder id = new StringBuilder(64);
        OpenVR.System.GetStringTrackedDeviceProperty(device, ETrackedDeviceProperty.Prop_RenderModelName_String, id, 64, ref error);
        string ID = device + "|" + id;
        return ID;
    }
    public void CreatePhysicalTrackedDevice(OpenVRDevice device, string ID)
    {
        GameObject gameObject = new GameObject(ID);
        gameObject.transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
        BasisOpenVRInput BasisOpenVRInput = gameObject.AddComponent<BasisOpenVRInput>();
        BasisOpenVRInput.Initialize(device, ID);
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