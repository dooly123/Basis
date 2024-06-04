// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace VIVE.OpenXR.Raycast
{
    public class ControllerRaycastPointer : RaycastPointer
    {
        const string LOG_TAG = "VIVE.OpenXR.Raycast.ControllerRaycastPointer";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }

        #region Inspector
        [SerializeField]
        private InputActionReference m_IsTracked = null;
        public InputActionReference IsTracked { get => m_IsTracked; set => m_IsTracked = value; }

        [Tooltip("Keys for control.")]
        [SerializeField]
        private List<InputActionReference> m_ActionsKeys = new List<InputActionReference>();
        public List<InputActionReference> ActionKeys { get { return m_ActionsKeys; } set { m_ActionsKeys = value; } }
        bool getBool(InputActionReference actionReference)
        {
            if (OpenXRHelper.VALIDATE(actionReference, out string value))
            {
                if (actionReference.action.activeControl.valueType == typeof(bool))
                    return actionReference.action.ReadValue<bool>();
                if (actionReference.action.activeControl.valueType == typeof(float))
                    return actionReference.action.ReadValue<float>() > 0;
            }

            return false;
        }

        [Tooltip("To show the ray anymore.")]
        [SerializeField]
        private bool m_AlwaysEnable = false;
        public bool AlwaysEnable { get { return m_AlwaysEnable; } set { m_AlwaysEnable = value; } }
        #endregion

        #region MonoBehaviour overrides
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void Update()
        {
            base.Update();

            if (!IsInteractable()) { return; }

            UpdateButtonStates();
        }
        protected override void Start()
        {
            base.Start();

            DEBUG("Start()");
        }
        private void OnApplicationPause(bool pause)
        {
            DEBUG("OnApplicationPause() " + pause);
        }
        #endregion

        private bool IsInteractable()
        {
            bool enabled = RaycastSwitch.Controller.Enabled;
            bool validPose = getBool(m_IsTracked);

            m_Interactable = (m_AlwaysEnable || enabled) && validPose;

            if (printIntervalLog)
            {
                DEBUG("IsInteractable() enabled: " + enabled
                    + ", validPose: " + validPose
                    + ", m_AlwaysEnable: " + m_AlwaysEnable
                    + ", m_Interactable: " + m_Interactable);
            }

            return m_Interactable;
        }

        private void UpdateButtonStates()
        {
            if (m_ActionsKeys == null) { return; }

            down = false;
            for (int i = 0; i < m_ActionsKeys.Count; i++)
            {
                if (!hold)
                {
                    down |= getBool(m_ActionsKeys[i]);
                }
            }

            hold = false;
            for (int i = 0; i < m_ActionsKeys.Count; i++)
            {
                hold |= getBool(m_ActionsKeys[i]);
            }
        }

        #region RaycastImpl Actions overrides
        internal bool down = false, hold = false;
        protected override bool OnDown()
        {
            return down;
        }
        protected override bool OnHold()
        {
            return hold;
        }
        #endregion
    }
}
