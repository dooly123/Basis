using UnityEngine;
using Valve.VR;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputTrackingAggregator;
[DefaultExecutionOrder(15101)]
public class BasisOpenVRInput : MonoBehaviour
{
    public OpenVRDevice Device;
    public BasisLocalBoneDriver Driver;
    public BasisBoneTrackedRole Type;
    public BasisBoneControl Control;
    public string ID;
    public SteamVR_ActionSet actionSet;
    public SteamVR_Action_Pose poseAction;
    public Vector3 LocalRawPosition;
    public Quaternion LocalRawRotation;
    public void OnDisable()
    {
        DisableTracking();
    }
    public void Initialize(OpenVRDevice device, string iD)
    {
        // Get a reference to the pose action by its name
        actionSet = SteamVR_Input.GetActionSet("default");
        
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        ID = iD;
        Device = device;
        if (Device.deviceType == ETrackedDeviceClass.HMD)
        {
            Type = BasisBoneTrackedRole.CenterEye;
            actionSet.Activate(SteamVR_Input_Sources.Head);
        }
        else
        {
            // Get handedness of controller
            if (Device.deviceType == ETrackedDeviceClass.Controller)
            {
                bool isLeftHand = false;
                isLeftHand = OpenVR.System.GetControllerRoleForTrackedDeviceIndex((uint)device.deviceIndex) == ETrackedControllerRole.LeftHand;
                
                if (isLeftHand)
                {
                    Type = BasisBoneTrackedRole.LeftHand;
                }
                else
                {
                    Type = BasisBoneTrackedRole.RightHand;
                }
                
                Debug.Log(isLeftHand);
            }
        }
        ActivateTracking();
    }
    public void ActivateTracking()
    {
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        if (Driver.FindBone(out Control, Type))
        {

        }
        Driver.OnSimulate += SetPosRot;
        SetRealTrackers(BasisBoneControl.BasisHasTracked.HasVRTracker);
    }
    public void DisableTracking()
    {
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        Driver.OnSimulate -= SetPosRot;
        SetRealTrackers(BasisBoneControl.BasisHasTracked.HasNoTracker);
    }
    
    
    public Vector2 primary2DAxisL;
    public Vector2 primary2DAxisR;
    public Vector2 secondary2DAxis;
    public bool gripButton;
    public bool menuButton;
    public bool primaryButton;
    public bool secondaryButton;
    public float Trigger;
    public bool secondary2DAxisClick;
    public bool primary2DAxisClick;
    

    public void SetPosRot()
    {
        
        TrackedDevicePose_t devicePose = new TrackedDevicePose_t();
        TrackedDevicePose_t deviceGamePose = new TrackedDevicePose_t();
        var result = SteamVR.instance.compositor.GetLastPoseForTrackedDeviceIndex((uint)Device.deviceIndex, ref devicePose, ref deviceGamePose);
        
        if (result != EVRCompositorError.None)
        {
            Debug.LogError("Error getting device pose: " + result);
            return;
        }
        
        var deviceTransform = new SteamVR_Utils.RigidTransform(deviceGamePose.mDeviceToAbsoluteTracking);
        LocalRawPosition = deviceTransform.pos;
        LocalRawRotation = deviceTransform.rot;
        
        if (Control.HasTrackerPositionDriver != BasisBoneControl.BasisHasTracked.HasNoTracker)
        {
            if (LocalRawPosition != Vector3.zero)
            {
                Control.LocalRawPosition = LocalRawPosition;
            }
        }

        if (Control.HasTrackerPositionDriver != BasisBoneControl.BasisHasTracked.HasNoTracker)
        {
            if (LocalRawRotation != Quaternion.identity)
            {
                Control.LocalRawRotation = LocalRawRotation;
            }
        }
        
        
        primary2DAxisL = SteamVR_Actions.default_Move.GetAxis(SteamVR_Input_Sources.Any);
        primary2DAxisR = SteamVR_Actions.default_Rotate.GetAxis(SteamVR_Input_Sources.Any);
        
        if (Type == BasisBoneTrackedRole.LeftHand)
        {
            BasisLocalPlayer.Instance.Move.MovementVector = primary2DAxisL;
            if (primaryButton)
            {
                BasisLocalPlayer.Instance.Move.HandleJump();
            }
            if (secondaryButton)
            {
                if (BasisHamburgerMenu.Instance == null)
                {
                    if (BasisHamburgerMenu.IsLoading == false)
                    {
                        BasisHamburgerMenu.OpenMenu();
                    }
                }
                else
                {
                    BasisHamburgerMenu.Instance.CloseThisMenu();
                }
            }
        }
        else
        {
            if (Type == BasisBoneTrackedRole.RightHand)
            {
                BasisLocalPlayer.Instance.Move.Rotation = primary2DAxisR;
            }
        }
        
        this.transform.SetLocalPositionAndRotation(LocalRawPosition, LocalRawRotation);
    }
    public void SetRealTrackers(BasisBoneControl.BasisHasTracked value)
    {
        if (Driver.FindBone(out BasisBoneControl Control, Type))
        {
            Control.HasTrackerPositionDriver = value;
            Control.HasTrackerRotationDriver = value;
        }
    }
}