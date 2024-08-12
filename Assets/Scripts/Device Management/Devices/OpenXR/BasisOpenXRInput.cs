using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
using UnityEngine;

namespace Basis.Scripts.Device_Management.Devices.OpenXR
{
    [DefaultExecutionOrder(15001)]
    public class BasisOpenXRInput : BasisInput
    {
        public UnityEngine.XR.InputDevice Device;
        public float[] FingerCurls;
        public void Initialize(UnityEngine.XR.InputDevice device, string UniqueID, string UnUniqueID, string subSystems, bool AssignTrackedRole, BasisBoneTrackedRole basisBoneTrackedRole)
        {
            Device = device;
            InitalizeTracking(UniqueID, UnUniqueID, subSystems, AssignTrackedRole, basisBoneTrackedRole);
            FingerCurls = new float[5]; // 0: thumb, 1: index, 2: middle, 3: ring, 4: pinky
        }
        public override void PollData()
        {
            if (Device.isValid)
            {
                // Rotation and Position
                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out LocalRawRotation))
                {
                    if (hasRoleAssigned && Control.HasTracked != BasisHasTracked.HasNoTracker)
                    {
                        Control.IncomingData.rotation = FinalRotation * AvatarRotationOffset;
                    }
                }

                if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out LocalRawPosition))
                {
                    if (hasRoleAssigned && Control.HasTracked != BasisHasTracked.HasNoTracker)
                    {
                        Control.IncomingData.position = LocalRawPosition - LocalRawRotation * AvatarPositionOffset;
                    }
                }

                FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;
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
                            BasisLocalPlayer.Instance.AvatarDriver.AnimatorDriver.ApplyLeftHandAnims(FingerCurls);
                            break;
                        case BasisBoneTrackedRole.RightHand:
                            BasisLocalPlayer.Instance.AvatarDriver.AnimatorDriver.ApplyRightHandAnims(FingerCurls);
                            break;
                    }
                }

                UpdatePlayerControl();
            }
        }

        private void CalculateFingerCurls()
        {
            FingerCurls[0] = InputState.GripButton ? 1.0f : 0.0f;
            FingerCurls[1] = InputState.Trigger; // Index finger curl
            FingerCurls[2] = InputState.PrimaryButtonGetState ? 1.0f : 0.0f; // Middle finger curl
            FingerCurls[3] = InputState.SecondaryButtonGetState ? 1.0f : 0.0f; // Ring finger curl
            FingerCurls[4] = InputState.MenuButton ? 1.0f : 0.0f; // Pinky finger curl
        }
    }
}