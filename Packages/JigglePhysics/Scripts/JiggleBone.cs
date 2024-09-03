using UnityEngine;
namespace JigglePhysics
{
    // Uses Verlet to resolve constraints easily 
    public class JiggleBone
    {
        public bool hasTransform;
        public bool HasJiggleParent;

        public float projectionAmount;
        public float normalizedIndex;

        public Transform transform;
        public PositionSignal targetAnimatedBoneSignal;
        public JiggleBone JiggleParent;
        public JiggleBone child;
        public PositionSignal particleSignal;

        public Quaternion boneRotationChangeCheck;
        public Quaternion lastValidPoseBoneRotation;

        public Vector3 currentFixedAnimatedBonePosition;
        public Vector3 bonePositionChangeCheck;
        public Vector3 lastValidPoseBoneLocalPosition;
        public Vector3 workingPosition;
        public Vector3 preTeleportPosition;
        public Vector3 extrapolatedPosition;
    }
}