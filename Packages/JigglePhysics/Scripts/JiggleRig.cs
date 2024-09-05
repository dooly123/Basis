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
        public JiggleBone[] JiggleBones;
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
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                Runtimedata.currentFixedAnimatedBonePosition[SimulatedIndex] = PositionSignalHelper.SamplePosition(Runtimedata.targetAnimatedBoneSignal[SimulatedIndex], TimeAsDouble);
                if (JiggleBones[SimulatedIndex].JiggleParentIndex == -1)
                {
                    Runtimedata.workingPosition[SimulatedIndex] = Runtimedata.currentFixedAnimatedBonePosition[SimulatedIndex];
                    var output = Runtimedata.particleSignal[SimulatedIndex];
                    PositionSignalHelper.SetPosition(ref output, Runtimedata.workingPosition[SimulatedIndex], TimeAsDouble);
                    output = Runtimedata.particleSignal[SimulatedIndex];
                    continue;
                }
                Vector3 CurrentSignal = PositionSignalHelper.GetCurrent(Runtimedata.particleSignal[SimulatedIndex]);
                Vector3 PreviousSignal = PositionSignalHelper.GetPrevious(Runtimedata.particleSignal[SimulatedIndex]);
                int JiggleParentindex = JiggleBones[SimulatedIndex].JiggleParentIndex;
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
                    Runtimedata.workingPosition[Index] = ConstrainLengthBackwards(JiggleBones[Index], Runtimedata.workingPosition[Index], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity * 0.5f);
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (JiggleBones[SimulatedIndex].JiggleParentIndex == -1)
                {
                    continue;
                }
                Vector3 ConstrainAngle(JiggleBone JiggleBone, Vector3 newPosition, float elasticity, float elasticitySoften)
                {
                    int Index = Array.IndexOf(JiggleBones, JiggleBone);
                    if (!Runtimedata.hasTransform[Index])
                    {
                        return newPosition;
                    }
                    Vector3 parentParentPosition;
                    Vector3 poseParentParent;
                    int ParentIndex = JiggleBone.JiggleParentIndex;
                    int ParentsParentIndex = this.JiggleBones[ParentIndex].JiggleParentIndex;
                    if (ParentsParentIndex == -1)
                    {
                        poseParentParent = Runtimedata.currentFixedAnimatedBonePosition[ParentIndex] + (Runtimedata.currentFixedAnimatedBonePosition[ParentIndex] - Runtimedata.currentFixedAnimatedBonePosition[Index]);
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
                    Vector3 currentPose = Runtimedata.currentFixedAnimatedBonePosition[Index] - poseParentParent;
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
                Runtimedata.workingPosition[SimulatedIndex] = ConstrainAngle(JiggleBones[SimulatedIndex], Runtimedata.workingPosition[SimulatedIndex], jiggleSettingsdata.angleElasticity * jiggleSettingsdata.angleElasticity, jiggleSettingsdata.elasticitySoften);
                Runtimedata.workingPosition[SimulatedIndex] = ConstrainLength(JiggleBones[SimulatedIndex], Runtimedata.workingPosition[SimulatedIndex], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity);
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
                    PositionSignalHelper.SetPosition(ref BoneSignalHas, GetProjectedPosition(PointIndex, ParentIndex), timeAsDouble);
                    Runtimedata.targetAnimatedBoneSignal[PointIndex] = BoneSignalHas;
                    continue;
                }
                var BoneSignal = Runtimedata.targetAnimatedBoneSignal[PointIndex];
                PositionSignalHelper.SetPosition(ref BoneSignal, ComputedTransforms[PointIndex].position, timeAsDouble);
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
            Runtimedata.extrapolatedPosition[0] = PositionSignalHelper.SamplePosition(Runtimedata.particleSignal[0], timeAsDouble);

            Vector3 virtualPosition = Runtimedata.extrapolatedPosition[0];

            Vector3 offset = ComputedTransforms[0].transform.position - virtualPosition;
            int simulatedPointsLength = JiggleBones.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsLength; SimulatedIndex++)
            {
                Runtimedata.extrapolatedPosition[SimulatedIndex] = offset + PositionSignalHelper.SamplePosition(Runtimedata.particleSignal[SimulatedIndex], timeAsDouble);
            }

            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (JiggleBones[SimulatedIndex].childIndex == -1)
                {
                    continue; // Early exit if there's no child
                }
                int ChildIndex = JiggleBones[SimulatedIndex].childIndex;
                // Cache frequently accessed values
                Vector3 targetPosition = PositionSignalHelper.SamplePosition(Runtimedata.targetAnimatedBoneSignal[SimulatedIndex], timeAsDouble);
                Vector3 childTargetPosition = PositionSignalHelper.SamplePosition(Runtimedata.targetAnimatedBoneSignal[ChildIndex], timeAsDouble);
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
        /// Computes the projected position of a JiggleBone based on its parent JiggleBone.
        /// </summary>
        /// <param name="JiggleBone">Index of the JiggleBone.</param>
        /// <param name="JiggleParent">Index of the JiggleParent.</param>
        /// <returns>The projected position as a Vector3.</returns>
        public Vector3 GetProjectedPosition(int JiggleBone, int JiggleParent)
        {
            Transform parentTransform;

            // Get the parent transform
            if (JiggleBones[JiggleBone].JiggleParentIndex != -1)
            {
                int ParentIndex = JiggleBones[JiggleBone].JiggleParentIndex;
                parentTransform = ComputedTransforms[ParentIndex].transform;
            }
            else
            {
                parentTransform = ComputedTransforms[JiggleBone].parent;
            }

            // Compute and return the projected position
            return ComputedTransforms[JiggleParent].TransformPoint(parentTransform.InverseTransformPoint(ComputedTransforms[JiggleParent].position));
        }
        public Vector3 GetTransformPosition(int BoneIndex)
        {
            if (!Runtimedata.hasTransform[BoneIndex])
            {
                return GetProjectedPosition(BoneIndex, JiggleBones[BoneIndex].JiggleParentIndex);
            }
            else
            {
                return ComputedTransforms[BoneIndex].position;
            }
        }
        public float GetLengthToParent(JiggleBone JiggleBone)
        {
            int ParentIndex = JiggleBone.JiggleParentIndex;
            int BoneIndex = Array.IndexOf(JiggleBones, JiggleBone);
            return Vector3.Distance(Runtimedata.currentFixedAnimatedBonePosition[BoneIndex], Runtimedata.currentFixedAnimatedBonePosition[ParentIndex]);
        }
        public void MatchAnimationInstantly(int JiggleBoneIndex, double time)
        {
            Vector3 position = GetTransformPosition(JiggleBoneIndex);
            var outputA = Runtimedata.targetAnimatedBoneSignal[JiggleBoneIndex];
            var outputB = Runtimedata.particleSignal[JiggleBoneIndex];
            PositionSignalHelper.FlattenSignal(ref outputA, time, position);
            PositionSignalHelper.FlattenSignal(ref outputB, time, position);

            Runtimedata.targetAnimatedBoneSignal[JiggleBoneIndex] = outputA;
            Runtimedata.particleSignal[JiggleBoneIndex] = outputB;
        }
        /// <summary>
        /// Physically accurate teleportation, maintains the existing signals of motion and keeps their trajectories through a teleport. First call PrepareTeleport(), then move the character, then call FinishTeleport().
        /// Use MatchAnimationInstantly() instead if you don't want jiggles to be maintained through a teleport.
        /// </summary>
        public void PrepareTeleport(int JiggleBone)
        {
            Runtimedata.preTeleportPosition[JiggleBone] = GetTransformPosition(JiggleBone);
        }
        public void PrepareTeleport()
        {
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                PrepareTeleport(PointsIndex);
            }
        }
        /// <summary>
        /// The companion function to PrepareTeleport, it discards all the movement that has happened since the call to PrepareTeleport, assuming that they've both been called on the same frame.
        /// </summary>
        public void FinishTeleport(double timeAsDouble)
        {
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                Vector3 position = GetTransformPosition(PointsIndex);
                Vector3 diff = position - Runtimedata.preTeleportPosition[PointsIndex];
                var outputA = Runtimedata.targetAnimatedBoneSignal[PointsIndex];
                var outputB = Runtimedata.particleSignal[PointsIndex];
                PositionSignalHelper.FlattenSignal(ref outputA, timeAsDouble, position);
                PositionSignalHelper.OffsetSignal(ref outputB, diff);
                Runtimedata.targetAnimatedBoneSignal[PointsIndex] = outputA;
                Runtimedata.particleSignal[PointsIndex] = outputB;
                Runtimedata.workingPosition[PointsIndex] += diff;
            }
        }
    }
}