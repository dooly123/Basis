using Basis.Scripts.Drivers;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static BasisMuscleDriver;
[DefaultExecutionOrder(15001)]
public abstract class BasisBaseMuscleDriver : MonoBehaviour
{
    public Animator Animator; // Reference to the Animator component
    public HumanPoseHandler poseHandler;
    public BasisLocalAvatarDriver BasisLocalAvatarDriver;
    public HumanPose pose;
    public float[] LeftThumb;
    public float[] LeftIndex;
    public float[] LeftMiddle;
    public float[] LeftRing;
    public float[] LeftLittle;

    public float[] RightThumb;
    public float[] RightIndex;
    public float[] RightMiddle;
    public float[] RightRing;
    public float[] RightLittle;
    public Vector2 LeftThumbPercentage;
    public Vector2 LeftIndexPercentage;
    public Vector2 LeftMiddlePercentage;
    public Vector2 LeftRingPercentage;
    public Vector2 LeftLittlePercentage;

    public Vector2 RightThumbPercentage;
    public Vector2 RightIndexPercentage;
    public Vector2 RightMiddlePercentage;
    public Vector2 RightRingPercentage;
    public Vector2 RightLittlePercentage;

    public Vector2 LastLeftThumbPercentage = new Vector2(-1.1f, -1.1f);
    public Vector2 LastLeftIndexPercentage = new Vector2(-1.1f, -1.1f);
    public Vector2 LastLeftMiddlePercentage = new Vector2(-1.1f, -1.1f);
    public Vector2 LastLeftRingPercentage = new Vector2(-1.1f, -1.1f);
    public Vector2 LastLeftLittlePercentage = new Vector2(-1.1f, -1.1f);

    public Vector2 LastRightThumbPercentage = new Vector2(-1.1f, -1.1f);
    public Vector2 LastRightIndexPercentage = new Vector2(-1.1f, -1.1f);
    public Vector2 LastRightMiddlePercentage = new Vector2(-1.1f, -1.1f);
    public Vector2 LastRightRingPercentage = new Vector2(-1.1f, -1.1f);
    public Vector2 LastRightLittlePercentage = new Vector2(-1.1f, -1.1f);
    // Dictionary to store the mapping
    public Dictionary<Vector2, PoseData> pointMap;

    [SerializeField]
    public SquarePoseData BoxedPoseData;

