using UnityEngine;
namespace JigglePhysics
{
    // Uses Verlet to resolve constraints easily 
    public class JiggleBone
    {
        public Transform transform;

        public JiggleBone JiggleParent;
        public JiggleBone child;

        public bool hasTransform;
        public bool HasJiggleParent;

        public float projectionAmount;
        public float normalizedIndex;

        public PositionSignal targetAnimatedBoneSignal;
        public PositionSignal particleSignal;
    }
}