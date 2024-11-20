using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;

namespace Basis.Scripts.Drivers
{
    public class BasisRemoteAvatarDriver : BasisAvatarDriver
    {
        public BasisRemotePlayer RemotePlayer;
        public void RemoteCalibration(BasisRemotePlayer remotePlayer)
        {
            RemotePlayer = remotePlayer;
            if (IsAble())
            {
                Debug.Log("RemoteCalibration Underway");
            }
            else
            {
                return;
            }
            Calibration(RemotePlayer.Avatar);
            SetAllMatrixRecalculation(false);
            updateWhenOffscreen(false);
            RemotePlayer.Avatar.Animator.logWarnings = false;
            for (int Index = 0; Index < SkinnedMeshRenderer.Length; Index++)
            {
                SkinnedMeshRenderer[Index].forceMatrixRecalculationPerRender = false;
            }
            CalculateTransformPositions(RemotePlayer.Avatar.Animator, remotePlayer.RemoteBoneDriver);
            ComputeOffsets(remotePlayer.RemoteBoneDriver);
            RemotePlayer.Avatar.Animator.enabled = false;
            CalibrationComplete?.Invoke();
        }
        public void ComputeOffsets(BaseBoneDriver BBD)
        {
            //head
            //   SetAndCreateLock(BaseBoneDriver, BoneTrackedRole.CenterEye, BoneTrackedRole.Head, RotationalControl.ClampData.None, 5, 12, true, 5f);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Neck,  40, 12, true);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.CenterEye,  40, 12, true);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Mouth,  40, 12, true);


            SetAndCreateLock(BBD, BasisBoneTrackedRole.Neck, BasisBoneTrackedRole.Chest,  40, 12, true);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.Spine,  40, 12, true);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Hips, 40, 12, true);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.LeftShoulder,  40, 12, true);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.RightShoulder,  40, 12, true);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftShoulder, BasisBoneTrackedRole.LeftUpperArm,  40, 12, true);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightShoulder, BasisBoneTrackedRole.RightUpperArm,  40, 12, true);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftUpperArm, BasisBoneTrackedRole.LeftLowerArm,  40, 12, true);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightUpperArm, BasisBoneTrackedRole.RightLowerArm,  40, 12, true);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftLowerArm, BasisBoneTrackedRole.LeftHand,  40, 12, true);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightLowerArm, BasisBoneTrackedRole.RightHand,  40, 12, true);

            //legs
            SetAndCreateLock(BBD, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.LeftUpperLeg,  40, 12, true);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.RightUpperLeg,  40, 12, true);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftUpperLeg, BasisBoneTrackedRole.LeftLowerLeg,  40, 12, true);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightUpperLeg, BasisBoneTrackedRole.RightLowerLeg,  40, 12, true);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftLowerLeg, BasisBoneTrackedRole.LeftFoot,  40, 12, true);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightLowerLeg, BasisBoneTrackedRole.RightFoot,  40, 12, true);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftToes,  40, 12, true);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightToes,  40, 12, true);


            // Setting up locks for Left Hand
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftThumbProximal,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftThumbProximal, BasisBoneTrackedRole.LeftThumbIntermediate,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftThumbIntermediate, BasisBoneTrackedRole.LeftThumbDistal,  40, 12, false);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftIndexProximal,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftIndexProximal, BasisBoneTrackedRole.LeftIndexIntermediate,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftIndexIntermediate, BasisBoneTrackedRole.LeftIndexDistal,  40, 12, false);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftMiddleProximal,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftMiddleProximal, BasisBoneTrackedRole.LeftMiddleIntermediate,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftMiddleIntermediate, BasisBoneTrackedRole.LeftMiddleDistal,  40, 12, false);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftRingProximal,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftRingProximal, BasisBoneTrackedRole.LeftRingIntermediate,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftRingIntermediate, BasisBoneTrackedRole.LeftRingDistal,  40, 12, false);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftLittleProximal,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftLittleProximal, BasisBoneTrackedRole.LeftLittleIntermediate,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.LeftLittleIntermediate, BasisBoneTrackedRole.LeftLittleDistal,  40, 12, false);

            // Setting up locks for Right Hand
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightThumbProximal,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightThumbProximal, BasisBoneTrackedRole.RightThumbIntermediate,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightThumbIntermediate, BasisBoneTrackedRole.RightThumbDistal,  40, 12, false);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightIndexProximal,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightIndexProximal, BasisBoneTrackedRole.RightIndexIntermediate,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightIndexIntermediate, BasisBoneTrackedRole.RightIndexDistal,  40, 12, false);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightMiddleProximal,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightMiddleProximal, BasisBoneTrackedRole.RightMiddleIntermediate,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightMiddleIntermediate, BasisBoneTrackedRole.RightMiddleDistal,  40, 12, false);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightRingProximal,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightRingProximal, BasisBoneTrackedRole.RightRingIntermediate,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightRingIntermediate, BasisBoneTrackedRole.RightRingDistal,  40, 12, false);

            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightLittleProximal,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightLittleProximal, BasisBoneTrackedRole.RightLittleIntermediate,  40, 12, false);
            SetAndCreateLock(BBD, BasisBoneTrackedRole.RightLittleIntermediate, BasisBoneTrackedRole.RightLittleDistal,  40, 12, false);
        }
        public bool IsAble()
        {
            if (IsNull(RemotePlayer.Avatar))
            {
                return false;
            }
            if (IsNull(RemotePlayer.RemoteBoneDriver))
            {
                return false;
            }
            if (IsNull(RemotePlayer.Avatar.Animator))
            {
                return false;
            }

            if (IsNull(RemotePlayer))
            {
                return false;
            }
            return true;
        }
    }
}