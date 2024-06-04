// Copyright HTC Corporation All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using System.Collections.Generic;
using System.Linq;
using VIVE.OpenXR.CompositionLayer;

namespace VIVE.OpenXR.CompositionLayer
{
	public class CompositionLayer : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField]
		public bool isPreviewingCylinder = false;
		[SerializeField]
		public bool isPreviewingQuad = false;
		[SerializeField]
		public GameObject generatedPreview = null;

		public const string CylinderPreviewName = "CylinderPreview";
		public const string QuadPreviewName = "QuadPreview";
#endif
		public const string CylinderUnderlayMeshName = "Underlay Alpha Mesh (Cylinder)";
		public const string QuadUnderlayMeshName = "Underlay Alpha Mesh (Quad)";
		public const string FallbackMeshName = "FallbackMeshGO";

		[SerializeField]
		public LayerType layerType = LayerType.Overlay;

		[SerializeField]
		public uint compositionDepth = 0;

		[SerializeField]
		public LayerShape layerShape = LayerShape.Quad;

		[SerializeField]
		public Visibility layerVisibility = Visibility.Both;

		[SerializeField]
		[Min(0.001f)]
		private float m_QuadWidth = 1f;
		public float quadWidth { get { return m_QuadWidth; } }
#if UNITY_EDITOR
		public float QuadWidth { get { return m_QuadWidth; } set { m_QuadWidth = value; } }
#endif

		[SerializeField]
		[Min(0.001f)]
		private float m_QuadHeight = 1f;
		public float quadHeight { get { return m_QuadHeight; } }
#if UNITY_EDITOR
		public float QuadHeight { get { return m_QuadHeight; } set { m_QuadHeight = value; } }
#endif

		[SerializeField]
		[Min(0.001f)]
		private float m_CylinderHeight = 1f;
		public float cylinderHeight { get { return m_CylinderHeight; } }
#if UNITY_EDITOR
		public float CylinderHeight { get { return m_CylinderHeight; } set { m_CylinderHeight = value; } }
#endif

		[SerializeField]
		[Min(0.001f)]
		private float m_CylinderArcLength = 1f;
		public float cylinderArcLength { get { return m_CylinderArcLength; } }
#if UNITY_EDITOR
		public float CylinderArcLength { get { return m_CylinderArcLength; } set { m_CylinderArcLength = value; } }
#endif

		[SerializeField]
		[Min(0.001f)]
		private float m_CylinderRadius = 1f;
		public float cylinderRadius { get { return m_CylinderRadius; } }
#if UNITY_EDITOR
		public float CylinderRadius { get { return m_CylinderRadius; } set { m_CylinderRadius = value; } }
#endif

		[SerializeField]
		[Range(0f, 360f)]
		private float m_CylinderAngleOfArc = 180f;
		public float cylinderAngleOfArc { get { return m_CylinderAngleOfArc; } }
#if UNITY_EDITOR
		public float CylinderAngleOfArc { get { return m_CylinderAngleOfArc; } set { m_CylinderAngleOfArc = value; } }
#endif

#if UNITY_EDITOR
		[SerializeField]
		public CylinderLayerParamLockMode lockMode = CylinderLayerParamLockMode.ArcLength;
