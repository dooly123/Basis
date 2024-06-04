using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
	[CustomEditor(typeof(HandMeshManager))]
	public class HandMeshManagerEditor : Editor
	{
		private HandMeshManager m_HandJointManager;
		private SerializedProperty m_Handedness, m_HandGrabber, m_RootJointType, m_HandRootJoint, m_HandJoints;

		private bool showJoints = false;
		public static readonly GUIContent findJoints = EditorGUIUtility.TrTextContent("Find Joints");
		public static readonly GUIContent clearJoints = EditorGUIUtility.TrTextContent("Clear");

		private void OnEnable()
		{
			m_Handedness = serializedObject.FindProperty("m_Handedness");
			m_HandGrabber = serializedObject.FindProperty("m_HandGrabber");
			m_RootJointType = serializedObject.FindProperty("m_RootJointType");
			m_HandRootJoint = serializedObject.FindProperty("m_HandRootJoint");
			m_HandJoints = serializedObject.FindProperty("m_HandJoints");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(m_Handedness);
			EditorGUILayout.PropertyField(m_HandGrabber);
			EditorGUILayout.HelpBox("Without HandGrabber, it still works but won't stop when colliding with Immovable objects.", MessageType.Info);
			EditorGUILayout.PropertyField(m_RootJointType);
			EditorGUILayout.PropertyField(m_HandRootJoint);
			showJoints = EditorGUILayout.Foldout(showJoints, "Hand Joints");
			if (showJoints)
			{
				for (int i = 0; i < m_HandJoints.arraySize; i++)
				{
					SerializedProperty joint = m_HandJoints.GetArrayElementAtIndex(i);
					JointType jointType = (JointType)i;
					EditorGUILayout.PropertyField(joint, new GUIContent(jointType.ToString()));
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					m_HandJointManager = target as HandMeshManager;
					using (new EditorGUI.DisabledScope())
					{
						if (GUILayout.Button(findJoints))
						{
							m_HandJointManager.FindJoints();
						}
					}

					using (new EditorGUI.DisabledScope())
					{
						if (GUILayout.Button(clearJoints))
						{
							m_HandJointManager.ClearJoints();
						}
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif