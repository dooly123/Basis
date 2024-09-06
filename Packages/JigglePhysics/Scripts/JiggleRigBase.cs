using JigglePhysics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static JiggleRigConstruction;

public class JiggleRigBase
{
    public InitalizationData PreInitalData = new InitalizationData();
    public RuntimeData Runtimedata = new RuntimeData();
    public JiggleBone[] JiggleBones;
    public Transform[] ComputedTransforms;
    public int simulatedPointsCount;

    public void InitializeNativeArrays()
    {
        Runtimedata.boneRotationChangeCheck = CreateNativeArray(PreInitalData.boneRotationChangeCheck);
        Runtimedata.lastValidPoseBoneRotation = CreateNativeArray(PreInitalData.boneRotationChangeCheck);
        Runtimedata.currentFixedAnimatedBonePosition = CreateNativeArray(PreInitalData.currentFixedAnimatedBonePosition);
        Runtimedata.bonePositionChangeCheck = CreateNativeArray(PreInitalData.bonePositionChangeCheck);
        Runtimedata.lastValidPoseBoneLocalPosition = CreateNativeArray(PreInitalData.lastValidPoseBoneLocalPosition);
        Runtimedata.workingPosition = CreateNativeArray(PreInitalData.workingPosition);
        Runtimedata.preTeleportPosition = CreateNativeArray(PreInitalData.preTeleportPosition);
        Runtimedata.extrapolatedPosition = CreateNativeArray(PreInitalData.extrapolatedPosition);
        Runtimedata.hasTransform = CreateNativeArray(PreInitalData.hasTransform);
        Runtimedata.normalizedIndex = CreateNativeArray(PreInitalData.normalizedIndex);
        Runtimedata.targetAnimatedBoneSignalCurrent = CreateNativeArray(PreInitalData.targetAnimatedBoneSignalCurrent);
        Runtimedata.targetAnimatedBoneSignalPrevious = CreateNativeArray(PreInitalData.targetAnimatedBoneSignalPrevious);
        Runtimedata.particleSignalCurrent = CreateNativeArray(PreInitalData.particleSignalCurrent);
        Runtimedata.particleSignalPrevious = CreateNativeArray(PreInitalData.particleSignalPrevious);
    }

    public NativeArray<T> CreateNativeArray<T>(List<T> array) where T : struct
    {
        return new NativeArray<T>(array.ToArray(), Allocator.Persistent);
    }

    public void OnDestroy()
    {
        DisposeNativeArrays();
    }

    public void DisposeNativeArrays()
    {
        Runtimedata.boneRotationChangeCheck.Dispose();
        Runtimedata.lastValidPoseBoneRotation.Dispose();
        Runtimedata.currentFixedAnimatedBonePosition.Dispose();
        Runtimedata.bonePositionChangeCheck.Dispose();
        Runtimedata.lastValidPoseBoneLocalPosition.Dispose();
        Runtimedata.workingPosition.Dispose();
        Runtimedata.preTeleportPosition.Dispose();
        Runtimedata.extrapolatedPosition.Dispose();
        Runtimedata.hasTransform.Dispose();
        Runtimedata.normalizedIndex.Dispose();
        Runtimedata.targetAnimatedBoneSignalCurrent.Dispose();
        Runtimedata.targetAnimatedBoneSignalPrevious.Dispose();
        Runtimedata.particleSignalCurrent.Dispose();
        Runtimedata.particleSignalPrevious.Dispose();
    }

    public void InitalizeIndexes()
    {
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

    public void MatchAnimationInstantly(JiggleRigBuilder Builder, int JiggleBoneIndex, double time)
    {
        Vector3 position = GetTransformPosition(JiggleBoneIndex);

        Vector3 AnimatedCurrent = Runtimedata.targetAnimatedBoneSignalCurrent[JiggleBoneIndex];
        Vector3 AnimatedPrevious = Runtimedata.targetAnimatedBoneSignalPrevious[JiggleBoneIndex];

        Vector3 particleCurrent = Runtimedata.particleSignalCurrent[JiggleBoneIndex];
        Vector3 particlePrevious = Runtimedata.particleSignalPrevious[JiggleBoneIndex];

        Builder.LockFrame(time);

        // Inline FlattenSignal
        AnimatedCurrent = position;
        AnimatedPrevious = position;

        // Inline OffsetSignal
        particleCurrent += position - AnimatedCurrent;
        particlePrevious += position - AnimatedPrevious;

        Runtimedata.targetAnimatedBoneSignalCurrent[JiggleBoneIndex] = AnimatedCurrent;
        Runtimedata.targetAnimatedBoneSignalPrevious[JiggleBoneIndex] = AnimatedPrevious;

        Runtimedata.particleSignalCurrent[JiggleBoneIndex] = particleCurrent;
        Runtimedata.particleSignalPrevious[JiggleBoneIndex] = particlePrevious;
    }

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
        Vector3 PositionOut = parentTransform.InverseTransformPoint(ComputedTransforms[JiggleParent].position);
        return ComputedTransforms[JiggleParent].TransformPoint(PositionOut);
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

    public void FinishTeleport()
    {
        for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
        {
            Vector3 position = GetTransformPosition(PointsIndex);
            Vector3 diff = position - Runtimedata.preTeleportPosition[PointsIndex];
            Vector3 particleCurrent = Runtimedata.particleSignalCurrent[PointsIndex];
            Vector3 particlePrevious = Runtimedata.particleSignalPrevious[PointsIndex];

            // Inline OffsetSignal
            particleCurrent += diff;
            particlePrevious += diff;

            Runtimedata.targetAnimatedBoneSignalCurrent[PointsIndex] = position;
            Runtimedata.targetAnimatedBoneSignalPrevious[PointsIndex] = position;

            Runtimedata.particleSignalCurrent[PointsIndex] = particleCurrent;
            Runtimedata.particleSignalPrevious[PointsIndex] = particlePrevious;

            Runtimedata.workingPosition[PointsIndex] += diff;
        }
    }
}