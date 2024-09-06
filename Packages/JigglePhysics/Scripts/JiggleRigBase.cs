using JigglePhysics;
using UnityEngine;
using static JiggleRigConstruction;
public class JiggleRigBase
{
    public InitalizationData PreInitalData = new InitalizationData();
    public RuntimeData Runtimedata = new RuntimeData();
    public JiggleBone[] JiggleBones;
    public Transform[] ComputedTransforms;
    public int simulatedPointsCount;
    public double currentFrame;
    public double previousFrame;
    public void InitalizeIndexes()
    {
        // Precompute normalized indices in a single pass
        for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
        {
            JiggleBone test = JiggleBones[SimulatedIndex];
            int distanceToRoot = 0, distanceToChild = 0;

            // Calculate distance to root
            while (test.JiggleParentIndex != -1)
            {
                test = JiggleBones[test.JiggleParentIndex];
                distanceToRoot++;
            }

            test = JiggleBones[SimulatedIndex];
            // Calculate distance to child
            while (test.childIndex != -1)
            {
                test = JiggleBones[test.childIndex];
                distanceToChild++;
            }

            int max = distanceToRoot + distanceToChild;
            PreInitalData.normalizedIndex[SimulatedIndex] = (float)distanceToRoot / max;
        }
    }
    public void MatchAnimationInstantly(int JiggleBoneIndex, double time)
    {
        Vector3 position = GetTransformPosition(JiggleBoneIndex);

        Vector3 AnimatedCurrent = Runtimedata.targetAnimatedBoneSignalCurrent[JiggleBoneIndex];
        Vector3 AnimatedPrevious = Runtimedata.targetAnimatedBoneSignalPrevious[JiggleBoneIndex];

        Vector3 particleCurrent = Runtimedata.particleSignalCurrent[JiggleBoneIndex];
        Vector3 particlePrevious = Runtimedata.particleSignalPrevious[JiggleBoneIndex];

        previousFrame = time - JiggleRigBuilder.MAX_CATCHUP_TIME * 2f;
        currentFrame = time - JiggleRigBuilder.MAX_CATCHUP_TIME;

        FlattenSignal(ref AnimatedCurrent,ref AnimatedPrevious, position);
        FlattenSignal(ref particleCurrent,ref particlePrevious, position);

        Runtimedata.targetAnimatedBoneSignalCurrent[JiggleBoneIndex] = AnimatedCurrent;
        Runtimedata.targetAnimatedBoneSignalPrevious[JiggleBoneIndex] = AnimatedPrevious;

        Runtimedata.particleSignalCurrent[JiggleBoneIndex] = particleCurrent;
        Runtimedata.particleSignalPrevious[JiggleBoneIndex] = particlePrevious;
    }
    /// <summary>
    /// Computes the projected position of a JiggleBone based on its parent JiggleBone.
    /// </summary>
    /// <param name="JiggleBone">Index of the JiggleBone.</param>
    /// <param name="JiggleParent">Index of the JiggleParent.</param>
    /// <returns>The projected position as a Vector3.</returns>
    public Vector3 GetProjectedPosition(int JiggleBone, int JiggleParent)
    {
        Transform parentTransform;

        // Get the parent transform
        if (JiggleBones[JiggleBone].JiggleParentIndex != -1)
        {
            int ParentIndex = JiggleBones[JiggleBone].JiggleParentIndex;
            parentTransform = ComputedTransforms[ParentIndex].transform;
        }
        else
        {
            parentTransform = ComputedTransforms[JiggleBone].parent;
        }

        // Compute and return the projected position
        return ComputedTransforms[JiggleParent].TransformPoint(parentTransform.InverseTransformPoint(ComputedTransforms[JiggleParent].position));
    }
    public Vector3 GetTransformPosition(int BoneIndex)
    {
        if (!Runtimedata.hasTransform[BoneIndex])
        {
            return GetProjectedPosition(BoneIndex, JiggleBones[BoneIndex].JiggleParentIndex);
        }
        else
        {
            return ComputedTransforms[BoneIndex].position;
        }
    }
    public float GetLengthToParent(int BoneIndex)
    {
        int ParentIndex = JiggleBones[BoneIndex].JiggleParentIndex;
        return Vector3.Distance(Runtimedata.currentFixedAnimatedBonePosition[BoneIndex], Runtimedata.currentFixedAnimatedBonePosition[ParentIndex]);
    }
    /// <summary>
    /// Physically accurate teleportation, maintains the existing signals of motion and keeps their trajectories through a teleport. First call PrepareTeleport(), then move the character, then call FinishTeleport().
    /// Use MatchAnimationInstantly() instead if you don't want jiggles to be maintained through a teleport.
    /// </summary>
    public void PrepareTeleport(int JiggleBone)
    {
        Runtimedata.preTeleportPosition[JiggleBone] = GetTransformPosition(JiggleBone);
    }
    public void PrepareTeleport()
    {
        for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
        {
            PrepareTeleport(PointsIndex);
        }
    }
    /// <summary>
    /// The companion function to PrepareTeleport, it discards all the movement that has happened since the call to PrepareTeleport, assuming that they've both been called on the same frame.
    /// </summary>
    public void FinishTeleport(double timeAsDouble)
    {
        previousFrame = timeAsDouble - JiggleRigBuilder.MAX_CATCHUP_TIME * 2f;
        currentFrame = timeAsDouble - JiggleRigBuilder.MAX_CATCHUP_TIME;
        for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
        {
            Vector3 position = GetTransformPosition(PointsIndex);
            Vector3 diff = position - Runtimedata.preTeleportPosition[PointsIndex];

            Vector3 AnimatedCurrent = Runtimedata.targetAnimatedBoneSignalCurrent[PointsIndex];
            Vector3 AnimatedPrevious = Runtimedata.targetAnimatedBoneSignalPrevious[PointsIndex];

            Vector3 particleCurrent = Runtimedata.particleSignalCurrent[PointsIndex];
            Vector3 particlePrevious = Runtimedata.particleSignalPrevious[PointsIndex];

            FlattenSignal(ref AnimatedCurrent,ref AnimatedPrevious, position);
            OffsetSignal(ref particleCurrent, ref particlePrevious, diff);

            Runtimedata.targetAnimatedBoneSignalCurrent[PointsIndex] = AnimatedCurrent;
            Runtimedata.targetAnimatedBoneSignalPrevious[PointsIndex] = AnimatedPrevious;

            Runtimedata.particleSignalCurrent[PointsIndex] = particleCurrent;
            Runtimedata.particleSignalPrevious[PointsIndex] = particlePrevious;

            Runtimedata.workingPosition[PointsIndex] += diff;
        }
    }
    public void FlattenSignal(ref Vector3 previousFrame, ref Vector3 currentFrame, Vector3 position)
    {
        previousFrame = position;
        currentFrame = position;
    }
    public void OffsetSignal(ref Vector3 previousFrame, ref Vector3 currentFrame, Vector3 offset)
    {
        previousFrame += offset;
        currentFrame += offset;
    }
}