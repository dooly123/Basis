using UnityEngine;
namespace JigglePhysics
{
    public struct PositionSignal
    {
        public Frame previousFrame;
        public Frame currentFrame;

        public PositionSignal(Vector3 startPosition, double time)
        {
            currentFrame = previousFrame = new Frame
            {
                position = startPosition,
                time = time,
            };
        }
    }
}