using UnityEngine;
using Valve.VR;
[DefaultExecutionOrder(15101)]
///only used for trackers!
public class BasisOpenVRInput : BasisInput
{
    public OpenVRDevice Device;
    public TrackedDevicePose_t devicePose = new TrackedDevicePose_t();
    public TrackedDevicePose_t deviceGamePose = new TrackedDevicePose_t();
    public SteamVR_Utils.RigidTransform deviceTransform;
    public EVRCompositorError result;

    public void Initialize(OpenVRDevice device, string UniqueID, string UnUniqueID,string subSystems)
    {
        Device = device;

        ActivateTracking(UniqueID, UnUniqueID, SubSystem);
    }
    public void Update()
    {
        PollData();
    }
    public void LateUpdate()
    {
        PollData();
    }
    public override void PollData()
    {
        if (SteamVR.active)
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
                        Control.TrackerData.position = LocalRawPosition - LocalRawRotation * pivotOffset;
                    }
                    if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawRotation != Quaternion.identity)
                    {
                        Control.TrackerData.rotation = LocalRawRotation * rotationOffset;
                    }
                }
                if (HasInputSource)
                {
                    State.Primary2DAxis = SteamVR_Actions._default.Joystick.GetAxis(inputSource);
                    State.PrimaryButtonGetState = SteamVR_Actions._default.A_Button.GetState(inputSource);
                    State.SecondaryButtonGetState = SteamVR_Actions._default.B_Button.GetState(inputSource);
                    State.Trigger = SteamVR_Actions._default.Trigger.GetAxis(inputSource);
                }
                UpdatePlayerControl();
            }
            else
            {
                Debug.LogError("Error getting device pose: " + result);
            }
            transform.SetLocalPositionAndRotation(LocalRawPosition, LocalRawRotation);
        }
    }
    public bool HasInputSource = false;
    public SteamVR_Input_Sources inputSource;
}