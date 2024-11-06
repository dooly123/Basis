using System;
using UnityEngine;

namespace Basis.Scripts.TransformBinders.BoneControl
{
    [System.Serializable]
    public struct BasisPositionControl
    {
        public bool HasTarget;
        public Vector3 Offset;
        public float LerpAmount;
        [NonSerialized]
        public BasisBoneControl Target;
    }
}