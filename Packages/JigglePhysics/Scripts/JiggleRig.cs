using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static JiggleRigConstruction;
namespace JigglePhysics
{
    [Serializable]
    public class JiggleRig
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
        public int simulatedPointsCount;
        private bool NeedsCollisions => colliders.Length != 0;
        public JiggleBone[] JiggleBoneIndexes;
        public int collidersCount;
        public Vector3 Zero = Vector3.zero;
        public InitalizationData PreInitalData = new InitalizationData();
        public List<Transform> ComputedTransforms;
        public RuntimeData Runtimedata = new RuntimeData();
        public void Initialize()
        {
            ComputedTransforms = new List<Transform>();
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
            this.simulatedPointsCount = JiggleBoneIndexes.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                int distanceToRoot = 0;
                JiggleBone test = JiggleBoneIndexes[SimulatedIndex];
                while (test.JiggleParentIndex != -1)
                {
                    test = JiggleBoneIndexes[test.JiggleParentIndex];
                    distanceToRoot++;
                }

                int distanceToChild = 0;
                test = JiggleBoneIndexes[SimulatedIndex];
                while (test.childIndex != -1)
                {
                    test = JiggleBoneIndexes[test.childIndex];
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
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                Runtimedata.currentFixedAnimatedBonePosition[SimulatedIndex] = PositionSignalHelper.SamplePosition(Runtimedata.targetAnimatedBoneSignal[SimulatedIndex], TimeAsDouble);
                if (JiggleBoneIndexes[SimulatedIndex].JiggleParentIndex == -1)
                {
                    Runtimedata.workingPosition[SimulatedIndex] = Runtimedata.currentFixedAnimatedBonePosition[SimulatedIndex];
                    var output = Runtimedata.particleSignal[SimulatedIndex];
                    PositionSignalHelper.SetPosition(ref output, Runtimedata.workingPosition[SimulatedIndex], TimeAsDouble);
                    output = Runtimedata.particleSignal[SimulatedIndex];
                    continue;
                }
                Vector3 CurrentSignal = PositionSignalHelper.GetCurrent(Runtimedata.particleSignal[SimulatedIndex]);
                Vector3 PreviousSignal = PositionSignalHelper.GetPrevious(Runtimedata.particleSignal[SimulatedIndex]);
                int JiggleParentindex = JiggleBoneIndexes[SimulatedIndex].JiggleParentIndex;
                Vector3 ParentCurrentSignal = PositionSignalHelper.GetCurrent(Runtimedata.particleSignal[JiggleParentindex]);

                Vector3 ParentPreviousSignal = PositionSignalHelper.GetPrevious(Runtimedata.particleSignal[JiggleParentindex]);

                Vector3 localSpaceVelocity = (CurrentSignal - PreviousSignal) - (ParentCurrentSignal - ParentPreviousSignal);
                Runtimedata.workingPosition[SimulatedIndex] = CurrentSignal + (CurrentSignal - PreviousSignal - localSpaceVelocity) * (1f - jiggleSettingsdata.airDrag) + localSpaceVelocity * (1f - jiggleSettingsdata.friction) + Gravity * (jiggleSettingsdata.gravityMultiplier * squaredDeltaTime);
                Runtimedata.workingPosition[SimulatedIndex] += wind * (fixedDeltaTime * jiggleSettingsdata.airDrag);
            }

            if (NeedsCollisions)
            {
                Vector3 ConstrainLengthBackwards(JiggleBone JiggleBone, Vector3 newPosition, float elasticity)
                {
                    if (JiggleBone.childIndex == -1)
                    {
                        return newPosition;
                    }
                    Vector3 diff = newPosition - Runtimedata.workingPosition[JiggleBone.childIndex];
                    Vector3 dir = diff.normalized;
                    return Vector3.Lerp(newPosition, Runtimedata.workingPosition[JiggleBone.childIndex] + dir * GetLengthToParent(JiggleBone), elasticity);
                }
                for (int Index = simulatedPointsCount - 1; Index >= 0; Index--)
                {
                    Runtimedata.workingPosition[Index] = ConstrainLengthBackwards(JiggleBoneIndexes[Index], Runtimedata.workingPosition[Index], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity * 0.5f);
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (JiggleBoneIndexes[SimulatedIndex].JiggleParentIndex == -1)
                {
                    continue;
                }
                Vector3 ConstrainAngle(JiggleBone JiggleBone, Vector3 newPosition, float elasticity, float elasticitySoften)
                {
                    if (!Runtimedata.hasTransform[JiggleBone.boneIndex])
                    {
                        return newPosition;
                    }
                    Vector3 parentParentPosition;
                    Vector3 poseParentParent;
                    int ParentIndex = JiggleBone.JiggleParentIndex;
                    int ParentsParentIndex = JiggleBoneIndexes[ParentIndex].JiggleParentIndex;
                    if (ParentsParentIndex == -1)
                    {
                        poseParentParent = Runtimedata.currentFixedAnimatedBonePosition[ParentIndex] + (Runtimedata.currentFixedAnimatedBonePosition[ParentIndex] - Runtimedata.currentFixedAnimatedBonePosition[JiggleBone.boneIndex]);
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
                    Vector3 currentPose = Runtimedata.currentFixedAnimatedBonePosition[JiggleBone.boneIndex] - poseParentParent;
                    Vector3 constraintTarget = TargetPoseToPose * currentPose;
                    float error = Vector3.Distance(newPosition, parentParentPosition + constraintTarget);
                    error /= GetLengthToParent(JiggleBone);
                    error = Mathf.Clamp01(error);
                    error = Mathf.Pow(error, elasticitySoften * 2f);
                    return Vector3.Lerp(newPosition, parentParentPosition + constraintTarget, elasticity * error);
                }
                Vector3 ConstrainLength(JiggleBone JiggleBone, Vector3 newPosition, float elasticity)
                {
                    int Index = JiggleBone.JiggleParentIndex;
                    Vector3 diff = newPosition - Runtimedata.workingPosition[Index];
                    Vector3 dir = diff.normalized;
                    return Vector3.Lerp(newPosition, Runtimedata.workingPosition[Index] + dir * GetLengthToParent(JiggleBone), elasticity);
                }
                Runtimedata.workingPosition[SimulatedIndex] = ConstrainAngle(JiggleBoneIndexes[SimulatedIndex], Runtimedata.workingPosition[SimulatedIndex], jiggleSettingsdata.angleElasticity * jiggleSettingsdata.angleElasticity, jiggleSettingsdata.elasticitySoften);
                Runtimedata.workingPosition[SimulatedIndex] = ConstrainLength(JiggleBoneIndexes[SimulatedIndex], Runtimedata.workingPosition[SimulatedIndex], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity);
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
                PositionSignalHelper.SetPosition(ref output, Runtimedata.workingPosition[SimulatedIndex], TimeAsDouble);
                Runtimedata.particleSignal[SimulatedIndex] = output;
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
                if (Runtimedata.hasTransform[JiggleBoneIndexes[PointIndex].boneIndex])
                {
                    ComputedTransforms[JiggleBoneIndexes[PointIndex].boneIndex].GetLocalPositionAndRotation(out Vector3 localPosition, out Quaternion localrotation);
                    if (Runtimedata.boneRotationChangeCheck[JiggleBoneIndexes[PointIndex].boneIndex] == localrotation)
                    {
                        ComputedTransforms[JiggleBoneIndexes[PointIndex].boneIndex].localRotation = Runtimedata.lastValidPoseBoneRotation[JiggleBoneIndexes[PointIndex].boneIndex];
                    }
                    if (Runtimedata.bonePositionChangeCheck[JiggleBoneIndexes[PointIndex].boneIndex] == localPosition)
                    {
                        ComputedTransforms[JiggleBoneIndexes[PointIndex].boneIndex].localPosition = Runtimedata.lastValidPoseBoneLocalPosition[JiggleBoneIndexes[PointIndex].boneIndex];
                    }
                }
                if (!Runtimedata.hasTransform[JiggleBoneIndexes[PointIndex].boneIndex])
                {
                    PositionSignal BoneSignalHas = Runtimedata.targetAnimatedBoneSignal[JiggleBoneIndexes[PointIndex].boneIndex];
                    int ParentIndex = JiggleBoneIndexes[PointIndex].JiggleParentIndex;
                    PositionSignalHelper.SetPosition(ref BoneSignalHas, GetProjectedPosition(JiggleBoneIndexes[PointIndex], ParentIndex), timeAsDouble);
                    Runtimedata.targetAnimatedBoneSignal[JiggleBoneIndexes[PointIndex].boneIndex] = BoneSignalHas;
                    continue;
                }
                var BoneSignal = Runtimedata.targetAnimatedBoneSignal[JiggleBoneIndexes[PointIndex].boneIndex];
                PositionSignalHelper.SetPosition(ref BoneSignal, ComputedTransforms[JiggleBoneIndexes[PointIndex].boneIndex].position, timeAsDouble);
                Runtimedata.targetAnimatedBoneSignal[JiggleBoneIndexes[PointIndex].boneIndex] = BoneSignal;
                ComputedTransforms[JiggleBoneIndexes[PointIndex].boneIndex].GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
                Runtimedata.lastValidPoseBoneRotation[JiggleBoneIndexes[PointIndex].boneIndex] = Rot;
                Runtimedata.lastValidPoseBoneLocalPosition[JiggleBoneIndexes[PointIndex].boneIndex] = pos;
            }
            jiggleSettingsdata = jiggleSettings.GetData();
            jiggleSettingsdata = jiggleRigLOD != null ? jiggleRigLOD.AdjustJiggleSettingsData(position, jiggleSettingsdata) : jiggleSettingsdata;
        }
        public void Pose(double timeAsDouble)
        {
            Runtimedata.extrapolatedPosition[0] = PositionSignalHelper.SamplePosition(Runtimedata.particleSignal[0], timeAsDouble);

            Vector3 virtualPosition = Runtimedata.extrapolatedPosition[0];

            Vector3 offset = ComputedTransforms[0].transform.position - virtualPosition;
            int simulatedPointsLength = JiggleBoneIndexes.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsLength; SimulatedIndex++)
            {
                Runtimedata.extrapolatedPosition[SimulatedIndex] = offset + PositionSignalHelper.SamplePosition(Runtimedata.particleSignal[SimulatedIndex], timeAsDouble);
            }

            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (JiggleBoneIndexes[SimulatedIndex].childIndex == -1)
                {
                    continue; // Early exit if there's no child
                }
                int ChildIndex = JiggleBoneIndexes[SimulatedIndex].childIndex;
                // Cache frequently accessed values
                Vector3 targetPosition = PositionSignalHelper.SamplePosition(Runtimedata.targetAnimatedBoneSignal[SimulatedIndex], timeAsDouble);
                Vector3 childTargetPosition = PositionSignalHelper.SamplePosition(Runtimedata.targetAnimatedBoneSignal[ChildIndex], timeAsDouble);
                // Blend positions
                Vector3 positionBlend = Vector3.Lerp(targetPosition, Runtimedata.extrapolatedPosition[SimulatedIndex], jiggleSettingsdata.blend);

                Vector3 childPositionBlend = Vector3.Lerp(childTargetPosition, Runtimedata.extrapolatedPosition[ChildIndex], jiggleSettingsdata.blend);

                if (JiggleBoneIndexes[SimulatedIndex].JiggleParentIndex != -1)
                {
                    ComputedTransforms[SimulatedIndex].position = positionBlend;
                }

                // Calculate child position and vector differences
                int childIndex = JiggleBoneIndexes[SimulatedIndex].childIndex;
                Vector3 childPosition = GetTransformPosition(JiggleBoneIndexes[childIndex]);
                Vector3 cachedAnimatedVector = childPosition - positionBlend;
                Vector3 simulatedVector = childPositionBlend - positionBlend;

                // Rotate the transform based on the vector differences
                if (cachedAnimatedVector != Vector3.zero && simulatedVector != Vector3.zero)
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
        /// <summary>
        /// JiggleParent = JiggleBone.JiggleParent
        /// </summary>
        /// <param name="JiggleBone"></param>
        /// <param name="JiggleParent"></param>
        /// <returns></returns>
        public Vector3 GetProjectedPosition(JiggleBone JiggleBone, int JiggleParent)
        {
            Transform GetParentTransform(JiggleBone JiggleBone)
            {
                if (JiggleBone.JiggleParentIndex != -1)
                {
                    int ParentIndex = JiggleBone.JiggleParentIndex;

                    return ComputedTransforms[ParentIndex].transform;
                }
                return ComputedTransforms[JiggleBone.boneIndex].parent;
            }
            return ComputedTransforms[JiggleParent].TransformPoint(GetParentTransform(JiggleBone).InverseTransformPoint(ComputedTransforms[JiggleParent].position));
        }
        public Vector3 GetTransformPosition(JiggleBone JiggleBone)
        {
            if (!Runtimedata.hasTransform[JiggleBone.boneIndex])
            {
                return GetProjectedPosition(JiggleBone, JiggleBone.JiggleParentIndex);
            }
            else
            {
                return ComputedTransforms[JiggleBone.boneIndex].position;
            }
        }
        public float GetLengthToParent(JiggleBone JiggleBone)
        {
            int ParentIndex = JiggleBone.JiggleParentIndex;
            return Vector3.Distance(Runtimedata.currentFixedAnimatedBonePosition[JiggleBone.boneIndex], Runtimedata.currentFixedAnimatedBonePosition[ParentIndex]);
        }
        public void MatchAnimationInstantly(JiggleBone JiggleBone, double time)
        {
            Vector3 position = GetTransformPosition(JiggleBone);
            var outputA = Runtimedata.targetAnimatedBoneSignal[JiggleBone.boneIndex];
            var outputB = Runtimedata.particleSignal[JiggleBone.boneIndex];
            PositionSignalHelper.FlattenSignal(ref outputA, time, position);
            PositionSignalHelper.FlattenSignal(ref outputB, time, position);

            Runtimedata.targetAnimatedBoneSignal[JiggleBone.boneIndex] = outputA;
            Runtimedata.particleSignal[JiggleBone.boneIndex] = outputB;
        }
        /// <summary>
        /// Physically accurate teleportation, maintains the existing signals of motion and keeps their trajectories through a teleport. First call PrepareTeleport(), then move the character, then call FinishTeleport().
        /// Use MatchAnimationInstantly() instead if you don't want jiggles to be maintained through a teleport.
        /// </summary>
        public void PrepareTeleport(JiggleBone JiggleBone)
        {
            Runtimedata.preTeleportPosition[JiggleBone.boneIndex] = GetTransformPosition(JiggleBone);
        }
        public void PrepareTeleport()
        {
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                PrepareTeleport(JiggleBoneIndexes[PointsIndex]);
            }
        }
        /// <summary>
        /// The companion function to PrepareTeleport, it discards all the movement that has happened since the call to PrepareTeleport, assuming that they've both been called on the same frame.
        /// </summary>
        public void FinishTeleport(double timeAsDouble)
        {
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                Vector3 position = GetTransformPosition(JiggleBoneIndexes[PointsIndex]);
                Vector3 diff = position - Runtimedata.preTeleportPosition[JiggleBoneIndexes[PointsIndex].boneIndex];
                var outputA = Runtimedata.targetAnimatedBoneSignal[JiggleBoneIndexes[PointsIndex].boneIndex];
                var outputB = Runtimedata.particleSignal[JiggleBoneIndexes[PointsIndex].boneIndex];
                PositionSignalHelper.FlattenSignal(ref outputA, timeAsDouble, position);
                PositionSignalHelper.OffsetSignal(ref outputB, diff);
                Runtimedata.targetAnimatedBoneSignal[JiggleBoneIndexes[PointsIndex].boneIndex] = outputA;
                Runtimedata.particleSignal[JiggleBoneIndexes[PointsIndex].boneIndex] = outputB;
                Runtimedata.workingPosition[JiggleBoneIndexes[PointsIndex].boneIndex] += diff;
            }
        }
    }
}