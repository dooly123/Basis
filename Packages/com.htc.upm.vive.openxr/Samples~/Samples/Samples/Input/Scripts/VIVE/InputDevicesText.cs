// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace VIVE.OpenXR.Samples.OpenXRInput
{
    [RequireComponent(typeof(Text))]
    public class InputDevicesText : MonoBehaviour
    {
		public bool useXrNode = false;
		public bool useRole = false;
		public int StartIndex = 0, EndIndex = 0;

		private Text m_Text = null;
		private void Awake()
		{
			m_Text = GetComponent<Text>();
		}

		private readonly XRNode[] s_XrNodes = new XRNode[]
		{
			XRNode.LeftEye, // 0
			XRNode.RightEye,
			XRNode.CenterEye,
			XRNode.Head,
			XRNode.LeftHand,
			XRNode.RightHand, // 5
			XRNode.TrackingReference,
			XRNode.HardwareTracker,
		};
		private readonly InputDeviceRole[] s_DeviceRoles = new InputDeviceRole[]
		{
			InputDeviceRole.Generic, // 0
			InputDeviceRole.LeftHanded,
			InputDeviceRole.RightHanded,
			InputDeviceRole.GameController,
			InputDeviceRole.TrackingReference,
			InputDeviceRole.HardwareTracker, // 5
			InputDeviceRole.LegacyController,
		};
		private readonly InputDeviceCharacteristics[] s_Characteristics = new InputDeviceCharacteristics[]
		{
			InputDeviceCharacteristics.HeadMounted, // 0
			InputDeviceCharacteristics.Camera,
			InputDeviceCharacteristics.HeldInHand,
			InputDeviceCharacteristics.HandTracking,
			InputDeviceCharacteristics.EyeTracking,
			InputDeviceCharacteristics.TrackedDevice, // 5
			InputDeviceCharacteristics.Controller,
			InputDeviceCharacteristics.TrackingReference,
			InputDeviceCharacteristics.Left,
			InputDeviceCharacteristics.Right,
			InputDeviceCharacteristics.Simulated6DOF,
		};

		internal static List<InputDevice> s_InputDevicesXrNode = new List<InputDevice>();
		internal static List<InputDevice> s_InputDevices = new List<InputDevice>();
		internal static List<InputDevice> s_InputDevicesRole = new List<InputDevice>();
		private void Update()
		{
			if (m_Text == null) { return; }

			if (useXrNode)
			{
				m_Text.text = "";
				//for (int node = 0; node < s_XrNodes.Length; node++)
				{
					InputDevices.GetDevicesAtXRNode(s_XrNodes[7], s_InputDevicesXrNode);
					m_Text.text += "Node " + s_XrNodes[7] + " (" + s_InputDevicesXrNode.Count + "):\n";
					for (int i = 0; i < s_InputDevicesXrNode.Count; i++)
					{
						m_Text.text += "\n" + i + "." + s_InputDevicesXrNode[i].name;
						m_Text.text += "\n  characteristics: " + s_InputDevicesXrNode[i].characteristics;
						m_Text.text += "\n  serialNumber: " + s_InputDevicesXrNode[i].serialNumber;
						m_Text.text += "\n";
					}
					m_Text.text += "\n";
				}
				return;
			}

			if (useRole)
			{
				m_Text.text = "";
				//for (int role = 0; role < s_DeviceRoles.Length; role++)
				{
#pragma warning disable
					InputDevices.GetDevicesWithRole(s_DeviceRoles[5], s_InputDevicesRole);
#pragma warning enable
					//InputDevices.GetDevicesWithCharacteristics(s_Characteristics[5], s_InputDevicesRole);
					m_Text.text += "Role " + s_DeviceRoles[5] + " (" + s_InputDevicesRole.Count + "):\n";
					for (int i = 0; i < s_InputDevicesRole.Count; i++)
					{
						m_Text.text += "\n" + i + "." + s_InputDevicesRole[i].name;
						m_Text.text += "\n  characteristics: " + s_InputDevicesRole[i].characteristics;
						m_Text.text += "\n  serialNumber: " + s_InputDevicesRole[i].serialNumber;
						m_Text.text += "\n";
					}
					m_Text.text += "\n";
				}
				return;
			}

			InputDevices.GetDevices(s_InputDevices);
			m_Text.text = "Input Devices (" + s_InputDevices.Count + "):\n";
			for (int i = StartIndex; i < s_InputDevices.Count && i <= EndIndex; i++)
			{
				m_Text.text += "\n" + i + "." + s_InputDevices[i].name;
				m_Text.text += "\n  characteristics: " + s_InputDevices[i].characteristics;
				m_Text.text += "\n  serialNumber: " + s_InputDevices[i].serialNumber;
				m_Text.text += "\n";
			}
		}
	}
}