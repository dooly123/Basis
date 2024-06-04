#if UNITY_EDITOR
using UnityEditor;

namespace VIVE.OpenXR.CompositionLayer
{
	[CustomEditor(typeof(ViveCompositionLayer))]
	internal class ViveCompositionLayerEditor : UnityEditor.Editor
	{
		private SerializedProperty enableAutoFallback;

		void OnEnable()
		{
			enableAutoFallback = serializedObject.FindProperty("enableAutoFallback");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(enableAutoFallback);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif