using UnityEngine;
using UnityEngine.UIElements;
using Valve.VR;
[System.Serializable]
public class BasisOpenVRInputSkeleton : BasisInputSkeleton
{
    [SerializeField]
    public OpenVRDevice Device;
    [SerializeField]
    public SteamVR_Action_Skeleton skeletonAction;
    [SerializeField]
    public BasisOpenVRInputController BasisOpenVRInputController;
    public Vector3[] Positions;
    public Quaternion[] Rotations;
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
    private void SteamVR_Input_OnSkeletonsUpdated(bool skipSendingEvents)
    {
        onTrackingChanged();
    }
    private void onTrackingChanged()
    {
        if (BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.LeftHand || BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.RightHand)
        {
            Positions = skeletonAction.GetBonePositions();
            Rotations = skeletonAction.GetBoneRotations();
            indexCurl = 1 - skeletonAction.indexCurl;
            middleCurl = 1 - skeletonAction.middleCurl;
            pinkyCurl = 1 - skeletonAction.pinkyCurl;
            ringCurl = 1 - skeletonAction.ringCurl;
            thumbCurl = 1 - skeletonAction.thumbCurl;
            Simulate();
        }
    }
    public void DeInitalize()
    {
        SteamVR_Input.onSkeletonsUpdated -= SteamVR_Input_OnSkeletonsUpdated;
    }
}