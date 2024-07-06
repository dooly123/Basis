public enum BasisBoneTrackedRole
{
    CenterEye,
    Head,
    Neck,
    Chest,
    Hips,
    Spine, 

    LeftUpperLeg,
    RightUpperLeg,
    LeftLowerLeg,
    RightLowerLeg,
    LeftFoot,
    RightFoot,
    UpperChest,
    LeftShoulder,
    RightShoulder,
    LeftUpperArm,
    RightUpperArm,
    LeftLowerArm,
    RightLowerArm,
    LeftHand,
    RightHand,
    LeftToes,
    RightToes,

    Mouth
}
public static class BasisBoneTrackedRoleCommonCheck
{
    public static bool CheckItsFBTracker(BasisBoneTrackedRole Role)
    {
        if (Role != BasisBoneTrackedRole.CenterEye && Role != BasisBoneTrackedRole.LeftHand && Role != BasisBoneTrackedRole.RightHand)
        {
            return true;
        }
        return false;
    }
}