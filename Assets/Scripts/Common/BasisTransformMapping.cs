using System.Collections.Generic;
using UnityEngine;

public class BasisTransformMapping
{
    public Transform AnimatorRoot;
    public bool HasAnimatorRoot;
    public Transform Hips;
    public bool HasHips;
    public Transform spine;
    public bool Hasspine;
    public Transform chest;
    public bool Haschest;
    public Transform neck;
    public bool Hasneck;
    public Transform head;
    public bool Hashead;
    public Transform LeftEye;
    public bool HasLeftEye;
    public Transform RightEye;
    public bool HasRightEye;

    public Transform leftShoulder;
    public bool HasleftShoulder;
    public Transform leftUpperArm;
    public bool HasleftUpperArm;
    public Transform leftLowerArm;
    public bool HasleftLowerArm;
    public Transform leftHand;
    public bool HasleftHand;

    public Transform RightShoulder;
    public bool HasRightShoulder;
    public Transform RightUpperArm;
    public bool HasRightUpperArm;
    public Transform RightLowerArm;
    public bool HasRightLowerArm;
    public Transform rightHand;
    public bool HasrightHand;
    public Transform LeftUpperLeg;
    public bool HasLeftUpperLeg;
    public Transform LeftLowerLeg;
    public bool HasLeftLowerLeg;
    public Transform leftFoot;
    public bool HasleftFoot;
    public Transform leftToes;
    public bool HasleftToes;

    public Transform RightUpperLeg;
    public bool HasRightUpperLeg;
    public Transform RightLowerLeg;
    public bool HasRightLowerLeg;
    public Transform rightFoot;
    public bool HasrightFoot;
    public Transform rightToes;
    public bool HasrightToes;

    public static bool AutoDetectReferences(Animator Anim, Transform AnimatorRoot, out BasisTransformMapping references)
    {
        references = new BasisTransformMapping();
        if (!Anim.isHuman)
        {
            Debug.LogError("We need a Humanoid Animator");
            return false;
        }

        references.AnimatorRoot = AnimatorRoot;
        references.HasAnimatorRoot = BoolState(references.AnimatorRoot);

        references.Hips = Anim.GetBoneTransform(HumanBodyBones.Hips);
        references.HasHips = BoolState(references.Hips);
        references.spine = Anim.GetBoneTransform(HumanBodyBones.Spine);
        references.Hasspine = BoolState(references.spine);
        references.chest = Anim.GetBoneTransform(HumanBodyBones.Chest);
        references.Haschest = BoolState(references.chest);
        references.neck = Anim.GetBoneTransform(HumanBodyBones.Neck);
        references.Hasneck = BoolState(references.neck);
        references.head = Anim.GetBoneTransform(HumanBodyBones.Head);
        references.Hashead = BoolState(references.head);

        references.LeftEye = Anim.GetBoneTransform(HumanBodyBones.LeftEye);
        references.HasLeftEye = BoolState(references.LeftEye);
        references.RightEye = Anim.GetBoneTransform(HumanBodyBones.RightEye);
        references.HasRightEye = BoolState(references.RightEye);

        references.leftShoulder = Anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
        references.HasleftShoulder = BoolState(references.leftShoulder);
        references.leftUpperArm = Anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        references.HasleftUpperArm = BoolState(references.leftUpperArm);
        references.leftLowerArm = Anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        references.HasleftLowerArm = BoolState(references.leftLowerArm);
        references.leftHand = Anim.GetBoneTransform(HumanBodyBones.LeftHand);
        references.HasleftHand = BoolState(references.leftHand);

        references.RightShoulder = Anim.GetBoneTransform(HumanBodyBones.RightShoulder);
        references.HasRightShoulder = BoolState(references.RightShoulder);
        references.RightUpperArm = Anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        references.HasRightUpperArm = BoolState(references.RightUpperArm);
        references.RightLowerArm = Anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        references.HasRightLowerArm = BoolState(references.RightLowerArm);
        references.rightHand = Anim.GetBoneTransform(HumanBodyBones.RightHand);
        references.HasrightHand = BoolState(references.rightHand);

        references.LeftUpperLeg = Anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        references.HasLeftUpperLeg = BoolState(references.LeftUpperLeg);
        references.LeftLowerLeg = Anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        references.HasLeftLowerLeg = BoolState(references.LeftLowerLeg);
        references.leftFoot = Anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        references.HasleftFoot = BoolState(references.leftFoot);
        references.leftToes = Anim.GetBoneTransform(HumanBodyBones.LeftToes);
        references.HasleftToes = BoolState(references.leftToes);

        references.RightUpperLeg = Anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        references.HasRightUpperLeg = BoolState(references.RightUpperLeg);
        references.RightLowerLeg = Anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        references.HasRightLowerLeg = BoolState(references.RightLowerLeg);
        references.rightFoot = Anim.GetBoneTransform(HumanBodyBones.RightFoot);
        references.HasrightFoot = BoolState(references.rightFoot);
        references.rightToes = Anim.GetBoneTransform(HumanBodyBones.RightToes);
        references.HasrightToes = BoolState(references.rightToes);

        return true;
    }

    public static bool BoolState(Transform Transform)
    {
        return Transform != null;
    }
    public Dictionary<string, Transform> GetAllTransforms()
    {
        Dictionary<string, Transform> transforms = new Dictionary<string, Transform>
        {
            { "AnimatorRoot", AnimatorRoot },
            { "Hips", Hips },
            { "Spine", spine },
            { "Chest", chest },
            { "Neck", neck },
            { "Head", head },
            { "LeftEye", LeftEye },
            { "RightEye", RightEye },
            { "LeftShoulder", leftShoulder },
            { "LeftUpperArm", leftUpperArm },
            { "LeftLowerArm", leftLowerArm },
            { "LeftHand", leftHand },
            { "RightShoulder", RightShoulder },
            { "RightUpperArm", RightUpperArm },
            { "RightLowerArm", RightLowerArm },
            { "RightHand", rightHand },
            { "LeftUpperLeg", LeftUpperLeg },
            { "LeftLowerLeg", LeftLowerLeg },
            { "LeftFoot", leftFoot },
            { "LeftToes", leftToes },
            { "RightUpperLeg", RightUpperLeg },
            { "RightLowerLeg", RightLowerLeg },
            { "RightFoot", rightFoot },
            { "RightToes", rightToes }
        };
        return transforms;
    }
}