using Basis.Scripts.Avatar;
using Basis.Scripts.Common;
using System;
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
        [SerializeField]
        public BasisRotationalControl RotationControl = new BasisRotationalControl();
        [SerializeField]
        public BasisPositionControl PositionControl = new BasisPositionControl();
        [SerializeField]
        public BasisCalibratedCoords OutGoingData = new BasisCalibratedCoords();
        [SerializeField]
        public BasisCalibratedCoords LastRunData = new BasisCalibratedCoords();
        [SerializeField]
        public BasisCalibratedCoords TposeWorld = new BasisCalibratedCoords();
        [SerializeField]
        public BasisCalibratedCoords TposeLocal = new BasisCalibratedCoords();
        [SerializeField]
        public BasisCalibratedOffsetData InverseOffsetFromBone = new BasisCalibratedOffsetData();
        [SerializeField]
        public BasisCalibratedCoords IncomingData = new BasisCalibratedCoords();
        [SerializeField]
        public BasisCalibratedCoords OutgoingWorldData = new BasisCalibratedCoords();

        public Action VirtualRun;
        public bool HasVirtualOverride;
        public void ComputeMovement(float DeltaTime)
        {
            NotProcessing = !HasBone || Cullable;
            if (NotProcessing)
            {
                return;
            }
            if (HasTracked == BasisHasTracked.HasTracker)
            {
                if (InverseOffsetFromBone.Use)
                {
                    // Update the position of the secondary transform to maintain the initial offset
                    OutGoingData.position = IncomingData.position + math.mul(IncomingData.rotation, InverseOffsetFromBone.position);

                    // Update the rotation of the secondary transform to maintain the initial offset
                    OutGoingData.rotation = math.mul(IncomingData.rotation, InverseOffsetFromBone.rotation);
                }
                else
                {
                    ///this is going to the generic always accurate fake skeleton
                    OutGoingData.rotation = IncomingData.rotation;
                    OutGoingData.position = IncomingData.position;
                }
            }
            else
            {
                if (HasVirtualOverride)
                {
                    VirtualRun?.Invoke();
                }
                else
                {
                    //this is essentially the default behaviour, most of it is normally Virtually Overriden
                    //relying on a one size fits all shoe is wrong and as of such we barely use this anymore.
                    if (RotationControl.HasTarget)
                    {
                        OutGoingData.rotation = ApplyLerpToQuaternion(DeltaTime, LastRunData.rotation, RotationControl.Target.OutGoingData.rotation);
                    }

                    if (PositionControl.HasTarget)
                    {
                        // Apply the rotation offset using math.mul
                        float3 customDirection = math.mul(PositionControl.Target.OutGoingData.rotation, PositionControl.Offset);

                        // Calculate the target outgoing position with the rotated offset
                        float3 targetPosition = PositionControl.Target.OutGoingData.position + customDirection;

                        // Clamp the interpolation factor to ensure it stays between 0 and 1
                        float lerpFactor = math.clamp(PositionControl.LerpAmount * DeltaTime, 0f, 1f);

                        // Interpolate between the last position and the target position
                        OutGoingData.position = math.lerp(LastRunData.position, targetPosition, lerpFactor);
                    }
                }
            }
        }
        [BurstCompile]
        public Quaternion ApplyLerpToQuaternion(float DeltaTime, Quaternion CurrentRotation, Quaternion FutureRotation)
        {
            // Calculate the dot product once to check similarity between rotations
            float dotProduct = math.dot(CurrentRotation, FutureRotation);

            // If quaternions are nearly identical, skip interpolation
            if (dotProduct > 0.9999999999f)
            {
                return FutureRotation;
            }

            // Calculate angle difference (avoid acos for very small differences)
            float angleDifference = math.acos(math.clamp(dotProduct, -1f, 1f));

            // If the angle difference is too small, skip interpolation
            if (angleDifference < math.EPSILON)
            {
                return FutureRotation;
            }

            // Access cached LerpAmount values for normal and fast movement
            float lerpAmountNormal = RotationControl.LerpAmountNormal;
            float lerpAmountFastMovement = RotationControl.LerpAmountFastMovement;

            // Calculate timing factor based on angle threshold for speedup
            float timing = math.clamp(angleDifference / RotationControl.AngleBeforeSpeedup, 0f, 1f);

            // Interpolate between normal and fast movement rates based on angle
            float lerpAmount = math.lerp(lerpAmountNormal, lerpAmountFastMovement, timing);

            // Apply frame-rate-independent lerp factor
            float lerpFactor = lerpAmount * DeltaTime;

            // Perform the slerp (spherical interpolation) using the calculated factor
            return math.slerp(CurrentRotation, FutureRotation, lerpFactor);
        }
        [HideInInspector]
        public bool Cullable = false;
        [SerializeField]
        public string Name;
        [SerializeField]
        [HideInInspector]
        private Color gizmoColor = Color.blue;
        [SerializeField]
        [HideInInspector]
        public bool HasBone = false;
        [SerializeField]
        public Transform BoneTransform;
        [HideInInspector]
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
        [HideInInspector]
        public bool NotProcessing = false;
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
        [HideInInspector]
        [SerializeField]
        private float positionWeight = 1;
        [HideInInspector]
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
        [HideInInspector]
        public BasisGeneralLocation GeneralLocation;
        public Color Color { get => gizmoColor; set => gizmoColor = value; }
        public void Initialize()
        {
            if (HasBone)
            {
                BoneTransform.GetLocalPositionAndRotation(out Vector3 position, out Quaternion Rotation);
                LastRunData.position = position;
                LastRunData.rotation = Rotation;
            }
        }
        public void ApplyMovement()
        {
            if (NotProcessing)
            {
                return;
            }
            LastRunData.position = OutGoingData.position;
            LastRunData.rotation = OutGoingData.rotation;
            BoneTransform.SetLocalPositionAndRotation(OutGoingData.position, OutGoingData.rotation);
            BoneTransform.GetPositionAndRotation(out Vector3 position, out Quaternion Rotation);

            OutgoingWorldData.position = position;
            OutgoingWorldData.rotation = Rotation;
        }
    }
}