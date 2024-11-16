using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Tests;
using Unity.Jobs;
using UnityEngine;
namespace Basis.Scripts.Networking.Smoothing
{
    public static class BasisAvatarLerp
    {
        public static string Settings = "Assets/ScriptableObjects/Avatar Lerp Data.asset";

        public static void UpdateAvatar(ref BasisAvatarData CurrentOutput, BasisAvatarData FutureTarget, BasisDataJobs DataJobs,float SmoothingSpeedPosition, float DeltaTime, float MuscleLerp, float teleportThreshold)
        {
            DataJobs.positionJob.targetPositions = FutureTarget.Vectors;
            DataJobs.positionJob.positions = CurrentOutput.Vectors;
            DataJobs.positionJob.deltaTime = DeltaTime;
            DataJobs.positionJob.smoothingSpeed = SmoothingSpeedPosition;
            DataJobs.positionJob.teleportThreshold = teleportThreshold;
            DataJobs.muscleJob.targetMuscles = FutureTarget.Muscles;
            DataJobs.muscleJob.muscles = CurrentOutput.Muscles;
            DataJobs.muscleJob.lerpTime = MuscleLerp;
            DataJobs.positionHandle = DataJobs.positionJob.Schedule();
            DataJobs.muscleHandle = DataJobs.muscleJob.Schedule(95, 1, DataJobs.positionHandle);
            DataJobs.muscleHandle.Complete();
        }
    }
}