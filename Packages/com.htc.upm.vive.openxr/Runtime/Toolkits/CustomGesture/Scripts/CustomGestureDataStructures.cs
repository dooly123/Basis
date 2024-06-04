using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VIVE.OpenXR.Toolkits.CustomGesture
{
    [System.Serializable]
    public class CustomGesture
    {
        public string GestureName = "NewGesture";
        public CGEnums.HandFlag TargetHand;

        [HideInInspector] public CGEnums.FingerStatus ThumbStatus;
        [HideInInspector] public CGEnums.FingerStatus IndexStatus;
        [HideInInspector] public CGEnums.FingerStatus MiddleStatus;
        [HideInInspector] public CGEnums.FingerStatus RingStatus;
        [HideInInspector] public CGEnums.FingerStatus PinkyStatus;


        public TargetFingerStatus ThumbStatusIs;
        public TargetFingerStatus IndexStatusIs;
        public TargetFingerStatus MiddleStatusIs;
        public TargetFingerStatus RingStatusIs;
        public TargetFingerStatus PinkyStatusIs;

        public JointDistance DualHandDistance;
        [Range(20, 0)]
        public float DualNear = 0;
        [Range(20, 0)]
        public float DualFar = 0;
        public JointDistance ThumbIndexDistance;
        [Range(10, 0)]
        public float SingleNear = 0;
        [Range(10, 0)]
        public float SingleFar = 0;
    }
    public enum JointDistance
    {
        DontCare = 0,
        Near = 1,
        Far = 2,
    }

    public class HandJoint
    {
        /// <summary>
        /// Tells whether the data of this <see cref = "HandJoint">HandJoints</see> is valid or not; the data shouldn't be used if <c>isValid</c> returns false
        /// </summary>
        public bool isValid;
        /// <summary>
        /// Holds the position of the <see cref = "HandJoint">HandJoints</see>
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// Holds the rotation of the <see cref = "HandJoint">HandJoints</see>
        /// </summary>
        public Quaternion rotation;
        public HandJoint()
        {
            isValid = false;
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }
    }
    public class FingerStatusExpresstion
    {
        public bool Is = true;
        public CGEnums.FingerStatus Status = CGEnums.FingerStatus.None; //Straight;
    }

    public enum TargetFingerStatus
    {
        DontCare = 0,
        Straight = 1,
        Bending = 2,
        Bended = 3,
        NotStraight = 4,
        NotBending = 5,
        NotBended = 6,
    }

    static class EnumExtensions
    {
        public static FingerStatusExpresstion ToExpresstion(this TargetFingerStatus _Status)
        {
            FingerStatusExpresstion _Expresstion = new FingerStatusExpresstion();
            switch (_Status)
            {
                case TargetFingerStatus.Straight:
                    _Expresstion.Is = true;
                    _Expresstion.Status = CGEnums.FingerStatus.Straight;
                    break;
                case TargetFingerStatus.Bending:
                    _Expresstion.Is = true;
                    _Expresstion.Status = CGEnums.FingerStatus.Bending;
                    break;
                case TargetFingerStatus.Bended:
                    _Expresstion.Is = true;
                    _Expresstion.Status = CGEnums.FingerStatus.Bended;
                    break;
                case TargetFingerStatus.NotStraight: //using
                    _Expresstion.Is = false;
                    _Expresstion.Status = CGEnums.FingerStatus.Straight;
                    break;
                case TargetFingerStatus.NotBending:
                    _Expresstion.Is = false;
                    _Expresstion.Status = CGEnums.FingerStatus.Bending;
                    break;
                case TargetFingerStatus.NotBended: //using
                    _Expresstion.Is = false;
                    _Expresstion.Status = CGEnums.FingerStatus.Bended;
                    break;
                case TargetFingerStatus.DontCare:
                    _Expresstion.Is = false;//true;
                    _Expresstion.Status = CGEnums.FingerStatus.None;
                    break;
            }
            return _Expresstion;
        }
    }

    [System.Serializable]
    public class FingerStatusDefiner
    {
        [Range(180, 0)]
        public float StraightDistalLowBound = 160;
        [Range(180, 0)]
        public float StraightIntermediateLowBound = 160;
        [Range(180, 0)]
        public float StraightProximalLowBound = 160;

        [Range(180, 0)]
        public float BendingDistalLowBound = 120;
        [Range(180, 0)]
        public float BendingIntermediateLowBound = 120;
        [Range(180, 0)]
        public float BendingProximalLowBound = 120;

        [System.Serializable]
        public class AngleRange
        {
            [Range(180, 0)]
            public float LowBound = 0;
            [Range(180, 0)]
            public float HeighBound = 180;
            public AngleRange() { }
            public AngleRange(float _Heigh, float _Low)
            {
                HeighBound = _Heigh;
                LowBound = _Low;
            }
        }
    }
}
