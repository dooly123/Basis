using UnityEngine;

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
        SetMatrixRecalculation(false);
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


        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Neck, BasisBoneTrackedRole.UpperChest, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.Chest, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.Spine, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Hips, BasisTargetController.None, 40, BasisClampData.None, 0, 12, true, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.LeftShoulder, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.RightShoulder, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

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