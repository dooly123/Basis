using System;
using System.Collections.Generic;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class AutomaticTrackingModeChanger : MonoBehaviour
    {
        [SerializeField]
        float m_ChangeInterval = 5.0f;

        private float m_TimeRemainingTillChange;

        static List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();
        static List<TrackingOriginModeFlags> s_SupportedTrackingOriginModes = new List<TrackingOriginModeFlags>();

        void OnEnable()
        {
            m_TimeRemainingTillChange = m_ChangeInterval;
        }

        void Update()
        {
            m_TimeRemainingTillChange -= Time.deltaTime;
            if (m_TimeRemainingTillChange <= 0.0f)
            {
                List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
                SubsystemManager.GetInstances(inputSubsystems);
                XRInputSubsystem subsystem = inputSubsystems?[0];
                if (subsystem != null)
                {
                    UpdateSupportedTrackingOriginModes(subsystem);
                    SetToNextMode(subsystem);
                }
                m_TimeRemainingTillChange += m_ChangeInterval;
            }
        }

        void UpdateSupportedTrackingOriginModes(XRInputSubsystem subsystem)
        {
            TrackingOriginModeFlags supportedOriginModes = subsystem.GetSupportedTrackingOriginModes();
            s_SupportedTrackingOriginModes.Clear();
            for (int i = 0; i < 31; i++)
            {
                uint modeToCheck = 1u << i;
                if ((modeToCheck & ((UInt32)supportedOriginModes)) != 0)
                {
                    s_SupportedTrackingOriginModes.Add((TrackingOriginModeFlags)modeToCheck);
                }
            }
        }

        void SetToNextMode(XRInputSubsystem subsystem)
        {
            TrackingOriginModeFlags currentOriginMode = subsystem.GetTrackingOriginMode();
            for (int i = 0; i < s_SupportedTrackingOriginModes.Count; i++)
            {
                if (currentOriginMode == s_SupportedTrackingOriginModes[i])
                {
                    int nextModeIndex = (i + 1) % s_SupportedTrackingOriginModes.Count;
                    subsystem.TrySetTrackingOriginMode(s_SupportedTrackingOriginModes[nextModeIndex]);
                    break;
                }
            }
        }
    }
}
