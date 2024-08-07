using Basis.Scripts.Common;
using Basis.Scripts.Common.Enums;
using UnityEngine;
using UnityEngine.Events;
using static Basis.Scripts.Avatar.BasisAvatarIKStageCalibration;

namespace Basis.Scripts.TransformBinders.BoneControl
{
    [System.Serializable]
public class BasisBoneControl
{
    public bool Cullable = false;
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

    public UnityEvent<float, float> WeightsChanged = new UnityEvent<float, float>();
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
        if(Cullable)
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
                        ApplyTargetRotation();
                        QuaternionClamp();
                        ApplyLerpToQuaternion((RotationControl.LerpAmountNormal / 2) * DeltaTime);
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
            ApplyTargetPosition();
            ApplyLerpToVector(PositionControl.LerpAmount * DeltaTime);
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
                    OutGoingData.rotation = IncomingData.rotation * InverseOffsetFromBone.rotation;
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
        BoneModelTransform.SetLocalPositionAndRotation(Vector3.zero, TposeWorld.rotation);
    }
    public void RunRotationChange(float DeltaTime)
    {
        ApplyTargetRotation();
        QuaternionClamp();
        ApplyLerpToQuaternion(DeltaTime);
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
        if (Cullable)
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
    public void QuaternionClamp()
    {
        if (RotationControl.ClampStats != BasisClampData.Clamp)
        {
            return;
        }
        Vector3 clampedEulerAngles = OutGoingData.rotation.eulerAngles;
        if (RotationControl.ClampableAxis == BasisClampAxis.x)
        {
            clampedEulerAngles.x = Mathf.Clamp(clampedEulerAngles.x, clampedEulerAngles.x - RotationControl.ClampSize, RotationControl.ClampSize);
        }
        else if (RotationControl.ClampableAxis == BasisClampAxis.y)
        {
            clampedEulerAngles.y = Mathf.Clamp(clampedEulerAngles.y, clampedEulerAngles.y - RotationControl.ClampSize, RotationControl.ClampSize);
        }
        else if (RotationControl.ClampableAxis == BasisClampAxis.z)
        {
            clampedEulerAngles.z = Mathf.Clamp(clampedEulerAngles.z, clampedEulerAngles.z - RotationControl.ClampSize, RotationControl.ClampSize);
        }
        else if (RotationControl.ClampableAxis == BasisClampAxis.xz)
        {
            clampedEulerAngles.z = Mathf.Clamp(clampedEulerAngles.z, clampedEulerAngles.z - RotationControl.ClampSize, RotationControl.ClampSize);
            clampedEulerAngles.x = Mathf.Clamp(clampedEulerAngles.x, clampedEulerAngles.x - RotationControl.ClampSize, RotationControl.ClampSize);
        }
        OutGoingData.rotation = Quaternion.Euler(clampedEulerAngles);
    }
    public void ApplyTargetPosition()
    {
        switch (PositionControl.TaretInterpreter)
        {
            case BasisTargetController.Target:
                OutGoingData.position = PositionControl.Target.OutGoingData.position + PositionControl.Offset;
                break;

            case BasisTargetController.TargetDirectional:
                Vector3 customDirection = PositionControl.Target.OutGoingData.rotation * PositionControl.Offset;
                OutGoingData.position = PositionControl.Target.OutGoingData.position + customDirection;
                break;
        }
    }
    public void ApplyTargetRotation()
    {
        switch (RotationControl.TaretInterpreter)
        {
            case BasisTargetController.Target:
                OutGoingData.rotation = RotationControl.Target.OutGoingData.rotation;
                break;

            case BasisTargetController.TargetDirectional:
                OutGoingData.rotation = RotationControl.Target.OutGoingData.rotation * RotationControl.Offset;
                break;
        }
    }
    private void ApplyLerpToQuaternion(float deltaTime)
    {
        if (RotationControl.Lerp == BasisAxisLerp.None)
        {
            return;
        }

        float angleDifference = Quaternion.Angle(LastRunData.rotation, OutGoingData.rotation);
        float timing = Mathf.Clamp01(angleDifference / RotationControl.AngleBeforeSpeedup);
        float lerpAmount = Mathf.Lerp(RotationControl.LerpAmountNormal, RotationControl.LerpAmountFastMovement, timing);

        float lerpFactor = lerpAmount * deltaTime;

        Quaternion lastRotation = LastRunData.rotation;
        Quaternion outgoingRotation = OutGoingData.rotation;

        switch (RotationControl.Lerp)
        {
            case BasisAxisLerp.Lerp:
                OutGoingData.rotation = Quaternion.Lerp(lastRotation, outgoingRotation, lerpFactor);
                break;
            case BasisAxisLerp.SphericalLerp:
                OutGoingData.rotation = Quaternion.Slerp(lastRotation, outgoingRotation, lerpFactor);
                break;
            case BasisAxisLerp.LerpUnclamped:
                OutGoingData.rotation = Quaternion.LerpUnclamped(lastRotation, outgoingRotation, lerpFactor);
                break;
            case BasisAxisLerp.SphericalLerpUnclamped:
                OutGoingData.rotation = Quaternion.SlerpUnclamped(lastRotation, outgoingRotation, lerpFactor);
                break;
        }
    }
    private void ApplyLerpToVector(float DeltaTime)
    {
        switch (PositionControl.Lerp)
        {
            case BasisVectorLerp.None:
                break;
            case BasisVectorLerp.Lerp:
                OutGoingData.position = Vector3.Lerp(LastRunData.position, OutGoingData.position, PositionControl.LerpAmount * DeltaTime);
                break;
            case BasisVectorLerp.SphericalLerp:
                OutGoingData.position = Vector3.Slerp(LastRunData.position, OutGoingData.position, PositionControl.LerpAmount * DeltaTime);
                break;
            case BasisVectorLerp.LerpUnclamped:
                OutGoingData.position = Vector3.LerpUnclamped(LastRunData.position, OutGoingData.position, PositionControl.LerpAmount * DeltaTime);
                break;
            case BasisVectorLerp.SphericalLerpUnclamped:
                OutGoingData.position = Vector3.SlerpUnclamped(LastRunData.position, OutGoingData.position, PositionControl.LerpAmount * DeltaTime);
                break;
        }
    }
}
}