    public void RecordCurrentPose(ref PoseData poseData)
    {
        Basis.Scripts.Common.BasisTransformMapping Mapping = BasisLocalAvatarDriver.References;

        poseData.LeftThumb = RecordFingerPoses(Mapping.LeftThumbProximal, Mapping.LeftThumbIntermediate, Mapping.LeftThumbDistal, Mapping.HasLeftThumbProximal, Mapping.HasLeftThumbIntermediate, Mapping.HasLeftThumbDistal);

        poseData.LeftIndex = RecordFingerPoses(Mapping.LeftIndexProximal, Mapping.LeftIndexIntermediate, Mapping.LeftIndexDistal, Mapping.HasLeftIndexProximal, Mapping.HasLeftIndexIntermediate, Mapping.HasLeftIndexDistal);

        poseData.LeftMiddle = RecordFingerPoses(Mapping.LeftMiddleProximal, Mapping.LeftMiddleIntermediate, Mapping.LeftMiddleDistal, Mapping.HasLeftMiddleProximal, Mapping.HasLeftMiddleIntermediate, Mapping.HasLeftMiddleDistal);

        poseData.LeftRing = RecordFingerPoses(Mapping.LeftRingProximal, Mapping.LeftRingIntermediate, Mapping.LeftRingDistal, Mapping.HasLeftRingProximal, Mapping.HasLeftRingIntermediate, Mapping.HasLeftRingDistal);

        poseData.LeftLittle = RecordFingerPoses(Mapping.LeftLittleProximal, Mapping.LeftLittleIntermediate, Mapping.LeftLittleDistal, Mapping.HasLeftLittleProximal, Mapping.HasLeftLittleIntermediate, Mapping.HasLeftLittleDistal);

        poseData.RightThumb = RecordFingerPoses(Mapping.RightThumbProximal, Mapping.RightThumbIntermediate, Mapping.RightThumbDistal, Mapping.HasRightThumbProximal, Mapping.HasRightThumbIntermediate, Mapping.HasRightThumbDistal);

        poseData.RightIndex = RecordFingerPoses(Mapping.RightIndexProximal, Mapping.RightIndexIntermediate, Mapping.RightIndexDistal, Mapping.HasRightIndexProximal, Mapping.HasRightIndexIntermediate, Mapping.HasRightIndexDistal);

        poseData.RightMiddle = RecordFingerPoses(Mapping.RightMiddleProximal, Mapping.RightMiddleIntermediate, Mapping.RightMiddleDistal, Mapping.HasRightMiddleProximal, Mapping.HasRightMiddleIntermediate, Mapping.HasRightMiddleDistal);

        poseData.RightRing = RecordFingerPoses(Mapping.RightRingProximal, Mapping.RightRingIntermediate, Mapping.RightRingDistal, Mapping.HasRightRingProximal, Mapping.HasRightRingIntermediate, Mapping.HasRightRingDistal);

        poseData.RightLittle = RecordFingerPoses(Mapping.RightLittleProximal, Mapping.RightLittleIntermediate, Mapping.RightLittleDistal, Mapping.HasRightLittleProximal, Mapping.HasRightLittleIntermediate, Mapping.HasRightLittleDistal);
    }
    public MuscleLocalPose[] RecordFingerPoses(Transform proximal, Transform intermediate, Transform distal, bool hasProximal, bool hasIntermediate, bool hasDistal)
    {
        BasisMuscleDriver.MuscleLocalPose[] fingerPoses = new BasisMuscleDriver.MuscleLocalPose[3];
        fingerPoses[0] = ConvertToPose(proximal, hasProximal);
        fingerPoses[1] = ConvertToPose(intermediate, hasIntermediate);
        fingerPoses[2] = ConvertToPose(distal, hasDistal);
        return fingerPoses;
    }
    public MuscleLocalPose ConvertToPose(Transform Trans, bool HasTrans)
    {
        BasisMuscleDriver.MuscleLocalPose pose = new BasisMuscleDriver.MuscleLocalPose();
        if (HasTrans)
        {
            Trans.GetLocalPositionAndRotation(out pose.position, out pose.rotation);
        }
        return pose;
    }
    public void SetAndRecordPose(float fillValue, ref PoseData poseData, float Splane)
    {
        // Apply muscle data to both hands
        SetMuscleData(LeftThumb, fillValue, Splane);
        SetMuscleData(LeftIndex, fillValue, Splane);
        SetMuscleData(LeftMiddle, fillValue, Splane);
        SetMuscleData(LeftRing, fillValue, Splane);
        SetMuscleData(LeftLittle, fillValue, Splane);

        SetMuscleData(RightThumb, fillValue, Splane);
        SetMuscleData(RightIndex, fillValue, Splane);
        SetMuscleData(RightMiddle, fillValue, Splane);
        SetMuscleData(RightRing, fillValue, Splane);
        SetMuscleData(RightLittle, fillValue, Splane);

        ApplyMuscleData();
        poseHandler.SetHumanPose(ref pose);
        RecordCurrentPose(ref poseData);
    }
    public void ApplyMuscleData()
    {
        // Update the finger muscle values in the poses array using Array.Copy
        System.Array.Copy(LeftThumb, 0, pose.muscles, 55, 4);
        System.Array.Copy(LeftIndex, 0, pose.muscles, 59, 4);
        System.Array.Copy(LeftMiddle, 0, pose.muscles, 63, 4);
        System.Array.Copy(LeftRing, 0, pose.muscles, 67, 4);
        System.Array.Copy(LeftLittle, 0, pose.muscles, 71, 4);

        System.Array.Copy(RightThumb, 0, pose.muscles, 75, 4);
        System.Array.Copy(RightIndex, 0, pose.muscles, 79, 4);
        System.Array.Copy(RightMiddle, 0, pose.muscles, 83, 4);
        System.Array.Copy(RightRing, 0, pose.muscles, 87, 4);
        System.Array.Copy(RightLittle, 0, pose.muscles, 91, 4);
    }
    public void SetMuscleData(float[] muscleArray, float fillValue, float specificValue)
    {
        Array.Fill(muscleArray, fillValue);
        muscleArray[1] = specificValue;
    }
    public Dictionary<Vector2, PoseDataAdditional> CoordToPose = new Dictionary<Vector2, PoseDataAdditional>();
    public Vector2[] coordKeys; // Cached array of keys for optimization

    public PoseDataAdditional LeftThumbAdditional;
    public PoseDataAdditional LeftIndexAdditional;
    public PoseDataAdditional LeftMiddleAdditional;
    public PoseDataAdditional LeftRingAdditional;
    public PoseDataAdditional LeftLittleAdditional;

