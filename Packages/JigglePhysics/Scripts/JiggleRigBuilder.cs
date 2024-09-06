using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
namespace JigglePhysics
{
    [DefaultExecutionOrder(15001)]
    public class JiggleRigBuilder : MonoBehaviour
    {
        public const float VERLET_TIME_STEP = 0.02f;
        public const float MAX_CATCHUP_TIME = VERLET_TIME_STEP * 4f;

        public JiggleRig[] jiggleRigs;
        public JiggleRigRuntime[] JiggleRigsRuntime;
        [Tooltip("An air force that is applied to the entire rig, this is useful to plug in some wind volumes from external sources.")]
        [SerializeField]
        public Vector3 wind;

        [Tooltip("Level of detail manager. This system will control how the jiggle rig saves performance cost.")]
        [SerializeField]
        public JiggleRigSimpleLOD levelOfDetail;

        private double accumulation;
        private bool dirtyFromEnable = false;
        private bool wasLODActive = true;
        public int jiggleRigsCount;
        public Vector3 gravity;
        // Cached variables to avoid repeated Unity API calls
        private Vector3 JigPosition;
        public double currentFrame;
        public double previousFrame;
        public Vector3 VectorZero;
        public float squaredDeltaTime;

        public bool[] NeedsCollisions;
        public int[] collidersCount;
        public int[] simulatedPointsCount;

        public List<bool> TempNeedsCollisions = new List<bool>();
        public List<int> TempcollidersCount = new List<int>();
        public List<int> TempsimulatedPointsCount = new List<int>();
        public void Initialize()
        {
            VectorZero = Vector3.zero;
            gravity = Physics.gravity;
            accumulation = 0f;
            jiggleRigsCount = jiggleRigs.Length;
            JigPosition = transform.position;

            double CurrentTime = Time.timeAsDouble;
            currentFrame = CurrentTime;
            previousFrame = CurrentTime;
            squaredDeltaTime = VERLET_TIME_STEP * VERLET_TIME_STEP;
            TempsimulatedPointsCount.Clear();
            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                int Count = JiggleRigHelper.Initialize(this, ref jiggleRigs[JiggleIndex],ref JiggleRigsRuntime[JiggleIndex]);
                TempsimulatedPointsCount.Add(Count);
            }
            NeedsCollisions = TempNeedsCollisions.ToArray();
            collidersCount = TempcollidersCount.ToArray();
            simulatedPointsCount = TempsimulatedPointsCount.ToArray();

