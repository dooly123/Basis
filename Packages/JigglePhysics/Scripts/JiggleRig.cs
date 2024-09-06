using System;
using Unity.Jobs;
using UnityEngine;
using static JiggleRigConstruction;
namespace JigglePhysics
{
    [Serializable]
    public class JiggleRig : JiggleRigBase
    {
        [Tooltip("The settings that the rig should update with, create them using the Create->JigglePhysics->Settings menu option.")]
        public JiggleSettingsBase jiggleSettings;
        [SerializeField]
        public JiggleSettingsData jiggleSettingsdata;

        public bool NeedsCollisions;
        public int collidersCount;
        public Vector3 Zero = Vector3.zero;

        public UpdateParticleSignalsJob SignalJob;
        public ExtrapolationJob extrapolationJob;

        [SerializeField]
        [Tooltip("The list of transforms to ignore during the jiggle. Each bone listed will also ignore all the children of the specified bone.")]
        public Transform[] ignoredTransforms;
        public Collider[] colliders;
        [SerializeField]
        [Tooltip("The root bone from which an individual JiggleRig will be constructed. The JiggleRig encompasses all children of the specified root.")]
        public Transform rootTransform;
        public SphereCollider sphereCollider;
        public JiggleRigLOD JiggleRigLOD;

        public void Initialize(JiggleRigLOD jiggleRigLOD)
        {
            JiggleRigLOD = jiggleRigLOD;
            InitalizeLists(this);
            CreateSimulatedPoints(this, ignoredTransforms, rootTransform, null);
            InitalizeIndexes();
            simulatedPointsCount = JiggleBones.Length;

            // Precompute normalized indices in a single pass
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                JiggleBone test = JiggleBones[SimulatedIndex];
                int distanceToRoot = 0, distanceToChild = 0;

                // Calculate distance to root
                while (test.JiggleParentIndex != -1)
                {
                    test = JiggleBones[test.JiggleParentIndex];
                    distanceToRoot++;
                }
                test = JiggleBones[SimulatedIndex];
                // Calculate distance to child
                while (test.childIndex != -1)
                {
                    test = JiggleBones[test.childIndex];
                    distanceToChild++;
                }
                int max = distanceToRoot + distanceToChild;
                PreInitalData.normalizedIndex[SimulatedIndex] = (float)distanceToRoot / max;
            }
            InitializeNativeArrays();
            jiggleSettingsdata = jiggleSettings.GetData();
            NeedsCollisions = colliders.Length != 0; 
            if (NeedsCollisions)
            {
                if (!CachedSphereCollider.TryGet(out sphereCollider))
                {
                    Debug.LogError("Missing Sphere Collider Bailing!");
                    return;  // No need to proceed if there's no valid sphereCollider
                }
            }
            SignalJob = new UpdateParticleSignalsJob
            {
                workingPosition = Runtimedata.workingPosition,
                particleSignalCurrent = Runtimedata.particleSignalCurrent,
                particleSignalPrevious = Runtimedata.particleSignalPrevious
            };
            extrapolationJob = new ExtrapolationJob
            {
                ParticleSignalCurrent = Runtimedata.particleSignalCurrent,
                ParticleSignalPrevious = Runtimedata.particleSignalPrevious,
                ExtrapolatedPosition = Runtimedata.extrapolatedPosition
            };
        }
        public Vector3 ConstrainLengthBackwards(int JiggleIndex, Vector3 newPosition, float elasticity)
        {
            if (JiggleBones[JiggleIndex].childIndex == -1)
            {
                return newPosition;
            }

            Vector3 diff = newPosition - Runtimedata.workingPosition[JiggleBones[JiggleIndex].childIndex];
            Vector3 dir = diff.normalized;

            int ParentIndex = JiggleBones[JiggleIndex].JiggleParentIndex;
            float lengthToParent = Vector3.Distance(Runtimedata.currentFixedAnimatedBonePosition[JiggleIndex], Runtimedata.currentFixedAnimatedBonePosition[ParentIndex]);

            return Vector3.Lerp(newPosition, Runtimedata.workingPosition[JiggleBones[JiggleIndex].childIndex] + dir * lengthToParent, elasticity);
        }

