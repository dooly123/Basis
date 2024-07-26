using UnityEngine;

public class BasisInputSkeleton
{

    public BasisBoneControl ThumbProximal;
    public BasisBoneControl ThumbIntermediate;
    public BasisBoneControl ThumbDistal;

    public BasisBoneControl IndexProximal;
    public BasisBoneControl IndexIntermediate;
    public BasisBoneControl IndexDistal;

    public BasisBoneControl MiddleProximal;
    public BasisBoneControl MiddleIntermediate;
    public BasisBoneControl MiddleDistal;

    public BasisBoneControl RingProximal;
    public BasisBoneControl RingIntermediate;
    public BasisBoneControl RingDistal;

    public BasisBoneControl LittleProximal;
    public BasisBoneControl LittleIntermediate;
    public BasisBoneControl LittleDistal;

    public BasisBoneControl ActiveHand;
    public float thumbCurl;
    public float indexCurl;
    public float middleCurl;
    public float ringCurl;
    public float pinkyCurl;
    public void AssignAsLeft()
    {
        InitializeBones(BasisBoneTrackedRole.LeftThumbProximal, out ThumbProximal);
        InitializeBones(BasisBoneTrackedRole.LeftThumbIntermediate, out ThumbIntermediate);
        InitializeBones(BasisBoneTrackedRole.LeftThumbDistal, out ThumbDistal);

        InitializeBones(BasisBoneTrackedRole.LeftIndexProximal, out IndexProximal);
        InitializeBones(BasisBoneTrackedRole.LeftIndexIntermediate, out IndexIntermediate);
        InitializeBones(BasisBoneTrackedRole.LeftIndexDistal, out IndexDistal);

        InitializeBones(BasisBoneTrackedRole.LeftMiddleProximal, out MiddleProximal);
        InitializeBones(BasisBoneTrackedRole.LeftMiddleIntermediate, out MiddleIntermediate);
        InitializeBones(BasisBoneTrackedRole.LeftMiddleDistal, out MiddleDistal);

        InitializeBones(BasisBoneTrackedRole.LeftRingProximal, out RingProximal);
        InitializeBones(BasisBoneTrackedRole.LeftRingIntermediate, out RingIntermediate);
        InitializeBones(BasisBoneTrackedRole.LeftRingDistal, out RingDistal);

        InitializeBones(BasisBoneTrackedRole.LeftLittleProximal, out LittleProximal);
        InitializeBones(BasisBoneTrackedRole.LeftLittleIntermediate, out LittleIntermediate);
        InitializeBones(BasisBoneTrackedRole.LeftLittleDistal, out LittleDistal);

        InitializeBones(BasisBoneTrackedRole.LeftHand, out ActiveHand);
    }
    public void AssignAsRight()
    {
        InitializeBones(BasisBoneTrackedRole.RightThumbProximal, out ThumbProximal);
        InitializeBones(BasisBoneTrackedRole.RightThumbIntermediate, out ThumbIntermediate);
        InitializeBones(BasisBoneTrackedRole.RightThumbDistal, out ThumbDistal);

        InitializeBones(BasisBoneTrackedRole.RightIndexProximal, out IndexProximal);
        InitializeBones(BasisBoneTrackedRole.RightIndexIntermediate, out IndexIntermediate);
        InitializeBones(BasisBoneTrackedRole.RightIndexDistal, out IndexDistal);

        InitializeBones(BasisBoneTrackedRole.RightMiddleProximal, out MiddleProximal);
        InitializeBones(BasisBoneTrackedRole.RightMiddleIntermediate, out MiddleIntermediate);
        InitializeBones(BasisBoneTrackedRole.RightMiddleDistal, out MiddleDistal);

        InitializeBones(BasisBoneTrackedRole.RightRingProximal, out RingProximal);
        InitializeBones(BasisBoneTrackedRole.RightRingIntermediate, out RingIntermediate);
        InitializeBones(BasisBoneTrackedRole.RightRingDistal, out RingDistal);

        InitializeBones(BasisBoneTrackedRole.RightLittleProximal, out LittleProximal);
        InitializeBones(BasisBoneTrackedRole.RightLittleIntermediate, out LittleIntermediate);
        InitializeBones(BasisBoneTrackedRole.RightLittleDistal, out LittleDistal);

        InitializeBones(BasisBoneTrackedRole.RightHand, out ActiveHand);
    }
    private void InitializeBones(BasisBoneTrackedRole boneRole, out BasisBoneControl boneControl)
    {
        BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out boneControl, boneRole);
        //  boneControl.HasRigLayer = BasisHasRigLayer.HasRigLayer;
        //  boneControl.HasTracked = BasisHasTracked.HasTracker;
    }
    public void ApplyMovement()
    {
        SetASTracked(IndexDistal);
        Vector3 LocalStartPosition = IndexDistal.TposeLocal.position;
        Vector3 LocalEndPosition = ActiveHand.TposeLocal.position;
        Vector3 DifferenceBetweenStartAndEnd = LocalStartPosition - LocalEndPosition;
        Vector3 DifferenceRotated = ActiveHand.BoneTransform.rotation * DifferenceBetweenStartAndEnd;
        IndexDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + DifferenceRotated, ActiveHand.CurrentWorldData.position, skeletonAction.indexCurl);
    }
    public void SetASTracked(BasisBoneControl Input)
    {
        if (Input.HasTracked == BasisHasTracked.HasNoTracker)
        {
            Input.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            Input.HasTracked = BasisHasTracked.HasTracker;
        }
    }
}