            CachedSphereCollider.AddBuilder(this);
            dirtyFromEnable = true;
        }
        public void OnDestroy()
        {
            for (int jiggleIndex = 0; jiggleIndex < jiggleRigsCount; jiggleIndex++)
            {
                JiggleRigHelper.OnDestroy(ref jiggleRigs[jiggleIndex], ref JiggleRigsRuntime[jiggleIndex]);
            }
            //    NeedsCollisions.Dispose();
            //   collidersCount.Dispose();
            //   simulatedPointsCount.Dispose();
        }
        private void LateUpdate()
        {
            Advance(Time.deltaTime, Time.timeAsDouble, VERLET_TIME_STEP);
        }
        [BurstCompile]
        public void Advance(float deltaTime, double timeAsDouble, float verletTiming)
        {
            JigPosition = transform.position; // Cache the position at the start of Advance
            // Early exit if not active, avoiding unnecessary checks
            if (!levelOfDetail.CheckActive(JigPosition))
            {
                if (wasLODActive)
                {
                    PrepareTeleport();
                }

                CachedSphereCollider.StartPass();
                CachedSphereCollider.FinishedPass();
                wasLODActive = false;
                return;
            }

            // Handle the transition from inactive to active
            if (!wasLODActive)
            {
                FinishTeleport(timeAsDouble);
            }

            CachedSphereCollider.StartPass();

            // Combine similar loops for cache-friendliness
            currentFrame = timeAsDouble;
            for (int jiggleIndex = 0; jiggleIndex < jiggleRigsCount; jiggleIndex++)
            {
                for (int BoneChainIndex = 0; BoneChainIndex < simulatedPointsCount[jiggleIndex]; BoneChainIndex++)
                {
                    Vector3 CurrentSignal = JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalCurrent[BoneChainIndex];
                    Vector3 PreviousSignal;

                    // If bone is not animated, return to last unadulterated pose
                    if (JiggleRigsRuntime[jiggleIndex].Runtimedata.hasTransform[BoneChainIndex])
                    {
                        JiggleRigsRuntime[jiggleIndex].TransformAccessArray[BoneChainIndex].GetLocalPositionAndRotation(out Vector3 localPosition, out Quaternion localrotation);
                        if (JiggleRigsRuntime[jiggleIndex].Runtimedata.boneRotationChangeCheck[BoneChainIndex] == localrotation)
                        {
                            JiggleRigsRuntime[jiggleIndex].TransformAccessArray[BoneChainIndex].localRotation = JiggleRigsRuntime[jiggleIndex].Runtimedata.lastValidPoseBoneRotation[BoneChainIndex];
                        }
                        if (JiggleRigsRuntime[jiggleIndex].Runtimedata.bonePositionChangeCheck[BoneChainIndex] == localPosition)
                        {
                            JiggleRigsRuntime[jiggleIndex].TransformAccessArray[BoneChainIndex].localPosition = JiggleRigsRuntime[jiggleIndex].Runtimedata.lastValidPoseBoneLocalPosition[BoneChainIndex];
                        }
                    }
                    else
                    {
                        // Inline the logic from GetProjectedPositionRuntime
                        Transform parentTransform;
                        int ParentIndex = jiggleRigs[jiggleIndex].JiggleBones[BoneChainIndex].JiggleParentIndex;
                        // Get the parent transform
                        if (ParentIndex != -1)
                        {
                            parentTransform = JiggleRigsRuntime[jiggleIndex].TransformAccessArray[ParentIndex].transform;
                        }
                        else
                        {
                            parentTransform = JiggleRigsRuntime[jiggleIndex].TransformAccessArray[BoneChainIndex].parent;
                        }

                        // Compute the projected position
                        Vector3 PositionOut = parentTransform.InverseTransformPoint(JiggleRigsRuntime[jiggleIndex].TransformAccessArray[ParentIndex].position);
                        CurrentSignal = JiggleRigsRuntime[jiggleIndex].TransformAccessArray[ParentIndex].TransformPoint(PositionOut);

                        PreviousSignal = CurrentSignal;
                        JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalCurrent[BoneChainIndex] = CurrentSignal;
                        JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalPrevious[BoneChainIndex] = PreviousSignal;
                        continue;
                    }

                    PreviousSignal = CurrentSignal;
                    CurrentSignal = JiggleRigsRuntime[jiggleIndex].TransformAccessArray[BoneChainIndex].position;

                    JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalCurrent[BoneChainIndex] = CurrentSignal;
                    JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalPrevious[BoneChainIndex] = PreviousSignal;

                    JiggleRigsRuntime[jiggleIndex].TransformAccessArray[BoneChainIndex].GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
                    JiggleRigsRuntime[jiggleIndex].Runtimedata.lastValidPoseBoneRotation[BoneChainIndex] = Rot;
                    JiggleRigsRuntime[jiggleIndex].Runtimedata.lastValidPoseBoneLocalPosition[BoneChainIndex] = pos;
                }
                JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata = levelOfDetail.AdjustJiggleSettingsData(JigPosition, JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata);
            }

            if (dirtyFromEnable)
            {
                RecordFrame(timeAsDouble);
                for (int jiggleIndex = 0; jiggleIndex < jiggleRigsCount; jiggleIndex++)
                {
                    JiggleRigHelper.FinishTeleport(ref jiggleRigs[jiggleIndex], ref JiggleRigsRuntime[jiggleIndex], simulatedPointsCount[jiggleIndex]);
                }
                dirtyFromEnable = false;
            }

            // Cap accumulation to avoid too many iterations
            accumulation = Math.Min(accumulation + deltaTime, MAX_CATCHUP_TIME);
            double diff;
            diff = currentFrame - previousFrame;
            float percentage = (float)((timeAsDouble - previousFrame) / diff);
            // Update within while loop only when necessary
            if (accumulation > verletTiming)
            {
                do
                {
                    accumulation -= verletTiming;
                    currentFrame = timeAsDouble;
                    // Update each jiggleRig in the same loop to reduce loop overhead
                    for (int jiggleIndex = 0; jiggleIndex < jiggleRigsCount; jiggleIndex++)
                    {
                        // Precompute common values
                        Vector3 gravityEffect = gravity * (JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.gravityMultiplier * squaredDeltaTime);
                        float airDragDeltaTime = VERLET_TIME_STEP * JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.airDrag;
                        float inverseAirDrag = 1f - JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.airDrag;
                        float inverseFriction = 1f - JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.friction;

                        for (int PointIndex = 0; PointIndex < simulatedPointsCount[jiggleIndex]; PointIndex++)
                        {
                            // Cache values for better performance
                            Vector3 currentTargetSignal = JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalCurrent[PointIndex];
                            Vector3 previousTargetSignal = JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalPrevious[PointIndex];

                            Vector3 interpolatedBonePosition = Vector3.Lerp(previousTargetSignal, currentTargetSignal, percentage);
                            JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[PointIndex] = interpolatedBonePosition;

                            if (jiggleRigs[jiggleIndex].JiggleBones[PointIndex].JiggleParentIndex == -1)
                            {
                                JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex] = interpolatedBonePosition;

                                Vector3 particleCurrentSignal = JiggleRigsRuntime[jiggleIndex].Runtimedata.particleSignalCurrent[PointIndex];
                                JiggleRigsRuntime[jiggleIndex].Runtimedata.particleSignalPrevious[PointIndex] = particleCurrentSignal;
                                JiggleRigsRuntime[jiggleIndex].Runtimedata.particleSignalCurrent[PointIndex] = interpolatedBonePosition;

                                continue;
                            }

                            // Cache parent values
                            int parentIndex = jiggleRigs[jiggleIndex].JiggleBones[PointIndex].JiggleParentIndex;
                            Vector3 parentCurrentSignal = JiggleRigsRuntime[jiggleIndex].Runtimedata.particleSignalCurrent[parentIndex];
                            Vector3 parentPreviousSignal = JiggleRigsRuntime[jiggleIndex].Runtimedata.particleSignalPrevious[parentIndex];

                            Vector3 currentSignal = JiggleRigsRuntime[jiggleIndex].Runtimedata.particleSignalCurrent[PointIndex];
                            Vector3 previousSignal = JiggleRigsRuntime[jiggleIndex].Runtimedata.particleSignalPrevious[PointIndex];

                            // Compute deltas
                            Vector3 deltaSignal = currentSignal - previousSignal;
                            Vector3 parentDeltaSignal = parentCurrentSignal - parentPreviousSignal;
                            Vector3 localSpaceVelocity = deltaSignal - parentDeltaSignal;

                            // Update working position
                            JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex] = currentSignal + (deltaSignal - localSpaceVelocity) * inverseAirDrag + localSpaceVelocity * inverseFriction + gravityEffect + wind * airDragDeltaTime;
                        }

                        // Constrain length if needed
                        if (NeedsCollisions[jiggleIndex])
                        {
                            for (int PointIndex = simulatedPointsCount[jiggleIndex] - 1; PointIndex >= 0; PointIndex--)
                            {
                                // Inline implementation of ConstrainLengthBackwards
                                if (jiggleRigs[jiggleIndex].JiggleBones[PointIndex].childIndex != -1)
                                {
                                    Vector3 newPosition = JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex];
                                    Vector3 diffVector = newPosition - JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[jiggleRigs[jiggleIndex].JiggleBones[PointIndex].childIndex];
                                    Vector3 dir = diffVector.normalized;

                                    int ParentIndex = jiggleRigs[jiggleIndex].JiggleBones[PointIndex].JiggleParentIndex;
                                    float lengthToParent = Vector3.Distance(
                                        JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[PointIndex],
                                        JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[ParentIndex]
                                    );

                                    JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex] = Vector3.Lerp(
                                        newPosition,
                                        JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[jiggleRigs[jiggleIndex].JiggleBones[PointIndex].childIndex] + dir * lengthToParent,
                                        JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.lengthElasticity * JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.lengthElasticity * 0.5f
                                    );
                                }
                            }
                        }

