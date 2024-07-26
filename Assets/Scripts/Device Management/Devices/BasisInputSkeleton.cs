using UnityEngine;
[System.Serializable]
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

    public Vector3 ThumbPosition;
    public Vector3 indexPosition;
    public Vector3 middlePosition;
    public Vector3 ringPosition;
    public Vector3 pinkyPosition;

    public Vector3 ThumbPositionPoint = new Vector3(-0.04f, -0.07f, -0.04f);
    public Vector3 indexPositionPoint = new Vector3(-0.04f, -0.02f, -0.03f);
    public Vector3 middlePositionPoint = new Vector3(-0.04f, -0.09f, -0f);
    public Vector3 ringPositionPoint = new Vector3(-0.06f, -0.05f, -0.03f);
    public Vector3 pinkyPositionPoint = new Vector3(-0.06f, -0.045f, -0.04f);
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
    public void Simulate()
    {
        ThumbPosition = ApplyMovement(ThumbDistal, ActiveHand, thumbCurl, ThumbPositionPoint);
        indexPosition = ApplyMovement(IndexDistal, ActiveHand, indexCurl, indexPositionPoint);
        middlePosition = ApplyMovement(MiddleDistal, ActiveHand, middleCurl, middlePositionPoint);
        ringPosition = ApplyMovement(RingDistal, ActiveHand, ringCurl, ringPositionPoint);
        pinkyPosition = ApplyMovement(LittleDistal, ActiveHand, pinkyCurl, pinkyPositionPoint);
    }
    private void InitializeBones(BasisBoneTrackedRole boneRole, out BasisBoneControl boneControl)
    {
        BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out boneControl, boneRole);
    }
    public Vector3 ApplyMovement(BasisBoneControl Control, BasisBoneControl Target, float Curl,Vector3 Offset)
    {
        Control.PositionWeight = Curl;
        Control.RotationWeight = 0;
        SetASTracked(Control);
        Control.TrackerData.position = Target.FinalApplied.position + (Target.CurrentWorldData.rotation * Offset);
        Control.TrackerData.rotation = Quaternion.identity;
        Debug.DrawLine(Target.CurrentWorldData.position, Control.CurrentWorldData.position);
        return Control.TrackerData.position;
    }
    public void SetASTracked(BasisBoneControl Input)
    {
        if (Input.HasTracked == BasisHasTracked.HasNoTracker)
        {
            Input.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            Input.HasTracked = BasisHasTracked.HasTracker;
        }
    }
    public void OnDrawGizmos()
    {
       
    }
}