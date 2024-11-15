using System.Collections.Generic;
using UnityEngine;

namespace Basis.Scripts.Common
{
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
    public Transform Upperchest;
    public bool HasUpperchest;
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

    // Finger bones
    public Transform LeftThumbProximal;
    public bool HasLeftThumbProximal;
    public Transform LeftThumbIntermediate;
    public bool HasLeftThumbIntermediate;
    public Transform LeftThumbDistal;
    public bool HasLeftThumbDistal;

    public Transform LeftIndexProximal;
    public bool HasLeftIndexProximal;
    public Transform LeftIndexIntermediate;
    public bool HasLeftIndexIntermediate;
    public Transform LeftIndexDistal;
    public bool HasLeftIndexDistal;

    public Transform LeftMiddleProximal;
    public bool HasLeftMiddleProximal;
    public Transform LeftMiddleIntermediate;
    public bool HasLeftMiddleIntermediate;
    public Transform LeftMiddleDistal;
    public bool HasLeftMiddleDistal;

    public Transform LeftRingProximal;
    public bool HasLeftRingProximal;
    public Transform LeftRingIntermediate;
    public bool HasLeftRingIntermediate;
    public Transform LeftRingDistal;
    public bool HasLeftRingDistal;

    public Transform LeftLittleProximal;
    public bool HasLeftLittleProximal;
    public Transform LeftLittleIntermediate;
    public bool HasLeftLittleIntermediate;
    public Transform LeftLittleDistal;
    public bool HasLeftLittleDistal;

    public Transform RightThumbProximal;
    public bool HasRightThumbProximal;
    public Transform RightThumbIntermediate;
    public bool HasRightThumbIntermediate;
    public Transform RightThumbDistal;
    public bool HasRightThumbDistal;

    public Transform RightIndexProximal;
    public bool HasRightIndexProximal;
    public Transform RightIndexIntermediate;
    public bool HasRightIndexIntermediate;
    public Transform RightIndexDistal;
    public bool HasRightIndexDistal;

    public Transform RightMiddleProximal;
    public bool HasRightMiddleProximal;
    public Transform RightMiddleIntermediate;
    public bool HasRightMiddleIntermediate;
    public Transform RightMiddleDistal;
    public bool HasRightMiddleDistal;

    public Transform RightRingProximal;
    public bool HasRightRingProximal;
    public Transform RightRingIntermediate;
    public bool HasRightRingIntermediate;
    public Transform RightRingDistal;
    public bool HasRightRingDistal;

    public Transform RightLittleProximal;
    public bool HasRightLittleProximal;
    public Transform RightLittleIntermediate;
    public bool HasRightLittleIntermediate;
    public Transform RightLittleDistal;
    public bool HasRightLittleDistal;

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
        references.Upperchest = Anim.GetBoneTransform(HumanBodyBones.UpperChest);
        references.HasUpperchest = BoolState(references.Upperchest);

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

        // Left Hand Fingers
        references.LeftThumbProximal = Anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
        references.HasLeftThumbProximal = BoolState(references.LeftThumbProximal);
        references.LeftThumbIntermediate = Anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        references.HasLeftThumbIntermediate = BoolState(references.LeftThumbIntermediate);
        references.LeftThumbDistal = Anim.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
        references.HasLeftThumbDistal = BoolState(references.LeftThumbDistal);

        references.LeftIndexProximal = Anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
        references.HasLeftIndexProximal = BoolState(references.LeftIndexProximal);
        references.LeftIndexIntermediate = Anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
        references.HasLeftIndexIntermediate = BoolState(references.LeftIndexIntermediate);
        references.LeftIndexDistal = Anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        references.HasLeftIndexDistal = BoolState(references.LeftIndexDistal);

        references.LeftMiddleProximal = Anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        references.HasLeftMiddleProximal = BoolState(references.LeftMiddleProximal);
        references.LeftMiddleIntermediate = Anim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
        references.HasLeftMiddleIntermediate = BoolState(references.LeftMiddleIntermediate);
        references.LeftMiddleDistal = Anim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        references.HasLeftMiddleDistal = BoolState(references.LeftMiddleDistal);

        references.LeftRingProximal = Anim.GetBoneTransform(HumanBodyBones.LeftRingProximal);
        references.HasLeftRingProximal = BoolState(references.LeftRingProximal);
        references.LeftRingIntermediate = Anim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
        references.HasLeftRingIntermediate = BoolState(references.LeftRingIntermediate);
        references.LeftRingDistal = Anim.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        references.HasLeftRingDistal = BoolState(references.LeftRingDistal);

