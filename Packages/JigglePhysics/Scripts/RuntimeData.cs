using Unity.Collections;
using UnityEngine;
namespace JigglePhysics
{
    public struct RuntimeData
    {
        public Quaternion[] boneRotationChangeCheck;
        public Quaternion[] lastValidPoseBoneRotation;
        public Vector3[] currentFixedAnimatedBonePosition;
        public Vector3[] bonePositionChangeCheck;
        public Vector3[] lastValidPoseBoneLocalPosition;
        public Vector3[] preTeleportPosition;
        public bool[] hasTransform;
        public float[] normalizedIndex;

        public Vector3[] targetAnimatedBoneSignalCurrent;

        public Vector3[] targetAnimatedBoneSignalPrevious;


        public NativeArray<Vector3> workingPosition;
        public NativeArray<Vector3> particleSignalPrevious;
        public NativeArray<Vector3> extrapolatedPosition;
        public NativeArray<Vector3> particleSignalCurrent;
    }
}