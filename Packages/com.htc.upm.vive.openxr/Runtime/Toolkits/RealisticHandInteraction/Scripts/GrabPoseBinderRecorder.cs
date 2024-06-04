// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
#if UNITY_EDITOR
	/// <summary>
	/// The class is designed to update the grab poses of all hand Grabbables.
	/// </summary>
	[InitializeOnLoad]
	public class GrabbablePoseRecorder
	{
		private static readonly string filepath = "Assets/GrabablePoseRecording.asset";
		private static readonly string metaFilepath = filepath + ".meta";
		private static bool IsFileExist => File.Exists(filepath);

		static GrabbablePoseRecorder()
		{
			EditorApplication.playModeStateChanged += ApplyChanges;
		}

		/// <summary>
		/// Apply changes to grab poses when entering edit mode.
		/// </summary>
		/// <param name="state">The state of the play mode.</param>
		private static void ApplyChanges(PlayModeStateChange state)
		{
			if (IsFileExist && state == PlayModeStateChange.EnteredEditMode)
			{
				GrabPoseBinder binder = AssetDatabase.LoadAssetAtPath<GrabPoseBinder>(filepath);
				if (binder != null)
				{
					HandGrabInteractable[] grabbables = Object.FindObjectsOfType<HandGrabInteractable>();
					foreach (var grabbable in grabbables)
					{
						if (binder.FindGrabPosesWithGrabbable(grabbable, out List<GrabPose> updatedGrabPose))
						{
							for (int i = 0; i < updatedGrabPose.Count; i++)
							{
								GrabPose grabPose = updatedGrabPose[i];
								GrabPose oldGrabPose = grabbable.grabPoses.Find(x => x.grabPoseName == grabPose.grabPoseName);
								if (oldGrabPose != null)
								{
									grabPose.indicator.target = oldGrabPose.indicator.target;
								}
								updatedGrabPose[i] = grabPose;
							}
							grabbable.grabPoses.Clear();
							grabbable.grabPoses.AddRange(updatedGrabPose);
						}
					}
				}
				File.Delete(filepath);
				File.Delete(metaFilepath);
			}
		}

		/// <summary>
		/// Saves changes to grab pose bindings.
		/// </summary>
		public static void SaveChanges()
		{
			if (!IsFileExist)
			{
				GenerateAsset();
			}
			else
			{
				GrabPoseBinder binder = AssetDatabase.LoadAssetAtPath<GrabPoseBinder>(filepath);
				if (binder != null)
				{
					binder.UpdateBindingInfos();
					binder.StorageData();
				}
			}
		}

		/// <summary>
		///  Generates a new asset for storing grab pose bindings.
		/// </summary>
		private static void GenerateAsset()
		{
			GrabPoseBinder binder = ScriptableObject.CreateInstance<GrabPoseBinder>();
			binder.UpdateBindingInfos();
			AssetDatabase.CreateAsset(binder, filepath);
			AssetDatabase.SaveAssets();
		}
	}
#endif
}