    public PoseDataAdditional RightThumbAdditional;
    public PoseDataAdditional RightIndexAdditional;
    public PoseDataAdditional RightMiddleAdditional;
    public PoseDataAdditional RightRingAdditional;
    public PoseDataAdditional RightLittleAdditional;
    public NativeArray<Vector2> coordKeysArray;
    public NativeArray<float> distancesArray;
    public NativeArray<int> closestIndexArray;

    public void UpdateAllFingers(Basis.Scripts.Common.BasisTransformMapping Map, ref PoseData Current)
    {
        float Rotation = 10 * Time.deltaTime;

        // Update Thumb
        if (LeftThumbPercentage != LastLeftThumbPercentage)
        {
            GetClosestValue(LeftThumbPercentage, out LeftThumbAdditional);
            LastLeftThumbPercentage = LeftThumbPercentage;
        }
        UpdateFingerPoses(Map.LeftThumbProximal, Map.LeftThumbIntermediate, Map.LeftThumbDistal, LeftThumbAdditional.PoseData.LeftThumb, ref Current.LeftThumb, Map.HasLeftThumbProximal, Map.HasLeftThumbIntermediate, Map.HasLeftThumbDistal, Rotation);

        // Update Index
        if (LeftIndexPercentage != LastLeftIndexPercentage)
        {
            GetClosestValue(LeftIndexPercentage, out LeftIndexAdditional);
            LastLeftIndexPercentage = LeftIndexPercentage;
        }
        UpdateFingerPoses(Map.LeftIndexProximal, Map.LeftIndexIntermediate, Map.LeftIndexDistal, LeftIndexAdditional.PoseData.LeftIndex, ref Current.LeftIndex, Map.HasLeftIndexProximal, Map.HasLeftIndexIntermediate, Map.HasLeftIndexDistal, Rotation);

        // Update Middle
        if (LeftMiddlePercentage != LastLeftMiddlePercentage)
        {
            GetClosestValue(LeftMiddlePercentage, out LeftMiddleAdditional);
            LastLeftMiddlePercentage = LeftMiddlePercentage;
        }
        UpdateFingerPoses(Map.LeftMiddleProximal, Map.LeftMiddleIntermediate, Map.LeftMiddleDistal, LeftMiddleAdditional.PoseData.LeftMiddle, ref Current.LeftMiddle, Map.HasLeftMiddleProximal, Map.HasLeftMiddleIntermediate, Map.HasLeftMiddleDistal, Rotation);

        // Update Ring
        if (LeftRingPercentage != LastLeftRingPercentage)
        {
            GetClosestValue(LeftRingPercentage, out LeftRingAdditional);
            LastLeftRingPercentage = LeftRingPercentage;
        }
        UpdateFingerPoses(Map.LeftRingProximal, Map.LeftRingIntermediate, Map.LeftRingDistal, LeftRingAdditional.PoseData.LeftRing, ref Current.LeftRing, Map.HasLeftRingProximal, Map.HasLeftRingIntermediate, Map.HasLeftRingDistal, Rotation);

        // Update Little
        if (LeftLittlePercentage != LastLeftLittlePercentage)
        {
            GetClosestValue(LeftLittlePercentage, out LeftLittleAdditional);
            LastLeftLittlePercentage = LeftLittlePercentage;
        }
        UpdateFingerPoses(Map.LeftLittleProximal, Map.LeftLittleIntermediate, Map.LeftLittleDistal, LeftLittleAdditional.PoseData.LeftLittle, ref Current.LeftLittle, Map.HasLeftLittleProximal, Map.HasLeftLittleIntermediate, Map.HasLeftLittleDistal, Rotation);

        // Update Right Thumb
        if (RightThumbPercentage != LastRightThumbPercentage)
        {
            GetClosestValue(RightThumbPercentage, out RightThumbAdditional);
            LastRightThumbPercentage = RightThumbPercentage;
        }
        UpdateFingerPoses(Map.RightThumbProximal, Map.RightThumbIntermediate, Map.RightThumbDistal, RightThumbAdditional.PoseData.RightThumb, ref Current.RightThumb, Map.HasRightThumbProximal, Map.HasRightThumbIntermediate, Map.HasRightThumbDistal, Rotation);

        // Update Right Index
        if (RightIndexPercentage != LastRightIndexPercentage)
        {
            GetClosestValue(RightIndexPercentage, out RightIndexAdditional);
            LastRightIndexPercentage = RightIndexPercentage;
        }
        UpdateFingerPoses(Map.RightIndexProximal, Map.RightIndexIntermediate, Map.RightIndexDistal, RightIndexAdditional.PoseData.RightIndex, ref Current.RightIndex, Map.HasRightIndexProximal, Map.HasRightIndexIntermediate, Map.HasRightIndexDistal, Rotation);

        // Update Right Middle
        if (RightMiddlePercentage != LastRightMiddlePercentage)
        {
            GetClosestValue(RightMiddlePercentage, out RightMiddleAdditional);
            LastRightMiddlePercentage = RightMiddlePercentage;
        }
        UpdateFingerPoses(Map.RightMiddleProximal, Map.RightMiddleIntermediate, Map.RightMiddleDistal, RightMiddleAdditional.PoseData.RightMiddle, ref Current.RightMiddle, Map.HasRightMiddleProximal, Map.HasRightMiddleIntermediate, Map.HasRightMiddleDistal, Rotation);

        // Update Right Ring
        if (RightRingPercentage != LastRightRingPercentage)
        {
            GetClosestValue(RightRingPercentage, out RightRingAdditional);
            LastRightRingPercentage = RightRingPercentage;
        }
        UpdateFingerPoses(Map.RightRingProximal, Map.RightRingIntermediate, Map.RightRingDistal, RightRingAdditional.PoseData.RightRing, ref Current.RightRing, Map.HasRightRingProximal, Map.HasRightRingIntermediate, Map.HasRightRingDistal, Rotation);

        // Update Right Little
        if (RightLittlePercentage != LastRightLittlePercentage)
        {
            GetClosestValue(RightLittlePercentage, out RightLittleAdditional);
            LastRightLittlePercentage = RightLittlePercentage;
        }
        UpdateFingerPoses(Map.RightLittleProximal, Map.RightLittleIntermediate, Map.RightLittleDistal, RightLittleAdditional.PoseData.RightLittle, ref Current.RightLittle, Map.HasRightLittleProximal, Map.HasRightLittleIntermediate, Map.HasRightLittleDistal, Rotation);
    }
    public void UpdateFingerPoses(Transform proximal, Transform intermediate, Transform distal, MuscleLocalPose[] Poses, ref MuscleLocalPose[] currentPoses, bool hasProximal, bool hasIntermediate, bool hasDistal, float Rotation)
    {
        UpdatePose(proximal, ref currentPoses[0], Poses[0], hasProximal, Rotation);
        UpdatePose(intermediate, ref currentPoses[1], Poses[1], hasIntermediate, Rotation);
        UpdatePose(distal, ref currentPoses[2], Poses[2], hasDistal, Rotation);
    }
    public void UpdatePose(Transform trans, ref MuscleLocalPose currentPose, MuscleLocalPose First, bool hasTransform, float Rotation)
    {
        if (hasTransform)
        {
            currentPose.position = Vector3.Lerp(currentPose.position, First.position, Rotation);
            currentPose.rotation = Quaternion.Slerp(currentPose.rotation, First.rotation, Rotation);
            trans.SetLocalPositionAndRotation(currentPose.position, currentPose.rotation);
        }
    }
    public bool GetClosestValue(Vector2 percentage, out PoseDataAdditional first)
    {
        // Create and schedule the distance computation job
        var distanceJob = new FindClosestPointJob
        {
            target = percentage,
            coordKeys = coordKeysArray,
            distances = distancesArray
        };

        JobHandle distanceJobHandle = distanceJob.Schedule(coordKeysArray.Length, 64);
        distanceJobHandle.Complete();

        // Create and schedule the parallel reduction job
        var reductionJob = new FindMinDistanceJob
        {
            distances = distancesArray,
            closestIndex = closestIndexArray
        };

        JobHandle reductionJobHandle = reductionJob.Schedule();
        reductionJobHandle.Complete();

        // Find the closest point
        int closestIndex = closestIndexArray[0];
        Vector2 closestPoint = coordKeysArray[closestIndex];

        // Return result
        return CoordToPose.TryGetValue(closestPoint, out first);
    }

    [BurstCompile]
    private struct FindClosestPointJob : IJobParallelFor
    {
        public Vector2 target;
        public NativeArray<Vector2> coordKeys;
        public NativeArray<float> distances;

        public void Execute(int index)
        {
            float distance = Vector2.Distance(coordKeys[index], target);
            distances[index] = distance;
        }
    }

    [BurstCompile]
    private struct FindMinDistanceJob : IJob
    {
        [ReadOnly] public NativeArray<float> distances;
        public NativeArray<int> closestIndex;

        public void Execute()
        {
            float minDistance = float.MaxValue;
            int minIndex = -1;

            for (int i = 0; i < distances.Length; i++)
            {
                if (distances[i] < minDistance)
                {
                    minDistance = distances[i];
                    minIndex = i;
                }
            }

            closestIndex[0] = minIndex;
        }
    }
}