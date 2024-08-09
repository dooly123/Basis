using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;

namespace Basis.Scripts.Device_Management.Devices
{
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
    public void Simulate()
    {
    }
    /*
    public void ApplyMovement(BasisBoneControl Control, int Index)
    {
        // Get local positions
        Vector3 AHandposeLocal = ActiveHand.TposeLocal.position;
        Vector3 CTposeLocal = Control.TposeLocal.position;
        // Calculate the position offset
        Vector3 Pos = AHandposeLocal - CTposeLocal;
        // Apply rotation to position offset
        Vector3 rotatedPos = ActiveHand.IncomingData.rotation * Quaternion.Euler(RotationOffset) * (Pos + Positions[Index]);
        // Calculate the new position
        Vector3 newPos = ActiveHand.IncomingData.position - rotatedPos;
        // Apply the new position
        Control.IncomingData.position = newPos;
        // Calculate the new rotation
        Quaternion newRot = ActiveHand.IncomingData.rotation * Rotations[Index];
        // Apply the new rotation
        Control.IncomingData.rotation = newRot;
    }
    */
    public void SetASTracked(BasisBoneControl Input)
    {
        /*
        if (Input.HasTracked == BasisHasTracked.HasNoTracker)
        {
            Input.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            Input.HasTracked = BasisHasTracked.HasTracker;
        }
        */
    }
    public void OnDrawGizmos()
    {

    }
    private void InitializeBones(BasisBoneTrackedRole boneRole, out BasisBoneControl boneControl)
    {
        BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out boneControl, boneRole);
        SetASTracked(boneControl);
    }
    protected static readonly Quaternion rightFlipAngle = Quaternion.AngleAxis(180, Vector3.right);
    public static class Indexes
    {
        public const int root = 0;
        public const int wrist = 1;
        public const int thumbMetacarpal = 2;
        public const int thumbProximal = 2;
        public const int thumbMiddle = 3;
        public const int thumbDistal = 4;
        public const int thumbTip = 5;
        public const int indexMetacarpal = 6;
        public const int indexProximal = 7;
        public const int indexMiddle = 8;
        public const int indexDistal = 9;
        public const int indexTip = 10;
        public const int middleMetacarpal = 11;
        public const int middleProximal = 12;
        public const int middleMiddle = 13;
        public const int middleDistal = 14;
        public const int middleTip = 15;
        public const int ringMetacarpal = 16;
        public const int ringProximal = 17;
        public const int ringMiddle = 18;
        public const int ringDistal = 19;
        public const int ringTip = 20;
        public const int pinkyMetacarpal = 21;
        public const int pinkyProximal = 22;
        public const int pinkyMiddle = 23;
        public const int pinkyDistal = 24;
        public const int pinkyTip = 25;
        public const int thumbAux = 26;
        public const int indexAux = 27;
        public const int middleAux = 28;
        public const int ringAux = 29;
        public const int pinkyAux = 30;
    }
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
}
}