                        // Adjust working positions based on parent constraints
                        for (int PointIndex = 0; PointIndex < simulatedPointsCount[jiggleIndex]; PointIndex++)
                        {
                            if (jiggleRigs[jiggleIndex].JiggleBones[PointIndex].JiggleParentIndex == -1 || !JiggleRigsRuntime[jiggleIndex].Runtimedata.hasTransform[PointIndex])
                            {
                                continue;
                            }

                            int parentIndex = jiggleRigs[jiggleIndex].JiggleBones[PointIndex].JiggleParentIndex;
                            int grandParentIndex = jiggleRigs[jiggleIndex].JiggleBones[parentIndex].JiggleParentIndex;

                            Vector3 parentParentPosition;
                            Vector3 poseParentParent;

                            if (grandParentIndex == -1)
                            {
                                poseParentParent = JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[parentIndex] + (JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[parentIndex] - JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[PointIndex]);
                                parentParentPosition = poseParentParent;
                            }
                            else
                            {
                                parentParentPosition = JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[grandParentIndex];
                                poseParentParent = JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[parentIndex];
                            }

                            Vector3 parentAimTargetPose = JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[parentIndex] - poseParentParent;
                            Vector3 parentAim = JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[parentIndex] - parentParentPosition;
                            Quaternion rotationToTargetPose = Quaternion.FromToRotation(parentAimTargetPose, parentAim);

                            Vector3 currentPose = JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[PointIndex] - poseParentParent;
                            Vector3 constraintTarget = rotationToTargetPose * currentPose;

                            int ParentIndex = jiggleRigs[jiggleIndex].JiggleBones[PointIndex].JiggleParentIndex;
                            float lengthToParent = Vector3.Distance(JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[PointIndex], JiggleRigsRuntime[jiggleIndex].Runtimedata.currentFixedAnimatedBonePosition[ParentIndex]);

                            float error = Vector3.Distance(JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex], parentParentPosition + constraintTarget) / lengthToParent;
                            error = Mathf.Clamp01(error);
                            error = Mathf.Pow(error, JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.elasticitySoften * 2f);

