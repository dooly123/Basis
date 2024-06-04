// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.XR;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VIVE.OpenXR.Raycast
{
    public class HandRaycastPointer : RaycastPointer
    {
        const string LOG_TAG = "VIVE.OpenXR.Raycast.HandRaycastPointer ";

        #region Inspector
        public bool IsLeft = false;

        [Tooltip("To apply poses on the raycast pointer.")]
        [SerializeField]
        private bool m_UsePose = true;
        public bool UsePose { get { return m_UsePose; } set { m_UsePose = value; } }

#if ENABLE_INPUT_SYSTEM
        [SerializeField]
        private InputActionReference m_AimPose = null;
        public InputActionReference AimPose { get { return m_AimPose; } set { m_AimPose = value; } }
        bool getAimTracked(InputActionReference actionReference)
        {
            bool tracked = false;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
                {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    tracked = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().isTracked;
#else
                    tracked = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().isTracked;
#endif
                    if (printIntervalLog)
                    {
                        sb.Clear().Append(LOG_TAG).Append("getAimTracked(").Append(tracked).Append(")");
                        DEBUG(sb);
                    }
                }
            }
            else
            {
                if (printIntervalLog)
                {
                    sb.Clear().Append(LOG_TAG).Append("getAimTracked() invalid input: ").Append(value);
                    DEBUG(sb);
                }
            }

            return tracked;
        }
        InputTrackingState getAimTrackingState(InputActionReference actionReference)
        {
            InputTrackingState state = InputTrackingState.None;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
                {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    state = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().trackingState;
#else
                    state = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().trackingState;
#endif
                    if (printIntervalLog)
                    {
                        sb.Clear().Append(LOG_TAG).Append("getAimTrackingState(").Append(state).Append(")");
                        DEBUG(sb);
                    }
                }
            }
            else
            {
                if (printIntervalLog)
                {
                    sb.Clear().Append(LOG_TAG).Append("getAimTrackingState() invalid input: ").Append(value);
                    DEBUG(sb);
                }
            }

            return state;
        }
        Vector3 getAimPosition(InputActionReference actionReference)
        {
            var position = Vector3.zero;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
                {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    position = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().position;
#else
                    position = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().position;
#endif
                    if (printIntervalLog)
                    {
                        sb.Clear().Append(LOG_TAG).Append("getAimPosition(").Append(position.x).Append(", ").Append(position.y).Append(", ").Append(position.z).Append(")");
                        DEBUG(sb);
                    }
                }
            }
            else
            {
                if (printIntervalLog)
                {
                    sb.Clear().Append(LOG_TAG).Append("getAimPosition() invalid input: ").Append(value);
                    DEBUG(sb);
                }
            }

            return position;
        }
        Quaternion getAimRotation(InputActionReference actionReference)
        {
            var rotation = Quaternion.identity;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
                {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    rotation = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().rotation;
#else
                    rotation = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().rotation;
#endif
                    if (printIntervalLog)
                    {
                        sb.Clear().Append(LOG_TAG).Append("getAimRotation(").Append(rotation.x).Append(", ").Append(rotation.y).Append(", ").Append(rotation.z).Append(", ").Append(rotation.w).Append(")");
                        DEBUG(sb);
                    }
                }
            }
            else
            {
                if (printIntervalLog)
                {
                    sb.Clear().Append(LOG_TAG).Append("getAimRotation() invalid input: ").Append(value);
                    DEBUG(sb);
                }
            }

            return rotation;
        }

        [SerializeField]
        private InputActionReference m_PinchStrength = null;
        public InputActionReference PinchStrength { get => m_PinchStrength; set => m_PinchStrength = value; }
        float getStrength(InputActionReference actionReference)
        {
            float strength = 0;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
                if (actionReference.action.activeControl.valueType == typeof(float))
                {
                    strength = actionReference.action.ReadValue<float>();
                    if (printIntervalLog)
                    {
                        sb.Clear().Append(LOG_TAG).Append("getStrength(").Append(strength).Append(")");
                        DEBUG(sb);
                    }
                }
            }
            else
            {
                if (printIntervalLog)
                {
                    sb.Clear().Append(LOG_TAG).Append("getStrength() invalid input: ").Append(value);
                    DEBUG(sb);
                }
            }

            return strength;
        }
#endif

        [Tooltip("Pinch strength to trigger events.")]
        [SerializeField]
        private float m_PinchThreshold = .5f;
        public float PinchThreshold { get { return m_PinchThreshold; } set { m_PinchThreshold = value; } }

        [SerializeField]
        private bool m_AlwaysEnable = false;
        public bool AlwaysEnable { get { return m_AlwaysEnable; } set { m_AlwaysEnable = value; } }
        #endregion

#if ENABLE_INPUT_SYSTEM
        protected override void Update()
        {
            base.Update();

            validPose = getAimTracked(m_AimPose);
            trackingStatus = getAimTrackingState(m_AimPose);
            var origin = getAimPosition(m_AimPose);
            var rotation = getAimRotation(m_AimPose);
            strength = getStrength(m_PinchStrength);

            if (!IsInteractable()) { return; }

            if (m_UsePose)
            {
                transform.localPosition = origin;
                transform.localRotation = rotation;
            }
        }

        bool validPose = false;
        InputTrackingState trackingStatus = InputTrackingState.None;
        private bool IsInteractable()
        {
            bool enabled = RaycastSwitch.Hand.Enabled;
            bool positionTracked = ((trackingStatus & InputTrackingState.Position) != 0);
            bool rotationTracked = ((trackingStatus & InputTrackingState.Rotation) != 0);

            m_Interactable = (m_AlwaysEnable || enabled)
                && validPose
                && positionTracked
                && rotationTracked;

            if (printIntervalLog)
            {
                sb.Clear().Append(LOG_TAG).Append("IsInteractable() m_Interactable: ").Append(m_Interactable)
                    .Append(", enabled: ").Append(enabled)
                    .Append(", validPose: ").Append(validPose)
                    .Append(", positionTracked: ").Append(positionTracked)
                    .Append(", rotationTracked: ").Append(rotationTracked)
                    .Append(", m_AlwaysEnable: ").Append(m_AlwaysEnable);
                DEBUG(sb);
            }

            return m_Interactable;
        }
#endif

        #region RaycastImpl Actions overrides
        bool eligibleForClick = false;
        float strength = 0;
        protected override bool OnDown()
        {
            if (!eligibleForClick)
            {
                bool down = strength > m_PinchThreshold;
                if (down)
                {
                    eligibleForClick = true;
                    return true;
                }
            }

            return false;
        }
        protected override bool OnHold()
        {
            bool hold = strength > m_PinchThreshold;
            if (!hold)
                eligibleForClick = false;
            return hold;
        }
        #endregion
    }
}
