// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace VIVE.OpenXR.Raycast
{
    public static class CanvasProvider
    {
		const string LOG_TAG = "VIVE.OpenXR.Raycast.CanvasProvider";
		private static void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }

		private static List<Canvas> s_TargetCanvases = new List<Canvas>();

		public static bool RegisterTargetCanvas(Canvas canvas)
		{
			if (canvas != null && !s_TargetCanvases.Contains(canvas))
			{
				DEBUG("RegisterTargetCanvas() " + canvas.gameObject.name);
				s_TargetCanvases.Add(canvas);
				return true;
			}

			return false;
		}
		public static bool RemoveTargetCanvas(Canvas canvas)
		{
			if (canvas != null && s_TargetCanvases.Contains(canvas))
			{
				DEBUG("RemoveTargetCanvas() " + canvas.gameObject.name);
				s_TargetCanvases.Remove(canvas);
				return true;
			}

			return false;
		}
		public static Canvas[] GetTargetCanvas() { return s_TargetCanvases.ToArray(); }
	}
}