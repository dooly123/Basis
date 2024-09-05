using Unity.Collections;
using UnityEngine;
namespace JigglePhysics
{
    public struct RuntimeData
    {
        public NativeArray<Quaternion> boneRotationChangeCheck;
        public NativeArray<Quaternion> lastValidPoseBoneRotation;
        public NativeArray<Vector3> currentFixedAnimatedBonePosition;
        public NativeArray<Vector3> bonePositionChangeCheck;
        public NativeArray<Vector3> lastValidPoseBoneLocalPosition;
        public NativeArray<Vector3> workingPosition;
        public NativeArray<Vector3> preTeleportPosition;
        public NativeArray<Vector3> extrapolatedPosition;
        public NativeArray<bool> hasTransform;
        public NativeArray<float> normalizedIndex;
        public NativeArray<PositionSignal> targetAnimatedBoneSignal;
        public NativeArray<PositionSignal> particleSignal;
    }
}