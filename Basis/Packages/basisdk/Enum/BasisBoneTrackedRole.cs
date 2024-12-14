namespace Basis.Scripts.TransformBinders.BoneControl
{
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

        Mouth,

        LeftThumbProximal,
        LeftThumbIntermediate,
        LeftThumbDistal,
        LeftIndexProximal,
        LeftIndexIntermediate,
        LeftIndexDistal,
        LeftMiddleProximal,
        LeftMiddleIntermediate,
        LeftMiddleDistal,
        LeftRingProximal,
        LeftRingIntermediate,
        LeftRingDistal,
        LeftLittleProximal,
        LeftLittleIntermediate,
        LeftLittleDistal,

        RightThumbProximal,
        RightThumbIntermediate,
        RightThumbDistal,
        RightIndexProximal,
        RightIndexIntermediate,
        RightIndexDistal,
        RightMiddleProximal,
        RightMiddleIntermediate,
        RightMiddleDistal,
        RightRingProximal,
        RightRingIntermediate,
        RightRingDistal,
        RightLittleProximal,
        RightLittleIntermediate,
        RightLittleDistal,
    }
    public static class BasisBoneTrackedRoleCommonCheck
    {
        public static bool CheckItsFBTracker(BasisBoneTrackedRole role)
        {
            return CheckIfHeadAreaTracker(role) == false &&
                   role != BasisBoneTrackedRole.LeftHand &&
                   role != BasisBoneTrackedRole.RightHand &&
                   role != BasisBoneTrackedRole.LeftUpperLeg &&
                   role != BasisBoneTrackedRole.RightUpperLeg &&
                   role != BasisBoneTrackedRole.LeftUpperArm &&
                   role != BasisBoneTrackedRole.RightUpperArm &&
                   role != BasisBoneTrackedRole.Spine &&
                   CheckIfLeftHand(role) == false &&
                   CheckIfRightHand(role) == false;
        }
        public static bool CheckIfHintRole(BasisBoneTrackedRole role)
        {
            bool IsHintRole = (role == BasisBoneTrackedRole.LeftLowerArm || role == BasisBoneTrackedRole.RightLowerArm || role == BasisBoneTrackedRole.LeftLowerLeg || role == BasisBoneTrackedRole.RightLowerLeg);
            return IsHintRole;
        }
        public static bool CheckIfLeftHand(BasisBoneTrackedRole role)
        {
            switch (role)
            {
                case BasisBoneTrackedRole.LeftThumbProximal:
                case BasisBoneTrackedRole.LeftThumbIntermediate:
                case BasisBoneTrackedRole.LeftThumbDistal:
                case BasisBoneTrackedRole.LeftIndexProximal:
                case BasisBoneTrackedRole.LeftIndexIntermediate:
                case BasisBoneTrackedRole.LeftIndexDistal:
                case BasisBoneTrackedRole.LeftMiddleProximal:
                case BasisBoneTrackedRole.LeftMiddleIntermediate:
                case BasisBoneTrackedRole.LeftMiddleDistal:
                case BasisBoneTrackedRole.LeftRingProximal:
                case BasisBoneTrackedRole.LeftRingIntermediate:
                case BasisBoneTrackedRole.LeftRingDistal:
                case BasisBoneTrackedRole.LeftLittleProximal:
                case BasisBoneTrackedRole.LeftLittleIntermediate:
                case BasisBoneTrackedRole.LeftLittleDistal:
                    return true;
                default:
                    return false;
            }
        }
        public static bool CheckIfRightHand(BasisBoneTrackedRole role)
        {
            switch (role)
            {
                case BasisBoneTrackedRole.RightThumbProximal:
                case BasisBoneTrackedRole.RightThumbIntermediate:
                case BasisBoneTrackedRole.RightThumbDistal:
                case BasisBoneTrackedRole.RightIndexProximal:
                case BasisBoneTrackedRole.RightIndexIntermediate:
                case BasisBoneTrackedRole.RightIndexDistal:
                case BasisBoneTrackedRole.RightMiddleProximal:
                case BasisBoneTrackedRole.RightMiddleIntermediate:
                case BasisBoneTrackedRole.RightMiddleDistal:
                case BasisBoneTrackedRole.RightRingProximal:
                case BasisBoneTrackedRole.RightRingIntermediate:
                case BasisBoneTrackedRole.RightRingDistal:
                case BasisBoneTrackedRole.RightLittleProximal:
                case BasisBoneTrackedRole.RightLittleIntermediate:
                case BasisBoneTrackedRole.RightLittleDistal:
                    return true;
                default:
                    return false;
            }
        }
        public static bool CheckIfHeadAreaTracker(BasisBoneTrackedRole role)
        {
            return role == BasisBoneTrackedRole.CenterEye || role == BasisBoneTrackedRole.Head || role == BasisBoneTrackedRole.Neck || role == BasisBoneTrackedRole.Mouth;
        }
    }
}