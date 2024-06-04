// Copyright HTC Corporation All Rights Reserved.
#if UNITY_EDITOR
using UnityEditor;

namespace VIVE.OpenXR.Editor
{
	[CustomEditor(typeof(VIVEFocus3Feature))]
	internal class VIVEFocus3FeatureEditor : UnityEditor.Editor
	{
		//private SerializedProperty enableHandTracking;
		//private SerializedProperty enableTracker;

		void OnEnable()
		{
			//enableHandTracking = serializedObject.FindProperty("enableHandTracking");
			//enableTracker = serializedObject.FindProperty("enableTracker");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			//EditorGUILayout.PropertyField(enableHandTracking);
			//EditorGUILayout.PropertyField(enableTracker);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
