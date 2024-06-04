using UnityEngine;

public class BasisTransformMapping
{
    public Transform AnimatorRoot;
    public Transform Hips;
    public Transform spine;
    public Transform chest;
    public Transform neck;
    public Transform head;

    public Transform LeftEye;
    public Transform RightEye;

    public Transform leftShoulder;
    public Transform leftUpperArm;
    public Transform leftLowerArm;
    public Transform leftHand;

    public Transform RightShoulder;
    public Transform RightUpperArm;
    public Transform RightLowerArm;
    public Transform rightHand;

    public Transform LeftUpperLeg;
    public Transform LeftLowerLeg;
    public Transform leftFoot;
    public Transform leftToes;

    public Transform RightUpperLeg;
    public Transform RightLowerLeg;
    public Transform rightFoot;
    public Transform rightToes;
    public static bool AutoDetectReferences(Animator Anim, Transform AnimatorRoot, out BasisTransformMapping references)
    {
        references = new BasisTransformMapping();
        if (!Anim.isHuman)
        {
            Debug.LogError("We need a Humanoid Animator");
            return false;
        }

        references.AnimatorRoot = AnimatorRoot;
        references.Hips = Anim.GetBoneTransform(HumanBodyBones.Hips);
        references.spine = Anim.GetBoneTransform(HumanBodyBones.Spine);
        references.chest = Anim.GetBoneTransform(HumanBodyBones.Chest);
        references.neck = Anim.GetBoneTransform(HumanBodyBones.Neck);
        references.head = Anim.GetBoneTransform(HumanBodyBones.Head);

        references.LeftEye = Anim.GetBoneTransform(HumanBodyBones.LeftEye);
        references.RightEye = Anim.GetBoneTransform(HumanBodyBones.RightEye);

        references.leftShoulder = Anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
        references.leftUpperArm = Anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        references.leftLowerArm = Anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        references.leftHand = Anim.GetBoneTransform(HumanBodyBones.LeftHand);

        references.RightShoulder = Anim.GetBoneTransform(HumanBodyBones.RightShoulder);
        references.RightUpperArm = Anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        references.RightLowerArm = Anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        references.rightHand = Anim.GetBoneTransform(HumanBodyBones.RightHand);

        references.LeftUpperLeg = Anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        references.LeftLowerLeg = Anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        references.leftFoot = Anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        references.leftToes = Anim.GetBoneTransform(HumanBodyBones.LeftToes);

        references.RightUpperLeg = Anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        references.RightLowerLeg = Anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        references.rightFoot = Anim.GetBoneTransform(HumanBodyBones.RightFoot);
        references.rightToes = Anim.GetBoneTransform(HumanBodyBones.RightToes);

        return true;
    }
}