// Copyright HTC Corporation All Rights Reserved.

using System;
using UnityEngine;

namespace VIVE.OpenXR.Raycast
{
    [DisallowMultipleComponent]
    public sealed class RaycastSwitch : MonoBehaviour
    {
        const string LOG_TAG = "VIVE.OpenXR.Raycast.RaycastSwitch";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }

        [Serializable]
        public class GazeSettings
        {
            public bool Enabled = false;
        }
        [SerializeField]
        private GazeSettings m_GazeRaycast = new GazeSettings();
        public GazeSettings GazeRaycast { get { return m_GazeRaycast; } set { m_GazeRaycast = value; } }
        public static GazeSettings Gaze { get { return Instance.GazeRaycast; } }

        [Serializable]
        public class ControllerSettings
        {
            public bool Enabled = true;
        }
        [SerializeField]
        private ControllerSettings m_ControllerRaycast = new ControllerSettings();
        public ControllerSettings ControllerRaycast { get { return m_ControllerRaycast; } set { m_ControllerRaycast = value; } }
        public static ControllerSettings Controller { get { return Instance.ControllerRaycast; } }

        [Serializable]
        public class HandSettings
        {
            public bool Enabled = true;
        }
        [SerializeField]
        private HandSettings m_HandRaycast = new HandSettings();
        public HandSettings HandRaycast { get { return m_HandRaycast; } set { m_HandRaycast = value; } }
        public static HandSettings Hand { get { return Instance.HandRaycast; } }

        private static RaycastSwitch m_Instance = null;
        public static RaycastSwitch Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    var rs = new GameObject("RaycastSwitch");
                    m_Instance = rs.AddComponent<RaycastSwitch>();
                    // This object should survive all scene transitions.
                    DontDestroyOnLoad(rs);
                }
                return m_Instance;
            }
        }

        private void Awake()
        {
            m_Instance = this;
        }
        private bool m_Enabled = false;
        private void OnEnable()
        {
            if (!m_Enabled)
            {
                DEBUG("OnEnable()");
                m_Enabled = true;
            }
        }
        private void OnDisable()
        {
            if (m_Enabled)
            {
                DEBUG("OnDisable()");
                m_Enabled = false;
            }
        }

        int printFrame = 0;
        bool printLog = false;
        private void Update()
        {
            printFrame++;
            printFrame %= 300;
            printLog = (printFrame == 0);

            CheckSettings();

            if (printLog)
            {
                DEBUG("Update() Gaze.Enabled: " + GazeRaycast.Enabled
                    + ", Controller.Enabled: " + ControllerRaycast.Enabled
                    + ", Hand.Enabled: " + HandRaycast.Enabled);
            }
        }
        /// <summary> Updates Gaze, Controller and Hand settings in runtime. </summary>
        private void CheckSettings()
        {
        }
    }
}
