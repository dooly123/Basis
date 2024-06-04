using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using VIVE.OpenXR;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class TrackingModeOrigin : MonoBehaviour
    {
        [SerializeField]
        Image m_RecenteredImage = null;

        [SerializeField]
        Color m_RecenteredOffColor = Color.red;

        [SerializeField]
        Color m_RecenteredColor = Color.green;

        [SerializeField]
        float m_RecenteredColorResetTime = 1.0f;

        float m_LastRecenteredTime = 0.0f;

        [SerializeField]
        TrackingOriginModeFlags m_CurrentTrackingOriginMode;
        public TrackingOriginModeFlags currentTrackingOriginMode { get { return m_CurrentTrackingOriginMode; } }

        [SerializeField]
        Text m_CurrentTrackingOriginModeDisplay = null;

        [SerializeField]
        TrackingOriginModeFlags m_DesiredTrackingOriginMode;
        public TrackingOriginModeFlags desiredTrackingOriginMode { get { return m_DesiredTrackingOriginMode; } set { m_DesiredTrackingOriginMode = value; } }

        [SerializeField]
        TrackingOriginModeFlags m_SupportedTrackingOriginModes;
        public TrackingOriginModeFlags supportedTrackingOriginModes { get { return m_SupportedTrackingOriginModes; } }

        static List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();

        /*private void OnEnable()
        {
            SubsystemManager.GetInstances(s_InputSubsystems);
            for (int i = 0; i < s_InputSubsystems.Count; i++)
            {
                s_InputSubsystems[i].trackingOriginUpdated += TrackingOriginUpdated;
            }
        }

        private void OnDisable()
        {
            SubsystemManager.GetInstances(s_InputSubsystems);
            for (int i = 0; i < s_InputSubsystems.Count; i++)
            {
                s_InputSubsystems[i].trackingOriginUpdated -= TrackingOriginUpdated;
            }
        }

        private void TrackingOriginUpdated(XRInputSubsystem obj)
        {
            Debug.Log("VIVE.OpenXR.Samples.OpenXRInput.TrackingModeOrigin TrackingOriginUpdated() " + obj != null ? obj.ToString() : "");
            m_LastRecenteredTime = Time.time;
        }*/

        public void OnDesiredSelectionChanged(int newValue)
        {
            desiredTrackingOriginMode  = (TrackingOriginModeFlags)(newValue == 0 ? 0 : (1 << (newValue - 1)));

            if (desiredTrackingOriginMode == TrackingOriginModeFlags.Device)
            {
                float value;
                if (XR_FB_display_refresh_rate.GetDisplayRefreshRate(out value) == XrResult.XR_SUCCESS)
                {
                    Debug.Log("GetDisplayRefreshRate = " + value);
                }
				XR_FB_display_refresh_rate.RequestDisplayRefreshRate(90.0f);
            }
            else if (desiredTrackingOriginMode == TrackingOriginModeFlags.Floor)
            {
                float value;
                UInt32 count;
                float[] values = new float[2];
                if (XR_FB_display_refresh_rate.GetDisplayRefreshRate(out value) == XrResult.XR_SUCCESS)
                {
                    Debug.Log("GetDisplayRefreshRate = " + value);
                }
				XR_FB_display_refresh_rate.RequestDisplayRefreshRate(75.0f);
                XrResult result = XR_FB_display_refresh_rate.EnumerateDisplayRefreshRates(displayRefreshRateCapacityInput : 0, displayRefreshRateCountOutput:out count, displayRefreshRates: out values[0]);
                if (result == XrResult.XR_SUCCESS)
                {
                    Debug.Log("EnumerateDisplayRefreshRates = " + count);
                    Array.Resize(ref values, (int)count);
                    result = XR_FB_display_refresh_rate.EnumerateDisplayRefreshRates(displayRefreshRateCapacityInput: count, displayRefreshRateCountOutput: out count, displayRefreshRates: out values[0]);
                    if (result == XrResult.XR_SUCCESS)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            Debug.Log("EnumerateDisplayRefreshRates index " + i + " RefreshRates = " + values[i]);
                        }
                    }
                        
                }
            }
        }

        private void TrackingOriginUpdated(TrackingOriginModeFlags mode)
        {
            Debug.Log("VIVE.OpenXR.Samples.OpenXRInput.TrackingModeOrigin TrackingOriginUpdated() " + mode);
            m_LastRecenteredTime = Time.time;
        }

        bool userPresence = false;
        private void CheckUserPresence()
        {
            bool presence = ClientInterface.IsUserPresence();
            if (userPresence != presence)
            {
                userPresence = presence;
                Debug.Log("VIVE.OpenXR.Samples.OpenXRInput.TrackingModeOrigin CheckUserPresence() userPresence: " + userPresence);
            }
        }

        void Update()
        {
            CheckUserPresence();

            XRInputSubsystem subsystem = null;

            SubsystemManager.GetInstances(s_InputSubsystems);
            if(s_InputSubsystems.Count > 0)
            {
                subsystem = s_InputSubsystems[0];
            }

            m_SupportedTrackingOriginModes = subsystem?.GetSupportedTrackingOriginModes() ?? TrackingOriginModeFlags.Unknown;

            if(m_CurrentTrackingOriginMode != m_DesiredTrackingOriginMode && m_DesiredTrackingOriginMode != TrackingOriginModeFlags.Unknown)
            {
                subsystem?.TrySetTrackingOriginMode(m_DesiredTrackingOriginMode);
            }
            var currMode = subsystem?.GetTrackingOriginMode() ?? TrackingOriginModeFlags.Unknown;
            if (m_CurrentTrackingOriginMode != currMode)
            {
                m_CurrentTrackingOriginMode = currMode;
                TrackingOriginUpdated(m_CurrentTrackingOriginMode);
            }

            if (m_CurrentTrackingOriginModeDisplay != null)
                m_CurrentTrackingOriginModeDisplay.text = m_CurrentTrackingOriginMode.ToString();

            if(m_RecenteredImage != null)
            {
                float lerp = (Time.time - m_LastRecenteredTime) / m_RecenteredColorResetTime;
                lerp = Mathf.Clamp(lerp, 0.0f, 1.0f);
                m_RecenteredImage.color = Color.Lerp(m_RecenteredColor, m_RecenteredOffColor, lerp);
            }
        }
    }
}