                            JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex] = Vector3.Lerp(
                               JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex],
                                parentParentPosition + constraintTarget,
                               JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.angleElasticity * JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.angleElasticity * error);

                            // Constrain Length
                            Vector3 directionToParent = (JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex] - JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[parentIndex]).normalized;
                            JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex] = Vector3.Lerp(
                               JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex],
                               JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[parentIndex] + directionToParent * lengthToParent,
                              JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.lengthElasticity * JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.lengthElasticity);
                        }

                        // Handle collisions
                        if (NeedsCollisions[jiggleIndex])
                        {
                            for (int PointIndex = 0; PointIndex < simulatedPointsCount[jiggleIndex]; PointIndex++)
                            {
                                float radius = jiggleRigs[jiggleIndex].jiggleSettings.GetRadius(JiggleRigsRuntime[jiggleIndex].Runtimedata.normalizedIndex[PointIndex]);
                                if (radius <= 0) continue;

                                jiggleRigs[jiggleIndex].sphereCollider.radius = radius;
                                Vector3 position = JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex];

                                for (int ColliderIndex = 0; ColliderIndex < collidersCount[jiggleIndex]; ColliderIndex++)
                                {
                                    Collider collider = jiggleRigs[jiggleIndex].colliders[ColliderIndex];
                                    collider.transform.GetPositionAndRotation(out Vector3 colliderPosition, out Quaternion colliderRotation);

                                    if (Physics.ComputePenetration(
                                       jiggleRigs[jiggleIndex].sphereCollider,
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

                                JiggleRigsRuntime[jiggleIndex].Runtimedata.workingPosition[PointIndex] = position;
                            }
                        }
                        JobHandle jobHandle = JiggleRigsRuntime[jiggleIndex].SignalJob.Schedule(simulatedPointsCount[jiggleIndex], 64); // You can adjust the batch size (64 here) if needed
                        jobHandle.Complete();
                    }
                } while (accumulation > verletTiming);
            }
            // Final pose loop
            for (int jiggleIndex = 0; jiggleIndex < jiggleRigsCount; jiggleIndex++)
            {
                // jiggleRigs[jiggleIndex].Pose(Percentage);
                Vector3 PreviousSignal = JiggleRigsRuntime[jiggleIndex].Runtimedata.particleSignalPrevious[0];

                JiggleRigsRuntime[jiggleIndex].Runtimedata.extrapolatedPosition[0] = (percentage == 0) ? PreviousSignal : Vector3.Lerp(PreviousSignal, JiggleRigsRuntime[jiggleIndex].Runtimedata.particleSignalCurrent[0], percentage);
                Vector3 offset = JiggleRigsRuntime[jiggleIndex].TransformAccessArray[0].position - JiggleRigsRuntime[jiggleIndex].Runtimedata.extrapolatedPosition[0];

                // Update the job
                JiggleRigsRuntime[jiggleIndex].extrapolationJob.Percentage = percentage;
                JiggleRigsRuntime[jiggleIndex].extrapolationJob.Offset = offset;

                // Schedule the job
                JobHandle jobHandle = JiggleRigsRuntime[jiggleIndex].extrapolationJob.Schedule(simulatedPointsCount[jiggleIndex], 64);

                // Complete the job
                jobHandle.Complete();

                for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount[jiggleIndex]; SimulatedIndex++)
                {
                    if (jiggleRigs[jiggleIndex].JiggleBones[SimulatedIndex].childIndex == -1)
                    {
                        continue; // Early exit if there's no child
                    }
                    int ChildIndex = jiggleRigs[jiggleIndex].JiggleBones[SimulatedIndex].childIndex;
                    // Cache frequently accessed values
                    Vector3 CurrentAnimated = JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalCurrent[SimulatedIndex];
                    Vector3 PreviousAnimated = JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalPrevious[SimulatedIndex];

                    Vector3 ChildCurrentAnimated = JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalCurrent[ChildIndex];
                    Vector3 ChildPreviousAnimated = JiggleRigsRuntime[jiggleIndex].Runtimedata.targetAnimatedBoneSignalPrevious[ChildIndex];

                    Vector3 targetPosition = (percentage == 0) ? PreviousAnimated : Vector3.Lerp(PreviousAnimated, CurrentAnimated, percentage);
                    Vector3 childTargetPosition = (percentage == 0) ? ChildPreviousAnimated : Vector3.Lerp(ChildPreviousAnimated, ChildCurrentAnimated, percentage);
                    // Blend positions
                    Vector3 positionBlend = Vector3.Lerp(targetPosition, JiggleRigsRuntime[jiggleIndex].Runtimedata.extrapolatedPosition[SimulatedIndex], JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.blend);

                    Vector3 childPositionBlend = Vector3.Lerp(childTargetPosition, JiggleRigsRuntime[jiggleIndex].Runtimedata.extrapolatedPosition[ChildIndex], JiggleRigsRuntime[jiggleIndex].jiggleSettingsdata.blend);

                    if (jiggleRigs[jiggleIndex].JiggleBones[SimulatedIndex].JiggleParentIndex != -1)
                    {
                        JiggleRigsRuntime[jiggleIndex].TransformAccessArray[SimulatedIndex].position = positionBlend;
                    }

                    // Calculate child position and vector differences
                    int childIndex = jiggleRigs[jiggleIndex].JiggleBones[SimulatedIndex].childIndex;
                    Vector3 childPosition = JiggleRigHelper.GetTransformPositionRuntime(childIndex, ref jiggleRigs[jiggleIndex], ref JiggleRigsRuntime[jiggleIndex]);
                    Vector3 cachedAnimatedVector = childPosition - positionBlend;
                    Vector3 simulatedVector = childPositionBlend - positionBlend;

                    // Rotate the transform based on the vector differences
                    if (cachedAnimatedVector != VectorZero && simulatedVector != VectorZero)
                    {
                        Quaternion animPoseToPhysicsPose = Quaternion.FromToRotation(cachedAnimatedVector, simulatedVector);
                        JiggleRigsRuntime[jiggleIndex].TransformAccessArray[SimulatedIndex].rotation = animPoseToPhysicsPose * JiggleRigsRuntime[jiggleIndex].TransformAccessArray[SimulatedIndex].rotation;
                    }

                    // Cache transform changes if the bone has a transform
                    if (JiggleRigsRuntime[jiggleIndex].Runtimedata.hasTransform[SimulatedIndex])
                    {
                        JiggleRigsRuntime[jiggleIndex].TransformAccessArray[SimulatedIndex].GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
                        JiggleRigsRuntime[jiggleIndex].Runtimedata.boneRotationChangeCheck[SimulatedIndex] = Rot;
                        JiggleRigsRuntime[jiggleIndex].Runtimedata.bonePositionChangeCheck[SimulatedIndex] = pos;
                    }
                }
            }

            CachedSphereCollider.FinishedPass();
            wasLODActive = true;
        }
        void OnDisable()
        {
            CachedSphereCollider.RemoveBuilder(this);

            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                JiggleRigHelper.PrepareTeleport(ref jiggleRigs[JiggleIndex], ref JiggleRigsRuntime[JiggleIndex], simulatedPointsCount[JiggleIndex]);
            }
        }

        public void PrepareTeleport()
        {
            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                JiggleRigHelper.PrepareTeleport(ref jiggleRigs[JiggleIndex], ref JiggleRigsRuntime[JiggleIndex], simulatedPointsCount[JiggleIndex]);
            }
        }

        public void FinishTeleport(double TimeASDouble)
        {
            RecordFrame(TimeASDouble);
            for (int JiggleIndex = 0; JiggleIndex < jiggleRigsCount; JiggleIndex++)
            {
                JiggleRigHelper.FinishTeleport(ref jiggleRigs[JiggleIndex],ref JiggleRigsRuntime[JiggleIndex], simulatedPointsCount[JiggleIndex]);
            }
        }
        public void FinishTeleport()
        {
            FinishTeleport(Time.timeAsDouble);
        }
        public void RecordFrame(double timeAsDouble)
        {
            previousFrame = timeAsDouble - JiggleRigBuilder.MAX_CATCHUP_TIME * 2f;
            currentFrame = timeAsDouble - JiggleRigBuilder.MAX_CATCHUP_TIME;
        }
    }
}