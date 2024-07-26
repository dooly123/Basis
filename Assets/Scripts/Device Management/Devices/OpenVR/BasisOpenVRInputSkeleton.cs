using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
[System.Serializable]
public class BasisOpenVRInputSkeleton : BasisInputSkeleton
{
    public OpenVRDevice Device;
    public SteamVR_Action_Skeleton skeletonAction;

    public BasisOpenVRInputController BasisOpenVRInputController;
    public List<string> Ids = new List<string>();
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
            for (int Index = 0; Index < SteamVR_Action_Skeleton.numBones; Index++)
            {
                switch (Index)
                {
                    case 0:
                        // Logic for index 0
                        break;
                    case 1:
                        // 1 | wrist_l
                        break;
                    case 2:
                        // 2 | finger_thumb_0_l
                        break;
                    case 3:
                        //   ThumbProximal.TrackerData.rotation = Rotations[Index];
                        // ThumbProximal.TrackerData.position = Vector3.Lerp(ThumbProximal.TposeLocal.position, ActiveHand.TposeLocal.position, skeletonAction.thumbCurl);
                        // ThumbProximal.ApplyMovement();
                        break;
                    case 4:
                        // ThumbIntermediate.TrackerData.rotation = Rotations[Index];
                        // ThumbIntermediate.TrackerData.position = Vector3.Lerp(ThumbIntermediate.TposeLocal.position, ActiveHand.TposeLocal.position, skeletonAction.thumbCurl);
                        //ThumbIntermediate.ApplyMovement();
                        break;
                    case 5:
                        // ThumbDistal.TrackerData.rotation = Rotations[Index];
                        SetASTracked(ThumbDistal);///Fix
                        //      Vector3 Difference = ThumbDistal.TposeLocal.position - ActiveHand.TposeLocal.position;
                        //  ThumbDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + Difference, ActiveHand.CurrentWorldData.position, skeletonAction.thumbCurl);
                        //ThumbDistal.ApplyMovement();
                        break;
                    case 6:
                        // skip
                        break;
                    case 7:
                        //  IndexProximal.TrackerData.rotation = Rotations[Index];
                        // IndexProximal.TrackerData.position = Vector3.Lerp(IndexProximal.TposeLocal.position, ActiveHand.TposeLocal.position, skeletonAction.thumbCurl);
                        //  IndexProximal.ApplyMovement();
                        break;
                    case 8:
                        //  IndexIntermediate.TrackerData.rotation = Rotations[Index];
                        //   IndexIntermediate.TrackerData.position = Vector3.Lerp(IndexIntermediate.TposeLocal.position, ActiveHand.TposeLocal.position, skeletonAction.thumbCurl);
                        //  IndexIntermediate.ApplyMovement();
                        break;
                    case 9:
                        //  IndexDistal.TrackerData.rotation = Rotations[Index];
                        SetASTracked(IndexDistal);

                        Vector3 LocalStartPosition = IndexDistal.TposeLocal.position;
                        Vector3 LocalEndPosition = ActiveHand.TposeLocal.position;
                        Vector3 DifferenceBetweenStartAndEnd = LocalStartPosition - LocalEndPosition;
                        Vector3 DifferenceRotated =  ActiveHand.BoneTransform.rotation * DifferenceBetweenStartAndEnd;
                        IndexDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + DifferenceRotated, ActiveHand.CurrentWorldData.position, skeletonAction.indexCurl);
                        //  IndexDistal.ApplyMovement();
                        break;
                    case 10:
                        // skip
                        break;
                    case 11:
                        // MiddleProximal.TrackerData.rotation = Rotations[Index];
                        //           MiddleProximal.TrackerData.position = Vector3.Lerp(MiddleProximal.TposeLocal.position, ActiveHand.TposeLocal.position, skeletonAction.thumbCurl);
                        //   MiddleProximal.ApplyMovement();
                        break;
                    case 12:
                        // MiddleIntermediate.TrackerData.rotation = Rotations[Index];
                        //         MiddleIntermediate.TrackerData.position = Vector3.Lerp(MiddleIntermediate.TposeLocal.position, ActiveHand.TposeLocal.position, skeletonAction.thumbCurl);
                        //  MiddleIntermediate.ApplyMovement();
                        break;
                    case 13:
                        //here  MiddleDistal.TrackerData.rotation = Rotations[Index];
                        // SetASTracked(MiddleDistal);
                        // Difference = MiddleDistal.TposeLocal.position - ActiveHand.TposeLocal.position;
                        // MiddleDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + Difference, ActiveHand.CurrentWorldData.position, skeletonAction.middleCurl);
                        // MiddleDistal.ApplyMovement();
                        break;
                    case 14:
                        // skip
                        break;
                    case 15:
                        //   RingProximal.TrackerData.rotation = Rotations[Index];
                        //       RingProximal.TrackerData.position = Vector3.Lerp(RingProximal.TposeLocal.position, ActiveHand.TposeLocal.position, skeletonAction.thumbCurl);
                        //  RingProximal.ApplyMovement();
                        break;
                    case 16:
                        //  RingIntermediate.TrackerData.rotation = Rotations[Index];
                        //     RingIntermediate.TrackerData.position = Vector3.Lerp(RingIntermediate.TposeLocal.position, ActiveHand.TposeLocal.position, skeletonAction.thumbCurl);
                        //      RingIntermediate.ApplyMovement();
                        break;
                    case 17:
                        //   RingDistal.TrackerData.rotation = Rotations[Index];
                        //here  SetASTracked(RingDistal);
                        //  Difference = RingDistal.TposeLocal.position - ActiveHand.TposeLocal.position;
                        // RingDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + Difference, ActiveHand.CurrentWorldData.position, skeletonAction.ringCurl);
                        // RingDistal.ApplyMovement();
                        break;
                    case 18:
                        // skip
                        break;
                    case 19:
                        //    LittleProximal.TrackerData.rotation = Rotations[Index];
                        //     LittleProximal.TrackerData.position = Vector3.Lerp(LittleProximal.TposeLocal.position, ActiveHand.TposeLocal.position, skeletonAction.thumbCurl);
                        //  LittleProximal.ApplyMovement();
                        break;
                    case 20:
                        //   LittleIntermediate.TrackerData.rotation = Rotations[Index];
                        //       LittleIntermediate.TrackerData.position = Vector3.Lerp(LittleIntermediate.TposeLocal.position, ActiveHand.TposeLocal.position, skeletonAction.thumbCurl);
                        //   LittleIntermediate.ApplyMovement();
                        break;
                    case 21:
                        //   LittleDistal.TrackerData.rotation = Rotations[Index];
                        //here SetASTracked(LittleDistal);
                        // Difference = LittleDistal.TposeLocal.position - ActiveHand.TposeLocal.position;
                        // LittleDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + Difference, ActiveHand.CurrentWorldData.position, skeletonAction.pinkyCurl);
                        //   LittleDistal.ApplyMovement();
                        break;
                    // Add cases for other bones if necessary
                    default:
                        // Default case for other indices
                        break;
                }
            }
        }
    }
    public void DeInitalize()
    {
        SteamVR_Input.onSkeletonsUpdated -= SteamVR_Input_OnSkeletonsUpdated;
    }
}