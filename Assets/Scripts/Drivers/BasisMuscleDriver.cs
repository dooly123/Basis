using Basis.Scripts.Drivers;
using System;
using UnityEngine;
[DefaultExecutionOrder(15001)]
public class BasisMuscleDriver : MonoBehaviour
{
    public Animator Animator; // Reference to the Animator component
    public HumanPoseHandler poseHandler;
    public HumanPose pose;
    [SerializeField]
    public SquarePoseData BoxedPoseData;
    [SerializeField]
    public PoseData RestingOnePoseData;
    [SerializeField]
    public PoseData CurrentOnPoseData;
    public BasisLocalAvatarDriver BasisLocalAvatarDriver;
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
    [System.Serializable]
    public struct SquarePoseData
    {
        public PoseData TopLeft;
        public PoseData TopRight;

        public PoseData BottomLeft;
        public PoseData BottomRight;
    }
    public void Initialize(BasisLocalAvatarDriver basisLocalAvatarDriver, Animator animator)
    {

        Animator = animator;
        BasisLocalAvatarDriver = basisLocalAvatarDriver;
        // Initialize the HumanPoseHandler with the animator's avatar and transform
        poseHandler = new HumanPoseHandler(Animator.avatar, Animator.transform);
        // Initialize the HumanPose
        pose = new HumanPose();
        SetMusclesAndRecordPoses();
        MuscleNames = HumanTrait.MuscleName;
    }
    public void SetMusclesAndRecordPoses()
    {
        BoxedPoseData = new SquarePoseData(); 
        // Get the current human pose
        poseHandler.GetHumanPose(ref pose);
        LoadMuscleData();

        RecordCurrentPose(ref RestingOnePoseData);
        RecordCurrentPose(ref CurrentOnPoseData);

        // top left
        SetAndRecordPose(1, ref BoxedPoseData.TopLeft, 1);
        // top right
        SetAndRecordPose(1, ref BoxedPoseData.TopRight, -1);
        //bottom left
        SetAndRecordPose(-1, ref BoxedPoseData.BottomLeft, -1);
        //bottom right
        SetAndRecordPose(-1, ref BoxedPoseData.BottomRight, 1);
    }
    private void SetAndRecordPose(int fillValue, ref PoseData poseData, int Splane)
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
    private void SetMuscleData(float[] muscleArray, int fillValue, int specificValue)
    {
        Array.Fill(muscleArray, fillValue);
        muscleArray[1] = specificValue;
    }
    public void LoadMuscleData()
    {
        // Assign muscle indices to each finger array using Array.Copy
        LeftThumb = new float[4];
        System.Array.Copy(pose.muscles, 55, LeftThumb, 0, 4);
        LeftIndex = new float[4];
        System.Array.Copy(pose.muscles, 59, LeftIndex, 0, 4);
        LeftMiddle = new float[4];
        System.Array.Copy(pose.muscles, 63, LeftMiddle, 0, 4);
        LeftRing = new float[4];
        System.Array.Copy(pose.muscles, 67, LeftRing, 0, 4);
        LeftLittle = new float[4];
        System.Array.Copy(pose.muscles, 71, LeftLittle, 0, 4);

        RightThumb = new float[4];
        System.Array.Copy(pose.muscles, 75, RightThumb, 0, 4);
        RightIndex = new float[4];
        System.Array.Copy(pose.muscles, 79, RightIndex, 0, 4);
        RightMiddle = new float[4];
        System.Array.Copy(pose.muscles, 83, RightMiddle, 0, 4);
        RightRing = new float[4];
        System.Array.Copy(pose.muscles, 87, RightRing, 0, 4);
        RightLittle = new float[4];
        System.Array.Copy(pose.muscles, 91, RightLittle, 0, 4);
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
        UpdateAllFingers(BasisLocalAvatarDriver.References, RestingOnePoseData, ref CurrentOnPoseData);
    }
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
    public struct PoseAsSquare
    {
        public Pose[] TopLeft;
        public Pose[] TopRight;