#endif

		[SerializeField]
		public bool isDynamicLayer = false;

		[SerializeField]
		public bool applyColorScaleBias = false;

		[SerializeField]
		public bool solidEffect = false;

		[SerializeField]
		public Color colorScale = Color.white;

		[SerializeField]
		public Color colorBias = Color.clear;

		[SerializeField]
		public bool isProtectedSurface = false;

		[SerializeField]
		public Texture texture = null;

		[SerializeField]
		private uint renderPriority = 0;
		public uint GetRenderPriority() { return renderPriority; }
		public void SetRenderPriority(uint newRenderPriority)
		{
			renderPriority = newRenderPriority;
			isRenderPriorityChanged = true;
			CompositionLayerManager.GetInstance().SubscribeToLayerManager(this);
		}
		public bool isRenderPriorityChanged
		{
			get; private set;
		}

		[SerializeField]
		public GameObject trackingOrigin = null;

		public GameObject generatedUnderlayMesh = null;
		private MeshRenderer generatedUnderlayMeshRenderer = null;
		private MeshFilter generatedUnderlayMeshFilter = null;

		public GameObject generatedFallbackMesh = null;
		private MeshRenderer generatedFallbackMeshRenderer = null;
		private MeshFilter generatedFallbackMeshFilter = null;

		private LayerTextures layerTextures;
		private Material texture2DBlitMaterial;

		private GameObject compositionLayerPlaceholderPrefabGO = null;

		public readonly float angleOfArcLowerLimit = 1f;
		public readonly float angleOfArcUpperLimit = 360f;

		private LayerShape previousLayerShape = LayerShape.Quad;
		private float previousQuadWidth = 1f;
		private float previousQuadHeight = 1f;
		private float previousCylinderHeight = 1f;
		private float previousCylinderArcLength = 1f;
		private float previousCylinderRadius = 1f;
		private float previousAngleOfArc = 180f;
		private Texture previousTexture = null;
		private bool previousIsDynamicLayer = false;

		private int layerID; //For native
		private bool isHeadLock = false;
		private bool InitStatus = false;
		private bool isInitializationComplete = false;
		private bool reinitializationNeeded = false;
		private bool isOnBeforeRenderSubscribed = false;
		private bool isLayerReadyForSubmit = false;
		private bool isLinear = false;
		private bool isAutoFallbackActive = false;
		private bool placeholderGenerated = false;
		private static bool isSynchronized = false;
		private static RenderThreadSynchronizer synchronizer;
		private Camera hmd;

		private ViveCompositionLayer compositionLayerFeature = null;

		private const string LOG_TAG = "VIVE_CompositionLayer";
		static void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
		static void WARNING(string msg) { Debug.LogWarning(LOG_TAG + " " + msg); }
		static void ERROR(string msg) { Debug.LogError(LOG_TAG + " " + msg); }

		#region Composition Layer Lifecycle
		private bool CompositionLayerInit()
		{
			if (isInitializationComplete)
			{
				//DEBUG("CompositionLayerInit: Already initialized.");
				return true;
			}

			if (texture == null)
			{
				ERROR("CompositionLayerInit: Source Texture not found, abort init.");
				return false;
			}

			DEBUG("CompositionLayerInit");

			uint textureWidth = (uint)texture.width;
			uint textureHeight = (uint)texture.height;

			CompositionLayerRenderThreadSyncObject ObtainLayerSwapchainSyncObject = new CompositionLayerRenderThreadSyncObject(
				(taskQueue) =>
				{
					lock (taskQueue)
					{
						CompositionLayerRenderThreadTask task = (CompositionLayerRenderThreadTask)taskQueue.Dequeue();

						//Enumerate Swapchain formats
						compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();

						uint imageCount;

						GraphicsAPI graphicsAPI = GraphicsAPI.GLES3;

						switch(SystemInfo.graphicsDeviceType)
						{
							case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
								graphicsAPI = GraphicsAPI.GLES3;
								break;
							case UnityEngine.Rendering.GraphicsDeviceType.Vulkan:
								graphicsAPI = GraphicsAPI.Vulkan;
								break;
							default:
								ERROR("Unsupported Graphics API, aborting init process.");
								return;
						}

						layerID = compositionLayerFeature.CompositionLayer_Init(textureWidth, textureHeight, graphicsAPI, isDynamicLayer, isProtectedSurface, out imageCount);

						if (layerID != 0)
						{
							DEBUG("Init completed, ID: " + layerID + ", Image Count: " + imageCount);
							layerTextures = new LayerTextures(imageCount);
							InitStatus = true;
						}

						taskQueue.Release(task);
					}
				});

			CompositionLayerRenderThreadTask.IssueObtainSwapchainEvent(ObtainLayerSwapchainSyncObject);

			previousLayerShape = layerShape;
			previousQuadWidth = m_QuadWidth;
			previousQuadHeight = m_QuadHeight;
			previousCylinderHeight = m_CylinderHeight;
			previousCylinderArcLength = m_CylinderArcLength;
			previousCylinderRadius = m_CylinderRadius;
			previousAngleOfArc = m_CylinderAngleOfArc;
			previousTexture = texture;
			previousIsDynamicLayer = isDynamicLayer;

			return true;
		}

		private bool textureAcquired = false;
		private bool textureAcquiredOnce = false;
		XrOffset2Di offset = new XrOffset2Di();
		XrExtent2Di extent = new XrExtent2Di();
		XrRect2Di rect = new XrRect2Di();
		private bool SetLayerTexture()
		{
			if (!isInitializationComplete || !isSynchronized) return false;

			if (texture != null) //check for texture size change
			{
				if (TextureParamsChanged())
				{
					//Destroy queues
					DEBUG("SetLayerTexture: Texture params changed, need to re-init queues. layerID: " + layerID);
					DestroyCompositionLayer();
					reinitializationNeeded = true;
					return false;
				}
			}
			else
			{
				ERROR("SetLayerTexture: No texture found. layerID: " + layerID);
				return false;
			}

			if (isDynamicLayer || (!isDynamicLayer && !textureAcquiredOnce))
			{
				//Get available texture id
				compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();
				uint currentImageIndex;

				IntPtr newTextureID = compositionLayerFeature.CompositionLayer_GetTexture(layerID, out currentImageIndex);

				textureAcquired = true;
				textureAcquiredOnce = true;

				if (newTextureID == IntPtr.Zero)
				{
					ERROR("SetLayerTexture: Invalid Texture ID");
					if (compositionLayerFeature.CompositionLayer_ReleaseTexture(layerID))
					{
						textureAcquired = false;
					}
					return false;
				}

				bool textureIDUpdated = false;
				layerTextures.currentAvailableTextureIndex = currentImageIndex;
				IntPtr currentTextureID = layerTextures.GetCurrentAvailableTextureID();
				if (currentTextureID == IntPtr.Zero || currentTextureID != newTextureID)
				{
					DEBUG("SetLayerTexture: Update Texture ID. layerID: " + layerID);
					layerTextures.SetCurrentAvailableTextureID(newTextureID);
					textureIDUpdated = true;
				}

				if (layerTextures.GetCurrentAvailableTextureID() == IntPtr.Zero)
				{
					ERROR("SetLayerTexture: Failed to get texture.");
					return false;
				}

				// Create external texture
				if (layerTextures.GetCurrentAvailableExternalTexture() == null || textureIDUpdated)
				{
					DEBUG("SetLayerTexture: Create External Texture.");
					layerTextures.SetCurrentAvailableExternalTexture(Texture2D.CreateExternalTexture(texture.width, texture.height, TextureFormat.RGBA32, false, isLinear, layerTextures.GetCurrentAvailableTextureID()));
				}

				if (layerTextures.externalTextures[layerTextures.currentAvailableTextureIndex] == null)
				{
					ERROR("SetLayerTexture: Create External Texture Failed.");
					return false;
				}
			}

			//Set Texture Content

			bool isContentSet = layerTextures.textureContentSet[layerTextures.currentAvailableTextureIndex];

			if (!isDynamicLayer && isContentSet)
			{
				return true;
			}

			int currentTextureWidth = layerTextures.GetCurrentAvailableExternalTexture().width;
			int currentTextureHeight = layerTextures.GetCurrentAvailableExternalTexture().height;

			//Set Texture Layout
			offset.x = 0;
			offset.y = 0;
			extent.width = (int)currentTextureWidth;
			extent.height = (int)currentTextureHeight;
			rect.offset = offset;
			rect.extent = extent;

			layerTextures.textureLayout = rect;

			//Blit and copy texture
			RenderTexture srcTexture = texture as RenderTexture;
			int msaaSamples = 1;
			if (srcTexture != null)
			{
				msaaSamples = srcTexture.antiAliasing;
			}

			Material currentBlitMat = texture2DBlitMaterial;

			RenderTextureDescriptor rtDescriptor = new RenderTextureDescriptor(currentTextureWidth, currentTextureHeight, RenderTextureFormat.ARGB32, 0);
			rtDescriptor.msaaSamples = msaaSamples;
			rtDescriptor.autoGenerateMips = false;
			rtDescriptor.useMipMap = false;
			rtDescriptor.sRGB = false;

			RenderTexture blitTempRT = RenderTexture.GetTemporary(rtDescriptor);
			if (!blitTempRT.IsCreated())
			{
				blitTempRT.Create();
			}
			blitTempRT.DiscardContents();

			Texture dstTexture = layerTextures.GetCurrentAvailableExternalTexture();
			Graphics.Blit(texture, blitTempRT, currentBlitMat);
			Graphics.CopyTexture(blitTempRT, 0, 0, dstTexture, 0, 0);

			//DEBUG("Blit and CopyTexture complete.");

			if (blitTempRT != null)
			{
				RenderTexture.ReleaseTemporary(blitTempRT);
			}
			else
			{
				ERROR("blitTempRT not found");
				return false;
			}

			layerTextures.textureContentSet[layerTextures.currentAvailableTextureIndex] = true;

			bool releaseTextureResult = compositionLayerFeature.CompositionLayer_ReleaseTexture(layerID);
			if (releaseTextureResult)
			{
				textureAcquired = false;
			}

			return releaseTextureResult;
		}

		private void GetCompositionLayerPose(ref XrPosef pose) //Call at onBeforeRender
		{
			//Check if overlay is child of hmd transform i.e. head-lock
			Transform currentTransform = transform;
			isHeadLock = false;
			while (currentTransform != null)
			{
				if (currentTransform == hmd.transform) //Overlay is child of hmd transform
				{
					isHeadLock = true;
					break;
				}
				currentTransform = currentTransform.parent;
			}

			if (isHeadLock)
			{
				//Calculate Layer Rotation and Position relative to Camera
				Vector3 relativePosition = hmd.transform.InverseTransformPoint(transform.position);
				Quaternion relativeRotation = Quaternion.Inverse(hmd.transform.rotation).normalized * transform.rotation.normalized;

				UnityToOpenXRConversionHelper.GetOpenXRVector(relativePosition, ref pose.position);
				UnityToOpenXRConversionHelper.GetOpenXRQuaternion(relativeRotation.normalized, ref pose.orientation);
			}
			else
			{
				Vector3 trackingSpacePosition = transform.position;
				Quaternion trackingSpaceRotation = transform.rotation;

				if (trackingOrigin != null) //Apply origin correction to the layer pose
				{
					Matrix4x4 trackingSpaceOriginTRS = Matrix4x4.TRS(trackingOrigin.transform.position, trackingOrigin.transform.rotation, Vector3.one);
					Matrix4x4 worldSpaceLayerPoseTRS = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

					Matrix4x4 trackingSpaceLayerPoseTRS = trackingSpaceOriginTRS.inverse * worldSpaceLayerPoseTRS;

					trackingSpacePosition = trackingSpaceLayerPoseTRS.GetColumn(3); //4th Column of TRS Matrix is the position
					trackingSpaceRotation = Quaternion.LookRotation(trackingSpaceLayerPoseTRS.GetColumn(2), trackingSpaceLayerPoseTRS.GetColumn(1));
				}
				UnityToOpenXRConversionHelper.GetOpenXRVector(trackingSpacePosition, ref pose.position);
				UnityToOpenXRConversionHelper.GetOpenXRQuaternion(trackingSpaceRotation.normalized, ref pose.orientation);
			}
		}

		bool enabledColorScaleBiasInShader = false;
		XrCompositionLayerColorScaleBiasKHR CompositionLayerParamsColorScaleBias = new XrCompositionLayerColorScaleBiasKHR();
		private void SubmitCompositionLayer() //Call at onBeforeRender
		{
			if (!isInitializationComplete && !isLayerReadyForSubmit) return;
			compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();

			if (isInitializationComplete && isSynchronized)
			{
				ViveCompositionLayerColorScaleBias compositionLayerColorScaleBias = OpenXRSettings.Instance.GetFeature<ViveCompositionLayerColorScaleBias>();
				if (applyColorScaleBias && compositionLayerColorScaleBias.ColorScaleBiasExtensionEnabled)
				{
					if (layerType == LayerType.Underlay)
					{
						//if (!enabledColorScaleBiasInShader)
						{
							if (generatedUnderlayMeshRenderer != null && generatedUnderlayMeshRenderer.material != null)
							{
								generatedUnderlayMeshRenderer.material.EnableKeyword("COLOR_SCALE_BIAS_ENABLED");
								enabledColorScaleBiasInShader = true;
							}
						}

						if (generatedUnderlayMeshRenderer != null && generatedUnderlayMeshRenderer.material != null)
						{
							generatedUnderlayMeshRenderer.material.SetColor("_ColorScale", colorScale);
							generatedUnderlayMeshRenderer.material.SetColor("_ColorBias", colorBias);
						}
					}
					else if (enabledColorScaleBiasInShader) //Disable if not underlay
					{
						generatedUnderlayMeshRenderer.material.DisableKeyword("COLOR_SCALE_BIAS_ENABLED");
						enabledColorScaleBiasInShader = false;
					}

					CompositionLayerParamsColorScaleBias.type = XrStructureType.XR_TYPE_COMPOSITION_LAYER_COLOR_SCALE_BIAS_KHR;

					UnityToOpenXRConversionHelper.GetOpenXRColor4f(colorScale, ref CompositionLayerParamsColorScaleBias.colorScale);
					UnityToOpenXRConversionHelper.GetOpenXRColor4f(colorBias, ref CompositionLayerParamsColorScaleBias.colorBias);
					if (!solidEffect && enabledColorScaleBiasInShader)
					{
						CompositionLayerParamsColorScaleBias.colorScale.a = 1.0f;
						CompositionLayerParamsColorScaleBias.colorBias.a = 0.0f;
					}

					compositionLayerColorScaleBias.Submit_CompositionLayerColorBias(CompositionLayerParamsColorScaleBias, layerID);
				}
				else if (enabledColorScaleBiasInShader) //Disable if color scale bias is no longer active
				{
					generatedUnderlayMeshRenderer.material.DisableKeyword("COLOR_SCALE_BIAS_ENABLED");
					enabledColorScaleBiasInShader = false;
				}


				switch (layerShape)
				{
					default:
					case LayerShape.Quad:
						compositionLayerFeature.Submit_CompositionLayerQuad(AssignCompositionLayerParamsQuad(), (OpenXR.CompositionLayer.LayerType)layerType, compositionDepth, layerID);
						break;
					case LayerShape.Cylinder:
						ViveCompositionLayerCylinder compositionLayerCylinderFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayerCylinder>();
						if (compositionLayerCylinderFeature != null && compositionLayerCylinderFeature.CylinderExtensionEnabled)
						{
							compositionLayerCylinderFeature.Submit_CompositionLayerCylinder(AssignCompositionLayerParamsCylinder(), (OpenXR.CompositionLayer.LayerType)layerType, compositionDepth, layerID);
						}
						break;
				}
			}

		}


		public delegate void OnDestroyCompositionLayer();
		public event OnDestroyCompositionLayer OnDestroyCompositionLayerDelegate = null;

		private void DestroyCompositionLayer()
		{
			if (!isInitializationComplete || layerTextures == null)
			{
				DEBUG("DestroyCompositionLayer: Layer already destroyed/not initialized.");
				return;
			}

			DEBUG("DestroyCompositionLayer");

			compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();

			if (textureAcquired)
			{
				DEBUG("DestroyCompositionLayer: textureAcquired, releasing.");
				textureAcquired = !compositionLayerFeature.CompositionLayer_ReleaseTexture(layerID);
			}

			CompositionLayerRenderThreadSyncObject DestroyLayerSwapchainSyncObject = new CompositionLayerRenderThreadSyncObject(
				(taskQueue) =>
				{
					lock (taskQueue)
					{
						CompositionLayerRenderThreadTask task = (CompositionLayerRenderThreadTask)taskQueue.Dequeue();

						//Enumerate Swapchain formats
						compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();

						if (!compositionLayerFeature.CompositionLayer_Destroy(task.layerID))
						{
							ERROR("estroyCompositionLayer: CompositionLayer_Destroy failed.");
						}

						taskQueue.Release(task);
					}
				});

			CompositionLayerRenderThreadTask.IssueDestroySwapchainEvent(DestroyLayerSwapchainSyncObject, layerID);

			InitStatus = false;
			isLayerReadyForSubmit = false;
			isInitializationComplete = false;
			textureAcquiredOnce = false;

			foreach (Texture externalTexture in layerTextures.externalTextures)
			{
				DEBUG("DestroyCompositionLayer: External textures");
				if (externalTexture != null) Destroy(externalTexture);
			}
			layerTextures = null;

			if (generatedFallbackMeshFilter != null && generatedFallbackMeshFilter.mesh != null)
			{
				DEBUG("DestroyCompositionLayer: generatedFallbackMeshFilter");
				Destroy(generatedFallbackMeshFilter.mesh);
				generatedFallbackMeshFilter = null;
			}
			if (generatedFallbackMeshRenderer != null && generatedFallbackMeshRenderer.material != null)
			{
				DEBUG("DestroyCompositionLayer: generatedFallbackMeshRenderer");
				Destroy(generatedFallbackMeshRenderer.material);
				generatedFallbackMeshRenderer = null;
			}

			if (generatedUnderlayMeshFilter != null && generatedUnderlayMeshFilter.mesh != null)
			{
				DEBUG("DestroyCompositionLayer: generatedUnderlayMeshFilter");
				Destroy(generatedUnderlayMeshFilter.mesh);
				generatedUnderlayMeshFilter = null;
			}
			if (generatedUnderlayMeshRenderer != null && generatedUnderlayMeshRenderer.material != null)
			{
				DEBUG("DestroyCompositionLayer: generatedUnderlayMeshRenderer");
				Destroy(generatedUnderlayMeshRenderer.material);
				generatedUnderlayMeshRenderer = null;
			}

			if (generatedFallbackMesh != null)
			{
				Destroy(generatedFallbackMesh);
				generatedFallbackMesh = null;
			}

			if (generatedUnderlayMesh != null)
			{
				Destroy(generatedUnderlayMesh);
				generatedUnderlayMesh = null;
			}

			OnDestroyCompositionLayerDelegate?.Invoke();
		}

		private List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
		XrCompositionLayerQuad CompositionLayerParamsQuad = new XrCompositionLayerQuad();
		XrExtent2Df quadSize = new XrExtent2Df();
		private XrCompositionLayerQuad AssignCompositionLayerParamsQuad()
		{
			compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();

			CompositionLayerParamsQuad.type = XrStructureType.XR_TYPE_COMPOSITION_LAYER_QUAD;
			CompositionLayerParamsQuad.layerFlags = ViveCompositionLayerHelper.XR_COMPOSITION_LAYER_CORRECT_CHROMATIC_ABERRATION_BIT | ViveCompositionLayerHelper.XR_COMPOSITION_LAYER_BLEND_TEXTURE_SOURCE_ALPHA_BIT;

			if (!enabledColorScaleBiasInShader)
			{
				CompositionLayerParamsQuad.layerFlags |= ViveCompositionLayerHelper.XR_COMPOSITION_LAYER_UNPREMULTIPLIED_ALPHA_BIT;
			}

			switch (layerVisibility)
			{
				default:
				case Visibility.Both:
					CompositionLayerParamsQuad.eyeVisibility = XrEyeVisibility.XR_EYE_VISIBILITY_BOTH;
					break;
				case Visibility.Left:
					CompositionLayerParamsQuad.eyeVisibility = XrEyeVisibility.XR_EYE_VISIBILITY_LEFT;
					break;
				case Visibility.Right:
					CompositionLayerParamsQuad.eyeVisibility = XrEyeVisibility.XR_EYE_VISIBILITY_RIGHT;
					break;
			}

			CompositionLayerParamsQuad.subImage.imageRect = layerTextures.textureLayout;
			CompositionLayerParamsQuad.subImage.imageArrayIndex = 0;
			GetCompositionLayerPose(ref CompositionLayerParamsQuad.pose); //Update isHeadLock

			if (isHeadLock)
			{
				CompositionLayerParamsQuad.space = compositionLayerFeature.HeadLockSpace;
			}
			else
			{
				XRInputSubsystem subsystem = null;
				SubsystemManager.GetSubsystems(inputSubsystems);
				if (inputSubsystems.Count > 0)
				{
					subsystem = inputSubsystems[0];
				}

				if (subsystem != null)
				{
					TrackingOriginModeFlags trackingOriginMode = subsystem.GetTrackingOriginMode();

					switch (trackingOriginMode)
					{
						default:
						case TrackingOriginModeFlags.Floor:
							CompositionLayerParamsQuad.space = compositionLayerFeature.WorldLockSpaceOriginOnFloor;
							break;
						case TrackingOriginModeFlags.Device:
							CompositionLayerParamsQuad.space = compositionLayerFeature.WorldLockSpaceOriginOnHead;
							break;
					}
				}
				else
				{
					CompositionLayerParamsQuad.space = compositionLayerFeature.WorldLockSpaceOriginOnFloor;
				}
			}

			quadSize.width = m_QuadWidth;
			quadSize.height = m_QuadHeight;
			CompositionLayerParamsQuad.size = quadSize;

			return CompositionLayerParamsQuad;
		}

		XrCompositionLayerCylinderKHR CompositionLayerParamsCylinder = new XrCompositionLayerCylinderKHR();
		private XrCompositionLayerCylinderKHR AssignCompositionLayerParamsCylinder()
		{
			compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();

			CompositionLayerParamsCylinder.type = XrStructureType.XR_TYPE_COMPOSITION_LAYER_CYLINDER_KHR;
			CompositionLayerParamsCylinder.layerFlags = ViveCompositionLayerHelper.XR_COMPOSITION_LAYER_CORRECT_CHROMATIC_ABERRATION_BIT | ViveCompositionLayerHelper.XR_COMPOSITION_LAYER_BLEND_TEXTURE_SOURCE_ALPHA_BIT;

			if (!enabledColorScaleBiasInShader)
			{
				CompositionLayerParamsCylinder.layerFlags |= ViveCompositionLayerHelper.XR_COMPOSITION_LAYER_UNPREMULTIPLIED_ALPHA_BIT;
			}

			if (isHeadLock)
			{
				CompositionLayerParamsCylinder.space = compositionLayerFeature.HeadLockSpace;
			}
			else
			{
				XRInputSubsystem subsystem = null;
				SubsystemManager.GetSubsystems(inputSubsystems);
				if (inputSubsystems.Count > 0)
				{
					subsystem = inputSubsystems[0];
				}

				if (subsystem != null)
				{
					TrackingOriginModeFlags trackingOriginMode = subsystem.GetTrackingOriginMode();

					switch (trackingOriginMode)
					{
						default:
						case TrackingOriginModeFlags.Floor:
							DEBUG("TrackingOriginModeFlags.Floor");
							CompositionLayerParamsCylinder.space = compositionLayerFeature.WorldLockSpaceOriginOnFloor;
							break;
						case TrackingOriginModeFlags.Device:
							DEBUG("TrackingOriginModeFlags.Device");
							CompositionLayerParamsCylinder.space = compositionLayerFeature.WorldLockSpaceOriginOnHead;
							break;
					}
				}
				else
				{
					CompositionLayerParamsCylinder.space = compositionLayerFeature.WorldLockSpaceOriginOnFloor;
				}
			}

			switch (layerVisibility)
			{
				default:
				case Visibility.Both:
					CompositionLayerParamsCylinder.eyeVisibility = XrEyeVisibility.XR_EYE_VISIBILITY_BOTH;
					break;
				case Visibility.Left:
					CompositionLayerParamsCylinder.eyeVisibility = XrEyeVisibility.XR_EYE_VISIBILITY_LEFT;
					break;
				case Visibility.Right:
					CompositionLayerParamsCylinder.eyeVisibility = XrEyeVisibility.XR_EYE_VISIBILITY_RIGHT;
					break;
			}

			CompositionLayerParamsCylinder.subImage.imageRect = layerTextures.textureLayout;
			CompositionLayerParamsCylinder.subImage.imageArrayIndex = 0;
			GetCompositionLayerPose(ref CompositionLayerParamsCylinder.pose);
			CompositionLayerParamsCylinder.radius = m_CylinderRadius;
			CompositionLayerParamsCylinder.centralAngle = Mathf.Deg2Rad * m_CylinderAngleOfArc;
			CompositionLayerParamsCylinder.aspectRatio = m_CylinderArcLength / m_CylinderHeight;

			return CompositionLayerParamsCylinder;
		}

		private void ActivatePlaceholder()
		{
			if (Debug.isDebugBuild && !placeholderGenerated)
			{
				if (CompositionLayerManager.GetInstance().MaxLayerCount() == 0)//Platform does not support multi-layer. Show placeholder instead if in development build
				{
					DEBUG("Generate Placeholder");
					compositionLayerPlaceholderPrefabGO = Instantiate((GameObject)Resources.Load("Prefabs/CompositionLayerDebugPlaceholder", typeof(GameObject)));
					compositionLayerPlaceholderPrefabGO.name = "CompositionLayerDebugPlaceholder";
					compositionLayerPlaceholderPrefabGO.transform.SetParent(this.transform);
					compositionLayerPlaceholderPrefabGO.transform.position = this.transform.position;
					compositionLayerPlaceholderPrefabGO.transform.rotation = this.transform.rotation;
					compositionLayerPlaceholderPrefabGO.transform.localScale = this.transform.localScale;

					Text placeholderText = compositionLayerPlaceholderPrefabGO.transform.GetChild(0).Find("Text").GetComponent<Text>();

					placeholderText.text = placeholderText.text.Replace("{REASON}", "Device does not support Multi-Layer");

					placeholderGenerated = true;
				}
				else if (CompositionLayerManager.GetInstance().MaxLayerCount() <= CompositionLayerManager.GetInstance().CurrentLayerCount())//Do not draw layer as limit is reached. Show placeholder instead if in development build 
				{
					DEBUG("Generate Placeholder");
					compositionLayerPlaceholderPrefabGO = Instantiate((GameObject)Resources.Load("Prefabs/CompositionLayerDebugPlaceholder", typeof(GameObject)));
					compositionLayerPlaceholderPrefabGO.name = "CompositionLayerDebugPlaceholder";
					compositionLayerPlaceholderPrefabGO.transform.SetParent(this.transform);
					compositionLayerPlaceholderPrefabGO.transform.position = this.transform.position;
					compositionLayerPlaceholderPrefabGO.transform.rotation = this.transform.rotation;
					compositionLayerPlaceholderPrefabGO.transform.localScale = this.transform.localScale;

					Text placeholderText = compositionLayerPlaceholderPrefabGO.transform.GetChild(0).Find("Text").GetComponent<Text>();

					placeholderText.text = placeholderText.text.Replace("{REASON}", "Max number of layers exceeded");

					placeholderGenerated = true;
				}
			}
			else if (placeholderGenerated && compositionLayerPlaceholderPrefabGO != null)
			{
				DEBUG("Placeholder already generated, activating.");
				compositionLayerPlaceholderPrefabGO.SetActive(true);
			}
		}

		public bool RenderAsLayer()
		{
			if (placeholderGenerated && compositionLayerPlaceholderPrefabGO != null)
			{
				compositionLayerPlaceholderPrefabGO.SetActive(false);
			}

			if (isAutoFallbackActive)
			{
				generatedFallbackMesh.SetActive(false);
				isAutoFallbackActive = false;
			}

			isRenderPriorityChanged = false;

			//if Underlay Mesh is present but needs to be reconstructed
			if (layerType == LayerType.Underlay)
			{
				if (!UnderlayMeshIsValid()) //Underlay Mesh needs to be generated
				{
					UnderlayMeshGeneration();
				}
				else if (LayerDimensionsChanged()) //if Underlay Mesh is present but needs to be updated
				{
					UnderlayMeshUpdate();
				}
				else generatedUnderlayMesh.SetActive(true);
			}

			return CompositionLayerInit();
		}

		public void RenderInGame()
		{
			compositionLayerFeature = compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();
			if (compositionLayerFeature.enableAutoFallback)
			{
				if (!isAutoFallbackActive)
				{
					//Activate auto fallback
					//Activate auto fallback
					if (!FallbackMeshIsValid())
					{
						AutoFallbackMeshGeneration();
					}
					else if (LayerDimensionsChanged())
					{
						AutoFallbackMeshUpdate();
					}
					generatedFallbackMesh.SetActive(true);
					isAutoFallbackActive = true;
				}
			}
			else //Use placeholder
			{
				ActivatePlaceholder();
			}

			if (layerType == LayerType.Underlay && generatedUnderlayMesh != null)
			{
				generatedUnderlayMesh.SetActive(false);
			}

			isRenderPriorityChanged = false;
		}

		public void TerminateLayer()
		{
			DEBUG("TerminateLayer: layerID: " + layerID);
			DestroyCompositionLayer();

			if (placeholderGenerated && compositionLayerPlaceholderPrefabGO != null)
			{
				compositionLayerPlaceholderPrefabGO.SetActive(false);
			}

			if (isAutoFallbackActive)
			{
				generatedFallbackMesh.SetActive(false);
				isAutoFallbackActive = false;
			}
		}

		public bool TextureParamsChanged()
		{
			if (previousTexture != texture)
			{
				previousTexture = texture;
				return true;
			}

			if (previousIsDynamicLayer != isDynamicLayer)
			{
				previousIsDynamicLayer = isDynamicLayer;
				return true;
			}

			return false;
		}

		public bool LayerDimensionsChanged()
		{
			bool isChanged = false;

			if (layerShape == LayerShape.Cylinder)
			{
				if (previousAngleOfArc != m_CylinderAngleOfArc ||
					previousCylinderArcLength != m_CylinderArcLength ||
					previousCylinderHeight != m_CylinderHeight ||
					previousCylinderRadius != m_CylinderRadius)
				{
					previousAngleOfArc = m_CylinderAngleOfArc;
					previousCylinderArcLength = m_CylinderArcLength;
					previousCylinderHeight = m_CylinderHeight;
					previousCylinderRadius = m_CylinderRadius;
					isChanged = true;
				}
			}
			else if (layerShape == LayerShape.Quad)
			{
				if (previousQuadWidth != m_QuadWidth ||
					previousQuadHeight != m_QuadHeight)
				{
					previousQuadWidth = m_QuadWidth;
					previousQuadHeight = m_QuadHeight;
					isChanged = true;
				}
			}

			if (previousLayerShape != layerShape)
			{
				previousLayerShape = layerShape;
				isChanged = true;
			}

			return isChanged;
		}

		#region Quad Runtime Parameter Change
		/// <summary>
		/// Use this function to update the width of a Quad Layer.
		/// </summary>
		/// <param name="inWidth"></param>
		/// <returns></returns>
		public bool SetQuadLayerWidth(float inWidth)
		{
			if (inWidth <= 0)
			{
				return false;
			}

			m_QuadWidth = inWidth;

			return true;
		}

		/// <summary>
		/// Use this function to update the height of a Quad Layer.
		/// </summary>
		/// <param name="inHeight"></param>
		/// <returns></returns>
		public bool SetQuadLayerHeight(float inHeight)
		{
			if (inHeight <= 0)
			{
				return false;
			}

			m_QuadHeight = inHeight;

			return true;
		}
		#endregion

		#region Cylinder Runtime Parameter Change
		/// <summary>
		/// Use this function to update the radius and arc angle of a Cylinder Layer. 
		/// A new arc length will be calculated automatically.
		/// </summary>
		/// <param name="inRadius"></param>
		/// <param name="inArcAngle"></param>
		/// <returns>True if the parameters are valid and successfully updated.</returns>
		public bool SetCylinderLayerRadiusAndArcAngle(float inRadius, float inArcAngle)
		{
			//Check if radius is valid
			if (inRadius <= 0)
			{
				return false;
			}

			//Check if angle of arc is valid
			if (inArcAngle < angleOfArcLowerLimit || inArcAngle > angleOfArcUpperLimit)
			{
				return false;
			}

			//Check if new arc length is valid
			float newArcLength = CylinderParameterHelper.RadiusAndDegAngleOfArcToArcLength(inArcAngle, inRadius);
			if (newArcLength <= 0)
			{
				return false;
			}

			//All parameters are valid, assign to layer
			m_CylinderArcLength = newArcLength;
			m_CylinderRadius = inRadius;
			m_CylinderAngleOfArc = inArcAngle;

			return true;
		}

		/// <summary>
		/// Use this function to update the radius and arc length of a Cylinder Layer. 
		/// A new arc angle will be calculated automatically.
		/// </summary>
		/// <param name="inRadius"></param>
		/// <param name="inArcLength"></param>
		/// <returns>True if the parameters are valid and successfully updated.</returns>
		public bool SetCylinderLayerRadiusAndArcLength(float inRadius, float inArcLength)
		{
			//Check if radius is valid
			if (inRadius <= 0)
			{
				return false;
			}

			//Check if arc length is valid
			if (inArcLength <= 0)
			{
				return false;
			}

			//Check if new arc angle is valid
			float newArcAngle = CylinderParameterHelper.RadiusAndArcLengthToDegAngleOfArc(inArcLength, inRadius);
			if (newArcAngle < angleOfArcLowerLimit || newArcAngle > angleOfArcUpperLimit)
			{
				return false;
			}

			//All parameters are valid, assign to layer
			m_CylinderArcLength = inArcLength;
			m_CylinderRadius = inRadius;
			m_CylinderAngleOfArc = newArcAngle;

			return true;
		}

		/// <summary>
		/// Use this function to update the arc angle and arc length of a Cylinder Layer. 
		/// A new radius will be calculated automatically.
		/// </summary>
		/// <param name="inArcAngle"></param>
		/// <param name="inArcLength"></param>
		/// <returns>True if the parameters are valid and successfully updated.</returns>
		public bool SetCylinderLayerArcAngleAndArcLength(float inArcAngle, float inArcLength)
		{
			//Check if arc angle is valid
			if (inArcAngle < angleOfArcLowerLimit || inArcAngle > angleOfArcUpperLimit)
			{
				return false;
			}

			//Check if arc length is valid
			if (inArcLength <= 0)
			{
				return false;
			}

			//Check if new radius is valid
			float newRadius = CylinderParameterHelper.ArcLengthAndDegAngleOfArcToRadius(inArcLength, inArcAngle);
			if (newRadius <= 0)
			{
				return false;
			}

			//All parameters are valid, assign to layer
			m_CylinderArcLength = inArcLength;
			m_CylinderRadius = newRadius;
			m_CylinderAngleOfArc = inArcAngle;

			return true;

		}

		/// <summary>
		/// Use this function to update the height of a Cylinder Layer.
		/// </summary>
		/// <param name="inHeight"></param>
		/// <returns></returns>
		public bool SetCylinderLayerHeight(float inHeight)
		{
			if (inHeight <= 0)
			{
				return false;
			}

			m_CylinderHeight = inHeight;

			return true;
		}

		#endregion

