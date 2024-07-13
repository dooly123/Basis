using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class BasisLocalAvatarDriver : BasisAvatarDriver
{
    public Vector3 HeadScale;
    public Vector3 HeadScaledDown;
    public BasisLocalBoneDriver LocalDriver;
    public BasisLocalAnimatorDriver AnimatorDriver;
    public BasisLocalPlayer LocalPlayer;
    public TwoBoneIKConstraint HeadTwoBoneIK;
    public TwoBoneIKConstraint LeftFootTwoBoneIK;
    public TwoBoneIKConstraint RightFootTwoBoneIK;
    public TwoBoneIKConstraint LeftHandTwoBoneIK;
    public TwoBoneIKConstraint RightHandTwoBoneIK;
    public TwoBoneIKConstraint UpperChestTwoBoneIK;
    public TwoBoneIKConstraint LeftShoulderTwoBoneIK;
    public TwoBoneIKConstraint RightShoulderTwoBoneIK;
    [SerializeField]
    public List<TwoBoneIKConstraint> LeftFingers = new List<TwoBoneIKConstraint>();
    [SerializeField]
    public List<TwoBoneIKConstraint> RightFingers = new List<TwoBoneIKConstraint>();
    public Rig LeftToeRig;
    public Rig RightToeRig;

    public Rig RigHeadRig;
    public Rig LeftHandRig;
    public Rig RightHandRig;
    public Rig LeftFootRig;
    public Rig RightFootRig;
    public Rig ChestSpineRig;
    public Rig LeftShoulderRig;
    public Rig RightShoulderRig;

    public Rig LeftFingersRig;
    public Rig RightFingersRig;

    public RigLayer LeftFingersLayer;
    public RigLayer RightFingersLayer;

    public RigLayer LeftHandLayer;
    public RigLayer RightHandLayer;
    public RigLayer LeftFootLayer;
    public RigLayer RightFootLayer;
    public RigLayer LeftToeLayer;
    public RigLayer RightToeLayer;

    public RigLayer HeadLayer;
    public RigLayer UpperChestLayer;

    public RigLayer LeftShoulderLayer;
    public RigLayer RightShoulderLayer;
    public List<Rig> Rigs = new List<Rig>();
    public RigBuilder Builder;
    public List<RigTransform> AdditionalTransforms = new List<RigTransform>();
    public MicrophoneRecorder MicrophoneRecorder;
    public void LocalCalibration()
    {
        InitialLocalCalibration(BasisLocalPlayer.Instance);
    }
    public void InitialLocalCalibration(BasisLocalPlayer Player)
    {
        Debug.Log("InitialLocalCalibration");
        LocalPlayer = Player;

        this.LocalDriver = LocalPlayer.LocalBoneDriver;
        if (IsAble())
        {
            // Debug.Log("LocalCalibration Underway");
        }
        else
        {
            return;
        }
        CleanupBeforeContinue();
        AdditionalTransforms.Clear();
        Rigs.Clear();
        PutAvatarIntoTPose();
        if (Builder == null)
        {
            Builder = BasisHelpers.GetOrAddComponent<RigBuilder>(Player.Avatar.Animator.gameObject);
        }
        else
        {
            Builder.Clear();
        }
        Calibration(Player.Avatar);
        BasisLocalEyeFollowDriver EyeFollowBase = BasisHelpers.GetOrAddComponent<BasisLocalEyeFollowDriver>(Player.Avatar.gameObject);
        EyeFollowBase.CreateEyeLook(this);
        HeadScaledDown = Vector3.zero;
        SetMatrixRecalculation(true);
        updateWhenOffscreen(true);
        if (References.Hashead)
        {
            HeadScale = References.head.localScale;
        }
        else
        {
            HeadScale = Vector3.one;
        }
        SetBodySettings(LocalDriver);
        CalculateTransformPositions(Player.Avatar.Animator, LocalDriver);
        ComputeOffsets(LocalDriver);
        Builder.Build();
        CalibrationComplete?.Invoke();
        AnimatorDriver = BasisHelpers.GetOrAddComponent<BasisLocalAnimatorDriver>(Player.Avatar.Animator.gameObject);
        AnimatorDriver.Initialize(Player.Avatar.Animator);
        if (MicrophoneRecorder == null)
        {
            MicrophoneRecorder = BasisHelpers.GetOrAddComponent<MicrophoneRecorder>(this.gameObject);
        }
        MicrophoneRecorder.TryInitialize();
        ResetAvatarAnimator();
    }
    public void CleanupBeforeContinue()
    {
        if (Builder != null)
        {
            Destroy(Builder);
        }
        Builder = null;
        if (RigHeadRig != null)
        {
            Destroy(RigHeadRig.gameObject);
        }
        if (LeftHandRig != null)
        {
            Destroy(LeftHandRig.gameObject);
        }
        if (RightHandRig != null)
        {
            Destroy(RightHandRig.gameObject);
        }
        if (LeftFootRig != null)
        {
            Destroy(LeftFootRig.gameObject);
        }
        if (RightFootRig != null)
        {
            Destroy(RightFootRig.gameObject);
        }
        if (ChestSpineRig != null)
        {
            Destroy(ChestSpineRig.gameObject);
        }
        if (LeftShoulderRig != null)
        {
            Destroy(LeftShoulderRig.gameObject);
        }
        if (RightShoulderRig != null)
        {
            Destroy(RightShoulderRig.gameObject);
        }

        if (LeftToeRig != null)
        {
            Destroy(LeftToeRig.gameObject);
        }
        if (RightToeRig != null)
        {
            Destroy(RightToeRig.gameObject);
        }
    }
    public void ComputeOffsets(BaseBoneDriver BaseBoneDriver)
    {
        //head
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.CenterEye, BasisBoneTrackedRole.Head, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 5, 12, true, 5f);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Neck, BasisTargetController.TargetDirectional, 40, BasisClampData.Clamp, 5, 12, true, 4, BasisTargetController.Target, BasisClampAxis.xz);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Mouth, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);


        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Neck, BasisBoneTrackedRole.UpperChest, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.Chest, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.Spine, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Hips, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, true, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.LeftShoulder, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.RightShoulder, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftShoulder, BasisBoneTrackedRole.LeftUpperArm, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightShoulder, BasisBoneTrackedRole.RightUpperArm, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperArm, BasisBoneTrackedRole.LeftLowerArm, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperArm, BasisBoneTrackedRole.RightLowerArm, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerArm, BasisBoneTrackedRole.LeftHand, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerArm, BasisBoneTrackedRole.RightHand, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        //legs
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.LeftUpperLeg, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.RightUpperLeg, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperLeg, BasisBoneTrackedRole.LeftLowerLeg, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperLeg, BasisBoneTrackedRole.RightLowerLeg, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerLeg, BasisBoneTrackedRole.LeftFoot, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerLeg, BasisBoneTrackedRole.RightFoot, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftToes, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightToes, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);



        // Setting up locks for Left Hand
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftThumbProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftThumbProximal, BasisBoneTrackedRole.LeftThumbIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftThumbIntermediate, BasisBoneTrackedRole.LeftThumbDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftIndexProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftIndexProximal, BasisBoneTrackedRole.LeftIndexIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftIndexIntermediate, BasisBoneTrackedRole.LeftIndexDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftMiddleProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftMiddleProximal, BasisBoneTrackedRole.LeftMiddleIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftMiddleIntermediate, BasisBoneTrackedRole.LeftMiddleDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftRingProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftRingProximal, BasisBoneTrackedRole.LeftRingIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftRingIntermediate, BasisBoneTrackedRole.LeftRingDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftLittleProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLittleProximal, BasisBoneTrackedRole.LeftLittleIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLittleIntermediate, BasisBoneTrackedRole.LeftLittleDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        // Setting up locks for Right Hand
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightThumbProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightThumbProximal, BasisBoneTrackedRole.RightThumbIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightThumbIntermediate, BasisBoneTrackedRole.RightThumbDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightIndexProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightIndexProximal, BasisBoneTrackedRole.RightIndexIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightIndexIntermediate, BasisBoneTrackedRole.RightIndexDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightMiddleProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightMiddleProximal, BasisBoneTrackedRole.RightMiddleIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightMiddleIntermediate, BasisBoneTrackedRole.RightMiddleDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightRingProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightRingProximal, BasisBoneTrackedRole.RightRingIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightRingIntermediate, BasisBoneTrackedRole.RightRingDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightLittleProximal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLittleProximal, BasisBoneTrackedRole.RightLittleIntermediate, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLittleIntermediate, BasisBoneTrackedRole.RightLittleDistal, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, false, 4);
    }
    public bool IsAble()
    {
        if (IsNull(LocalPlayer))
        {
            return false;
        }
        if (IsNull(LocalDriver))
        {
            return false;
        }
        if (IsNull(Player.Avatar))
        {
            return false;
        }
        if (IsNull(Player.Avatar.Animator))
        {
            return false;
        }
        return true;
    }
    public void SetBodySettings(BasisLocalBoneDriver driver)
    {
        Transform UpperChestOrChest = References.Upperchest;
        if (References.Upperchest == null)
        {
            UpperChestOrChest = References.chest;
        }

        GameObject HeadRig = CreateRig("chest, neck, head", true, out RigHeadRig, out HeadLayer);
        CreateTwoBone(driver, HeadRig, References.chest, References.neck, References.head, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Neck, true, out HeadTwoBoneIK, false, false);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.Head))
        {
            WriteUpEvents(Control, HeadLayer);
        }

        GameObject RightShoulder = CreateRig("Upperchest, RightShoulder, RightUpperArm", true, out RightShoulderRig, out RightShoulderLayer);
        CreateTwoBone(driver, RightShoulder, UpperChestOrChest, References.RightShoulder, References.RightUpperArm, BasisBoneTrackedRole.RightUpperArm, BasisBoneTrackedRole.RightShoulder, true, out RightShoulderTwoBoneIK, false, false);
        if (driver.FindBone(out Control, BasisBoneTrackedRole.RightShoulder))
        {
            WriteUpEvents(Control, RightShoulderLayer);
        }

        GameObject LeftShoulder = CreateRig("UpperChest, leftShoulder, leftUpperArm", true, out LeftShoulderRig, out LeftShoulderLayer);
        CreateTwoBone(driver, LeftShoulder, UpperChestOrChest, References.leftShoulder, References.leftUpperArm, BasisBoneTrackedRole.LeftUpperArm, BasisBoneTrackedRole.LeftShoulder, true, out LeftShoulderTwoBoneIK, false, false);
        if (driver.FindBone(out Control, BasisBoneTrackedRole.LeftShoulder))
        {
            WriteUpEvents(Control, LeftShoulderLayer);
        }

        GameObject Body = CreateRig("UpperChest, Chest, Spine", true, out ChestSpineRig, out UpperChestLayer);
        CreateTwoBone(driver, Body, References.spine, References.Upperchest, References.chest, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Neck, true, out UpperChestTwoBoneIK, false, false);
        if (driver.FindBone(out Control, BasisBoneTrackedRole.UpperChest))
        {
            WriteUpEvents(Control, UpperChestLayer);
        }
        LeftHand(driver);
        RightHand(driver);
        LeftFoot(driver);
        RightFoot(driver);
        LeftToe(driver);
        RightToe(driver);
        CreateLeftHandFingers(driver);
        CreateRightHandFingers(driver);
    }
    public void LeftHand(BasisLocalBoneDriver driver)
    {
        GameObject Hands = CreateRig("leftUpperArm, leftLowerArm, leftHand", false, out LeftHandRig, out LeftHandLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.LeftHand))
        {
            WriteUpEvents(Control, LeftHandLayer);
        }
        CreateTwoBone(driver, Hands, References.leftUpperArm, References.leftLowerArm, References.leftHand, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftLowerArm, true, out LeftHandTwoBoneIK, false, false);
    }
    public void RightHand(BasisLocalBoneDriver driver)
    {
        GameObject Hands = CreateRig("RightUpperArm, RightLowerArm, rightHand", false, out RightHandRig, out RightHandLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightHand))
        {
            WriteUpEvents(Control, RightHandLayer);
        }
        CreateTwoBone(driver, Hands, References.RightUpperArm, References.RightLowerArm, References.rightHand, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightLowerArm, true, out RightHandTwoBoneIK, false, false);
    }
    public void CreateLeftHandFingers(BasisLocalBoneDriver driver)
    {
        GameObject Hands = CreateRig("Left Hand Fingers", false, out LeftFingersRig, out LeftFingersLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.LeftHand))
        {
            WriteUpEvents(Control, LeftFingersLayer);
        }
        // Create fingers
        /*
     CreateTwoBone(driver, Hands, References.LeftThumbProximal, References.LeftThumbIntermediate, References.LeftThumbDistal, BasisBoneTrackedRole.LeftThumbDistal, BasisBoneTrackedRole.LeftThumbIntermediate, false, out TwoBoneIKConstraint A, false, false);
     CreateTwoBone(driver, Hands, References.LeftIndexProximal, References.LeftIndexIntermediate, References.LeftIndexDistal, BasisBoneTrackedRole.LeftIndexDistal, BasisBoneTrackedRole.LeftIndexIntermediate, false, out TwoBoneIKConstraint B, false, false);
     CreateTwoBone(driver, Hands, References.LeftMiddleProximal, References.LeftMiddleIntermediate, References.LeftMiddleDistal, BasisBoneTrackedRole.LeftMiddleDistal, BasisBoneTrackedRole.LeftMiddleIntermediate, false, out TwoBoneIKConstraint C, false, false);
     CreateTwoBone(driver, Hands, References.LeftRingProximal, References.LeftRingIntermediate, References.LeftRingDistal, BasisBoneTrackedRole.LeftRingDistal, BasisBoneTrackedRole.LeftRingIntermediate, false, out TwoBoneIKConstraint D, false, false);
     CreateTwoBone(driver, Hands, References.LeftLittleProximal, References.LeftLittleIntermediate, References.LeftLittleDistal, BasisBoneTrackedRole.LeftLittleDistal, BasisBoneTrackedRole.LeftLittleIntermediate, false, out TwoBoneIKConstraint E, false, false);
     LeftFingers.Add(A);
     LeftFingers.Add(B);
     LeftFingers.Add(C);
     LeftFingers.Add(D);
     LeftFingers.Add(E);
            */
    }
    public void CreateRightHandFingers(BasisLocalBoneDriver driver)
    {
        GameObject Hands = CreateRig("Right Hand Fingers", false, out RightFingersRig, out RightFingersLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightHand))
        {
            WriteUpEvents(Control, RightFingersLayer);
        }
        // Create fingers
        /*
        CreateTwoBone(driver, Hands, References.RightThumbProximal, References.RightThumbIntermediate, References.RightThumbDistal, BasisBoneTrackedRole.RightThumbDistal, BasisBoneTrackedRole.RightThumbIntermediate, false, out TwoBoneIKConstraint A, false, false);
        CreateTwoBone(driver, Hands, References.RightIndexProximal, References.RightIndexIntermediate, References.RightIndexDistal, BasisBoneTrackedRole.RightIndexDistal, BasisBoneTrackedRole.RightIndexIntermediate, false, out TwoBoneIKConstraint B, false, false);
        CreateTwoBone(driver, Hands, References.RightMiddleProximal, References.RightMiddleIntermediate, References.RightMiddleDistal, BasisBoneTrackedRole.RightMiddleDistal, BasisBoneTrackedRole.RightMiddleIntermediate, false, out TwoBoneIKConstraint C, false, false);
        CreateTwoBone(driver, Hands, References.RightRingProximal, References.RightRingIntermediate, References.RightRingDistal, BasisBoneTrackedRole.RightRingDistal, BasisBoneTrackedRole.RightRingIntermediate, false, out TwoBoneIKConstraint D, false, false);
        CreateTwoBone(driver, Hands, References.RightLittleProximal, References.RightLittleIntermediate, References.RightLittleDistal, BasisBoneTrackedRole.RightLittleDistal, BasisBoneTrackedRole.RightLittleIntermediate, false, out TwoBoneIKConstraint E, false, false);
        RightFingers.Add(A);
        RightFingers.Add(B);
        RightFingers.Add(C);
        RightFingers.Add(D);
        RightFingers.Add(E);
        */
    }
    public void LeftFoot(BasisLocalBoneDriver driver)
    {
        GameObject feet = CreateRig("LeftUpperLeg, LeftLowerLeg, leftFoot", false, out LeftFootRig, out LeftFootLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.LeftFoot))
        {
            WriteUpEvents(Control, LeftFootLayer);
        }
        CreateTwoBone(driver, feet, References.LeftUpperLeg, References.LeftLowerLeg, References.leftFoot, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftLowerLeg, true, out LeftFootTwoBoneIK, false, false);
    }
    public void RightFoot(BasisLocalBoneDriver driver)
    {
        GameObject feet = CreateRig("RightUpperLeg, RightLowerLeg, rightFoot", false, out RightFootRig, out RightFootLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightFoot))
        {
            WriteUpEvents(Control, RightFootLayer);
        }
        CreateTwoBone(driver, feet, References.RightUpperLeg, References.RightLowerLeg, References.rightFoot, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightLowerLeg, true, out RightFootTwoBoneIK, false, false);
    }

    public void LeftToe(BasisLocalBoneDriver driver)
    {
        GameObject LeftToe = CreateRig("LeftToe", false, out LeftToeRig, out LeftToeLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.LeftToes))
        {
            WriteUpEvents(Control, LeftToeLayer);
        }
        Damp(driver, LeftToe, References.leftToes, BasisBoneTrackedRole.LeftToes, 0, 0);
    }
    public void RightToe(BasisLocalBoneDriver driver)
    {
        GameObject RightToe = CreateRig("RightToe", false, out RightToeRig, out RightToeLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightToes))
        {
            WriteUpEvents(Control, RightToeLayer);
        }
        Damp(driver, RightToe, References.rightToes, BasisBoneTrackedRole.RightToes, 0, 0);
    }
    public void CalibrateRoles()
    {
        for (int Index = 0; Index < BasisLocalPlayer.Instance.LocalBoneDriver.trackedRoles.Length; Index++)
        {
            BasisBoneTrackedRole role = BasisLocalPlayer.Instance.LocalBoneDriver.trackedRoles[Index];
            BasisBoneControl BoneControl = BasisLocalPlayer.Instance.LocalBoneDriver.Controls[Index];
            if (BoneControl.HasRigLayer == BasisHasRigLayer.HasRigLayer)
            {
                ApplyHint(role, 1);
            }
            else
            {
                ApplyHint(role, 0);
            }
        }
    }
    public void ApplyHint(BasisBoneTrackedRole RoleWithHint, int weight)
    {
        switch (RoleWithHint)
        {
            case BasisBoneTrackedRole.Neck:
                // Debug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                HeadTwoBoneIK.data.hintWeight = weight;
                break;

            case BasisBoneTrackedRole.RightLowerLeg:
                // Debug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                RightFootTwoBoneIK.data.hintWeight = weight;
                break;

            case BasisBoneTrackedRole.LeftLowerLeg:
                // Debug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                LeftFootTwoBoneIK.data.hintWeight = weight;
                break;

            case BasisBoneTrackedRole.RightUpperArm:
                // Debug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                RightHandTwoBoneIK.data.hintWeight = weight;
                break;

            case BasisBoneTrackedRole.LeftUpperArm:
                // Debug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                LeftHandTwoBoneIK.data.hintWeight = weight;
                break;

            case BasisBoneTrackedRole.LeftShoulder:
                // Debug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                LeftShoulderTwoBoneIK.data.hintWeight = weight;
                break;

            case BasisBoneTrackedRole.RightShoulder:
                // Debug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                RightShoulderTwoBoneIK.data.hintWeight = weight;
                break;

            default:
                // Optional: Handle cases where RoleWithHint does not match any of the expected roles
                // Debug.Log("Unknown role: " + RoleWithHint);
                break;
        }
    }
    public void WriteUpEvents(BasisBoneControl Control, RigLayer Layer)
    {
        // Define a method to update the active state of the Layer
        void UpdateLayerActiveState()
        {
            Layer.active = Control.HasRigLayer == BasisHasRigLayer.HasRigLayer;
        }

        // Subscribe to the events
        Control.OnHasRigChanged += delegate { UpdateLayerActiveState(); };

        // Set the initial state
        UpdateLayerActiveState();
    }
    public GameObject CreateRig(string Role, bool Enabled, out Rig Rig, out RigLayer RigLayer)
    {
        GameObject RigGameobject = CreateAndSetParent(Player.Avatar.Animator.transform, "Rig " + Role);
        Rig = BasisHelpers.GetOrAddComponent<Rig>(RigGameobject);
        Rigs.Add(Rig);
        RigLayer = new RigLayer(Rig, Enabled);
        Builder.layers.Add(RigLayer);
        return RigGameobject;
    }
}