// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Text;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VIVE.OpenXR
{
	[DisallowMultipleComponent]
	public sealed class VIVERig : MonoBehaviour
	{
		const string LOG_TAG = "VIVE.OpenXR.VIVERig ";
		StringBuilder m_sb = null;
		StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Debug.Log(msg); }

		[SerializeField]
		private GameObject m_CameraOffset = null;
		public GameObject CameraOffset { get { return m_CameraOffset; } set { m_CameraOffset = value; } }

		[SerializeField]
		private GameObject m_CameraObject = null;
		[System.Obsolete("No Used")]
		public GameObject CameraObject { get { return m_CameraObject; } set { m_CameraObject = value; } }

		private TrackingOriginModeFlags m_TrackingOriginEx = TrackingOriginModeFlags.Device;
		[SerializeField]
		private TrackingOriginModeFlags m_TrackingOrigin = TrackingOriginModeFlags.Device;
		public TrackingOriginModeFlags TrackingOrigin { get { return m_TrackingOrigin; } set { m_TrackingOrigin = value; } }

		private Vector3 cameraPosOffset = Vector3.zero;
		[SerializeField]
		private float m_CameraYOffset = 1;
		public float CameraYOffset { get { return m_CameraYOffset; } set { m_CameraYOffset = value; } }

#if ENABLE_INPUT_SYSTEM
		[SerializeField]
		private InputActionAsset m_ActionAsset;
		public InputActionAsset actionAsset { get => m_ActionAsset; set => m_ActionAsset = value; }
#endif

		static List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();
		private void OnEnable()
		{
			SubsystemManager.GetSubsystems(s_InputSubsystems);
			for (int i = 0; i < s_InputSubsystems.Count; i++)
			{
				s_InputSubsystems[i].trackingOriginUpdated += TrackingOriginUpdated;
			}

#if ENABLE_INPUT_SYSTEM
			if (m_ActionAsset != null)
			{
				m_ActionAsset.Enable();
			}
#endif
		}
		private void OnDisable()
		{
			SubsystemManager.GetSubsystems(s_InputSubsystems);
			for (int i = 0; i < s_InputSubsystems.Count; i++)
			{
				s_InputSubsystems[i].trackingOriginUpdated -= TrackingOriginUpdated;
			}
		}

		float m_LastRecenteredTime = 0.0f;
		private void TrackingOriginUpdated(XRInputSubsystem obj)
		{
			m_LastRecenteredTime = Time.time;
			sb.Clear().Append(LOG_TAG).Append("TrackingOriginUpdated() m_LastRecenteredTime: ").Append(m_LastRecenteredTime); DEBUG(sb);
		}

		XRInputSubsystem m_InputSystem = null;
		void UpdateInputSystem()
		{
			SubsystemManager.GetSubsystems(s_InputSubsystems);
			if (s_InputSubsystems.Count > 0)
			{
				m_InputSystem = s_InputSubsystems[0];
			}
		}
		private void Awake()
		{
			UpdateInputSystem();
			if (m_InputSystem != null)
			{
				m_InputSystem.TrySetTrackingOriginMode(m_TrackingOrigin);

				TrackingOriginModeFlags mode = m_InputSystem.GetTrackingOriginMode();
				sb.Clear().Append(LOG_TAG).Append("Awake() Tracking mode is set to ").Append(mode); DEBUG(sb);
			}
			else
			{
				sb.Clear().Append(LOG_TAG).Append("Awake() no XRInputSubsystem."); DEBUG(sb);
			}
			m_TrackingOriginEx = m_TrackingOrigin;
		}

		private void Update()
		{
			UpdateInputSystem();
			if (m_InputSystem != null)
			{
				TrackingOriginModeFlags mode = m_InputSystem.GetTrackingOriginMode();
				if ((mode != m_TrackingOrigin || m_TrackingOriginEx != m_TrackingOrigin) && m_TrackingOrigin != TrackingOriginModeFlags.Unknown)
				{
					m_InputSystem.TrySetTrackingOriginMode(m_TrackingOrigin);

					mode = m_InputSystem.GetTrackingOriginMode();
					sb.Clear().Append(LOG_TAG).Append("Update() Tracking mode is set to " + mode);
					m_TrackingOriginEx = m_TrackingOrigin;
				}
			}

			if (m_CameraOffset != null)
			{
				cameraPosOffset.x = m_CameraOffset.transform.localPosition.x;
				cameraPosOffset.y = m_CameraYOffset;
				cameraPosOffset.z = m_CameraOffset.transform.localPosition.z;

				m_CameraOffset.transform.localPosition = cameraPosOffset;
			}
		}
	}
}
