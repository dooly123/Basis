using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace JigglePhysics
{
    // Define the job for updating particle signals
    [BurstCompile]
    public struct UpdateParticleSignalsJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> workingPosition;
        public NativeArray<Vector3> particleSignalCurrent;
        public NativeArray<Vector3> particleSignalPrevious;

        public void Execute(int index)
        {
            // Update particle signals
            Vector3 previousSignal = particleSignalCurrent[index];
            particleSignalCurrent[index] = workingPosition[index];
            particleSignalPrevious[index] = previousSignal;
        }
    }
}