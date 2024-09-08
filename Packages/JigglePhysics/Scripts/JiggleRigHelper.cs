using JigglePhysics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static JiggleRigConstruction;
public static class JiggleRigHelper
{
    public static void InitializeNativeArrays(ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime)
    {
        JiggleRigRuntime.TransformAccessArray = JiggleRigBase.RawTransforms.ToArray();
        JiggleRigRuntime.Runtimedata.boneRotationChangeCheck = JiggleRigBase.PreInitalData.boneRotationChangeCheck.ToArray();
        JiggleRigRuntime.Runtimedata.lastValidPoseBoneRotation = JiggleRigBase.PreInitalData.boneRotationChangeCheck.ToArray();
        JiggleRigRuntime.Runtimedata.currentFixedAnimatedBonePosition = JiggleRigBase.PreInitalData.currentFixedAnimatedBonePosition.ToArray();
        JiggleRigRuntime.Runtimedata.bonePositionChangeCheck = JiggleRigBase.PreInitalData.bonePositionChangeCheck.ToArray();
        JiggleRigRuntime.Runtimedata.lastValidPoseBoneLocalPosition = JiggleRigBase.PreInitalData.lastValidPoseBoneLocalPosition.ToArray();
        JiggleRigRuntime.Runtimedata.preTeleportPosition = JiggleRigBase.PreInitalData.preTeleportPosition.ToArray();
        JiggleRigRuntime.Runtimedata.hasTransform = JiggleRigBase.PreInitalData.hasTransform.ToArray();
        JiggleRigRuntime.Runtimedata.normalizedIndex = JiggleRigBase.PreInitalData.normalizedIndex.ToArray();
        JiggleRigRuntime.Runtimedata.targetAnimatedBoneSignalCurrent = JiggleRigBase.PreInitalData.targetAnimatedBoneSignalCurrent.ToArray();
        JiggleRigRuntime.Runtimedata.targetAnimatedBoneSignalPrevious = JiggleRigBase.PreInitalData.targetAnimatedBoneSignalPrevious.ToArray();


        JiggleRigRuntime.Runtimedata.extrapolatedPosition = CreateNativeArray(JiggleRigBase.PreInitalData.extrapolatedPosition);
        JiggleRigRuntime.Runtimedata.workingPosition = CreateNativeArray(JiggleRigBase.PreInitalData.workingPosition);
        JiggleRigRuntime.Runtimedata.particleSignalCurrent = CreateNativeArray(JiggleRigBase.PreInitalData.particleSignalCurrent);
        JiggleRigRuntime.Runtimedata.particleSignalPrevious = CreateNativeArray(JiggleRigBase.PreInitalData.particleSignalPrevious);
    }
    public static int Initialize(JiggleRigBuilder JiggleRigBuilder, ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime)
    {
        JiggleRigBase.PreInitalData = new InitalizationData();
        JiggleRigRuntime.Runtimedata = new RuntimeData();
        InitalizeLists(ref JiggleRigBase);
        CreateSimulatedPoints(ref JiggleRigBase, JiggleRigBase.ignoredTransforms, JiggleRigBase.rootTransform, null);
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
        JiggleRigHelper.InitializeNativeArrays(ref JiggleRigBase,ref JiggleRigRuntime);
        JiggleRigRuntime.jiggleSettingsdata = JiggleRigBase.jiggleSettings.GetData();
        bool NeedsCollision = JiggleRigBase.colliders.Length != 0;
        JiggleRigBuilder.TempNeedsCollisions.Add(NeedsCollision);
        if (NeedsCollision)
        {
          //  if (!CachedSphereCollider.TryGet(out JiggleRigBase.sphereCollider))
          //  {
             //   Debug.LogError("Missing Sphere Collider Bailing!");
             //   return Count;  // No need to proceed if there's no valid sphereCollider
           // }
        }
        JiggleRigRuntime.SignalJob = new UpdateParticleSignalsJob
        {
            workingPosition = JiggleRigRuntime.Runtimedata.workingPosition,
            particleSignalCurrent = JiggleRigRuntime.Runtimedata.particleSignalCurrent,
            particleSignalPrevious = JiggleRigRuntime.Runtimedata.particleSignalPrevious
        };
        JiggleRigRuntime.extrapolationJob = new ExtrapolationJob
        {
            ParticleSignalCurrent = JiggleRigRuntime.Runtimedata.particleSignalCurrent,
            ParticleSignalPrevious = JiggleRigRuntime.Runtimedata.particleSignalPrevious,
            ExtrapolatedPosition = JiggleRigRuntime.Runtimedata.extrapolatedPosition
        };
        return Count;
    }
    public static NativeArray<T> CreateNativeArray<T>(List<T> array) where T : struct
    {
        return new NativeArray<T>(array.ToArray(), Allocator.Persistent);
    }
    public static void OnDestroy(ref JiggleRig JiggleRigBase, ref JiggleRigRuntime JiggleRigRuntime)
    {
        DisposeNativeArrays(ref JiggleRigBase,ref JiggleRigRuntime);
    }
    public static void DisposeNativeArrays(ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime)
    {
       // JiggleRigRuntime.Runtimedata.boneRotationChangeCheck.Dispose();
       // JiggleRigRuntime.Runtimedata.lastValidPoseBoneRotation.Dispose();
      //  JiggleRigRuntime.Runtimedata.currentFixedAnimatedBonePosition.Dispose();
       // JiggleRigRuntime.Runtimedata.bonePositionChangeCheck.Dispose();
       // JiggleRigRuntime.Runtimedata.lastValidPoseBoneLocalPosition.Dispose();
      //  JiggleRigRuntime.Runtimedata.workingPosition.Dispose();
      //  JiggleRigRuntime.Runtimedata.preTeleportPosition.Dispose();
      //  JiggleRigRuntime.Runtimedata.extrapolatedPosition.Dispose();
      //  JiggleRigRuntime.Runtimedata.hasTransform.Dispose();
      //  JiggleRigRuntime.Runtimedata.normalizedIndex.Dispose();
      //  JiggleRigRuntime.Runtimedata.targetAnimatedBoneSignalCurrent.Dispose();
      //  JiggleRigRuntime.Runtimedata.targetAnimatedBoneSignalPrevious.Dispose();
      //  JiggleRigRuntime.Runtimedata.particleSignalCurrent.Dispose();
      //  JiggleRigRuntime.Runtimedata.particleSignalPrevious.Dispose();
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
    public static void MatchAnimationInstantly(JiggleRigBuilder Builder, ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime, int JiggleBoneIndex, double time)
    {
        Vector3 position = GetTransformPosition(JiggleBoneIndex, ref JiggleRigBase,ref JiggleRigRuntime);

        Vector3 particleCurrent = JiggleRigRuntime.Runtimedata.particleSignalCurrent[JiggleBoneIndex];
        Vector3 particlePrevious = JiggleRigRuntime.Runtimedata.particleSignalPrevious[JiggleBoneIndex];

        Builder.RecordFrame(time);

        // Inline FlattenSignal
        Vector3 AnimatedCurrent = position;
        Vector3 AnimatedPrevious = position;

        // Inline OffsetSignal
        particleCurrent += position - AnimatedCurrent;
        particlePrevious += position - AnimatedPrevious;

        JiggleRigRuntime.Runtimedata.targetAnimatedBoneSignalCurrent[JiggleBoneIndex] = AnimatedCurrent;
        JiggleRigRuntime.Runtimedata.targetAnimatedBoneSignalPrevious[JiggleBoneIndex] = AnimatedPrevious;

        JiggleRigRuntime.Runtimedata.particleSignalCurrent[JiggleBoneIndex] = particleCurrent;
        JiggleRigRuntime.Runtimedata.particleSignalPrevious[JiggleBoneIndex] = particlePrevious;
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
    public static Vector3 GetProjectedPositionRuntime(int JiggleBone, int JiggleParent, ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime)
    {
        Transform parentTransform;

        // Get the parent transform
        if (JiggleRigBase.JiggleBones[JiggleBone].JiggleParentIndex != -1)
        {
            int ParentIndex = JiggleRigBase.JiggleBones[JiggleBone].JiggleParentIndex;
            parentTransform = JiggleRigRuntime.TransformAccessArray[ParentIndex].transform;
        }
        else
        {
            parentTransform = JiggleRigRuntime.TransformAccessArray[JiggleBone].parent;
        }

        // Compute and return the projected position
        Vector3 PositionOut = parentTransform.InverseTransformPoint(JiggleRigRuntime.TransformAccessArray[JiggleParent].position);
        return JiggleRigRuntime.TransformAccessArray[JiggleParent].TransformPoint(PositionOut);
    }
    public static Vector3 GetTransformPositionRuntime(int BoneIndex, ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime)
    {
        if (!JiggleRigRuntime.Runtimedata.hasTransform[BoneIndex])
        {
            return GetProjectedPosition(BoneIndex, JiggleRigBase.JiggleBones[BoneIndex].JiggleParentIndex, ref JiggleRigBase);
        }
        else
        {
            return JiggleRigRuntime.TransformAccessArray[BoneIndex].position;
        }
    }
    public static Vector3 GetTransformPosition(int BoneIndex, ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime)
    {
        if (!JiggleRigRuntime.Runtimedata.hasTransform[BoneIndex])
        {
            return GetProjectedPosition(BoneIndex, JiggleRigBase.JiggleBones[BoneIndex].JiggleParentIndex, ref JiggleRigBase);
        }
        else
        {
            return JiggleRigBase.RawTransforms[BoneIndex].position;
        }
    }
    public static void PrepareTeleport(int JiggleBone, ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime)
    {
        JiggleRigRuntime.Runtimedata.preTeleportPosition[JiggleBone] = GetTransformPosition(JiggleBone, ref JiggleRigBase,ref JiggleRigRuntime);
    }
    public static void PrepareTeleport(ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime, int simulatedPointsCount)
    {
        for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
        {
            PrepareTeleport(PointsIndex, ref JiggleRigBase,ref JiggleRigRuntime);
        }
    }
    public static void FinishTeleport(ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime, int simulatedPointsCount)
    {
        for (int PointsIndex = 0; PointsIndex < simulatedPointsCount; PointsIndex++)
        {
            Vector3 position = GetTransformPosition(PointsIndex, ref JiggleRigBase, ref JiggleRigRuntime);
            Vector3 diff = position - JiggleRigRuntime.Runtimedata.preTeleportPosition[PointsIndex];
            Vector3 particleCurrent = JiggleRigRuntime.Runtimedata.particleSignalCurrent[PointsIndex];
            Vector3 particlePrevious = JiggleRigRuntime.Runtimedata.particleSignalPrevious[PointsIndex];

            // Inline OffsetSignal
            particleCurrent += diff;
            particlePrevious += diff;

            JiggleRigRuntime.Runtimedata.targetAnimatedBoneSignalCurrent[PointsIndex] = position;
            JiggleRigRuntime.Runtimedata.targetAnimatedBoneSignalPrevious[PointsIndex] = position;

            JiggleRigRuntime.Runtimedata.particleSignalCurrent[PointsIndex] = particleCurrent;
            JiggleRigRuntime.Runtimedata.particleSignalPrevious[PointsIndex] = particlePrevious;

            JiggleRigRuntime.Runtimedata.workingPosition[PointsIndex] += diff;
        }
    }
    public static Vector3 ConstrainLengthBackwards(int JiggleIndex, Vector3 newPosition, float elasticity, ref JiggleRig JiggleRigBase,ref JiggleRigRuntime JiggleRigRuntime)
    {
        if (JiggleRigBase.JiggleBones[JiggleIndex].childIndex == -1)
        {
            return newPosition;
        }

        Vector3 diff = newPosition - JiggleRigRuntime.Runtimedata.workingPosition[JiggleRigBase.JiggleBones[JiggleIndex].childIndex];
        Vector3 dir = diff.normalized;

        int ParentIndex = JiggleRigBase.JiggleBones[JiggleIndex].JiggleParentIndex;
        float lengthToParent = Vector3.Distance(JiggleRigRuntime.Runtimedata.currentFixedAnimatedBonePosition[JiggleIndex], JiggleRigRuntime.Runtimedata.currentFixedAnimatedBonePosition[ParentIndex]);

        return Vector3.Lerp(newPosition, JiggleRigRuntime.Runtimedata.workingPosition[JiggleRigBase.JiggleBones[JiggleIndex].childIndex] + dir * lengthToParent, elasticity);
    }
}