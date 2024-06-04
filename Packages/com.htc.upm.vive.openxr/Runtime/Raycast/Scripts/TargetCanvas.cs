// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;

namespace VIVE.OpenXR.Raycast
{
    public class TargetCanvas : MonoBehaviour
    {
		const string LOG_TAG = "VIVE.OpenXR.Raycast.TargetCanvas";
		private void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + gameObject.name + ", " + msg); }

		Canvas m_Canvas = null;
		private void Awake()
		{
			m_Canvas = GetComponent<Canvas>();
		}
		private void OnEnable()
		{
			DEBUG("OnEnable()");
			if (m_Canvas != null)
			{
				DEBUG("OnEnable() RegisterTargetCanvas.");
				CanvasProvider.RegisterTargetCanvas(m_Canvas);
			}
		}
		private void OnDisable()
		{
			DEBUG("OnDisable()");
			if (m_Canvas != null)
			{
				DEBUG("OnDisable() RemoveTargetCanvas.");
				CanvasProvider.RemoveTargetCanvas(m_Canvas);
			}
		}

		Canvas[] s_ChildrenCanvas = null;
		private void Update()
		{
			Canvas[] canvases = GetComponentsInChildren<Canvas>();
			if (canvases != null && canvases.Length > 0) // find children canvas
			{
				s_ChildrenCanvas = canvases;

				for (int i = 0; i < s_ChildrenCanvas.Length; i++)
					CanvasProvider.RegisterTargetCanvas(s_ChildrenCanvas[i]);

				return;
			}
			if (s_ChildrenCanvas != null && s_ChildrenCanvas.Length > 0) // remove old children canvas
			{
				for (int i = 0; i < s_ChildrenCanvas.Length; i++)
					CanvasProvider.RemoveTargetCanvas(s_ChildrenCanvas[i]);

				s_ChildrenCanvas = null;
			}
		}
	}
}