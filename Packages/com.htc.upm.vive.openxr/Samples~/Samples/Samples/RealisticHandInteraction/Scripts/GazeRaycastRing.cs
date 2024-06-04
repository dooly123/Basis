// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Wave.Generic.Sample
{
	public class GazeRaycastRing : RaycastRing
	{
		const string LOG_TAG = "Wave.Generic.Sample.GazeRaycastRing";
		void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
		void INTERVAL(string msg) { if (printIntervalLog) { DEBUG(msg); } }

		[Serializable]
		public class ButtonOption
		{
			[SerializeField]
			private bool m_Primary2DAxisClick = false;
			public bool Primary2DAxisClick
			{
				get { return m_Primary2DAxisClick; }
				set
				{
					if (m_Primary2DAxisClick != value) { Update(); }
					m_Primary2DAxisClick = value;
				}
			}
			[SerializeField]
			private bool m_TriggerButton = true;
			public bool TriggerButton
			{
				get { return m_TriggerButton; }
				set
				{
					if (m_TriggerButton != value) { Update(); }
					m_TriggerButton = value;
				}
			}

			private List<InputFeatureUsage<bool>> m_OptionList = new List<InputFeatureUsage<bool>>();
			public List<InputFeatureUsage<bool>> OptionList { get { return m_OptionList; } }

			[HideInInspector]
			public List<bool> State = new List<bool>(), StateEx = new List<bool>();
			public void Update()
			{
				m_OptionList.Clear();
				State.Clear();
				StateEx.Clear();
				if (m_Primary2DAxisClick)
				{
					m_OptionList.Add(UnityEngine.XR.CommonUsages.primary2DAxisClick);
					State.Add(false);
					StateEx.Add(false);
				}
				if (m_TriggerButton)
				{
					m_OptionList.Add(UnityEngine.XR.CommonUsages.triggerButton);
					State.Add(false);
					StateEx.Add(false);
				}
			}
		}

		#region Inspector
		[Tooltip("Event triggered by gaze.")]
		[SerializeField]
		private GazeEvent m_InputEvent = GazeEvent.Down;
		public GazeEvent InputEvent { get { return m_InputEvent; } set { m_InputEvent = value; } }

		[SerializeField]
		private ButtonOption m_ControlKey = new ButtonOption();
		public ButtonOption ControlKey { get { return m_ControlKey; } set { m_ControlKey = value; } }

#if ENABLE_INPUT_SYSTEM
		[SerializeField]
		private bool m_UseInputAction = false;
		public bool UseInputAction { get { return m_UseInputAction; } set { m_UseInputAction = value; } }

		[SerializeField]
		private InputActionProperty m_RotationInput;
		public InputActionProperty RotationInput
		{
			get => m_RotationInput;
			set => m_RotationInput = value;
		}
		public static bool VALIDATE(InputActionProperty actionReference, out string msg)
		{
			msg = "Normal";

			if (actionReference == null)
			{
				msg = "Null reference.";
				return false;
			}
			else if (actionReference.action == null)
			{
				msg = "Null reference action.";
				return false;
			}

			return true;
		}
#endif

		[SerializeField]
		private bool m_AlwaysEnable = false;
		public bool AlwaysEnable { get { return m_AlwaysEnable; } set { m_AlwaysEnable = value; } }
		#endregion

		#region MonoBehaviour overrides
		protected override void Awake()
		{
			base.Awake();

			m_ControlKey.Update();
			for (int i = 0; i < m_ControlKey.OptionList.Count; i++)
			{
				DEBUG("Awake() m_ControlKey[" + i + "] = " + m_ControlKey.OptionList[i].name);
			}
		}

		private bool m_KeyDown = false;
		protected override void Update()
		{
			base.Update();

			m_KeyDown = ButtonPressed();

			INTERVAL("Update() m_InputEvent: " + m_InputEvent
				+ ", m_AlwaysEnable: " + m_AlwaysEnable
				+ ", m_ControlKey.Primary2DAxisClick: " + m_ControlKey.Primary2DAxisClick
				+ ", m_ControlKey.TriggerButton: " + m_ControlKey.TriggerButton
				);
		}
		#endregion

		internal static List<UnityEngine.XR.InputDevice> m_InputDevices = new List<UnityEngine.XR.InputDevice>();
		/// <summary> Wave Left Controller Characteristics </summary>
		const InputDeviceCharacteristics kControllerLeftCharacteristics = (
			InputDeviceCharacteristics.Left |
			InputDeviceCharacteristics.TrackedDevice |
			InputDeviceCharacteristics.Controller |
			InputDeviceCharacteristics.HeldInHand
		);
		/// <summary> Wave Right Controller Characteristics </summary>
		const InputDeviceCharacteristics kControllerRightCharacteristics = (
			InputDeviceCharacteristics.Right |
			InputDeviceCharacteristics.TrackedDevice |
			InputDeviceCharacteristics.Controller |
			InputDeviceCharacteristics.HeldInHand
		);
		internal bool KeyDown(InputDeviceCharacteristics device, InputFeatureUsage<bool> button)
		{
			bool isDown = false;

			InputDevices.GetDevices(m_InputDevices);
			foreach (UnityEngine.XR.InputDevice id in m_InputDevices)
			{
				// The device is connected.
				if (id.characteristics.Equals(device))
				{
					if (id.TryGetFeatureValue(button, out bool value))
						isDown = value;
				}
			}

			return isDown;
		}
		private bool ButtonPressed()
		{
			bool down = false;

			for (int i = 0; i < m_ControlKey.OptionList.Count; i++)
			{
				m_ControlKey.StateEx[i] = m_ControlKey.State[i];
				m_ControlKey.State[i] =
					KeyDown(kControllerLeftCharacteristics, m_ControlKey.OptionList[i]) ||
					KeyDown(kControllerRightCharacteristics, m_ControlKey.OptionList[i]);

				down |= (m_ControlKey.State[i] && !m_ControlKey.StateEx[i]);
			}

			return down;
		}

		#region RaycastImpl Actions overrides
		protected override bool OnDown()
		{
			if (m_InputEvent != GazeEvent.Down) { return false; }

			bool down = false;
			if (m_RingPercent >= 100 || m_KeyDown)
			{
				m_RingPercent = 0;
				m_GazeOnTime = Time.unscaledTime;
				down = true;
				DEBUG("OnDown()");
			}

			return down;
		}
		protected override bool OnSubmit()
		{
			if (m_InputEvent != GazeEvent.Submit) { return false; }

			bool submit = false;
			if (m_RingPercent >= 100 || m_KeyDown)
			{
				m_RingPercent = 0;
				m_GazeOnTime = Time.unscaledTime;
				submit = true;
				DEBUG("OnSubmit()");
			}

			return submit;
		}
		#endregion
	}
}
