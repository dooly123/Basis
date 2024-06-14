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
    public Rig RigHead;
    public Rig LeftHandRig;
    public Rig RightHandRig;
    public Rig LeftFootRig;
    public Rig RightFootRig;
    public Rig ChestSpine;
    public Rig Spine;
    public RigLayer LeftHandLayer;
    public RigLayer RightHandLayer;
    public RigLayer LeftFootLayer;
    public RigLayer RightFootLayer;
    public RigLayer HeadLayer;
    public RigLayer UpperChestLayer;
    public RigLayer SpineLayer;

    public MicrophoneRecorder MicrophoneRecorder;
    public void LocalCalibration()
    {
        LocalCalibration(BasisLocalPlayer.Instance);
    }
    public void LocalCalibration(BasisLocalPlayer Player)
    {
        LocalPlayer = Player;

        this.LocalDriver = LocalPlayer.LocalBoneDriver;
        if (IsAble())
        {
            Debug.Log("LocalCalibration Underway");
        }
        else
        {
            return;
        }
        Calibration(Player.Avatar);
        BasisLocalEyeFollowDriver EyeFollowBase = BasisHelpers.GetOrAddComponent<BasisLocalEyeFollowDriver>(Player.Avatar.gameObject);
        EyeFollowBase.CreateEyeLook(this);
        HeadScaledDown = Vector3.zero;
        SetMatrixRecalculation(true);
        updateWhenOffscreen(true);
        HeadScale = References.head.localScale;
        SetBodySettings(LocalDriver);
        CalculateTransformPositions(Player.Avatar.Animator, LocalDriver);
        ComputeOffsets(LocalDriver);
        Builder.Build();
        CalibrationComplete.Invoke();

        if (AnimatorDriver == null)
        {
            AnimatorDriver = BasisHelpers.GetOrAddComponent<BasisLocalAnimatorDriver>(Player.Avatar.Animator.gameObject);
        }
        AnimatorDriver.Initalize(LocalDriver, Player.Avatar.Animator);

        if (MicrophoneRecorder == null)
        {
            MicrophoneRecorder = BasisHelpers.GetOrAddComponent<MicrophoneRecorder>(this.gameObject);
        }
        else
        {
            MicrophoneRecorder.DeInitialize();
        }
        MicrophoneRecorder.Initialize();
    }
    public void ComputeOffsets(BaseBoneDriver BaseBoneDriver)
    {
        //head
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.CenterEye, BasisBoneTrackedRole.Head, BasisClampData.None, 5, 12, true, 5f);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Neck, BasisClampData.Clamp, 5, 12, true, 4, BasisTargetController.Target, BasisClampAxis.xz);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Mouth, BasisClampData.None, 0, 12, false, 4);


        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Neck, BasisBoneTrackedRole.UpperChest, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.Chest, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.Spine, BasisClampData.None, 0, 12, true, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Hips, BasisClampData.None, 0, 12, true, 4, BasisTargetController.Target, BasisClampAxis.x, false);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.LeftShoulder, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.RightShoulder, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftShoulder, BasisBoneTrackedRole.LeftUpperArm, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightShoulder, BasisBoneTrackedRole.RightUpperArm, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperArm, BasisBoneTrackedRole.LeftLowerArm, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperArm, BasisBoneTrackedRole.RightLowerArm, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerArm, BasisBoneTrackedRole.LeftHand, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerArm, BasisBoneTrackedRole.RightHand, BasisClampData.None, 0, 12, false, 4);

        //legs
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.LeftUpperLeg, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.RightUpperLeg, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperLeg, BasisBoneTrackedRole.LeftLowerLeg, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperLeg, BasisBoneTrackedRole.RightLowerLeg, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerLeg, BasisBoneTrackedRole.LeftFoot, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerLeg, BasisBoneTrackedRole.RightFoot, BasisClampData.None, 0, 12, false, 4);

        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftToes, BasisClampData.None, 0, 12, false, 4);
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightToes, BasisClampData.None, 0, 12, false, 4);
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
        GameObject HeadRig = CreateRig("Head", true, out RigHead, out HeadLayer);
        Hands(driver);
        Feet(driver);
        CreateTwoBone(driver, HeadRig, References.chest, References.neck, References.head, BasisBoneTrackedRole.Head, out HeadTwoBoneIK, false, true);

        GameObject Body = CreateRig("Upper Chest", true, out ChestSpine, out UpperChestLayer);
        CreateTwoBone(driver, Body, null, References.spine, References.chest, BasisBoneTrackedRole.UpperChest, out UpperChestTwoBoneIK, true, true);

        GameObject SpineGo = CreateRig("Spine", true, out Spine, out SpineLayer);
        CreateTwoBone(driver, SpineGo, null, null, References.spine, BasisBoneTrackedRole.Spine, out UpperChestTwoBoneIK, true, true);

        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.Head))
        {
            WriteUpEvents(Control, HeadLayer);
        }
        if (driver.FindBone(out Control, BasisBoneTrackedRole.UpperChest))
        {
            WriteUpEvents(Control, UpperChestLayer);
        }
        if (driver.FindBone(out Control, BasisBoneTrackedRole.Spine))
        {
            WriteUpEvents(Control, SpineLayer);
        }
    }
    public void Hands(BasisLocalBoneDriver driver)
    {
        LeftHand(driver);
        RightHand(driver);
    }
    public void Feet(BasisLocalBoneDriver driver)
    {
        LeftFoot(driver);
        RightFoot(driver);
    }
    public void LeftHand(BasisLocalBoneDriver driver)
    {
        GameObject Hands = CreateRig("LeftHand", false, out LeftHandRig, out LeftHandLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.LeftHand))
        {
            WriteUpEvents(Control, LeftHandLayer);
        }
        CreateTwoBone(driver, Hands, References.leftUpperArm, References.leftLowerArm, References.leftHand, BasisBoneTrackedRole.LeftHand, out LeftHandTwoBoneIK, false, true);
    }
    public void RightHand(BasisLocalBoneDriver driver)
    {
        GameObject Hands = CreateRig("RightHand", false, out RightHandRig, out RightHandLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightHand))
        {
            WriteUpEvents(Control, RightHandLayer);
        }
        CreateTwoBone(driver, Hands, References.RightUpperArm, References.RightLowerArm, References.rightHand, BasisBoneTrackedRole.RightHand, out RightHandTwoBoneIK, false, true);
    }
    public void LeftFoot(BasisLocalBoneDriver driver)
    {
        GameObject feet = CreateRig("LeftFoot", false, out LeftFootRig, out LeftFootLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.LeftFoot))
        {
            WriteUpEvents(Control, LeftFootLayer);
        }
        CreateTwoBone(driver, feet, References.LeftUpperLeg, References.LeftLowerLeg, References.leftFoot, BasisBoneTrackedRole.LeftFoot, out LeftFootTwoBoneIK, false, true);
    }
    public void RightFoot(BasisLocalBoneDriver driver)
    {
        GameObject feet = CreateRig("RightFoot", false, out RightFootRig, out RightFootLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightFoot))
        {
            WriteUpEvents(Control, RightFootLayer);
        }
        CreateTwoBone(driver, feet, References.RightUpperLeg, References.RightLowerLeg, References.rightFoot, BasisBoneTrackedRole.RightFoot, out RightFootTwoBoneIK, false, true);
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
}