using System;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;
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
        [HideInInspector]
        protected JiggleBone[] SPoints;
        public int collidersCount;
        public Vector3 Zero;
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
                SPoints[SimulatedIndex].currentFixedAnimatedBonePosition = SPoints[SimulatedIndex].targetAnimatedBoneSignal.SamplePosition(TimeAsDouble);
                if (SPoints[SimulatedIndex].JiggleParent == null)
                {
                    SPoints[SimulatedIndex].workingPosition = SPoints[SimulatedIndex].currentFixedAnimatedBonePosition;
                    SPoints[SimulatedIndex].particleSignal.SetPosition(SPoints[SimulatedIndex].workingPosition, TimeAsDouble);
                    continue;
                }
                Vector3 localSpaceVelocity = (SPoints[SimulatedIndex].particleSignal.GetCurrent() - SPoints[SimulatedIndex].particleSignal.GetPrevious()) - (SPoints[SimulatedIndex].JiggleParent.particleSignal.GetCurrent() - SPoints[SimulatedIndex].JiggleParent.particleSignal.GetPrevious());
                SPoints[SimulatedIndex].workingPosition = NextPhysicsPosition(SPoints[SimulatedIndex].particleSignal.GetCurrent(), SPoints[SimulatedIndex].particleSignal.GetPrevious(), localSpaceVelocity, Gravity, squaredDeltaTime, jiggleSettingsdata.gravityMultiplier, jiggleSettingsdata.friction, jiggleSettingsdata.airDrag
                    );
                SPoints[SimulatedIndex].workingPosition += wind * (fixedDeltaTime * jiggleSettingsdata.airDrag);
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (SPoints[SimulatedIndex].JiggleParent == null)
                {
                    SPoints[SimulatedIndex].particleSignal.SetPosition(SPoints[SimulatedIndex].workingPosition, TimeAsDouble);
                    continue;
                }
            }

            if (NeedsCollisions)
            {
                for (int Index = simulatedPointsCount - 1; Index >= 0; Index--)
                {
                    SPoints[Index].workingPosition = ConstrainLengthBackwards(SPoints[Index], SPoints[Index].workingPosition, jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity * 0.5f);
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                if (SPoints[SimulatedIndex].JiggleParent == null)
                {
                    continue;
                }
                SPoints[SimulatedIndex].workingPosition = ConstrainAngle(SPoints[SimulatedIndex], SPoints[SimulatedIndex].workingPosition, jiggleSettingsdata.angleElasticity * jiggleSettingsdata.angleElasticity, jiggleSettingsdata.elasticitySoften);
                SPoints[SimulatedIndex].workingPosition = ConstrainLength(SPoints[SimulatedIndex], SPoints[SimulatedIndex].workingPosition, jiggleSettingsdata.lengthElasticity * jiggleSettingsdata.lengthElasticity);
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
                        if (Physics.ComputePenetration(sphereCollider, SPoints[SimulatedIndex].workingPosition, Quaternion.identity, collider, position, rotation, out Vector3 dir, out float dist))
                        {
                            SPoints[SimulatedIndex].workingPosition += dir * dist;
                        }
                    }
                }
            }
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                SPoints[SimulatedIndex].particleSignal.SetPosition(SPoints[SimulatedIndex].workingPosition, TimeAsDouble);
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
            Vector3 virtualPosition = DeriveFinalSolvePosition(SPoints[0], Zero, timeAsDouble);
            Vector3 offset = SPoints[0].transform.position - virtualPosition;
            int simulatedPointsLength = SPoints.Length;
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsLength; SimulatedIndex++)
            {
                DeriveFinalSolvePosition(SPoints[SimulatedIndex], offset, timeAsDouble);
            }
        }
        public void Pose(bool debugDraw, float deltaTime, double timeAsDouble)
        {
            DeriveFinalSolve(timeAsDouble);
            for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
            {
                PoseBone(SPoints[SimulatedIndex], jiggleSettingsdata.blend, deltaTime);

                if (debugDraw)
                {
                    DebugDraw(SPoints[SimulatedIndex], Color.red, Color.blue, true);
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
                OnDrawGizmos(SPoints[PointsIndex], jiggleSettings, TimeAsDouble);
            }
        }
        protected virtual void CreateSimulatedPoints(ref JiggleBone[] outputPoints, Transform[] ignoredTransforms, Transform currentTransform, JiggleBone parentJiggleBone)
        {
            // Use a list to store the JiggleBones
            List<JiggleBone> jiggleBoneList = new List<JiggleBone>(outputPoints ?? new JiggleBone[0]);
            // Recursive function to create simulated points using a list
            void CreateSimulatedPointsInternal(List<JiggleBone> list, Transform[] ignored, Transform current, JiggleBone parent)
            {
                // Create a new JiggleBone and add it to the list
                JiggleBone newJiggleBone = new JiggleBone(current, parent);
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
                            list.Add(new JiggleBone(null, newJiggleBone));
                            return;
                        }
                    }
                    // Add another virtual JiggleBone
                    list.Add(new JiggleBone(null, newJiggleBone));
                    return;
                }
                // Iterate through child transforms
                int childCount = current.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = current.GetChild(i);
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
        public static Vector3 GetProjectedPosition(JiggleBone JiggleBone)
        {
            return JiggleBone.JiggleParent.transform.TransformPoint(GetParentTransform(JiggleBone).InverseTransformPoint(JiggleBone.JiggleParent.transform.position) * JiggleBone.projectionAmount);
        }
        public static Vector3 GetTransformPosition(JiggleBone JiggleBone)
        {
            if (!JiggleBone.hasTransform)
            {
                return GetProjectedPosition(JiggleBone);
            }
            else
            {
                return JiggleBone.transform.position;
            }
        }
        public static Transform GetParentTransform(JiggleBone JiggleBone)
        {
            if (JiggleBone.JiggleParent != null)
            {
                return JiggleBone.JiggleParent.transform;
            }
            return JiggleBone.transform.parent;
        }
        public static void CacheAnimationPosition(JiggleBone JiggleBone, double timeAsDouble)
        {
            if (!JiggleBone.hasTransform)
            {
                JiggleBone.targetAnimatedBoneSignal.SetPosition(GetProjectedPosition(JiggleBone), timeAsDouble);
                return;
            }
            JiggleBone.targetAnimatedBoneSignal.SetPosition(JiggleBone.transform.position, timeAsDouble);
            JiggleBone.transform.GetLocalPositionAndRotation(out JiggleBone.lastValidPoseBoneLocalPosition, out JiggleBone.lastValidPoseBoneRotation);
        }
        public static Vector3 ConstrainLengthBackwards(JiggleBone JiggleBone, Vector3 newPosition, float elasticity)
        {
            if (JiggleBone.child == null)
            {
                return newPosition;
            }
            Vector3 diff = newPosition - JiggleBone.child.workingPosition;
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, JiggleBone.child.workingPosition + dir * GetLengthToParent(JiggleBone), elasticity);
        }
        public static Vector3 ConstrainLength(JiggleBone JiggleBone, Vector3 newPosition, float elasticity)
        {
            Vector3 diff = newPosition - JiggleBone.JiggleParent.workingPosition;
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, JiggleBone.JiggleParent.workingPosition + dir * GetLengthToParent(JiggleBone), elasticity);
        }
        public static float GetLengthToParent(JiggleBone JiggleBone)
        {
            return Vector3.Distance(JiggleBone.currentFixedAnimatedBonePosition, JiggleBone.JiggleParent.currentFixedAnimatedBonePosition);
        }
        public static void MatchAnimationInstantly(JiggleBone JiggleBone, double time, float fixedDeltaTime)
        {
            Vector3 position = GetTransformPosition(JiggleBone);
            JiggleBone.targetAnimatedBoneSignal.FlattenSignal(time, position, fixedDeltaTime);
            JiggleBone.particleSignal.FlattenSignal(time, position, fixedDeltaTime);
        }
        /// <summary>
        /// Physically accurate teleportation, maintains the existing signals of motion and keeps their trajectories through a teleport. First call PrepareTeleport(), then move the character, then call FinishTeleport().
        /// Use MatchAnimationInstantly() instead if you don't want jiggles to be maintained through a teleport.
        /// </summary>
        public static void PrepareTeleport(JiggleBone JiggleBone)
        {
            JiggleBone.preTeleportPosition = GetTransformPosition(JiggleBone);
        }
        /// <summary>
        /// The companion function to PrepareTeleport, it discards all the movement that has happened since the call to PrepareTeleport, assuming that they've both been called on the same frame.
        /// </summary>
        public static void FinishTeleport(JiggleBone JiggleBone, double timeAsDouble, float FixedDeltaTime)
        {
            Vector3 position = GetTransformPosition(JiggleBone);
            Vector3 diff = position - JiggleBone.preTeleportPosition;
            JiggleBone.targetAnimatedBoneSignal.FlattenSignal(timeAsDouble, position, FixedDeltaTime);
            JiggleBone.particleSignal.OffsetSignal(diff);
            JiggleBone.workingPosition += diff;
        }
        public static Vector3 ConstrainAngle(JiggleBone JiggleBone, Vector3 newPosition, float elasticity, float elasticitySoften)
        {
            if (!JiggleBone.hasTransform && JiggleBone.projectionAmount == 0f)
            {
                return newPosition;
            }
            Vector3 parentParentPosition;
            Vector3 poseParentParent;
            if (JiggleBone.JiggleParent.JiggleParent == null)
            {
                poseParentParent = JiggleBone.JiggleParent.currentFixedAnimatedBonePosition + (JiggleBone.JiggleParent.currentFixedAnimatedBonePosition - JiggleBone.currentFixedAnimatedBonePosition);
                parentParentPosition = poseParentParent;
            }
            else
            {
                parentParentPosition = JiggleBone.JiggleParent.JiggleParent.workingPosition;
                poseParentParent = JiggleBone.JiggleParent.JiggleParent.currentFixedAnimatedBonePosition;
            }
            Vector3 parentAimTargetPose = JiggleBone.JiggleParent.currentFixedAnimatedBonePosition - poseParentParent;
            Vector3 parentAim = JiggleBone.JiggleParent.workingPosition - parentParentPosition;
            Quaternion TargetPoseToPose = Quaternion.FromToRotation(parentAimTargetPose, parentAim);
            Vector3 currentPose = JiggleBone.currentFixedAnimatedBonePosition - poseParentParent;
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
        public static Vector3 DeriveFinalSolvePosition(JiggleBone JiggleBone, Vector3 offset, double timeAsDouble)
        {
            JiggleBone.extrapolatedPosition = offset + JiggleBone.particleSignal.SamplePosition(timeAsDouble);
            return JiggleBone.extrapolatedPosition;
        }
        public static Vector3 GetCachedSolvePosition(JiggleBone JiggleBone)
        {
            return JiggleBone.extrapolatedPosition;
        }
        public static void PrepareBone(JiggleBone JiggleBone, double currentTime)
        {
            // If bone is not animated, return to last unadulterated pose
            if (JiggleBone.hasTransform)
            {
                if (JiggleBone.boneRotationChangeCheck == JiggleBone.transform.localRotation)
                {
                    JiggleBone.transform.localRotation = JiggleBone.lastValidPoseBoneRotation;
                }
                if (JiggleBone.bonePositionChangeCheck == JiggleBone.transform.localPosition)
                {
                    JiggleBone.transform.localPosition = JiggleBone.lastValidPoseBoneLocalPosition;
                }
            }
            CacheAnimationPosition(JiggleBone, currentTime);
        }
        public static void PoseBone(JiggleBone JiggleBone, float blend, double currentTime)
        {
            if (JiggleBone.child == null) return; // Early exit if there's no child

            // Cache frequently accessed values
            Vector3 targetPosition = JiggleBone.targetAnimatedBoneSignal.SamplePosition(currentTime);
            Vector3 childTargetPosition = JiggleBone.child.targetAnimatedBoneSignal.SamplePosition(currentTime);

            // Blend positions
            Vector3 positionBlend = Vector3.Lerp(targetPosition, JiggleBone.extrapolatedPosition, blend);
            Vector3 childPositionBlend = Vector3.Lerp(childTargetPosition, JiggleBone.child.extrapolatedPosition, blend);

            if (JiggleBone.JiggleParent != null)
            {
                JiggleBone.transform.position = positionBlend;
            }

            // Calculate child position and vector differences
            Vector3 childPosition = GetTransformPosition(JiggleBone.child);
            Vector3 cachedAnimatedVector = childPosition - positionBlend;
            Vector3 simulatedVector = childPositionBlend - positionBlend;

            // Rotate the transform based on the vector differences
            if (cachedAnimatedVector != Vector3.zero && simulatedVector != Vector3.zero)
            {
                Quaternion animPoseToPhysicsPose = Quaternion.FromToRotation(cachedAnimatedVector, simulatedVector);
                JiggleBone.transform.rotation = animPoseToPhysicsPose * JiggleBone.transform.rotation;
            }

            // Cache transform changes if the bone has a transform
            if (JiggleBone.hasTransform)
            {
                JiggleBone.transform.GetLocalPositionAndRotation(out JiggleBone.bonePositionChangeCheck, out JiggleBone.boneRotationChangeCheck);
            }
        }
        public static void DebugDraw(JiggleBone JiggleBone, Color simulateColor, Color targetColor, bool interpolated)
        {
            if (JiggleBone.JiggleParent == null) return;
            if (interpolated)
            {
                Debug.DrawLine(JiggleBone.extrapolatedPosition, JiggleBone.JiggleParent.extrapolatedPosition, simulateColor, 0, false);
            }
            else
            {
                Debug.DrawLine(JiggleBone.workingPosition, JiggleBone.JiggleParent.workingPosition, simulateColor, 0, false);
            }
            Debug.DrawLine(JiggleBone.currentFixedAnimatedBonePosition, JiggleBone.JiggleParent.currentFixedAnimatedBonePosition, targetColor, 0, false);
        }
        public static void OnDrawGizmos(JiggleBone JiggleBone, JiggleSettingsBase jiggleSettings, double TimeAsDouble)
        {
            Vector3 pos = JiggleBone.particleSignal.SamplePosition(TimeAsDouble);
            if (JiggleBone.child != null)
            {
                Gizmos.Line(pos, JiggleBone.child.particleSignal.SamplePosition(TimeAsDouble));
            }
            if (jiggleSettings != null)
            {
                Gizmos.Sphere(pos, jiggleSettings.GetRadius(JiggleBone.normalizedIndex));
            }
        }
    }
    public struct Frame
    {
        public Vector3 position;
        public double time;
    }
    public class PositionSignal
    {
        private Frame previousFrame;
        private Frame currentFrame;

        public PositionSignal(Vector3 startPosition, double time)
        {
            currentFrame = previousFrame = new Frame
            {
                position = startPosition,
                time = time,
            };
        }

        public void SetPosition(Vector3 position, double time)
        {
            previousFrame = currentFrame;
            currentFrame = new Frame
            {
                position = position,
                time = time,
            };
        }

        public void OffsetSignal(Vector3 offset)
        {
            previousFrame = new Frame
            {
                position = previousFrame.position + offset,
                time = previousFrame.time,
            };
            currentFrame = new Frame
            {
                position = currentFrame.position + offset,
                time = previousFrame.time,
            };
        }

        public void FlattenSignal(double time, Vector3 position, float fixedDeltaTime)
        {
            previousFrame = new Frame
            {
                position = position,
                time = time - JiggleRigBuilder.GetmaxCatchupTime(fixedDeltaTime) * 2f,
            };
            currentFrame = new Frame
            {
                position = position,
                time = time - JiggleRigBuilder.GetmaxCatchupTime(fixedDeltaTime),
            };
        }

        public Vector3 GetCurrent() => currentFrame.position;
        public Vector3 GetPrevious() => previousFrame.position;

        public Vector3 SamplePosition(double time)
        {
            double diff = currentFrame.time - previousFrame.time;
            if (diff == 0)
            {
                return previousFrame.position;
            }
            double t = ((double)(time) - (double)previousFrame.time) / (double)diff;
            return Vector3.Lerp(previousFrame.position, currentFrame.position, (float)t);
        }
    }
}