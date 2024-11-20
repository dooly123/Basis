using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics; // Using Unity.Mathematics for math operations
using UnityEngine;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    [System.Serializable]
    public struct BasisAvatarData
    {
        public NativeArray<float3> Vectors; // hips position, player's position, scale (3 length)
        public Quaternion Rotation; // hip rotation
        public NativeArray<float> Muscles; // 90 floats for each muscle (90 length)
        public float[] floatArray;
    }
    [BurstCompile]
    public struct UpdateAvatarRotationJob : IJob
    {
        public NativeArray<Quaternion> rotations;
        public NativeArray<Quaternion> targetRotations;
        public NativeArray<Vector3> positions;
        public NativeArray<Vector3> targetPositions;
        public NativeArray<Vector3> scales;
        public NativeArray<Vector3> targetScales;
        public float t;

        public void Execute()
        {
            // Interpolate rotations
            rotations[0] = Quaternion.Slerp(rotations[0], targetRotations[0], t);
            // Interpolate positions
            positions[0] = Vector3.Lerp(positions[0], targetPositions[0], t);
            // Interpolate scales
            scales[0] = Vector3.Lerp(scales[0], targetScales[0], t);
        }
    }
    [BurstCompile]
    public struct UpdateAvatarMusclesJob : IJobParallelFor
    {
        public NativeArray<float> muscles;
        public NativeArray<float> targetMuscles;
        public float t;

        public void Execute(int index)
        {
            muscles[index] = math.lerp(muscles[index], targetMuscles[index], t);
        }
    }
}