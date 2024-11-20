using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics; // Using Unity.Mathematics for math operations
using UnityEngine;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    [BurstCompile]
    public struct UpdateAvatarRotationJob : IJob
    {
        public Quaternion rotations;
        public Quaternion targetRotations;
        public NativeArray<float3> TransformationalOutput;
        public NativeArray<float3> TransformationalInput;
        public float Time;

        public void Execute()
        {
            // Interpolate rotations
            rotations = math.slerp(rotations, targetRotations, Time);
            // Interpolate positions
            TransformationalOutput[0] = math.lerp(TransformationalOutput[0], TransformationalInput[0], Time);

            TransformationalOutput[1] = math.lerp(TransformationalOutput[1], TransformationalInput[1], Time);
        }
    }
    [BurstCompile]
    public struct UpdateAvatarMusclesJob : IJobParallelFor
    {
        public NativeArray<float> muscles;
        public NativeArray<float> targetMuscles;
        public float Time;

        public void Execute(int index)
        {
            muscles[index] = math.lerp(muscles[index], targetMuscles[index], Time);
        }
    }
}