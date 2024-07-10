using UnityEngine;
using UnityEngine.XR;
[DefaultExecutionOrder(15101)]
public class BasisOpenXRInput : BasisInput
{
    public UnityEngine.XR.InputDevice Device;


    public void Initialize(UnityEngine.XR.InputDevice device, string UniqueID,string UnUniqueID,string subSystems, bool AssignTrackedRole, BasisBoneTrackedRole basisBoneTrackedRole)
    {
        Device = device;
        InitalizeTracking(UniqueID, UnUniqueID, subSystems, AssignTrackedRole, basisBoneTrackedRole);
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
                    if (Control.HasTracked != BasisHasTracked.HasNoTracker)
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
                    if (Control.HasTracked != BasisHasTracked.HasNoTracker)
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
}