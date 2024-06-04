// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace VIVE.OpenXR.Toolkits.BodyTracking.Demo
{
	public class BodyTrackingMenu : MonoBehaviour
	{
		public BodyTrackingSample ikScript = null;
		public Text canvasTitle = null;
		public Button beginBtn = null;

		private void Update()
		{
			if (ikScript != null && canvasTitle != null)
			{
				string autoUpdateText = "Manually Tracking";
				canvasTitle.text = ikScript.TrackingMode.Name() + "\n" + autoUpdateText;
			}
		}

		public void SetArmMode()
		{
			if (ikScript != null)
				ikScript.SetArmMode();
		}
		public void SetUpperMode()
		{
			if (ikScript != null)
				ikScript.SetUpperMode();
		}
		public void SetFullMode()
		{
			if (ikScript != null)
				ikScript.SetFullMode();
		}
		public void SetUpperBodyAndLegMode()
		{
			if (ikScript != null)
				ikScript.SetUpperBodyAndLegMode();
		}
		public void BeginTracking()
		{
			if (ikScript != null)
			{
				if (beginBtn != null) { beginBtn.interactable = false; }
				ikScript.BeginTracking();
			}
		}
		public void StopTracking()
		{
			if (ikScript != null)
			{
				if (beginBtn != null) { beginBtn.interactable = true; }
				ikScript.StopTracking();
			}
		}
	}
}
