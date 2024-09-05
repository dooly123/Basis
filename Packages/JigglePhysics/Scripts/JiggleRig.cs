using System;
using Unity.Collections;
using UnityEngine;
using static JiggleRigConstruction;
namespace JigglePhysics
{
    [Serializable]
    public class JiggleRig : JiggleRigBase
    {
        [SerializeField]
        [Tooltip("The root bone from which an individual JiggleRig will be constructed. The JiggleRig encompasses all children of the specified root.")]
        public Transform rootTransform;
        [Tooltip("The settings that the rig should update with, create them using the Create->JigglePhysics->Settings menu option.")]
        public JiggleSettingsBase jiggleSettings;
        [SerializeField]
        [Tooltip("The list of transforms to ignore during the jiggle. Each bone listed will also ignore all the children of the specified bone.")]
        public Transform[] ignoredTransforms;
        public Collider[] colliders;
        [SerializeField]
        public JiggleSettingsData jiggleSettingsdata;
        public Transform GetRootTransform() => rootTransform;
        private bool NeedsCollisions => colliders.Length != 0;
        public int collidersCount;
        public Vector3 Zero = Vector3.zero;
        public void Initialize()
        {
            JiggleRigConstruction.InitalizeLists(this);
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
        }

        private void InitializeNativeArrays()
        {
            // Consolidate NativeArray initialization into one method
            Runtimedata.boneRotationChangeCheck = new NativeArray<Quaternion>(PreInitalData.boneRotationChangeCheck.ToArray(), Allocator.Persistent);
            Runtimedata.lastValidPoseBoneRotation = new NativeArray<Quaternion>(PreInitalData.boneRotationChangeCheck.ToArray(), Allocator.Persistent);
            Runtimedata.currentFixedAnimatedBonePosition = new NativeArray<Vector3>(PreInitalData.currentFixedAnimatedBonePosition.ToArray(), Allocator.Persistent);
            Runtimedata.bonePositionChangeCheck = new NativeArray<Vector3>(PreInitalData.bonePositionChangeCheck.ToArray(), Allocator.Persistent);
            Runtimedata.lastValidPoseBoneLocalPosition = new NativeArray<Vector3>(PreInitalData.lastValidPoseBoneLocalPosition.ToArray(), Allocator.Persistent);
            Runtimedata.workingPosition = new NativeArray<Vector3>(PreInitalData.workingPosition.ToArray(), Allocator.Persistent);
            Runtimedata.preTeleportPosition = new NativeArray<Vector3>(PreInitalData.preTeleportPosition.ToArray(), Allocator.Persistent);
            Runtimedata.extrapolatedPosition = new NativeArray<Vector3>(PreInitalData.extrapolatedPosition.ToArray(), Allocator.Persistent);
            Runtimedata.hasTransform = new NativeArray<bool>(PreInitalData.hasTransform.ToArray(), Allocator.Persistent);
            Runtimedata.normalizedIndex = new NativeArray<float>(PreInitalData.normalizedIndex.ToArray(), Allocator.Persistent);
            Runtimedata.targetAnimatedBoneSignal = new NativeArray<PositionSignal>(PreInitalData.targetAnimatedBoneSignal.ToArray(), Allocator.Persistent);
            Runtimedata.particleSignal = new NativeArray<PositionSignal>(PreInitalData.particleSignal.ToArray(), Allocator.Persistent);
        }
        public Vector3 ConstrainLengthBackwards(int JiggleIndex, Vector3 newPosition, float elasticity)
        {
            if (JiggleBones[JiggleIndex].childIndex == -1)
            {
                return newPosition;
            }

            Vector3 diff = newPosition - Runtimedata.workingPosition[JiggleBones[JiggleIndex].childIndex];
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, Runtimedata.workingPosition[JiggleBones[JiggleIndex].childIndex] + dir * GetLengthToParent(JiggleIndex), elasticity);
        }
        public void Update(Vector3 wind, double TimeAsDouble, float fixedDeltaTime, float squaredDeltaTime, Vector3 Gravity)
        {
            Vector3 gravityEffect = Gravity * (jiggleSettingsdata.gravityMultiplier * squaredDeltaTime);
            float airDragDeltaTime = fixedDeltaTime * jiggleSettingsdata.airDrag;
            float inverseAirDrag = 1f - jiggleSettingsdata.airDrag;
            float inverseFriction = 1f - jiggleSettingsdata.friction;

            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                // Cache values for better performance
                Vector3 currentFixedAnimatedBonePosition = SamplePosition(Runtimedata.targetAnimatedBoneSignal[SimulatedIndex], TimeAsDouble);
                Runtimedata.currentFixedAnimatedBonePosition[SimulatedIndex] = currentFixedAnimatedBonePosition;

                if (JiggleBones[SimulatedIndex].JiggleParentIndex == -1)
                {
                    Runtimedata.workingPosition[SimulatedIndex] = currentFixedAnimatedBonePosition;

                    PositionSignal output = Runtimedata.particleSignal[SimulatedIndex];
                    SetPosition(ref output, currentFixedAnimatedBonePosition, TimeAsDouble);
                    Runtimedata.particleSignal[SimulatedIndex] = output;
                    continue;
                }

                // Cache signals for better performance
                Vector3 currentSignal = Runtimedata.particleSignal[SimulatedIndex].currentFrame.position;
                Vector3 previousSignal = Runtimedata.particleSignal[SimulatedIndex].previousFrame.position;

                int parentIndex = JiggleBones[SimulatedIndex].JiggleParentIndex;
                Vector3 parentCurrentSignal = Runtimedata.particleSignal[parentIndex].currentFrame.position;
                Vector3 parentPreviousSignal = Runtimedata.particleSignal[parentIndex].previousFrame.position;

                // Precompute deltas
                Vector3 deltaSignal = currentSignal - previousSignal;
                Vector3 parentDeltaSignal = parentCurrentSignal - parentPreviousSignal;

                // Calculate local space velocity
                Vector3 localSpaceVelocity = deltaSignal - parentDeltaSignal;

                // Update working position using the precomputed values
                Vector3 workingPosition = currentSignal + (deltaSignal - localSpaceVelocity) * inverseAirDrag + localSpaceVelocity * inverseFriction + gravityEffect;
                workingPosition += wind * airDragDeltaTime;
                Runtimedata.workingPosition[SimulatedIndex] = workingPosition;
            }
            if (NeedsCollisions)
            {
                for (int Index = simulatedPointsCount - 1; Index >= 0; Index--)
                {
                    Runtimedata.workingPosition[Index] = ConstrainLengthBackwards(Index, Runtimedata.workingPosition[Index], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity * 0.5f);
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (JiggleBones[SimulatedIndex].JiggleParentIndex == -1)
                {
                    continue;
                }
                Vector3 ConstrainAngle(int JiggleIndex, Vector3 newPosition, float elasticity, float elasticitySoften)
                {
                    if (!Runtimedata.hasTransform[JiggleIndex])
                    {
                        return newPosition;
                    }
                    Vector3 parentParentPosition;
                    Vector3 poseParentParent;
                    int ParentIndex = JiggleBones[JiggleIndex].JiggleParentIndex;
                    int ParentsParentIndex = this.JiggleBones[ParentIndex].JiggleParentIndex;
                    if (ParentsParentIndex == -1)
                    {
                        poseParentParent = Runtimedata.currentFixedAnimatedBonePosition[ParentIndex] + (Runtimedata.currentFixedAnimatedBonePosition[ParentIndex] - Runtimedata.currentFixedAnimatedBonePosition[JiggleIndex]);
                        parentParentPosition = poseParentParent;
                    }
                    else
                    {
                        parentParentPosition = Runtimedata.workingPosition[ParentsParentIndex];
                        poseParentParent = Runtimedata.currentFixedAnimatedBonePosition[ParentsParentIndex];
                    }
                    Vector3 parentAimTargetPose = Runtimedata.currentFixedAnimatedBonePosition[ParentIndex] - poseParentParent;
                    Vector3 parentAim = Runtimedata.workingPosition[ParentIndex] - parentParentPosition;
                    Quaternion TargetPoseToPose = Quaternion.FromToRotation(parentAimTargetPose, parentAim);
                    Vector3 currentPose = Runtimedata.currentFixedAnimatedBonePosition[JiggleIndex] - poseParentParent;
                    Vector3 constraintTarget = TargetPoseToPose * currentPose;
                    float error = Vector3.Distance(newPosition, parentParentPosition + constraintTarget);
                    error /= GetLengthToParent(JiggleIndex);
                    error = Mathf.Clamp01(error);
                    error = Mathf.Pow(error, elasticitySoften * 2f);
                    return Vector3.Lerp(newPosition, parentParentPosition + constraintTarget, elasticity * error);
                }
                Vector3 ConstrainLength(int JiggleIndex, Vector3 newPosition, float elasticity)
                {
                    int Index = JiggleBones[JiggleIndex].JiggleParentIndex;
                    Vector3 diff = newPosition - Runtimedata.workingPosition[Index];
                    Vector3 dir = diff.normalized;
                    return Vector3.Lerp(newPosition, Runtimedata.workingPosition[Index] + dir * GetLengthToParent(JiggleIndex), elasticity);
                }
                Runtimedata.workingPosition[SimulatedIndex] = ConstrainAngle(SimulatedIndex, Runtimedata.workingPosition[SimulatedIndex], jiggleSettingsdata.angleElasticity * jiggleSettingsdata.angleElasticity, jiggleSettingsdata.elasticitySoften);
                Runtimedata.workingPosition[SimulatedIndex] = ConstrainLength(SimulatedIndex, Runtimedata.workingPosition[SimulatedIndex], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity);
            }
            if (NeedsCollisions)
            {
                for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
                {
                    if (!CachedSphereCollider.TryGet(out SphereCollider sphereCollider))
                    {
                        continue;
                    }
                    for (int ColliderIndex = 0; ColliderIndex < collidersCount; ColliderIndex++)
                    {
                        sphereCollider.radius = jiggleSettings.GetRadius(Runtimedata.normalizedIndex[SimulatedIndex]);
                        if (sphereCollider.radius <= 0)
                        {
                            continue;
                        }
                        Collider collider = colliders[ColliderIndex];
                        collider.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                        if (Physics.ComputePenetration(sphereCollider, Runtimedata.workingPosition[SimulatedIndex], Quaternion.identity, collider, position, rotation, out Vector3 dir, out float dist))
                        {
                            Runtimedata.workingPosition[SimulatedIndex] += dir * dist;
                        }
                    }
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                var output = Runtimedata.particleSignal[SimulatedIndex];
                SetPosition(ref output, Runtimedata.workingPosition[SimulatedIndex], TimeAsDouble);
                Runtimedata.particleSignal[SimulatedIndex] = output;
            }
        }
        public void PrepareBone(Vector3 position, JiggleRigLOD jiggleRigLOD, double timeAsDouble)
        {
            for (int PointIndex = 0; PointIndex < simulatedPointsCount; PointIndex++)
            {
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
                if (!Runtimedata.hasTransform[PointIndex])
                {
                    PositionSignal BoneSignalHas = Runtimedata.targetAnimatedBoneSignal[PointIndex];
                    int ParentIndex = JiggleBones[PointIndex].JiggleParentIndex;
                    SetPosition(ref BoneSignalHas, GetProjectedPosition(PointIndex, ParentIndex), timeAsDouble);
                    Runtimedata.targetAnimatedBoneSignal[PointIndex] = BoneSignalHas;
                    continue;
                }
                var BoneSignal = Runtimedata.targetAnimatedBoneSignal[PointIndex];
                SetPosition(ref BoneSignal, ComputedTransforms[PointIndex].position, timeAsDouble);
                Runtimedata.targetAnimatedBoneSignal[PointIndex] = BoneSignal;
                ComputedTransforms[PointIndex].GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
                Runtimedata.lastValidPoseBoneRotation[PointIndex] = Rot;
                Runtimedata.lastValidPoseBoneLocalPosition[PointIndex] = pos;
            }
            jiggleSettingsdata = jiggleRigLOD != null ? jiggleRigLOD.AdjustJiggleSettingsData(position, jiggleSettingsdata) : jiggleSettingsdata;
        }
        public void Pose(double timeAsDouble)
        {
            Runtimedata.extrapolatedPosition[0] = SamplePosition(Runtimedata.particleSignal[0], timeAsDouble);

            Vector3 virtualPosition = Runtimedata.extrapolatedPosition[0];

            Vector3 offset = ComputedTransforms[0].transform.position - virtualPosition;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                Runtimedata.extrapolatedPosition[SimulatedIndex] = offset + SamplePosition(Runtimedata.particleSignal[SimulatedIndex], timeAsDouble);
            }

            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (JiggleBones[SimulatedIndex].childIndex == -1)
                {
                    continue; // Early exit if there's no child
                }
                int ChildIndex = JiggleBones[SimulatedIndex].childIndex;
                // Cache frequently accessed values
                Vector3 targetPosition = SamplePosition(Runtimedata.targetAnimatedBoneSignal[SimulatedIndex], timeAsDouble);
                Vector3 childTargetPosition = SamplePosition(Runtimedata.targetAnimatedBoneSignal[ChildIndex], timeAsDouble);
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
        public void SetPosition(ref PositionSignal signal, Vector3 position, double time)
        {
            signal.previousFrame = signal.currentFrame;
            signal.currentFrame = new Frame
            {
                position = position,
                time = time,
            };
        }
        public Vector3 SamplePosition(PositionSignal signal, double time)
        {
            double diff = signal.currentFrame.time - signal.previousFrame.time;
            if (diff == 0)
            {
                return signal.previousFrame.position;
            }
            double t = (time - signal.previousFrame.time) / diff;
            return Vector3.Lerp(signal.previousFrame.position, signal.currentFrame.position, (float)t);
        }
    }
}