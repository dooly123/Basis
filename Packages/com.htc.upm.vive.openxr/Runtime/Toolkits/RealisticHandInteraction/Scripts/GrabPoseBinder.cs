// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
#if UNITY_EDITOR
	/// <summary>
	/// The class is designed to record all grab poses for all hand Grabbables.
	/// </summary>
	public class GrabPoseBinder : ScriptableObject
	{
		/// <summary>
		/// This struct records the grab pose for grabbable object.
		/// </summary>
		[Serializable]
		private struct GrabPoseBindFormat
		{
			[SerializeField]
			public string grabbableName;
			[SerializeField]
			public List<GrabPose> grabPoses;

			public GrabPoseBindFormat(string in_GrabbableName, List<GrabPose> in_GrabPoses)
			{
				grabbableName = in_GrabbableName;
				grabPoses = in_GrabPoses;
			}

			public GrabPoseBindFormat Identity => new GrabPoseBindFormat(string.Empty, new List<GrabPose>());

			public void Update(List<GrabPose> grabPoses)
			{
				this.grabPoses.Clear();
				this.grabPoses.AddRange(grabPoses);
			}

			public void Reset()
			{
				grabbableName = string.Empty;
				grabPoses.Clear();
			}

			public override bool Equals(object obj)
			{
				return obj is GrabPoseBindFormat grabPoseBindFormat &&
					   grabbableName == grabPoseBindFormat.grabbableName &&
					   grabPoses == grabPoseBindFormat.grabPoses;
			}
			public override int GetHashCode()
			{
				return grabbableName.GetHashCode() ^ grabPoses.GetHashCode();
			}
			public static bool operator ==(GrabPoseBindFormat source, GrabPoseBindFormat target) => source.Equals(target);
			public static bool operator !=(GrabPoseBindFormat source, GrabPoseBindFormat target) => !(source == target);
		}

		[SerializeField]
		private List<GrabPoseBindFormat> m_BindingInfos = new List<GrabPoseBindFormat>();

		/// <summary>
		/// Update the binding information for each hand grabbable object.
		/// </summary>
		public void UpdateBindingInfos()
		{
			m_BindingInfos.Clear();
			foreach (HandGrabInteractable grabbable in GrabManager.handGrabbables)
			{
				m_BindingInfos.Add(new GrabPoseBindFormat(grabbable.name, grabbable.grabPoses));
			}
		}

		/// <summary>
		/// Stores the binding information.
		/// </summary>
		/// <returns>True if storage is successful; otherwise, false.</returns>
		public bool StorageData()
		{
			if (m_BindingInfos.Count == 0) { return false; }

			EditorApplication.delayCall += () =>
			{
				AssetDatabase.Refresh();
				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssets();
			};
			return true;
		}

		/// <summary>
		/// Finds grab poses associated with the specified hand grabbable object.
		/// </summary>
		/// <param name="grabbable">The hand grabbable object to search for.</param>
		/// <param name="grabPoses">The output parameter to store the found grab poses.</param>
		/// <returns>True if grab poses are found for the grabbable object; otherwise, false.</returns>
		public bool FindGrabPosesWithGrabbable(HandGrabInteractable grabbable, out List<GrabPose> grabPoses)
		{
			grabPoses = new List<GrabPose>();
			GrabPoseBindFormat bindingInfo = m_BindingInfos.Find(x => x.grabbableName == grabbable.name);
			if (bindingInfo != null)
			{
				grabPoses = bindingInfo.grabPoses;
				return true;
			}
			return false;
		}
	}
#endif
}
