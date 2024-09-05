using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
        private bool initialized;
        public Transform GetRootTransform() => rootTransform;
        private bool NeedsCollisions => colliders.Length != 0;
        public int collidersCount;
        public Vector3 Zero = Vector3.zero;
        public void Initialize()
        {
            ComputedTransforms = new Transform[] { };
            PreInitalData.boneRotationChangeCheck = new List<Quaternion>();
            PreInitalData.lastValidPoseBoneRotation = new List<Quaternion>();
            PreInitalData.currentFixedAnimatedBonePosition = new List<Vector3>();
            PreInitalData.bonePositionChangeCheck = new List<Vector3>();
            PreInitalData.lastValidPoseBoneLocalPosition = new List<Vector3>();
            PreInitalData.workingPosition = new List<Vector3>();
            PreInitalData.preTeleportPosition = new List<Vector3>();
            PreInitalData.extrapolatedPosition = new List<Vector3>();
            PreInitalData.hasTransform = new List<bool>();
            PreInitalData.normalizedIndex = new List<float>();
            PreInitalData.targetAnimatedBoneSignal = new List<PositionSignal>();
            PreInitalData.particleSignal = new List<PositionSignal>();

            CreateSimulatedPoints(this, ignoredTransforms, rootTransform, null);
            this.simulatedPointsCount = JiggleBones.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                int distanceToRoot = 0;
                JiggleBone test = JiggleBones[SimulatedIndex];
                while (test.JiggleParentIndex != -1)
                {
                    test = JiggleBones[test.JiggleParentIndex];
                    distanceToRoot++;
                }

                int distanceToChild = 0;
                test = JiggleBones[SimulatedIndex];
                while (test.childIndex != -1)
                {
                    test = JiggleBones[test.childIndex];
                    distanceToChild++;
                }

                int max = distanceToRoot + distanceToChild;
                float frac = (float)distanceToRoot / max;
                PreInitalData.normalizedIndex[SimulatedIndex] = frac;
            }
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
            initialized = true;
        }
        public void Update(Vector3 wind, double TimeAsDouble, float fixedDeltaTime, Vector3 Gravity)
        {
            float squaredDeltaTime = fixedDeltaTime * fixedDeltaTime;
            Vector3 gravityEffect = Gravity * (jiggleSettingsdata.gravityMultiplier * squaredDeltaTime);
            float airDragDeltaTime = fixedDeltaTime * jiggleSettingsdata.airDrag;

            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                Runtimedata.currentFixedAnimatedBonePosition[SimulatedIndex] = SamplePosition(Runtimedata.targetAnimatedBoneSignal[SimulatedIndex], TimeAsDouble);
                if (JiggleBones[SimulatedIndex].JiggleParentIndex == -1)
                {
                    Runtimedata.workingPosition[SimulatedIndex] = Runtimedata.currentFixedAnimatedBonePosition[SimulatedIndex];
                    var output = Runtimedata.particleSignal[SimulatedIndex];
                    SetPosition(ref output, Runtimedata.workingPosition[SimulatedIndex], TimeAsDouble);
                    Runtimedata.particleSignal[SimulatedIndex] = output;
                    continue;
                }

                Vector3 CurrentSignal = GetCurrent(Runtimedata.particleSignal[SimulatedIndex]);
                Vector3 PreviousSignal = GetPrevious(Runtimedata.particleSignal[SimulatedIndex]);
                int JiggleParentindex = JiggleBones[SimulatedIndex].JiggleParentIndex;
                Vector3 ParentCurrentSignal = GetCurrent(Runtimedata.particleSignal[JiggleParentindex]);
                Vector3 ParentPreviousSignal = GetPrevious(Runtimedata.particleSignal[JiggleParentindex]);
                Vector3 localSpaceVelocity = (CurrentSignal - PreviousSignal) - (ParentCurrentSignal - ParentPreviousSignal);
                Runtimedata.workingPosition[SimulatedIndex] = CurrentSignal + (CurrentSignal - PreviousSignal - localSpaceVelocity) * (1f - jiggleSettingsdata.airDrag) + localSpaceVelocity * (1f - jiggleSettingsdata.friction)+ gravityEffect;
                Runtimedata.workingPosition[SimulatedIndex] += wind * airDragDeltaTime;
            }

            if (NeedsCollisions)
            {
                Vector3 ConstrainLengthBackwards(int JiggleIndex, Vector3 newPosition, float elasticity)
                {
                    if (JiggleBones[JiggleIndex].childIndex == -1)
                    {
                        return newPosition;
                    }
                    Vector3 diff = newPosition - Runtimedata.workingPosition[JiggleBones[JiggleIndex].childIndex];
                    Vector3 dir = diff.normalized;
                    return Vector3.Lerp(newPosition, Runtimedata.workingPosition[JiggleBones[JiggleIndex].childIndex] + dir * GetLengthToParent(JiggleIndex), elasticity);
                }
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
        // Define a job for updating positions
        [BurstCompile]
        struct JiggleUpdateJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] public NativeArray<Vector3> workingPosition;
            [ReadOnly] public NativeArray<Vector3> currentFixedAnimatedBonePosition;
            [ReadOnly] public NativeArray<PositionSignal> particleSignal;
            [ReadOnly] public NativeArray<float> normalizedIndex;
            public Vector3 wind;
            public Vector3 gravity;
            public float squaredDeltaTime;
            public JiggleSettingsData jiggleSettingsData;
            public void Execute(int index)
            {
                // Perform necessary calculations
                Vector3 CurrentSignal = particleSignal[index].currentFrame.position;
                Vector3 PreviousSignal = particleSignal[index].previousFrame.position;
                Vector3 velocity = (CurrentSignal - PreviousSignal);
                workingPosition[index] = CurrentSignal + velocity * (1f - jiggleSettingsData.airDrag) + gravity * (jiggleSettingsData.gravityMultiplier * squaredDeltaTime);
                workingPosition[index] += wind * jiggleSettingsData.airDrag * squaredDeltaTime;
            }
        }
        public void PrepareBone(Vector3 position, JiggleRigLOD jiggleRigLOD, double timeAsDouble)
        {
            if (!initialized)
            {
                throw new UnityException("JiggleRig was never initialized. Please call JiggleRig.Initialize() if you're going to manually timestep.");
            }
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
            jiggleSettingsdata = jiggleSettings.GetData();
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

        public Vector3 GetCurrent(PositionSignal signal)
        {
            return signal.currentFrame.position;
        }

        public Vector3 GetPrevious(PositionSignal signal)
        {
            return signal.previousFrame.position;
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