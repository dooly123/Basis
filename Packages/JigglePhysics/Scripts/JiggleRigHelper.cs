using JigglePhysics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public static class JiggleRigHelper
{

    public static void InitializeNativeArrays(JiggleRig JiggleRigBase)
    {
        JiggleRigBase.Runtimedata.boneRotationChangeCheck = CreateNativeArray(JiggleRigBase.PreInitalData.boneRotationChangeCheck);
        JiggleRigBase.Runtimedata.lastValidPoseBoneRotation = CreateNativeArray(JiggleRigBase.PreInitalData.boneRotationChangeCheck);
        JiggleRigBase.Runtimedata.currentFixedAnimatedBonePosition = CreateNativeArray(JiggleRigBase.PreInitalData.currentFixedAnimatedBonePosition);
        JiggleRigBase.Runtimedata.bonePositionChangeCheck = CreateNativeArray(JiggleRigBase.PreInitalData.bonePositionChangeCheck);
        JiggleRigBase.Runtimedata.lastValidPoseBoneLocalPosition = CreateNativeArray(JiggleRigBase.PreInitalData.lastValidPoseBoneLocalPosition);
        JiggleRigBase.Runtimedata.workingPosition = CreateNativeArray(JiggleRigBase.PreInitalData.workingPosition);
        JiggleRigBase.Runtimedata.preTeleportPosition = CreateNativeArray(JiggleRigBase.PreInitalData.preTeleportPosition);
        JiggleRigBase.Runtimedata.extrapolatedPosition = CreateNativeArray(JiggleRigBase.PreInitalData.extrapolatedPosition);
        JiggleRigBase.Runtimedata.hasTransform = CreateNativeArray(JiggleRigBase.PreInitalData.hasTransform);
        JiggleRigBase.Runtimedata.normalizedIndex = CreateNativeArray(JiggleRigBase.PreInitalData.normalizedIndex);
        JiggleRigBase.Runtimedata.targetAnimatedBoneSignalCurrent = CreateNativeArray(JiggleRigBase.PreInitalData.targetAnimatedBoneSignalCurrent);
        JiggleRigBase.Runtimedata.targetAnimatedBoneSignalPrevious = CreateNativeArray(JiggleRigBase.PreInitalData.targetAnimatedBoneSignalPrevious);
        JiggleRigBase.Runtimedata.particleSignalCurrent = CreateNativeArray(JiggleRigBase.PreInitalData.particleSignalCurrent);
        JiggleRigBase.Runtimedata.particleSignalPrevious = CreateNativeArray(JiggleRigBase.PreInitalData.particleSignalPrevious);
    }
    public static void Initialize(JiggleRig JiggleRigBase,JiggleRigLOD jiggleRigLOD)
    {
        JiggleRigBase.JiggleRigLOD = jiggleRigLOD;
        JiggleRigConstruction.InitalizeLists(JiggleRigBase);
        JiggleRigConstruction.CreateSimulatedPoints(JiggleRigBase, JiggleRigBase.ignoredTransforms, JiggleRigBase.rootTransform, null);
        JiggleRigHelper.InitalizeIndexes(JiggleRigBase);
        JiggleRigBase.simulatedPointsCount = JiggleRigBase.JiggleBones.Length;

        // Precompute normalized indices in a single pass
        for (int SimulatedIndex = 0; SimulatedIndex < JiggleRigBase.simulatedPointsCount; SimulatedIndex++)
        {
            JiggleBone test = JiggleRigBase.JiggleBones[SimulatedIndex];
            int distanceToRoot = 0, distanceToChild = 0;

            // Calculate distance to root
            while (test.JiggleParentIndex != -1)
            {
                test = JiggleRigBase.JiggleBones[test.JiggleParentIndex];
                distanceToRoot++;
            }
            test = JiggleRigBase.JiggleBones[SimulatedIndex];
            // Calculate distance to child
            while (test.childIndex != -1)
            {
                test = JiggleRigBase.JiggleBones[test.childIndex];
                distanceToChild++;
            }
            int max = distanceToRoot + distanceToChild;
            JiggleRigBase.PreInitalData.normalizedIndex[SimulatedIndex] = (float)distanceToRoot / max;
        }
        JiggleRigHelper.InitializeNativeArrays(JiggleRigBase);
        JiggleRigBase.jiggleSettingsdata = JiggleRigBase.jiggleSettings.GetData();
        JiggleRigBase.NeedsCollisions = JiggleRigBase.colliders.Length != 0;
        if (JiggleRigBase.NeedsCollisions)
        {
            if (!CachedSphereCollider.TryGet(out JiggleRigBase.sphereCollider))
            {
                Debug.LogError("Missing Sphere Collider Bailing!");
                return;  // No need to proceed if there's no valid sphereCollider
            }
        }
        JiggleRigBase.SignalJob = new UpdateParticleSignalsJob
        {
            workingPosition = JiggleRigBase.Runtimedata.workingPosition,
            particleSignalCurrent = JiggleRigBase.Runtimedata.particleSignalCurrent,
            particleSignalPrevious = JiggleRigBase.Runtimedata.particleSignalPrevious
        };
        JiggleRigBase.extrapolationJob = new ExtrapolationJob
        {
            ParticleSignalCurrent = JiggleRigBase.Runtimedata.particleSignalCurrent,
            ParticleSignalPrevious = JiggleRigBase.Runtimedata.particleSignalPrevious,
            ExtrapolatedPosition = JiggleRigBase.Runtimedata.extrapolatedPosition
        };
    }
    public static NativeArray<T> CreateNativeArray<T>(List<T> array) where T : struct
    {
        return new NativeArray<T>(array.ToArray(), Allocator.Persistent);
    }

    public static void OnDestroy(JiggleRig JiggleRigBase)
    {
        DisposeNativeArrays(JiggleRigBase);
    }

    public static void DisposeNativeArrays(JiggleRig JiggleRigBase)
    {
        JiggleRigBase.Runtimedata.boneRotationChangeCheck.Dispose();
        JiggleRigBase.Runtimedata.lastValidPoseBoneRotation.Dispose();
        JiggleRigBase.Runtimedata.currentFixedAnimatedBonePosition.Dispose();
        JiggleRigBase.Runtimedata.bonePositionChangeCheck.Dispose();
        JiggleRigBase.Runtimedata.lastValidPoseBoneLocalPosition.Dispose();
        JiggleRigBase.Runtimedata.workingPosition.Dispose();
        JiggleRigBase.Runtimedata.preTeleportPosition.Dispose();
        JiggleRigBase.Runtimedata.extrapolatedPosition.Dispose();
        JiggleRigBase.Runtimedata.hasTransform.Dispose();
        JiggleRigBase.Runtimedata.normalizedIndex.Dispose();
        JiggleRigBase.Runtimedata.targetAnimatedBoneSignalCurrent.Dispose();
        JiggleRigBase.Runtimedata.targetAnimatedBoneSignalPrevious.Dispose();
        JiggleRigBase.Runtimedata.particleSignalCurrent.Dispose();
        JiggleRigBase.Runtimedata.particleSignalPrevious.Dispose();
    }

    public static void InitalizeIndexes(JiggleRig JiggleRigBase)
    {
        for (int SimulatedIndex = 0; SimulatedIndex < JiggleRigBase.simulatedPointsCount; SimulatedIndex++)
        {
            JiggleBone test = JiggleRigBase.JiggleBones[SimulatedIndex];
            int distanceToRoot = 0, distanceToChild = 0;

            // Calculate distance to root
            while (test.JiggleParentIndex != -1)
            {
                test = JiggleRigBase.JiggleBones[test.JiggleParentIndex];
                distanceToRoot++;
            }

            test = JiggleRigBase.JiggleBones[SimulatedIndex];
            // Calculate distance to child
            while (test.childIndex != -1)
            {
                test = JiggleRigBase.JiggleBones[test.childIndex];
                distanceToChild++;
            }

            int max = distanceToRoot + distanceToChild;
            JiggleRigBase.PreInitalData.normalizedIndex[SimulatedIndex] = (float)distanceToRoot / max;
        }
    }

    public static void MatchAnimationInstantly(JiggleRigBuilder Builder, JiggleRig JiggleRigBase, int JiggleBoneIndex, double time)
    {
        Vector3 position = GetTransformPosition(JiggleBoneIndex, JiggleRigBase);

        Vector3 AnimatedCurrent = JiggleRigBase.Runtimedata.targetAnimatedBoneSignalCurrent[JiggleBoneIndex];
        Vector3 AnimatedPrevious = JiggleRigBase.Runtimedata.targetAnimatedBoneSignalPrevious[JiggleBoneIndex];

        Vector3 particleCurrent = JiggleRigBase.Runtimedata.particleSignalCurrent[JiggleBoneIndex];
        Vector3 particlePrevious = JiggleRigBase.Runtimedata.particleSignalPrevious[JiggleBoneIndex];

        Builder.RecordFrame(time);

        // Inline FlattenSignal
        AnimatedCurrent = position;
        AnimatedPrevious = position;

        // Inline OffsetSignal
        particleCurrent += position - AnimatedCurrent;
        particlePrevious += position - AnimatedPrevious;

        JiggleRigBase.Runtimedata.targetAnimatedBoneSignalCurrent[JiggleBoneIndex] = AnimatedCurrent;
        JiggleRigBase.Runtimedata.targetAnimatedBoneSignalPrevious[JiggleBoneIndex] = AnimatedPrevious;

        JiggleRigBase.Runtimedata.particleSignalCurrent[JiggleBoneIndex] = particleCurrent;
        JiggleRigBase.Runtimedata.particleSignalPrevious[JiggleBoneIndex] = particlePrevious;
    }

    public static Vector3 GetProjectedPosition(int JiggleBone, int JiggleParent, JiggleRig JiggleRigBase)
    {
        Transform parentTransform;

        // Get the parent transform
        if (JiggleRigBase.JiggleBones[JiggleBone].JiggleParentIndex != -1)
        {
            int ParentIndex = JiggleRigBase.JiggleBones[JiggleBone].JiggleParentIndex;
            parentTransform = JiggleRigBase.ComputedTransforms[ParentIndex].transform;
        }
        else
        {
            parentTransform = JiggleRigBase.ComputedTransforms[JiggleBone].parent;
        }

        // Compute and return the projected position
        Vector3 PositionOut = parentTransform.InverseTransformPoint(JiggleRigBase.ComputedTransforms[JiggleParent].position);
        return JiggleRigBase.ComputedTransforms[JiggleParent].TransformPoint(PositionOut);
    }

    public static Vector3 GetTransformPosition(int BoneIndex, JiggleRig JiggleRigBase)
    {
        if (!JiggleRigBase.Runtimedata.hasTransform[BoneIndex])
        {
            return GetProjectedPosition(BoneIndex, JiggleRigBase.JiggleBones[BoneIndex].JiggleParentIndex, JiggleRigBase);
        }
        else
        {
            return JiggleRigBase.ComputedTransforms[BoneIndex].position;
        }
    }

    public static void PrepareTeleport(int JiggleBone, JiggleRig JiggleRigBase)
    {
        JiggleRigBase.Runtimedata.preTeleportPosition[JiggleBone] = GetTransformPosition(JiggleBone, JiggleRigBase);
    }

    public static void PrepareTeleport(JiggleRig JiggleRigBase)
    {
        for (int PointsIndex = 0; PointsIndex < JiggleRigBase.simulatedPointsCount; PointsIndex++)
        {
            PrepareTeleport(PointsIndex, JiggleRigBase);
        }
    }

    public static void FinishTeleport(JiggleRig JiggleRigBase)
    {
        for (int PointsIndex = 0; PointsIndex < JiggleRigBase.simulatedPointsCount; PointsIndex++)
        {
            Vector3 position = GetTransformPosition(PointsIndex, JiggleRigBase);
            Vector3 diff = position - JiggleRigBase.Runtimedata.preTeleportPosition[PointsIndex];
            Vector3 particleCurrent = JiggleRigBase.Runtimedata.particleSignalCurrent[PointsIndex];
            Vector3 particlePrevious = JiggleRigBase.Runtimedata.particleSignalPrevious[PointsIndex];

            // Inline OffsetSignal
            particleCurrent += diff;
            particlePrevious += diff;

            JiggleRigBase.Runtimedata.targetAnimatedBoneSignalCurrent[PointsIndex] = position;
            JiggleRigBase.Runtimedata.targetAnimatedBoneSignalPrevious[PointsIndex] = position;

            JiggleRigBase.Runtimedata.particleSignalCurrent[PointsIndex] = particleCurrent;
            JiggleRigBase.Runtimedata.particleSignalPrevious[PointsIndex] = particlePrevious;

            JiggleRigBase.Runtimedata.workingPosition[PointsIndex] += diff;
        }
    }
    public static Vector3 ConstrainLengthBackwards(int JiggleIndex, Vector3 newPosition, float elasticity, JiggleRig JiggleRigBase)
    {
        if (JiggleRigBase.JiggleBones[JiggleIndex].childIndex == -1)
        {
            return newPosition;
        }

        Vector3 diff = newPosition - JiggleRigBase.Runtimedata.workingPosition[JiggleRigBase.JiggleBones[JiggleIndex].childIndex];
        Vector3 dir = diff.normalized;

        int ParentIndex = JiggleRigBase.JiggleBones[JiggleIndex].JiggleParentIndex;
        float lengthToParent = Vector3.Distance(JiggleRigBase.Runtimedata.currentFixedAnimatedBonePosition[JiggleIndex], JiggleRigBase.Runtimedata.currentFixedAnimatedBonePosition[ParentIndex]);

        return Vector3.Lerp(newPosition, JiggleRigBase.Runtimedata.workingPosition[JiggleRigBase.JiggleBones[JiggleIndex].childIndex] + dir * lengthToParent, elasticity);
    }
}