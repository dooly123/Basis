using Basis.Scripts.Avatar;
using Basis.Scripts.Common;
using Basis.Scripts.Common.Enums;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Basis.Scripts.TransformBinders.BoneControl
{
    [System.Serializable]
    [BurstCompile]
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

        public BasisGeneralLocation GeneralLocation;
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
            if (Cullable)
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
        [BurstCompile]
public bool AngleCheck(quaternion AngleA, quaternion AngleB, float MaximumTolerance = 0.005f)
{
    // Calculate the dot product between the quaternions
    float dotProduct = math.dot(AngleA, AngleB);

    // Clamp the dot product to avoid numerical errors leading to invalid acos input
    dotProduct = math.clamp(dotProduct, -1f, 1f);

    // Calculate the angle between the quaternions in radians using acos
    float angle = math.acos(dotProduct) * 2f; // Multiply by 2 because the dot product returns half of the angle

    // Convert angle to degrees (if needed, can be omitted if using radians is fine)
    float angleInDegrees = math.degrees(angle);

    // Check if the angle exceeds the tolerance
    return angleInDegrees > MaximumTolerance;
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
        [BurstCompile]
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
        [BurstCompile]
        private void ApplyLerpToQuaternion(float deltaTime)
        {
            // Calculate the dot product once
            float dotProduct = math.dot(LastRunData.rotation, OutGoingData.rotation);

            // If the dot product is close to 1, then the quaternions are nearly identical
            if (dotProduct > 0.9999999999f)
            {
                return;  // No need for interpolation
            }

            // Calculate the angle difference (avoid acos for very small differences)
            float angleDifference = math.acos(math.clamp(dotProduct, -1f, 1f));

            // If the angle difference is small enough, skip interpolation
            if (angleDifference < math.EPSILON)
            {
                return;
            }

            // Use a cached version of the LerpAmount values to avoid repeated accesses
            float lerpAmountNormal = RotationControl.LerpAmountNormal;
            float lerpAmountFastMovement = RotationControl.LerpAmountFastMovement;

            // Calculate the lerp factor
            float timing = math.clamp(angleDifference / RotationControl.AngleBeforeSpeedup, 0f, 1f);
            float lerpAmount = math.lerp(lerpAmountNormal, lerpAmountFastMovement, timing);
            float lerpFactor = lerpAmount * deltaTime;

            // Apply spherical interpolation (slerp)
            OutGoingData.rotation = math.slerp(LastRunData.rotation, OutGoingData.rotation, lerpFactor);
        }
        [BurstCompile]
        private void ApplyLerpToVector(float DeltaTime)
        {

            // Calculate the interpolation factor
            float lerpFactor = math.clamp(PositionControl.LerpAmount * DeltaTime, 0f, 1f);

            // Use math.lerp for interpolation with float3 (which is the equivalent of Vector3 in Unity.Mathematics)
            OutGoingData.position = math.lerp(LastRunData.position, OutGoingData.position, lerpFactor);
        }
    }
}