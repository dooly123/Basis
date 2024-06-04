// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
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
	public class ControllerTracking : MonoBehaviour
	{
		const string LOG_TAG = "VIVE.OpenXR.Sample.OpenXRInput.ControllerTracking ";
		StringBuilder m_sb = null;
		StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Debug.Log(msg); }

		[SerializeField]
		private bool m_IsLeft = false;
		public bool IsLeft { get { return m_IsLeft; } set { m_IsLeft = value; } }

#if ENABLE_INPUT_SYSTEM
		[SerializeField]
		private bool m_UseInputAction = true;
		public bool UseInputAction { get { return m_UseInputAction; } set { m_UseInputAction = value; } }

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
#endif
		/// <summary> VIVE Left Controller Characteristics </summary>
		public const InputDeviceCharacteristics kControllerLeftCharacteristics = (
			InputDeviceCharacteristics.Left |
			InputDeviceCharacteristics.TrackedDevice |
			InputDeviceCharacteristics.Controller |
			InputDeviceCharacteristics.HeldInHand
		);
		/// <summary> VIVE Right Controller Characteristics </summary>
		public const InputDeviceCharacteristics kControllerRightCharacteristics = (
			InputDeviceCharacteristics.Right |
			InputDeviceCharacteristics.TrackedDevice |
			InputDeviceCharacteristics.Controller |
			InputDeviceCharacteristics.HeldInHand
		);

		private List<UnityEngine.XR.InputDevice> m_InputDevices = new List<UnityEngine.XR.InputDevice>();
		private bool GetIsTracked(out bool isTracked, out string msg)
		{
			isTracked = false;
			msg = "No Device";

			InputDevices.GetDevices(m_InputDevices);
			foreach (UnityEngine.XR.InputDevice id in m_InputDevices)
			{
				// The device is connected.
				if (id.characteristics.Equals((m_IsLeft ? kControllerLeftCharacteristics : kControllerRightCharacteristics)))
				{
					if (id.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool value))
					{
						isTracked = value;
						return true;
					}
					else
					{
						msg = "Get CommonUsages.isTracked failed.";
					}
				}
				else
				{
					msg = (m_IsLeft ? "No Left Controller" : "No Right Controller");
				}
			}

			return false;
		}
		private bool GetTrackingState(out InputTrackingState trackingState, out string msg)
		{
			trackingState = InputTrackingState.None;
			msg = "No Device";

			InputDevices.GetDevices(m_InputDevices);
			foreach (UnityEngine.XR.InputDevice id in m_InputDevices)
			{
				// The device is connected.
				if (id.characteristics.Equals((m_IsLeft ? kControllerLeftCharacteristics : kControllerRightCharacteristics)))
				{
					if (id.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trackingState, out InputTrackingState value))
					{
						trackingState = value;
						return true;
					}
					else
					{
						msg = "Get CommonUsages.trackingState failed.";
					}
				}
				else
				{
					msg = (m_IsLeft ? "No Left Controller" : "No Right Controller");
				}
			}

			return false;
		}
		private bool GetPosition(out Vector3 position, out string msg)
		{
			position = Vector3.zero;
			msg = "No Device";

			InputDevices.GetDevices(m_InputDevices);
			foreach (UnityEngine.XR.InputDevice id in m_InputDevices)
			{
				// The device is connected.
				if (id.characteristics.Equals((m_IsLeft ? kControllerLeftCharacteristics : kControllerRightCharacteristics)))
				{
					if (id.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 value))
					{
						position = value;
						return true;
					}
					else
					{
						msg = "Get CommonUsages.devicePosition failed.";
					}
				}
				else
				{
					msg = (m_IsLeft ? "No Left Controller" : "No Right Controller");
				}
			}

			return false;
		}
		private bool GetRotation(out Quaternion rotation, out string msg)
		{
			rotation = Quaternion.identity;
			msg = "No Device";

			InputDevices.GetDevices(m_InputDevices);
			foreach (UnityEngine.XR.InputDevice id in m_InputDevices)
			{
				// The device is connected.
				if (id.characteristics.Equals((m_IsLeft ? kControllerLeftCharacteristics : kControllerRightCharacteristics)))
				{
					if (id.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion value))
					{
						rotation = value;
						return true;
					}
					else
					{
						msg = "Get CommonUsages.deviceRotation failed.";
					}
				}
				else
				{
					msg = (m_IsLeft ? "No Left Controller" : "No Right Controller");
				}
			}

			return false;
		}

		private Text m_Text = null;
		private void Start()
		{
			m_Text = GetComponent<Text>();
		}

		void Update()
		{
			if (m_Text == null) { return; }

			m_Text.text = (m_IsLeft ? "Left Controller: " : "Right Controller: ");

#if ENABLE_INPUT_SYSTEM
			if (m_UseInputAction)
			{
				m_Text.text += "isTracked: ";
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
			else
#endif
			{
				m_Text.text += "isTracked: ";
				{
					if (GetIsTracked(out bool isTracked, out string msg))
					{
						m_Text.text += isTracked;
					}
					else
					{
						m_Text.text += msg;
					}
				}
				m_Text.text += "\n";
				m_Text.text += "trackingState: ";
				{
					if (GetTrackingState(out InputTrackingState trackingState, out string msg))
					{
						m_Text.text += trackingState;
					}
					else
					{
						m_Text.text += msg;
					}
				}
				m_Text.text += "\n";
				m_Text.text += "position (";
				{
					if (GetPosition(out Vector3 position, out string msg))
					{
						m_Text.text += position.x.ToString() + ", " + position.y.ToString() + ", " + position.z.ToString();
					}
					else
					{
						m_Text.text += msg;
					}
				}
				m_Text.text += ")\n";
				m_Text.text += "rotation (";
				{
					if (GetRotation(out Quaternion rotation, out string msg))
					{
						m_Text.text += rotation.x.ToString() + ", " + rotation.y.ToString() + ", " + rotation.z.ToString() + ", " + rotation.w.ToString();
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
}
