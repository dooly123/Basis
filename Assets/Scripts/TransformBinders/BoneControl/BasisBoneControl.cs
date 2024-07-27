#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using static BasisAvatarIKStageCalibration;
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
    public bool HasEvents = false;
    // Events for property changes
    public System.Action<BasisHasTracked> OnHasTrackerDriverChanged;
    // Backing fields for the properties
    [SerializeField]
    private BasisHasTracked hasTrackerDriver = BasisHasTracked.HasNoTracker;
    // Properties with get/set accessors
    public BasisHasTracked HasTracked
    {
        get => hasTrackerDriver;
        set
        {
            if (hasTrackerDriver != value)
            {
               // Debug.Log("Setting Tracker To has Tracker Position Driver " + value);
                hasTrackerDriver = value;
                OnHasTrackerDriverChanged?.Invoke(value);
            }
        }
    }
    // Events for property changes
    public UnityEvent OnHasRigChanged = new UnityEvent();

    public UnityEvent<float, float> WeightsChanged = new UnityEvent<float,float>();
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
                OnHasRigChanged.Invoke();
            }
        }
    }
    [SerializeField]
    private float positionWeight = 1;
    [SerializeField]
    private float rotationWeight = 1;
    public float PositionWeight
    {
        get => positionWeight;
        set
        {
            if (positionWeight != value)
            {
                positionWeight = value;
                WeightsChanged.Invoke(positionWeight, rotationWeight);
            }
        }
    }
    public float RotationWeight
    {
        get => rotationWeight;
        set
        {
            if (rotationWeight != value)
            {
                rotationWeight = value;
                WeightsChanged.Invoke(positionWeight, rotationWeight);
            }
        }
    }

    public GeneralLocation GeneralLocation;
    [SerializeField]
    public BasisRotationalControl RotationControl = new BasisRotationalControl();
    [SerializeField]
    public BasisPositionControl PositionControl = new BasisPositionControl();
    [SerializeField]
    public BasisCalibratedOffsetData OutGoingData = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData LastRunData = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData TposeWorld = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData TposeLocal = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData InverseOffsetFromBone = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData IncomingData = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData OutgoingWorldData = new BasisCalibratedOffsetData();
    [SerializeField]
    public BasisCalibratedOffsetData LastWorldData = new BasisCalibratedOffsetData();
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
    public void ComputeMovement(double time, float DeltaTime)
    {
        if (!HasBone)
        {
            return;
        }

        if (HasTracked == BasisHasTracked.HasNoTracker)
        {
            //if angle is larger then 4 then lets then lets begin checking to see if we can snap it back
            if (RotationControl.UseAngle && AngleCheck(OutGoingData.rotation, OutGoingData.rotation, RotationControl.AngleBeforeMove))
            {
                if (RotationControl.HasActiveTimer)
                {
                    if (time > RotationControl.NextReset)
                    {
                        ApplyTargetRotation(ref OutGoingData.rotation, RotationControl);
                        QuaternionClamp(ref OutGoingData.rotation, RotationControl);
                        ApplyLerpToQuaternion(ref OutGoingData.rotation, RotationControl, (RotationControl.LerpAmountNormal / 2) * DeltaTime);
                        if (AngleCheck(OutGoingData.rotation, RotationControl.Target.OutGoingData.rotation))
                        {
                            RotationControl.NextReset = double.MaxValue;
                            RotationControl.HasActiveTimer = false;
                        }
                    }
                    else
                    {
                        RunRotationChange(DeltaTime);
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
                RunRotationChange(DeltaTime);
            }
            ApplyTargetPosition(ref OutGoingData.position, PositionControl);
            ApplyLerpToVector(ref OutGoingData.position, PositionControl, PositionControl.LerpAmount * DeltaTime);
        }
        else
        {
            if (HasTracked == BasisHasTracked.HasTracker)
            {
                if (InverseOffsetFromBone.Use)
                {
                    // Update the position of the secondary transform to maintain the initial offset
                    OutGoingData.position = IncomingData.position + IncomingData.rotation * InverseOffsetFromBone.position;

                    // Update the rotation of the secondary transform to maintain the initial offset
                    OutGoingData.rotation =  IncomingData.rotation * InverseOffsetFromBone.rotation;
                }
                else
                {
                    OutGoingData.rotation = IncomingData.rotation;
                    OutGoingData.position = IncomingData.position;
                }
            }
        }
    }
    public void SetOffset()
    {
        BoneModelTransform.SetLocalPositionAndRotation(Vector3.zero,TposeWorld.rotation);
    }
    public void RunRotationChange(float DeltaTime)
    {
        ApplyTargetRotation(ref OutGoingData.rotation, RotationControl);
        QuaternionClamp(ref OutGoingData.rotation, RotationControl);
        ApplyLerpToQuaternion(ref OutGoingData.rotation, RotationControl, DeltaTime);
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
        LastRunData.position = OutGoingData.position;
        LastRunData.rotation = OutGoingData.rotation;
        LastWorldData.position = OutgoingWorldData.position;
        LastWorldData.rotation = OutgoingWorldData.rotation;
        BoneTransform.SetLocalPositionAndRotation(OutGoingData.position, OutGoingData.rotation);
        BoneTransform.GetPositionAndRotation(out OutgoingWorldData.position, out OutgoingWorldData.rotation);
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
                position = positionLock.Target.OutGoingData.position + positionLock.Offset;
                break;

            case BasisTargetController.TargetDirectional:
                Vector3 customDirection = positionLock.Target.OutGoingData.rotation * positionLock.Offset;
                position = positionLock.Target.OutGoingData.position + customDirection;
                break;
        }
    }
    public void ApplyTargetRotation(ref Quaternion rotation, BasisRotationalControl AxisLock)
    {
        switch (AxisLock.TaretInterpreter)
        {
            case BasisTargetController.Target:
                rotation = AxisLock.Target.OutGoingData.rotation;
                break;

            case BasisTargetController.TargetDirectional:
                rotation = AxisLock.Target.OutGoingData.rotation * AxisLock.Offset;
                break;
        }
    }
#if UNITY_EDITOR
    public static float DefaultGizmoSize = 0.05f;
    public static float HandGizmoSize = 0.015f;
    public void DrawGizmos()
    {
        if (HasBone)
        {
            Gizmos.color = Color;
            Vector3 BonePosition = OutgoingWorldData.position;
            if (PositionControl.TaretInterpreter != BasisTargetController.None)
            {
                Gizmos.DrawLine(BonePosition, PositionControl.Target.OutgoingWorldData.position);
            }
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindTrackedRole(this, out BasisBoneTrackedRole Frole))
            {
                if (BasisBoneTrackedRoleCommonCheck.CheckIfRightHand(Frole) || BasisBoneTrackedRoleCommonCheck.CheckIfLeftHand(Frole))
                {
                    Gizmos.DrawWireSphere(BonePosition, HandGizmoSize * BasisLocalPlayer.Instance.RatioAvatarToAvatarEyeDefaultScale);
                }
                else
                {
                    Gizmos.DrawWireSphere(BonePosition, DefaultGizmoSize * BasisLocalPlayer.Instance.RatioAvatarToAvatarEyeDefaultScale);
                }
            }
         //   Gizmos.DrawWireSphere(TposeLocal.position, DefaultGizmoSize * BasisLocalPlayer.Instance.RatioAvatarToAvatarEyeDefaultScale);
            Handles.Label(BonePosition, Name);
            if (BasisLocalPlayer.Instance.AvatarDriver.InTPose)
            {
                if (BasisLocalPlayer.Instance.LocalBoneDriver.FindTrackedRole(this, out BasisBoneTrackedRole role))
                {
                    Gizmos.DrawWireSphere(BonePosition, (BasisAvatarIKStageCalibration.MaxDistanceBeforeMax(role) /2) * BasisLocalPlayer.Instance.RatioAvatarToAvatarEyeDefaultScale);
                }
            }
        }
    }
