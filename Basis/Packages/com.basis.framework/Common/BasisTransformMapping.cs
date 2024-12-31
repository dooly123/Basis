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
        public Transform[] LeftThumb = new Transform[3];
        public bool[] HasLeftThumb = new bool[3];

        public Transform[] LeftIndex = new Transform[3];
        public bool[] HasLeftIndex = new bool[3];

        public Transform[] LeftMiddle = new Transform[3];
        public bool[] HasLeftMiddle = new bool[3];

        public Transform[] LeftRing = new Transform[3];
        public bool[] HasLeftRing = new bool[3];

        public Transform[] LeftLittle = new Transform[3];
        public bool[] HasLeftLittle = new bool[3];

        public Transform[] RightThumb = new Transform[3];
        public bool[] HasRightThumb = new bool[3];

        public Transform[] RightIndex = new Transform[3];
        public bool[] HasRightIndex = new bool[3];

        public Transform[] RightMiddle = new Transform[3];
        public bool[] HasRightMiddle = new bool[3];

        public Transform[] RightRing = new Transform[3];
        public bool[] HasRightRing = new bool[3];

        public Transform[] RightLittle = new Transform[3];
        public bool[] HasRightLittle = new bool[3];

        public static bool AutoDetectReferences(Animator anim, Transform AnimatorRoot, out BasisTransformMapping references)
        {
            references = new BasisTransformMapping();
            if (!anim.isHuman)
            {
                BasisDebug.LogError("We need a Humanoid Animator");
                return false;
            }

            references.AnimatorRoot = AnimatorRoot;
            references.HasAnimatorRoot = BoolState(references.AnimatorRoot);

            references.Hips = anim.GetBoneTransform(HumanBodyBones.Hips);
            references.HasHips = BoolState(references.Hips);

            references.spine = anim.GetBoneTransform(HumanBodyBones.Spine);
            references.Hasspine = BoolState(references.spine);

            references.chest = anim.GetBoneTransform(HumanBodyBones.Chest);
            references.Haschest = BoolState(references.chest);

            references.Upperchest = anim.GetBoneTransform(HumanBodyBones.UpperChest);
            references.HasUpperchest = BoolState(references.Upperchest);

            references.neck = anim.GetBoneTransform(HumanBodyBones.Neck);
            references.Hasneck = BoolState(references.neck);
            references.head = anim.GetBoneTransform(HumanBodyBones.Head);
            references.Hashead = BoolState(references.head);

            references.LeftEye = anim.GetBoneTransform(HumanBodyBones.LeftEye);
            references.HasLeftEye = BoolState(references.LeftEye);
            references.RightEye = anim.GetBoneTransform(HumanBodyBones.RightEye);
            references.HasRightEye = BoolState(references.RightEye);

            references.leftShoulder = anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
            references.HasleftShoulder = BoolState(references.leftShoulder);
            references.leftUpperArm = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            references.HasleftUpperArm = BoolState(references.leftUpperArm);
            references.leftLowerArm = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            references.HasleftLowerArm = BoolState(references.leftLowerArm);
            references.leftHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
            references.HasleftHand = BoolState(references.leftHand);

            references.RightShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
            references.HasRightShoulder = BoolState(references.RightShoulder);
            references.RightUpperArm = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
            references.HasRightUpperArm = BoolState(references.RightUpperArm);
            references.RightLowerArm = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
            references.HasRightLowerArm = BoolState(references.RightLowerArm);
            references.rightHand = anim.GetBoneTransform(HumanBodyBones.RightHand);
            references.HasrightHand = BoolState(references.rightHand);

            references.LeftUpperLeg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            references.HasLeftUpperLeg = BoolState(references.LeftUpperLeg);
            references.LeftLowerLeg = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            references.HasLeftLowerLeg = BoolState(references.LeftLowerLeg);
            references.leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            references.HasleftFoot = BoolState(references.leftFoot);
            references.leftToes = anim.GetBoneTransform(HumanBodyBones.LeftToes);
            references.HasleftToes = BoolState(references.leftToes);

            references.RightUpperLeg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            references.HasRightUpperLeg = BoolState(references.RightUpperLeg);
            references.RightLowerLeg = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            references.HasRightLowerLeg = BoolState(references.RightLowerLeg);
            references.rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
            references.HasrightFoot = BoolState(references.rightFoot);
            references.rightToes = anim.GetBoneTransform(HumanBodyBones.RightToes);
            references.HasrightToes = BoolState(references.rightToes);

            references.LeftThumb[0] = anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
            references.HasLeftThumb[0] = BoolState(references.LeftThumb[0]);
            references.LeftThumb[1] = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
            references.HasLeftThumb[1] = BoolState(references.LeftThumb[1]);
            references.LeftThumb[2] = anim.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
            references.HasLeftThumb[2] = BoolState(references.LeftThumb[2]);

            references.LeftIndex[0] = anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
            references.HasLeftIndex[0] = BoolState(references.LeftIndex[0]);
            references.LeftIndex[1] = anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
            references.HasLeftIndex[1] = BoolState(references.LeftIndex[1]);
            references.LeftIndex[2] = anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
            references.HasLeftIndex[2] = BoolState(references.LeftIndex[2]);

            references.LeftMiddle[0] = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
            references.HasLeftMiddle[0] = BoolState(references.LeftMiddle[0]);
            references.LeftMiddle[1] = anim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
            references.HasLeftMiddle[1] = BoolState(references.LeftMiddle[1]);
            references.LeftMiddle[2] = anim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
            references.HasLeftMiddle[2] = BoolState(references.LeftMiddle[2]);

            references.LeftRing[0] = anim.GetBoneTransform(HumanBodyBones.LeftRingProximal);
            references.HasLeftRing[0] = BoolState(references.LeftRing[0]);
            references.LeftRing[1] = anim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
            references.HasLeftRing[1] = BoolState(references.LeftRing[1]);
            references.LeftRing[2] = anim.GetBoneTransform(HumanBodyBones.LeftRingDistal);
            references.HasLeftRing[2] = BoolState(references.LeftRing[2]);

            references.LeftLittle[0] = anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
            references.HasLeftLittle[0] = BoolState(references.LeftLittle[0]);
            references.LeftLittle[1] = anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
            references.HasLeftLittle[1] = BoolState(references.LeftLittle[1]);
            references.LeftLittle[2] = anim.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
            references.HasLeftLittle[2] = BoolState(references.LeftLittle[2]);

            // Right Hand
            references.RightThumb[0] = anim.GetBoneTransform(HumanBodyBones.RightThumbProximal);
            references.HasRightThumb[0] = BoolState(references.RightThumb[0]);
            references.RightThumb[1] = anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
            references.HasRightThumb[1] = BoolState(references.RightThumb[1]);
            references.RightThumb[2] = anim.GetBoneTransform(HumanBodyBones.RightThumbDistal);
            references.HasRightThumb[2] = BoolState(references.RightThumb[2]);

            references.RightIndex[0] = anim.GetBoneTransform(HumanBodyBones.RightIndexProximal);
            references.HasRightIndex[0] = BoolState(references.RightIndex[0]);
            references.RightIndex[1] = anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
            references.HasRightIndex[1] = BoolState(references.RightIndex[1]);
            references.RightIndex[2] = anim.GetBoneTransform(HumanBodyBones.RightIndexDistal);
            references.HasRightIndex[2] = BoolState(references.RightIndex[2]);

            references.RightMiddle[0] = anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            references.HasRightMiddle[0] = BoolState(references.RightMiddle[0]);
            references.RightMiddle[1] = anim.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
            references.HasRightMiddle[1] = BoolState(references.RightMiddle[1]);
            references.RightMiddle[2] = anim.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
            references.HasRightMiddle[2] = BoolState(references.RightMiddle[2]);

            references.RightRing[0] = anim.GetBoneTransform(HumanBodyBones.RightRingProximal);
            references.HasRightRing[0] = BoolState(references.RightRing[0]);
            references.RightRing[1] = anim.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
            references.HasRightRing[1] = BoolState(references.RightRing[1]);
            references.RightRing[2] = anim.GetBoneTransform(HumanBodyBones.RightRingDistal);
            references.HasRightRing[2] = BoolState(references.RightRing[2]);

            references.RightLittle[0] = anim.GetBoneTransform(HumanBodyBones.RightLittleProximal);
            references.HasRightLittle[0] = BoolState(references.RightLittle[0]);
            references.RightLittle[1] = anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
            references.HasRightLittle[1] = BoolState(references.RightLittle[1]);
            references.RightLittle[2] = anim.GetBoneTransform(HumanBodyBones.RightLittleDistal);
            references.HasRightLittle[2] = BoolState(references.RightLittle[2]);

            return true;
        }

        public static bool BoolState(Transform Transform)
        {
            return Transform != null;
        }
    }
}