using Assets.Scripts.Common.Enums;
using System;
using UnityEngine;

namespace Assets.Scripts.TransformBinders.BoneControl
{
[System.Serializable]
public struct BasisPositionControl
{
    public Vector3 Offset;
    public float LerpAmount;
    public BasisVectorLerp Lerp;
    public BasisTargetController TaretInterpreter;
    [NonSerialized]
    public BasisBoneControl Target;
}
}