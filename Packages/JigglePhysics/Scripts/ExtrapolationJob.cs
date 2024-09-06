using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace JigglePhysics
{
    [BurstCompile]
    public struct ExtrapolationJob : IJobParallelFor
    {
        public float Percentage;
        public Vector3 Offset;

        [ReadOnly] public NativeArray<Vector3> ParticleSignalCurrent;
        [ReadOnly] public NativeArray<Vector3> ParticleSignalPrevious;
        [WriteOnly] public NativeArray<Vector3> ExtrapolatedPosition;

        public void Execute(int index)
        {
            Vector3 currentSignal = ParticleSignalCurrent[index];
            Vector3 previousSignal = ParticleSignalPrevious[index];
            ExtrapolatedPosition[index] = Offset + ((Percentage == 0) ? previousSignal : Vector3.Lerp(previousSignal, currentSignal, Percentage));
        }
    }
}