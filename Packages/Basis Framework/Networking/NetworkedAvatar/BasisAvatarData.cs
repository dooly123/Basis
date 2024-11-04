using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    [System.Serializable]
    public struct BasisAvatarData
    {
        public NativeArray<Vector3> Vectors;//hips positon,players position, scale. (3 length)
        public NativeArray<Quaternion> Quaternions;//hip rotation rotation (1 length)
        public NativeArray<float> Muscles;//95 floats for each muscle. (95 length)
        public float[] floatArray;
    }
[BurstCompile]
public struct UpdateAvatarPositionJob : IJob
{
    public NativeArray<Vector3> positions;
    public NativeArray<Vector3> targetPositions;
    public float LerpTime;
    public float teleportThreshold;

    public void Execute()
    {
        float distance = Vector3.Distance(positions[0], targetPositions[0]);

        if (distance > teleportThreshold)
        {
            positions[0] = targetPositions[0];
            positions[1] = targetPositions[1];
            positions[2] = targetPositions[2];
        }
        else
        {
            positions[0] = Vector3.Lerp(positions[0], targetPositions[0], LerpTime);
            positions[1] = Vector3.Lerp(positions[1], targetPositions[1], LerpTime);
            positions[2] = Vector3.Lerp(positions[2], targetPositions[2], LerpTime);
        }
    }
}

[BurstCompile]
public struct UpdateAvatarRotationJob : IJob
{
    public NativeArray<Quaternion> rotations;
    public NativeArray<Quaternion> targetRotations;
    public float LerpTime;

    public void Execute()
    {
        rotations[0] = Quaternion.Slerp(rotations[0], targetRotations[0], LerpTime);
    }
}

[BurstCompile]
public struct UpdateAvatarMusclesJob : IJobParallelFor
{
    public NativeArray<float> muscles;
    public NativeArray<float> targetMuscles;
    public float LerpTime;

    public void Execute(int index)
    {
        muscles[index] = Mathf.Lerp(muscles[index], targetMuscles[index], LerpTime);
    }
}
}