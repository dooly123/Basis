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

    public Vector3[] Positions;
    public Quaternion[] Rotations;
    public Vector3 RotationOffset;
    public bool MIrrored = false;
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
    public void Simulate()
    {
        if (MIrrored)
        {
            for (int boneIndex = 0; boneIndex < Rotations.Length; boneIndex++)
            {
                Rotations[boneIndex] = MirrorRotation(boneIndex, Rotations[boneIndex]);
            }
            for (int Index = 0; Index < Positions.Length; Index++)
            {
                Positions[Index] = MirrorPosition(Index, Positions[Index]);
            }
        }
        // Left Hand
        ApplyMovement(ref LeftThumbDistalP, ref LeftThumbDistalQ, Indexes.thumbDistal);
        ApplyMovement(ref LeftIndexDistalP, ref LeftIndexDistalQ, Indexes.indexDistal);
        ApplyMovement(ref LeftMiddleDistalP, ref LeftMiddleDistalQ, Indexes.middleDistal);
        ApplyMovement(ref LeftRingDistalP, ref LeftRingDistalQ, Indexes.ringDistal);
        ApplyMovement(ref LeftLittleDistalP, ref LeftLittleDistalQ, Indexes.pinkyDistal);

        ApplyMovement(ref LeftThumbIntermediateP, ref LeftThumbIntermediateQ, Indexes.thumbMiddle);
        ApplyMovement(ref LeftIndexIntermediateP, ref LeftIndexIntermediateQ, Indexes.indexMiddle);
        ApplyMovement(ref LeftMiddleIntermediateP, ref LeftMiddleIntermediateQ, Indexes.middleMiddle);
        ApplyMovement(ref LeftRingIntermediateP, ref LeftRingIntermediateQ, Indexes.ringMiddle);
        ApplyMovement(ref LeftLittleIntermediateP, ref LeftLittleIntermediateQ, Indexes.pinkyMiddle);

        ApplyMovement(ref LeftThumbProximalP, ref LeftThumbProximalQ, Indexes.thumbProximal);
        ApplyMovement(ref LeftIndexProximalP, ref LeftIndexProximalQ, Indexes.indexProximal);
        ApplyMovement(ref LeftMiddleProximalP, ref LeftMiddleProximalQ, Indexes.middleProximal);
        ApplyMovement(ref LeftRingProximalP, ref LeftRingProximalQ, Indexes.ringProximal);
        ApplyMovement(ref LeftLittleProximalP, ref LeftLittleProximalQ, Indexes.pinkyProximal);

        // Right Hand
        ApplyMovement(ref RightThumbDistalP, ref RightThumbDistalQ, Indexes.thumbDistal);
        ApplyMovement(ref RightIndexDistalP, ref RightIndexDistalQ, Indexes.indexDistal);
        ApplyMovement(ref RightMiddleDistalP, ref RightMiddleDistalQ, Indexes.middleDistal);
        ApplyMovement(ref RightRingDistalP, ref RightRingDistalQ, Indexes.ringDistal);
        ApplyMovement(ref RightLittleDistalP, ref RightLittleDistalQ, Indexes.pinkyDistal);

        ApplyMovement(ref RightThumbIntermediateP, ref RightThumbIntermediateQ, Indexes.thumbMiddle);
        ApplyMovement(ref RightIndexIntermediateP, ref RightIndexIntermediateQ, Indexes.indexMiddle);
        ApplyMovement(ref RightMiddleIntermediateP, ref RightMiddleIntermediateQ, Indexes.middleMiddle);
        ApplyMovement(ref RightRingIntermediateP, ref RightRingIntermediateQ, Indexes.ringMiddle);
        ApplyMovement(ref RightLittleIntermediateP, ref RightLittleIntermediateQ, Indexes.pinkyMiddle);

        ApplyMovement(ref RightThumbProximalP, ref RightThumbProximalQ, Indexes.thumbProximal);
        ApplyMovement(ref RightIndexProximalP, ref RightIndexProximalQ, Indexes.indexProximal);
        ApplyMovement(ref RightMiddleProximalP, ref RightMiddleProximalQ, Indexes.middleProximal);
        ApplyMovement(ref RightRingProximalP, ref RightRingProximalQ, Indexes.ringProximal);
        ApplyMovement(ref RightLittleProximalP, ref RightLittleProximalQ, Indexes.pinkyProximal);
    }
    private void ApplyMovement(ref Vector3 position, ref Quaternion rotation, int index)
    {
        position = Positions[index];
        rotation = Rotations[index];
    }
    public Vector3 LeftThumbDistalP;
    public Quaternion LeftThumbDistalQ;

    public Vector3 LeftThumbProximalP;
    public Quaternion LeftThumbProximalQ;

    public Vector3 LeftThumbIntermediateP;
    public Quaternion LeftThumbIntermediateQ;

    public Vector3 LeftIndexDistalP;
    public Quaternion LeftIndexDistalQ;

    public Vector3 LeftIndexProximalP;
    public Quaternion LeftIndexProximalQ;

    public Vector3 LeftIndexIntermediateP;
    public Quaternion LeftIndexIntermediateQ;

    public Vector3 LeftMiddleDistalP;
    public Quaternion LeftMiddleDistalQ;

    public Vector3 LeftMiddleProximalP;
    public Quaternion LeftMiddleProximalQ;

    public Vector3 LeftMiddleIntermediateP;
    public Quaternion LeftMiddleIntermediateQ;

    public Vector3 LeftRingDistalP;
    public Quaternion LeftRingDistalQ;

    public Vector3 LeftRingProximalP;
    public Quaternion LeftRingProximalQ;

    public Vector3 LeftRingIntermediateP;
    public Quaternion LeftRingIntermediateQ;

    public Vector3 LeftLittleDistalP;
    public Quaternion LeftLittleDistalQ;

    public Vector3 LeftLittleProximalP;
    public Quaternion LeftLittleProximalQ;

    public Vector3 LeftLittleIntermediateP;
    public Quaternion LeftLittleIntermediateQ;

    // Right Hand
    public Vector3 RightThumbDistalP;
    public Quaternion RightThumbDistalQ;

    public Vector3 RightThumbProximalP;
    public Quaternion RightThumbProximalQ;

    public Vector3 RightThumbIntermediateP;
    public Quaternion RightThumbIntermediateQ;

    public Vector3 RightIndexDistalP;
    public Quaternion RightIndexDistalQ;

    public Vector3 RightIndexProximalP;
    public Quaternion RightIndexProximalQ;

    public Vector3 RightIndexIntermediateP;
    public Quaternion RightIndexIntermediateQ;

    public Vector3 RightMiddleDistalP;
    public Quaternion RightMiddleDistalQ;

    public Vector3 RightMiddleProximalP;
    public Quaternion RightMiddleProximalQ;

    public Vector3 RightMiddleIntermediateP;
    public Quaternion RightMiddleIntermediateQ;

    public Vector3 RightRingDistalP;
    public Quaternion RightRingDistalQ;

    public Vector3 RightRingProximalP;
    public Quaternion RightRingProximalQ;

    public Vector3 RightRingIntermediateP;
    public Quaternion RightRingIntermediateQ;

    public Vector3 RightLittleDistalP;
    public Quaternion RightLittleDistalQ;

    public Vector3 RightLittleProximalP;
    public Quaternion RightLittleProximalQ;

    public Vector3 RightLittleIntermediateP;
    public Quaternion RightLittleIntermediateQ;
    public void SimulateLateUpdate()
    {
        BasisLocalPlayer.Instance.AvatarDriver.References.LeftThumbProximal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation *  LeftThumbProximalP, ActiveHand.IncomingData.rotation * LeftThumbProximalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.LeftThumbIntermediate.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftThumbIntermediateP, ActiveHand.IncomingData.rotation * LeftThumbIntermediateQ);

        BasisLocalPlayer.Instance.AvatarDriver.References.LeftIndexDistal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftIndexDistalP, ActiveHand.IncomingData.rotation * LeftIndexDistalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.LeftIndexProximal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftIndexProximalP, ActiveHand.IncomingData.rotation * LeftIndexProximalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.LeftIndexIntermediate.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftIndexIntermediateP, ActiveHand.IncomingData.rotation * LeftIndexIntermediateQ);

        BasisLocalPlayer.Instance.AvatarDriver.References.LeftMiddleDistal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftMiddleDistalP, ActiveHand.IncomingData.rotation * LeftMiddleDistalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.LeftMiddleProximal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftMiddleProximalP, ActiveHand.IncomingData.rotation * LeftMiddleProximalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.LeftMiddleIntermediate.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftMiddleIntermediateP, ActiveHand.IncomingData.rotation * LeftMiddleIntermediateQ);

        BasisLocalPlayer.Instance.AvatarDriver.References.LeftRingDistal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftRingDistalP, ActiveHand.IncomingData.rotation * LeftRingDistalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.LeftRingProximal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftRingProximalP, ActiveHand.IncomingData.rotation * LeftRingProximalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.LeftRingIntermediate.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftRingIntermediateP, ActiveHand.IncomingData.rotation * LeftRingIntermediateQ);

        BasisLocalPlayer.Instance.AvatarDriver.References.LeftLittleDistal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftLittleDistalP, ActiveHand.IncomingData.rotation * LeftLittleDistalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.LeftLittleProximal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftLittleProximalP, ActiveHand.IncomingData.rotation * LeftLittleProximalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.LeftLittleIntermediate.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * LeftLittleIntermediateP, ActiveHand.IncomingData.rotation * LeftLittleIntermediateQ);

        // Right Hand
        BasisLocalPlayer.Instance.AvatarDriver.References.RightThumbDistal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightThumbDistalP, ActiveHand.IncomingData.rotation * RightThumbDistalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.RightThumbProximal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightThumbProximalP, ActiveHand.IncomingData.rotation * RightThumbProximalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.RightThumbIntermediate.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightThumbIntermediateP, ActiveHand.IncomingData.rotation * RightThumbIntermediateQ);

        BasisLocalPlayer.Instance.AvatarDriver.References.RightIndexDistal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightIndexDistalP, ActiveHand.IncomingData.rotation * RightIndexDistalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.RightIndexProximal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightIndexProximalP, ActiveHand.IncomingData.rotation * RightIndexProximalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.RightIndexIntermediate.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightIndexIntermediateP, ActiveHand.IncomingData.rotation * RightIndexIntermediateQ);

        BasisLocalPlayer.Instance.AvatarDriver.References.RightMiddleDistal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightMiddleDistalP, ActiveHand.IncomingData.rotation * RightMiddleDistalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.RightMiddleProximal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightMiddleProximalP, ActiveHand.IncomingData.rotation * RightMiddleProximalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.RightMiddleIntermediate.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightMiddleIntermediateP, ActiveHand.IncomingData.rotation * RightMiddleIntermediateQ);

        BasisLocalPlayer.Instance.AvatarDriver.References.RightRingDistal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightRingDistalP, ActiveHand.IncomingData.rotation * RightRingDistalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.RightRingProximal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightRingProximalP, ActiveHand.IncomingData.rotation * RightRingProximalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.RightRingIntermediate.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightRingIntermediateP, ActiveHand.IncomingData.rotation * RightRingIntermediateQ);

        BasisLocalPlayer.Instance.AvatarDriver.References.RightLittleDistal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightLittleDistalP, ActiveHand.IncomingData.rotation * RightLittleDistalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.RightLittleProximal.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightLittleProximalP, ActiveHand.IncomingData.rotation * RightLittleProximalQ);
        BasisLocalPlayer.Instance.AvatarDriver.References.RightLittleIntermediate.SetLocalPositionAndRotation(ActiveHand.IncomingData.rotation * RightLittleIntermediateP, ActiveHand.IncomingData.rotation * RightLittleIntermediateQ);
    }
    public static Vector3 MirrorPosition(int boneIndex, Vector3 rawPosition)
    {
        if (boneIndex == Indexes.wrist || IsMetacarpal(boneIndex))
        {
            rawPosition.Scale(new Vector3(-1, 1, 1));
        }
        else if (boneIndex != Indexes.root)
        {
            rawPosition *= -1;
        }

        return rawPosition;
    }
    public static Quaternion MirrorRotation(int boneIndex, Quaternion rawRotation)
    {
        if (boneIndex == Indexes.wrist)
        {
            rawRotation.y = rawRotation.y * -1;
            rawRotation.z = rawRotation.z * -1;
        }

        if (IsMetacarpal(boneIndex))
        {
            rawRotation = rightFlipAngle * rawRotation;
        }

        return rawRotation;
    }
    protected static bool IsMetacarpal(int boneIndex)
    {
        return (boneIndex == Indexes.indexMetacarpal ||
            boneIndex == Indexes.middleMetacarpal ||
            boneIndex == Indexes.ringMetacarpal ||
            boneIndex == Indexes.pinkyMetacarpal ||
            boneIndex == Indexes.thumbMetacarpal);
    }
    private void InitializeBones(BasisBoneTrackedRole boneRole, out BasisBoneControl boneControl)
    {
        BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out boneControl, boneRole);
    }
    public void ApplyMovement(BasisBoneControl Control, int Index)
    {
        SetASTracked(Control);
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