        public void Update(Vector3 wind, float velvetTiming, float squaredDeltaTime, Vector3 gravity, float percentage)
        {
            // Precompute common values
            Vector3 gravityEffect = gravity * (jiggleSettingsdata.gravityMultiplier * squaredDeltaTime);
            float airDragDeltaTime = velvetTiming * jiggleSettingsdata.airDrag;
            float inverseAirDrag = 1f - jiggleSettingsdata.airDrag;
            float inverseFriction = 1f - jiggleSettingsdata.friction;

            for (int PointIndex = 0; PointIndex < simulatedPointsCount; PointIndex++)
            {
                // Cache values for better performance
                Vector3 currentTargetSignal = Runtimedata.targetAnimatedBoneSignalCurrent[PointIndex];
                Vector3 previousTargetSignal = Runtimedata.targetAnimatedBoneSignalPrevious[PointIndex];

                Vector3 interpolatedBonePosition = Vector3.Lerp(previousTargetSignal, currentTargetSignal, percentage);
                Runtimedata.currentFixedAnimatedBonePosition[PointIndex] = interpolatedBonePosition;

                if (JiggleBones[PointIndex].JiggleParentIndex == -1)
                {
                    Runtimedata.workingPosition[PointIndex] = interpolatedBonePosition;

                    Vector3 particleCurrentSignal = Runtimedata.particleSignalCurrent[PointIndex];
                    Runtimedata.particleSignalPrevious[PointIndex] = particleCurrentSignal;
                    Runtimedata.particleSignalCurrent[PointIndex] = interpolatedBonePosition;

                    continue;
                }

                // Cache parent values
                int parentIndex = JiggleBones[PointIndex].JiggleParentIndex;
                Vector3 parentCurrentSignal = Runtimedata.particleSignalCurrent[parentIndex];
                Vector3 parentPreviousSignal = Runtimedata.particleSignalPrevious[parentIndex];

                Vector3 currentSignal = Runtimedata.particleSignalCurrent[PointIndex];
                Vector3 previousSignal = Runtimedata.particleSignalPrevious[PointIndex];

                // Compute deltas
                Vector3 deltaSignal = currentSignal - previousSignal;
                Vector3 parentDeltaSignal = parentCurrentSignal - parentPreviousSignal;
                Vector3 localSpaceVelocity = deltaSignal - parentDeltaSignal;

                // Update working position
                Vector3 workingPos = currentSignal
                    + (deltaSignal - localSpaceVelocity) * inverseAirDrag
                    + localSpaceVelocity * inverseFriction
                    + gravityEffect
                    + wind * airDragDeltaTime;

                Runtimedata.workingPosition[PointIndex] = workingPos;
            }

            // Constrain length if needed
            if (NeedsCollisions)
            {
                for (int PointIndex = simulatedPointsCount - 1; PointIndex >= 0; PointIndex--)
                {
                    Runtimedata.workingPosition[PointIndex] = ConstrainLengthBackwards( PointIndex, Runtimedata.workingPosition[PointIndex], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity * 0.5f);
                }
            }

            // Adjust working positions based on parent constraints
            for (int PointIndex = 0; PointIndex < simulatedPointsCount; PointIndex++)
            {
                if (JiggleBones[PointIndex].JiggleParentIndex == -1 || !Runtimedata.hasTransform[PointIndex])
                {
                    continue;
                }

                int parentIndex = JiggleBones[PointIndex].JiggleParentIndex;
                int grandParentIndex = JiggleBones[parentIndex].JiggleParentIndex;

                Vector3 parentParentPosition;
                Vector3 poseParentParent;

                if (grandParentIndex == -1)
                {
                    poseParentParent = Runtimedata.currentFixedAnimatedBonePosition[parentIndex] + (Runtimedata.currentFixedAnimatedBonePosition[parentIndex] - Runtimedata.currentFixedAnimatedBonePosition[PointIndex]);
                    parentParentPosition = poseParentParent;
                }
                else
                {
                    parentParentPosition = Runtimedata.workingPosition[grandParentIndex];
                    poseParentParent = Runtimedata.currentFixedAnimatedBonePosition[parentIndex];
                }

                Vector3 parentAimTargetPose = Runtimedata.currentFixedAnimatedBonePosition[parentIndex] - poseParentParent;
                Vector3 parentAim = Runtimedata.workingPosition[parentIndex] - parentParentPosition;
                Quaternion rotationToTargetPose = Quaternion.FromToRotation(parentAimTargetPose, parentAim);

                Vector3 currentPose = Runtimedata.currentFixedAnimatedBonePosition[PointIndex] - poseParentParent;
                Vector3 constraintTarget = rotationToTargetPose * currentPose;

               int ParentIndex = JiggleBones[PointIndex].JiggleParentIndex;
                float  lengthToParent = Vector3.Distance(Runtimedata.currentFixedAnimatedBonePosition[PointIndex], Runtimedata.currentFixedAnimatedBonePosition[ParentIndex]);

                float error = Vector3.Distance(Runtimedata.workingPosition[PointIndex], parentParentPosition + constraintTarget) / lengthToParent;
                error = Mathf.Clamp01(error);
                error = Mathf.Pow(error, jiggleSettingsdata.elasticitySoften * 2f);

                Runtimedata.workingPosition[PointIndex] = Vector3.Lerp(
                    Runtimedata.workingPosition[PointIndex],
                    parentParentPosition + constraintTarget,
                    jiggleSettingsdata.angleElasticity * jiggleSettingsdata.angleElasticity * error);

                // Constrain Length
                Vector3 directionToParent = (Runtimedata.workingPosition[PointIndex] - Runtimedata.workingPosition[parentIndex]).normalized;
                Runtimedata.workingPosition[PointIndex] = Vector3.Lerp(
                    Runtimedata.workingPosition[PointIndex],
                    Runtimedata.workingPosition[parentIndex] + directionToParent * lengthToParent,
                    jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity);
            }

            // Handle collisions
            if (NeedsCollisions)
            {
                for (int PointIndex = 0; PointIndex < simulatedPointsCount; PointIndex++)
                {
                    float radius = jiggleSettings.GetRadius(Runtimedata.normalizedIndex[PointIndex]);
                    if (radius <= 0) continue;

                    sphereCollider.radius = radius;
                    Vector3 position = Runtimedata.workingPosition[PointIndex];

                    for (int ColliderIndex = 0; ColliderIndex < collidersCount; ColliderIndex++)
                    {
                        Collider collider = colliders[ColliderIndex];
                        collider.transform.GetPositionAndRotation(out Vector3 colliderPosition, out Quaternion colliderRotation);

                        if (Physics.ComputePenetration(
                            sphereCollider,
                            position,
                            Quaternion.identity,
                            collider,
                            colliderPosition,
                            colliderRotation,
                            out Vector3 penetrationDirection,
                            out float penetrationDistance))
                        {
                            position += penetrationDirection * penetrationDistance;
                        }
                    }

                    Runtimedata.workingPosition[PointIndex] = position;
                }
            }
            UpdateParticleSignals();
        }

