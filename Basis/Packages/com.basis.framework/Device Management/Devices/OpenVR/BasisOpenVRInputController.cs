using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management.Devices.OpenVR.Structs;
using Basis.Scripts.TransformBinders.BoneControl;
using Unity.Mathematics;
using UnityEngine;
using Valve.VR;

namespace Basis.Scripts.Device_Management.Devices.OpenVR
{
    [DefaultExecutionOrder(15001)]
    public class BasisOpenVRInputController : BasisInput
    {
        public OpenVRDevice Device;
        public SteamVR_Input_Sources inputSource;
        public SteamVR_Action_Pose poseAction = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose");
        public BasisOpenVRInputSkeleton SkeletonHandInput = null;
        public bool HasOnUpate = false;
        public void Initialize(OpenVRDevice device, string UniqueID, string UnUniqueID, string subSystems, bool AssignTrackedRole, BasisBoneTrackedRole basisBoneTrackedRole, SteamVR_Input_Sources SteamVR_Input_Sources)
        {
            if (HasOnUpate && poseAction != null)
            {
                poseAction[inputSource].onUpdate -= SteamVR_Behaviour_Pose_OnUpdate;
                HasOnUpate = false;
            }
            inputSource = SteamVR_Input_Sources;
            Device = device;
            InitalizeTracking(UniqueID, UnUniqueID, subSystems, AssignTrackedRole, basisBoneTrackedRole);
            if (poseAction != null)
            {
                if (HasOnUpate == false)
                {
                    poseAction[inputSource].onUpdate += SteamVR_Behaviour_Pose_OnUpdate;
                    HasOnUpate = true;
                }
            }
            if (inputSource == SteamVR_Input_Sources.LeftHand || inputSource == SteamVR_Input_Sources.RightHand)
            {
                SkeletonHandInput = new BasisOpenVRInputSkeleton();
                SkeletonHandInput.Initalize(this);
            }
            BasisDebug.Log("set Controller to inputSource " + inputSource + " bone role " + basisBoneTrackedRole);
        }
        public new void OnDestroy()
        {
            if (poseAction != null)
            {
                poseAction[inputSource].onUpdate -= SteamVR_Behaviour_Pose_OnUpdate;
                HasOnUpate = false;
            }
            if (SkeletonHandInput != null)
            {
                SkeletonHandInput.DeInitalize();
            }
            historyBuffer.Clear();
            base.OnDestroy();
        }
        public override void DoPollData()
        {
            if (SteamVR.active)
            {
                InputState.Primary2DAxis = SteamVR_Actions._default.Joystick.GetAxis(inputSource);
                InputState.PrimaryButtonGetState = SteamVR_Actions._default.A_Button.GetState(inputSource);
                InputState.SecondaryButtonGetState = SteamVR_Actions._default.B_Button.GetState(inputSource);
                InputState.Trigger = SteamVR_Actions._default.Trigger.GetAxis(inputSource);
                UpdatePlayerControl();
            }
        }
        private void SteamVR_Behaviour_Pose_OnUpdate(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
        {
            UpdateHistoryBuffer();
            if (HasOnUpate)
            {
                LocalRawPosition = poseAction[inputSource].localPosition;
                LocalRawRotation = poseAction[inputSource].localRotation;
            }
            FinalPosition = LocalRawPosition * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale;
            FinalRotation = LocalRawRotation;
            if (hasRoleAssigned)
            {
                if (Control.HasTracked != BasisHasTracked.HasNoTracker)
                {
                    // Apply position offset using math.mul for quaternion-vector multiplication
                    Control.IncomingData.position = FinalPosition - math.mul(FinalRotation, AvatarPositionOffset * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale);

                    // Apply rotation offset using math.mul for quaternion multiplication
                    Control.IncomingData.rotation = math.mul(FinalRotation, Quaternion.Euler(AvatarRotationOffset));
                }
            }
        }
        #region Mostly Unused Steam
        protected SteamVR_HistoryBuffer historyBuffer = new SteamVR_HistoryBuffer(30);
        protected int lastFrameUpdated;
        protected void UpdateHistoryBuffer()
        {
            int currentFrame = Time.frameCount;
            if (lastFrameUpdated != currentFrame)
            {
                historyBuffer.Update(poseAction[inputSource].localPosition, poseAction[inputSource].localRotation, poseAction[inputSource].velocity, poseAction[inputSource].angularVelocity);
                lastFrameUpdated = currentFrame;
            }
        }
        public Vector3 GetVelocity()
        {
            return poseAction[inputSource].velocity;
        }
        public Vector3 GetAngularVelocity()
        {
            return poseAction[inputSource].angularVelocity;
        }
        public bool GetVelocitiesAtTimeOffset(float secondsFromNow, out Vector3 velocity, out Vector3 angularVelocity)
        {
            return poseAction[inputSource].GetVelocitiesAtTimeOffset(secondsFromNow, out velocity, out angularVelocity);
        }
        public void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity)
        {
            int top = historyBuffer.GetTopVelocity(10, 1);

            historyBuffer.GetAverageVelocities(out velocity, out angularVelocity, 2, top);
        }
        public bool isValid { get { return poseAction[inputSource].poseIsValid; } }
        public bool isActive { get { return poseAction[inputSource].active; } }
        #endregion
    }
}