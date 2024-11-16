using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics; // Using Unity.Mathematics for math operations
using UnityEngine;
using static Unity.Mathematics.math;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    [System.Serializable]
    public struct BasisAvatarData
    {
        public NativeArray<float3> Vectors; // hips position, player's position, scale (3 length)
        public Quaternion Rotation; // hip rotation
        public NativeArray<float> Muscles; // 95 floats for each muscle (95 length)
        public float[] floatArray;
        // Method to deep copy the BasisAvatarData struct
        public BasisAvatarData DeepCopy(Allocator Allocator = Allocator.Persistent)
        {
            // Create a new BasisAvatarData struct for the deep copy
            BasisAvatarData copy = new BasisAvatarData
            {
                // Deep copy the NativeArrays using the appropriate methods
                Vectors = new NativeArray<float3>(Vectors.Length, Allocator)
            };
            Vectors.CopyTo(copy.Vectors);

            copy.Muscles = new NativeArray<float>(Muscles.Length, Allocator);
            Muscles.CopyTo(copy.Muscles);

            // Deep copy the float[] array (this is a simple array, so a new array is sufficient)
            copy.floatArray = new float[floatArray.Length];
            floatArray.CopyTo(copy.floatArray, 0);

            // Deep copy the rotation
            copy.Rotation = Rotation;

            return copy;
        }
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