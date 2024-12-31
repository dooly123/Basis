using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management.Devices.OpenVR;
using Basis.Scripts.TransformBinders.BoneControl;
using Unity.Mathematics;
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
        public BasisVirtualSpineDriver BasisVirtualSpine = new BasisVirtualSpineDriver();
        public void Initialize(TrackedPoseDriver.TrackedPose trackedPose, string UniqueID, string UnUniqueID, string subSystems, bool AssignTrackedRole, BasisBoneTrackedRole basisBoneTrackedRole, SteamVR_Input_Sources SteamVR_Input_Sources)
        {
            TrackedPose = trackedPose;
            InitalizeTracking(UniqueID, UnUniqueID, subSystems, AssignTrackedRole, basisBoneTrackedRole);
            if (basisBoneTrackedRole == BasisBoneTrackedRole.CenterEye)
            {
                BasisOpenVRInputEye = gameObject.AddComponent<BasisOpenVRInputEye>();
                BasisOpenVRInputEye.Initalize();
                BasisVirtualSpine.Initialize();
            }
        }
        public new void OnDestroy()
        {
            BasisVirtualSpine.DeInitialize();
            base.OnDestroy();
        }
        public override void DoPollData()
        {
            if (PoseDataSource.TryGetDataFromSource(TrackedPose, out Pose resultPose))
            {
                LocalRawPosition = (float3)resultPose.position;
                LocalRawRotation = resultPose.rotation;
                if (hasRoleAssigned)
                {
                    if (Control.HasTracked != BasisHasTracked.HasNoTracker)
                    {
                        Control.IncomingData.position = FinalPosition - math.mul(FinalRotation, AvatarPositionOffset * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale);
                        Control.IncomingData.rotation = math.mul(FinalRotation, Quaternion.Euler(AvatarRotationOffset));
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
            FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale;
            FinalRotation = LocalRawRotation;
            UpdatePlayerControl();
        }
    }
}