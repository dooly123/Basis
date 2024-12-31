using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Common;
using Basis.Scripts.Device_Management;
using Basis.Scripts.TransformBinders.BoneControl;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Animations.Rigging;

namespace Basis.Scripts.Drivers
{
    public abstract class BasisAvatarDriver : MonoBehaviour
    {
        public float ActiveAvatarEyeHeight()
        {
            if (BasisLocalPlayer.Instance.BasisAvatar != null)
            {
                return BasisLocalPlayer.Instance.BasisAvatar.AvatarEyePosition.x;
            }
            else
            {
                return 1.64f;
            }
        }
        private static string TPose = "Assets/Animator/Animated TPose.controller";
        public static string BoneData = "Assets/ScriptableObjects/BoneData.asset";
        public Action CalibrationComplete;
        public Action TposeStateChange;
        public BasisTransformMapping References = new BasisTransformMapping();
        public RuntimeAnimatorController SavedruntimeAnimatorController;
        public SkinnedMeshRenderer[] SkinnedMeshRenderer;
        public BasisPlayer Player;
        public bool CurrentlyTposing = false;
        public bool HasEvents = false;
        public void Calibration(BasisAvatar Avatar)
        {
            FindSkinnedMeshRenders();
            BasisTransformMapping.AutoDetectReferences(Player.BasisAvatar.Animator, Avatar.transform, out References);
            Player.FaceisVisible = false;
            if (Avatar == null)
            {
                BasisDebug.LogError("Missing Avatar");
            }
            if (Avatar.FaceVisemeMesh == null)
            {
                BasisDebug.Log("Missing Face for " + Player.DisplayName, BasisDebug.LogTag.Avatar);
            }
            Player.UpdateFaceVisibility(Avatar.FaceVisemeMesh.isVisible);
            if (Player.FaceRenderer != null)
            {
                GameObject.Destroy(Player.FaceRenderer);
            }
            Player.FaceRenderer = BasisHelpers.GetOrAddComponent<BasisMeshRendererCheck>(Avatar.FaceVisemeMesh.gameObject);
            Player.FaceRenderer.Check += Player.UpdateFaceVisibility;

            if (BasisFacialBlinkDriver.MeetsRequirements(Avatar))
            {
                BasisFacialBlinkDriver FacialBlinkDriver = BasisHelpers.GetOrAddComponent<BasisFacialBlinkDriver>(Avatar.gameObject);
                FacialBlinkDriver.Initialize(Player,Avatar);
            }
        }
        public void PutAvatarIntoTPose()
        {
            BasisDebug.Log("PutAvatarIntoTPose", BasisDebug.LogTag.Avatar);
            CurrentlyTposing = true;
            if (SavedruntimeAnimatorController == null)
            {
                SavedruntimeAnimatorController = Player.BasisAvatar.Animator.runtimeAnimatorController;
            }
            UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<RuntimeAnimatorController> op = Addressables.LoadAssetAsync<RuntimeAnimatorController>(TPose);
            RuntimeAnimatorController RAC = op.WaitForCompletion();
            Player.BasisAvatar.Animator.runtimeAnimatorController = RAC;
            ForceUpdateAnimator(Player.BasisAvatar.Animator);
            BasisDeviceManagement.UnassignFBTrackers();
            TposeStateChange?.Invoke();
        }
        public void ResetAvatarAnimator()
        {
            BasisDebug.Log("ResetAvatarAnimator", BasisDebug.LogTag.Avatar);
            Player.BasisAvatar.Animator.runtimeAnimatorController = SavedruntimeAnimatorController;
            SavedruntimeAnimatorController = null;
            CurrentlyTposing = false;
            TposeStateChange?.Invoke();
        }
        public Bounds GetBounds(Transform animatorParent)
        {
            // Get all renderers in the parent GameObject
            Renderer[] renderers = animatorParent.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.zero, new Vector3(0.3f, 1.7f, 0.3f));
            }
            Bounds bounds = renderers[0].bounds;
            for (int Index = 1; Index < renderers.Length; Index++)
            {
                bounds.Encapsulate(renderers[Index].bounds);
            }
            return bounds;
        }
        public static bool TryConvertToBoneTrackingRole(HumanBodyBones body, out BasisBoneTrackedRole result)
        {
            result = BasisBoneTrackedRole.Chest; // Set a default value or handle it based on your requirements

            if (Enum.TryParse(body.ToString(), out BasisBoneTrackedRole parsedRole))
            {
                result = parsedRole;
                return true; // Successfully parsed
            }

            return false; // Failed to parse
        }
        public static bool TryConvertToHumanoidRole(BasisBoneTrackedRole body, out HumanBodyBones result)
        {
            result = HumanBodyBones.Hips; // Set a default value or handle it based on your requirements

            if (Enum.TryParse(body.ToString(), out HumanBodyBones parsedRole))
            {
                result = parsedRole;
                return true; // Successfully parsed
            }

            return false; // Failed to parse
        }
        public static bool IsApartOfSpineVertical(BasisBoneTrackedRole Role)
        {
            if (Role == BasisBoneTrackedRole.Hips ||
                Role == BasisBoneTrackedRole.Chest ||
                Role == BasisBoneTrackedRole.Hips ||
                Role == BasisBoneTrackedRole.Spine ||
                Role == BasisBoneTrackedRole.CenterEye ||
                Role == BasisBoneTrackedRole.Mouth ||
                Role == BasisBoneTrackedRole.Head)
            {
                return true;
            }
            return false;
        }
        public void CalculateTransformPositions(Animator anim, BaseBoneDriver driver)
        {
            BasisDebug.Log("CalculateTransformPositions", BasisDebug.LogTag.Avatar);
            UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<BasisFallBackBoneData> BasisFallBackBoneDataAsync = Addressables.LoadAssetAsync<BasisFallBackBoneData>(BoneData);
            BasisFallBackBoneData FBBD = BasisFallBackBoneDataAsync.WaitForCompletion();
            for (int Index = 0; Index < driver.ControlsLength; Index++)
            {
                BasisBoneControl Control = driver.Controls[Index];
                if (driver.trackedRoles[Index] == BasisBoneTrackedRole.CenterEye)
                {
                    GetWorldSpaceRotAndPos(() => Player.BasisAvatar.AvatarEyePosition, out quaternion Rotation, out float3 TposeWorld);
                    SetInitialData(anim, Control, driver.trackedRoles[Index], TposeWorld);
                }
                else
                {
                    if (driver.trackedRoles[Index] == BasisBoneTrackedRole.Mouth)
                    {
                        GetWorldSpaceRotAndPos(() => Player.BasisAvatar.AvatarMouthPosition, out quaternion Rotation, out float3 TposeWorld);
                        SetInitialData(anim, Control, driver.trackedRoles[Index], TposeWorld);
                    }
                    else
                    {
                        if (FBBD.FindBone(out BasisFallBone FallBackBone, driver.trackedRoles[Index]))
                        {
                            if (TryConvertToHumanoidRole(driver.trackedRoles[Index], out HumanBodyBones HumanBones))
                            {
                                GetBoneRotAndPos(driver, anim, HumanBones, FallBackBone.PositionPercentage, out quaternion Rotation, out float3 TposeWorld, out bool UsedFallback);
                                SetInitialData(anim, Control, driver.trackedRoles[Index], TposeWorld);
                            }
                            else
                            {
                                BasisDebug.LogError("cant Convert to humanbodybone " + driver.trackedRoles[Index]);
                            }
                        }
                        else
                        {
                            BasisDebug.LogError("cant find Fallback Bone for " + driver.trackedRoles[Index]);
                        }
                    }
                }
            }
            Addressables.Release(BasisFallBackBoneDataAsync);
        }
        public void GetBoneRotAndPos(BaseBoneDriver driver, Animator anim, HumanBodyBones bone, Vector3 heightPercentage, out quaternion Rotation, out float3 Position, out bool UsedFallback)
        {
            if (anim.avatar != null && anim.avatar.isHuman)
            {
                Transform boneTransform = anim.GetBoneTransform(bone);
                if (boneTransform == null)
                {
                    Rotation = driver.transform.rotation;
                    if (BasisHelpers.TryGetFloor(anim, out Position))
                    {

                    }
                    // Position = new Vector3(0, Position.y, 0);
                    Position += CalculateFallbackOffset(bone, ActiveAvatarEyeHeight(), heightPercentage);
                    //Position = new Vector3(0, Position.y, 0);
                    UsedFallback = true;
                }
                else
                {
                    UsedFallback = false;
                    boneTransform.GetPositionAndRotation(out Vector3 VPosition, out Quaternion QRotation);
                    Position = VPosition;
                    Rotation = QRotation;
                }
            }
            else
            {
                Rotation = driver.transform.rotation;
                if (BasisHelpers.TryGetFloor(anim, out Position))
                {

                }
                Position = new Vector3(0, Position.y, 0);
                Position += CalculateFallbackOffset(bone, ActiveAvatarEyeHeight(), heightPercentage);
                Position = new Vector3(0, Position.y, 0);
                UsedFallback = true;
            }
        }
        public float3 CalculateFallbackOffset(HumanBodyBones bone, float fallbackHeight, float3 heightPercentage)
        {
            Vector3 height = fallbackHeight * heightPercentage;
            return bone == HumanBodyBones.Hips ? Multiply(height, -Vector3.up) : Multiply(height, Vector3.up);
        }
        public static Vector3 Multiply(Vector3 value, Vector3 scale)
        {
            return new Vector3(value.x * scale.x, value.y * scale.y, value.z * scale.z);
        }
        public void GetWorldSpaceRotAndPos(Func<Vector2> positionSelector, out quaternion rotation, out float3 position)
        {
            rotation = Quaternion.identity;
            position = Vector3.zero;
            if (BasisHelpers.TryGetFloor(Player.BasisAvatar.Animator, out float3 bottom))
            {
                Vector3 convertedToVector3 = BasisHelpers.AvatarPositionConversion(positionSelector());
                position = BasisHelpers.ConvertFromLocalSpace(convertedToVector3, bottom);
            }
            else
            {
                BasisDebug.LogError("Missing bottom");
            }
        }
        public void ForceUpdateAnimator(Animator Anim)
        {
            // Specify the time you want the Animator to update to (in seconds)
            float desiredTime = Time.deltaTime;

            // Call the Update method to force the Animator to update to the desired time
            Anim.Update(desiredTime);
        }
        public GameObject CreateAndSetParent(Transform parent, string name)
        {
            // Create a new empty GameObject
            GameObject newObject = new GameObject(name);

            // Set its parent
            newObject.transform.SetParent(parent);
            return newObject;
        }
        public bool IsNull(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                BasisDebug.LogError("Missing Object during calibration");
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SetInitialData(Animator animator, BasisBoneControl bone, BasisBoneTrackedRole Role,Vector3 WorldTpose)
        {
            bone.OutGoingData.position = BasisLocalBoneDriver.ConvertToAvatarSpaceInital(animator, WorldTpose, 0.1f * animator.transform.localScale.y);//out Vector3 WorldSpaceFloor
            bone.TposeLocal.position = bone.OutGoingData.position;
            bone.TposeLocal.rotation = bone.OutGoingData.rotation;
            if (IsApartOfSpineVertical(Role))
            {
                bone.OutGoingData.position = new Vector3(0, bone.OutGoingData.position.y, bone.OutGoingData.position.z);
                bone.TposeLocal.position = bone.OutGoingData.position;
            }
            if (Role == BasisBoneTrackedRole.Hips)
            {
                bone.TposeLocal.rotation = quaternion.identity;
            }
        }
        public void SetAndCreateLock(BaseBoneDriver BaseBoneDriver, BasisBoneTrackedRole LockToBoneRole, BasisBoneTrackedRole AssignedTo, float PositionLerpAmount, float QuaternionLerpAmount, bool CreateLocks = true)
        {
            if (CreateLocks)
            {

                if (BaseBoneDriver.FindBone(out BasisBoneControl AssignedToAddToBone, AssignedTo) == false)
                {
                    BasisDebug.LogError("Cant Find Bone " + AssignedTo);
                }
                if (BaseBoneDriver.FindBone(out BasisBoneControl LockToBone, LockToBoneRole) == false)
                {
                    BasisDebug.LogError("Cant Find Bone " + LockToBoneRole);
                }
                BaseBoneDriver.CreateRotationalLock(AssignedToAddToBone, LockToBone, PositionLerpAmount, QuaternionLerpAmount);
            }
        }
        public void FindSkinnedMeshRenders()
        {
            SkinnedMeshRenderer = Player.BasisAvatar.Animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }
        public void SetAllMatrixRecalculation(bool State)
        {
            foreach (SkinnedMeshRenderer Render in SkinnedMeshRenderer)
            {
                Render.forceMatrixRecalculationPerRender = State;
            }
        }
        public void SetHeadMatrixRecalculation(bool State)
        {
            if (Player.BasisAvatar.FaceBlinkMesh != null)
            {
                Player.BasisAvatar.FaceBlinkMesh.forceMatrixRecalculationPerRender = State;
            }
            if (Player.BasisAvatar.FaceVisemeMesh != null)
            {
                Player.BasisAvatar.FaceVisemeMesh.forceMatrixRecalculationPerRender = State;
            }
        }
        public void updateWhenOffscreen(bool State)
        {
            for (int Index = 0; Index < SkinnedMeshRenderer.Length; Index++)
            {
                SkinnedMeshRenderer Render = SkinnedMeshRenderer[Index];
                Render.updateWhenOffscreen = State;
            }
        }
        public void EnableTwoBoneIk(TwoBoneIKConstraint Constraint, bool maintainTargetPositionOffset = false, bool maintainTargetRotationOffset = false)
        {
            Constraint.data.targetPositionWeight = 1;
            Constraint.data.targetRotationWeight = 1;
            Constraint.data.maintainTargetPositionOffset = maintainTargetPositionOffset;
            Constraint.data.maintainTargetRotationOffset = maintainTargetRotationOffset;
        }
        public void Damp(BaseBoneDriver driver, GameObject Parent, Transform Source, BasisBoneTrackedRole Role, float rotationWeight = 1, float positionWeight = 1)
        {
            driver.FindBone(out BasisBoneControl Target, Role);
            GameObject DTData = CreateAndSetParent(Parent.transform, "Bone Role " + Role.ToString());
            DampedTransform DT = BasisHelpers.GetOrAddComponent<DampedTransform>(DTData);

            DT.data.constrainedObject = Source;
            DT.data.sourceObject = Target.BoneTransform;
            DT.data.dampRotation = rotationWeight;
            DT.data.dampPosition = positionWeight;
            DT.data.maintainAim = false;
            GeneratedRequiredTransforms(Source, References.Hips);
            WriteUpWeights(Target, DT);
        }
        public void MultiRotation(GameObject Parent, Transform Source, Transform Target, float rotationWeight = 1)
        {
            GameObject DTData = CreateAndSetParent(Parent.transform, "Eye Target");
            MultiAimConstraint DT = BasisHelpers.GetOrAddComponent<MultiAimConstraint>(DTData);
            DT.data.constrainedObject = Source;
            WeightedTransformArray Array = new WeightedTransformArray(0);
            WeightedTransform Weighted = new WeightedTransform(Target, rotationWeight);
            Array.Add(Weighted);
            DT.data.sourceObjects = Array;
            DT.data.maintainOffset = false;
            DT.data.aimAxis = MultiAimConstraintData.Axis.Z;
            DT.data.upAxis = MultiAimConstraintData.Axis.Y;
            DT.data.limits = new Vector2(-180, 180);
            DT.data.constrainedXAxis = true;
            DT.data.constrainedYAxis = true;
            DT.data.constrainedZAxis = true;

            GeneratedRequiredTransforms(Source, References.Hips);
        }
        public void MultiRotation(BaseBoneDriver driver, GameObject Parent, Transform Source, BasisBoneTrackedRole Role, float rotationWeight = 1)
        {
            driver.FindBone(out BasisBoneControl Target, Role);
            GameObject DTData = CreateAndSetParent(Parent.transform, "Bone Role " + Role.ToString());
            MultiAimConstraint DT = BasisHelpers.GetOrAddComponent<MultiAimConstraint>(DTData);
            DT.data.constrainedObject = Source;
            WeightedTransformArray Array = new WeightedTransformArray(0);
            WeightedTransform Weighted = new WeightedTransform(Target.BoneTransform, rotationWeight);
            Array.Add(Weighted);
            DT.data.sourceObjects = Array;
            DT.data.maintainOffset = false;
            DT.data.aimAxis = MultiAimConstraintData.Axis.Z;
            DT.data.upAxis = MultiAimConstraintData.Axis.Y;
            DT.data.limits = new Vector2(-180, 180);
            DT.data.constrainedXAxis = true;
            DT.data.constrainedYAxis = true;
            DT.data.constrainedZAxis = true;

            GeneratedRequiredTransforms(Source, References.Hips);
        }
        public void MultiPositional(BaseBoneDriver driver, GameObject Parent, Transform Source, BasisBoneTrackedRole Role, float positionWeight = 1)
        {
            driver.FindBone(out BasisBoneControl Target, Role);
            GameObject DTData = CreateAndSetParent(Parent.transform, "Bone Role " + Role.ToString());
            MultiPositionConstraint DT = BasisHelpers.GetOrAddComponent<MultiPositionConstraint>(DTData);
            DT.data.constrainedObject = Source;
            WeightedTransformArray Array = new WeightedTransformArray(0);
            WeightedTransform Weighted = new WeightedTransform(Target.BoneTransform, positionWeight);
            Array.Add(Weighted);
            DT.data.sourceObjects = Array;
            DT.data.maintainOffset = false;
            DT.data.constrainedXAxis = true;
            DT.data.constrainedYAxis = true;
            DT.data.constrainedZAxis = true;

            GeneratedRequiredTransforms(Source, References.Hips);
        }
        public void OverrideTransform(BaseBoneDriver driver, GameObject Parent, Transform Source, BasisBoneTrackedRole Role, float rotationWeight = 1, float positionWeight = 1, OverrideTransformData.Space Space = OverrideTransformData.Space.World)
        {
            driver.FindBone(out BasisBoneControl Target, Role);
            GameObject DTData = CreateAndSetParent(Parent.transform, "Bone Role " + Role.ToString());
            OverrideTransform DT = BasisHelpers.GetOrAddComponent<OverrideTransform>(DTData);
            DT.data.constrainedObject = Source;
            DT.data.sourceObject = Target.BoneTransform;
            DT.data.rotationWeight = rotationWeight;
            DT.data.positionWeight = positionWeight;
            DT.data.space = Space;
            GeneratedRequiredTransforms(Source, References.Hips);
        }
        public void TwistChain(BaseBoneDriver driver, GameObject Parent, Transform Source, BasisBoneTrackedRole Role, float rotationWeight = 1, float positionWeight = 1, OverrideTransformData.Space Space = OverrideTransformData.Space.World)
        {
            driver.FindBone(out BasisBoneControl Target, Role);
            GameObject DTData = CreateAndSetParent(Parent.transform, "Bone Role " + Role.ToString());
            TwistChainConstraint DT = BasisHelpers.GetOrAddComponent<TwistChainConstraint>(DTData);
            DT.data.root = Source;
            DT.data.tip = Source;
            // DT.data.curve = new AnimationCurve(new Keyframe[2] {);
            DT.data.tipTarget = Target.BoneTransform;
            GeneratedRequiredTransforms(Source, References.Hips);
        }
        public void CreateTwoBone(BaseBoneDriver driver, GameObject Parent, Transform root, Transform mid, Transform tip, BasisBoneTrackedRole TargetRole, BasisBoneTrackedRole BendRole, bool UseBoneRole, out TwoBoneIKConstraint TwoBoneIKConstraint, bool maintainTargetPositionOffset, bool maintainTargetRotationOffset)
        {
            driver.FindBone(out BasisBoneControl BoneControl, TargetRole);
            GameObject BoneRole = CreateAndSetParent(Parent.transform, "Bone Role " + TargetRole.ToString());
            TwoBoneIKConstraint = BasisHelpers.GetOrAddComponent<TwoBoneIKConstraint>(BoneRole);
            EnableTwoBoneIk(TwoBoneIKConstraint, maintainTargetPositionOffset, maintainTargetRotationOffset);
            TwoBoneIKConstraint.data.target = BoneControl.BoneTransform;
            if (UseBoneRole)
            {
                if (driver.FindBone(out BasisBoneControl BendBoneControl, BendRole))
                {
                    TwoBoneIKConstraint.data.hint = BendBoneControl.BoneTransform;
                }
            }
            TwoBoneIKConstraint.data.root = root;
            TwoBoneIKConstraint.data.mid = mid;
            TwoBoneIKConstraint.data.tip = tip;
            GeneratedRequiredTransforms(tip, References.Hips);
        }
        public void WriteUpWeights(BasisBoneControl Control, DampedTransform Constraint)
        {
            Control.WeightsChanged.AddListener(delegate (float positionWeight, float rotationWeight)
            {
                UpdateIKRig(positionWeight, rotationWeight, Constraint);
            });
        }

        void UpdateIKRig(float PositionWeight, float RotationWeight, DampedTransform Constraint)
        {
            // Constraint.weight = PositionWeight;
        }
        public void GeneratedRequiredTransforms(Transform BaseLevel, Transform TopLevelParent)
        {
            BasisLocalAvatarDriver Driver = (BasisLocalAvatarDriver)this;
            // Go up the hierarchy until you hit the TopLevelParent
            if (BaseLevel != null)
            {
                Transform currentTransform = BaseLevel.parent;
                while (currentTransform != null && currentTransform != TopLevelParent)
                {
                    // Add component if the current transform doesn't have it
                    if (currentTransform.TryGetComponent<RigTransform>(out RigTransform RigTransform))
                    {
                        if (Driver.AdditionalTransforms.Contains(RigTransform) == false)
                        {
                            Driver.AdditionalTransforms.Add(RigTransform);
                        }
                    }
                    else
                    {
                        RigTransform = currentTransform.gameObject.AddComponent<RigTransform>();
                        Driver.AdditionalTransforms.Add(RigTransform);
                    }
                    // Move to the parent for the next iteration
                    currentTransform = currentTransform.parent;
                }
            }
        }
    }
}
