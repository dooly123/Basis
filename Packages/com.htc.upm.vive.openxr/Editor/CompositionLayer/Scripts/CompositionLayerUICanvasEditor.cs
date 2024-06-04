// "VIVE SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the VIVE SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VIVE.OpenXR.CompositionLayer.Editor
{
	using UnityEditor;
	using UnityEngine.UI;

	[CustomEditor(typeof(CompositionLayerUICanvas))]
	public class CompositionLayerUICanvasEditor : Editor
	{
		static string PropertyName_MaxRenderTextureSize = "maxRenderTextureSize";
		static GUIContent Label_MaxRenderTextureSize = new GUIContent("Max Render Texture Size", "Maximum render texture dimension. e.g. If maxRenderTextureSize is 1024, the render texture dimensions of a canvas with an Aspect Ratio of 2:1 will be 1024 x 512.");
		SerializedProperty Property_MaxRenderTextureSize;

		static string PropertyName_LayerType = "layerType";
		static GUIContent Label_LayerType = new GUIContent("Layer Type", "Overlays render on top of all in-game objects.\nUnderlays can be occluded by in-game objects but may introduce alpha blending issues with transparent objects.");
		SerializedProperty Property_LayerType;

		static string PropertyName_LayerVisibility = "layerVisibility";
		static GUIContent Label_LayerVisibility = new GUIContent("Visibility", "Specify the visibility of the layer.");
		SerializedProperty Property_LayerVisibility;

		static string PropertyName_CameraBGColor = "cameraBGColor";
		static GUIContent Label_CameraBGColor = new GUIContent("Camera Background Color", "Background color of the camera for rendering the Canvas to the render texture target.\nChanging this option will affect the tint of the final image of the Canvas if no background gameobject is assigned.");
		SerializedProperty Property_CameraBGColor;

		static string PropertyName_BackgroundGO = "backgroundGO";
		static GUIContent Label_BackgroundGO = new GUIContent("Background GameObject", "GameObject that contains a UI Component and will be used as the background of the Canvas.\nWhen succesfully assigned, the area which the background UI component covers will no longer be affected by the background color of the camera.");
		SerializedProperty Property_BackgroundGO;

		static string PropertyName_EnableAlphaBlendingCorrection = "enableAlphaBlendingCorrection";
		static GUIContent Label_EnableAlphaBlendingCorrection = new GUIContent("Enable Alpha Blending Correction", "Enable this option if transparent UI elements are rendering darker than expected in an overall sense.\nNote that enabling this option will consume more resources.");
		SerializedProperty Property_EnableAlphaBlendingCorrection;

		static string PropertyName_CompositionDepth = "compositionDepth";
		static GUIContent Label_CompositionDepth = new GUIContent("Composition Depth", "Specify Layer Composition Depth.");
		SerializedProperty Property_CompositionDepth;

		static string PropertyName_RenderPriority = "renderPriority";
		static GUIContent Label_RenderPriority = new GUIContent("Render Priority", "When Auto Fallback is enabled, layers with a higher render priority will be rendered as normal layers first.");
		SerializedProperty Property_RenderPriority;

		static string PropertyName_TrackingOrigin = "trackingOrigin";
		static GUIContent Label_TrackingOrigin = new GUIContent("Tracking Origin", "Assign the tracking origin here to offset the pose of the Composition Layer.");
		SerializedProperty Property_TrackingOrigin;

		bool isCurrentBackgroundGOValid = true, isMaterialReplaced = true, backgroundUINotFoundError = false, backgroundObjectNotChildError = false;
		List<GameObject> validbackgroundGO;

		private const string layerNameString = "CompositionLayerUICanvas";

		public override void OnInspectorGUI()
		{
			//Check if current selected layer is rendered by main camera

			if (Property_MaxRenderTextureSize == null) Property_MaxRenderTextureSize = serializedObject.FindProperty(PropertyName_MaxRenderTextureSize);
			if (Property_LayerType == null) Property_LayerType = serializedObject.FindProperty(PropertyName_LayerType);
			if (Property_LayerVisibility == null) Property_LayerVisibility = serializedObject.FindProperty(PropertyName_LayerVisibility);
			if (Property_CameraBGColor == null) Property_CameraBGColor = serializedObject.FindProperty(PropertyName_CameraBGColor);
			if (Property_BackgroundGO == null) Property_BackgroundGO = serializedObject.FindProperty(PropertyName_BackgroundGO);
			if (Property_EnableAlphaBlendingCorrection == null) Property_EnableAlphaBlendingCorrection = serializedObject.FindProperty(PropertyName_EnableAlphaBlendingCorrection);
			if (Property_CompositionDepth == null) Property_CompositionDepth = serializedObject.FindProperty(PropertyName_CompositionDepth);
			if (Property_RenderPriority == null) Property_RenderPriority = serializedObject.FindProperty(PropertyName_RenderPriority);
			if (Property_TrackingOrigin == null) Property_TrackingOrigin = serializedObject.FindProperty(PropertyName_TrackingOrigin);


			CompositionLayerUICanvas targetLayerCanvasUI = target as CompositionLayerUICanvas;

			Graphic[] graphicComponents = targetLayerCanvasUI.GetComponentsInChildren<Graphic>();

			EditorGUILayout.HelpBox("CompositionLayerUICanvas will automatically generate the components necessary for rendering UI Canvas(es) with CompositionLayer(s).", MessageType.Info);

			EditorGUILayout.PropertyField(Property_MaxRenderTextureSize, Label_MaxRenderTextureSize);
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.PropertyField(Property_LayerType, Label_LayerType);
			serializedObject.ApplyModifiedProperties();

			if (targetLayerCanvasUI.layerType == CompositionLayer.LayerType.Underlay)
			{
				EditorGUILayout.HelpBox("When using Underlay, overlapping non-opaque canvas elements (i.e. elements with alpha value < 1) might look different during runtime due to inherent alpha blending limitations.\n", MessageType.Warning);
				EditorGUILayout.PropertyField(Property_EnableAlphaBlendingCorrection, Label_EnableAlphaBlendingCorrection);
				serializedObject.ApplyModifiedProperties();
			}
			else
			{
				targetLayerCanvasUI.enableAlphaBlendingCorrection = false;
			}

			EditorGUILayout.PropertyField(Property_LayerVisibility, new GUIContent(Label_LayerVisibility));
			serializedObject.ApplyModifiedProperties();

			if (isCurrentBackgroundGOValid) //Cache valid result
			{
				validbackgroundGO = new List<GameObject>();
				foreach (GameObject backgroundGO in targetLayerCanvasUI.backgroundGO)
				{
					validbackgroundGO.Add(backgroundGO);
				}
			}

			List<GameObject> prevBackgroundGO = new List<GameObject>();
			foreach (GameObject backgroundGO in targetLayerCanvasUI.backgroundGO)
			{
				prevBackgroundGO.Add(backgroundGO);
			}

			EditorGUILayout.PropertyField(Property_BackgroundGO, Label_BackgroundGO);
			serializedObject.ApplyModifiedProperties();
			bool needMaterialReplacement = false;

			if (targetLayerCanvasUI.backgroundGO != null)
			{
				List<Graphic> backgroundGraphics = new List<Graphic>();

				foreach (GameObject backgroundGO in targetLayerCanvasUI.backgroundGO)
				{
					if (backgroundGO == null) continue;
					
					backgroundGraphics.Add(backgroundGO.GetComponent<Graphic>());
				}

				bool backgroundGraphicIsInChild = false;
				if (backgroundGraphics.Count > 0)
				{
					foreach (Graphic backgroundGraphic in backgroundGraphics) //Loop through graphic components of selected background objects
					{
						if (backgroundGraphic != null)
						{
							backgroundUINotFoundError = false;
							foreach (Graphic graphicComponent in graphicComponents) //Loop through graphic components under current canvas
							{
								if (graphicComponent == backgroundGraphic) 
								{
									backgroundGraphicIsInChild = true;
									backgroundObjectNotChildError = false;
									break;
								}
								backgroundGraphicIsInChild = false;
							}

							if (!backgroundGraphicIsInChild) //Triggers when one of the selected objects is invalid
							{
								backgroundObjectNotChildError = true;
								break;
							}
						}
						else
						{
							backgroundUINotFoundError = true;
							break;
						}
					}

					if (!backgroundUINotFoundError && !backgroundObjectNotChildError)
					{
						isCurrentBackgroundGOValid = true;
						foreach (GameObject backgroundGOCurr in targetLayerCanvasUI.backgroundGO)
						{
							if (backgroundGOCurr == null) continue;

							if (!prevBackgroundGO.Contains(backgroundGOCurr)) //Needs material replacement
							{ 
								needMaterialReplacement = true;
								isMaterialReplaced = false;
								break; 
							}
						}
						EditorGUILayout.HelpBox("The blending mode of the background UI shader will be changed in order to ignore the background color of the camera.", MessageType.Info);
					}
					else
					{
						isCurrentBackgroundGOValid = false;
						if (backgroundUINotFoundError)
						{
							EditorGUILayout.HelpBox("The background object you are trying to assign does not contain a UI Component.", MessageType.Error);
						}

						if (backgroundObjectNotChildError)
						{
							EditorGUILayout.HelpBox("The background object you are trying to assign is not under the current Canvas.", MessageType.Error);
						}

						if (GUILayout.Button("Revert Background GameObjects"))
						{
							targetLayerCanvasUI.backgroundGO = validbackgroundGO;
						}
					}
				}
			}

			EditorGUILayout.PropertyField(Property_CameraBGColor, Label_CameraBGColor);
			serializedObject.ApplyModifiedProperties();

			//Check the material config of the UI elements
			foreach (Graphic graphicComponent in graphicComponents)
			{
				if (graphicComponent.material == null || graphicComponent.material == graphicComponent.defaultMaterial)
				{
					needMaterialReplacement = true;
					isMaterialReplaced = false;
					break;
				}
			}

			if (needMaterialReplacement || !isMaterialReplaced)
			{
				EditorGUILayout.HelpBox("The current material configurations of the UI elements will lead to incorrect alpha blending.\n" +
					"Replace the materials to yield better visual results.", MessageType.Error);

				if (GUILayout.Button("Replace UI Materials"))
				{
					targetLayerCanvasUI.ReplaceUIMaterials();
					isMaterialReplaced = true;
				}
			}
				

			//Check if current selected layer is rendered by main camera
			if (Camera.main != null)
			{
				if ((Camera.main.cullingMask & (1 << targetLayerCanvasUI.gameObject.layer)) != 0)
				{
					EditorGUILayout.HelpBox("Currently selected layer: " + LayerMask.LayerToName(targetLayerCanvasUI.gameObject.layer) + "\nThis layer is not culled by the Main Camera.\nSelect a layer that will not be rendered by the Main Camera and apply it to all child objects.", MessageType.Error);

					//TODO: Add Auto Layer button
					if (GUILayout.Button("Auto Select Layer"))
					{
						// Open tag manager
						SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
						// Layers Property
						SerializedProperty layersProp = tagManager.FindProperty("layers");

						//Check if the layer CompositionLayerUICanvas exists
						bool layerExists = false, firstEmptyLayerFound = false;
						int emptyLayerIndex = 0;
						for (int i = 0; i < layersProp.arraySize; i++)
						{
							if (layersProp.GetArrayElementAtIndex(i).stringValue == layerNameString)
							{
								layerExists = true;
								ApplyLayerToGameObjectRecursive(targetLayerCanvasUI.gameObject, i);
								break;
							}
							else if (layersProp.GetArrayElementAtIndex(i).stringValue == "")
							{
								if (!firstEmptyLayerFound) //Remember the index of the first empty layer
								{
									firstEmptyLayerFound = true;
									emptyLayerIndex = i;
								}
							}
						}

						if (!layerExists) //Create layer and apply it
						{
							layersProp.GetArrayElementAtIndex(emptyLayerIndex).stringValue = layerNameString;

							ApplyLayerToGameObjectRecursive(targetLayerCanvasUI.gameObject, emptyLayerIndex);

							tagManager.ApplyModifiedProperties();
						}
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox("Main Camera not found, and hence cannot confirm the status of its Culling Mask.\nMake sure that the Main Camera does not draw the " + LayerMask.LayerToName(targetLayerCanvasUI.gameObject.layer) + " layer." , MessageType.Warning);
			}


			EditorGUILayout.PropertyField(Property_CompositionDepth, Label_CompositionDepth);
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.PropertyField(Property_RenderPriority, Label_RenderPriority);
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.PropertyField(Property_TrackingOrigin, Label_TrackingOrigin);
			serializedObject.ApplyModifiedProperties();
		}

		private void ApplyLayerToGameObjectRecursive(GameObject targetGO, int layerID)
		{
			if (targetGO.transform.childCount > 0)
			{
				for (int i=0; i<targetGO.transform.childCount; i++) 
				{
					ApplyLayerToGameObjectRecursive(targetGO.transform.GetChild(i).gameObject, layerID);
				}
			}

			targetGO.layer = layerID;
		}
	}
}
