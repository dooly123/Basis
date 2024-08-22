using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Common.Enums;
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
        for (int Index = 0; Index < SkinnedMeshRenderer.Length; Index++)
        {
            SkinnedMeshRenderer Render = SkinnedMeshRenderer[Index];
            Render.forceMatrixRecalculationPerRender = false;
        }
        CalculateTransformPositions(RemotePlayer.Avatar.Animator, remotePlayer.RemoteBoneDriver);
        ComputeOffsets(remotePlayer.RemoteBoneDriver);
        RemotePlayer.Avatar.Animator.enabled = false;
        CalibrationComplete?.Invoke();
    }
    public void ComputeOffsets(BaseBoneDriver BaseBoneDriver)
    {
        //head
        //   SetAndCreateLock(BaseBoneDriver, BoneTrackedRole.CenterEye, BoneTrackedRole.Head, RotationalControl.ClampData.None, 5, 12, true, 5f);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Neck, BasisTargetController.TargetDirectional, 40, BasisClampData.Clamp, 5, 12, true, 4, BasisTargetController.Target, BasisClampAxis.xz);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.CenterEye, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 5, 12, true, 5f);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Mouth, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);


        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Neck, BasisBoneTrackedRole.Chest, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.Spine, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Hips, BasisTargetController.None, 40, BasisClampData.None, 0, 12, true, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.LeftShoulder, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.RightShoulder, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftShoulder, BasisBoneTrackedRole.LeftUpperArm, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightShoulder, BasisBoneTrackedRole.RightUpperArm, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperArm, BasisBoneTrackedRole.LeftLowerArm, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperArm, BasisBoneTrackedRole.RightLowerArm, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerArm, BasisBoneTrackedRole.LeftHand, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerArm, BasisBoneTrackedRole.RightHand, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        //legs
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.LeftUpperLeg, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.RightUpperLeg, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperLeg, BasisBoneTrackedRole.LeftLowerLeg, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperLeg, BasisBoneTrackedRole.RightLowerLeg, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerLeg, BasisBoneTrackedRole.LeftFoot, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerLeg, BasisBoneTrackedRole.RightFoot, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftToes, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightToes, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);


        // Setting up locks for Left Hand
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftThumbProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftThumbProximal, BasisBoneTrackedRole.LeftThumbIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftThumbIntermediate, BasisBoneTrackedRole.LeftThumbDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftIndexProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftIndexProximal, BasisBoneTrackedRole.LeftIndexIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftIndexIntermediate, BasisBoneTrackedRole.LeftIndexDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftMiddleProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftMiddleProximal, BasisBoneTrackedRole.LeftMiddleIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftMiddleIntermediate, BasisBoneTrackedRole.LeftMiddleDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftRingProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftRingProximal, BasisBoneTrackedRole.LeftRingIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftRingIntermediate, BasisBoneTrackedRole.LeftRingDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftLittleProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLittleProximal, BasisBoneTrackedRole.LeftLittleIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLittleIntermediate, BasisBoneTrackedRole.LeftLittleDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        // Setting up locks for Right Hand
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightThumbProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightThumbProximal, BasisBoneTrackedRole.RightThumbIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightThumbIntermediate, BasisBoneTrackedRole.RightThumbDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightIndexProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightIndexProximal, BasisBoneTrackedRole.RightIndexIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightIndexIntermediate, BasisBoneTrackedRole.RightIndexDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightMiddleProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightMiddleProximal, BasisBoneTrackedRole.RightMiddleIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightMiddleIntermediate, BasisBoneTrackedRole.RightMiddleDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightRingProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightRingProximal, BasisBoneTrackedRole.RightRingIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightRingIntermediate, BasisBoneTrackedRole.RightRingDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightLittleProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLittleProximal, BasisBoneTrackedRole.RightLittleIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLittleIntermediate, BasisBoneTrackedRole.RightLittleDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
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