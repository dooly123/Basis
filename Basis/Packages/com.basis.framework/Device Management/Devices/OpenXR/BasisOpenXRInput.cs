using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
using Unity.Mathematics;
using UnityEngine;
using static BasisBaseMuscleDriver;

namespace Basis.Scripts.Device_Management.Devices.OpenXR
{
    [DefaultExecutionOrder(15001)]
    public class BasisOpenXRInput : BasisInput
    {
        public UnityEngine.XR.InputDevice Device;
        public FingerPose FingerCurls;
        public BasisOpenXRInputEye BasisOpenXRInputEye;
        public BasisVirtualSpineDriver BasisVirtualSpine = new BasisVirtualSpineDriver(); 
        public void Initialize(UnityEngine.XR.InputDevice device, string UniqueID, string UnUniqueID, string subSystems, bool AssignTrackedRole, BasisBoneTrackedRole basisBoneTrackedRole)
        {
            Device = device;
            InitalizeTracking(UniqueID, UnUniqueID, subSystems, AssignTrackedRole, basisBoneTrackedRole);
            if (basisBoneTrackedRole == BasisBoneTrackedRole.CenterEye)
            {
                BasisOpenXRInputEye = this.gameObject.AddComponent<BasisOpenXRInputEye>();
                BasisOpenXRInputEye.Initalize();
                BasisVirtualSpine.Initialize();
            }
        }
        public new void OnDestroy()
        {
            BasisVirtualSpine.DeInitialize();
            base.OnDestroy();
        }
        public override void DoPollData()
        {
            if (Device.isValid)
            {
                // Rotation and Position
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion Rotation))
                {
                    LocalRawRotation = Rotation;
                    if (hasRoleAssigned && Control.HasTracked != BasisHasTracked.HasNoTracker)
                    {
                        Control.IncomingData.rotation = math.mul(LocalRawRotation, Quaternion.Euler(AvatarRotationOffset));
                    }
                }

                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 Position))
                {
                    LocalRawPosition = Position;
                    if (hasRoleAssigned && Control.HasTracked != BasisHasTracked.HasNoTracker)
                    {
                        // Apply the inverse rotation to position offset
                        Control.IncomingData.position = LocalRawPosition - math.mul(LocalRawRotation, AvatarPositionOffset * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale);
                    }
                }

                FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale;
                FinalRotation = LocalRawRotation;

                // Input States
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 Primary2DAxis))
                {
                    InputState.Primary2DAxis = Primary2DAxis;
                }
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondary2DAxis, out Vector2 secondary2DAxis))
                {
                    InputState.Secondary2DAxis = secondary2DAxis;
                }
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool gripButton))
                {
                    InputState.GripButton = gripButton;
                }
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool menuButton))
                {
                    InputState.MenuButton = menuButton;
                }
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryButtonGetState))
                {
                    InputState.PrimaryButtonGetState = primaryButtonGetState;
                }
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryButtonGetState))
                {
                    InputState.SecondaryButtonGetState = secondaryButtonGetState;
                }
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float Trigger))
                {
                    InputState.Trigger = Trigger;
                }
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondary2DAxisClick, out bool secondary2DAxisClick))
                {
                    InputState.Secondary2DAxisClick = secondary2DAxisClick;
                }
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool primary2DAxisClick))
                {
                    InputState.Primary2DAxisClick = primary2DAxisClick;
                }

                // Calculate Finger Curls
                CalculateFingerCurls();

                // Apply Animations
                if (TryGetRole(out BasisBoneTrackedRole BBTR))
                {
                    switch (BBTR)
                    {
                        case BasisBoneTrackedRole.LeftHand:
                            BasisLocalPlayer.Instance.AvatarDriver.BasisMuscleDriver.LeftFinger = FingerCurls;
                            break;
                        case BasisBoneTrackedRole.RightHand:
                            BasisLocalPlayer.Instance.AvatarDriver.BasisMuscleDriver.RightFinger = FingerCurls;
                            break;
                        case BasisBoneTrackedRole.CenterEye:
                            BasisOpenXRInputEye = this.gameObject.AddComponent<BasisOpenXRInputEye>();
                            BasisOpenXRInputEye.Simulate();
                            break;
                    }
                }

                UpdatePlayerControl();
            }
        }

        private void CalculateFingerCurls()
        {
            FingerCurls.ThumbPercentage = new Vector2(InputState.GripButton ? -1f : 0.7f, 0);//thumb
            FingerCurls.IndexPercentage = new Vector2(BasisBaseMuscleDriver.MapValue(InputState.Trigger, 0, 1, -1f, 0.7f), 0);// Index finger curl
            FingerCurls.MiddlePercentage = new Vector2(InputState.PrimaryButtonGetState ? -1f : 0.7f, 0);// Middle finger curl
            FingerCurls.RingPercentage = new Vector2(InputState.SecondaryButtonGetState ? -1f : 0.7f, 0); // Ring finger curl
            FingerCurls.LittlePercentage = new Vector2(InputState.MenuButton ? 1 - 1f : 0.7f, 0); // Pinky finger curl
        }
    }
}