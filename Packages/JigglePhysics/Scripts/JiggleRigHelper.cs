using JigglePhysics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static JiggleRigConstruction;
public static class JiggleRigHelper
{
    public static void InitializeNativeArrays(ref JiggleRig JiggleRigBase)
    {
        JiggleRigBase.TransformAccessArray = new UnityEngine.Jobs.TransformAccessArray(JiggleRigBase.RawTransforms.ToArray());
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
    public static int Initialize(JiggleRigBuilder JiggleRigBuilder, ref JiggleRig JiggleRigBase, JiggleRigLOD jiggleRigLOD)
    {
        JiggleRigBase.JiggleRigLOD = jiggleRigLOD;
        JiggleRigBase.PreInitalData = new InitalizationData();
        JiggleRigBase.Runtimedata = new RuntimeData();
        JiggleRigConstruction.InitalizeLists(ref JiggleRigBase);
        JiggleRigConstruction.CreateSimulatedPoints(ref JiggleRigBase, JiggleRigBase.ignoredTransforms, JiggleRigBase.rootTransform, null);
        int Count = JiggleRigBase.JiggleBones.Length;

        JiggleRigHelper.InitalizeIndexes(ref JiggleRigBase, Count);
        JiggleRigBuilder.TempcollidersCount.Add(Count);

        // Precompute normalized indices in a single pass
        for (int SimulatedIndex = 0; SimulatedIndex < Count; SimulatedIndex++)
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
        JiggleRigHelper.InitializeNativeArrays(ref JiggleRigBase);
        JiggleRigBase.jiggleSettingsdata = JiggleRigBase.jiggleSettings.GetData();
        bool NeedsCollision = JiggleRigBase.colliders.Length != 0;
        JiggleRigBuilder.TempNeedsCollisions.Add(NeedsCollision);
        if (NeedsCollision)
        {
            if (!CachedSphereCollider.TryGet(out JiggleRigBase.sphereCollider))
            {
                Debug.LogError("Missing Sphere Collider Bailing!");
                return Count;  // No need to proceed if there's no valid sphereCollider
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
        return Count;
    }
    public static NativeArray<T> CreateNativeArray<T>(List<T> array) where T : struct
    {
        return new NativeArray<T>(array.ToArray(), Allocator.Persistent);
    }
    public static void OnDestroy(ref JiggleRig JiggleRigBase)
    {
        DisposeNativeArrays(ref JiggleRigBase);
    }
    public static void DisposeNativeArrays(ref JiggleRig JiggleRigBase)
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
    public static void InitalizeIndexes(ref JiggleRig JiggleRigBase,int simulatedPointsCount)
    {
        for (int SimulatedIndex = 0; SimulatedIndex < simulatedPointsCount; SimulatedIndex++)
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
    public static void MatchAnimationInstantly(JiggleRigBuilder Builder, ref JiggleRig JiggleRigBase, int JiggleBoneIndex, double time)
    {
        Vector3 position = GetTransformPosition(JiggleBoneIndex, ref JiggleRigBase);

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
    public static Vector3 GetProjectedPosition(int JiggleBone, int JiggleParent, ref JiggleRig JiggleRigBase)
    {
        Transform parentTransform;

        // Get the parent transform
        if (JiggleRigBase.JiggleBones[JiggleBone].JiggleParentIndex != -1)
        {
            int ParentIndex = JiggleRigBase.JiggleBones[JiggleBone].JiggleParentIndex;
            parentTransform = JiggleRigBase.RawTransforms[ParentIndex].transform;
        }
        else
        {
            parentTransform = JiggleRigBase.RawTransforms[JiggleBone].parent;
        }

        // Compute and return the projected position
        Vector3 PositionOut = parentTransform.InverseTransformPoint(JiggleRigBase.RawTransforms[JiggleParent].position);
        return JiggleRigBase.RawTransforms[JiggleParent].TransformPoint(PositionOut);
    }
    public static Vector3 GetProjectedPositionRuntime(int JiggleBone, int JiggleParent, ref JiggleRig JiggleRigBase)
    {
        Transform parentTransform;

        // Get the parent transform
        if (JiggleRigBase.JiggleBones[JiggleBone].JiggleParentIndex != -1)
        {
            int ParentIndex = JiggleRigBase.JiggleBones[JiggleBone].JiggleParentIndex;
            parentTransform = JiggleRigBase.TransformAccessArray[ParentIndex].transform;
        }
        else
        {
            parentTransform = JiggleRigBase.TransformAccessArray[JiggleBone].parent;
        }

        // Compute and return the projected position
        Vector3 PositionOut = parentTransform.InverseTransformPoint(JiggleRigBase.TransformAccessArray[JiggleParent].position);
        return JiggleRigBase.TransformAccessArray[JiggleParent].TransformPoint(PositionOut);
    }
    public static Vector3 GetTransformPositionRuntime(int BoneIndex, ref JiggleRig JiggleRigBase)
    {
        if (!JiggleRigBase.Runtimedata.hasTransform[BoneIndex])
        {
            return GetProjectedPosition(BoneIndex, JiggleRigBase.JiggleBones[BoneIndex].JiggleParentIndex, ref JiggleRigBase);
        }
        else
        {
            return JiggleRigBase.TransformAccessArray[BoneIndex].position;
        }
    }
    public static Vector3 GetTransformPosition(int BoneIndex, ref JiggleRig JiggleRigBase)
    {
        if (!JiggleRigBase.Runtimedata.hasTransform[BoneIndex])
        {
            return GetProjectedPosition(BoneIndex, JiggleRigBase.JiggleBones[BoneIndex].JiggleParentIndex, ref JiggleRigBase);
        }
        else
        {
            return JiggleRigBase.RawTransforms[BoneIndex].position;
        }
    }
    public static void PrepareTeleport(int JiggleBone, ref JiggleRig JiggleRigBase)
    {
        JiggleRigBase.Runtimedata.preTeleportPosition[JiggleBone] = GetTransformPosition(JiggleBone, ref JiggleRigBase);
    }
    public static void PrepareTeleport(ref JiggleRig JiggleRigBase,int simulatedPointsCount)
    {
        for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
        {
            PrepareTeleport(PointsIndex, ref JiggleRigBase);
        }
    }
    public static void FinishTeleport(ref JiggleRig JiggleRigBase, int simulatedPointsCount)
    {
        for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
        {
            Vector3 position = GetTransformPosition(PointsIndex, ref JiggleRigBase);
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
    public static Vector3 ConstrainLengthBackwards(int JiggleIndex, Vector3 newPosition, float elasticity, ref JiggleRig JiggleRigBase)
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