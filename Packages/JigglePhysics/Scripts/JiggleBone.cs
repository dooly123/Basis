using UnityEngine;
using Gizmos = Popcron.Gizmos;
namespace JigglePhysics
{
    // Uses Verlet to resolve constraints easily 
    public partial class JiggleBone
    {
        public readonly bool hasTransform;
        public readonly PositionSignal targetAnimatedBoneSignal;
        public Vector3 currentFixedAnimatedBonePosition;
        public readonly JiggleBone JiggleParent;
        public bool HasJiggleParent;
        public JiggleBone child;
        public Quaternion boneRotationChangeCheck;
        public Vector3 bonePositionChangeCheck;
        public Quaternion lastValidPoseBoneRotation;
        public float projectionAmount;
        public Vector3 lastValidPoseBoneLocalPosition;
        public float normalizedIndex;
        public readonly Transform transform;
        public readonly PositionSignal particleSignal;
        public Vector3 workingPosition;
        public Vector3 preTeleportPosition;
        public Vector3 extrapolatedPosition;
        public Vector3 Zero;
        public JiggleBone(Transform transform, JiggleBone parent, float projectionAmount = 1f)
        {
            this.transform = transform;
            this.JiggleParent = parent;
            this.projectionAmount = projectionAmount;
            Zero = Vector3.zero;
            Vector3 position;
            if (transform != null)
            {
                transform.GetLocalPositionAndRotation(out lastValidPoseBoneLocalPosition, out lastValidPoseBoneRotation);
                position = transform.position;
            }
            else
            {
                position = GetProjectedPosition();
            }
            double timeAsDouble = Time.timeAsDouble;
            targetAnimatedBoneSignal = new PositionSignal(position, timeAsDouble);
            particleSignal = new PositionSignal(position, Time.timeAsDouble);

            hasTransform = transform != null;
            if (parent == null)
            {
                return;
            }
            JiggleParent.child = this;
        }
        public void CalculateNormalizedIndex()
        {
            int distanceToRoot = 0;
            JiggleBone test = this;
            while (test.JiggleParent != null)
            {
                test = test.JiggleParent;
                distanceToRoot++;
            }

            int distanceToChild = 0;
            test = this;
            while (test.child != null)
            {
                test = test.child;
                distanceToChild++;
            }

            int max = distanceToRoot + distanceToChild;
            float frac = (float)distanceToRoot / max;
            normalizedIndex = frac;
        }
        public void VerletPass(JiggleSettingsData jiggleSettings, Vector3 wind, double time,float fixedDeltaTime,float squaredDeltaTime, Vector3 Gravity)
        {
            currentFixedAnimatedBonePosition = targetAnimatedBoneSignal.SamplePosition(time);
            if (JiggleParent == null)
            {
                workingPosition = currentFixedAnimatedBonePosition;
                particleSignal.SetPosition(workingPosition, time);
                return;
            }
            Vector3 localSpaceVelocity = (particleSignal.GetCurrent() - particleSignal.GetPrevious()) - (JiggleParent.particleSignal.GetCurrent() - JiggleParent.particleSignal.GetPrevious());
            workingPosition = NextPhysicsPosition( particleSignal.GetCurrent(), particleSignal.GetPrevious(), localSpaceVelocity, Gravity, squaredDeltaTime, jiggleSettings.gravityMultiplier, jiggleSettings.friction,jiggleSettings.airDrag
            );
            workingPosition += wind * (fixedDeltaTime * jiggleSettings.airDrag);
        }
        public void CollisionPreparePass(JiggleSettingsData jiggleSettings)
        {
            workingPosition = ConstrainLengthBackwards(workingPosition, jiggleSettings.lengthElasticity * jiggleSettings.lengthElasticity * 0.5f);
        }
        public void ConstraintPass(JiggleSettingsData jiggleSettings)
        {
            if (JiggleParent == null)
            {
                return;
            }
            workingPosition = ConstrainAngle(workingPosition, jiggleSettings.angleElasticity * jiggleSettings.angleElasticity, jiggleSettings.elasticitySoften);
            workingPosition = ConstrainLength(workingPosition, jiggleSettings.lengthElasticity * jiggleSettings.lengthElasticity);
        }
        public void CollisionPass(JiggleSettingsBase jiggleSettings, Collider[] colliders,int CollidersCount)
        {
            if (!CachedSphereCollider.TryGet(out SphereCollider sphereCollider))
            {
                return;
            }
            for (int ColliderIndex = 0; ColliderIndex < CollidersCount; ColliderIndex++)
            {
                sphereCollider.radius = jiggleSettings.GetRadius(normalizedIndex);
                if (sphereCollider.radius <= 0)
                {
                    continue;
                }
                Collider collider = colliders[ColliderIndex];
                collider.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                if (Physics.ComputePenetration(sphereCollider, workingPosition, Quaternion.identity,collider, position, rotation, out Vector3 dir, out float dist))
                {
                    workingPosition += dir * dist;
                }
            }
        }
        public void SignalWritePosition(double time)
        {
            particleSignal.SetPosition(workingPosition, time);
        }
        private Vector3 GetProjectedPosition()
        {
            return JiggleParent.transform.TransformPoint(JiggleParent.GetParentTransform().InverseTransformPoint(JiggleParent.transform.position) * projectionAmount);
        }
        private Vector3 GetTransformPosition()
        {
            if (!hasTransform)
            {
                return GetProjectedPosition();
            }
            else
            {
                return transform.position;
            }
        }
        private Transform GetParentTransform()
        {
            if (JiggleParent != null)
            {
                return JiggleParent.transform;
            }
            return transform.parent;
        }
        private void CacheAnimationPosition(double timeAsDouble)
        {
            if (!hasTransform)
            {
                targetAnimatedBoneSignal.SetPosition(GetProjectedPosition(), timeAsDouble);
                return;
            }
            targetAnimatedBoneSignal.SetPosition(transform.position, timeAsDouble);
            transform.GetLocalPositionAndRotation(out lastValidPoseBoneLocalPosition,out lastValidPoseBoneRotation);
        }
        private Vector3 ConstrainLengthBackwards(Vector3 newPosition, float elasticity)
        {
            if (child == null)
            {
                return newPosition;
            }
            Vector3 diff = newPosition - child.workingPosition;
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, child.workingPosition + dir * child.GetLengthToParent(), elasticity);
        }
        private Vector3 ConstrainLength(Vector3 newPosition, float elasticity)
        {
            Vector3 diff = newPosition - JiggleParent.workingPosition;
            Vector3 dir = diff.normalized;
            return Vector3.Lerp(newPosition, JiggleParent.workingPosition + dir * GetLengthToParent(), elasticity);
        }
        private float GetLengthToParent()
        {
            return Vector3.Distance(currentFixedAnimatedBonePosition, JiggleParent.currentFixedAnimatedBonePosition);
        }
        public void MatchAnimationInstantly(double time, float fixedDeltaTime)
        {
            Vector3 position = GetTransformPosition();
            targetAnimatedBoneSignal.FlattenSignal(time, position, fixedDeltaTime);
            particleSignal.FlattenSignal(time, position, fixedDeltaTime);
        }
        /// <summary>
        /// Physically accurate teleportation, maintains the existing signals of motion and keeps their trajectories through a teleport. First call PrepareTeleport(), then move the character, then call FinishTeleport().
        /// Use MatchAnimationInstantly() instead if you don't want jiggles to be maintained through a teleport.
        /// </summary>
        public void PrepareTeleport()
        {
            preTeleportPosition = GetTransformPosition();
        }
        /// <summary>
        /// The companion function to PrepareTeleport, it discards all the movement that has happened since the call to PrepareTeleport, assuming that they've both been called on the same frame.
        /// </summary>
        public void FinishTeleport(double timeAsDouble,float FixedDeltaTime)
        {
            /*
            if (!preTeleportPosition.HasValue)
            {
                MatchAnimationInstantly(timeAsDouble, FixedDeltaTime);
                return;
            }
            */
            Vector3 position = GetTransformPosition();
            Vector3 diff = position - preTeleportPosition;
            targetAnimatedBoneSignal.FlattenSignal(timeAsDouble, position, FixedDeltaTime);
            particleSignal.OffsetSignal(diff);
            workingPosition += diff;
        }
        private Vector3 ConstrainAngle(Vector3 newPosition, float elasticity, float elasticitySoften)
        {
            if (!hasTransform && projectionAmount == 0f)
            {
                return newPosition;
            }
            Vector3 parentParentPosition;
            Vector3 poseParentParent;
            if (JiggleParent.JiggleParent == null)
            {
                poseParentParent = JiggleParent.currentFixedAnimatedBonePosition + (JiggleParent.currentFixedAnimatedBonePosition - currentFixedAnimatedBonePosition);
                parentParentPosition = poseParentParent;
            }
            else
            {
                parentParentPosition = JiggleParent.JiggleParent.workingPosition;
                poseParentParent = JiggleParent.JiggleParent.currentFixedAnimatedBonePosition;
            }
            Vector3 parentAimTargetPose = JiggleParent.currentFixedAnimatedBonePosition - poseParentParent;
            Vector3 parentAim = JiggleParent.workingPosition - parentParentPosition;
            Quaternion TargetPoseToPose = Quaternion.FromToRotation(parentAimTargetPose, parentAim);
            Vector3 currentPose = currentFixedAnimatedBonePosition - poseParentParent;
            Vector3 constraintTarget = TargetPoseToPose * currentPose;
            float error = Vector3.Distance(newPosition, parentParentPosition + constraintTarget);
            error /= GetLengthToParent();
            error = Mathf.Clamp01(error);
            error = Mathf.Pow(error, elasticitySoften * 2f);
            return Vector3.Lerp(newPosition, parentParentPosition + constraintTarget, elasticity * error);
        }
        public static Vector3 NextPhysicsPosition(Vector3 newPosition, Vector3 previousPosition, Vector3 localSpaceVelocity,Vector3 Gravity, float squaredDeltaTime, float gravityMultiplier, float friction, float airFriction)
        {
            Vector3 vel = newPosition - previousPosition - localSpaceVelocity;
            return newPosition + vel * (1f - airFriction) + localSpaceVelocity * (1f - friction) + Gravity * (gravityMultiplier * squaredDeltaTime);
        }
        public Vector3 DeriveFinalSolvePosition(Vector3 offset,double timeAsDouble)
        {
            extrapolatedPosition = offset + particleSignal.SamplePosition(timeAsDouble);
            return extrapolatedPosition;
        }
        public Vector3 GetCachedSolvePosition() => extrapolatedPosition;
        public void PrepareBone(double currentTime)
        {
            // If bone is not animated, return to last unadulterated pose
            if (hasTransform)
            {
                if (boneRotationChangeCheck == transform.localRotation)
                {
                    transform.localRotation = lastValidPoseBoneRotation;
                }
                if (bonePositionChangeCheck == transform.localPosition)
                {
                    transform.localPosition = lastValidPoseBoneLocalPosition;
                }
            }
            CacheAnimationPosition(currentTime);
        }
        public void PoseBone(float blend,double currentTime)
        {
            if (child == null) return; // Early exit if there's no child

            // Cache frequently accessed values
            Vector3 targetPosition = targetAnimatedBoneSignal.SamplePosition(currentTime);
            Vector3 childTargetPosition = child.targetAnimatedBoneSignal.SamplePosition(currentTime);

            // Blend positions
            Vector3 positionBlend = Vector3.Lerp(targetPosition, extrapolatedPosition, blend);
            Vector3 childPositionBlend = Vector3.Lerp(childTargetPosition, child.extrapolatedPosition, blend);

            if (JiggleParent != null)
            {
                transform.position = positionBlend;
            }

            // Calculate child position and vector differences
            Vector3 childPosition = child.GetTransformPosition();
            Vector3 cachedAnimatedVector = childPosition - positionBlend;
            Vector3 simulatedVector = childPositionBlend - positionBlend;

            // Rotate the transform based on the vector differences
            if (cachedAnimatedVector != Zero && simulatedVector != Zero)
            {
                Quaternion animPoseToPhysicsPose = Quaternion.FromToRotation(cachedAnimatedVector, simulatedVector);
                transform.rotation = animPoseToPhysicsPose * transform.rotation;
            }

            // Cache transform changes if the bone has a transform
            if (hasTransform)
            {
                transform.GetLocalPositionAndRotation(out bonePositionChangeCheck, out boneRotationChangeCheck);
            }
        }
        public void DebugDraw(Color simulateColor, Color targetColor, bool interpolated)
        {
            if (JiggleParent == null) return;
            if (interpolated)
            {
                Debug.DrawLine(extrapolatedPosition, JiggleParent.extrapolatedPosition, simulateColor, 0, false);
            }
            else
            {
                Debug.DrawLine(workingPosition, JiggleParent.workingPosition, simulateColor, 0, false);
            }
            Debug.DrawLine(currentFixedAnimatedBonePosition, JiggleParent.currentFixedAnimatedBonePosition, targetColor, 0, false);
        }
        public void OnDrawGizmos(JiggleSettingsBase jiggleSettings, double TimeAsDouble)
        {
            Vector3 pos = particleSignal.SamplePosition(TimeAsDouble);
            if (child != null)
            {
                Gizmos.Line(pos, child.particleSignal.SamplePosition(TimeAsDouble));
            }
            if (jiggleSettings != null)
            {
                Gizmos.Sphere(pos, jiggleSettings.GetRadius(normalizedIndex));
            }
        }
    }
}