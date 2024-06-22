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
        BeginningCalibration.Invoke();
        for (int Index = 0; Index < SkinnedMeshRenderer.Length; Index++)
        {
            SkinnedMeshRenderer Render = SkinnedMeshRenderer[Index];
            Render.forceMatrixRecalculationPerRender = false;
        }
        CalculateTransformPositions(RemotePlayer.Avatar.Animator, remotePlayer.RemoteBoneDriver);
        ComputeOffsets(remotePlayer.RemoteBoneDriver);
        RemotePlayer.Avatar.Animator.enabled = false;
        CalibrationComplete.Invoke();
    }
    public void ComputeOffsets(BaseBoneDriver BaseBoneDriver)
    {
        //head
        //   SetAndCreateLock(BaseBoneDriver, BoneTrackedRole.CenterEye, BoneTrackedRole.Head, RotationalControl.ClampData.None, 5, 12, true, 5f);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Neck, BasisClampData.Clamp, 5, 12, true, 4, BasisTargetController.Target, BasisClampAxis.xz);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.CenterEye, BasisClampData.None, 5, 12, true, 5f);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Mouth, BasisClampData.None, 0, 12, false, 4);


        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Neck, BasisBoneTrackedRole.UpperChest, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.Chest, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.Spine, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Hips, BasisClampData.None, 0, 12, true, 4, BasisTargetController.Target, BasisClampAxis.x, false);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.LeftShoulder, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.RightShoulder, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftShoulder, BasisBoneTrackedRole.LeftUpperArm, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightShoulder, BasisBoneTrackedRole.RightUpperArm, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperArm, BasisBoneTrackedRole.LeftLowerArm, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperArm, BasisBoneTrackedRole.RightLowerArm, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerArm, BasisBoneTrackedRole.LeftHand, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerArm, BasisBoneTrackedRole.RightHand, BasisClampData.None, 0, 12, false, 4);

        //legs
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.LeftUpperLeg, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.RightUpperLeg, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperLeg, BasisBoneTrackedRole.LeftLowerLeg, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperLeg, BasisBoneTrackedRole.RightLowerLeg, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerLeg, BasisBoneTrackedRole.LeftFoot, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerLeg, BasisBoneTrackedRole.RightFoot, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftToes, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightToes, BasisClampData.None, 0, 12, false, 4);
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