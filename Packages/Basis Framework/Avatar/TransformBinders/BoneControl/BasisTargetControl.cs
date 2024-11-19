using System;
using Unity.Mathematics;
namespace Basis.Scripts.TransformBinders.BoneControl
{
    [System.Serializable]
    public struct BasisTargetControl
    {
        public float LerpAmountNormal;
        public float LerpAmountFastMovement;
        public float AngleBeforeSpeedup;
        public bool HasRotationalTarget;
        [NonSerialized]
        public BasisBoneControl Target;

        public bool HasLineDraw;
        public int LineDrawIndex;
        public bool HasTarget;
        public float3 Offset;
        public float LerpAmount;
    }
}