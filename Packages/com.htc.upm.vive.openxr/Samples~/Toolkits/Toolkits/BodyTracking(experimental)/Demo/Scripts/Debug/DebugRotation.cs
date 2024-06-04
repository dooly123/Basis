// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;

namespace VIVE.OpenXR.Toolkits.BodyTracking.Demo
{
	public class DebugRotation : MonoBehaviour
	{
		public Vector4 Rot;
		public void Rotate()
		{
			transform.rotation = new Quaternion(Rot.x, Rot.y, Rot.z, Rot.w);
			Debug.Log("VIVE.OpenXR.Toolkits.BodyTracking.Demo.DebugRotation " + gameObject.name
				+ " rotation ("
				+ transform.rotation.eulerAngles.x.ToString() + ", "
				+ transform.rotation.eulerAngles.y.ToString() + ", "
				+ transform.rotation.eulerAngles.z.ToString() + ")");
		}
	}
}
