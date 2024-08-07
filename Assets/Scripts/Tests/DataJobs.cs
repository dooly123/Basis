using Assets.Scripts.Networking.NetworkedAvatar;
using Unity.Jobs;

namespace Assets.Scripts.Tests
{
public struct DataJobs
{
    public JobHandle positionHandle;
    public JobHandle rotationHandle;
    public JobHandle muscleHandle;
    public UpdateAvatarPositionJob positionJob;
    public UpdateAvatarRotationJob rotationJob;
    public UpdateAvatarMusclesJob muscleJob;
}
}