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
                if (hasRoleAssigned)
                {
                    if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawRotation != Quaternion.identity)
                    {
                        Control.TrackerData.rotation = LocalRawRotation * rotationOffset;
                    }
                }
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out LocalRawPosition))
            {
                if (hasRoleAssigned)
                {
                    if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawPosition != Vector3.zero)
                    {
                        Vector3 LocalPivot = LocalRawRotation * pivotOffset;
                        Control.TrackerData.position = LocalRawPosition - LocalPivot;
                    }
                }
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out State.primary2DAxis))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondary2DAxis, out State.secondary2DAxis))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out State.gripButton))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out State.menuButton))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out State.primaryButtonGetState))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out State.secondaryButtonGetState))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out State.Trigger))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondary2DAxisClick, out State.secondary2DAxisClick))
            {
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out State.primary2DAxisClick))
            {
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