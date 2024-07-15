using NUnit.Framework;
using System.Collections.Generic;
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

    public BasisBoneControl ActiveHand;
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

                InitializeBones(BasisBoneTrackedRole.LeftHand, out ActiveHand);
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

                InitializeBones(BasisBoneTrackedRole.RightHand, out ActiveHand);
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
      //  boneControl.HasRigLayer = BasisHasRigLayer.HasRigLayer;
      //  boneControl.HasTracked = BasisHasTracked.HasTracker;
    }
    private void SteamVR_Input_OnSkeletonsUpdated(bool skipSendingEvents)
    {
        onTrackingChanged();
    }
    public void SetASTracked(BasisBoneControl Input)
    {
        if (Input.HasTracked == BasisHasTracked.HasNoTracker)
        {
            Input.HasRigLayer = BasisHasRigLayer.HasRigLayer;
            Input.HasTracked = BasisHasTracked.HasTracker;
        }
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
                        SetASTracked(ThumbDistal);
                        Vector3 Difference = ThumbDistal.TposeLocal.position - ActiveHand.TposeLocal.position;
                        ThumbDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + Difference, ActiveHand.CurrentWorldData.position, skeletonAction.thumbCurl);
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
                        Difference = IndexDistal.TposeLocal.position - ActiveHand.TposeLocal.position;
                        IndexDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + Difference, ActiveHand.CurrentWorldData.position, skeletonAction.indexCurl);
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
                        //  MiddleDistal.TrackerData.rotation = Rotations[Index];
                        SetASTracked(MiddleDistal);
                        Difference = MiddleDistal.TposeLocal.position - ActiveHand.TposeLocal.position;
                        MiddleDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + Difference, ActiveHand.CurrentWorldData.position, skeletonAction.middleCurl);
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
                        SetASTracked(RingDistal);
                        Difference = RingDistal.TposeLocal.position - ActiveHand.TposeLocal.position;
                        RingDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + Difference, ActiveHand.CurrentWorldData.position, skeletonAction.ringCurl);
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
                        SetASTracked(LittleDistal);
                        Difference = LittleDistal.TposeLocal.position - ActiveHand.TposeLocal.position;
                        LittleDistal.TrackerData.position = Vector3.Lerp(ActiveHand.CurrentWorldData.position + Difference, ActiveHand.CurrentWorldData.position, skeletonAction.pinkyCurl);
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