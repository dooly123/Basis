using Basis.Scripts.Drivers;
using System;
using System.Collections.Generic;
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

    public string[] MuscleNames;

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
    public PoseDataAdditional First;
    public Dictionary<Vector2, PoseDataAdditional> CoordToPose = new Dictionary<Vector2, PoseDataAdditional>();
    public Vector2[] coordKeys; // Cached array of keys for optimization
    public void UpdateAllFingers(Basis.Scripts.Common.BasisTransformMapping Map, ref PoseData Current)
    {
        float Rotation = 10 * Time.deltaTime;

        GetLerpedValue(LeftThumbPercentage, out First);
        UpdateFingerPoses(Map.LeftThumbProximal, Map.LeftThumbIntermediate, Map.LeftThumbDistal, First.PoseData.LeftThumb, ref Current.LeftThumb, Map.HasLeftThumbProximal, Map.HasLeftThumbIntermediate, Map.HasLeftThumbDistal, Rotation);

        GetLerpedValue(LeftIndexPercentage, out First);
        UpdateFingerPoses(Map.LeftIndexProximal, Map.LeftIndexIntermediate, Map.LeftIndexDistal, First.PoseData.LeftIndex, ref Current.LeftIndex, Map.HasLeftIndexProximal, Map.HasLeftIndexIntermediate, Map.HasLeftIndexDistal, Rotation);

        GetLerpedValue(LeftMiddlePercentage, out First);
        UpdateFingerPoses(Map.LeftMiddleProximal, Map.LeftMiddleIntermediate, Map.LeftMiddleDistal, First.PoseData.LeftMiddle, ref Current.LeftMiddle, Map.HasLeftMiddleProximal, Map.HasLeftMiddleIntermediate, Map.HasLeftMiddleDistal, Rotation);

        GetLerpedValue(LeftRingPercentage, out First);
        UpdateFingerPoses(Map.LeftRingProximal, Map.LeftRingIntermediate, Map.LeftRingDistal, First.PoseData.LeftRing, ref Current.LeftRing, Map.HasLeftRingProximal, Map.HasLeftRingIntermediate, Map.HasLeftRingDistal, Rotation);

        GetLerpedValue(LeftLittlePercentage, out First);
        UpdateFingerPoses(Map.LeftLittleProximal, Map.LeftLittleIntermediate, Map.LeftLittleDistal, First.PoseData.LeftLittle, ref Current.LeftLittle, Map.HasLeftLittleProximal, Map.HasLeftLittleIntermediate, Map.HasLeftLittleDistal, Rotation);

        GetLerpedValue(RightThumbPercentage, out First);
        UpdateFingerPoses(Map.RightThumbProximal, Map.RightThumbIntermediate, Map.RightThumbDistal, First.PoseData.RightThumb, ref Current.RightThumb, Map.HasRightThumbProximal, Map.HasRightThumbIntermediate, Map.HasRightThumbDistal, Rotation);

        GetLerpedValue(RightIndexPercentage, out First);
        UpdateFingerPoses(Map.RightIndexProximal, Map.RightIndexIntermediate, Map.RightIndexDistal, First.PoseData.RightIndex, ref Current.RightIndex, Map.HasRightIndexProximal, Map.HasRightIndexIntermediate, Map.HasRightIndexDistal, Rotation);

        GetLerpedValue(RightMiddlePercentage, out First);
        UpdateFingerPoses(Map.RightMiddleProximal, Map.RightMiddleIntermediate, Map.RightMiddleDistal, First.PoseData.RightMiddle, ref Current.RightMiddle, Map.HasRightMiddleProximal, Map.HasRightMiddleIntermediate, Map.HasRightMiddleDistal, Rotation);

        GetLerpedValue(RightRingPercentage, out First);
        UpdateFingerPoses(Map.RightRingProximal, Map.RightRingIntermediate, Map.RightRingDistal, First.PoseData.RightRing, ref Current.RightRing, Map.HasRightRingProximal, Map.HasRightRingIntermediate, Map.HasRightRingDistal, Rotation);

        GetLerpedValue(RightLittlePercentage, out First);
        UpdateFingerPoses(Map.RightLittleProximal, Map.RightLittleIntermediate, Map.RightLittleDistal, First.PoseData.RightLittle, ref Current.RightLittle, Map.HasRightLittleProximal, Map.HasRightLittleIntermediate, Map.HasRightLittleDistal, Rotation);
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
    private bool GetLerpedValue(Vector2 percentage, out PoseDataAdditional first)
    {
        // Find closest point in the cached array
        Vector2 closestPoint = default;
        float minDistance = float.MaxValue;
        if (CoordToPose.TryGetValue(percentage, out first))
        {
            return true;
        }
        for (int i = 0; i < coordKeys.Length; i++)
        {
            float distance = Vector2.Distance(coordKeys[i], percentage);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = coordKeys[i];
            }
        }

        return CoordToPose.TryGetValue(closestPoint, out first);
    }
}