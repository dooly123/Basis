using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
[BurstCompile]
public struct VerletPassJob : IJobParallelFor
{
    public NativeArray<float3> WorkingPositions;//output
    public NativeArray<float3> CurrentPositions;//input
    public NativeArray<float3> PreviousPositions;//input
    public NativeArray<float3> ParentCurrentPositions;//input
    public NativeArray<float3> ParentPreviousPositions;//input
    public NativeArray<float3> CurrentFixedAnimatedBonePositions;//input

    [ReadOnly] public NativeArray<bool> HasParent;//input
    [ReadOnly] public float3 Gravity;//input
    [ReadOnly] public float3 Wind;//input
    [ReadOnly] public float SquaredDeltaTime;//input
    [ReadOnly] public float FixedDeltaTime;//input
    [ReadOnly] public float GravityMultiplier;//input
    [ReadOnly] public float Friction;//input
    [ReadOnly] public float AirDrag;//input

    public void Execute(int index)
    {
        if (!HasParent[index])
        {
            WorkingPositions[index] = CurrentFixedAnimatedBonePositions[index];
            return;
        }

        float3 localSpaceVelocity = (CurrentPositions[index] - PreviousPositions[index]) - (ParentCurrentPositions[index] - ParentPreviousPositions[index]);

        float3 newPosition = NextPhysicsPosition(CurrentPositions[index], PreviousPositions[index], localSpaceVelocity, Gravity, SquaredDeltaTime, GravityMultiplier, Friction, AirDrag);

        WorkingPositions[index] = newPosition + Wind * (FixedDeltaTime * AirDrag);
    }

    private static float3 NextPhysicsPosition(float3 newPosition, float3 previousPosition, float3 localSpaceVelocity,float3 gravity, float squaredDeltaTime, float gravityMultiplier, float friction, float airFriction)
    {
        float3 vel = newPosition - previousPosition - localSpaceVelocity;
        return newPosition + vel * (1f - airFriction) + localSpaceVelocity * (1f - friction) + gravity * (gravityMultiplier * squaredDeltaTime);
    }
}