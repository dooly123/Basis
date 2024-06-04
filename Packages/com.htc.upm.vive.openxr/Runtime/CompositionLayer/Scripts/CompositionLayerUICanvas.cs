// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace VIVE.OpenXR.CompositionLayer
{
	[RequireComponent(typeof(Canvas))]
	public class CompositionLayerUICanvas : MonoBehaviour
	{
		private Canvas sourceCanvas;
		private RectTransform sourceCanvasRectTransform;
		private Graphic[] graphicComponents;

		private Camera canvasRenderCamera;
		private RenderTexture canvasRenderTexture;

		private GameObject canvasCompositionLayerGO;
		private CompositionLayer canvasCompositionLayer;

		[SerializeField]
		public uint maxRenderTextureSize = 1024;

		[SerializeField]
		public CompositionLayer.LayerType layerType = CompositionLayer.LayerType.Underlay;

		[SerializeField]
		public CompositionLayer.Visibility layerVisibility = CompositionLayer.Visibility.Both;

		[SerializeField]
		public Color cameraBGColor = Color.clear;

		[SerializeField]
		public List<GameObject> backgroundGO = new List<GameObject>();

		[SerializeField]
		public bool enableAlphaBlendingCorrection = false;

		[SerializeField]
		public uint compositionDepth = 0;

		[SerializeField]
		private uint renderPriority = 0;
		public uint GetRenderPriority() { return renderPriority; }
		public void SetRenderPriority(uint newRenderPriority)
		{
			renderPriority = newRenderPriority;
			canvasCompositionLayer.SetRenderPriority(renderPriority);
		}

		[SerializeField]
		public GameObject trackingOrigin = null;

		private CompositionLayer.LayerType previousLayerType;
		private CompositionLayer.Visibility previousLayerVisibility;
		private uint previousCompositionDepth;
		private GameObject previousTrackingOrigin;


		private void Start()
		{
			sourceCanvas = GetComponent<Canvas>();
			sourceCanvasRectTransform = sourceCanvas.GetComponent<RectTransform>();

			UpdateUIElementBlendMode();

			//Calulate Aspect Ratio of the Canvas
			float canvasRectWidth = sourceCanvasRectTransform.rect.width;
			float canvasRectHeight = sourceCanvasRectTransform.rect.height;

			float canvasAspectRatio_X = 1, canvasAspectRatio_Y = 1;

			if (canvasRectWidth > canvasRectHeight)
			{
				canvasAspectRatio_X = canvasRectWidth / canvasRectHeight;
			}
			else if (canvasRectWidth < canvasRectHeight)
			{
				canvasAspectRatio_Y = canvasRectHeight / canvasRectWidth;
			}

			//Create Render Texture
			int renderTextureWidth = Mathf.CeilToInt(maxRenderTextureSize * canvasAspectRatio_X);
			int renderTextureHeight = Mathf.CeilToInt(maxRenderTextureSize * canvasAspectRatio_Y);

			canvasRenderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			canvasRenderTexture.useMipMap = false;
			canvasRenderTexture.filterMode = FilterMode.Bilinear;
			canvasRenderTexture.autoGenerateMips = false;

			canvasRenderTexture.Create();

			//Create Canvas Rendering Camera
			GameObject canvasRenderCameraGO = new GameObject(name + "_CanvasRenderCamera");
			canvasRenderCameraGO.transform.SetParent(transform, false);

			canvasRenderCamera = canvasRenderCameraGO.AddComponent<Camera>();
			canvasRenderCamera.stereoTargetEye = StereoTargetEyeMask.None;
			canvasRenderCamera.transform.position = transform.position - transform.forward; //1m away from canvas
			canvasRenderCamera.orthographic = true;
			canvasRenderCamera.enabled = false;
			canvasRenderCamera.targetTexture = canvasRenderTexture;
			canvasRenderCamera.cullingMask = 1 << gameObject.layer;
			canvasRenderCamera.clearFlags = CameraClearFlags.SolidColor;
			canvasRenderCamera.backgroundColor = cameraBGColor;

			float widthWithScale = canvasRectWidth * sourceCanvasRectTransform.localScale.x;
			float heightWithScale = canvasRectHeight * sourceCanvasRectTransform.localScale.y;

			canvasRenderCamera.orthographicSize = 0.5f * heightWithScale;

			canvasRenderCamera.nearClipPlane = 0.99f;
			canvasRenderCamera.farClipPlane = 1.01f;

			//Create Composition Layer Component
			canvasCompositionLayerGO = new GameObject(name + "_CanvasCompositionLayer");
			canvasCompositionLayerGO.transform.SetParent(transform, false);
			canvasCompositionLayerGO.transform.localPosition = Vector3.zero;
			canvasCompositionLayerGO.transform.localRotation = Quaternion.identity;
			canvasCompositionLayerGO.transform.localScale = Vector3.one;

			canvasCompositionLayer = canvasCompositionLayerGO.AddComponent<CompositionLayer>();
			canvasCompositionLayer.isDynamicLayer = true;
			canvasCompositionLayer.texture = canvasRenderTexture;
			canvasCompositionLayer.layerShape = CompositionLayer.LayerShape.Quad;
			canvasCompositionLayer.layerType = previousLayerType = layerType;
			canvasCompositionLayer.layerVisibility = previousLayerVisibility = layerVisibility;
			canvasCompositionLayer.SetQuadLayerHeight(heightWithScale);
			canvasCompositionLayer.SetQuadLayerWidth(widthWithScale);
			canvasCompositionLayer.compositionDepth = previousCompositionDepth = compositionDepth;
			canvasCompositionLayer.SetRenderPriority(renderPriority);
			canvasCompositionLayer.trackingOrigin = previousTrackingOrigin = trackingOrigin;
			if (enableAlphaBlendingCorrection && layerType == CompositionLayer.LayerType.Underlay) 
			{ 
				canvasCompositionLayer.ChangeBlitShadermode(CompositionLayer.BlitShaderMode.LINEAR_TO_SRGB_ALPHA, true); 
			}
			else
			{
				canvasCompositionLayer.ChangeBlitShadermode(CompositionLayer.BlitShaderMode.LINEAR_TO_SRGB_ALPHA, false);
			}
		}

		private void Update()
		{
			canvasRenderCamera.Render();

			if (layerType != previousLayerType)
			{
				canvasCompositionLayer.layerType = previousLayerType = layerType;

				if (enableAlphaBlendingCorrection && layerType == CompositionLayer.LayerType.Underlay)
				{
					canvasCompositionLayer.ChangeBlitShadermode(CompositionLayer.BlitShaderMode.LINEAR_TO_SRGB_ALPHA, true);
				}
				else
				{
					canvasCompositionLayer.ChangeBlitShadermode(CompositionLayer.BlitShaderMode.LINEAR_TO_SRGB_ALPHA, false);
				}
			}

			if (layerVisibility != previousLayerVisibility)
			{
				canvasCompositionLayer.layerVisibility = previousLayerVisibility = layerVisibility;
			}

			if (compositionDepth != previousCompositionDepth)
			{
				canvasCompositionLayer.compositionDepth = previousCompositionDepth = compositionDepth;
			}

			if (trackingOrigin != previousTrackingOrigin)
			{
				canvasCompositionLayer.trackingOrigin = previousTrackingOrigin = trackingOrigin;
			}

			if (canvasRenderCamera.backgroundColor != cameraBGColor)
			{
				canvasRenderCamera.backgroundColor = cameraBGColor;
			}


		}

		private void OnDestroy()
		{
			if (canvasRenderTexture != null && canvasRenderTexture.IsCreated())
			{
				canvasRenderTexture.Release();
				Destroy(canvasRenderTexture);
			}
			if (canvasCompositionLayerGO)
			{
				Destroy(canvasCompositionLayerGO);
			}
		}

		private void OnEnable()
		{
			if (canvasRenderCamera)
			{
				canvasRenderCamera.enabled = true;
			}

			if (canvasCompositionLayerGO)
			{
				canvasCompositionLayerGO.SetActive(true);
			}
		}

		private void OnDisable()
		{
			if (canvasRenderCamera)
			{
				canvasRenderCamera.enabled = false;
			}

			if (canvasCompositionLayerGO)
			{
				canvasCompositionLayerGO.SetActive(false);
			}
		}

		public void ReplaceUIMaterials()
		{
			sourceCanvas = GetComponent<Canvas>();
			graphicComponents = sourceCanvas.GetComponentsInChildren<Graphic>();
			
			Material underlayCanvasUIMat = new Material(Shader.Find("VIVE/OpenXR/CompositionLayerUICanvas/MultiLayerCanvasUI"));

			foreach (Graphic graphicComponent in graphicComponents)
			{
				if (backgroundGO != null && backgroundGO.Contains(graphicComponent.gameObject))
				{
					graphicComponent.material = new Material(Shader.Find("VIVE/OpenXR/CompositionLayerUICanvas/MultiLayerCanvasUI")); //Seperate material instance for background
				}
				else
				{
					graphicComponent.material = underlayCanvasUIMat;
				}
			}
		}

		public void UpdateUIElementBlendMode()
		{
			sourceCanvas = GetComponent<Canvas>();
			graphicComponents = sourceCanvas.GetComponentsInChildren<Graphic>();

			foreach (Graphic graphicComponent in graphicComponents)
			{
				if (backgroundGO != null && backgroundGO.Contains(graphicComponent.gameObject))
				{
					SetUIShaderBlendMode(graphicComponent.material, UIShaderBlendMode.Background);
				}
				else
				{
					SetUIShaderBlendMode(graphicComponent.material, UIShaderBlendMode.Others);
				}
			}
		}

		public void SetUIShaderBlendMode(Material canvasUIMaterial, UIShaderBlendMode blendMode = UIShaderBlendMode.Others)
		{
			switch (blendMode)
			{
				case UIShaderBlendMode.Background: //Discard camera background color and alpha values
					canvasUIMaterial.SetInt("_SrcColBlendMode", (int)BlendMode.One);
					canvasUIMaterial.SetInt("_DstColBlendMode", (int)BlendMode.Zero);
					canvasUIMaterial.SetInt("_SrcAlpBlendMode", (int)BlendMode.One);
					canvasUIMaterial.SetInt("_DstAlpBlendMode", (int)BlendMode.Zero);

					break;

				case UIShaderBlendMode.Others: //Nornmal transparency blending
				default:
					canvasUIMaterial.SetInt("_SrcColBlendMode", (int)BlendMode.SrcAlpha);
					canvasUIMaterial.SetInt("_DstColBlendMode", (int)BlendMode.OneMinusSrcAlpha);
					canvasUIMaterial.SetInt("_SrcAlpBlendMode", (int)BlendMode.One);
					canvasUIMaterial.SetInt("_DstAlpBlendMode", (int)BlendMode.OneMinusSrcAlpha);

					break;
			}
		}

		public enum UIShaderBlendMode
		{
			Background,
			Others,
		}
	}
}
