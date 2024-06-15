using UnityEngine;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputTrackingAggregator;

[DefaultExecutionOrder(15101)]
public class BasisOpenXRInput : BasisInput
{
    public UnityEngine.XR.InputDevice Device;


    public void Initialize(UnityEngine.XR.InputDevice device, string UniqueID,string UnUniqueID)
    {
        Device = device;
        GetControllerOrHMD();
        ActivateTracking(UniqueID, UnUniqueID);
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
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out LocalRawPosition))
            {
                if (hasRoleAssigned)
                {
                    if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawPosition != Vector3.zero)
                    {
                        Control.LocalRawPosition = LocalRawPosition;
                    }
                }
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out LocalRawRotation))
            {
                if (hasRoleAssigned)
                {
                    if (Control.HasTrackerPositionDriver != BasisHasTracked.HasNoTracker && LocalRawRotation != Quaternion.identity)
                    {
                        Control.LocalRawRotation = LocalRawRotation;
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
}