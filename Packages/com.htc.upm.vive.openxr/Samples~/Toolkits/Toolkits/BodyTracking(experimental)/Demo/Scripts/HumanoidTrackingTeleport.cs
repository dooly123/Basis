// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;

namespace VIVE.OpenXR.Toolkits.BodyTracking.Demo
{
	public class HumanoidTrackingTeleport : MonoBehaviour
	{
		public HumanoidTracking humanoidTracking = null;
		public Transform offsetOrigin = null;
		public Transform offset1 = null;
		public Transform offset2 = null;
		public Transform offset3 = null;

		public void TeleportOrigin()
		{
			if (humanoidTracking != null && offsetOrigin != null)
				humanoidTracking.AvatarOffset = offsetOrigin;
		}
		public void Teleport1()
		{
			if (humanoidTracking != null && offset1 != null)
				humanoidTracking.AvatarOffset = offset1;
		}
		public void Teleport2()
		{
			if (humanoidTracking != null && offset2 != null)
				humanoidTracking.AvatarOffset = offset2;
		}
		public void Teleport3()
		{
			if (humanoidTracking != null && offset3 != null)
				humanoidTracking.AvatarOffset = offset3;
		}
	}
}
