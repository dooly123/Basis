using UnityEngine;
using Valve.VR;

[DefaultExecutionOrder(15101)]
public class BasisOpenVRInput : BasisInput
{
    public OpenVRDevice Device;
    public SteamVR_ActionSet actionSet;
    public SteamVR_Action_Pose poseAction;

    public Vector2 primary2DAxisL;
    public Vector2 primary2DAxisR;
    public static string ActionName = "default";
    public void Initialize(OpenVRDevice device, string iD)
    {
        base.Initialize(iD);
        Device = device;
        TrackedRole = device.deviceType;
        actionSet = SteamVR_Input.GetActionSet(ActionName);
        actionSet.Activate();
    }

    public override void PollData()
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

        if (Control.HasTrackerPositionDriver != BasisBoneControl.BasisHasTracked.HasNoTracker && LocalRawPosition != Vector3.zero)
        {
            Control.LocalRawPosition = LocalRawPosition;
        }

        if (Control.HasTrackerPositionDriver != BasisBoneControl.BasisHasTracked.HasNoTracker && LocalRawRotation != Quaternion.identity)
        {
            Control.LocalRawRotation = LocalRawRotation;
        }

        UpdatePlayerControl();
        transform.SetLocalPositionAndRotation(LocalRawPosition, LocalRawRotation);
    }

    private void UpdatePlayerControl()
    {
        if (TrackedRole == BasisBoneTrackedRole.LeftHand)
        {
            primary2DAxisL = SteamVR_Actions.default_Move.GetAxis(SteamVR_Input_Sources.LeftHand);
            primaryButton = SteamVR_Actions.default_Jump.GetStateDown(SteamVR_Input_Sources.Any);
            secondaryButton = SteamVR_Actions.default_Menu.GetStateDown(SteamVR_Input_Sources.Any);

            BasisLocalPlayer.Instance.Move.MovementVector = primary2DAxisL;
            if (primaryButton)
            {
                BasisLocalPlayer.Instance.Move.HandleJump();
            }
            if (secondaryButton)
            {
                if (BasisHamburgerMenu.Instance == null && !BasisHamburgerMenu.IsLoading)
                {
                    BasisHamburgerMenu.OpenMenu();
                }
                else
                {
                    BasisHamburgerMenu.Instance.CloseThisMenu();
                }
            }
        }
        else if (TrackedRole == BasisBoneTrackedRole.RightHand)
        {
            primary2DAxisR = SteamVR_Actions.default_Rotate.GetAxis(SteamVR_Input_Sources.Any);
            BasisLocalPlayer.Instance.Move.Rotation = primary2DAxisR;
        }
    }
}