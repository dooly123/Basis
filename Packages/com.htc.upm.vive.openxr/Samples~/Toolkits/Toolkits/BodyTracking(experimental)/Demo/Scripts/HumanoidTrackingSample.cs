// Copyright HTC Corporation All Rights Reserved.

using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace VIVE.OpenXR.Toolkits.BodyTracking.Demo
{
	public class HumanoidTrackingSample : MonoBehaviour
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.Demo.HumanoidTrackingSample";
		private StringBuilder m_sb = null;
		private StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Debug.Log(LOG_TAG + " " + msg); }

		public HumanoidTracking humanoidTracking = null;
		public Button beginBtn = null;
		public Text canvasTitle = null;

		private void Update()
		{
			if (humanoidTracking == null || canvasTitle == null) { return; }

			canvasTitle.text = humanoidTracking.Tracking + "\n" + "Manually Tracking";
		}

		public void SetArmMode()
		{
			if (humanoidTracking != null)
				humanoidTracking.Tracking = HumanoidTracking.TrackingMode.Arm;
		}
		public void SetUpperMode()
		{
			if (humanoidTracking != null)
				humanoidTracking.Tracking = HumanoidTracking.TrackingMode.UpperBody;
		}
		public void SetFullMode()
		{
			if (humanoidTracking != null)
				humanoidTracking.Tracking = HumanoidTracking.TrackingMode.FullBody;
		}
		public void SetUpperBodyAndLegMode()
		{
			if (humanoidTracking != null)
				humanoidTracking.Tracking = HumanoidTracking.TrackingMode.UpperBodyAndLeg;
		}	

		public void BeginTracking()
		{
			if (humanoidTracking != null)
			{
				if (beginBtn != null) { beginBtn.interactable = false; }
				humanoidTracking.BeginTracking();
			}
		}
		public void EndTracking()
		{
			if (humanoidTracking != null)
			{
				if (beginBtn != null) { beginBtn.interactable = true; }
				humanoidTracking.StopTracking();
			}
		}
	}
}
