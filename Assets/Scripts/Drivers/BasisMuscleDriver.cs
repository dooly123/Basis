using Basis.Scripts.Drivers;
using UnityEngine;
[DefaultExecutionOrder(15001)]
public class BasisMuscleDriver : MonoBehaviour
{
    public Animator Animator; // Reference to the Animator component
    public HumanPoseHandler poseHandler;
    public HumanPose pose;
    [SerializeField]
    public PoseData negativeOnePoseData;
    [SerializeField]
    public PoseData positiveOnePoseData;
    [SerializeField]
    public PoseData RestingOnePoseData;
    [SerializeField]
    public PoseData CurrentOnPoseData;
    public BasisLocalAvatarDriver BasisLocalAvatarDriver;
    public float LeftThumbPercentage;
    public void Initialize(BasisLocalAvatarDriver basisLocalAvatarDriver, Animator animator)
    {
        Animator = animator;
        BasisLocalAvatarDriver = basisLocalAvatarDriver;
        // Initialize the HumanPoseHandler with the animator's avatar and transform
        poseHandler = new HumanPoseHandler(Animator.avatar, Animator.transform);
        // Initialize the HumanPose
        pose = new HumanPose();
        SetMusclesAndRecordPoses();
    }
    public void SetMusclesAndRecordPoses()
    {
        // Get the current human pose
        poseHandler.GetHumanPose(ref pose);
        RecordCurrentPose(ref RestingOnePoseData);
        RecordCurrentPose(ref CurrentOnPoseData);
        // Set all muscle values to -1 and record pose
        for (int Index = 0; Index < pose.muscles.Length; Index++)
        {
            pose.muscles[Index] = -1f;
        }
        poseHandler.SetHumanPose(ref pose);
        RecordCurrentPose(ref negativeOnePoseData);
        // Set all muscle values to 1 and record pose
        for (int Index = 0; Index < pose.muscles.Length; Index++)
        {
            pose.muscles[Index] = 1f;
        }
        poseHandler.SetHumanPose(ref pose);
        RecordCurrentPose(ref positiveOnePoseData);
    }
    private void RecordCurrentPose(ref PoseData poseData)
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

    private Pose[] RecordFingerPoses(Transform proximal, Transform intermediate, Transform distal, bool hasProximal, bool hasIntermediate, bool hasDistal)
    {
        Pose[] fingerPoses = new Pose[3];
        fingerPoses[0] = ConvertToPose(proximal, hasProximal);
        fingerPoses[1] = ConvertToPose(intermediate, hasIntermediate);
        fingerPoses[2] = ConvertToPose(distal, hasDistal);
        return fingerPoses;
    }
    public void LateUpdate()
    {
        UpdateAllFingers(BasisLocalAvatarDriver.References, RestingOnePoseData, positiveOnePoseData,negativeOnePoseData,ref CurrentOnPoseData);
    }
    public float LeftIndexPercentage;
    public float LeftMiddlePercentage;
    public float LeftRingPercentage;
    public float LeftLittlePercentage;

