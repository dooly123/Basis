using UnityEngine;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputTrackingAggregator;

[DefaultExecutionOrder(15101)]
public class BasisOpenXRInput : BasisInput
{
    public UnityEngine.XR.InputDevice Device;

    public void Initialize(UnityEngine.XR.InputDevice device, string iD)
    {
        base.Initialize(iD);
        Device = device;
        DetermineDeviceType();
    }

    private void DetermineDeviceType()
    {
        if (Device.characteristics == Characteristics.hmd)
        {
            Type = BasisBoneTrackedRole.CenterEye;
        }
        else if (Device.characteristics == Characteristics.leftController || Device.characteristics == Characteristics.leftTrackedHand)
        {
            Type = BasisBoneTrackedRole.LeftHand;
        }
        else if (Device.characteristics == Characteristics.rightController || Device.characteristics == Characteristics.rightTrackedHand)
        {
            Type = BasisBoneTrackedRole.RightHand;
        }
    }

    public override void PollData()
    {
        if (Device.isValid)
        {
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out LocalRawPosition))
            {
                if (Control.HasTrackerPositionDriver != BasisBoneControl.BasisHasTracked.HasNoTracker && LocalRawPosition != Vector3.zero)
                {
                    Control.LocalRawPosition = LocalRawPosition;
                }
            }
            if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out LocalRawRotation))
            {
                if (Control.HasTrackerPositionDriver != BasisBoneControl.BasisHasTracked.HasNoTracker && LocalRawRotation != Quaternion.identity)
                {
                    Control.LocalRawRotation = LocalRawRotation;
                }
            }
            // Other feature value checks...

            UpdatePlayerControl();
        }
        transform.SetLocalPositionAndRotation(LocalRawPosition, LocalRawRotation);
    }

    private void UpdatePlayerControl()
    {
        if (Type == BasisBoneTrackedRole.LeftHand)
        {
            BasisLocalPlayer.Instance.Move.MovementVector = primary2DAxis;
            if (primaryButton)
            {
                BasisLocalPlayer.Instance.Move.HandleJump();
            }
            if (secondaryButton)
            {
                if (BasisHamburgerMenu.Instance == null && !BasisHamburgerMenu.IsLoading)
                {
                    BasisHamburgerMenu.OpenMenu();
                }
                else
                {
                    BasisHamburgerMenu.Instance.CloseThisMenu();
                }
            }
        }
        else if (Type == BasisBoneTrackedRole.RightHand)
        {
            BasisLocalPlayer.Instance.Move.Rotation = primary2DAxis;
        }
    }
}