        references.LeftLittleProximal = Anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
        references.HasLeftLittleProximal = BoolState(references.LeftLittleProximal);
        references.LeftLittleIntermediate = Anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        references.HasLeftLittleIntermediate = BoolState(references.LeftLittleIntermediate);
        references.LeftLittleDistal = Anim.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        references.HasLeftLittleDistal = BoolState(references.LeftLittleDistal);

        // Right Hand Fingers
        references.RightThumbProximal = Anim.GetBoneTransform(HumanBodyBones.RightThumbProximal);
        references.HasRightThumbProximal = BoolState(references.RightThumbProximal);
        references.RightThumbIntermediate = Anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        references.HasRightThumbIntermediate = BoolState(references.RightThumbIntermediate);
        references.RightThumbDistal = Anim.GetBoneTransform(HumanBodyBones.RightThumbDistal);
        references.HasRightThumbDistal = BoolState(references.RightThumbDistal);

        references.RightIndexProximal = Anim.GetBoneTransform(HumanBodyBones.RightIndexProximal);
        references.HasRightIndexProximal = BoolState(references.RightIndexProximal);
        references.RightIndexIntermediate = Anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
        references.HasRightIndexIntermediate = BoolState(references.RightIndexIntermediate);
        references.RightIndexDistal = Anim.GetBoneTransform(HumanBodyBones.RightIndexDistal);
        references.HasRightIndexDistal = BoolState(references.RightIndexDistal);

        references.RightMiddleProximal = Anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        references.HasRightMiddleProximal = BoolState(references.RightMiddleProximal);
        references.RightMiddleIntermediate = Anim.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
        references.HasRightMiddleIntermediate = BoolState(references.RightMiddleIntermediate);
        references.RightMiddleDistal = Anim.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        references.HasRightMiddleDistal = BoolState(references.RightMiddleDistal);

        references.RightRingProximal = Anim.GetBoneTransform(HumanBodyBones.RightRingProximal);
        references.HasRightRingProximal = BoolState(references.RightRingProximal);
        references.RightRingIntermediate = Anim.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
        references.HasRightRingIntermediate = BoolState(references.RightRingIntermediate);
        references.RightRingDistal = Anim.GetBoneTransform(HumanBodyBones.RightRingDistal);
        references.HasRightRingDistal = BoolState(references.RightRingDistal);

        references.RightLittleProximal = Anim.GetBoneTransform(HumanBodyBones.RightLittleProximal);
        references.HasRightLittleProximal = BoolState(references.RightLittleProximal);
        references.RightLittleIntermediate = Anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        references.HasRightLittleIntermediate = BoolState(references.RightLittleIntermediate);
        references.RightLittleDistal = Anim.GetBoneTransform(HumanBodyBones.RightLittleDistal);
        references.HasRightLittleDistal = BoolState(references.RightLittleDistal);

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
            { "Upperchest", Upperchest },
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
            { "RightToes", rightToes },
            { "LeftThumbProximal", LeftThumbProximal },
            { "LeftThumbIntermediate", LeftThumbIntermediate },
            { "LeftThumbDistal", LeftThumbDistal },
            { "LeftIndexProximal", LeftIndexProximal },
            { "LeftIndexIntermediate", LeftIndexIntermediate },
            { "LeftIndexDistal", LeftIndexDistal },
            { "LeftMiddleProximal", LeftMiddleProximal },
            { "LeftMiddleIntermediate", LeftMiddleIntermediate },
            { "LeftMiddleDistal", LeftMiddleDistal },
            { "LeftRingProximal", LeftRingProximal },
            { "LeftRingIntermediate", LeftRingIntermediate },
            { "LeftRingDistal", LeftRingDistal },
            { "LeftLittleProximal", LeftLittleProximal },
            { "LeftLittleIntermediate", LeftLittleIntermediate },
            { "LeftLittleDistal", LeftLittleDistal },
            { "RightThumbProximal", RightThumbProximal },
            { "RightThumbIntermediate", RightThumbIntermediate },
            { "RightThumbDistal", RightThumbDistal },
            { "RightIndexProximal", RightIndexProximal },
            { "RightIndexIntermediate", RightIndexIntermediate },
            { "RightIndexDistal", RightIndexDistal },
            { "RightMiddleProximal", RightMiddleProximal },
            { "RightMiddleIntermediate", RightMiddleIntermediate },
            { "RightMiddleDistal", RightMiddleDistal },
            { "RightRingProximal", RightRingProximal },
            { "RightRingIntermediate", RightRingIntermediate },
            { "RightRingDistal", RightRingDistal },
            { "RightLittleProximal", RightLittleProximal },
            { "RightLittleIntermediate", RightLittleIntermediate },
            { "RightLittleDistal", RightLittleDistal }
        };

        return transforms;
    }
    }
}