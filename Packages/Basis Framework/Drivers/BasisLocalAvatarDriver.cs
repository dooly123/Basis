using Basis.Scripts.Animator_Driver;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Eye_Follow;
using Basis.Scripts.TransformBinders.BoneControl;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Animations.Rigging;

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

        public RigLayer LeftHandLayer;
        public RigLayer RightHandLayer;
        public RigLayer LeftFootLayer;
        public RigLayer RightFootLayer;
        public RigLayer LeftToeLayer;
        public RigLayer RightToeLayer;

        public RigLayer RigHeadLayer;
        public RigLayer ChestSpineLayer;

        public RigLayer LeftShoulderLayer;
        public RigLayer RightShoulderLayer;
        public List<Rig> Rigs = new List<Rig>();
        public RigBuilder Builder;
        public List<RigTransform> AdditionalTransforms = new List<RigTransform>();
        public bool HasTposeEvent = false;
        public string Locomotion = "Locomotion";
        public BasisMuscleDriver BasisMuscleDriver;
        public BasisLocalEyeFollowDriver BasisLocalEyeFollowDriver;
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
            if (Player.Avatar.Animator.runtimeAnimatorController == null)
            {
                UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<RuntimeAnimatorController> op = Addressables.LoadAssetAsync<RuntimeAnimatorController>(Locomotion);
                RuntimeAnimatorController RAC = op.WaitForCompletion();
                Player.Avatar.Animator.runtimeAnimatorController = RAC;
            }
            Player.Avatar.Animator.applyRootMotion = false;
            PutAvatarIntoTPose();
            if (Builder != null)
            {
                GameObject.Destroy(Builder);
            }
            Builder = BasisHelpers.GetOrAddComponent<RigBuilder>(Player.Avatar.Animator.gameObject);
            Calibration(Player.Avatar);
            BasisLocalPlayer.Instance.LocalBoneDriver.RemoveAllListeners();
            BasisLocalPlayer.Instance.LocalBoneDriver.CalculateHeading();
            BasisLocalEyeFollowDriver = BasisHelpers.GetOrAddComponent<BasisLocalEyeFollowDriver>(Player.Avatar.gameObject);
            BasisLocalEyeFollowDriver.Initalize(this);
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
            CalculateTransformPositions(Player.Avatar.Animator, LocalDriver);
            ComputeOffsets(LocalDriver);
            CalibrationComplete?.Invoke();

            BasisMuscleDriver = BasisHelpers.GetOrAddComponent<BasisMuscleDriver>(Player.Avatar.Animator.gameObject);
            BasisMuscleDriver.Initialize(this, Player.Avatar.Animator);

            AnimatorDriver = BasisHelpers.GetOrAddComponent<BasisLocalAnimatorDriver>(Player.Avatar.Animator.gameObject);
            AnimatorDriver.Initialize(Player.Avatar.Animator);

            ResetAvatarAnimator();

            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl Head, BasisBoneTrackedRole.Head))
            {
                Head.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            }
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl Hips, BasisBoneTrackedRole.Hips))
            {
                Hips.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            }
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl Chest, BasisBoneTrackedRole.Chest))
            {
                Chest.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            }
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl Spine, BasisBoneTrackedRole.Spine))
            {
                Spine.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            }
            if (HasTposeEvent == false)
            {
                TposeStateChange += OnTpose;
                HasTposeEvent = true;
            }
            Player.Avatar.transform.parent = Hips.BoneTransform;
            Player.Avatar.transform.SetLocalPositionAndRotation(-Hips.TposeLocal.position, Quaternion.identity);
            BasisLocalPlayer.Instance.LocalBoneDriver.CalibrateOffsets();
            BuildBuilder();
            if (Builder.enabled == false)
            {
                Builder.enabled = true;
            }
        }
        public void BuildBuilder()
        {
            if (Builder.Build())
            {

            }
            else
            {
                Debug.LogError("Unable to Build Builder for IK!!! major issue");
            }
        }
        public void OnTpose()
        {
            if (Builder != null)
            {
                if (InTPose)
                {
                }
                else
                {
                }
            }
        }
        public void GlobalWeight()
        {

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
            GameObject HeadRig = CreateRig("Chest, Neck, Head", true, out RigHeadRig, out RigHeadLayer);
            CreateTwoBone(driver, HeadRig, References.chest, References.neck, References.head, BasisBoneTrackedRole.Head, BasisBoneTrackedRole.Neck, true, out HeadTwoBoneIK, false, true);
            if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.Head))
            {
                WriteUpEvents(Control, RigHeadLayer);
            }

            GameObject RightShoulder = CreateRig("UpperChest, RightShoulder, RightUpperArm", true, out RightShoulderRig, out RightShoulderLayer);
            CreateTwoBone(driver, RightShoulder, References.chest, References.RightShoulder, References.RightUpperArm, BasisBoneTrackedRole.RightUpperArm, BasisBoneTrackedRole.RightShoulder, true, out RightShoulderTwoBoneIK, false, true);
            if (driver.FindBone(out Control, BasisBoneTrackedRole.RightShoulder))
            {
                WriteUpEvents(Control, RightShoulderLayer);
            }

            GameObject LeftShoulder = CreateRig("UpperChest, LeftShoulder, LeftUpperArm", true, out LeftShoulderRig, out LeftShoulderLayer);
            CreateTwoBone(driver, LeftShoulder, References.chest, References.leftShoulder, References.leftUpperArm, BasisBoneTrackedRole.LeftUpperArm, BasisBoneTrackedRole.LeftShoulder, true, out LeftShoulderTwoBoneIK, false, true);
            if (driver.FindBone(out Control, BasisBoneTrackedRole.LeftShoulder))
            {
                WriteUpEvents(Control, LeftShoulderLayer);
            }

            GameObject Body = CreateRig("Spine", true, out ChestSpineRig, out ChestSpineLayer);
            CreateTwoBone(driver, Body, References.spine, null, null, BasisBoneTrackedRole.Spine, BasisBoneTrackedRole.Neck, true, out UpperChestTwoBoneIK, false, true);
            if (driver.FindBone(out Control, BasisBoneTrackedRole.Chest))
            {
                WriteUpEvents(Control, ChestSpineLayer);
            }
            LeftHand(driver);
            RightHand(driver);
            LeftFoot(driver);
            RightFoot(driver);
            LeftToe(driver);
            RightToe(driver);
        }
        public void LeftHand(BasisLocalBoneDriver driver)
        {
            GameObject Hands = CreateRig("LeftUpperArm, LeftLowerArm, LeftHand", false, out LeftHandRig, out LeftHandLayer);
            if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.LeftHand))
            {
                WriteUpEvents(Control, LeftHandLayer);
            }
            CreateTwoBone(driver, Hands, References.leftUpperArm, References.leftLowerArm, References.leftHand, BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.LeftLowerArm, true, out LeftHandTwoBoneIK, false, true);
        }
        public void RightHand(BasisLocalBoneDriver driver)
        {
            GameObject Hands = CreateRig("RightUpperArm, RightLowerArm, RightHand", false, out RightHandRig, out RightHandLayer);
            if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightHand))
            {
                WriteUpEvents(Control, RightHandLayer);
            }
            CreateTwoBone(driver, Hands, References.RightUpperArm, References.RightLowerArm, References.rightHand, BasisBoneTrackedRole.RightHand, BasisBoneTrackedRole.RightLowerArm, true, out RightHandTwoBoneIK, false, true);
        }
        public void LeftFoot(BasisLocalBoneDriver driver)
        {
            GameObject feet = CreateRig("LeftUpperLeg, LeftLowerLeg, LeftFoot", false, out LeftFootRig, out LeftFootLayer);
            if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.LeftFoot))
            {
                WriteUpEvents(Control, LeftFootLayer);
            }
            CreateTwoBone(driver, feet, References.LeftUpperLeg, References.LeftLowerLeg, References.leftFoot, BasisBoneTrackedRole.LeftFoot, BasisBoneTrackedRole.LeftLowerLeg, true, out LeftFootTwoBoneIK, false, true);
        }
        public void RightFoot(BasisLocalBoneDriver driver)
        {
            GameObject feet = CreateRig("RightUpperLeg, RightLowerLeg, RightFoot", false, out RightFootRig, out RightFootLayer);
            if (driver.FindBone(out BasisBoneControl Control, BasisBoneTrackedRole.RightFoot))
            {
                WriteUpEvents(Control, RightFootLayer);
            }
            CreateTwoBone(driver, feet, References.RightUpperLeg, References.RightLowerLeg, References.rightFoot, BasisBoneTrackedRole.RightFoot, BasisBoneTrackedRole.RightLowerLeg, true, out RightFootTwoBoneIK, false, true);
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
                ApplyHint(role, 1);
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
        /// <summary>
        /// this gets cleared on a calibration
        /// </summary>
        /// <param name="Control"></param>
        /// <param name="Layer"></param>
        public void WriteUpEvents(BasisBoneControl Control, RigLayer Layer)
        {
            Control.OnHasRigChanged.AddListener(delegate { UpdateLayerActiveState(Control, Layer); });
            Control.HasEvents = true;
            // Set the initial state
            UpdateLayerActiveState(Control, Layer);
        }
        // Define a method to update the active state of the Layer
        void UpdateLayerActiveState(BasisBoneControl Control, RigLayer Layer)
        {
            // Debug.Log("setting Layer State to " + Control.HasRigLayer == BasisHasRigLayer.HasRigLayer + " for " + Control.Name);
            Layer.active = Control.HasRigLayer == BasisHasRigLayer.HasRigLayer;
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
}