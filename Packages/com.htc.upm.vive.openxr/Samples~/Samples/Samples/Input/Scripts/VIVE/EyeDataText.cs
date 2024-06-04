// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;

namespace VIVE.OpenXR.Samples.OpenXRInput
{
    [RequireComponent(typeof(Text))]
    public class EyeDataText : MonoBehaviour
    {
        const string LOG_TAG = "VIVE.OpenXR.Samples.OpenXRInput.EyeDataText";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
        void INTERVAL(string msg) { if (printIntervalLog) { DEBUG(msg); } }

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
        Vector3 getPosition(InputActionReference actionReference)
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
                    INTERVAL("getPosition(" + position.x.ToString() + ", " + position.y.ToString() + ", " + position.z.ToString() + ")");
                }
            }
            else
            {
                INTERVAL("getPosition() invalid input: " + value);
            }

            return position;
        }
        Quaternion getRotation(InputActionReference actionReference)
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
                    INTERVAL("getRotation(" + rotation.x.ToString() + ", " + rotation.y.ToString() + ", " + rotation.z.ToString() + ", " + rotation.w.ToString() + ")");
                }
            }
            else
            {
                INTERVAL("getRotation() invalid input: " + value);
            }

            return rotation;
        }

        private Text m_Text = null;

        private void Awake()
        {
            m_Text = GetComponent<Text>();
        }

        int printFrame = 0;
        private bool printIntervalLog = false;
        private void Update()
        {
            printFrame++;
            printFrame %= 300;
            printIntervalLog = (printFrame == 0);

            if (m_Text == null) { return; }

            m_Text.text = "Eye ";

            bool tracked = getTracked(m_EyePose);
            m_Text.text += "tracked: " + tracked + "\n";

            InputTrackingState trackingState = getTrackingState(m_EyePose);
            m_Text.text += "tracking state: " + trackingState + "\n";

            Vector3 position = getPosition(m_EyePose);
            m_Text.text += "position (" + position.x.ToString() + ", " + position.y.ToString() + ", " + position.z.ToString() + ")\n";

            m_Text.text += "refresh rate: " + GetRefreshRate().ToString();
        }

        private float GetRefreshRate()
		{
			if (!OpenXRRuntime.IsExtensionEnabled("XR_FB_display_refresh_rate"))
				return 0.0f;

			if (XR_FB_display_refresh_rate.GetDisplayRefreshRate(out float rate) == XrResult.XR_SUCCESS) { return rate; }

            return 0.0f;
		}
    }
}
