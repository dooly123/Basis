using UnityEngine;
using Valve.VR;
[DefaultExecutionOrder(15101)]
public class BasisOpenVRInput : BasisInput
{
    public OpenVRDevice Device;
    public SteamVR_ActionSet actionSet;
    public static string ActionName = "default";
    public TrackedDevicePose_t devicePose = new TrackedDevicePose_t();
    public TrackedDevicePose_t deviceGamePose = new TrackedDevicePose_t();
    public SteamVR_Utils.RigidTransform deviceTransform;
    public EVRCompositorError result;
    public void Initialize(OpenVRDevice device, string iD)
    {
        Device = device;
        base.Initialize(iD);
        GetControllerOrHMD(Device.SteamVR_Input_Sources);
        actionSet = SteamVR_Input.GetActionSet(ActionName);
        actionSet.Activate();
    }

    public void GetControllerOrHMD(SteamVR_Input_Sources SteamVR_Input_Sources)
    {
        switch (SteamVR_Input_Sources)
        {
            case SteamVR_Input_Sources.LeftHand:
                TrackedRole = BasisBoneTrackedRole.LeftHand;
                break;
            case SteamVR_Input_Sources.RightHand:
                TrackedRole = BasisBoneTrackedRole.RightHand;
                break;
            case SteamVR_Input_Sources.Head:
                TrackedRole = BasisBoneTrackedRole.CenterEye;
                break;
        }
    }
    public override void PollData()
    {
        result = SteamVR.instance.compositor.GetLastPoseForTrackedDeviceIndex((uint)Device.SteamVR_Input_Sources, ref devicePose, ref deviceGamePose);
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
            SteamVR_Actions.default_Move.GetAxis(Device.SteamVR_Input_Sources);
            primary2DAxis = SteamVR_Actions.default_Move.GetAxis(Device.SteamVR_Input_Sources);
            primaryButton = SteamVR_Actions.default_Jump.GetStateDown(Device.SteamVR_Input_Sources);
            secondaryButton = SteamVR_Actions.default_Menu.GetStateDown(Device.SteamVR_Input_Sources);
            UpdatePlayerControl();
        }
        else
        {
            Debug.LogError("Error getting device pose: " + result);
        }
        transform.SetLocalPositionAndRotation(LocalRawPosition, LocalRawRotation);
    }
}