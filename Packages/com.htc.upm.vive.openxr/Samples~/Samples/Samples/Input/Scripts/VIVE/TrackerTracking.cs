// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Text;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VIVE.OpenXR.Samples.OpenXRInput
{
    [RequireComponent(typeof(Text))]
    public class TrackerTracking : MonoBehaviour
    {
        const string LOG_TAG = "VIVE.XR.Sample.OpenXRInput.TrackerTracking ";
        StringBuilder m_sb = null;
        StringBuilder sb {
            get {
                if (m_sb == null) { m_sb = new StringBuilder(); }
                return m_sb;
            }
        }
        void DEBUG(StringBuilder msg)
        {
            msg.Insert(0, LOG_TAG);
            Debug.Log(msg);
        }

        [SerializeField]
        private string m_TrackerName = "";
        public string TrackerName { get { return m_TrackerName; } set { m_TrackerName = value; } }

        [SerializeField]
        private InputActionReference m_IsTracked = null;
        public InputActionReference IsTracked { get { return m_IsTracked; } set { m_IsTracked = value; } }

        [SerializeField]
        private InputActionReference m_TrackingState = null;
        public InputActionReference TrackingState { get { return m_TrackingState; } set { m_TrackingState = value; } }

        [SerializeField]
        private InputActionReference m_Position = null;
        public InputActionReference Position { get { return m_Position; } set { m_Position = value; } }

        [SerializeField]
        private InputActionReference m_Rotation = null;
        public InputActionReference Rotation { get { return m_Rotation; } set { m_Rotation = value; } }

        private Text m_Text = null;
        private void Start()
        {
            m_Text = GetComponent<Text>();
        }

        void Update()
        {
            if (m_Text == null) { return; }

            m_Text.text = m_TrackerName;

            m_Text.text += " isTracked: ";
            {
                if (Utils.GetButton(m_IsTracked, out bool value, out string msg))
                {
                    m_Text.text += value;
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += "\n";
            m_Text.text += "trackingState: ";
            {
                if (Utils.GetInteger(m_TrackingState, out InputTrackingState value, out string msg))
                {
                    m_Text.text += value;
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += "\n";
            m_Text.text += "position (";
            {
                if (Utils.GetVector3(m_Position, out Vector3 value, out string msg))
                {
                    m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString();
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += ")\n";
            m_Text.text += "rotation (";
            {
                if (Utils.GetQuaternion(m_Rotation, out Quaternion value, out string msg))
                {
                    m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString() + ", " + value.w.ToString();
                }
                else
                {
                    m_Text.text += msg;
                }
            }
            m_Text.text += ")";
        }
    }
}
