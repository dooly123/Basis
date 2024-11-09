using Basis.Scripts.Avatar;
using Basis.Scripts.BasisSdk.Players;
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
        public BasisCalibratedCoords LastOutGoingData = new BasisCalibratedCoords();
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
            LastOutGoingData.position = OutGoingData.position;
            LastOutGoingData.rotation = OutGoingData.rotation;
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
                    //just basic lerp with some speedup
                    if (RotationControl.Target != null)
                    {
                        OutGoingData.rotation = RotationControl.Target.OutGoingData.rotation;
                    }
                    ApplyLerpToQuaternion(DeltaTime);

                    if (PositionControl.HasTarget)
                    {
                        Vector3 customDirection = PositionControl.Target.OutGoingData.rotation * PositionControl.Offset;
                        OutGoingData.position = PositionControl.Target.OutGoingData.position + customDirection;
                    }
                    // Calculate the interpolation factor
                    float lerpFactor = math.clamp(PositionControl.LerpAmount * DeltaTime, 0f, 1f);

                    // Use math.lerp for interpolation with float3 (which is the equivalent of Vector3 in Unity.Mathematics)
                    OutGoingData.position = math.lerp(LastRunData.position, OutGoingData.position, lerpFactor);
                }
            }
        }
        [BurstCompile]
        public void ApplyLerpToQuaternion(float LerpAmount)
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
            float lerpFactor = lerpAmount * LerpAmount;

            // Apply spherical interpolation (slerp)
            OutGoingData.rotation = math.slerp(LastRunData.rotation, OutGoingData.rotation, lerpFactor);
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
                BoneTransform.GetLocalPositionAndRotation(out LastRunData.position, out LastRunData.rotation);
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
            BoneTransform.localPosition = OutGoingData.position;
            BoneTransform.localRotation = OutGoingData.rotation;
            // (OutGoingData.position, OutGoingData.rotation);
            BoneTransform.GetPositionAndRotation(out OutgoingWorldData.position, out OutgoingWorldData.rotation);
        }
    }
}