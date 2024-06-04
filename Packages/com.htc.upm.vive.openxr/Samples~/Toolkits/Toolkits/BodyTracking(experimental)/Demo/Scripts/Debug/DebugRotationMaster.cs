// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace VIVE.OpenXR.Toolkits.BodyTracking.Demo
{
	public class DebugRotationMaster : MonoBehaviour
	{
		public List<DebugRotation> DRs;
		void Start()
		{
			for (int i = 0; i < DRs.Count; i++)
			{
				DRs[i].Rotate();
			}
		}
	}
}
