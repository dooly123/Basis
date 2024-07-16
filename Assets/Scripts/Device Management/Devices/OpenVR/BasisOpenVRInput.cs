using UnityEngine;
using Valve.VR;
[DefaultExecutionOrder(15101)]
///only used for trackers!
public class BasisOpenVRInput : BasisInput
{
    [SerializeField]
    public OpenVRDevice Device;
    public TrackedDevicePose_t devicePose = new TrackedDevicePose_t();
    public TrackedDevicePose_t deviceGamePose = new TrackedDevicePose_t();
    public SteamVR_Utils.RigidTransform deviceTransform;
    public EVRCompositorError result;
    public bool HasInputSource = false;
    public SteamVR_Input_Sources inputSource;
    public void Initialize(OpenVRDevice device, string UniqueID, string UnUniqueID, string subSystems, bool AssignTrackedRole, BasisBoneTrackedRole basisBoneTrackedRole)
    {
        Device = device;
        InitalizeTracking(UniqueID, UnUniqueID, subSystems, AssignTrackedRole, basisBoneTrackedRole);
        BasisLocalPlayer.Instance.Move.ReadyToRead += ApplyFinalMovement;
    }
    public void ApplyFinalMovement()
    {
        transform.SetLocalPositionAndRotation(FinalPosition, FinalRotation);
    }
    public override void PollData()
    {
        if (SteamVR.active)
        {
            result = SteamVR.instance.compositor.GetLastPoseForTrackedDeviceIndex(Device.deviceIndex, ref devicePose, ref deviceGamePose);
            if (result == EVRCompositorError.None)
            {
                if (deviceGamePose.bPoseIsValid)
                {
                    deviceTransform = new SteamVR_Utils.RigidTransform(deviceGamePose.mDeviceToAbsoluteTracking);
                    LocalRawPosition = deviceTransform.pos;
                    LocalRawRotation = deviceTransform.rot;

                    FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;
                    FinalRotation = LocalRawRotation;
                    if (hasRoleAssigned)
                    {
                        if (Control.HasTracked != BasisHasTracked.HasNoTracker)
                        {
                            Control.TrackerData.position = FinalPosition - FinalRotation * AvatarPositionOffset;
                        }
                        if (Control.HasTracked != BasisHasTracked.HasNoTracker)
                        {
                            Control.TrackerData.rotation = FinalRotation * AvatarRotationOffset;
                        }
                    }
                    if (HasInputSource)
                    {
                        InputState.Primary2DAxis = SteamVR_Actions._default.Joystick.GetAxis(inputSource);
                        InputState.PrimaryButtonGetState = SteamVR_Actions._default.A_Button.GetState(inputSource);
                        InputState.SecondaryButtonGetState = SteamVR_Actions._default.B_Button.GetState(inputSource);
                        InputState.Trigger = SteamVR_Actions._default.Trigger.GetAxis(inputSource);
                    }
                    UpdatePlayerControl();
                }
            }
            else
            {
                Debug.LogError("Error getting device pose: " + result);
            }
        }
    }
}