using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Tests;
using Unity.Jobs;
using UnityEngine;
namespace Basis.Scripts.Networking.Smoothing
{
    public static class BasisAvatarLerp
    {
        public static string Settings = "Assets/ScriptableObjects/Avatar Lerp Data.asset";

        public static void UpdateAvatar(float SyncTiming,ref BasisAvatarData Output, BasisAvatarData LastData, BasisAvatarData Target, BasisDataJobs DataJobs, double LastSyncTime, float SmoothingSpeedPosition, float PositiondeltaTime, float MuscleLerp, float teleportThreshold)
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

            double TimeAsDouble = Time.realtimeSinceStartup;  // Use Unity's time system
            float timeElapsed = (float)(TimeAsDouble - LastSyncTime);  // Elapsed time since last update

            // Interpolate the rotation smoothly
            SmoothStep(ref Output, timeElapsed, SyncTiming, LastData, Target);
            DataJobs.muscleHandle.Complete();
        }

        // SmoothStep interpolation function for rotation (slerp)
        public static void SmoothStep(ref BasisAvatarData Output, float time, float duration, BasisAvatarData LastData, BasisAvatarData Target)
        {
            // Normalize the time between 0 and 1
            float t = time / duration;
            t = t * t * (3f - 2f * t); // Smoothstep interpolation formula (ease in/out)

            // Perform spherical linear interpolation (slerp) for rotation
            Output.Rotation = Quaternion.Slerp(LastData.Rotation, Target.Rotation, t);
        }
    }
}