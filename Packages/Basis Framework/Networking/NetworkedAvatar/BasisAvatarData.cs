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
        public Quaternion rotations;
        public Quaternion targetRotations;
        public NativeArray<float3> TransformationalOutput;
        public NativeArray<float3> TransformationalInput;
        public float t;

        public void Execute()
        {
            // Interpolate rotations
            rotations = math.slerp(rotations, targetRotations, t);
            // Interpolate positions
            TransformationalOutput[0] = math.lerp(TransformationalOutput[0], TransformationalInput[0], t);

            TransformationalOutput[1] = math.lerp(TransformationalOutput[1], TransformationalInput[1], t);
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