    public float RightThumbPercentage;
    public float RightIndexPercentage;
    public float RightMiddlePercentage;
    public float RightRingPercentage;
    public float RightLittlePercentage;
    private void UpdateAllFingers(Basis.Scripts.Common.BasisTransformMapping Map, PoseData Rest, PoseData PlusDirection, PoseData NegativeDirection, ref PoseData Current)
    {
        UpdateFingerPoses(Map.LeftThumbProximal, Map.LeftThumbIntermediate, Map.LeftThumbDistal, Rest.LeftThumb, PlusDirection.LeftThumb, NegativeDirection.LeftThumb, ref Current.LeftThumb, Map.HasLeftThumbProximal, Map.HasLeftThumbIntermediate, Map.HasLeftThumbDistal, LeftThumbPercentage);

        UpdateFingerPoses(Map.LeftIndexProximal, Map.LeftIndexIntermediate, Map.LeftIndexDistal, Rest.LeftIndex, PlusDirection.LeftIndex, NegativeDirection.LeftIndex, ref Current.LeftIndex, Map.HasLeftIndexProximal, Map.HasLeftIndexIntermediate, Map.HasLeftIndexDistal, LeftIndexPercentage);

        UpdateFingerPoses(Map.LeftMiddleProximal, Map.LeftMiddleIntermediate, Map.LeftMiddleDistal, Rest.LeftMiddle, PlusDirection.LeftMiddle, NegativeDirection.LeftMiddle, ref Current.LeftMiddle, Map.HasLeftMiddleProximal, Map.HasLeftMiddleIntermediate, Map.HasLeftMiddleDistal, LeftMiddlePercentage);

        UpdateFingerPoses(Map.LeftRingProximal, Map.LeftRingIntermediate, Map.LeftRingDistal, Rest.LeftRing, PlusDirection.LeftRing, NegativeDirection.LeftRing, ref Current.LeftRing, Map.HasLeftRingProximal, Map.HasLeftRingIntermediate, Map.HasLeftRingDistal, LeftRingPercentage);

        UpdateFingerPoses(Map.LeftLittleProximal, Map.LeftLittleIntermediate, Map.LeftLittleDistal, Rest.LeftLittle, PlusDirection.LeftLittle, NegativeDirection.LeftLittle, ref Current.LeftLittle, Map.HasLeftLittleProximal, Map.HasLeftLittleIntermediate, Map.HasLeftLittleDistal, LeftLittlePercentage);

        UpdateFingerPoses(Map.RightThumbProximal, Map.RightThumbIntermediate, Map.RightThumbDistal, Rest.RightThumb, PlusDirection.RightThumb, NegativeDirection.RightThumb, ref Current.RightThumb, Map.HasRightThumbProximal, Map.HasRightThumbIntermediate, Map.HasRightThumbDistal, RightThumbPercentage);

        UpdateFingerPoses(Map.RightIndexProximal, Map.RightIndexIntermediate, Map.RightIndexDistal, Rest.RightIndex, PlusDirection.RightIndex, NegativeDirection.RightIndex, ref Current.RightIndex, Map.HasRightIndexProximal, Map.HasRightIndexIntermediate, Map.HasRightIndexDistal, RightIndexPercentage);

        UpdateFingerPoses(Map.RightMiddleProximal, Map.RightMiddleIntermediate, Map.RightMiddleDistal, Rest.RightMiddle, PlusDirection.RightMiddle, NegativeDirection.RightMiddle, ref Current.RightMiddle, Map.HasRightMiddleProximal, Map.HasRightMiddleIntermediate, Map.HasRightMiddleDistal, RightMiddlePercentage);

        UpdateFingerPoses(Map.RightRingProximal, Map.RightRingIntermediate, Map.RightRingDistal, Rest.RightRing, PlusDirection.RightRing, NegativeDirection.RightRing, ref Current.RightRing, Map.HasRightRingProximal, Map.HasRightRingIntermediate, Map.HasRightRingDistal, RightRingPercentage);

        UpdateFingerPoses(Map.RightLittleProximal, Map.RightLittleIntermediate, Map.RightLittleDistal, Rest.RightLittle, PlusDirection.RightLittle, NegativeDirection.RightLittle, ref Current.RightLittle, Map.HasRightLittleProximal, Map.HasRightLittleIntermediate, Map.HasRightLittleDistal, RightLittlePercentage);
    }

    private void UpdateFingerPoses(Transform proximal, Transform intermediate, Transform distal, Pose[] restingPoses, Pose[] positivePoses, Pose[] negativePoses, ref Pose[] currentPoses, bool hasProximal, bool hasIntermediate, bool hasDistal, float percentage)
    {
        UpdatePose(proximal, ref currentPoses[0], restingPoses[0], positivePoses[0], negativePoses[0], hasProximal, percentage);
        UpdatePose(intermediate, ref currentPoses[1], restingPoses[1], positivePoses[1], negativePoses[1], hasIntermediate, percentage);
        UpdatePose(distal, ref currentPoses[2], restingPoses[2], positivePoses[2], negativePoses[2], hasDistal, percentage);
    }

    private void UpdatePose(Transform trans, ref Pose currentPose, Pose restingPose, Pose positivePose, Pose negativePose, bool hasTransform, float percentage)
    {
        if (hasTransform)
        {
            if (percentage >= 0)
            {
                // Interpolate between resting and positive poses for positive percentage
                currentPose.position = Vector3.Lerp(restingPose.position, positivePose.position, percentage);
                currentPose.rotation = Quaternion.Slerp(restingPose.rotation, positivePose.rotation, percentage);
            }
            else
            {
                // Interpolate between resting and negative poses for negative percentage
                currentPose.position = Vector3.Lerp(restingPose.position, negativePose.position, -percentage);
                currentPose.rotation = Quaternion.Slerp(restingPose.rotation, negativePose.rotation, -percentage);
            }

            // Apply the interpolated pose to the transform
            trans.SetLocalPositionAndRotation(currentPose.position, currentPose.rotation);
        }
    }
    public Pose ConvertToPose(Transform Trans,bool HasTrans)
    {
        Pose pose = new Pose();
        if (HasTrans)
        {
            Trans.GetLocalPositionAndRotation(out pose.position, out pose.rotation);
        }
        return pose;
    }
    [System.Serializable]
    public struct PoseData
    {
        [SerializeField]
        public Pose[] LeftThumb;
        [SerializeField]
        public Pose[] LeftIndex;
        [SerializeField]
        public Pose[] LeftMiddle;
        [SerializeField]
        public Pose[] LeftRing;
        [SerializeField]
        public Pose[] LeftLittle;
        [SerializeField]
        public Pose[] RightThumb;
        [SerializeField]
        public Pose[] RightIndex;
        [SerializeField]
        public Pose[] RightMiddle;
        [SerializeField]
        public Pose[] RightRing;
        [SerializeField]
        public Pose[] RightLittle;
    }
    [System.Serializable]
    public struct Pose
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}