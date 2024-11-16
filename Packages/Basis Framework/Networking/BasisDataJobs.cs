using Basis.Scripts.Networking.NetworkedAvatar;
using Unity.Jobs;
using UnityEngine;

namespace Basis.Scripts.Tests
{
    public struct BasisDataJobs
    {
        public JobHandle AvatarHandle;
        public JobHandle muscleHandle;
        public float DeltaTime;
        public UpdateAvatarMusclesJob muscleJob;
        public UpdateAvatarRotationJob AvatarJob;
    }
}