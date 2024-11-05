using Basis.Scripts.Networking.NetworkedAvatar;
using Unity.Jobs;
using UnityEngine;

namespace Basis.Scripts.Tests
{
    public struct BasisDataJobs
    {
        public JobHandle positionHandle;
        public JobHandle muscleHandle;
        public float DeltaTime;
        public UpdateAvatarPositionJob positionJob;
        public UpdateAvatarMusclesJob muscleJob;
    }
}