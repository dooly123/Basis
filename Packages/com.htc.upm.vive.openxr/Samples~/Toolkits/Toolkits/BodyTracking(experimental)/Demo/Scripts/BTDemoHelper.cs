// Copyright HTC Corporation All Rights Reserved.

using System;

namespace VIVE.OpenXR.Toolkits.BodyTracking.Demo
{
	public static class BTDemoHelper
	{
		public enum TrackingMode : UInt32
		{
			Arm = BodyTrackingMode.ARMIK,
			UpperBody = BodyTrackingMode.UPPERBODYIK,
			FullBody = BodyTrackingMode.FULLBODYIK,
			UpperBodyAndLeg = BodyTrackingMode.UPPERIKANDLEGFK,
		}

		public static string Name(this TrackingMode mode)
		{
			if (mode == TrackingMode.Arm) { return "Arm"; }
			if (mode == TrackingMode.FullBody) { return "FullBody"; }
			if (mode == TrackingMode.UpperBody) { return "UpperBody"; }
			if (mode == TrackingMode.UpperBodyAndLeg) { return "UpperLeg"; }

			return "";
		}
	}
}
