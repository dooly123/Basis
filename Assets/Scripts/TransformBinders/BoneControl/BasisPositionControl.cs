using System;
using UnityEngine;

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