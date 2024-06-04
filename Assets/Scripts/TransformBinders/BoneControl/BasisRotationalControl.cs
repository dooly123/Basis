using System;
using UnityEngine;
[System.Serializable]
public partial struct BasisRotationalControl
{
    public BasisAxisLerp Lerp;
    public float LerpAmountNormal;
    public float LerpAmountFastMovement;
    public float AngleBeforeSpeedup;
    public BasisTargetController TaretInterpreter;
    public Quaternion Offset;
    public BasisClampData ClampStats;
    public BasisClampAxis ClampableAxis;
    public float ClampSize;
    public bool UseAngle;

    public float AngleBeforeMove;
    public float AngleBeforeSame;
    public double ResetAfterTime;
    public double NextReset;
    public bool HasActiveTimer;
    public bool RotationOverride;
    [NonSerialized]
    public BasisBoneControl Target;
}