using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Tests;
using Unity.Jobs;
using UnityEngine.Animations;

namespace Basis.Scripts.Networking.Smoothing
{
public static class BasisAvatarLerp
{
    public static string Settings = "Assets/ScriptableObjects/Avatar Lerp Data.asset";

    public static void UpdateAvatar(ref BasisAvatarData Output, BasisAvatarData Target, BasisDataJobs DataJobs, float PositionLerp, float RotationLerp, float MuscleLerp, float teleportThreshold)
    {
        DataJobs.positionJob.targetPositions = Target.Vectors;
        DataJobs.positionJob.positions = Output.Vectors;
        DataJobs.positionJob.LerpTime = PositionLerp;
        DataJobs.positionJob.teleportThreshold = teleportThreshold;

        DataJobs.rotationJob.targetRotations = Target.Quaternions;
        DataJobs.rotationJob.rotations = Output.Quaternions;
        DataJobs.rotationJob.LerpTime = RotationLerp;

        DataJobs.muscleJob.targetMuscles = Target.Muscles;
        DataJobs.muscleJob.muscles = Output.Muscles;
        DataJobs.muscleJob.LerpTime = MuscleLerp;

        DataJobs.positionHandle =  DataJobs.positionJob.Schedule();
        DataJobs.rotationHandle = DataJobs.rotationJob.Schedule(DataJobs.positionHandle);
        DataJobs.muscleHandle = DataJobs.muscleJob.Schedule(95, 1, DataJobs.rotationHandle);

        DataJobs.muscleHandle.Complete();
    }
}
}