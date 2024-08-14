using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management.Devices.OpenVR.Structs;
using UnityEngine;
using Valve.VR;

namespace Basis.Scripts.Device_Management.Devices.OpenVR
{
[System.Serializable]
public class BasisOpenVRInputSkeleton : BasisInputSkeleton
{
    [SerializeField]
    public OpenVRDevice Device;
    [SerializeField]
    public SteamVR_Action_Skeleton skeletonAction;
    [SerializeField]
    public BasisOpenVRInputController BasisOpenVRInputController;
    public void Initalize(BasisOpenVRInputController basisOpenVRInputController)
    {
        BasisOpenVRInputController = basisOpenVRInputController;
        string Action = "Skeleton" + BasisOpenVRInputController.inputSource.ToString();
        skeletonAction = SteamVR_Input.GetAction<SteamVR_Action_Skeleton>(Action);
        if (skeletonAction != null)
        {
            if (BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.LeftHand)
            {
                AssignAsLeft();
            }
            else if (BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.RightHand)
            {
                AssignAsRight();
            }
            SteamVR_Input.onSkeletonsUpdated += SteamVR_Input_OnSkeletonsUpdated;
        }
        else
        {
            Debug.LogError("Missing Skeleton Action for " + Action);
        }

    }
    public void LateUpdate()
    {
        Simulate();
    }
    private void SteamVR_Input_OnSkeletonsUpdated(bool skipSendingEvents)
    {
        onTrackingChanged();
    }
    private void onTrackingChanged()
    {
        if (BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.LeftHand)
        {
            BasisLocalPlayer.Instance.AvatarDriver.AnimatorDriver.ApplyLeftHandAnims(skeletonAction.fingerCurls);
        }
        if (BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.RightHand)
        {
            BasisLocalPlayer.Instance.AvatarDriver.AnimatorDriver.ApplyRightHandAnims(skeletonAction.fingerCurls);
        }
    }
    public void DeInitalize()
    {
        SteamVR_Input.onSkeletonsUpdated -= SteamVR_Input_OnSkeletonsUpdated;
    }
}
}