using UnityEngine;
namespace JigglePhysics
{
    // Uses Verlet to resolve constraints easily 
    public class JiggleBone
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
        public JiggleBone(Transform transform, JiggleBone parent, float projectionAmount = 1f)
        {
            this.transform = transform;
            this.JiggleParent = parent;
            this.projectionAmount = projectionAmount;
            Vector3 position;
            if (transform != null)
            {
                transform.GetLocalPositionAndRotation(out lastValidPoseBoneLocalPosition, out lastValidPoseBoneRotation);
                position = transform.position;
            }
            else
            {
                position = JiggleRig.GetProjectedPosition(this);
            }
            double timeAsDouble = Time.timeAsDouble;
            targetAnimatedBoneSignal = new PositionSignal(position, timeAsDouble);
            particleSignal = new PositionSignal(position, timeAsDouble);

            hasTransform = transform != null;
            if (parent == null)
            {
                return;
            }
            JiggleParent.child = this;
        }
    }
}