using UnityEngine;
namespace JigglePhysics
{
    public static class PositionSignalHelper
    {
        public static void SetPosition(ref PositionSignal signal, Vector3 position, double time)
        {
            signal.previousFrame = signal.currentFrame;
            signal.currentFrame = new Frame
            {
                position = position,
                time = time,
            };
        }

        public static void OffsetSignal(ref PositionSignal signal, Vector3 offset)
        {
            signal.previousFrame = new Frame
            {
                position = signal.previousFrame.position + offset,
                time = signal.previousFrame.time,
            };
            signal.currentFrame = new Frame
            {
                position = signal.currentFrame.position + offset,
                time = signal.currentFrame.time,
            };
        }

        public static void FlattenSignal(ref PositionSignal signal, double time, Vector3 position)
        {
            signal.previousFrame = new Frame
            {
                position = position,
                time = time - JiggleRigBuilder.MAX_CATCHUP_TIME * 2f,
            };
            signal.currentFrame = new Frame
            {
                position = position,
                time = time - JiggleRigBuilder.MAX_CATCHUP_TIME,
            };
        }

        public static Vector3 GetCurrent(PositionSignal signal)
        {
            return signal.currentFrame.position;
        }

        public static Vector3 GetPrevious(PositionSignal signal)
        {
            return signal.previousFrame.position;
        }
        public static Vector3 SamplePosition(PositionSignal signal, double time)
        {
            double diff = signal.currentFrame.time - signal.previousFrame.time;
            if (diff == 0)
            {
                return signal.previousFrame.position;
            }
            double t = (time - signal.previousFrame.time) / diff;
            return Vector3.Lerp(signal.previousFrame.position, signal.currentFrame.position, (float)t);
        }
    }
}