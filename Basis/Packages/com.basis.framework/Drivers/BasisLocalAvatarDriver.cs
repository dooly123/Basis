using Basis.Scripts.Animator_Driver;
using Basis.Scripts.Avatar;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management;
using Basis.Scripts.Eye_Follow;
using Basis.Scripts.TransformBinders.BoneControl;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

namespace Basis.Scripts.Drivers
{
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
       // public TwoBoneIKConstraint UpperChestTwoBoneIK;
        [SerializeField]
        public List<TwoBoneIKConstraint> LeftFingers = new List<TwoBoneIKConstraint>();
        [SerializeField]
        public List<TwoBoneIKConstraint> RightFingers = new List<TwoBoneIKConstraint>();
        public Rig LeftToeRig;
        public Rig RightToeRig;

        public Rig RigChestRig;
        public Rig RigHeadRig;
        public Rig LeftHandRig;
        public Rig RightHandRig;
        public Rig LeftFootRig;
        public Rig RightFootRig;
      //  public Rig ChestSpineRig;
        public Rig LeftShoulderRig;
        public Rig RightShoulderRig;

        public RigLayer LeftHandLayer;
        public RigLayer RightHandLayer;
        public RigLayer LeftFootLayer;
        public RigLayer RightFootLayer;
        public RigLayer LeftToeLayer;
        public RigLayer RightToeLayer;

        public RigLayer RigHeadLayer;
        public RigLayer RigChestLayer;
       // public RigLayer ChestSpineLayer;

