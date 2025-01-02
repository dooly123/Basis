using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Eye_Follow;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;

namespace Basis.Scripts.Drivers
{
    public class BasisRemoteAvatarDriver : BasisAvatarDriver
    {
        public BasisRemotePlayer RemotePlayer;
        public BasisEyeFollowBase BasisEyeFollowDriver;
        public void RemoteCalibration(BasisRemotePlayer remotePlayer)
        {
            RemotePlayer = remotePlayer;
            if (IsAble())
            {
                BasisDebug.Log("RemoteCalibration Underway", BasisDebug.LogTag.Avatar);
            }
            else
            {
                return;
            }
            Calibration(RemotePlayer.BasisAvatar);
            BasisEyeFollowDriver = BasisHelpers.GetOrAddComponent<BasisEyeFollowBase>(Player.BasisAvatar.gameObject);
            BasisEyeFollowDriver.Initalize(this, Player);
            SetAllMatrixRecalculation(false);
            updateWhenOffscreen(false);
            RemotePlayer.BasisAvatar.Animator.logWarnings = false;
            for (int Index = 0; Index < SkinnedMeshRenderer.Length; Index++)
            {
                SkinnedMeshRenderer[Index].forceMatrixRecalculationPerRender = false;
            }
            CalculateTransformPositions(RemotePlayer.BasisAvatar.Animator, remotePlayer.RemoteBoneDriver);
            ComputeOffsets(remotePlayer.RemoteBoneDriver);
            RemotePlayer.BasisAvatar.Animator.enabled = false;
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
        }
        public bool IsAble()
        {
            if (IsNull(RemotePlayer.BasisAvatar))
            {
                return false;
            }
            if (IsNull(RemotePlayer.RemoteBoneDriver))
            {
                return false;
            }
            if (IsNull(RemotePlayer.BasisAvatar.Animator))
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
