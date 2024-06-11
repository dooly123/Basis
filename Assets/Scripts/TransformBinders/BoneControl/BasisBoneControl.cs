using UnityEditor;
using UnityEngine;
[System.Serializable]
public class BasisBoneControl
{
    public string Name;
    public bool HasBone = false;
    // Events for property changes
    public event System.Action<BasisHasTracked> OnHasTrackerPositionDriverChanged;
    public event System.Action<BasisHasTracked> OnHasTrackerRotationDriverChanged;
    // Backing fields for the properties
    [SerializeField] private BasisHasTracked hasTrackerPositionDriver = BasisHasTracked.HasNoTracker;
    [SerializeField] private BasisHasTracked hasTrackerRotationDriver = BasisHasTracked.HasNoTracker;
    // Properties with get/set accessors
    public BasisHasTracked HasTrackerPositionDriver
    {
        get => hasTrackerPositionDriver;
        set
        {
            if (hasTrackerPositionDriver != value)
            {
                hasTrackerPositionDriver = value;
                OnHasTrackerPositionDriverChanged?.Invoke(value);
            }
        }
    }
    public BasisHasTracked HasTrackerRotationDriver
    {
        get => hasTrackerRotationDriver;
        set
        {
            if (hasTrackerRotationDriver != value)
            {
                hasTrackerRotationDriver = value;
                OnHasTrackerRotationDriverChanged?.Invoke(value);
            }
        }
    }
    // Events for property changes
    public event System.Action<BasisHasRigLayer> OnHasRigChanged;
    // Backing fields for the properties
    [SerializeField] private BasisHasRigLayer hasRigLayer = BasisHasRigLayer.HasNoRigLayer;
    // Properties with get/set accessors
    public BasisHasRigLayer HasRigLayer
    {
        get => hasRigLayer;
        set
        {
            if (hasRigLayer != value)
            {
                hasRigLayer = value;
                OnHasRigChanged?.Invoke(value);
            }
        }
    }
    [SerializeField]
    public BasisRotationalControl RotationControl;
    [SerializeField]
    public BasisPositionControl PositionControl;
    [SerializeField]
    public Transform BoneTransform;
    [SerializeField]
    public Vector3 LocalRawPosition;
    [SerializeField]
    public Vector3 WorldSpaceFloor;
    [SerializeField]
    public Quaternion LocalRawRotation;
    [SerializeField]
    public Vector3 RestingWorldSpacePosition;
    [SerializeField]
    public Quaternion RestingWorldSpaceRotation;
    [SerializeField]
    public Vector3 LastBonePosition;
    [SerializeField]
    public Quaternion LastBoneRotation;
    private Color gizmoColor = Color.blue;
    [SerializeField]
    public BasisCalibratedRestingData RestingLocalSpace = new BasisCalibratedRestingData();
    public Color Color { get => gizmoColor; set => gizmoColor = value; }
    public void Initialize()
    {
        if (HasBone)
        {
            BoneTransform.GetLocalPositionAndRotation(out LastBonePosition, out LastBoneRotation);
        }
    }
    /// <summary>
    /// Compute a rotation and position in stages
    /// 1. get rotation
    /// 2. 
    /// </summary>
    /// <param name="time"></param>
    public void ComputeMovement(double time)
    {
        if (!HasBone)
        {
            return;
        }

        if (HasTrackerRotationDriver == BasisHasTracked.HasNoTracker)
        {
            //if angle is larger then 4 then lets then lets begin checking to see if we can snap it back
            if (RotationControl.UseAngle && AngleCheck(LocalRawRotation, RotationControl.Target.LocalRawRotation, RotationControl.AngleBeforeMove))
            {
                if (RotationControl.HasActiveTimer)
                {
                    if (time > RotationControl.NextReset)
                    {
                        ApplyTargetRotation(ref LocalRawRotation, RotationControl);
                        QuaternionClamp(ref LocalRawRotation, RotationControl);
                        ApplyLerpToQuaternion(ref LocalRawRotation, RotationControl, (RotationControl.LerpAmountNormal / 2) * Time.deltaTime);
                        if (AngleCheck(LocalRawRotation, RotationControl.Target.LocalRawRotation))
                        {
                            RotationControl.NextReset = double.MaxValue;
                            RotationControl.HasActiveTimer = false;
                        }
                    }
                    else
                    {
                        RunRotationChange();
                    }
                }
                else
                {
                    RotationControl.NextReset = time + RotationControl.ResetAfterTime;
                    RotationControl.HasActiveTimer = true;
                }
            }
            else
            {
                //normal
                RunRotationChange();
            }
        }

        if (HasTrackerPositionDriver == BasisHasTracked.HasNoTracker)
        {
            ApplyTargetPosition(ref LocalRawPosition, PositionControl);
            ApplyLerpToVector(ref LocalRawPosition, PositionControl);
        }
    }
    public void RunRotationChange()
    {
        ApplyTargetRotation(ref LocalRawRotation, RotationControl);
        QuaternionClamp(ref LocalRawRotation, RotationControl);
        ApplyLerpToQuaternion(ref LocalRawRotation, RotationControl);
    }
    public bool HasNoAngleChange(Quaternion AngleA, Quaternion AngleB, float MaximumTolerance = 0.005f)
    {
        float Angle = Quaternion.Angle(AngleA, AngleB);
        bool AngleLargeEnough = Angle < MaximumTolerance;
        return AngleLargeEnough;
    }
    public bool AngleCheck(Quaternion AngleA, Quaternion AngleB, float MaximumTolerance = 0.005f)
    {
        float Angle = Quaternion.Angle(AngleA, AngleB);
        bool AngleLargeEnough = Angle > MaximumTolerance;
        return AngleLargeEnough;
    }
    public void ApplyMovement()
    {
        LastBonePosition = LocalRawPosition;
        LastBoneRotation = LocalRawRotation;
        BoneTransform.SetLocalPositionAndRotation(LocalRawPosition, LocalRawRotation);
    }
    public void QuaternionClamp(ref Quaternion rotation, BasisRotationalControl AxisLock)
    {
        if (AxisLock.ClampStats != BasisClampData.Clamp)
        {
            return;
        }
        Vector3 clampedEulerAngles = rotation.eulerAngles;
        if (AxisLock.ClampableAxis == BasisClampAxis.x)
        {
            clampedEulerAngles.x = Mathf.Clamp(clampedEulerAngles.x, clampedEulerAngles.x - AxisLock.ClampSize, AxisLock.ClampSize);
        }
        else if (AxisLock.ClampableAxis == BasisClampAxis.y)
        {
            clampedEulerAngles.y = Mathf.Clamp(clampedEulerAngles.y, clampedEulerAngles.y - AxisLock.ClampSize, AxisLock.ClampSize);
        }
        else if (AxisLock.ClampableAxis == BasisClampAxis.z)
        {
            clampedEulerAngles.z = Mathf.Clamp(clampedEulerAngles.z, clampedEulerAngles.z - AxisLock.ClampSize, AxisLock.ClampSize);
        }
        else if (AxisLock.ClampableAxis == BasisClampAxis.xz)
        {
            clampedEulerAngles.z = Mathf.Clamp(clampedEulerAngles.z, clampedEulerAngles.z - AxisLock.ClampSize, AxisLock.ClampSize);
            clampedEulerAngles.x = Mathf.Clamp(clampedEulerAngles.x, clampedEulerAngles.x - AxisLock.ClampSize, AxisLock.ClampSize);
        }
        rotation = Quaternion.Euler(clampedEulerAngles);
    }
    public void ApplyTargetPosition(ref Vector3 position, BasisPositionControl positionLock)
    {
        switch (positionLock.TaretInterpreter)
        {
            case BasisTargetController.Target:
                position = positionLock.Target.LocalRawPosition + positionLock.Offset;
                break;

            case BasisTargetController.TargetDirectional:
                Vector3 customDirection = positionLock.Target.LocalRawRotation * positionLock.Offset;
                position = positionLock.Target.LocalRawPosition + customDirection;
                break;
        }
    }
    public void ApplyTargetRotation(ref Quaternion rotation, BasisRotationalControl AxisLock)
    {
        switch (AxisLock.TaretInterpreter)
        {
            case BasisTargetController.Target:
                rotation = AxisLock.Target.LocalRawRotation;
                break;

            case BasisTargetController.TargetDirectional:
                rotation = AxisLock.Target.LocalRawRotation * AxisLock.Offset;
                break;
        }
    }
#if UNITY_EDITOR
    public void DrawGizmos()
    {
        if (HasBone)
        {
            Gizmos.color = Color;
            Vector3 BonePosition = BoneTransform.position;
            if (PositionControl.TaretInterpreter != BasisTargetController.None)
            {
                Gizmos.DrawLine(BonePosition, PositionControl.Target.BoneTransform.position);
            }
            Gizmos.DrawWireSphere(BonePosition, 0.05f);
            Handles.Label(BonePosition, Name);
        }
    }
#endif
    private void ApplyLerpToQuaternion(ref Quaternion quaternionRotation, BasisRotationalControl Rotation)
    {
        if (Rotation.Lerp != BasisAxisLerp.None)
        {
            float angleDifference = Quaternion.Angle(LastBoneRotation, quaternionRotation);
            float Timing = Mathf.Clamp01(angleDifference / Rotation.AngleBeforeSpeedup);
            float lerpAmount = Mathf.Lerp(Rotation.LerpAmountNormal, Rotation.LerpAmountFastMovement, Timing);

            float lerpFactor = lerpAmount * Time.deltaTime;
            ApplyLerpToQuaternion(ref quaternionRotation, Rotation, lerpFactor);
        }
    }
    private void ApplyLerpToQuaternion(ref Quaternion NewQuaternionRotation, BasisRotationalControl Rotation, float lerpFactor)
    {

        switch (Rotation.Lerp)
        {
            case BasisAxisLerp.Lerp:
                NewQuaternionRotation = Quaternion.Lerp(LastBoneRotation, NewQuaternionRotation, lerpFactor);
                break;
            case BasisAxisLerp.SphericalLerp:
                NewQuaternionRotation = Quaternion.Slerp(LastBoneRotation, NewQuaternionRotation, lerpFactor);
                break;
            case BasisAxisLerp.LerpUnclamped:
                NewQuaternionRotation = Quaternion.LerpUnclamped(LastBoneRotation, NewQuaternionRotation, lerpFactor);
                break;
            case BasisAxisLerp.SphericalLerpUnclamped:
                NewQuaternionRotation = Quaternion.SlerpUnclamped(LastBoneRotation, NewQuaternionRotation, lerpFactor);
                break;
        }
    }
    private void ApplyLerpToVector(ref Vector3 position, BasisPositionControl axis)
    {
        switch (axis.Lerp)
        {
            case BasisVectorLerp.None:
                break;
            case BasisVectorLerp.Lerp:
                position = Vector3.Lerp(LastBonePosition, position, axis.LerpAmount * Time.deltaTime);
                break;
            case BasisVectorLerp.SphericalLerp:
                position = Vector3.Slerp(LastBonePosition, position, axis.LerpAmount * Time.deltaTime);
                break;
            case BasisVectorLerp.LerpUnclamped:
                position = Vector3.LerpUnclamped(LastBonePosition, position, axis.LerpAmount * Time.deltaTime);
                break;
            case BasisVectorLerp.SphericalLerpUnclamped:
                position = Vector3.SlerpUnclamped(LastBonePosition, position, axis.LerpAmount * Time.deltaTime);
                break;
        }
    }
}