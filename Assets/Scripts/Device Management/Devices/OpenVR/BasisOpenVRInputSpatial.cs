using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;
using UnityEngine.SpatialTracking;
using Valve.VR;

namespace Basis.Scripts.Device_Management.Devices.Unity_Spatial_Tracking
{
[DefaultExecutionOrder(15101)]
public class BasisOpenVRInputSpatial : BasisInput
{
    public TrackedPoseDriver.TrackedPose TrackedPose = TrackedPoseDriver.TrackedPose.Center;
    public void Initialize(TrackedPoseDriver.TrackedPose trackedPose, string UniqueID, string UnUniqueID, string subSystems, bool AssignTrackedRole, BasisBoneTrackedRole basisBoneTrackedRole, SteamVR_Input_Sources SteamVR_Input_Sources)
    {
        TrackedPose = trackedPose;
        InitalizeTracking(UniqueID, UnUniqueID, subSystems, AssignTrackedRole, basisBoneTrackedRole);
    }
    public new void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void PollData()
    {
        if (PoseDataSource.TryGetDataFromSource(TrackedPose, out Pose resultPose))
        {
            LocalRawPosition = resultPose.position;
            LocalRawRotation = resultPose.rotation;

            FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;
            FinalRotation = LocalRawRotation;
            if (hasRoleAssigned)
            {
                if (Control.HasTracked != BasisHasTracked.HasNoTracker)
                {
                    Control.IncomingData.position = FinalPosition - FinalRotation * AvatarPositionOffset;
                }
                if (Control.HasTracked != BasisHasTracked.HasNoTracker)
                {
                    Control.IncomingData.rotation = FinalRotation * AvatarRotationOffset;
                }
            }
        }
        UpdatePlayerControl();
    }
}
}