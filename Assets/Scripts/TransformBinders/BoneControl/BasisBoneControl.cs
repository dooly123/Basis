#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
[System.Serializable]
public class BasisBoneControl
{
    [SerializeField]
    public string Name;
    [SerializeField]
    private Color gizmoColor = Color.blue;
    [SerializeField]
    public bool HasBone = false;
    [SerializeField]
    public Transform BoneTransform;
    [SerializeField]
    public Transform BoneModelTransform;
    // Events for property changes
    public event System.Action<BasisHasTracked> OnHasTrackerPositionDriverChanged;
    public event System.Action<BasisHasTracked> OnHasTrackerRotationDriverChanged;
    // Backing fields for the properties
    [SerializeField]
    private BasisHasTracked hasTrackerPositionDriver = BasisHasTracked.HasNoTracker;
    [SerializeField]
    private BasisHasTracked hasTrackerRotationDriver = BasisHasTracked.HasNoTracker;
    // Properties with get/set accessors
    public BasisHasTracked HasTrackerPositionDriver
    {
        get => hasTrackerPositionDriver;
        set
        {
            if (hasTrackerPositionDriver != value)
            {
               // Debug.Log("Setting Tracker To has Tracker Position Driver " + value);
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
               // Debug.Log("Setting Tracker To has Tracker Rotation Driver " + value);
                hasTrackerRotationDriver = value;
                OnHasTrackerRotationDriverChanged?.Invoke(value);
            }
        }
    }
    // Events for property changes
    public event System.Action<BasisHasRigLayer> OnHasRigChanged;
    // Backing fields for the properties
    [SerializeField]
    private BasisHasRigLayer hasRigLayer = BasisHasRigLayer.HasNoRigLayer;
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
    public BasisRotationalControl RotationControl = new BasisRotationalControl();
    [SerializeField]
    public BasisPositionControl PositionControl = new BasisPositionControl();
    [SerializeField]
    public BasisCalibratedOffsetData RawLocalData = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData LastRunData = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData RestingWorldSpace = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData RestingLocalSpace = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData InitialOffset = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData TrackerData = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData FinalisedWorldData = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData FinalisedLastWorldData = new BasisCalibratedOffsetData();
    public Color Color { get => gizmoColor; set => gizmoColor = value; }
    public void Initialize()
    {
        if (HasBone)
        {
            BoneTransform.GetLocalPositionAndRotation(out LastRunData.position, out LastRunData.rotation);
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
            if (RotationControl.UseAngle && AngleCheck(RawLocalData.rotation, RawLocalData.rotation, RotationControl.AngleBeforeMove))
            {
                if (RotationControl.HasActiveTimer)
                {
                    if (time > RotationControl.NextReset)
                    {
                        ApplyTargetRotation(ref RawLocalData.rotation, RotationControl);
                        QuaternionClamp(ref RawLocalData.rotation, RotationControl);
                        ApplyLerpToQuaternion(ref RawLocalData.rotation, RotationControl, (RotationControl.LerpAmountNormal / 2) * Time.deltaTime);
                        if (AngleCheck(RawLocalData.rotation, RotationControl.Target.RawLocalData.rotation))
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
            ApplyTargetPosition(ref RawLocalData.position, PositionControl);
            ApplyLerpToVector(ref RawLocalData.position, PositionControl);
        }
        if (HasTrackerRotationDriver == BasisHasTracked.HasTracker && HasTrackerPositionDriver == BasisHasTracked.HasTracker)
        {
            if (InitialOffset.Use)
            {
                // Update the position of the secondary transform to maintain the initial offset
                RawLocalData.position = TrackerData.position + TrackerData.rotation * InitialOffset.position;

                // Update the rotation of the secondary transform to maintain the initial offset
                RawLocalData.rotation = TrackerData.rotation * InitialOffset.rotation;
            }
            else
            {
                RawLocalData.rotation = TrackerData.rotation;
                RawLocalData.position = TrackerData.position;
            }
        }
    }
    public void SetOffset()
    {
        BoneModelTransform.SetLocalPositionAndRotation(Vector3.zero, RestingWorldSpace.rotation);
    }
    public void RunRotationChange()
    {
        ApplyTargetRotation(ref RawLocalData.rotation, RotationControl);
        QuaternionClamp(ref RawLocalData.rotation, RotationControl);
        ApplyLerpToQuaternion(ref RawLocalData.rotation, RotationControl);
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
        if (!HasBone)
        {
            return;
        }
        LastRunData.position = RawLocalData.position;
        LastRunData.rotation = RawLocalData.rotation;
        FinalisedLastWorldData.position = FinalisedWorldData.position;
        FinalisedLastWorldData.rotation = FinalisedWorldData.rotation;
        BoneTransform.SetLocalPositionAndRotation(RawLocalData.position, RawLocalData.rotation);
        BoneTransform.GetPositionAndRotation(out FinalisedWorldData.position, out FinalisedWorldData.rotation);
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
                position = positionLock.Target.RawLocalData.position + positionLock.Offset;
                break;

            case BasisTargetController.TargetDirectional:
                Vector3 customDirection = positionLock.Target.RawLocalData.rotation * positionLock.Offset;
                position = positionLock.Target.RawLocalData.position + customDirection;
                break;
        }
    }
    public void ApplyTargetRotation(ref Quaternion rotation, BasisRotationalControl AxisLock)
    {
        switch (AxisLock.TaretInterpreter)
        {
            case BasisTargetController.Target:
                rotation = AxisLock.Target.RawLocalData.rotation;
                break;

            case BasisTargetController.TargetDirectional:
                rotation = AxisLock.Target.RawLocalData.rotation * AxisLock.Offset;
                break;
        }
    }
#if UNITY_EDITOR
    public void DrawGizmos()
    {
        if (HasBone)
        {
            Gizmos.color = Color;
            Vector3 BonePosition = FinalisedWorldData.position;
            if (PositionControl.TaretInterpreter != BasisTargetController.None)
            {
                Gizmos.DrawLine(BonePosition, PositionControl.Target.FinalisedWorldData.position);
            }
            Gizmos.DrawWireSphere(BonePosition, 0.05f * BasisLocalPlayer.Instance.ScaledUpPlayerPositions);
            Handles.Label(BonePosition, Name);
        }
    }
#endif
    private void ApplyLerpToQuaternion(ref Quaternion quaternionRotation, BasisRotationalControl Rotation)
    {
        if (Rotation.Lerp != BasisAxisLerp.None)
        {
            float angleDifference = Quaternion.Angle(LastRunData.rotation, quaternionRotation);
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
                NewQuaternionRotation = Quaternion.Lerp(LastRunData.rotation, NewQuaternionRotation, lerpFactor);
                break;
            case BasisAxisLerp.SphericalLerp:
                NewQuaternionRotation = Quaternion.Slerp(LastRunData.rotation, NewQuaternionRotation, lerpFactor);
                break;
            case BasisAxisLerp.LerpUnclamped:
                NewQuaternionRotation = Quaternion.LerpUnclamped(LastRunData.rotation, NewQuaternionRotation, lerpFactor);
                break;
            case BasisAxisLerp.SphericalLerpUnclamped:
                NewQuaternionRotation = Quaternion.SlerpUnclamped(LastRunData.rotation, NewQuaternionRotation, lerpFactor);
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
                position = Vector3.Lerp(LastRunData.position, position, axis.LerpAmount * Time.deltaTime);
                break;
            case BasisVectorLerp.SphericalLerp:
                position = Vector3.Slerp(LastRunData.position, position, axis.LerpAmount * Time.deltaTime);
                break;
            case BasisVectorLerp.LerpUnclamped:
                position = Vector3.LerpUnclamped(LastRunData.position, position, axis.LerpAmount * Time.deltaTime);
                break;
            case BasisVectorLerp.SphericalLerpUnclamped:
                position = Vector3.SlerpUnclamped(LastRunData.position, position, axis.LerpAmount * Time.deltaTime);
                break;
        }
    }
}