#endif
    private void ApplyLerpToQuaternion(ref Quaternion quaternionRotation, BasisRotationalControl Rotation,float DeltaTime)
    {
        if (Rotation.Lerp != BasisAxisLerp.None)
        {
            float angleDifference = Quaternion.Angle(LastRunData.rotation, quaternionRotation);
            float Timing = Mathf.Clamp01(angleDifference / Rotation.AngleBeforeSpeedup);
            float lerpAmount = Mathf.Lerp(Rotation.LerpAmountNormal, Rotation.LerpAmountFastMovement, Timing);

            float lerpFactor = lerpAmount * DeltaTime;
            ApplyActualLerpToQuaternion(ref quaternionRotation, Rotation, lerpFactor);
        }
    }
    private void ApplyActualLerpToQuaternion(ref Quaternion NewQuaternionRotation, BasisRotationalControl Rotation, float lerpFactor)
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
    private void ApplyLerpToVector(ref Vector3 position, BasisPositionControl axis,float DeltaTime)
    {
        switch (axis.Lerp)
        {
            case BasisVectorLerp.None:
                break;
            case BasisVectorLerp.Lerp:
                position = Vector3.Lerp(LastRunData.position, position, axis.LerpAmount * DeltaTime);
                break;
            case BasisVectorLerp.SphericalLerp:
                position = Vector3.Slerp(LastRunData.position, position, axis.LerpAmount * DeltaTime);
                break;
            case BasisVectorLerp.LerpUnclamped:
                position = Vector3.LerpUnclamped(LastRunData.position, position, axis.LerpAmount * DeltaTime);
                break;
            case BasisVectorLerp.SphericalLerpUnclamped:
                position = Vector3.SlerpUnclamped(LastRunData.position, position, axis.LerpAmount * DeltaTime);
                break;
        }
    }
}