using System;
using System.Collections.Generic;
using UnityEngine;
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
        public JiggleBone[] SPoints;
        public int collidersCount;
        public Vector3 Zero;
        public InitalizationData InitalData = new InitalizationData();
        public struct InitalizationData
        {
            public List<Transform> ComputedTransforms;
            public List<Quaternion> boneRotationChangeCheck;
            public List<Quaternion> lastValidPoseBoneRotation;
            public List<Vector3> currentFixedAnimatedBonePosition;
            public List<Vector3> bonePositionChangeCheck;
            public List<Vector3> lastValidPoseBoneLocalPosition;
            public List<Vector3> workingPosition;
            public List<Vector3> preTeleportPosition;
            public List<Vector3> extrapolatedPosition;
            public List<bool> hasTransform;
            public List<float> normalizedIndex;
            public List<PositionSignal> targetAnimatedBoneSignal;
            public List<PositionSignal> particleSignal;
        }
        public void Initialize()
        {
            Zero = Vector3.zero;

            InitalData.ComputedTransforms = new List<Transform>();
            InitalData.boneRotationChangeCheck = new List<Quaternion>();
            InitalData.lastValidPoseBoneRotation = new List<Quaternion>();

            InitalData.currentFixedAnimatedBonePosition = new List<Vector3>();
            InitalData.bonePositionChangeCheck = new List<Vector3>();
            InitalData.lastValidPoseBoneLocalPosition = new List<Vector3>();
            InitalData.workingPosition = new List<Vector3>();
            InitalData.preTeleportPosition = new List<Vector3>();
            InitalData.extrapolatedPosition = new List<Vector3>();
            InitalData.hasTransform = new List<bool>();
            InitalData.normalizedIndex = new List<float>();

            InitalData.targetAnimatedBoneSignal = new List<PositionSignal>();
            InitalData.particleSignal = new List<PositionSignal>();

            CreateSimulatedPoints(ignoredTransforms, rootTransform, null);
            this.simulatedPointsCount = SPoints.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                int distanceToRoot = 0;
                JiggleBone test = SPoints[SimulatedIndex];
                while (test.JiggleParentIndex != -1)
                {
                    test = SPoints[test.JiggleParentIndex];
                    distanceToRoot++;
                }

                int distanceToChild = 0;
                test = SPoints[SimulatedIndex];
                while (test.childIndex != -1)
                {
                    test = SPoints[test.childIndex];
                    distanceToChild++;
                }

                int max = distanceToRoot + distanceToChild;
                float frac = (float)distanceToRoot / max;
                InitalData.normalizedIndex[SimulatedIndex] = frac;
            }
            initialized = true;
        }
        public void Update(Vector3 wind, double TimeAsDouble, float fixedDeltaTime, Vector3 Gravity)
        {
            float squaredDeltaTime = fixedDeltaTime * fixedDeltaTime;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                InitalData.currentFixedAnimatedBonePosition[SimulatedIndex] = PositionSignalHelper.SamplePosition(InitalData.targetAnimatedBoneSignal[SimulatedIndex], TimeAsDouble);
                if (SPoints[SimulatedIndex].JiggleParentIndex == -1)
                {
                    InitalData.workingPosition[SimulatedIndex] = InitalData.currentFixedAnimatedBonePosition[SimulatedIndex];
                    var output = InitalData.particleSignal[SimulatedIndex];
                    PositionSignalHelper.SetPosition(ref output, InitalData.workingPosition[SimulatedIndex], TimeAsDouble);
                    continue;
                }
                Vector3 CurrentSignal = PositionSignalHelper.GetCurrent(InitalData.particleSignal[SimulatedIndex]);
                Vector3 PreviousSignal = PositionSignalHelper.GetPrevious(InitalData.particleSignal[SimulatedIndex]);
                int JiggleParentindex = SPoints[SimulatedIndex].JiggleParentIndex;
                Vector3 ParentCurrentSignal = PositionSignalHelper.GetCurrent(InitalData.particleSignal[JiggleParentindex]);

                Vector3 ParentPreviousSignal = PositionSignalHelper.GetPrevious(InitalData.particleSignal[JiggleParentindex]);

                Vector3 localSpaceVelocity = (CurrentSignal - PreviousSignal) - (ParentCurrentSignal - ParentPreviousSignal);
                InitalData.workingPosition[SimulatedIndex] = NextPhysicsPosition(CurrentSignal, PreviousSignal, localSpaceVelocity, Gravity, squaredDeltaTime, jiggleSettingsdata.gravityMultiplier, jiggleSettingsdata.friction, jiggleSettingsdata.airDrag);
                InitalData.workingPosition[SimulatedIndex] += wind * (fixedDeltaTime * jiggleSettingsdata.airDrag);
            }

            if (NeedsCollisions)
            {
                for (int Index = simulatedPointsCount - 1; Index >= 0; Index--)
                {
                    InitalData.workingPosition[Index] = ConstrainLengthBackwards(SPoints[Index], InitalData.workingPosition[Index], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity * 0.5f);
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (SPoints[SimulatedIndex].JiggleParentIndex == -1)
                {
                    continue;
                }
                InitalData.workingPosition[SimulatedIndex] = ConstrainAngle(SPoints[SimulatedIndex], InitalData.workingPosition[SimulatedIndex], jiggleSettingsdata.angleElasticity * jiggleSettingsdata.angleElasticity, jiggleSettingsdata.elasticitySoften);
                InitalData.workingPosition[SimulatedIndex] = ConstrainLength(SPoints[SimulatedIndex], InitalData.workingPosition[SimulatedIndex], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity);
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
                        sphereCollider.radius = jiggleSettings.GetRadius(InitalData.normalizedIndex[SimulatedIndex]);
                        if (sphereCollider.radius <= 0)
                        {
                            continue;
                        }
                        Collider collider = colliders[ColliderIndex];
                        collider.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                        if (Physics.ComputePenetration(sphereCollider, InitalData.workingPosition[SimulatedIndex], Quaternion.identity, collider, position, rotation, out Vector3 dir, out float dist))
                        {
                            InitalData.workingPosition[SimulatedIndex] += dir * dist;
                        }
                    }
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                var output = InitalData.particleSignal[SimulatedIndex];
                PositionSignalHelper.SetPosition(ref output, InitalData.workingPosition[SimulatedIndex], TimeAsDouble);
                InitalData.particleSignal[SimulatedIndex] = output;
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
                PrepareBone(SPoints[PointIndex], timeAsDouble);
            }
            jiggleSettingsdata = jiggleSettings.GetData();
            jiggleSettingsdata = jiggleRigLOD != null ? jiggleRigLOD.AdjustJiggleSettingsData(position, jiggleSettingsdata) : jiggleSettingsdata;
        }
        public void DeriveFinalSolve(double timeAsDouble)
        {
            InitalData.extrapolatedPosition[0] = PositionSignalHelper.SamplePosition(InitalData.particleSignal[0], timeAsDouble);

            Vector3 virtualPosition = InitalData.extrapolatedPosition[0];

            Vector3 offset = InitalData.ComputedTransforms[0].transform.position - virtualPosition;
            int simulatedPointsLength = SPoints.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsLength; SimulatedIndex++)
            {
                InitalData.extrapolatedPosition[SimulatedIndex] = offset + PositionSignalHelper.SamplePosition(InitalData.particleSignal[SimulatedIndex], timeAsDouble);
            }
        }
        public void Pose(bool debugDraw, double timeAsDouble)
        {
            DeriveFinalSolve(timeAsDouble);
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (SPoints[SimulatedIndex].childIndex == -1)
                {
                    continue; // Early exit if there's no child
                }
                int ChildIndex = SPoints[SimulatedIndex].childIndex;
                // Cache frequently accessed values
                Vector3 targetPosition = PositionSignalHelper.SamplePosition(InitalData.targetAnimatedBoneSignal[SimulatedIndex], timeAsDouble);
                Vector3 childTargetPosition = PositionSignalHelper.SamplePosition(InitalData.targetAnimatedBoneSignal[ChildIndex], timeAsDouble);
                // Blend positions
                Vector3 positionBlend = Vector3.Lerp(targetPosition, InitalData.extrapolatedPosition[SimulatedIndex], jiggleSettingsdata.blend);

                Vector3 childPositionBlend = Vector3.Lerp(childTargetPosition, InitalData.extrapolatedPosition[ChildIndex], jiggleSettingsdata.blend);

                if (SPoints[SimulatedIndex].JiggleParentIndex != -1)
                {
                    InitalData.ComputedTransforms[SimulatedIndex].position = positionBlend;
                }

                // Calculate child position and vector differences
                int childIndex = SPoints[SimulatedIndex].childIndex;
                Vector3 childPosition = GetTransformPosition(SPoints[childIndex]);
                Vector3 cachedAnimatedVector = childPosition - positionBlend;
                Vector3 simulatedVector = childPositionBlend - positionBlend;

                // Rotate the transform based on the vector differences
                if (cachedAnimatedVector != Vector3.zero && simulatedVector != Vector3.zero)
                {
                    Quaternion animPoseToPhysicsPose = Quaternion.FromToRotation(cachedAnimatedVector, simulatedVector);
                    InitalData.ComputedTransforms[SimulatedIndex].rotation = animPoseToPhysicsPose * InitalData.ComputedTransforms[SimulatedIndex].rotation;
                }

                // Cache transform changes if the bone has a transform
                if (InitalData.hasTransform[SimulatedIndex])
                {
                    InitalData.ComputedTransforms[SimulatedIndex].GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
                    InitalData.boneRotationChangeCheck[SimulatedIndex] = Rot;
                    InitalData.bonePositionChangeCheck[SimulatedIndex] = pos;
                }
                if (debugDraw)
                {
                    //  JiggleRigGizmos.DebugDraw(SPoints[SimulatedIndex], Color.red, Color.blue, true);
                }
            }
        }
        public void PrepareTeleport()
        {
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                PrepareTeleport(SPoints[PointsIndex]);
            }
        }
        public void FinishTeleport(double timeAsDouble)
        {
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                FinishTeleport(SPoints[PointsIndex], timeAsDouble);
            }
        }
        public void OnRenderObject(double TimeAsDouble)
        {
            if (!initialized || SPoints == null)
            {
                Initialize();
            }
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                //JiggleRigGizmos.OnDrawGizmos(SPoints[PointsIndex], jiggleSettings, TimeAsDouble);
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
            return InitalData.ComputedTransforms[JiggleParent].TransformPoint(GetParentTransform(JiggleBone).InverseTransformPoint(InitalData.ComputedTransforms[JiggleParent].position));
        }
        public Vector3 GetTransformPosition(JiggleBone JiggleBone)
        {
            if (!InitalData.hasTransform[JiggleBone.boneIndex])
            {
                return GetProjectedPosition(JiggleBone, JiggleBone.JiggleParentIndex);
            }
            else
            {
                return InitalData.ComputedTransforms[JiggleBone.boneIndex].position;
            }
        }
        public Transform GetParentTransform(JiggleBone JiggleBone)
        {
            if (JiggleBone.JiggleParentIndex != -1)
            {
                int ParentIndex = JiggleBone.JiggleParentIndex;

                return InitalData.ComputedTransforms[ParentIndex].transform;
            }
            return InitalData.ComputedTransforms[JiggleBone.boneIndex].parent;
        }
        public void CacheAnimationPosition(JiggleBone JiggleBone, double timeAsDouble)
        {
            if (!InitalData.hasTransform[JiggleBone.boneIndex])
            {
                PositionSignal BoneSignalHas = InitalData.targetAnimatedBoneSignal[JiggleBone.boneIndex];
                int ParentIndex = JiggleBone.JiggleParentIndex;
                PositionSignalHelper.SetPosition(ref BoneSignalHas, GetProjectedPosition(JiggleBone, ParentIndex), timeAsDouble);
                InitalData.targetAnimatedBoneSignal[JiggleBone.boneIndex] = BoneSignalHas;
                return;
            }
            var BoneSignal = InitalData.targetAnimatedBoneSignal[JiggleBone.boneIndex];
            PositionSignalHelper.SetPosition(ref BoneSignal, InitalData.ComputedTransforms[JiggleBone.boneIndex].position, timeAsDouble);
            InitalData.targetAnimatedBoneSignal[JiggleBone.boneIndex] = BoneSignal;
            InitalData.ComputedTransforms[JiggleBone.boneIndex].GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
            InitalData.lastValidPoseBoneRotation[JiggleBone.boneIndex] = Rot;
            InitalData.lastValidPoseBoneLocalPosition[JiggleBone.boneIndex] = pos;
        }
        public Vector3 ConstrainLengthBackwards(JiggleBone JiggleBone, Vector3 newPosition, float elasticity)
        {
            if (JiggleBone.childIndex == -1)
            {
                return newPosition;
            }
            Vector3 diff = newPosition - InitalData.workingPosition[JiggleBone.childIndex];
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, InitalData.workingPosition[JiggleBone.childIndex] + dir * GetLengthToParent(JiggleBone), elasticity);
        }
        public Vector3 ConstrainLength(JiggleBone JiggleBone, Vector3 newPosition, float elasticity)
        {
            int Index = JiggleBone.JiggleParentIndex;
            Vector3 diff = newPosition - InitalData.workingPosition[Index];
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, InitalData.workingPosition[Index] + dir * GetLengthToParent(JiggleBone), elasticity);
        }
        public float GetLengthToParent(JiggleBone JiggleBone)
        {
            int ParentIndex = JiggleBone.JiggleParentIndex;
            return Vector3.Distance(InitalData.currentFixedAnimatedBonePosition[JiggleBone.boneIndex], InitalData.currentFixedAnimatedBonePosition[ParentIndex]);
        }
        public void MatchAnimationInstantly(JiggleBone JiggleBone, double time)
        {
            Vector3 position = GetTransformPosition(JiggleBone);
            var outputA = InitalData.targetAnimatedBoneSignal[JiggleBone.boneIndex];
            var outputB = InitalData.particleSignal[JiggleBone.boneIndex];
            PositionSignalHelper.FlattenSignal(ref outputA, time, position);
            PositionSignalHelper.FlattenSignal(ref outputB, time, position);

            InitalData.targetAnimatedBoneSignal[JiggleBone.boneIndex] = outputA;
            InitalData.particleSignal[JiggleBone.boneIndex] = outputB;
        }
        /// <summary>
        /// Physically accurate teleportation, maintains the existing signals of motion and keeps their trajectories through a teleport. First call PrepareTeleport(), then move the character, then call FinishTeleport().
        /// Use MatchAnimationInstantly() instead if you don't want jiggles to be maintained through a teleport.
        /// </summary>
        public void PrepareTeleport(JiggleBone JiggleBone)
        {
            InitalData.preTeleportPosition[JiggleBone.boneIndex] = GetTransformPosition(JiggleBone);
        }
        /// <summary>
        /// The companion function to PrepareTeleport, it discards all the movement that has happened since the call to PrepareTeleport, assuming that they've both been called on the same frame.
        /// </summary>
        public void FinishTeleport(JiggleBone JiggleBone, double timeAsDouble)
        {
            Vector3 position = GetTransformPosition(JiggleBone);
            Vector3 diff = position - InitalData.preTeleportPosition[JiggleBone.boneIndex];
            var outputA = InitalData.targetAnimatedBoneSignal[JiggleBone.boneIndex];
            var outputB = InitalData.particleSignal[JiggleBone.boneIndex];
            PositionSignalHelper.FlattenSignal(ref outputA, timeAsDouble, position);
            PositionSignalHelper.OffsetSignal(ref outputB, diff);
            InitalData.targetAnimatedBoneSignal[JiggleBone.boneIndex] = outputA;
            InitalData.particleSignal[JiggleBone.boneIndex] = outputB;
            InitalData.workingPosition[JiggleBone.boneIndex] += diff;
        }
        public Vector3 ConstrainAngle(JiggleBone JiggleBone, Vector3 newPosition, float elasticity, float elasticitySoften)
        {
            if (!InitalData.hasTransform[JiggleBone.boneIndex])
            {
                return newPosition;
            }
            Vector3 parentParentPosition;
            Vector3 poseParentParent;
            int ParentIndex = JiggleBone.JiggleParentIndex;
            int ParentsParentIndex = SPoints[ParentIndex].JiggleParentIndex;
            if (ParentsParentIndex == -1)
            {
                poseParentParent = InitalData.currentFixedAnimatedBonePosition[ParentIndex] + (InitalData.currentFixedAnimatedBonePosition[ParentIndex] - InitalData.currentFixedAnimatedBonePosition[JiggleBone.boneIndex]);
                parentParentPosition = poseParentParent;
            }
            else
            {
                parentParentPosition = InitalData.workingPosition[ParentsParentIndex];
                poseParentParent = InitalData.currentFixedAnimatedBonePosition[ParentsParentIndex];
            }
            Vector3 parentAimTargetPose = InitalData.currentFixedAnimatedBonePosition[ParentIndex] - poseParentParent;
            Vector3 parentAim = InitalData.workingPosition[ParentIndex] - parentParentPosition;
            Quaternion TargetPoseToPose = Quaternion.FromToRotation(parentAimTargetPose, parentAim);
            Vector3 currentPose = InitalData.currentFixedAnimatedBonePosition[JiggleBone.boneIndex] - poseParentParent;
            Vector3 constraintTarget = TargetPoseToPose * currentPose;
            float error = Vector3.Distance(newPosition, parentParentPosition + constraintTarget);
            error /= GetLengthToParent(JiggleBone);
            error = Mathf.Clamp01(error);
            error = Mathf.Pow(error, elasticitySoften * 2f);
            return Vector3.Lerp(newPosition, parentParentPosition + constraintTarget, elasticity * error);
        }
        public static Vector3 NextPhysicsPosition(Vector3 newPosition, Vector3 previousPosition, Vector3 localSpaceVelocity, Vector3 Gravity, float squaredDeltaTime, float gravityMultiplier, float friction, float airFriction)
        {
            return newPosition + (newPosition - previousPosition - localSpaceVelocity) * (1f - airFriction) + localSpaceVelocity * (1f - friction) + Gravity * (gravityMultiplier * squaredDeltaTime);
        }
        public Vector3 GetCachedSolvePosition(JiggleBone JiggleBone)
        {
            return InitalData.extrapolatedPosition[JiggleBone.boneIndex];
        }
        public void PrepareBone(JiggleBone JiggleBone, double currentTime)
        {
            // If bone is not animated, return to last unadulterated pose
            if (InitalData.hasTransform[JiggleBone.boneIndex])
            {
                InitalData.ComputedTransforms[JiggleBone.boneIndex].GetLocalPositionAndRotation(out Vector3 localPosition, out Quaternion localrotation);
                if (InitalData.boneRotationChangeCheck[JiggleBone.boneIndex] == localrotation)
                {
                    InitalData.ComputedTransforms[JiggleBone.boneIndex].localRotation = InitalData.lastValidPoseBoneRotation[JiggleBone.boneIndex];
                }
                if (InitalData.bonePositionChangeCheck[JiggleBone.boneIndex] == localPosition)
                {
                    InitalData.ComputedTransforms[JiggleBone.boneIndex].localPosition = InitalData.lastValidPoseBoneLocalPosition[JiggleBone.boneIndex];
                }
            }
            CacheAnimationPosition(JiggleBone, currentTime);
        }
        protected virtual void CreateSimulatedPoints(Transform[] ignoredTransforms, Transform currentTransform, JiggleBone parentJiggleBone)
        {
            // Recursive function to create simulated points using a list
            void CreateSimulatedPointsInternal(Transform[] ignored, Transform current, JiggleBone parent)
            {
                // Create a new JiggleBone and add it to the list
                JiggleBone newJiggleBone = JiggleBone(current, parent);
                // Check if the currentTransform has no children
                if (current.childCount == 0)
                {
                    // Handle the case where newJiggleBone has no parent
                    if (newJiggleBone.JiggleParentIndex == -1)
                    {
                        if (InitalData.ComputedTransforms[newJiggleBone.boneIndex].parent == null)
                        {
                            throw new UnityException("Can't have a singular jiggle bone with no parents. That doesn't even make sense!");
                        }
                        else
                        {
                            // Add an extra virtual JiggleBone
                            JiggleBone ExtraBone = JiggleBone(null, newJiggleBone);
                            return;
                        }
                    }
                    // Add another virtual JiggleBone
                    JiggleBone virtualBone = JiggleBone(null, newJiggleBone);
                    return;
                }
                // Iterate through child transforms
                int childCount = current.childCount;
                for (int ChildIndex = 0; ChildIndex < childCount; ChildIndex++)
                {
                    Transform child = current.GetChild(ChildIndex);
                    // Check if the child is in the ignoredTransforms array
                    if (Array.Exists(ignored, t => t == child))
                    {
                        continue;
                    }
                    // Recursively create simulated points for child transforms
                    CreateSimulatedPointsInternal(ignored, child, newJiggleBone);
                }
            }
            // Call the internal recursive method
            CreateSimulatedPointsInternal(ignoredTransforms, currentTransform, parentJiggleBone);
        }
        public static JiggleBone[] AddToArray(JiggleBone[] originalArray, JiggleBone newItem)
        {
            // Create a new array with one extra slot
            JiggleBone[] newArray;
            if (originalArray == null)
            {
                originalArray = new JiggleBone[] { };
            }
            newArray = new JiggleBone[originalArray.Length + 1];

            // Copy the original array into the new array
            for (int i = 0; i < originalArray.Length; i++)
            {
                newArray[i] = originalArray[i];
            }

            // Add the new item to the end of the new array
            newArray[originalArray.Length] = newItem;

            return newArray;
        }
        public JiggleBone JiggleBone(Transform transform, JiggleBone parent)
        {
            JiggleBone JiggleBone = new JiggleBone
            {
                JiggleParentIndex = -1,
                childIndex = -1
            };
            SPoints = AddToArray(SPoints, JiggleBone);
            int ParentIndex = Array.IndexOf(SPoints, parent);
            JiggleBone.boneIndex = Array.IndexOf(SPoints, JiggleBone);
            JiggleBone.JiggleParentIndex = ParentIndex;
            InitalData.ComputedTransforms.Add(transform);
            InitalData.boneRotationChangeCheck.Add(Quaternion.identity);
            InitalData.currentFixedAnimatedBonePosition.Add(Vector3.zero);
            InitalData.bonePositionChangeCheck.Add(Vector3.zero);
            InitalData.workingPosition.Add(Vector3.zero);
            InitalData.preTeleportPosition.Add(Vector3.zero);
            InitalData.extrapolatedPosition.Add(Vector3.zero);
            InitalData.normalizedIndex.Add(0);
            Vector3 position;
            if (transform != null)
            {
                transform.GetLocalPositionAndRotation(out Vector3 Position, out Quaternion Rotation);
                position = transform.position;
                InitalData.lastValidPoseBoneRotation.Add(Rotation);
                InitalData.lastValidPoseBoneLocalPosition.Add(Position);
            }
            else
            {
                InitalData.lastValidPoseBoneRotation.Add(Quaternion.identity);
                InitalData.lastValidPoseBoneLocalPosition.Add(Vector3.zero);
                if (JiggleBone.JiggleParentIndex != -1)
                {
                    position = GetProjectedPosition(JiggleBone, JiggleBone.JiggleParentIndex);
                }
                else
                {
                    position = Vector3.zero;
                }
            }
            double timeAsDouble = Time.timeAsDouble;
            InitalData.targetAnimatedBoneSignal.Add(new PositionSignal(position, timeAsDouble));
            InitalData.particleSignal.Add(new PositionSignal(position, timeAsDouble));
            InitalData.hasTransform.Add(transform != null);
            if (parent == null)
            {
                return JiggleBone;
            }
            int childIndex = Array.IndexOf(SPoints, JiggleBone);

            int SParentIndex = JiggleBone.JiggleParentIndex;
            SPoints[SParentIndex].childIndex = childIndex;
            return JiggleBone;
        }
    }
}