using Unity.Jobs;

public struct DataJobs
{
    public JobHandle positionHandle;
    public JobHandle rotationHandle;
    public JobHandle muscleHandle;
    public UpdateAvatarPositionJob positionJob;
    public UpdateAvatarRotationJob rotationJob;
    public UpdateAvatarMusclesJob muscleJob;
}