using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Tests;
using Unity.Jobs;
using Unity.Mathematics;
namespace Basis.Scripts.Networking.Smoothing
{
    public static class BasisAvatarLerp
    {
        public static string Settings = "Assets/ScriptableObjects/Avatar Lerp Data.asset";

        public static void UpdateAvatar(ref BasisAvatarData Output, BasisAvatarData Target, BasisDataJobs DataJobs, float SmoothingSpeedPosition, float PositiondeltaTime, float RotationLerp, float MuscleLerp, float teleportThreshold)
        {
            DataJobs.positionJob.targetPositions = Target.Vectors;
            DataJobs.positionJob.positions = Output.Vectors;
            DataJobs.positionJob.deltaTime = PositiondeltaTime;
            DataJobs.positionJob.smoothingSpeed = SmoothingSpeedPosition;
            DataJobs.positionJob.teleportThreshold = teleportThreshold;
            DataJobs.muscleJob.targetMuscles = Target.Muscles;
            DataJobs.muscleJob.muscles = Output.Muscles;
            DataJobs.muscleJob.lerpTime = MuscleLerp;
            DataJobs.positionHandle = DataJobs.positionJob.Schedule();
            DataJobs.muscleHandle = DataJobs.muscleJob.Schedule(95, 1, DataJobs.positionHandle);
            Output.Rotation = math.slerp(Output.Rotation, Target.Rotation, RotationLerp);
            DataJobs.muscleHandle.Complete();
        }
    }
}