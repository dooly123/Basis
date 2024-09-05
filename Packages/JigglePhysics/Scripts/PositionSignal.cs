using UnityEngine;
namespace JigglePhysics
{
    public struct PositionSignal
    {
        public Vector3 previousFrame;
        public Vector3 currentFrame;

        public PositionSignal(Vector3 startPosition)
        {
            currentFrame = previousFrame = startPosition;
        }
    }
}