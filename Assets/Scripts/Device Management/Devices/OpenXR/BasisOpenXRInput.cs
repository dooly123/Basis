using UnityEngine;
using UnityEngine.XR;
[DefaultExecutionOrder(15101)]
public class BasisOpenXRInput : BasisInput
{
    public UnityEngine.XR.InputDevice Device;


    public void Initialize(UnityEngine.XR.InputDevice device, string UniqueID,string UnUniqueID,string subSystems)
    {
        Device = device;
        GetControllerOrHMD();
        ActivateTracking(UniqueID, UnUniqueID, subSystems);
    }

    private void GetControllerOrHMD()
    {
      if (Device.characteristics == Characteristics.hmd)
        {
            TrackedRole = BasisBoneTrackedRole.CenterEye;
        }
        else if (Device.characteristics == Characteristics.leftController || Device.characteristics == Characteristics.leftTrackedHand)
        {
            TrackedRole = BasisBoneTrackedRole.LeftHand;
        }
        else if (Device.characteristics == Characteristics.rightController || Device.characteristics == Characteristics.rightTrackedHand)
        {
            TrackedRole = BasisBoneTrackedRole.RightHand;
        }
    }

    public override void PollData()
    {
        if (Device.isValid)
        {
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out LocalRawRotation))
            {
                FinalRotation = LocalRawRotation;
                if (hasRoleAssigned)
                {
                    if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker)
                    {
                        Control.TrackerData.rotation = FinalRotation * AvatarRotationOffset;
                    }
                }
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out LocalRawPosition))
            {
                FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;
                if (hasRoleAssigned)
                {
                    if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker)
                    {
                        Control.TrackerData.position = LocalRawPosition - LocalRawRotation * AvatarPositionOffset;
                    }
                }
            }
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

            // Other feature value checks...

            UpdatePlayerControl();
        }
        transform.SetLocalPositionAndRotation(LocalRawPosition, LocalRawRotation);
    }
    public static class Characteristics
    {
        /// <summary>
        /// HMD characteristics.
        /// <see cref="InputDeviceCharacteristics.HeadMounted"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/>
        /// </summary>
        public static InputDeviceCharacteristics hmd => InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.TrackedDevice;

        /// <summary>
        /// Eye gaze characteristics.
        /// <see cref="InputDeviceCharacteristics.HeadMounted"/> <c>|</c> <see cref="InputDeviceCharacteristics.EyeTracking"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/>
        /// </summary>
        public static InputDeviceCharacteristics eyeGaze => InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice;

        /// <summary>
        /// Left controller characteristics.
        /// <see cref="InputDeviceCharacteristics.HeldInHand"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Controller"/> <c>|</c> <see cref="InputDeviceCharacteristics.Left"/>
        /// </summary>
        public static InputDeviceCharacteristics leftController => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

        /// <summary>
        /// Right controller characteristics.
        /// <see cref="InputDeviceCharacteristics.HeldInHand"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Controller"/> <c>|</c> <see cref="InputDeviceCharacteristics.Right"/>
        /// </summary>
        public static InputDeviceCharacteristics rightController => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;

        /// <summary>
        /// Left tracked hand characteristics.
        /// <see cref="InputDeviceCharacteristics.HandTracking"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Left"/>
        /// </summary>
        public static InputDeviceCharacteristics leftTrackedHand => InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left;

        /// <summary>
        /// Right tracked hand characteristics.
        /// <see cref="InputDeviceCharacteristics.HandTracking"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Right"/>
        /// </summary>
        public static InputDeviceCharacteristics rightTrackedHand => InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right;
    }
}