#if UNITY_EDITOR
		public CylinderLayerParamAdjustmentMode CurrentAdjustmentMode()
		{
			if (previousCylinderArcLength != m_CylinderArcLength)
			{
				return CylinderLayerParamAdjustmentMode.ArcLength;
			}
			else if (previousAngleOfArc != m_CylinderAngleOfArc)
			{
				return CylinderLayerParamAdjustmentMode.ArcAngle;
			}
			else
			{
				return CylinderLayerParamAdjustmentMode.Radius;
			}
		}
#endif

		public void ChangeBlitShadermode(BlitShaderMode shaderMode, bool enable)
		{
			if (texture2DBlitMaterial == null) return;

			switch (shaderMode)
			{
				case BlitShaderMode.LINEAR_TO_SRGB_COLOR:
					if (enable)
					{
						texture2DBlitMaterial.EnableKeyword("LINEAR_TO_SRGB_COLOR");
					}
					else
					{
						texture2DBlitMaterial.DisableKeyword("LINEAR_TO_SRGB_COLOR");
					}
					break;
				case BlitShaderMode.LINEAR_TO_SRGB_ALPHA:
					if (enable)
					{
						texture2DBlitMaterial.EnableKeyword("LINEAR_TO_SRGB_ALPHA");
					}
					else
					{
						texture2DBlitMaterial.DisableKeyword("LINEAR_TO_SRGB_ALPHA");
					}
					break;
				default:
					break;
			}
		}
		#endregion

		#region Monobehavior Lifecycle
		private void Awake()
		{
			//Create blit mat
			texture2DBlitMaterial = new Material(Shader.Find("VIVE/OpenXR/CompositionLayer/Texture2DBlitShader"));

			//Create render thread synchornizer
			if (synchronizer == null) synchronizer = new RenderThreadSynchronizer();

			ColorSpace currentActiveColorSpace = QualitySettings.activeColorSpace;
			if (currentActiveColorSpace == ColorSpace.Linear)
			{
				isLinear = true;
			}
			else
			{
				isLinear = false;
			}

			compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();
		}

		private void OnEnable()
		{
			hmd = Camera.main;

			CompositionLayerManager.GetInstance().SubscribeToLayerManager(this);
			if (!isOnBeforeRenderSubscribed)
			{
				CompositionLayerManager.GetInstance().CompositionLayerManagerOnBeforeRenderDelegate += OnBeforeRender;
				isOnBeforeRenderSubscribed = true;
			}
		}

		private void OnDisable()
		{
			if (isOnBeforeRenderSubscribed)
			{
				CompositionLayerManager.GetInstance().CompositionLayerManagerOnBeforeRenderDelegate -= OnBeforeRender;
				isOnBeforeRenderSubscribed = false;
			}
			CompositionLayerManager.GetInstance().UnsubscribeFromLayerManager(this, false);
		}

		private void OnDestroy()
		{
			if (isOnBeforeRenderSubscribed)
			{
				CompositionLayerManager.GetInstance().CompositionLayerManagerOnBeforeRenderDelegate -= OnBeforeRender;
				isOnBeforeRenderSubscribed = false;
			}

			Destroy(texture2DBlitMaterial);

			CompositionLayerManager.GetInstance().UnsubscribeFromLayerManager(this, true);
		}

		private void LateUpdate()
		{
			if (isAutoFallbackActive) //Do not submit when auto fallback is active
			{
				//Check if auto fallback mesh needs to be updated
				if (!FallbackMeshIsValid()) //fallback Mesh needs to be generated
				{
					AutoFallbackMeshGeneration();
				}
				else if (LayerDimensionsChanged()) //if fallback Mesh is present but needs to be updated
				{
					AutoFallbackMeshUpdate();
				}

				//Handle possible lossy scale change
				if (generatedFallbackMesh.transform.lossyScale != Vector3.one)
				{
					generatedFallbackMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);
				}

				return;
			}

			if (layerType == LayerType.Underlay)
			{
				if (!UnderlayMeshIsValid()) //Underlay Mesh needs to be generated
				{
					UnderlayMeshGeneration();
				}
				else if (LayerDimensionsChanged()) //if Underlay Mesh is present but needs to be updated
				{
					UnderlayMeshUpdate();
				}

				//Handle possible lossy scale change
				if (generatedUnderlayMesh.transform.lossyScale != Vector3.one)
				{
					generatedUnderlayMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);
				}

				generatedUnderlayMesh.SetActive(true);
			}
		}

		private void OnBeforeRender()
		{
			compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();

			isLayerReadyForSubmit = false;

			if (compositionLayerFeature.XrSessionEnding) return;

			if (!isInitializationComplete) //Layer not marked as initialized
			{
				if (InitStatus) //Initialized
				{
					reinitializationNeeded = false;
					isInitializationComplete = true;
					isSynchronized = false;
				}
				else if (reinitializationNeeded) //Layer is still active but needs to be reinitialized
				{
					CompositionLayerInit();
					return;
				}
				else
				{
					DEBUG("Composition Layer Lifecycle OnBeforeRender: Layer not initialized.");
					return;
				}
			}

			if (!isSynchronized)
			{
				DEBUG("CompositionLayer: Sync");
				if (synchronizer != null)
				{
					synchronizer.sync();
					isSynchronized = true;
				}
			}

			if (isAutoFallbackActive || ((compositionLayerPlaceholderPrefabGO != null) && compositionLayerPlaceholderPrefabGO.activeSelf)) //Do not submit when auto fallback or placeholder is active
			{
				return;
			}

			if (compositionLayerFeature.XrSessionCurrentState != XrSessionState.XR_SESSION_STATE_VISIBLE && compositionLayerFeature.XrSessionCurrentState != XrSessionState.XR_SESSION_STATE_FOCUSED)
			{
				//DEBUG("XrSessionCurrentState is not focused or visible");
				return;
			}

			if (SetLayerTexture())
			{
				isLayerReadyForSubmit = true;
			}

			if (!isLayerReadyForSubmit)
			{
				DEBUG("Composition Layer Lifecycle OnBeforeRender: Layer not ready for submit.");
				return;
			}
			SubmitCompositionLayer();

			isLayerReadyForSubmit = false; //reset flag after submit
		}

		#endregion

		#region Mesh Generation
		public void AutoFallbackMeshGeneration()
		{
			if (generatedFallbackMeshFilter != null && generatedFallbackMeshFilter.mesh != null)
			{
				Destroy(generatedFallbackMeshFilter.mesh);
			}
			if (generatedFallbackMeshRenderer != null && generatedFallbackMeshRenderer.material != null)
			{
				Destroy(generatedFallbackMeshRenderer.material);
			}
			if (generatedFallbackMesh != null) Destroy(generatedFallbackMesh);

			Mesh generatedMesh = null;

			switch (layerShape)
			{
				case LayerShape.Quad:
					generatedMesh = MeshGenerationHelper.GenerateQuadMesh(MeshGenerationHelper.GenerateQuadVertex(m_QuadWidth, m_QuadHeight));
					break;
				case LayerShape.Cylinder:
					generatedMesh = MeshGenerationHelper.GenerateCylinderMesh(m_CylinderAngleOfArc, MeshGenerationHelper.GenerateCylinderVertex(m_CylinderAngleOfArc, m_CylinderRadius, m_CylinderHeight));
					break;
			}

			generatedFallbackMesh = new GameObject();
			generatedFallbackMesh.SetActive(false);

			generatedFallbackMesh.name = FallbackMeshName;
			generatedFallbackMesh.transform.SetParent(gameObject.transform);
			generatedFallbackMesh.transform.localPosition = Vector3.zero;
			generatedFallbackMesh.transform.localRotation = Quaternion.identity;

			generatedFallbackMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);

			generatedFallbackMeshRenderer = generatedFallbackMesh.AddComponent<MeshRenderer>();
			generatedFallbackMeshFilter = generatedFallbackMesh.AddComponent<MeshFilter>();

			generatedFallbackMeshFilter.mesh = generatedMesh;

			Material fallBackMat = new Material(Shader.Find("Unlit/Transparent"));
			fallBackMat.mainTexture = texture;
			generatedFallbackMeshRenderer.material = fallBackMat;
		}

		public void AutoFallbackMeshUpdate()
		{
			if (generatedFallbackMesh == null || generatedFallbackMeshRenderer == null || generatedFallbackMeshFilter == null)
			{
				return;
			}

			Mesh generatedMesh = null;

			switch (layerShape)
			{
				case LayerShape.Quad:
					generatedMesh = MeshGenerationHelper.GenerateQuadMesh(MeshGenerationHelper.GenerateQuadVertex(m_QuadWidth, m_QuadHeight));
					break;
				case LayerShape.Cylinder:
					generatedMesh = MeshGenerationHelper.GenerateCylinderMesh(m_CylinderAngleOfArc, MeshGenerationHelper.GenerateCylinderVertex(m_CylinderAngleOfArc, m_CylinderRadius, m_CylinderHeight));
					break;
			}

			generatedFallbackMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);
			Destroy(generatedFallbackMeshFilter.mesh);
			generatedFallbackMeshFilter.mesh = generatedMesh;
			generatedFallbackMeshRenderer.material.mainTexture = texture;
		}

		public bool FallbackMeshIsValid()
		{
			if (generatedFallbackMesh == null || generatedFallbackMeshRenderer == null || generatedFallbackMeshFilter == null)
			{
				return false;
			}
			else if (generatedFallbackMeshFilter.mesh == null || generatedFallbackMeshRenderer.material == null)
			{
				return false;
			}
			return true;
		}

		public void UnderlayMeshGeneration()
		{
			if (generatedUnderlayMeshFilter != null && generatedUnderlayMeshFilter.mesh != null)
			{
				Destroy(generatedUnderlayMeshFilter.mesh);
			}
			if (generatedUnderlayMeshRenderer != null && generatedUnderlayMeshRenderer.material != null)
			{
				Destroy(generatedUnderlayMeshRenderer.material);
			}
			if (generatedUnderlayMesh != null) Destroy(generatedUnderlayMesh);

			switch (layerShape)
			{
				case LayerShape.Cylinder:
					//Generate vertices
					Vector3[] cylinderVertices = MeshGenerationHelper.GenerateCylinderVertex(m_CylinderAngleOfArc, m_CylinderRadius, m_CylinderHeight);

					//Add components to Game Object
					generatedUnderlayMesh = new GameObject();
					generatedUnderlayMesh.name = CylinderUnderlayMeshName;
					generatedUnderlayMesh.transform.SetParent(transform);
					generatedUnderlayMesh.transform.localPosition = Vector3.zero;
					generatedUnderlayMesh.transform.localRotation = Quaternion.identity;

					generatedUnderlayMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);

					generatedUnderlayMeshRenderer = generatedUnderlayMesh.AddComponent<MeshRenderer>();
					generatedUnderlayMeshFilter = generatedUnderlayMesh.AddComponent<MeshFilter>();
					if (solidEffect)
						generatedUnderlayMeshRenderer.sharedMaterial = new Material(Shader.Find("VIVE/OpenXR/CompositionLayer/UnderlayAlphaZeroSolid"));
					else
						generatedUnderlayMeshRenderer.sharedMaterial = new Material(Shader.Find("VIVE/OpenXR/CompositionLayer/UnderlayAlphaZero"));
					generatedUnderlayMeshRenderer.material.mainTexture = texture;

					//Generate Mesh
					generatedUnderlayMeshFilter.mesh = MeshGenerationHelper.GenerateCylinderMesh(m_CylinderAngleOfArc, cylinderVertices);
					break;
				case LayerShape.Quad:
				default:
					//Generate vertices
					Vector3[] quadVertices = MeshGenerationHelper.GenerateQuadVertex(m_QuadWidth, m_QuadHeight);

					//Add components to Game Object
					generatedUnderlayMesh = new GameObject();
					generatedUnderlayMesh.name = QuadUnderlayMeshName;
					generatedUnderlayMesh.transform.SetParent(transform);
					generatedUnderlayMesh.transform.localPosition = Vector3.zero;
					generatedUnderlayMesh.transform.localRotation = Quaternion.identity;

					generatedUnderlayMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);

					generatedUnderlayMeshRenderer = generatedUnderlayMesh.AddComponent<MeshRenderer>();
					generatedUnderlayMeshFilter = generatedUnderlayMesh.AddComponent<MeshFilter>();
					if (solidEffect)
						generatedUnderlayMeshRenderer.material = new Material(Shader.Find("VIVE/OpenXR/CompositionLayer/UnderlayAlphaZeroSolid"));
					else
						generatedUnderlayMeshRenderer.material = new Material(Shader.Find("VIVE/OpenXR/CompositionLayer/UnderlayAlphaZero"));
					generatedUnderlayMeshRenderer.material.mainTexture = texture;

					//Generate Mesh
					generatedUnderlayMeshFilter.mesh = MeshGenerationHelper.GenerateQuadMesh(quadVertices);
					break;
			}
		}

		public void UnderlayMeshUpdate()
		{
			if (generatedUnderlayMesh == null || generatedUnderlayMeshRenderer == null || generatedUnderlayMeshFilter == null)
			{
				return;
			}

			switch (layerShape)
			{
				case LayerShape.Cylinder:
					//Generate vertices
					Vector3[] cylinderVertices = MeshGenerationHelper.GenerateCylinderVertex(m_CylinderAngleOfArc, m_CylinderRadius, m_CylinderHeight);

					//Generate Mesh
					Destroy(generatedUnderlayMeshFilter.mesh);
					generatedUnderlayMeshFilter.mesh = MeshGenerationHelper.GenerateCylinderMesh(m_CylinderAngleOfArc, cylinderVertices);
					break;
				case LayerShape.Quad:
				default:
					//Generate vertices
					Vector3[] quadVertices = MeshGenerationHelper.GenerateQuadVertex(m_QuadWidth, m_QuadHeight);

					//Generate Mesh
					Destroy(generatedUnderlayMeshFilter.mesh);
					generatedUnderlayMeshFilter.mesh = MeshGenerationHelper.GenerateQuadMesh(quadVertices);
					break;
			}

			generatedUnderlayMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);
		}

		public Vector3 GetNormalizedLocalScale(Transform targetTransform, Vector3 targetGlobalScale) //Return the local scale needed to make it match to the target global scale
		{
			Vector3 normalizedLocalScale = new Vector3(targetGlobalScale.x / targetTransform.lossyScale.x, targetGlobalScale.y / targetTransform.lossyScale.y, targetGlobalScale.z / targetTransform.lossyScale.z);

			return normalizedLocalScale;
		}

		public bool UnderlayMeshIsValid()
		{
			if (generatedUnderlayMesh == null || generatedUnderlayMeshRenderer == null || generatedUnderlayMeshFilter == null)
			{
				return false;
			}
			else if (generatedUnderlayMeshFilter.mesh == null || generatedUnderlayMeshRenderer.material == null)
			{
				return false;
			}

			return true;
		}

		#endregion

		#region Enum Definitions
		public enum LayerType
		{
			Overlay = 1,
			Underlay = 2,
		}

		public enum LayerShape
		{
			Quad = 0,
			Cylinder = 1,
		}

		public enum Visibility
		{
			Both = 0,
			Left = 1,
			Right = 2,
		}