        public RigLayer LeftShoulderLayer;
        public RigLayer RightShoulderLayer;
        public List<Rig> Rigs = new List<Rig>();
        public RigBuilder Builder;
        public List<RigTransform> AdditionalTransforms = new List<RigTransform>();
        public bool HasTposeEvent = false;
        public string Locomotion = "Locomotion";
        public BasisMuscleDriver BasisMuscleDriver;
        public BasisEyeFollowBase BasisLocalEyeFollowDriver;
        public PlayableGraph PlayableGraph;
        public void InitialLocalCalibration(BasisLocalPlayer Player)
        {
            BasisDebug.Log("InitialLocalCalibration");
            if (HasTposeEvent == false)
            {
                TposeStateChange += OnTpose;
                HasTposeEvent = true;
            }
            LocalPlayer = Player;
            this.LocalDriver = LocalPlayer.LocalBoneDriver;
            if (IsAble())
            {
                // BasisDebug.Log("LocalCalibration Underway");
            }
            else
            {
                return;
            }
            CleanupBeforeContinue();
            AdditionalTransforms.Clear();
            Rigs.Clear();
            Player.BasisAvatar.Animator.updateMode = AnimatorUpdateMode.Normal;
            Player.BasisAvatar.Animator.logWarnings = false;
            if (Player.BasisAvatar.Animator.runtimeAnimatorController == null)
            {
                UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<RuntimeAnimatorController> op = Addressables.LoadAssetAsync<RuntimeAnimatorController>(Locomotion);
                RuntimeAnimatorController RAC = op.WaitForCompletion();
                Player.BasisAvatar.Animator.runtimeAnimatorController = RAC;
            }
            Player.BasisAvatar.Animator.applyRootMotion = false;
            PutAvatarIntoTPose();
            if (Builder != null)
            {
                GameObject.Destroy(Builder);
            }
            Builder = BasisHelpers.GetOrAddComponent<RigBuilder>(Player.BasisAvatar.Animator.gameObject);
            Builder.enabled = false;
            Calibration(Player.BasisAvatar);
            BasisLocalPlayer.Instance.LocalBoneDriver.RemoveAllListeners();
            BasisLocalPlayer.Instance.LocalBoneDriver.CalculateHeading();
            BasisLocalEyeFollowDriver = BasisHelpers.GetOrAddComponent<BasisEyeFollowBase>(Player.BasisAvatar.gameObject);
            BasisLocalEyeFollowDriver.Initalize(this,Player);
            HeadScaledDown = Vector3.zero;
            SetAllMatrixRecalculation(true);
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
            CalculateTransformPositions(Player.BasisAvatar.Animator, LocalDriver);
            ComputeOffsets(LocalDriver);
            BasisMuscleDriver = BasisHelpers.GetOrAddComponent<BasisMuscleDriver>(Player.BasisAvatar.Animator.gameObject);
            BasisMuscleDriver.DisposeAllJobsData();
            BasisMuscleDriver.Initialize(Player, Player.BasisAvatar.Animator);

            CalibrationComplete?.Invoke();

            AnimatorDriver = BasisHelpers.GetOrAddComponent<BasisLocalAnimatorDriver>(Player.BasisAvatar.Animator.gameObject);
            AnimatorDriver.Initialize(Player.BasisAvatar.Animator);

            ResetAvatarAnimator();
            BasisAvatarIKStageCalibration.HasFBIKTrackers = false;
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl Head, BasisBoneTrackedRole.Head))
            {
                Head.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            }
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl Hips, BasisBoneTrackedRole.Hips))
            {
                Hips.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            }
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl Spine, BasisBoneTrackedRole.Spine))
            {
                Spine.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            }
            StoredRolesTransforms = BasisAvatarIKStageCalibration.GetAllRolesAsTransform();
            Player.BasisAvatar.transform.parent = Hips.BoneTransform;
            Player.BasisAvatar.transform.SetLocalPositionAndRotation(-Hips.TposeLocal.position,Quaternion.identity);
            CalibrateOffsets();
            BuildBuilder();
        }
        public void OnDestroy()
        {
            if (BasisMuscleDriver != null)
            {
                BasisMuscleDriver.DisposeAllJobsData();
            }
        }
        public Dictionary<BasisBoneTrackedRole, Transform> StoredRolesTransforms;
        public void CalibrateOffsets()
        {
            BasisLocalBoneDriver Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
            for (int Index = 0; Index < Driver.ControlsLength; Index++)
            {
                Driver.Controls[Index].BoneTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

        }
        public void BuildBuilder()
        {
            PlayableGraph = Player.BasisAvatar.Animator.playableGraph;
            PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            Builder.Build(PlayableGraph);
        }
        public void OnTpose()
        {
            if (Builder != null)
            {
                foreach (RigLayer Layer in Builder.layers)
                {
                    if (CurrentlyTposing)
                    {
                        Layer.active = false;
                    }
                    else
                    {
                    }
                }
                if (CurrentlyTposing == false)
                {
                    foreach (BasisBoneControl control in BasisLocalPlayer.Instance.LocalBoneDriver.Controls)
                    {
                        control.OnHasRigChanged?.Invoke();
                    }
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

            if (RigChestRig != null)
            {
                Destroy(RigChestRig.gameObject);
            }
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
            //if (ChestSpineRig != null)
           // {
            //    Destroy(ChestSpineRig.gameObject);
            //}
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
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.CenterEye, BasisBoneTrackedRole.Head, 40, 35, true);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Neck, 40, 35, true);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Mouth, 40, 30, true);


            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Neck, BasisBoneTrackedRole.Chest, 40, 30, true);



            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.Spine, 40, 14, true);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Hips, 40, 14, true);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.LeftShoulder, 40, 14, true);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Chest, BasisBoneTrackedRole.RightShoulder, 40, 14, true);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftShoulder, BasisBoneTrackedRole.LeftUpperArm, 40, 14, true);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightShoulder, BasisBoneTrackedRole.RightUpperArm, 40, 14, true);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperArm, BasisBoneTrackedRole.LeftLowerArm, 40, 14, true);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperArm, BasisBoneTrackedRole.RightLowerArm, 40, 14, true);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerArm, BasisBoneTrackedRole.LeftHand, 40, 14, true);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerArm, BasisBoneTrackedRole.RightHand, 40, 14, true);

            //legs
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.LeftUpperLeg, 40, 14, true);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.Hips, BasisBoneTrackedRole.RightUpperLeg, 40, 14, true);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftUpperLeg, BasisBoneTrackedRole.LeftLowerLeg, 40, 14, true);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightUpperLeg, BasisBoneTrackedRole.RightLowerLeg, 40, 14, true);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLowerLeg, BasisBoneTrackedRole.LeftFoot, 40, 14, true);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLowerLeg, BasisBoneTrackedRole.RightFoot, 40, 14, true);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftToes, 40, 14, true);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightToes, 4, 14, true);



            // Setting up locks for Left Hand
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftThumbProximal, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftThumbProximal, BasisBoneTrackedRole.LeftThumbIntermediate, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftThumbIntermediate, BasisBoneTrackedRole.LeftThumbDistal, 40, 7, false);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftIndexProximal, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftIndexProximal, BasisBoneTrackedRole.LeftIndexIntermediate, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftIndexIntermediate, BasisBoneTrackedRole.LeftIndexDistal, 40, 7, false);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftMiddleProximal, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftMiddleProximal, BasisBoneTrackedRole.LeftMiddleIntermediate, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftMiddleIntermediate, BasisBoneTrackedRole.LeftMiddleDistal, 40, 7, false);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftRingProximal, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftRingProximal, BasisBoneTrackedRole.LeftRingIntermediate, 40, 12, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftRingIntermediate, BasisBoneTrackedRole.LeftRingDistal, 40, 12, false);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftLittleProximal, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLittleProximal, BasisBoneTrackedRole.LeftLittleIntermediate, 40, 12, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.LeftLittleIntermediate, BasisBoneTrackedRole.LeftLittleDistal, 40, 12, false);

            // Setting up locks for Right Hand
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightThumbProximal, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightThumbProximal, BasisBoneTrackedRole.RightThumbIntermediate, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightThumbIntermediate, BasisBoneTrackedRole.RightThumbDistal, 40, 7, false);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightIndexProximal, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightIndexProximal, BasisBoneTrackedRole.RightIndexIntermediate, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightIndexIntermediate, BasisBoneTrackedRole.RightIndexDistal, 40, 7, false);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightMiddleProximal, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightMiddleProximal, BasisBoneTrackedRole.RightMiddleIntermediate, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightMiddleIntermediate, BasisBoneTrackedRole.RightMiddleDistal, 40, 7, false);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightRingProximal, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightRingProximal, BasisBoneTrackedRole.RightRingIntermediate, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightRingIntermediate, BasisBoneTrackedRole.RightRingDistal, 40, 7, false);

            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightLittleProximal, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLittleProximal, BasisBoneTrackedRole.RightLittleIntermediate, 40, 7, false);
            SetAndCreateLock(BaseBoneDriver, BasisBoneTrackedRole.RightLittleIntermediate, BasisBoneTrackedRole.RightLittleDistal, 40, 7, false);
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
            if (IsNull(Player.BasisAvatar))
            {
                return false;
            }
            if (IsNull(Player.BasisAvatar.Animator))
            {
                return false;
            }
            return true;
        }
        public void SetBodySettings(BasisLocalBoneDriver driver)
        {
            SetupHeadRig(driver);
            //  SetupChestRig(driver);
            //  SetupRightShoulderRig(driver);
            //  SetupLeftShoulderRig(driver);
            LeftHand(driver);
            RightHand(driver);
            LeftFoot(driver);
            RightFoot(driver);

            LeftToe(driver);
            RightToe(driver);
        }
        /// <summary>
        /// Sets up the Head rig, including chest, neck, and head bones.
        /// </summary>
        private void SetupChestRig(BasisLocalBoneDriver driver)
        {
            GameObject ChestRig = CreateRig("Chest", false, out RigChestRig, out RigChestLayer);
            Damp(driver, ChestRig, References.chest, BasisBoneTrackedRole.Chest, 1, 1);

            List<BasisBoneControl> controls = new List<BasisBoneControl>();
            if (driver.FindBone(out BasisBoneControl Chest, BasisBoneTrackedRole.Chest))
            {
                controls.Add(Chest);
            } 
            WriteUpEvents(controls, RigChestLayer);
        }
        /// <summary>
        /// Sets up the Head rig, including chest, neck, and head bones.
        /// </summary>
        private void SetupHeadRig(BasisLocalBoneDriver driver)
        {
            GameObject HeadRig = CreateRig("Chest, Neck, Head", true, out RigHeadRig, out RigHeadLayer);
            CreateTwoBone(driver, HeadRig, References.chest, References.neck, References.head, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Chest, true, out HeadTwoBoneIK, true, true);

            List<BasisBoneControl> controls = new List<BasisBoneControl>();
            if (driver.FindBone(out BasisBoneControl Head, BasisBoneTrackedRole.Head))
            {
                controls.Add(Head);
            }
            if (driver.FindBone(out BasisBoneControl Chest, BasisBoneTrackedRole.Chest))
            {
                controls.Add(Chest);
            }
            WriteUpEvents(controls, RigHeadLayer);
        }

        /// <summary>
        /// Sets up the Right Shoulder rig, including chest, right shoulder, and right upper arm bones.
        /// </summary>
        private void SetupRightShoulderRig(BasisLocalBoneDriver driver)
        {
            GameObject RightShoulder = CreateRig("RightShoulder", false, out RightShoulderRig, out RightShoulderLayer);
            Damp(driver, RightShoulder, References.RightShoulder, BasisBoneTrackedRole.RightShoulder, 1, 1);
            List<BasisBoneControl> controls = new List<BasisBoneControl>();
            if (driver.FindBone(out BasisBoneControl RightShoulderRole, BasisBoneTrackedRole.RightShoulder))
            {
                controls.Add(RightShoulderRole);
            }
            WriteUpEvents(controls, RightShoulderLayer);
        }

        /// <summary>
        /// Sets up the Left Shoulder rig, including chest, left shoulder, and left upper arm bones.
        /// </summary>
        private void SetupLeftShoulderRig(BasisLocalBoneDriver driver)
        {
            GameObject LeftShoulder = CreateRig("LeftShoulder", false, out LeftShoulderRig, out LeftShoulderLayer);
            Damp(driver, LeftShoulder, References.leftShoulder, BasisBoneTrackedRole.LeftShoulder, 1, 1);
            List<BasisBoneControl> controls = new List<BasisBoneControl>();
            if (driver.FindBone(out BasisBoneControl LeftShoulderRole, BasisBoneTrackedRole.LeftShoulder))
            {
                controls.Add(LeftShoulderRole);
            }
            WriteUpEvents(controls, LeftShoulderLayer);
        }
        public void LeftHand(BasisLocalBoneDriver driver)
        {
            GameObject Hands = CreateRig("LeftUpperArm, LeftLowerArm, LeftHand", false, out LeftHandRig, out LeftHandLayer);
            List<BasisBoneControl> controls = new List<BasisBoneControl>();
            if (driver.FindBone(out BasisBoneControl LeftHand, BasisBoneTrackedRole.LeftHand))
            {
                controls.Add(LeftHand);
            }
            if (driver.FindBone(out BasisBoneControl LeftLowerArm, BasisBoneTrackedRole.LeftLowerArm))
            {
                controls.Add(LeftLowerArm);
            }
            WriteUpEvents(controls, LeftHandLayer);
            CreateTwoBone(driver, Hands, References.leftUpperArm, References.leftLowerArm, References.leftHand, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftLowerArm, true, out LeftHandTwoBoneIK, false, true);
        }
        public void RightHand(BasisLocalBoneDriver driver)
        {
            GameObject Hands = CreateRig("RightUpperArm, RightLowerArm, RightHand", false, out RightHandRig, out RightHandLayer);
            List<BasisBoneControl> controls = new List<BasisBoneControl>();
            if (driver.FindBone(out BasisBoneControl RightHand, BasisBoneTrackedRole.RightHand))
            {
                controls.Add(RightHand);
            }
            if (driver.FindBone(out BasisBoneControl RightLowerArm, BasisBoneTrackedRole.RightLowerArm))
            {
                controls.Add(RightLowerArm);
            }
            WriteUpEvents(controls, RightHandLayer);
            CreateTwoBone(driver, Hands, References.RightUpperArm, References.RightLowerArm, References.rightHand, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightLowerArm, true, out RightHandTwoBoneIK, false, true);
        }
        public void LeftFoot(BasisLocalBoneDriver driver)
        {
            GameObject feet = CreateRig("LeftUpperLeg, LeftLowerLeg, LeftFoot", false, out LeftFootRig, out LeftFootLayer);
            List<BasisBoneControl> controls = new List<BasisBoneControl>();
            if (driver.FindBone(out BasisBoneControl LeftFoot, BasisBoneTrackedRole.LeftFoot))
            {
                controls.Add(LeftFoot);
            }
            WriteUpEvents(controls, LeftFootLayer);
            CreateTwoBone(driver, feet, References.LeftUpperLeg, References.LeftLowerLeg, References.leftFoot, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftLowerLeg, true, out LeftFootTwoBoneIK, false, true);
        }
        public void RightFoot(BasisLocalBoneDriver driver)
        {
            GameObject feet = CreateRig("RightUpperLeg, RightLowerLeg, RightFoot", false, out RightFootRig, out RightFootLayer);
            List<BasisBoneControl> controls = new List<BasisBoneControl>();
            if (driver.FindBone(out BasisBoneControl RightFoot, BasisBoneTrackedRole.RightFoot))
            {
                controls.Add(RightFoot);
            }
            if (driver.FindBone(out BasisBoneControl RightLowerLeg, BasisBoneTrackedRole.RightLowerLeg))
            {
                controls.Add(RightLowerLeg);
            }

            WriteUpEvents(controls, RightFootLayer);

            CreateTwoBone(driver, feet, References.RightUpperLeg, References.RightLowerLeg, References.rightFoot, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightLowerLeg, true, out RightFootTwoBoneIK, false, true);
        }
        public void LeftToe(BasisLocalBoneDriver driver)
        {
            GameObject LeftToe = CreateRig("LeftToe", false, out LeftToeRig, out LeftToeLayer);
            if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.LeftToes))
            {
                WriteUpEvents(new List<BasisBoneControl>() { Control }, LeftToeLayer);
            }
            Damp(driver, LeftToe, References.leftToes, BasisBoneTrackedRole.LeftToes, 0, 0);
        }
        public void RightToe(BasisLocalBoneDriver driver)
        {
            GameObject RightToe = CreateRig("RightToe", false, out RightToeRig, out RightToeLayer);
            if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightToes))
            {
                WriteUpEvents(new List<BasisBoneControl>() { Control }, RightToeLayer);
            }
            Damp(driver, RightToe, References.rightToes, BasisBoneTrackedRole.RightToes, 0, 0);
        }
        public void CalibrateRoles()
        {
            foreach (BasisBoneTrackedRole Role in Enum.GetValues(typeof(BasisBoneTrackedRole)))
            {
                ApplyHint(Role, 0);
            }
            for (int Index = 0; Index < BasisDeviceManagement.Instance.AllInputDevices.Count; Index++)
            {
                Device_Management.Devices.BasisInput BasisInput = BasisDeviceManagement.Instance.AllInputDevices[Index];
                if (BasisInput.TryGetRole(out BasisBoneTrackedRole Role))
                {
                    ApplyHint(Role, 1);
                }
            }
        }
        public void ApplyHint(BasisBoneTrackedRole RoleWithHint, int weight)
        {
            switch (RoleWithHint)
            {
                case BasisBoneTrackedRole.Chest:
                    // BasisDebug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                    HeadTwoBoneIK.data.hintWeight = weight;
                    break;

                case BasisBoneTrackedRole.RightLowerLeg:
                    // BasisDebug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                    RightFootTwoBoneIK.data.hintWeight = weight;
                    break;

                case BasisBoneTrackedRole.LeftLowerLeg:
                    // BasisDebug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                    LeftFootTwoBoneIK.data.hintWeight = weight;
                    break;

                case BasisBoneTrackedRole.RightUpperArm:
                    // BasisDebug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                    RightHandTwoBoneIK.data.hintWeight = weight;
                    break;

                case BasisBoneTrackedRole.LeftUpperArm:
                    // BasisDebug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                    LeftHandTwoBoneIK.data.hintWeight = weight;
                    break;
                case BasisBoneTrackedRole.LeftLowerArm:
                    // BasisDebug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                    RightHandTwoBoneIK.data.hintWeight = weight;
                    break;

                case BasisBoneTrackedRole.RightLowerArm:
                    // BasisDebug.Log("Setting Hint For " + RoleWithHint + " with weight " + weight);
                    LeftHandTwoBoneIK.data.hintWeight = weight;
                    break;
                default:
                    // Optional: Handle cases where RoleWithHint does not match any of the expected roles
                    // BasisDebug.Log("Unknown role: " + RoleWithHint);
                    break;
            }
        }
        /// <summary>
        /// Clears on a calibration, setting up event listeners for a list of controls.
        /// </summary>
        /// <param name="Controls">List of BasisBoneControl objects</param>
        /// <param name="Layer">The RigLayer to update</param>
        public void WriteUpEvents(List<BasisBoneControl> Controls, RigLayer Layer)
        {
            foreach (var control in Controls)
            {
                // Add event listener for each control to update Layer's active state when HasRigLayer changes
                control.OnHasRigChanged.AddListener(delegate { UpdateLayerActiveState(Controls, Layer); });
                control.HasEvents = true;
            }

            // Set the initial state based on the current controls' states
            UpdateLayerActiveState(Controls, Layer);
        }

        // Define a method to update the active state of the Layer based on the list of controls
        void UpdateLayerActiveState(List<BasisBoneControl> Controls, RigLayer Layer)
        {
            // Check if any control in the list has HasRigLayer set to true
            Layer.active = Controls.Any(control => control.HasRigLayer == BasisHasRigLayer.HasRigLayer);
           // BasisDebug.Log("Update Layer to State " + Layer.active + " for layer " + Layer);
        }
        public GameObject CreateRig(string Role, bool Enabled, out Rig Rig, out RigLayer RigLayer)
        {
            GameObject RigGameobject = CreateAndSetParent(Player.BasisAvatar.Animator.transform, "Rig " + Role);
            Rig = BasisHelpers.GetOrAddComponent<Rig>(RigGameobject);
            Rigs.Add(Rig);
            RigLayer = new RigLayer(Rig, Enabled);
            Builder.layers.Add(RigLayer);
            return RigGameobject;
        }
    }
}