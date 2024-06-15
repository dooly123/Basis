using System;
using UnityEngine;
using UnityEngine.Serialization;
using Valve.VR;

[DefaultExecutionOrder(15101)]
public class BasisOpenVRInput : BasisInput
{
    public OpenVRDevice Device;
    public SteamVR_Input_Sources inputSource;
    public TrackedDevicePose_t devicePose = new TrackedDevicePose_t();
    public TrackedDevicePose_t deviceGamePose = new TrackedDevicePose_t();
    public SteamVR_Utils.RigidTransform deviceTransform;
    public EVRCompositorError result;

    public void Initialize(OpenVRDevice device, string UniqueID, string UnUniqueID)
    {
        Device = device;
        TryAssignRole(Device.deviceClass);
        ActivateTracking(UniqueID, UnUniqueID);
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
                if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawPosition != Vector3.zero)
                {
                    Vector3 pivotOffset = LocalRawRotation * base.pivotOffset;
                    Control.LocalRawPosition = LocalRawPosition - pivotOffset;
                }
                if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawRotation != Quaternion.identity)
                {
                    Control.LocalRawRotation = LocalRawRotation * Quaternion.Euler(base.rotationOffset);
                }
            }

            State.primary2DAxis = SteamVR_Actions._default.Joystick.GetAxis(inputSource);
            State.primaryButtonGetState = SteamVR_Actions._default.A_Button.GetState(inputSource);
            State.secondaryButtonGetState = SteamVR_Actions._default.B_Button.GetState(inputSource);
            UpdatePlayerControl();
        }
        else
        {
            Debug.LogError("Error getting device pose: " + result);
        }
        transform.SetLocalPositionAndRotation(LocalRawPosition, LocalRawRotation);
    }
}