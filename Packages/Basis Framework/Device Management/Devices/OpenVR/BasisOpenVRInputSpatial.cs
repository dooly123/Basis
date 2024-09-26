using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management.Devices.OpenVR;
using Basis.Scripts.TransformBinders.BoneControl;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SpatialTracking;
using Valve.VR;

namespace Basis.Scripts.Device_Management.Devices.Unity_Spatial_Tracking
{
    [DefaultExecutionOrder(15001)]
    public class BasisOpenVRInputSpatial : BasisInput
    {
        public TrackedPoseDriver.TrackedPose TrackedPose = TrackedPoseDriver.TrackedPose.Center;
        public BasisOpenVRInputEye BasisOpenVRInputEye;
        public async Task Initialize(TrackedPoseDriver.TrackedPose trackedPose, string UniqueID, string UnUniqueID, string subSystems, bool AssignTrackedRole, BasisBoneTrackedRole basisBoneTrackedRole, SteamVR_Input_Sources SteamVR_Input_Sources)
        {
            TrackedPose = trackedPose;
            await InitalizeTracking(UniqueID, UnUniqueID, subSystems, AssignTrackedRole, basisBoneTrackedRole);
            if (basisBoneTrackedRole == BasisBoneTrackedRole.CenterEye)
            {
                BasisOpenVRInputEye = gameObject.AddComponent<BasisOpenVRInputEye>();
                BasisOpenVRInputEye.Initalize();
            }
        }
        public new void OnDestroy()
        {
            base.OnDestroy();
        }
        public override void DoPollData()
        {
            if (PoseDataSource.TryGetDataFromSource(TrackedPose, out Pose resultPose))
            {
                LocalRawPosition = resultPose.position;
                LocalRawRotation = resultPose.rotation;

                FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale;
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
                if (TryGetRole(out var CurrentRole))
                {
                    if (CurrentRole == BasisBoneTrackedRole.CenterEye)
                    {
                        BasisOpenVRInputEye.Simulate();
                    }
                }
            }
            UpdatePlayerControl();
        }
    }
}