        public Pose[] BottomLeft;
        public Pose[] BottomRight;
    }
    private void UpdateAllFingers(Basis.Scripts.Common.BasisTransformMapping Map, PoseData Rest, ref PoseData Current)
    {
        PoseAsSquare PoseAsSquare = new PoseAsSquare();

        PoseAsSquare.TopLeft = BoxedPoseData.TopLeft.LeftThumb;
        PoseAsSquare.TopRight = BoxedPoseData.TopRight.LeftThumb;
        PoseAsSquare.BottomLeft = BoxedPoseData.BottomLeft.LeftThumb;
        PoseAsSquare.BottomRight = BoxedPoseData.BottomRight.LeftThumb;

        UpdateFingerPoses(Map.LeftThumbProximal, Map.LeftThumbIntermediate, Map.LeftThumbDistal, Rest.LeftThumb, PoseAsSquare, ref Current.LeftThumb, Map.HasLeftThumbProximal, Map.HasLeftThumbIntermediate, Map.HasLeftThumbDistal, LeftThumbPercentage);

        PoseAsSquare.TopLeft = BoxedPoseData.TopLeft.LeftIndex;
        PoseAsSquare.TopRight = BoxedPoseData.TopRight.LeftIndex;
        PoseAsSquare.BottomLeft = BoxedPoseData.BottomLeft.LeftIndex;
        PoseAsSquare.BottomRight = BoxedPoseData.BottomRight.LeftIndex;

        UpdateFingerPoses(Map.LeftIndexProximal, Map.LeftIndexIntermediate, Map.LeftIndexDistal, Rest.LeftIndex, PoseAsSquare, ref Current.LeftIndex, Map.HasLeftIndexProximal, Map.HasLeftIndexIntermediate, Map.HasLeftIndexDistal, LeftIndexPercentage);

        PoseAsSquare.TopLeft = BoxedPoseData.TopLeft.LeftMiddle;
        PoseAsSquare.TopRight = BoxedPoseData.TopRight.LeftMiddle;
        PoseAsSquare.BottomLeft = BoxedPoseData.BottomLeft.LeftMiddle;
        PoseAsSquare.BottomRight = BoxedPoseData.BottomRight.LeftMiddle;

        UpdateFingerPoses(Map.LeftMiddleProximal, Map.LeftMiddleIntermediate, Map.LeftMiddleDistal, Rest.LeftMiddle, PoseAsSquare, ref Current.LeftMiddle, Map.HasLeftMiddleProximal, Map.HasLeftMiddleIntermediate, Map.HasLeftMiddleDistal, LeftMiddlePercentage);

        PoseAsSquare.TopLeft = BoxedPoseData.TopLeft.LeftRing;
        PoseAsSquare.TopRight = BoxedPoseData.TopRight.LeftRing;
        PoseAsSquare.BottomLeft = BoxedPoseData.BottomLeft.LeftRing;
        PoseAsSquare.BottomRight = BoxedPoseData.BottomRight.LeftRing;

        UpdateFingerPoses(Map.LeftRingProximal, Map.LeftRingIntermediate, Map.LeftRingDistal, Rest.LeftRing, PoseAsSquare, ref Current.LeftRing, Map.HasLeftRingProximal, Map.HasLeftRingIntermediate, Map.HasLeftRingDistal, LeftRingPercentage);

        PoseAsSquare.TopLeft = BoxedPoseData.TopLeft.LeftLittle;
        PoseAsSquare.TopRight = BoxedPoseData.TopRight.LeftLittle;
        PoseAsSquare.BottomLeft = BoxedPoseData.BottomLeft.LeftLittle;
        PoseAsSquare.BottomRight = BoxedPoseData.BottomRight.LeftLittle;

        UpdateFingerPoses(Map.LeftLittleProximal, Map.LeftLittleIntermediate, Map.LeftLittleDistal, Rest.LeftLittle, PoseAsSquare, ref Current.LeftLittle, Map.HasLeftLittleProximal, Map.HasLeftLittleIntermediate, Map.HasLeftLittleDistal, LeftLittlePercentage);

        PoseAsSquare.TopLeft = BoxedPoseData.TopLeft.RightThumb;
        PoseAsSquare.TopRight = BoxedPoseData.TopRight.RightThumb;
        PoseAsSquare.BottomLeft = BoxedPoseData.BottomLeft.RightThumb;
        PoseAsSquare.BottomRight = BoxedPoseData.BottomRight.RightThumb;

        UpdateFingerPoses(Map.RightThumbProximal, Map.RightThumbIntermediate, Map.RightThumbDistal, Rest.RightThumb, PoseAsSquare, ref Current.RightThumb, Map.HasRightThumbProximal, Map.HasRightThumbIntermediate, Map.HasRightThumbDistal, RightThumbPercentage);

        PoseAsSquare.TopLeft = BoxedPoseData.TopLeft.RightIndex;
        PoseAsSquare.TopRight = BoxedPoseData.TopRight.RightIndex;
        PoseAsSquare.BottomLeft = BoxedPoseData.BottomLeft.RightIndex;
        PoseAsSquare.BottomRight = BoxedPoseData.BottomRight.RightIndex;

        UpdateFingerPoses(Map.RightIndexProximal, Map.RightIndexIntermediate, Map.RightIndexDistal, Rest.RightIndex, PoseAsSquare, ref Current.RightIndex, Map.HasRightIndexProximal, Map.HasRightIndexIntermediate, Map.HasRightIndexDistal, RightIndexPercentage);

        PoseAsSquare.TopLeft = BoxedPoseData.TopLeft.RightMiddle;
        PoseAsSquare.TopRight = BoxedPoseData.TopRight.RightMiddle;
        PoseAsSquare.BottomLeft = BoxedPoseData.BottomLeft.RightMiddle;
        PoseAsSquare.BottomRight = BoxedPoseData.BottomRight.RightMiddle;

        UpdateFingerPoses(Map.RightMiddleProximal, Map.RightMiddleIntermediate, Map.RightMiddleDistal, Rest.RightMiddle, PoseAsSquare, ref Current.RightMiddle, Map.HasRightMiddleProximal, Map.HasRightMiddleIntermediate, Map.HasRightMiddleDistal, RightMiddlePercentage);

        PoseAsSquare.TopLeft = BoxedPoseData.TopLeft.RightRing;
        PoseAsSquare.TopRight = BoxedPoseData.TopRight.RightRing;
        PoseAsSquare.BottomLeft = BoxedPoseData.BottomLeft.RightRing;
        PoseAsSquare.BottomRight = BoxedPoseData.BottomRight.RightRing;

        UpdateFingerPoses(Map.RightRingProximal, Map.RightRingIntermediate, Map.RightRingDistal, Rest.RightRing, PoseAsSquare, ref Current.RightRing, Map.HasRightRingProximal, Map.HasRightRingIntermediate, Map.HasRightRingDistal, RightRingPercentage);

        PoseAsSquare.TopLeft = BoxedPoseData.TopLeft.RightLittle;
        PoseAsSquare.TopRight = BoxedPoseData.TopRight.RightLittle;
        PoseAsSquare.BottomLeft = BoxedPoseData.BottomLeft.RightLittle;
        PoseAsSquare.BottomRight = BoxedPoseData.BottomRight.RightLittle;
        UpdateFingerPoses(Map.RightLittleProximal, Map.RightLittleIntermediate, Map.RightLittleDistal, Rest.RightLittle, PoseAsSquare, ref Current.RightLittle, Map.HasRightLittleProximal, Map.HasRightLittleIntermediate, Map.HasRightLittleDistal, RightLittlePercentage);
    }

