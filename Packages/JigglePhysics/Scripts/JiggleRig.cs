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
        private Transform rootTransform;
        [Tooltip("The settings that the rig should update with, create them using the Create->JigglePhysics->Settings menu option.")]
        public JiggleSettingsBase jiggleSettings;
        [SerializeField]
        [Tooltip("The list of transforms to ignore during the jiggle. Each bone listed will also ignore all the children of the specified bone.")]
        private Transform[] ignoredTransforms;
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




        public List<Quaternion> boneRotationChangeCheck = new List<Quaternion>();
        public List<Quaternion> lastValidPoseBoneRotation = new List<Quaternion>();

        public List<Vector3> currentFixedAnimatedBonePosition = new List<Vector3>();
        public List<Vector3> bonePositionChangeCheck = new List<Vector3>();
        public List<Vector3> lastValidPoseBoneLocalPosition = new List<Vector3>();
        public List<Vector3> workingPosition = new List<Vector3>();
        public List<Vector3> preTeleportPosition = new List<Vector3>();
        public List<Vector3> extrapolatedPosition = new List<Vector3>();
        public List<bool> hasTransform = new List<bool>();




        public JiggleRig(Transform rootTransform, JiggleSettingsBase jiggleSettings, Transform[] ignoredTransforms, Collider[] colliders)
        {
            this.rootTransform = rootTransform;
            this.jiggleSettings = jiggleSettings;
            this.ignoredTransforms = ignoredTransforms;
            this.colliders = colliders;
            this.collidersCount = colliders.Length;
            Zero = Vector3.zero;
            Initialize();
        }
        public void Initialize()
        {
            if (rootTransform == null)
            {
                return;
            }
            CreateSimulatedPoints(ref SPoints, ignoredTransforms, rootTransform, null);
            this.simulatedPointsCount = SPoints.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                int distanceToRoot = 0;
                JiggleBone test = SPoints[SimulatedIndex];
                while (test.JiggleParent != null)
                {
                    test = test.JiggleParent;
                    distanceToRoot++;
                }

                int distanceToChild = 0;
                test = SPoints[SimulatedIndex];
                while (test.child != null)
                {
                    test = test.child;
                    distanceToChild++;
                }

                int max = distanceToRoot + distanceToChild;
                float frac = (float)distanceToRoot / max;
                SPoints[SimulatedIndex].normalizedIndex = frac;
            }
            initialized = true;
        }
        public void Update(Vector3 wind, double TimeAsDouble, float fixedDeltaTime, Vector3 Gravity)
        {
            float squaredDeltaTime = fixedDeltaTime * fixedDeltaTime;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                currentFixedAnimatedBonePosition[SimulatedIndex] = PositionSignalHelper.SamplePosition(SPoints[SimulatedIndex].targetAnimatedBoneSignal, TimeAsDouble);
                if (SPoints[SimulatedIndex].JiggleParent == null)
                {
                    workingPosition[SimulatedIndex] = currentFixedAnimatedBonePosition[SimulatedIndex];
                    PositionSignalHelper.SetPosition(ref SPoints[SimulatedIndex].particleSignal, workingPosition[SimulatedIndex], TimeAsDouble);
                    continue;
                }
                Vector3 CurrentSignal = PositionSignalHelper.GetCurrent(SPoints[SimulatedIndex].particleSignal);
                Vector3 PreviousSignal = PositionSignalHelper.GetPrevious(SPoints[SimulatedIndex].particleSignal);

                Vector3 ParentCurrentSignal = PositionSignalHelper.GetCurrent(SPoints[SimulatedIndex].JiggleParent.particleSignal);

                Vector3 ParentPreviousSignal = PositionSignalHelper.GetPrevious(SPoints[SimulatedIndex].JiggleParent.particleSignal);

                Vector3 localSpaceVelocity = (CurrentSignal - PreviousSignal) - (ParentCurrentSignal - ParentPreviousSignal);
                workingPosition[SimulatedIndex] = NextPhysicsPosition(CurrentSignal, PreviousSignal, localSpaceVelocity, Gravity, squaredDeltaTime, jiggleSettingsdata.gravityMultiplier, jiggleSettingsdata.friction, jiggleSettingsdata.airDrag);
                workingPosition[SimulatedIndex] += wind * (fixedDeltaTime * jiggleSettingsdata.airDrag);
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (SPoints[SimulatedIndex].JiggleParent == null)
                {
                    PositionSignalHelper.SetPosition(ref SPoints[SimulatedIndex].particleSignal, workingPosition[SimulatedIndex], TimeAsDouble);
                    continue;
                }
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
                if (SPoints[SimulatedIndex].JiggleParent == null)
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
                        sphereCollider.radius = jiggleSettings.GetRadius(SPoints[SimulatedIndex].normalizedIndex);
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
                PositionSignalHelper.SetPosition(ref SPoints[SimulatedIndex].particleSignal, workingPosition[SimulatedIndex], TimeAsDouble);
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
            extrapolatedPosition[0] = PositionSignalHelper.SamplePosition(SPoints[0].particleSignal, timeAsDouble);

            Vector3 virtualPosition = extrapolatedPosition[0];

            Vector3 offset = SPoints[0].transform.position - virtualPosition;
            int simulatedPointsLength = SPoints.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsLength; SimulatedIndex++)
            {
                extrapolatedPosition[SimulatedIndex] = offset + PositionSignalHelper.SamplePosition(SPoints[SimulatedIndex].particleSignal, timeAsDouble);
            }
        }
        public void Pose(bool debugDraw, double timeAsDouble)
        {
            DeriveFinalSolve(timeAsDouble);
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (SPoints[SimulatedIndex].child == null)
                {
                    continue; // Early exit if there's no child
                }
                // Cache frequently accessed values
                Vector3 targetPosition = PositionSignalHelper.SamplePosition(SPoints[SimulatedIndex].targetAnimatedBoneSignal, timeAsDouble);
                Vector3 childTargetPosition = PositionSignalHelper.SamplePosition(SPoints[SimulatedIndex].child.targetAnimatedBoneSignal, timeAsDouble);
                // Blend positions
                Vector3 positionBlend = Vector3.Lerp(targetPosition, extrapolatedPosition[SimulatedIndex], jiggleSettingsdata.blend);


                int Index = Array.IndexOf(SPoints, SPoints[SimulatedIndex].child);

                Vector3 childPositionBlend = Vector3.Lerp(childTargetPosition, extrapolatedPosition[Index], jiggleSettingsdata.blend);

                if (SPoints[SimulatedIndex].JiggleParent != null)
                {
                    SPoints[SimulatedIndex].transform.position = positionBlend;
                }

                // Calculate child position and vector differences
                Vector3 childPosition = GetTransformPosition(SPoints[SimulatedIndex].child);
                Vector3 cachedAnimatedVector = childPosition - positionBlend;
                Vector3 simulatedVector = childPositionBlend - positionBlend;

                // Rotate the transform based on the vector differences
                if (cachedAnimatedVector != Vector3.zero && simulatedVector != Vector3.zero)
                {
                    Quaternion animPoseToPhysicsPose = Quaternion.FromToRotation(cachedAnimatedVector, simulatedVector);
                    SPoints[SimulatedIndex].transform.rotation = animPoseToPhysicsPose * SPoints[SimulatedIndex].transform.rotation;
                }

                // Cache transform changes if the bone has a transform
                if (hasTransform[SimulatedIndex])
                {
                    SPoints[SimulatedIndex].transform.GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
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
        public void FinishTeleport(double timeAsDouble, float FixedDeltaTime)
        {
            for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
            {
                FinishTeleport(SPoints[PointsIndex], timeAsDouble, FixedDeltaTime);
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
        public Vector3 GetProjectedPosition(JiggleBone JiggleBone)
        {
            return JiggleBone.JiggleParent.transform.TransformPoint(GetParentTransform(JiggleBone).InverseTransformPoint(JiggleBone.JiggleParent.transform.position) * JiggleBone.projectionAmount);
        }
        public Vector3 GetTransformPosition(JiggleBone JiggleBone)
        {
            int Index = Array.IndexOf(SPoints, JiggleBone);
            if (!hasTransform[Index])
            {
                return GetProjectedPosition(JiggleBone);
            }
            else
            {
                return JiggleBone.transform.position;
            }
        }
        public Transform GetParentTransform(JiggleBone JiggleBone)
        {
            if (JiggleBone.JiggleParent != null)
            {
                return JiggleBone.JiggleParent.transform;
            }
            return JiggleBone.transform.parent;
        }
        public void CacheAnimationPosition(JiggleBone JiggleBone, double timeAsDouble)
        {
            int Index = Array.IndexOf(SPoints, JiggleBone);
            if (!hasTransform[Index])
            {
                PositionSignalHelper.SetPosition(ref JiggleBone.targetAnimatedBoneSignal, GetProjectedPosition(JiggleBone), timeAsDouble);
                return;
            }
            PositionSignalHelper.SetPosition(ref JiggleBone.targetAnimatedBoneSignal, JiggleBone.transform.position, timeAsDouble);
            JiggleBone.transform.GetLocalPositionAndRotation(out Vector3 pos, out Quaternion Rot);
            lastValidPoseBoneRotation[Index] = Rot;
            lastValidPoseBoneLocalPosition[Index] = pos;
        }
        public Vector3 ConstrainLengthBackwards(JiggleBone JiggleBone, Vector3 newPosition, float elasticity)
        {
            if (JiggleBone.child == null)
            {
                return newPosition;
            }
            int IndexChild = Array.IndexOf(SPoints, JiggleBone.child);
            Vector3 diff = newPosition - workingPosition[IndexChild];
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, workingPosition[IndexChild] + dir * GetLengthToParent(JiggleBone), elasticity);
        }
        public Vector3 ConstrainLength(JiggleBone JiggleBone, Vector3 newPosition, float elasticity)
        {
            int Index = Array.IndexOf(SPoints, JiggleBone.JiggleParent);
            Vector3 diff = newPosition - workingPosition[Index];
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, workingPosition[Index] + dir * GetLengthToParent(JiggleBone), elasticity);
        }
        public float GetLengthToParent(JiggleBone JiggleBone)
        {
            int Index = Array.IndexOf(SPoints, JiggleBone);
            int ParentIndex = Array.IndexOf(SPoints, JiggleBone.JiggleParent);
            return Vector3.Distance(currentFixedAnimatedBonePosition[Index], currentFixedAnimatedBonePosition[ParentIndex]);
        }
        public void MatchAnimationInstantly(JiggleBone JiggleBone, double time, float fixedDeltaTime)
        {
            Vector3 position = GetTransformPosition(JiggleBone);
            PositionSignalHelper.FlattenSignal(ref JiggleBone.targetAnimatedBoneSignal, time, position, fixedDeltaTime);
            PositionSignalHelper.FlattenSignal(ref JiggleBone.particleSignal, time, position, fixedDeltaTime);
        }
        /// <summary>
        /// Physically accurate teleportation, maintains the existing signals of motion and keeps their trajectories through a teleport. First call PrepareTeleport(), then move the character, then call FinishTeleport().
        /// Use MatchAnimationInstantly() instead if you don't want jiggles to be maintained through a teleport.
        /// </summary>
        public void PrepareTeleport(JiggleBone JiggleBone)
        {
            int Index = Array.IndexOf(SPoints, JiggleBone);
            preTeleportPosition[Index] = GetTransformPosition(JiggleBone);
        }
        /// <summary>
        /// The companion function to PrepareTeleport, it discards all the movement that has happened since the call to PrepareTeleport, assuming that they've both been called on the same frame.
        /// </summary>
        public void FinishTeleport(JiggleBone JiggleBone, double timeAsDouble, float FixedDeltaTime)
        {
            Vector3 position = GetTransformPosition(JiggleBone);
            int Index = Array.IndexOf(SPoints, JiggleBone);
            Vector3 diff = position - preTeleportPosition[Index];
            PositionSignalHelper.FlattenSignal(ref JiggleBone.targetAnimatedBoneSignal, timeAsDouble, position, FixedDeltaTime);
            PositionSignalHelper.OffsetSignal(ref JiggleBone.particleSignal, diff);
            workingPosition[Index] += diff;
        }
        public Vector3 ConstrainAngle(JiggleBone JiggleBone, Vector3 newPosition, float elasticity, float elasticitySoften)
        {
            int Index = Array.IndexOf(SPoints, JiggleBone);
            if (!hasTransform[Index] && JiggleBone.projectionAmount == 0f)
            {
                return newPosition;
            }
            Vector3 parentParentPosition;
            Vector3 poseParentParent;
            int ParentIndex = Array.IndexOf(SPoints, JiggleBone.JiggleParent);
            if (JiggleBone.JiggleParent.JiggleParent == null)
            {
                poseParentParent = currentFixedAnimatedBonePosition[ParentIndex] + (currentFixedAnimatedBonePosition[ParentIndex] - currentFixedAnimatedBonePosition[Index]);
                parentParentPosition = poseParentParent;
            }
            else
            {
                int ParentparentIndex = Array.IndexOf(SPoints, JiggleBone.JiggleParent.JiggleParent);
                parentParentPosition = workingPosition[ParentparentIndex];
                poseParentParent = currentFixedAnimatedBonePosition[ParentparentIndex];
            }
            Vector3 parentAimTargetPose = currentFixedAnimatedBonePosition[ParentIndex] - poseParentParent;
            Vector3 parentAim = workingPosition[ParentIndex] - parentParentPosition;
            Quaternion TargetPoseToPose = Quaternion.FromToRotation(parentAimTargetPose, parentAim);
            Vector3 currentPose = currentFixedAnimatedBonePosition[Index] - poseParentParent;
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
            int Index = Array.IndexOf(SPoints, JiggleBone);
            return extrapolatedPosition[Index];
        }
        public void PrepareBone(JiggleBone JiggleBone, double currentTime)
        {
            // If bone is not animated, return to last unadulterated pose
            int Index = Array.IndexOf(SPoints, JiggleBone);
            if (hasTransform[Index])
            {
                JiggleBone.transform.GetLocalPositionAndRotation(out Vector3 localPosition, out Quaternion localrotation);
                if (boneRotationChangeCheck[Index] == localrotation)
                {
                    JiggleBone.transform.localRotation = lastValidPoseBoneRotation[Index];
                }
                if (bonePositionChangeCheck[Index] == localPosition)
                {
                    JiggleBone.transform.localPosition = lastValidPoseBoneLocalPosition[Index];
                }
            }
            CacheAnimationPosition(JiggleBone, currentTime);
        }
        protected virtual void CreateSimulatedPoints(ref JiggleBone[] outputPoints, Transform[] ignoredTransforms, Transform currentTransform, JiggleBone parentJiggleBone)
        {
            // Use a list to store the JiggleBones
            List<JiggleBone> jiggleBoneList = new List<JiggleBone>(outputPoints ?? new JiggleBone[0]);
            // Recursive function to create simulated points using a list
            void CreateSimulatedPointsInternal(List<JiggleBone> list, Transform[] ignored, Transform current, JiggleBone parent)
            {
                // Create a new JiggleBone and add it to the list
                JiggleBone newJiggleBone = JiggleBone(current, parent);
                list.Add(newJiggleBone);
                // Check if the currentTransform has no children
                if (current.childCount == 0)
                {
                    // Handle the case where newJiggleBone has no parent
                    if (newJiggleBone.JiggleParent == null)
                    {
                        if (newJiggleBone.transform.parent == null)
                        {
                            throw new UnityException("Can't have a singular jiggle bone with no parents. That doesn't even make sense!");
                        }
                        else
                        {
                            // Add an extra virtual JiggleBone
                            list.Add(JiggleBone(null, newJiggleBone));
                            return;
                        }
                    }
                    // Add another virtual JiggleBone
                    list.Add(JiggleBone(null, newJiggleBone));
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
                    CreateSimulatedPointsInternal(list, ignored, child, newJiggleBone);
                }
            }
            // Call the internal recursive method
            CreateSimulatedPointsInternal(jiggleBoneList, ignoredTransforms, currentTransform, parentJiggleBone);
            // Convert the list back to an array and assign it to outputPoints
            outputPoints = jiggleBoneList.ToArray();
        }
        public JiggleBone JiggleBone(Transform transform, JiggleBone parent, float projectionAmount = 1f)
        {
            JiggleBone JiggleBone = new JiggleBone
            {
                transform = transform,
                JiggleParent = parent,
                projectionAmount = projectionAmount
            };
            boneRotationChangeCheck.Add(Quaternion.identity);
            currentFixedAnimatedBonePosition.Add(Vector3.zero);
            bonePositionChangeCheck.Add(Vector3.zero);
            workingPosition.Add(Vector3.zero);
            preTeleportPosition.Add(Vector3.zero);
            extrapolatedPosition.Add(Vector3.zero);
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
                position = GetProjectedPosition(JiggleBone);
            }
            double timeAsDouble = Time.timeAsDouble;
            JiggleBone.targetAnimatedBoneSignal = new PositionSignal(position, timeAsDouble);
            JiggleBone.particleSignal = new PositionSignal(position, timeAsDouble);
            hasTransform.Add(transform != null);
            if (parent == null)
            {
                return JiggleBone;
            }
            JiggleBone.JiggleParent.child = JiggleBone;
            return JiggleBone;
        }
    }
}