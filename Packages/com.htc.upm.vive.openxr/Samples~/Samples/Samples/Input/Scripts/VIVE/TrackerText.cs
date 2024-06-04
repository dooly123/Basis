// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

namespace VIVE.OpenXR.Samples.OpenXRInput
{
    [RequireComponent(typeof(Text))]
    public class TrackerText : MonoBehaviour
    {
        const string LOG_TAG = "VIVE.OpenXR.Samples.OpenXRInput.TrackerText";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + msg); }

        #region Right Tracker
        [SerializeField]
        private InputActionReference m_TrackedR = null;
        public InputActionReference TrackedR { get => m_TrackedR; set => m_TrackedR = value; }

        [SerializeField]
        private InputActionReference m_TrackingStateR = null;
        public InputActionReference TrackingStateR { get => m_TrackingStateR; set => m_TrackingStateR = value; }

        [SerializeField]
        private InputActionReference m_RightA = null;
        public InputActionReference RightA { get => m_RightA; set => m_RightA = value; }
        #endregion

        #region Left Tracker
        [SerializeField]
        private InputActionReference m_TrackedL = null;
        public InputActionReference TrackedL { get => m_TrackedL; set => m_TrackedL = value; }

        [SerializeField]
        private InputActionReference m_TrackingStateL = null;
        public InputActionReference TrackingStateL { get => m_TrackingStateL; set => m_TrackingStateL = value; }

        [SerializeField]
        private InputActionReference m_LeftX = null;
        public InputActionReference LeftX { get => m_LeftX; set => m_LeftX = value; }

        [SerializeField]
        private InputActionReference m_LeftMenu = null;
        public InputActionReference LeftMenu { get => m_LeftMenu; set => m_LeftMenu = value; }
        #endregion

        private Text m_Text = null;

        private void Start()
        {
            m_Text = GetComponent<Text>();
        }
        private void Update()
        {
            if (m_Text == null) { return; }

            // Left tracker text
            m_Text.text = "Left Tracker ";

            { // Tracked
                if (Utils.GetButton(m_TrackedL, out bool value, out string msg))
                {
                    m_Text.text += "tracked: " + value + ", ";
                }
            }
            { // trackingState
                if (Utils.GetInteger(m_TrackingStateL, out InputTrackingState value, out string msg))
                {
                    m_Text.text += "state: " + value + ", ";
                }
            }

            { // Left X
                if (Utils.GetButton(m_LeftX, out bool value, out string msg))
                {
                    if (value)
                    {
                        DEBUG("Update() Left X is pressed.");
                        m_Text.text += "Left X";
                    }
                }
            }
            { // Left Menu
                if (Utils.GetButton(m_LeftMenu, out bool value, out string msg))
                {
                    if (value)
                    {
                        DEBUG("Update() Left Menu is pressed.");
                        m_Text.text += "Left Menu";
                    }
                }
            }

            // Right tracker text
            m_Text.text += "\nRight Tracker ";

            { // Tracked
                if (Utils.GetButton(m_TrackedR, out bool value, out string msg))
                {
                    m_Text.text += "tracked: " + value + ", ";
                }
            }
            { // trackingState
                if (Utils.GetInteger(m_TrackingStateR, out InputTrackingState value, out string msg))
                {
                    m_Text.text += "state: " + value + ", ";
                }
            }

            { // Right A
                if (Utils.GetButton(m_RightA, out bool value, out string msg))
                {
                    if (value)
                    {
                        DEBUG("Update() Right A is pressed.");
                        m_Text.text += "Right A";
                    }
                }
            }
        }
    }
}