        // Method to schedule and execute the job
        public void UpdateParticleSignals()
        {
            JobHandle jobHandle = SignalJob.Schedule(simulatedPointsCount, 64); // You can adjust the batch size (64 here) if needed
            jobHandle.Complete();
        }

        public void PrepareBone(Vector3 position)
        {
            for (int PointIndex = 0; PointIndex < simulatedPointsCount; PointIndex++)
            {
                Vector3 CurrentSignal = Runtimedata.targetAnimatedBoneSignalCurrent[PointIndex];
                Vector3 PreviousSignal;

                // If bone is not animated, return to last unadulterated pose
                if (Runtimedata.hasTransform[PointIndex])
                {
                    ComputedTransforms[PointIndex].GetLocalPositionAndRotation(out Vector3 localPosition, out Quaternion localrotation);
                    if (Runtimedata.boneRotationChangeCheck[PointIndex] == localrotation)
                    {
                        ComputedTransforms[PointIndex].localRotation = Runtimedata.lastValidPoseBoneRotation[PointIndex];
                    }
                    if (Runtimedata.bonePositionChangeCheck[PointIndex] == localPosition)
                    {
                        ComputedTransforms[PointIndex].localPosition = Runtimedata.lastValidPoseBoneLocalPosition[PointIndex];
                    }
                }
                else
                {
                    int ParentIndex = JiggleBones[PointIndex].JiggleParentIndex;
                    PreviousSignal = CurrentSignal;
                    CurrentSignal = GetProjectedPosition(PointIndex, ParentIndex);

                    Runtimedata.targetAnimatedBoneSignalCurrent[PointIndex] = CurrentSignal;
                    Runtimedata.targetAnimatedBoneSignalPrevious[PointIndex] = PreviousSignal;
                    continue;
                }

                PreviousSignal = CurrentSignal;
                CurrentSignal = ComputedTransforms[PointIndex].position;

                Runtimedata.targetAnimatedBoneSignalCurrent[PointIndex] = CurrentSignal;
                Runtimedata.targetAnimatedBoneSignalPrevious[PointIndex] = PreviousSignal;

                ComputedTransforms[PointIndex].GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
                Runtimedata.lastValidPoseBoneRotation[PointIndex] = Rot;
                Runtimedata.lastValidPoseBoneLocalPosition[PointIndex] = pos;
            }
            jiggleSettingsdata = JiggleRigLOD.AdjustJiggleSettingsData(position, jiggleSettingsdata);
        }
        public void Pose(float Percentage)
        {
            Vector3 CurrentSignal = Runtimedata.particleSignalCurrent[0];
            Vector3 PreviousSignal = Runtimedata.particleSignalPrevious[0];

            Runtimedata.extrapolatedPosition[0] = (Percentage == 0) ? PreviousSignal : Vector3.Lerp(PreviousSignal, CurrentSignal, Percentage);
            Vector3 offset = ComputedTransforms[0].transform.position - Runtimedata.extrapolatedPosition[0];

            // Update the job
            extrapolationJob.Percentage = Percentage;
            extrapolationJob.Offset = offset;

            // Schedule the job
            JobHandle jobHandle = extrapolationJob.Schedule(simulatedPointsCount, 64);

            // Complete the job
            jobHandle.Complete();

            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (JiggleBones[SimulatedIndex].childIndex == -1)
                {
                    continue; // Early exit if there's no child
                }
                int ChildIndex = JiggleBones[SimulatedIndex].childIndex;
                // Cache frequently accessed values
                Vector3 CurrentAnimated = Runtimedata.targetAnimatedBoneSignalCurrent[SimulatedIndex];
                Vector3 PreviousAnimated = Runtimedata.targetAnimatedBoneSignalPrevious[SimulatedIndex];

                Vector3 ChildCurrentAnimated = Runtimedata.targetAnimatedBoneSignalCurrent[ChildIndex];
                Vector3 ChildPreviousAnimated = Runtimedata.targetAnimatedBoneSignalPrevious[ChildIndex];

                Vector3 targetPosition = (Percentage == 0) ? PreviousAnimated : Vector3.Lerp(PreviousAnimated, CurrentAnimated, Percentage);
                Vector3 childTargetPosition = (Percentage == 0) ? ChildPreviousAnimated : Vector3.Lerp(ChildPreviousAnimated, ChildCurrentAnimated, Percentage);
                // Blend positions
                Vector3 positionBlend = Vector3.Lerp(targetPosition, Runtimedata.extrapolatedPosition[SimulatedIndex], jiggleSettingsdata.blend);

                Vector3 childPositionBlend = Vector3.Lerp(childTargetPosition, Runtimedata.extrapolatedPosition[ChildIndex], jiggleSettingsdata.blend);

                if (JiggleBones[SimulatedIndex].JiggleParentIndex != -1)
                {
                    ComputedTransforms[SimulatedIndex].position = positionBlend;
                }

                // Calculate child position and vector differences
                int childIndex = JiggleBones[SimulatedIndex].childIndex;
                Vector3 childPosition = GetTransformPosition(childIndex);
                Vector3 cachedAnimatedVector = childPosition - positionBlend;
                Vector3 simulatedVector = childPositionBlend - positionBlend;

                // Rotate the transform based on the vector differences
                if (cachedAnimatedVector != Zero && simulatedVector != Zero)
                {
                    Quaternion animPoseToPhysicsPose = Quaternion.FromToRotation(cachedAnimatedVector, simulatedVector);
                    ComputedTransforms[SimulatedIndex].rotation = animPoseToPhysicsPose * ComputedTransforms[SimulatedIndex].rotation;
                }

                // Cache transform changes if the bone has a transform
                if (Runtimedata.hasTransform[SimulatedIndex])
                {
                    ComputedTransforms[SimulatedIndex].GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
                    Runtimedata.boneRotationChangeCheck[SimulatedIndex] = Rot;
                    Runtimedata.bonePositionChangeCheck[SimulatedIndex] = pos;
                }
            }
        }
    }
}