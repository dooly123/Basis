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
using UnityEditorInternal;
namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
     [CustomEditor(typeof(HandGrabInteractable))]
	public class HandGrabInteractableEditor : UnityEditor.Editor
    {
        private SerializedProperty m_IsGrabbable, m_ForceMovable, m_FingerRequirement, m_GrabPoses, m_ShowAllIndicator;
		private SerializedProperty grabPoseName, gestureThumbPose, gestureIndexPose, gestureMiddlePose, gestureRingPose, gesturePinkyPose,
			recordedGrabRotations, isLeft, enableIndicator, autoIndicator, indicatorObject, grabOffset;
		private ReorderableList grabPoses;
		private bool showGrabPoses = false;

		private void OnEnable()
        {
			m_IsGrabbable = serializedObject.FindProperty("m_IsGrabbable");
			m_ForceMovable = serializedObject.FindProperty("m_ForceMovable");
			m_FingerRequirement = serializedObject.FindProperty("m_FingerRequirement");
			m_GrabPoses = serializedObject.FindProperty("m_GrabPoses");
			m_ShowAllIndicator = serializedObject.FindProperty("m_ShowAllIndicator");

			#region ReorderableList
			grabPoses = new ReorderableList(serializedObject, m_GrabPoses, true, true, true, true);
			grabPoses.drawHeaderCallback = (Rect rect) =>
			{
				EditorGUI.LabelField(rect, "Grab Pose List");
			};
			grabPoses.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				if (!UpdateGrabPose(m_GrabPoses.GetArrayElementAtIndex(index))) { return; }

				if (string.IsNullOrEmpty(grabPoseName.stringValue))
				{
					grabPoseName.stringValue = $"Grab Pose {index + 1}";
				}

				Rect gestureRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
				grabPoseName.stringValue = EditorGUI.TextField(gestureRect, grabPoseName.stringValue);
				if (recordedGrabRotations.arraySize == 0)
				{
					// Draw GrabGesture fields
					gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.PropertyField(gestureRect, gestureThumbPose);
					gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.PropertyField(gestureRect, gestureIndexPose);
					gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.PropertyField(gestureRect, gestureMiddlePose);
					gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.PropertyField(gestureRect, gestureRingPose);
					gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.PropertyField(gestureRect, gesturePinkyPose);
				}

				// Draw Handness fields
				gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				bool isToggle = EditorGUI.Toggle(gestureRect, "Is Left", isLeft.boolValue);
				if (isToggle != isLeft.boolValue)
				{
					isLeft.boolValue = isToggle;
					SwitchRotations(ref recordedGrabRotations);
				}
				// Draw Indicator fields
				gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				enableIndicator.boolValue = EditorGUI.Toggle(gestureRect, "Show Indicator", enableIndicator.boolValue);
				if (enableIndicator.boolValue)
				{
					gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					autoIndicator.boolValue = EditorGUI.Toggle(gestureRect, "Auto Generator Indicator", autoIndicator.boolValue);
					if (!autoIndicator.boolValue)
					{
						gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
						indicatorObject.objectReferenceValue = (GameObject)EditorGUI.ObjectField(gestureRect, "Indicator", (GameObject)indicatorObject.objectReferenceValue, typeof(GameObject), true);
					}
				}
				else
				{
					m_ShowAllIndicator.boolValue = false;
				}

				// Draw Mirror Pose fields
				gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				Rect labelRect = new Rect(gestureRect.x, gestureRect.y, EditorGUIUtility.labelWidth, gestureRect.height);
				EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Mirror Pose"));

				Rect buttonRect1 = new Rect(gestureRect.x + EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing, gestureRect.y, (gestureRect.width - EditorGUIUtility.labelWidth - EditorGUIUtility.standardVerticalSpacing * 4) / 3, gestureRect.height);
				Rect buttonRect2 = new Rect(buttonRect1.x + buttonRect1.width + EditorGUIUtility.standardVerticalSpacing, gestureRect.y, buttonRect1.width, gestureRect.height);
				Rect buttonRect3 = new Rect(buttonRect2.x + buttonRect2.width + EditorGUIUtility.standardVerticalSpacing, gestureRect.y, buttonRect1.width, gestureRect.height);
				if (GUI.Button(buttonRect1, "Align X axis"))
				{
					MirrorPose(ref grabOffset, Vector3.right);
				}
				if (GUI.Button(buttonRect2, "Align Y axis"))
				{
					MirrorPose(ref grabOffset, Vector3.up);
				}
				if (GUI.Button(buttonRect3, "Align Z axis"))
				{
					MirrorPose(ref grabOffset, Vector3.forward);
				}

				// Draw Position fields
				gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(gestureRect, grabOffset.FindPropertyRelative("position"));

				// Draw Rotation fields
				SerializedProperty rotationProperty = grabOffset.FindPropertyRelative("rotation");
				Vector4 rotationVector = new Vector4(rotationProperty.quaternionValue.x, rotationProperty.quaternionValue.y, rotationProperty.quaternionValue.z, rotationProperty.quaternionValue.w);
				gestureRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				rotationVector = EditorGUI.Vector4Field(gestureRect, "Rotation", rotationVector);
				rotationProperty.quaternionValue = new Quaternion(rotationVector.x, rotationVector.y, rotationVector.z, rotationVector.w);
			};
			grabPoses.elementHeightCallback = (int index) =>
			{
				if (!UpdateGrabPose(m_GrabPoses.GetArrayElementAtIndex(index))) { return EditorGUIUtility.singleLineHeight; }

				// Including Title, Handness, Show Indicator, Mirror Pose, Position, Rotation
				int minHeight = 6;
				// To Show GrabGesture
				if (recordedGrabRotations.arraySize == 0)
				{ 
					minHeight += 5;
				}
				if (enableIndicator.boolValue)
				{
					// To Show Auto Indicator
					minHeight += 1;
					// To Show Indicator Gameobject
					if (!autoIndicator.boolValue)
					{ 
						minHeight += 1;
					}
				}
				return EditorGUIUtility.singleLineHeight * minHeight + EditorGUIUtility.standardVerticalSpacing * (minHeight + 2);
			};
			grabPoses.onAddCallback = (ReorderableList list) =>
			{
				m_GrabPoses.arraySize++;
				if (UpdateGrabPose(m_GrabPoses.GetArrayElementAtIndex(list.count - 1)))
				{
					grabPoseName.stringValue = $"Grab Pose {list.count}";
				}
			};
			#endregion
		}

		public override void OnInspectorGUI()
        {
            serializedObject.Update();
			EditorGUILayout.PropertyField(m_IsGrabbable);
			EditorGUILayout.PropertyField(m_ForceMovable);
			EditorGUILayout.PropertyField(m_FingerRequirement);
			showGrabPoses = EditorGUILayout.Foldout(showGrabPoses, "Grab Pose Settings");
			if (showGrabPoses)
			{
				if(m_GrabPoses.arraySize == 0)
				{
					grabPoses.elementHeight = EditorGUIUtility.singleLineHeight;
				}
				grabPoses.DoLayoutList();

				bool isToggle = EditorGUILayout.Toggle("Show All Indicator", m_ShowAllIndicator.boolValue);
				if (isToggle != m_ShowAllIndicator.boolValue)
				{
					m_ShowAllIndicator.boolValue = isToggle;
					for (int i = 0; i < m_GrabPoses.arraySize; i++)
					{
						if (UpdateGrabPose(m_GrabPoses.GetArrayElementAtIndex(i)))
						{
							enableIndicator.boolValue = m_ShowAllIndicator.boolValue;
						}
					}
				}
			}
			serializedObject.ApplyModifiedProperties();
        }

		private bool UpdateGrabPose(SerializedProperty grabPose)
		{
			SerializedProperty indicator = grabPose.FindPropertyRelative("indicator");
			if (grabPose == null || indicator == null) { return false; }

			grabPoseName = grabPose.FindPropertyRelative("grabPoseName");
			gestureThumbPose = grabPose.FindPropertyRelative("handGrabGesture.thumbPose");
			gestureIndexPose = grabPose.FindPropertyRelative("handGrabGesture.indexPose");
			gestureMiddlePose = grabPose.FindPropertyRelative("handGrabGesture.middlePose");
			gestureRingPose = grabPose.FindPropertyRelative("handGrabGesture.ringPose");
			gesturePinkyPose = grabPose.FindPropertyRelative("handGrabGesture.pinkyPose");
			recordedGrabRotations = grabPose.FindPropertyRelative("recordedGrabRotations");
			isLeft = grabPose.FindPropertyRelative("isLeft");
			enableIndicator = indicator.FindPropertyRelative("enableIndicator");
			autoIndicator = indicator.FindPropertyRelative("autoIndicator");
			indicatorObject = indicator.FindPropertyRelative("target");
			grabOffset = grabPose.FindPropertyRelative("grabOffset");
			return true;
		}

		/// <summary>
		/// Convert the rotation of joints of the current hand into those of another hand.
		/// </summary>
		/// <param name="rotations">Rotation of joints of the current hand.</param>
		private void SwitchRotations(ref SerializedProperty rotations)
		{
			for (int i = 0; i < rotations.arraySize; i++)
			{
				Quaternion rotation = rotations.GetArrayElementAtIndex(i).quaternionValue;
				Quaternion newRotation = Quaternion.Euler(rotation.eulerAngles.x, -rotation.eulerAngles.y, -rotation.eulerAngles.z);
				rotations.GetArrayElementAtIndex(i).quaternionValue = newRotation;
			}
		}

		/// <summary>
		/// Mirrors the pose properties (position and rotation) of a serialized object along a specified mirror axis.
		/// </summary>
		/// <param name="pose">The serialized property representing the pose to be mirrored.</param>
		/// <param name="mirrorAxis">The axis along which the mirroring should occur.</param>
		private void MirrorPose(ref SerializedProperty pose, Vector3 mirrorAxis)
        {
            Vector3 sourcePos = pose.FindPropertyRelative("position").vector3Value;
			Quaternion sourceRot = pose.FindPropertyRelative("rotation").quaternionValue;
			Vector3 sourceFwd = sourceRot * Vector3.forward;
			Vector3 sourceUp = sourceRot * Vector3.up;

			// Calculate the mirrored position using Vector3.Reflect along the specified mirror axis.
			Vector3 mirroredPosition = Vector3.Reflect(sourcePos, mirrorAxis);
			// Calculate the mirrored rotation using Quaternion.LookRotation and Vector3.Reflect for the forward and up vectors.
			Quaternion mirroredRotation = Quaternion.LookRotation(Vector3.Reflect(sourceFwd, mirrorAxis), Vector3.Reflect(sourceUp, mirrorAxis));

			pose.FindPropertyRelative("position").vector3Value = mirroredPosition;
            pose.FindPropertyRelative("rotation").quaternionValue = mirroredRotation;
        }
    }
}
#endif
