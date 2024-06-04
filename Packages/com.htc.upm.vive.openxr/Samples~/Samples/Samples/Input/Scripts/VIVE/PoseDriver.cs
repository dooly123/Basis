// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VIVE.OpenXR.Samples.OpenXRInput
{
    public class PoseDriver : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
		[SerializeField]
		private InputActionReference m_DevicePose = null;
		public InputActionReference DevicePose { get { return m_DevicePose; } set { m_DevicePose = value; } }
#endif

		private void Update()
		{
			if (m_DevicePose == null) { return; }

			string msg = "";
			if (Utils.GetPosePosition(m_DevicePose, out Vector3 pos, out msg))
			{
				transform.localPosition = pos;
			}
			if (Utils.GetPoseRotation(m_DevicePose, out Quaternion rot, out msg))
			{
				transform.localRotation = rot;
			}
		}
	}
}