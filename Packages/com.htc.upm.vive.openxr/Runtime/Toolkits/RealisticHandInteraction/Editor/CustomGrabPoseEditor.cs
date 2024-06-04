// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
	[CustomEditor(typeof(CustomGrabPose))]
	public class CustomGrabPoseEditor : UnityEditor.Editor
	{
		private CustomGrabPose m_GrabPoseDesigner;

		private void Awake()
		{
			m_GrabPoseDesigner = target as CustomGrabPose;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			GUILayout.Space(10f);
			ShowGrabPosesMenu();
		}

		private void ShowGrabPosesMenu()
		{
			if (GUILayout.Button("Save HandGrab Pose"))
			{
				m_GrabPoseDesigner.FindNearInteractable();
				m_GrabPoseDesigner.SavePoseWithCandidate();
			}
		}
	}
}
#endif
