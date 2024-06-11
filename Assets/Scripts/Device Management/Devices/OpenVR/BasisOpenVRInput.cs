using UnityEngine;
using Valve.VR;
using VIVE.OpenXR.CompositionLayer;

[DefaultExecutionOrder(15101)]
public class BasisOpenVRInput : BasisInput
{
    public OpenVRDevice Device;
    public SteamVR_Input_Sources inputSource;
    public TrackedDevicePose_t devicePose = new TrackedDevicePose_t();
    public TrackedDevicePose_t deviceGamePose = new TrackedDevicePose_t();
    public SteamVR_Utils.RigidTransform deviceTransform;
    public EVRCompositorError result;

    public void Initialize(OpenVRDevice device, string iD)
    {
        Device = device;
        TryAssignRole(Device.deviceClass);
        base.Initialize(iD);
    }

    public void TryAssignRole(ETrackedDeviceClass deviceClass)
    {
        if (deviceClass == ETrackedDeviceClass.Controller)
        {
            bool isLeftHand = SteamVR.instance.hmd.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand) == Device.deviceIndex;
            if (isLeftHand)
            {
                TrackedRole = BasisBoneTrackedRole.LeftHand;
            }
            else
            {
                TrackedRole = BasisBoneTrackedRole.RightHand;
            }
        }
        if (deviceClass == ETrackedDeviceClass.HMD)
        {
            TrackedRole = BasisBoneTrackedRole.CenterEye;
        }
    }
    
    public override void PollData()
    {
        result = SteamVR.instance.compositor.GetLastPoseForTrackedDeviceIndex(Device.deviceIndex, ref devicePose, ref deviceGamePose);

        if (result == EVRCompositorError.None)
        {
            deviceTransform = new SteamVR_Utils.RigidTransform(deviceGamePose.mDeviceToAbsoluteTracking);
            LocalRawPosition = deviceTransform.pos;
            LocalRawRotation = deviceTransform.rot;
            if (hasRoleAssigned)
            {
                if (Control.HasTrackerPositionDriver != BasisBoneControl.BasisHasTracked.HasNoTracker && LocalRawPosition != Vector3.zero)
                {
                    Control.LocalRawPosition = LocalRawPosition;
                }
                if (Control.HasTrackerPositionDriver != BasisBoneControl.BasisHasTracked.HasNoTracker && LocalRawRotation != Quaternion.identity)
                {
                    Control.LocalRawRotation = LocalRawRotation;
                }
            }
            
            primary2DAxis = SteamVR_Actions._default.Joystick.GetAxis(inputSource);
            primaryButton = SteamVR_Actions._default.A_Button.GetStateDown(inputSource);
            secondaryButton = SteamVR_Actions._default.B_Button.GetStateDown(inputSource);
            UpdatePlayerControl();
        }
        else
        {
            Debug.LogError("Error getting device pose: " + result);
        }
        transform.SetLocalPositionAndRotation(LocalRawPosition, LocalRawRotation);
    }
}