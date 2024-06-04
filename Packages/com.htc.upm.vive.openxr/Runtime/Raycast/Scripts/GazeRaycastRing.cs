// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace VIVE.OpenXR.Raycast
{
    public class GazeRaycastRing : RaycastRing
    {
        const string LOG_TAG = "VIVE.OpenXR.Raycast.GazeRaycastRing";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
        void INTERVAL(string msg) { if (printIntervalLog) { DEBUG(msg); } }

        #region Inspector
        [SerializeField]
        [Tooltip("Use Eye Tracking data for Gaze.")]
        private bool m_EyeTracking = false;
        public bool EyeTracking { get { return m_EyeTracking; } set { m_EyeTracking = value; } }

        [SerializeField]
        private InputActionReference m_EyePose = null;
        public InputActionReference EyePose { get => m_EyePose; set => m_EyePose = value; }
        bool getTracked(InputActionReference actionReference)
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
                    INTERVAL("getTracked(" + tracked + ")");
                }
            }
            else
            {
                INTERVAL("getTracked() invalid input: " + value);
            }

            return tracked;
        }
        InputTrackingState getTrackingState(InputActionReference actionReference)
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
                    INTERVAL("getTrackingState(" + state + ")");
                }
            }
            else
            {
                INTERVAL("getTrackingState() invalid input: " + value);
            }

            return state;
        }
        Vector3 getDirection(InputActionReference actionReference)
        {
            Quaternion rotation = Quaternion.identity;

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
                    INTERVAL("getDirection(" + rotation.x.ToString() + ", " + rotation.y.ToString() + ", " + rotation.z.ToString() + ", " + rotation.w.ToString() + ")");
                    return (rotation * Vector3.forward);
                }
            }
            else
            {
                INTERVAL("getDirection() invalid input: " + value);
            }

            return Vector3.forward;
        }
        Vector3 getOrigin(InputActionReference actionReference)
        {
            var origin = Vector3.zero;

            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
                {
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    origin = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().position;
#else
                    origin = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().position;
#endif
                    INTERVAL("getOrigin(" + origin.x.ToString() + ", " + origin.y.ToString() + ", " + origin.z.ToString() + ")");
                }
            }
            else
            {
                INTERVAL("getOrigin() invalid input: " + value);
            }

            return origin;
        }

        [Tooltip("Event triggered by gaze.")]
        [SerializeField]
        private GazeEvent m_InputEvent = GazeEvent.Down;
        public GazeEvent InputEvent { get { return m_InputEvent; } set { m_InputEvent = value; } }

        [Tooltip("Keys for control.")]
        [SerializeField]
        private List<InputActionReference> m_ActionsKeys = new List<InputActionReference>();
        public List<InputActionReference> ActionKeys { get { return m_ActionsKeys; } set { m_ActionsKeys = value; } }

        bool getButton(InputActionReference actionReference)
        {
            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
                if (actionReference.action.activeControl.valueType == typeof(bool))
                    return actionReference.action.ReadValue<bool>();
                if (actionReference.action.activeControl.valueType == typeof(float))
                    return actionReference.action.ReadValue<float>() > 0;
            }
            else
            {
                INTERVAL("getButton() invalid input: " + value);
            }

            return false;
        }

        [SerializeField]
        private bool m_AlwaysEnable = false;
        public bool AlwaysEnable { get { return m_AlwaysEnable; } set { m_AlwaysEnable = value; } }
        #endregion

        #region MonoBehaviour overrides
        protected override void Awake()
        {
            base.Awake();
        }

        private bool m_KeyDown = false;
        protected override void Update()
        {
            base.Update();

            if (!IsInteractable()) { return; }

            m_KeyDown = ButtonPressed();
        }
        #endregion

        private bool IsInteractable()
        {
            bool enabled = RaycastSwitch.Gaze.Enabled;

            m_Interactable = (m_AlwaysEnable || enabled);

            if (printIntervalLog)
            {
                DEBUG("IsInteractable() enabled: " + enabled + ", m_AlwaysEnable: " + m_AlwaysEnable);
            }

            return m_Interactable;
        }

        internal bool m_Down = false, m_Hold = false;
        private bool ButtonPressed()
        {
            if (m_ActionsKeys == null) { return false; }

            bool keyDown = false;
            for (int i = 0; i < m_ActionsKeys.Count; i++)
            {
                var pressed = getButton(m_ActionsKeys[i]);
                if (pressed)
                    DEBUG("ButtonPressed()" + m_ActionsKeys[i].name + " is pressed.");
                keyDown |= pressed;
            }

            m_Down = false;
            if (!m_Hold) { m_Down |= keyDown; }
            m_Hold = keyDown;

            return m_Down;
        }

        protected override bool UseEyeData(out Vector3 direction)
        {
            bool tracked = getTracked(m_EyePose);
            bool useEye = m_EyeTracking && tracked;

            getTrackingState(m_EyePose);
            getOrigin(m_EyePose);
            direction = getDirection(m_EyePose);

            INTERVAL("UseEyeData() m_EyeTracking: " + m_EyeTracking + ", tracked: " + tracked);

            if (!useEye) { return base.UseEyeData(out direction); }

            return useEye;
        }

        #region RaycastImpl Actions overrides
        protected override bool OnDown()
        {
            if (m_InputEvent != GazeEvent.Down) { return false; }

            bool down = false;
            if (m_RingPercent >= 100 || m_KeyDown)
            {
                m_RingPercent = 0;
                m_GazeOnTime = Time.unscaledTime;
                down = true;
                DEBUG("OnDown()");
            }

            return down;
        }
        protected override bool OnSubmit()
        {
            if (m_InputEvent != GazeEvent.Submit) { return false; }

            bool submit = false;
            if (m_RingPercent >= 100 || m_KeyDown)
            {
                m_RingPercent = 0;
                m_GazeOnTime = Time.unscaledTime;
                submit = true;
                DEBUG("OnSubmit()");
            }

            return submit;
        }
        #endregion
    }
}
