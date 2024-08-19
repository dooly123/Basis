using Basis.Scripts.Networking.NetworkedAvatar;
using Unity.Jobs;

namespace Basis.Scripts.Tests
{
public struct BasisDataJobs
{
    public JobHandle positionHandle;
    public JobHandle rotationHandle;
    public JobHandle muscleHandle;
    public UpdateAvatarPositionJob positionJob;
    public UpdateAvatarRotationJob rotationJob;
    public UpdateAvatarMusclesJob muscleJob;
}
}