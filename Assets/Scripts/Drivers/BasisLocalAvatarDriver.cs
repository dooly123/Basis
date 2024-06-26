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
        CleanupBeforeContinue();
        AdditionalTransforms.Clear();
        Rigs.Clear();
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
        CalibrationComplete.Invoke();
        AnimatorDriver = BasisHelpers.GetOrAddComponent<BasisLocalAnimatorDriver>(Player.Avatar.Animator.gameObject);
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
    /// <summary>
    /// only called when dealing with trackers
    /// </summary>
    public void CalculateOffsetsAndTpose()
    {
        PutAvatarIntoTpose();
     //   CalculateTransformPositions(Player.Avatar.Animator, LocalDriver);
     //   ComputeOffsets(LocalDriver);
      //  Builder.Build();
        ResetAvatarAnimator();
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
    public void CleanupBeforeContinue()
    {
        if (Builder != null)
        {
            Destroy(Builder);
        }
        Builder = null;
        if (RigHead != null)
        {
            Destroy(RigHead.gameObject);
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
        if (ChestSpine != null)
        {
            Destroy(ChestSpine.gameObject);
        }
        if (Spine != null)
        {
            Destroy(Spine.gameObject);
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
        SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Hips, BasisTargetController.TargetDirectional, 40, BasisClampData.None, 0, 12, true, 4        );

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
        CreateTwoBone(driver, HeadRig, References.chest, References.neck, References.head, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Head,false, out HeadTwoBoneIK, false, false);

        GameObject Body = CreateRig("Upper Chest", true, out ChestSpine, out UpperChestLayer);
        CreateTwoBone(driver, Body, null, References.spine, References.chest, BasisBoneTrackedRole.UpperChest, BasisBoneTrackedRole.UpperChest, false, out UpperChestTwoBoneIK, true, true);

        GameObject SpineGo = CreateRig("Spine", true, out Spine, out SpineLayer);
        CreateTwoBone(driver, SpineGo, null, null, References.spine, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Spine, false, out UpperChestTwoBoneIK, true, true);

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
        CreateTwoBone(driver, Hands, References.leftUpperArm, References.leftLowerArm, References.leftHand, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftUpperArm, false, out LeftHandTwoBoneIK, false, true);
    }
    public void RightHand(BasisLocalBoneDriver driver)
    {
        GameObject Hands = CreateRig("RightHand", false, out RightHandRig, out RightHandLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightHand))
        {
            WriteUpEvents(Control, RightHandLayer);
        }
        CreateTwoBone(driver, Hands, References.RightUpperArm, References.RightLowerArm, References.rightHand, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightUpperArm, false, out RightHandTwoBoneIK, false, true);
    }
    public void LeftFoot(BasisLocalBoneDriver driver)
    {
        GameObject feet = CreateRig("LeftFoot", false, out LeftFootRig, out LeftFootLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.LeftFoot))
        {
            WriteUpEvents(Control, LeftFootLayer);
        }
        CreateTwoBone(driver, feet, References.LeftUpperLeg, References.LeftLowerLeg, References.leftFoot, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftLowerLeg, false, out LeftFootTwoBoneIK, false, true);
    }
    public void RightFoot(BasisLocalBoneDriver driver)
    {
        GameObject feet = CreateRig("RightFoot", false, out RightFootRig, out RightFootLayer);
        if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightFoot))
        {
            WriteUpEvents(Control, RightFootLayer);
        }
        CreateTwoBone(driver, feet, References.RightUpperLeg, References.RightLowerLeg, References.rightFoot, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightLowerLeg, false, out RightFootTwoBoneIK, false, true);
    }
    public void ApplyHint(BasisBoneTrackedRole RoleWithHint, int weight)
    {
        if (RoleWithHint == BasisBoneTrackedRole.RightLowerLeg)
        {
            RightFootTwoBoneIK.data.hintWeight = weight;
        }
        if (RoleWithHint == BasisBoneTrackedRole.LeftLowerLeg)
        {
            LeftFootTwoBoneIK.data.hintWeight = weight;

        }
        if (RoleWithHint == BasisBoneTrackedRole.RightUpperArm)
        {
            RightHandTwoBoneIK.data.hintWeight = weight;
        }
        if (RoleWithHint == BasisBoneTrackedRole.LeftUpperArm)
        {
            LeftHandTwoBoneIK.data.hintWeight = weight;
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