#if UNITY_EDITOR
		public enum CylinderLayerParamAdjustmentMode
		{
			Radius = 0,
			ArcLength = 1,
			ArcAngle = 2,
		}

		public enum CylinderLayerParamLockMode
		{
			ArcLength = 0,
			ArcAngle = 1,
		}
#endif

		public enum BlitShaderMode
		{
			LINEAR_TO_SRGB_COLOR = 0,
			LINEAR_TO_SRGB_ALPHA = 1,
		}
		#endregion

		#region Helper Classes
		private class LayerTextures
		{
			private uint layerTextureQueueLength;

			public uint currentAvailableTextureIndex { get; set; }
			public IntPtr[] textureIDs;
			public Texture[] externalTextures;
			public bool[] textureContentSet;
			public XrRect2Di textureLayout { get; set; }

			public LayerTextures(uint swapchainImageCount)
			{
				layerTextureQueueLength = swapchainImageCount;
				textureIDs = new IntPtr[swapchainImageCount];
				externalTextures = new Texture[swapchainImageCount];
				textureContentSet = new bool[swapchainImageCount];

				for (int i = 0; i < swapchainImageCount; i++)
				{
					textureContentSet[i] = false;
					textureIDs[i] = IntPtr.Zero;
				}
			}

			public IntPtr GetCurrentAvailableTextureID()
			{
				if (currentAvailableTextureIndex < 0 || currentAvailableTextureIndex > layerTextureQueueLength - 1)
				{
					return IntPtr.Zero;
				}
				return textureIDs[currentAvailableTextureIndex];
			}

			public void SetCurrentAvailableTextureID(IntPtr newTextureID)
			{
				if (currentAvailableTextureIndex < 0 || currentAvailableTextureIndex > layerTextureQueueLength - 1)
				{
					return;
				}
				textureIDs[currentAvailableTextureIndex] = newTextureID;
			}

			public Texture GetCurrentAvailableExternalTexture()
			{
				if (currentAvailableTextureIndex < 0 || currentAvailableTextureIndex > layerTextureQueueLength - 1)
				{
					return null;
				}
				return externalTextures[currentAvailableTextureIndex];
			}

			public void SetCurrentAvailableExternalTexture(Texture newExternalTexture)
			{
				if (currentAvailableTextureIndex < 0 || currentAvailableTextureIndex > layerTextureQueueLength - 1)
				{
					return;
				}
				externalTextures[currentAvailableTextureIndex] = newExternalTexture;
			}
		}

		private class CompositionLayerRenderThreadTask : Task
		{
			public int layerID;

			public CompositionLayerRenderThreadTask() { }

			public static void IssueObtainSwapchainEvent(CompositionLayerRenderThreadSyncObject syncObject)
			{
				PreAllocatedQueue taskQueue = syncObject.Queue;
				lock (taskQueue)
				{
					CompositionLayerRenderThreadTask task = taskQueue.Obtain<CompositionLayerRenderThreadTask>();
					taskQueue.Enqueue(task);
				}
				syncObject.IssueEvent();
			}

			public static void IssueDestroySwapchainEvent(CompositionLayerRenderThreadSyncObject syncObject, int inLayerID)
			{
				PreAllocatedQueue taskQueue = syncObject.Queue;
				lock (taskQueue)
				{
					CompositionLayerRenderThreadTask task = taskQueue.Obtain<CompositionLayerRenderThreadTask>();
					task.layerID = inLayerID;
					taskQueue.Enqueue(task);
				}
				syncObject.IssueEvent();
			}
		}

		private class RenderThreadSynchronizer
		{
			RenderTexture mutable = new RenderTexture(1, 1, 0);
			public RenderThreadSynchronizer()
			{
				mutable.useMipMap = false;
				mutable.Create();
			}

			public void sync()
			{
				var originalLogType = Application.GetStackTraceLogType(LogType.Error);
				Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);

				// Sync
				mutable.GetNativeTexturePtr();

				Application.SetStackTraceLogType(LogType.Error, originalLogType);
			}
		}

		private class UnityToOpenXRConversionHelper
		{
			public static void GetOpenXRQuaternion(Quaternion rot, ref XrQuaternionf qua)
			{
				qua.x = rot.x;
				qua.y = rot.y;
				qua.z = -rot.z;
				qua.w = -rot.w;
			}

			public static void GetOpenXRVector(Vector3 pos, ref XrVector3f vec)
			{
				vec.x = pos.x;
				vec.y = pos.y;
				vec.z = -pos.z;
			}

			public static void GetOpenXRColor4f(Color color, ref XrColor4f color4f)
			{
				color4f.r = color.r;
				color4f.g = color.g;
				color4f.b = color.b;
				color4f.a = color.a;
			}
		}

		public static class MeshGenerationHelper
		{
			public static Vector3[] GenerateQuadVertex(float quadWidth, float quadHeight)
			{
				Vector3[] vertices = new Vector3[4]; //Four corners

				vertices[0] = new Vector3(-quadWidth / 2, -quadHeight / 2, 0); //Bottom Left
				vertices[1] = new Vector3(quadWidth / 2, -quadHeight / 2, 0); //Bottom Right
				vertices[2] = new Vector3(-quadWidth / 2, quadHeight / 2, 0); //Top Left
				vertices[3] = new Vector3(quadWidth / 2, quadHeight / 2, 0); //Top Right

				return vertices;
			}

			public static Mesh GenerateQuadMesh(Vector3[] vertices)
			{
				Mesh quadMesh = new Mesh();
				quadMesh.vertices = vertices;

				//Create array that represents vertices of the triangles
				int[] triangles = new int[6];
				triangles[0] = 0;
				triangles[1] = 2;
				triangles[2] = 1;

				triangles[3] = 1;
				triangles[4] = 2;
				triangles[5] = 3;

				quadMesh.triangles = triangles;
				Vector2[] uv = new Vector2[vertices.Length];
				Vector4[] tangents = new Vector4[vertices.Length];
				Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
				for (int i = 0, y = 0; y < 2; y++)
				{
					for (int x = 0; x < 2; x++, i++)
					{
						uv[i] = new Vector2((float)x, (float)y);
						tangents[i] = tangent;
					}
				}
				quadMesh.uv = uv;
				quadMesh.tangents = tangents;
				quadMesh.RecalculateNormals();

				return quadMesh;
			}

			public static Vector3[] GenerateCylinderVertex(float cylinderAngleOfArc, float cylinderRadius, float cylinderHeight)
			{
				float angleUpperLimitDeg = cylinderAngleOfArc / 2; //Degrees
				float angleLowerLimitDeg = -angleUpperLimitDeg; //Degrees

				float angleUpperLimitRad = angleUpperLimitDeg * Mathf.Deg2Rad; //Radians
				float angleLowerLimitRad = angleLowerLimitDeg * Mathf.Deg2Rad; //Radians

				int arcSegments = Mathf.RoundToInt(cylinderAngleOfArc / 5f);

				float anglePerArcSegmentRad = (cylinderAngleOfArc / arcSegments) * Mathf.Deg2Rad;

				Vector3[] vertices = new Vector3[2 * (arcSegments + 1)]; //Top and bottom lines * Vertex count per line

				int vertexCount = 0;
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < arcSegments + 1; j++) //Clockwise
					{
						float currentVertexAngleRad = angleLowerLimitRad + anglePerArcSegmentRad * j;
						float x = cylinderRadius * Mathf.Sin(currentVertexAngleRad);
						float y = 0;
						float z = cylinderRadius * Mathf.Cos(currentVertexAngleRad);

						if (i == 1) //Top
						{
							y += cylinderHeight / 2;

						}
						else //Bottom
						{
							y -= cylinderHeight / 2;
						}

						vertices[vertexCount] = new Vector3(x, y, z);
						vertexCount++;
					}
				}

				return vertices;
			}

			public static Mesh GenerateCylinderMesh(float cylinderAngleOfArc, Vector3[] vertices)
			{
				Mesh cylinderMesh = new Mesh();
				cylinderMesh.vertices = vertices;
				int arcSegments = Mathf.RoundToInt(cylinderAngleOfArc / 5f);

				//Create array that represents vertices of the triangles
				int[] triangles = new int[arcSegments * 6];
				for (int currentTriangleIndex = 0, currentVertexIndex = 0, y = 0; y < 1; y++, currentVertexIndex++)
				{
					for (int x = 0; x < arcSegments; x++, currentTriangleIndex += 6, currentVertexIndex++)
					{
						triangles[currentTriangleIndex] = currentVertexIndex;
						triangles[currentTriangleIndex + 1] = currentVertexIndex + arcSegments + 1;
						triangles[currentTriangleIndex + 2] = currentVertexIndex + 1;

						triangles[currentTriangleIndex + 3] = currentVertexIndex + 1;
						triangles[currentTriangleIndex + 4] = currentVertexIndex + arcSegments + 1;
						triangles[currentTriangleIndex + 5] = currentVertexIndex + arcSegments + 2;
					}
				}
				cylinderMesh.triangles = triangles;
				Vector2[] uv = new Vector2[vertices.Length];
				Vector4[] tangents = new Vector4[vertices.Length];
				Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
				for (int i = 0, y = 0; y < 2; y++)
				{
					for (int x = 0; x < arcSegments + 1; x++, i++)
					{
						uv[i] = new Vector2((float)x / arcSegments, (float)y);
						tangents[i] = tangent;
					}
				}
				cylinderMesh.uv = uv;
				cylinderMesh.tangents = tangents;
				cylinderMesh.RecalculateNormals();

				return cylinderMesh;
			}
		}

		public static class CylinderParameterHelper
		{
			public static float RadiusAndDegAngleOfArcToArcLength(float inDegAngleOfArc, float inRadius)
			{
				float arcLength = inRadius * (inDegAngleOfArc * Mathf.Deg2Rad);

				return arcLength;
			}

			public static float RadiusAndArcLengthToDegAngleOfArc(float inArcLength, float inRadius)
			{
				float degAngleOfArc = (inArcLength / inRadius) * Mathf.Rad2Deg;

				return degAngleOfArc;
			}

			public static float ArcLengthAndDegAngleOfArcToRadius(float inArcLength, float inDegAngleOfArc)
			{
				float radius = (inArcLength / (inDegAngleOfArc * Mathf.Deg2Rad));

				return radius;
			}
		}
		#endregion
	}
}