    private void UpdateFingerPoses(Transform proximal, Transform intermediate, Transform distal, Pose[] restingPoses, PoseAsSquare BoxedPoseData, ref Pose[] currentPoses, bool hasProximal, bool hasIntermediate, bool hasDistal, Vector2 VerticalAndHorizontal)
    {
        UpdatePose(proximal, ref currentPoses[0], restingPoses[0], BoxedPoseData, hasProximal, VerticalAndHorizontal,0);
        UpdatePose(intermediate, ref currentPoses[1], restingPoses[1], BoxedPoseData, hasIntermediate, VerticalAndHorizontal,1);
        UpdatePose(distal, ref currentPoses[2], restingPoses[2], BoxedPoseData, hasDistal, VerticalAndHorizontal,2);
    }

    private void UpdatePose(Transform trans, ref Pose currentPose, Pose restingPose, PoseAsSquare BoxedPoseData, bool hasTransform, Vector2 VerticalAndHorizontal,int index)
    {
        if (hasTransform)
        {
            currentPose.position = InterpolatePosition(VerticalAndHorizontal, restingPose, BoxedPoseData.BottomLeft[index], BoxedPoseData.BottomRight[index], BoxedPoseData.TopLeft[index], BoxedPoseData.TopRight[index]);
            currentPose.rotation = InterpolateRotation(VerticalAndHorizontal, restingPose, BoxedPoseData.BottomLeft[index], BoxedPoseData.BottomRight[index], BoxedPoseData.TopLeft[index], BoxedPoseData.TopRight[index]);
            trans.SetLocalPositionAndRotation(currentPose.position, currentPose.rotation);
        }
    }

