using UnityEngine;
using Valve.VR;
[System.Serializable]
public class BasisOpenVRInputSkeleton
{
    public OpenVRDevice Device;
    public SteamVR_Action_Skeleton skeletonAction;

    public BasisOpenVRInputController BasisOpenVRInputController;

    public BasisBoneControl ThumbProximal;
    public BasisBoneControl ThumbIntermediate;
    public BasisBoneControl ThumbDistal;

    public BasisBoneControl IndexProximal;
    public BasisBoneControl IndexIntermediate;
    public BasisBoneControl IndexDistal;

    public BasisBoneControl MiddleProximal;
    public BasisBoneControl MiddleIntermediate;
    public BasisBoneControl MiddleDistal;

    public BasisBoneControl RingProximal;
    public BasisBoneControl RingIntermediate;
    public BasisBoneControl RingDistal;

    public BasisBoneControl LittleProximal;
    public BasisBoneControl LittleIntermediate;
    public BasisBoneControl LittleDistal;
    public void Initalize(BasisOpenVRInputController basisOpenVRInputController)
    {
        BasisOpenVRInputController = basisOpenVRInputController;
        string Action = "Skeleton" + BasisOpenVRInputController.inputSource.ToString();
        skeletonAction = SteamVR_Input.GetAction<SteamVR_Action_Skeleton>(Action);
        if (skeletonAction != null)
        {
            if (BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.LeftHand)
            {
                InitializeBones(BasisBoneTrackedRole.LeftThumbProximal, out ThumbProximal);
                InitializeBones(BasisBoneTrackedRole.LeftThumbIntermediate, out ThumbIntermediate);
                InitializeBones(BasisBoneTrackedRole.LeftThumbDistal, out ThumbDistal);

                InitializeBones(BasisBoneTrackedRole.LeftIndexProximal, out IndexProximal);
                InitializeBones(BasisBoneTrackedRole.LeftIndexIntermediate, out IndexIntermediate);
                InitializeBones(BasisBoneTrackedRole.LeftIndexDistal, out IndexDistal);

                InitializeBones(BasisBoneTrackedRole.LeftMiddleProximal, out MiddleProximal);
                InitializeBones(BasisBoneTrackedRole.LeftMiddleIntermediate, out MiddleIntermediate);
                InitializeBones(BasisBoneTrackedRole.LeftMiddleDistal, out MiddleDistal);

                InitializeBones(BasisBoneTrackedRole.LeftRingProximal, out RingProximal);
                InitializeBones(BasisBoneTrackedRole.LeftRingIntermediate, out RingIntermediate);
                InitializeBones(BasisBoneTrackedRole.LeftRingDistal, out RingDistal);

                InitializeBones(BasisBoneTrackedRole.LeftLittleProximal, out LittleProximal);
                InitializeBones(BasisBoneTrackedRole.LeftLittleIntermediate, out LittleIntermediate);
                InitializeBones(BasisBoneTrackedRole.LeftLittleDistal, out LittleDistal);
            }
            else if (BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.RightHand)
            {
                InitializeBones(BasisBoneTrackedRole.RightThumbProximal, out ThumbProximal);
                InitializeBones(BasisBoneTrackedRole.RightThumbIntermediate, out ThumbIntermediate);
                InitializeBones(BasisBoneTrackedRole.RightThumbDistal, out ThumbDistal);

                InitializeBones(BasisBoneTrackedRole.RightIndexProximal, out IndexProximal);
                InitializeBones(BasisBoneTrackedRole.RightIndexIntermediate, out IndexIntermediate);
                InitializeBones(BasisBoneTrackedRole.RightIndexDistal, out IndexDistal);

                InitializeBones(BasisBoneTrackedRole.RightMiddleProximal, out MiddleProximal);
                InitializeBones(BasisBoneTrackedRole.RightMiddleIntermediate, out MiddleIntermediate);
                InitializeBones(BasisBoneTrackedRole.RightMiddleDistal, out MiddleDistal);

                InitializeBones(BasisBoneTrackedRole.RightRingProximal, out RingProximal);
                InitializeBones(BasisBoneTrackedRole.RightRingIntermediate, out RingIntermediate);
                InitializeBones(BasisBoneTrackedRole.RightRingDistal, out RingDistal);

                InitializeBones(BasisBoneTrackedRole.RightLittleProximal, out LittleProximal);
                InitializeBones(BasisBoneTrackedRole.RightLittleIntermediate, out LittleIntermediate);
                InitializeBones(BasisBoneTrackedRole.RightLittleDistal, out LittleDistal);
            }
            Quaternion[] Rotations = skeletonAction.GetBoneRotations();
            SteamVR_Input.onSkeletonsUpdated += SteamVR_Input_OnSkeletonsUpdated;
        }
        else
        {
            Debug.LogError("Missing Skeleton Action for " + Action);
        }
    }
    private void InitializeBones(BasisBoneTrackedRole boneRole, out BasisBoneControl boneControl)
    {
        BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out boneControl, boneRole);
        boneControl.HasRigLayer = BasisHasRigLayer.HasRigLayer;
        boneControl.HasTracked = BasisHasTracked.HasTracker;
    }
    private void SteamVR_Input_OnSkeletonsUpdated(bool skipSendingEvents)
    {
        onTrackingChanged();
    }
    private void onTrackingChanged()
    {
        if(BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.LeftHand || BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.RightHand)
        {
            Vector3[] Positions = skeletonAction.GetBonePositions();
            Quaternion[] Rotations = skeletonAction.GetBoneRotations();
            for (int Index = 0; Index < Rotations.Length; Index++)
            {
                switch (Index)
                {
                    case 0:
                        // Logic for index 0
                        break;
                    case 1:
                        // 1 | wrist_l
                     //  Debug.Log("wrist" + Positions[Index]);
                        break;
                    case 2:
                        //2 | finger_thumb_0_l
                        break;
                    case 3:
                        ThumbProximal.TrackerData.rotation =  Rotations[Index];
                        ThumbProximal.TrackerData.position = BasisOpenVRInputController.FinalPosition + Positions[Index];
                        ThumbProximal.ApplyMovement();
                        //3 | finger_thumb_1_l
                        break;
                    case 4:
                        //4 | finger_thumb_2_l
                        ThumbIntermediate.TrackerData.rotation =  Rotations[Index];
                        ThumbIntermediate.TrackerData.position = BasisOpenVRInputController.FinalPosition + Positions[Index];
                        ThumbIntermediate.ApplyMovement();
                        break;
                    case 5:
                        //5 | finger_thumb_l_end
                        ThumbDistal.TrackerData.rotation =  Rotations[Index];
                        ThumbDistal.TrackerData.position = BasisOpenVRInputController.FinalPosition + Positions[Index];
                        ThumbDistal.ApplyMovement();
                        break;
                    case 6:
  //skip
                        break;
                    case 7:
                        IndexProximal.TrackerData.rotation = Rotations[Index];
                        IndexProximal.TrackerData.position = BasisOpenVRInputController.FinalPosition + Positions[Index];
                        IndexProximal.ApplyMovement();
                        break;
                    case 8:
                        IndexIntermediate.TrackerData.rotation = Rotations[Index];
                        IndexIntermediate.TrackerData.position = BasisOpenVRInputController.FinalPosition + Positions[Index];
                        IndexIntermediate.ApplyMovement();
                        // Logic for index 8
                        break;
                    case 9:
                        IndexDistal.TrackerData.rotation = Rotations[Index];
                        IndexDistal.TrackerData.position = BasisOpenVRInputController.FinalPosition + Positions[Index];
                        IndexDistal.ApplyMovement();
                        // Logic for index 9
                        break;
                    // Continue adding cases for each index up to 31
                    case 31:
                        // Logic for index 31
                        break;
                    default:
                        // Default case for other indices
                        break;
                }
            }
        }
        // root 0,1,2,end
        // wrist
        // thumb
        // index
        // middle
        // ring
        // pinky
        // 31 
    }
    public void DeInitalize()
    {
        SteamVR_Input.onSkeletonsUpdated -= SteamVR_Input_OnSkeletonsUpdated;
    }
}