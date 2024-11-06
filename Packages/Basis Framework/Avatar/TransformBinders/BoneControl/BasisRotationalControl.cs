using System;
namespace Basis.Scripts.TransformBinders.BoneControl
{
    [System.Serializable]
    public struct BasisRotationalControl
    {
        public float LerpAmountNormal;
        public float LerpAmountFastMovement;
        public float AngleBeforeSpeedup;
        public bool HasTarget;
        [NonSerialized]
        public BasisBoneControl Target;
    }
}