    Vector3 InterpolatePosition(Vector2 VerticalAndHorizontal, Pose CenterPose, Pose bottomLeft, Pose bottomRight, Pose topLeft, Pose topRight)
    {
       return ConvertToPointInSpace(VerticalAndHorizontal, bottomLeft.position,bottomRight.position,topLeft.position,topRight.position);
    }

    Quaternion InterpolateRotation(Vector2 VerticalAndHorizontal, Pose CenterPose, Pose bottomLeft, Pose bottomRight, Pose topLeft, Pose topRight)
    {
       return ConvertToPointInSpace(VerticalAndHorizontal,bottomLeft.rotation, bottomRight.rotation, topLeft.rotation, topRight.rotation);
    }
    /// <summary>
    /// Converts a 2D percentage into a 3D point within a defined quadrilateral in space.
    /// </summary>
    /// <param name="VerticalAndHorizontalPercentage">A 2D vector with values ranging from -1,-1 to 1,1.</param>
    /// <param name="bottomLeft">The bottom-left corner of the quadrilateral when VerticalAndHorizontalPercentage is -1,-1.</param>
    /// <param name="bottomRight">The bottom-right corner of the quadrilateral when VerticalAndHorizontalPercentage is -1,1.</param>
    /// <param name="topLeft">The top-left corner of the quadrilateral when VerticalAndHorizontalPercentage is 1,-1.</param>
    /// <param name="topRight">The top-right corner of the quadrilateral when VerticalAndHorizontalPercentage is 1,1.</param>
    /// <returns>The 3D point corresponding to the specified percentage within the quadrilateral.</returns>
    Vector3 ConvertToPointInSpace(Vector2 VerticalAndHorizontalPercentage,Vector3 RestingPosition, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight)
    {
        //RestingPosition
        //the resting position is the starting position (VerticalAndHorizontalPercentage == 0,0
        //it needs to be made so the lerp is going between resting point



        // Interpolating along the horizontal direction between the left and right edges
        Vector3 bottomInterpolated = Vector3.Lerp(bottomLeft, bottomRight, (VerticalAndHorizontalPercentage.x + 1) / 2f);
        Vector3 topInterpolated = Vector3.Lerp(topLeft, topRight, (VerticalAndHorizontalPercentage.x + 1) / 2f);

        // Interpolating along the vertical direction between the bottom and top interpolations
        Vector3 finalPosition = Vector3.Lerp(bottomInterpolated, topInterpolated, (VerticalAndHorizontalPercentage.y + 1) / 2f);

        return finalPosition;
    }
    /// <summary>
    /// Converts a 2D percentage into a quaternion representing a rotation within a defined quadrilateral in space.
    /// </summary>
    /// <param name="VerticalAndHorizontalPercentage">A 2D vector with values ranging from -1,-1 to 1,1.</param>
    /// <param name="bottomLeft">The bottom-left corner rotation when VerticalAndHorizontalPercentage is -1,-1.</param>
    /// <param name="bottomRight">The bottom-right corner rotation when VerticalAndHorizontalPercentage is -1,1.</param>
    /// <param name="topLeft">The top-left corner rotation when VerticalAndHorizontalPercentage is 1,-1.</param>
    /// <param name="topRight">The top-right corner rotation when VerticalAndHorizontalPercentage is 1,1.</param>
    /// <returns>The quaternion corresponding to the specified percentage within the quadrilateral.</returns>
    Quaternion ConvertToPointInSpace(Vector2 VerticalAndHorizontalPercentage, Quaternion bottomLeft, Quaternion bottomRight, Quaternion topLeft, Quaternion topRight)
    {
        // Interpolating horizontally between the left and right edges at the bottom and top
        Quaternion bottomInterpolated = Quaternion.Slerp(bottomLeft, bottomRight, (VerticalAndHorizontalPercentage.x + 1) / 2f);
        Quaternion topInterpolated = Quaternion.Slerp(topLeft, topRight, (VerticalAndHorizontalPercentage.x + 1) / 2f);

        // Interpolating vertically between the interpolated bottom and top
        Quaternion finalRotation = Quaternion.Slerp(bottomInterpolated, topInterpolated, (VerticalAndHorizontalPercentage.y + 1) / 2f);

        return finalRotation;
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