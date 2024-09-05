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
        public void Initialize()
        {
            Zero = Vector3.zero;

            ComputedTransforms = new List<Transform>();
            boneRotationChangeCheck = new List<Quaternion>();
            lastValidPoseBoneRotation = new List<Quaternion>();

            currentFixedAnimatedBonePosition = new List<Vector3>();
            bonePositionChangeCheck = new List<Vector3>();
            lastValidPoseBoneLocalPosition = new List<Vector3>();
            workingPosition = new List<Vector3>();
            preTeleportPosition = new List<Vector3>();
            extrapolatedPosition = new List<Vector3>();
            hasTransform = new List<bool>();
            normalizedIndex = new List<float>();

            targetAnimatedBoneSignal = new List<PositionSignal>();
            particleSignal = new List<PositionSignal>();

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
                normalizedIndex[SimulatedIndex] = frac;
            }
            initialized = true;
        }
        public void Update(Vector3 wind, double TimeAsDouble, float fixedDeltaTime, Vector3 Gravity)
        {
            float squaredDeltaTime = fixedDeltaTime * fixedDeltaTime;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                currentFixedAnimatedBonePosition[SimulatedIndex] = PositionSignalHelper.SamplePosition(targetAnimatedBoneSignal[SimulatedIndex], TimeAsDouble);
                if (SPoints[SimulatedIndex].JiggleParentIndex == -1)
                {
                    workingPosition[SimulatedIndex] = currentFixedAnimatedBonePosition[SimulatedIndex];
                    var output = particleSignal[SimulatedIndex];
                    PositionSignalHelper.SetPosition(ref output, workingPosition[SimulatedIndex], TimeAsDouble);
                    continue;
                }
                Vector3 CurrentSignal = PositionSignalHelper.GetCurrent(particleSignal[SimulatedIndex]);
                Vector3 PreviousSignal = PositionSignalHelper.GetPrevious(particleSignal[SimulatedIndex]);
                int JiggleParentindex = SPoints[SimulatedIndex].JiggleParentIndex;
                Vector3 ParentCurrentSignal = PositionSignalHelper.GetCurrent(particleSignal[JiggleParentindex]);

                Vector3 ParentPreviousSignal = PositionSignalHelper.GetPrevious(particleSignal[JiggleParentindex]);

                Vector3 localSpaceVelocity = (CurrentSignal - PreviousSignal) - (ParentCurrentSignal - ParentPreviousSignal);
                workingPosition[SimulatedIndex] = NextPhysicsPosition(CurrentSignal, PreviousSignal, localSpaceVelocity, Gravity, squaredDeltaTime, jiggleSettingsdata.gravityMultiplier, jiggleSettingsdata.friction, jiggleSettingsdata.airDrag);
                workingPosition[SimulatedIndex] += wind * (fixedDeltaTime * jiggleSettingsdata.airDrag);
            }

            if (NeedsCollisions)
            {
                for (int Index = simulatedPointsCount - 1; Index >= 0; Index--)
                {
                    workingPosition[Index] = ConstrainLengthBackwards(SPoints[Index], workingPosition[Index], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity * 0.5f);
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (SPoints[SimulatedIndex].JiggleParentIndex == -1)
                {
                    continue;
                }
                workingPosition[SimulatedIndex] = ConstrainAngle(SPoints[SimulatedIndex], workingPosition[SimulatedIndex], jiggleSettingsdata.angleElasticity * jiggleSettingsdata.angleElasticity, jiggleSettingsdata.elasticitySoften);
                workingPosition[SimulatedIndex] = ConstrainLength(SPoints[SimulatedIndex], workingPosition[SimulatedIndex], jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity);
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
                        sphereCollider.radius = jiggleSettings.GetRadius(normalizedIndex[SimulatedIndex]);
                        if (sphereCollider.radius <= 0)
                        {
                            continue;
                        }
                        Collider collider = colliders[ColliderIndex];
                        collider.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                        if (Physics.ComputePenetration(sphereCollider, workingPosition[SimulatedIndex], Quaternion.identity, collider, position, rotation, out Vector3 dir, out float dist))
                        {
                            workingPosition[SimulatedIndex] += dir * dist;
                        }
                    }
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                var output = particleSignal[SimulatedIndex];
                PositionSignalHelper.SetPosition(ref output, workingPosition[SimulatedIndex], TimeAsDouble);
                particleSignal[SimulatedIndex] = output;
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
            extrapolatedPosition[0] = PositionSignalHelper.SamplePosition(particleSignal[0], timeAsDouble);

            Vector3 virtualPosition = extrapolatedPosition[0];

            Vector3 offset = ComputedTransforms[0].transform.position - virtualPosition;
            int simulatedPointsLength = SPoints.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsLength; SimulatedIndex++)
            {
                extrapolatedPosition[SimulatedIndex] = offset + PositionSignalHelper.SamplePosition(particleSignal[SimulatedIndex], timeAsDouble);
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
                Vector3 targetPosition = PositionSignalHelper.SamplePosition(targetAnimatedBoneSignal[SimulatedIndex], timeAsDouble);
                Vector3 childTargetPosition = PositionSignalHelper.SamplePosition(targetAnimatedBoneSignal[ChildIndex], timeAsDouble);
                // Blend positions
                Vector3 positionBlend = Vector3.Lerp(targetPosition, extrapolatedPosition[SimulatedIndex], jiggleSettingsdata.blend);

                Vector3 childPositionBlend = Vector3.Lerp(childTargetPosition, extrapolatedPosition[ChildIndex], jiggleSettingsdata.blend);

                if (SPoints[SimulatedIndex].JiggleParentIndex != -1)
                {
                    ComputedTransforms[SimulatedIndex].position = positionBlend;
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
                    ComputedTransforms[SimulatedIndex].rotation = animPoseToPhysicsPose * ComputedTransforms[SimulatedIndex].rotation;
                }

                // Cache transform changes if the bone has a transform
                if (hasTransform[SimulatedIndex])
                {
                    ComputedTransforms[SimulatedIndex].GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
                    boneRotationChangeCheck[SimulatedIndex] = Rot;
                    bonePositionChangeCheck[SimulatedIndex] = pos;
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
            return ComputedTransforms[JiggleParent].TransformPoint(GetParentTransform(JiggleBone).InverseTransformPoint(ComputedTransforms[JiggleParent].position));
        }
        public Vector3 GetTransformPosition(JiggleBone JiggleBone)
        {
            if (!hasTransform[JiggleBone.boneIndex])
            {
                return GetProjectedPosition(JiggleBone, JiggleBone.JiggleParentIndex);
            }
            else
            {
                return ComputedTransforms[JiggleBone.boneIndex].position;
            }
        }
        public Transform GetParentTransform(JiggleBone JiggleBone)
        {
            if (JiggleBone.JiggleParentIndex != -1)
            {
                int ParentIndex = JiggleBone.JiggleParentIndex;

                return ComputedTransforms[ParentIndex].transform;
            }
            return ComputedTransforms[JiggleBone.boneIndex].parent;
        }
        public void CacheAnimationPosition(JiggleBone JiggleBone, double timeAsDouble)
        {
            if (!hasTransform[JiggleBone.boneIndex])
            {
                PositionSignal BoneSignalHas = targetAnimatedBoneSignal[JiggleBone.boneIndex];
                int ParentIndex = JiggleBone.JiggleParentIndex;
                PositionSignalHelper.SetPosition(ref BoneSignalHas, GetProjectedPosition(JiggleBone, ParentIndex), timeAsDouble);
                targetAnimatedBoneSignal[JiggleBone.boneIndex] = BoneSignalHas;
                return;
            }
            var BoneSignal = targetAnimatedBoneSignal[JiggleBone.boneIndex];
            PositionSignalHelper.SetPosition(ref BoneSignal, ComputedTransforms[JiggleBone.boneIndex].position, timeAsDouble);
            targetAnimatedBoneSignal[JiggleBone.boneIndex] = BoneSignal;
            ComputedTransforms[JiggleBone.boneIndex].GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
            lastValidPoseBoneRotation[JiggleBone.boneIndex] = Rot;
            lastValidPoseBoneLocalPosition[JiggleBone.boneIndex] = pos;
        }
        public Vector3 ConstrainLengthBackwards(JiggleBone JiggleBone, Vector3 newPosition, float elasticity)
        {
            if (JiggleBone.childIndex == -1)
            {
                return newPosition;
            }
            Vector3 diff = newPosition - workingPosition[JiggleBone.childIndex];
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, workingPosition[JiggleBone.childIndex] + dir * GetLengthToParent(JiggleBone), elasticity);
        }
        public Vector3 ConstrainLength(JiggleBone JiggleBone, Vector3 newPosition, float elasticity)
        {
            int Index = JiggleBone.JiggleParentIndex;
            Vector3 diff = newPosition - workingPosition[Index];
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, workingPosition[Index] + dir * GetLengthToParent(JiggleBone), elasticity);
        }
        public float GetLengthToParent(JiggleBone JiggleBone)
        {
            int ParentIndex = JiggleBone.JiggleParentIndex;
            return Vector3.Distance(currentFixedAnimatedBonePosition[JiggleBone.boneIndex], currentFixedAnimatedBonePosition[ParentIndex]);
        }
        public void MatchAnimationInstantly(JiggleBone JiggleBone, double time)
        {
            Vector3 position = GetTransformPosition(JiggleBone);
            var outputA = targetAnimatedBoneSignal[JiggleBone.boneIndex];
            var outputB = particleSignal[JiggleBone.boneIndex];
            PositionSignalHelper.FlattenSignal(ref outputA, time, position);
            PositionSignalHelper.FlattenSignal(ref outputB, time, position);

            targetAnimatedBoneSignal[JiggleBone.boneIndex] = outputA;
            particleSignal[JiggleBone.boneIndex] = outputB;
        }
        /// <summary>
        /// Physically accurate teleportation, maintains the existing signals of motion and keeps their trajectories through a teleport. First call PrepareTeleport(), then move the character, then call FinishTeleport().
        /// Use MatchAnimationInstantly() instead if you don't want jiggles to be maintained through a teleport.
        /// </summary>
        public void PrepareTeleport(JiggleBone JiggleBone)
        {
            preTeleportPosition[JiggleBone.boneIndex] = GetTransformPosition(JiggleBone);
        }
        /// <summary>
        /// The companion function to PrepareTeleport, it discards all the movement that has happened since the call to PrepareTeleport, assuming that they've both been called on the same frame.
        /// </summary>
        public void FinishTeleport(JiggleBone JiggleBone, double timeAsDouble)
        {
            Vector3 position = GetTransformPosition(JiggleBone);
            Vector3 diff = position - preTeleportPosition[JiggleBone.boneIndex];
            var outputA = targetAnimatedBoneSignal[JiggleBone.boneIndex];
            var outputB = particleSignal[JiggleBone.boneIndex];
            PositionSignalHelper.FlattenSignal(ref outputA, timeAsDouble, position);
            PositionSignalHelper.OffsetSignal(ref outputB, diff);
            targetAnimatedBoneSignal[JiggleBone.boneIndex] = outputA;
            particleSignal[JiggleBone.boneIndex] = outputB;
            workingPosition[JiggleBone.boneIndex] += diff;
        }
        public Vector3 ConstrainAngle(JiggleBone JiggleBone, Vector3 newPosition, float elasticity, float elasticitySoften)
        {
            if (!hasTransform[JiggleBone.boneIndex])
            {
                return newPosition;
            }
            Vector3 parentParentPosition;
            Vector3 poseParentParent;
            int ParentIndex = JiggleBone.JiggleParentIndex;
            int ParentsParentIndex = SPoints[ParentIndex].JiggleParentIndex;
            if (ParentsParentIndex == -1)
            {
                poseParentParent = currentFixedAnimatedBonePosition[ParentIndex] + (currentFixedAnimatedBonePosition[ParentIndex] - currentFixedAnimatedBonePosition[JiggleBone.boneIndex]);
                parentParentPosition = poseParentParent;
            }
            else
            {
                parentParentPosition = workingPosition[ParentsParentIndex];
                poseParentParent = currentFixedAnimatedBonePosition[ParentsParentIndex];
            }
            Vector3 parentAimTargetPose = currentFixedAnimatedBonePosition[ParentIndex] - poseParentParent;
            Vector3 parentAim = workingPosition[ParentIndex] - parentParentPosition;
            Quaternion TargetPoseToPose = Quaternion.FromToRotation(parentAimTargetPose, parentAim);
            Vector3 currentPose = currentFixedAnimatedBonePosition[JiggleBone.boneIndex] - poseParentParent;
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
            return extrapolatedPosition[JiggleBone.boneIndex];
        }
        public void PrepareBone(JiggleBone JiggleBone, double currentTime)
        {
            // If bone is not animated, return to last unadulterated pose
            if (hasTransform[JiggleBone.boneIndex])
            {
                ComputedTransforms[JiggleBone.boneIndex].GetLocalPositionAndRotation(out Vector3 localPosition, out Quaternion localrotation);
                if (boneRotationChangeCheck[JiggleBone.boneIndex] == localrotation)
                {
                    ComputedTransforms[JiggleBone.boneIndex].localRotation = lastValidPoseBoneRotation[JiggleBone.boneIndex];
                }
                if (bonePositionChangeCheck[JiggleBone.boneIndex] == localPosition)
                {
                    ComputedTransforms[JiggleBone.boneIndex].localPosition = lastValidPoseBoneLocalPosition[JiggleBone.boneIndex];
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
                        if (ComputedTransforms[newJiggleBone.boneIndex].parent == null)
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
            ComputedTransforms.Add(transform);
            boneRotationChangeCheck.Add(Quaternion.identity);
            currentFixedAnimatedBonePosition.Add(Vector3.zero);
            bonePositionChangeCheck.Add(Vector3.zero);
            workingPosition.Add(Vector3.zero);
            preTeleportPosition.Add(Vector3.zero);
            extrapolatedPosition.Add(Vector3.zero);
            normalizedIndex.Add(0);
            Vector3 position;
            if (transform != null)
            {
                transform.GetLocalPositionAndRotation(out Vector3 Position, out Quaternion Rotation);
                position = transform.position;
                lastValidPoseBoneRotation.Add(Rotation);
                lastValidPoseBoneLocalPosition.Add(Position);
            }
            else
            {
                lastValidPoseBoneRotation.Add(Quaternion.identity);
                lastValidPoseBoneLocalPosition.Add(Vector3.zero);
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
            targetAnimatedBoneSignal.Add(new PositionSignal(position, timeAsDouble));
            particleSignal.Add(new PositionSignal(position, timeAsDouble));
            hasTransform.Add(transform != null);
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