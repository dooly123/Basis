// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace VIVE.OpenXR.Samples
{
    public class HideObjectsWhenUntracked : MonoBehaviour
    {
        const string LOG_TAG = "VIVE.OpenXR.Samples.HideObjectsWhenUntracked ";
        StringBuilder m_sb = null;
        StringBuilder sb {
            get {
                if (m_sb == null) { m_sb = new StringBuilder(); }
                return m_sb;
            }
        }
        void DEBUG(StringBuilder msg) { Debug.Log(msg); }

        [Serializable]
        public class ObjectInfo
		{
            public uint DeviceIndex = 0;
            public InputActionReference IsActive;
            public InputActionReference TrackingState;
            public GameObject ObjectToHide;
        }

        [SerializeField]
        public List<ObjectInfo> m_ObjectInfos = new List<ObjectInfo>();
        public List<ObjectInfo> ObjectInfos { get { return m_ObjectInfos; } set { m_ObjectInfos = value; } }

        int printFrame = 0;
        protected bool printIntervalLog = false;

        private void Update()
        {
            printFrame++;
            printFrame %= 300;
            printIntervalLog = (printFrame == 0);

            if (m_ObjectInfos == null) { return; }

            string errMsg = "";

            for (int i = 0; i < m_ObjectInfos.Count; i++)
            {
                bool isActive = false;
                int trackingState = 0;
                bool positionTracked = false, rotationTracked = false;

                // isActive
                if (OpenXRHelper.VALIDATE(m_ObjectInfos[i].IsActive, out errMsg))
                {
                    if (m_ObjectInfos[i].IsActive.action.activeControl.valueType == typeof(float))
                        isActive = m_ObjectInfos[i].IsActive.action.ReadValue<float>() > 0;
                    if (m_ObjectInfos[i].IsActive.action.activeControl.valueType == typeof(bool))
                        isActive = m_ObjectInfos[i].IsActive.action.ReadValue<bool>();
                }
                else
                {
                    if (printIntervalLog)
                    {
                        sb.Clear().Append(LOG_TAG).Append(m_ObjectInfos[i].DeviceIndex)
                            .Append(" Update() ").Append(m_ObjectInfos[i].IsActive.action.name).Append(", ").Append(errMsg);
                        DEBUG(sb);
                    }
                }

                // trackingState
                if (OpenXRHelper.VALIDATE(m_ObjectInfos[i].TrackingState, out errMsg))
                {
                    trackingState = m_ObjectInfos[i].TrackingState.action.ReadValue<int>();
                }
                else
                {
                    if (printIntervalLog)
                    {
                        sb.Clear().Append(LOG_TAG).Append(m_ObjectInfos[i].DeviceIndex)
                            .Append(" Update() ").Append(m_ObjectInfos[i].TrackingState.action.name).Append(", ").Append(errMsg);
                        DEBUG(sb);
                    }
                }

                if (printIntervalLog)
                {
                    sb.Clear().Append(LOG_TAG).Append(m_ObjectInfos[i].DeviceIndex)
                        .Append("Update() isActive: ").Append(isActive).Append(", trackingState: ").Append(trackingState);
                    DEBUG(sb);
                }

                positionTracked = ((uint)trackingState & (uint)InputTrackingState.Position) != 0;
                rotationTracked = ((uint)trackingState & (uint)InputTrackingState.Rotation) != 0;

                bool tracked = isActive /*&& positionTracked */&& rotationTracked; // Show the object with 3DoF.
                m_ObjectInfos[i].ObjectToHide.SetActive(tracked);
            }
        }
    }
}
