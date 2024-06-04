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
        CalculateTransformPositions(RemotePlayer.Avatar.Animator, remotePlayer.RemoteDriver);
        ComputeOffsets(remotePlayer.RemoteDriver);
        RemotePlayer.Avatar.Animator.enabled = false;
        CalibrationComplete.Invoke();
    }
    public void ComputeOffsets(BaseBoneDriver BaseBoneDriver)
    {
        //head
        //   SetAndCreateLock(BaseBoneDriver, BoneTrackedRole.CenterEye, BoneTrackedRole.Head, RotationalControl.ClampData.None, 5, 12, true, 5f);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Neck, BasisRotationalControl.BasisClampData.Clamp, 5, 12, true, 4, BasisTargetController.Target, BasisRotationalControl.BasisClampAxis.xz);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.CenterEye, BasisRotationalControl.BasisClampData.None, 5, 12, true, 5f);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Mouth, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);


        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Neck, BasisBoneTrackedRole.UpperChest, BasisRotationalControl.BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.Chest, BasisRotationalControl.BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.Spine, BasisRotationalControl.BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Hips, BasisRotationalControl.BasisClampData.None, 0, 12, true, 4, BasisTargetController.Target, BasisRotationalControl.BasisClampAxis.x, false);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.LeftShoulder, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.RightShoulder, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftShoulder, BasisBoneTrackedRole.LeftUpperArm, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightShoulder, BasisBoneTrackedRole.RightUpperArm, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperArm, BasisBoneTrackedRole.LeftLowerArm, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperArm, BasisBoneTrackedRole.RightLowerArm, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerArm, BasisBoneTrackedRole.LeftHand, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerArm, BasisBoneTrackedRole.RightHand, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);

        //legs
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.LeftUpperLeg, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.RightUpperLeg, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperLeg, BasisBoneTrackedRole.LeftLowerLeg, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperLeg, BasisBoneTrackedRole.RightLowerLeg, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerLeg, BasisBoneTrackedRole.LeftFoot, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerLeg, BasisBoneTrackedRole.RightFoot, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftToes, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightToes, BasisRotationalControl.BasisClampData.None, 0, 12, false, 4);
    }
    public bool IsAble()
    {
        if (IsNull(RemotePlayer.Avatar))
        {
            return false;
        }
        if (IsNull(RemotePlayer.RemoteDriver))
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