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
                Vector2 ThumbPercentage = new Vector2(BasisBaseMuscleDriver.MapValue(skeletonAction.fingerCurls[0], 0, 1, -1f, 0.7f),
                BasisBaseMuscleDriver.MapValue(skeletonAction.fingerSplays[0], 0, 1, -1f, 0.7f));
                BasisLocalPlayer.Instance.AvatarDriver.BasisMuscleDriver.LeftFinger.ThumbPercentage = ThumbPercentage;

                Vector2 IndexPercentage = new Vector2(BasisBaseMuscleDriver.MapValue(skeletonAction.fingerCurls[1], 0, 1, -1f, 0.7f),
                BasisBaseMuscleDriver.MapValue(skeletonAction.fingerSplays[1], 0, 1, -1f, 0.7f));
                BasisLocalPlayer.Instance.AvatarDriver.BasisMuscleDriver.LeftFinger.IndexPercentage = IndexPercentage;

                Vector2 MiddlePercentage = new Vector2(BasisBaseMuscleDriver.MapValue(skeletonAction.fingerCurls[2], 0, 1, -1f, 0.7f),
                BasisBaseMuscleDriver.MapValue(skeletonAction.fingerSplays[2], 0, 1, -1f, 0.7f));
                BasisLocalPlayer.Instance.AvatarDriver.BasisMuscleDriver.LeftFinger.MiddlePercentage = MiddlePercentage;

                Vector2 RingPercentage = new Vector2(BasisBaseMuscleDriver.MapValue(skeletonAction.fingerCurls[3], 0, 1, -1f, 0.7f),
                BasisBaseMuscleDriver.MapValue(skeletonAction.fingerSplays[3], 0, 1, -1f, 0.7f));
                BasisLocalPlayer.Instance.AvatarDriver.BasisMuscleDriver.LeftFinger.RingPercentage = RingPercentage;

                Vector2 LittlePercentage = new Vector2(BasisBaseMuscleDriver.MapValue(skeletonAction.fingerCurls[4], 0, 1, -1f, 0.7f), 0);
                // BasisBaseMuscleDriver.MapValue(skeletonAction.fingerSplays[4], 0, 1, -1f, 0.7f));
                BasisLocalPlayer.Instance.AvatarDriver.BasisMuscleDriver.LeftFinger.LittlePercentage = LittlePercentage;
            }
            if (BasisOpenVRInputController.inputSource == SteamVR_Input_Sources.RightHand)
            {
            }
        }
        public void DeInitalize()
        {
            SteamVR_Input.onSkeletonsUpdated -= SteamVR_Input_OnSkeletonsUpdated;
        }
    }
}