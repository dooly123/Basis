using UnityEditor.VersionControl;
using UnityEngine;
using Valve.VR;
[DefaultExecutionOrder(15101)]
public class BasisOpenVRInput : BasisInput
{
    public OpenVRDevice Device;

    public SteamVR_Action_Vector2 moveAction;
    public SteamVR_Action_Boolean jumpAction;
    public SteamVR_Action_Boolean menuAction;
    public SteamVR_Action_Vector2 rotateAction;
    
    public Vector2 primary2DAxisL;
    public Vector2 primary2DAxisR;
    public async void Initialize(OpenVRDevice device, string iD)
    {
        Device = device;
        TrackedRole = device.deviceType;
        
        SteamVR_Input.Initialize();
        SteamVR_Actions._default.Initialize();
        moveAction = SteamVR_Actions.default_Move;
        jumpAction = SteamVR_Actions.default_Jump;
        menuAction = SteamVR_Actions.default_Menu;
        rotateAction = SteamVR_Actions.default_Rotate;

        // This is terrible, but it keeps the input from being null first frame
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        
        // Initialize after setting the device and role
        base.Initialize(iD);
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
            primary2DAxisL = moveAction.GetAxis(SteamVR_Input_Sources.Any);
            primaryButton = jumpAction.GetStateDown(SteamVR_Input_Sources.Any);
            secondaryButton = menuAction.GetStateDown(SteamVR_Input_Sources.Any);

            BasisLocalPlayer.Instance.Move.MovementVector = primary2DAxisL;
            if (primaryButton)
            {
                BasisLocalPlayer.Instance.Move.HandleJump();
            }
            if (secondaryButton)
            {
                if (BasisHamburgerMenu.Instance == null)
                {
                    if (!BasisHamburgerMenu.IsLoading)
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
        else if (TrackedRole == BasisBoneTrackedRole.RightHand)
        {
            if (rotateAction.GetActive(SteamVR_Input_Sources.Any))
            {
                primary2DAxisR = rotateAction.GetAxis(SteamVR_Input_Sources.Any);
            }

            BasisLocalPlayer.Instance.Move.Rotation = primary2DAxisR;
        }
    }
}