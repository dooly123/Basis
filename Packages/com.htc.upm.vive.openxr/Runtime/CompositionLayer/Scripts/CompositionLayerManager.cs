// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.CompositionLayer;

namespace VIVE.OpenXR.CompositionLayer
{
	public class CompositionLayerManager : MonoBehaviour
	{
		private uint maxLayerCount = 0;

		private static CompositionLayerManager instance = null;
		private List<CompositionLayer> compositionLayers = new List<CompositionLayer>();
		private List<CompositionLayer> compositionLayersToBeSubscribed = new List<CompositionLayer>();
		private List<CompositionLayer> compositionLayersToBeUnsubscribed = new List<CompositionLayer>();

		private bool isOnBeforeRenderSubscribed = false;
		private ViveCompositionLayer compositionLayerFeature = null;

		private const string LOG_TAG = "VIVE_CompositionLayerManager";
        static void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
        static void WARNING(string msg) { Debug.LogWarning(LOG_TAG + " " + msg); }
        static void ERROR(string msg) { Debug.LogError(LOG_TAG + " " + msg); }

        #region public parameter access functions
        public static CompositionLayerManager GetInstance()
		{
			if (instance == null)
			{
				GameObject CompositionLayerManagerGO = new GameObject("MultiLayerManager", typeof(CompositionLayerManager));
				instance = CompositionLayerManagerGO.GetComponent<CompositionLayerManager>();
			}

			return instance;
		}

		public static bool CompositionLayerManagerExists()
		{
			return (instance != null);
		}

		public int MaxLayerCount()
		{
			return (int)maxLayerCount;
		}

		public int RemainingLayerCount()
		{
			int count = (int)maxLayerCount - compositionLayers.Count;
			if (count < 0)
			{
				return 0;
			}

			return count;
		}

		public int CurrentLayerCount()
		{
			return compositionLayers.Count;
		}
		#endregion

		#region Monobehaviour Lifecycle
		void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Destroy(instance);
				instance = this;
			}
		}

		private void Start()
		{
			UpdateMaxLayerCount();
		}

		public delegate void CompositionLayerManagerOnBeforeRender();
		public event CompositionLayerManagerOnBeforeRender CompositionLayerManagerOnBeforeRenderDelegate = null;

		private void OnBeforeRender()
		{
			compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();

			if (compositionLayerFeature != null)
			{
				if (compositionLayerFeature.XrSessionEnding)
				{
					DEBUG("XrSession is ending, stop all layers");
					foreach (CompositionLayer layer in compositionLayers) //All active layers
					{
						if (!compositionLayersToBeUnsubscribed.Contains(layer) && !compositionLayersToBeSubscribed.Contains(layer))
						{
							//Add currently active layers that are not scheduled for termination to the "To be subscribed" list
							compositionLayersToBeSubscribed.Add(layer);
						}

						layer.TerminateLayer();
					}
					compositionLayers.Clear();

					foreach (CompositionLayer layer in compositionLayersToBeUnsubscribed) //All layers to be terminated
					{
						layer.TerminateLayer();
					}
					compositionLayersToBeUnsubscribed.Clear();

					return;
				}
			}
			else
			{
				ERROR("compositionLayerFeature not found");
			}

			bool CompositionLayerStatusUpdateNeeded = false;

			//Process Sub and Unsub list in bulk at once per frame
			if (compositionLayersToBeUnsubscribed.Count > 0)
			{
				foreach (CompositionLayer layerToBeRemoved in compositionLayersToBeUnsubscribed)
				{
					DEBUG("CompositionLayersToBeUnsubscribed: Processing");
					if (compositionLayers.Contains(layerToBeRemoved) && !compositionLayersToBeSubscribed.Contains(layerToBeRemoved))
					{
						layerToBeRemoved.TerminateLayer();
						compositionLayers.Remove(layerToBeRemoved);
					}
				}
				compositionLayersToBeUnsubscribed.Clear();
				CompositionLayerStatusUpdateNeeded = true;
			}

			if (compositionLayersToBeSubscribed.Count > 0)
			{
				DEBUG("CompositionLayersToBeSubscribed: Processing");
				foreach (CompositionLayer layerToBeAdded in compositionLayersToBeSubscribed)
				{
					if (!compositionLayers.Contains(layerToBeAdded))
					{
						compositionLayers.Add(layerToBeAdded);
						DEBUG("Add new layer");
					}
					else if (layerToBeAdded.isRenderPriorityChanged)
					{
						DEBUG("Layer RenderPriority changed");
					}
				}
				compositionLayersToBeSubscribed.Clear();
				CompositionLayerStatusUpdateNeeded = true;
			}

			if (CompositionLayerStatusUpdateNeeded)
			{
				DEBUG("CompositionLayerStatusUpdateNeeded");
				UpdateLayerStatus();
				CompositionLayerStatusUpdateNeeded = false;
			}

			CompositionLayerManagerOnBeforeRenderDelegate?.Invoke();
		}

		private void OnEnable()
		{
			if (!isOnBeforeRenderSubscribed)
			{
				Application.onBeforeRender += OnBeforeRender;
				isOnBeforeRenderSubscribed = true;
			}
		}

		private void OnDisable()
		{
			if (isOnBeforeRenderSubscribed)
			{
				Application.onBeforeRender -= OnBeforeRender;
				isOnBeforeRenderSubscribed = false;
			}
		}

		private void OnDestroy()
		{
			if (isOnBeforeRenderSubscribed)
			{
				Application.onBeforeRender -= OnBeforeRender;
				isOnBeforeRenderSubscribed = false;
			}
		}
		#endregion

		public void SubscribeToLayerManager(CompositionLayer layerToBeAdded)
		{
			if (compositionLayersToBeSubscribed == null)
			{
				DEBUG("SubscribeToLayerManager: Layer List not found. Creating a new one.");
				compositionLayersToBeSubscribed = new List<CompositionLayer>();
			}

			if (!compositionLayersToBeSubscribed.Contains(layerToBeAdded))
			{
				DEBUG("SubscribeToLayerManager: Add layer");
				compositionLayersToBeSubscribed.Add(layerToBeAdded);
			}
		}

		public void UnsubscribeFromLayerManager(CompositionLayer layerToBeRemoved, bool isImmediate)
		{
			if (compositionLayersToBeUnsubscribed == null)
			{
				DEBUG("UnsubscribeFromLayerManager: Layer List not found. Creating a new one.");
				compositionLayersToBeUnsubscribed = new List<CompositionLayer>();
			}

			if (!compositionLayersToBeUnsubscribed.Contains(layerToBeRemoved) && !isImmediate)
			{
				DEBUG("UnsubscribeFromLayerManager: Remove layer");
				compositionLayersToBeUnsubscribed.Add(layerToBeRemoved);
			}
			else if (isImmediate)
			{
				layerToBeRemoved.TerminateLayer();

				if (compositionLayersToBeUnsubscribed.Contains(layerToBeRemoved))
				{
					compositionLayersToBeUnsubscribed.Remove(layerToBeRemoved);
				}

				if (compositionLayersToBeSubscribed.Contains(layerToBeRemoved))
				{
					compositionLayersToBeSubscribed.Remove(layerToBeRemoved);
				}

				if (compositionLayers.Contains(layerToBeRemoved))
				{
					compositionLayers.Remove(layerToBeRemoved);
				}
			}
		}

		private void UpdateLayerStatus()
		{
			SortCompositionLayers();
			RenderCompositionLayers();
		}

		private void SortCompositionLayers()
		{
			if (compositionLayers == null)
			{
				return;
			}

			CompositionLayerRenderPriorityComparer renderPriorityComparer = new CompositionLayerRenderPriorityComparer();
			compositionLayers.Sort(renderPriorityComparer);
		}

		private void RenderCompositionLayers()
		{
			UpdateMaxLayerCount();

			for (int layerIndex=0; layerIndex < compositionLayers.Count; layerIndex++)
			{
				if (layerIndex < maxLayerCount) //Render as normal layers
				{
					if (compositionLayers[layerIndex].RenderAsLayer()) //Successfully initialized
					{
						DEBUG("RenderCompositionLayers: Layer " + compositionLayers[layerIndex].name + " Initialized successfully, Priority: " + compositionLayers[layerIndex].GetRenderPriority() + " layerIndex: " + layerIndex);
					}
					else
					{
						DEBUG("RenderCompositionLayers: Layer Initialization failed." + " layerIndex: " + layerIndex);
					}
				}
				else //Fallback if enabled
				{
					compositionLayers[layerIndex].RenderInGame();
					DEBUG("RenderCompositionLayers: Layer " + compositionLayers[layerIndex].name + " Rendering in game, Priority: " + compositionLayers[layerIndex].GetRenderPriority() + " layerIndex: " + layerIndex);
				}
			}
		}

		private void UpdateMaxLayerCount()
		{
			XrSystemProperties xrSystemProperties = new XrSystemProperties();
			XrResult result;

			compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();

			if (compositionLayerFeature != null)
			{
				if ((result = compositionLayerFeature.GetSystemProperties(ref xrSystemProperties)) == XrResult.XR_SUCCESS)
				{
					maxLayerCount = xrSystemProperties.graphicsProperties.maxLayerCount;
				}
				else
				{
					ERROR("Failed to get max layer count: " + result);
				}
			}
			else
			{
				ERROR("compositionLayerFeature not found");
				maxLayerCount = 0;
			}
		}

		class CompositionLayerRenderPriorityComparer : IComparer<CompositionLayer>
		{
			public int Compare(CompositionLayer layerX, CompositionLayer layerY)
			{
				//Rule1: Higher Render Priority -> Front of the list
				//Rule2: Same Render Priority -> Do not move layer
				if (layerX.GetRenderPriority() > layerY.GetRenderPriority())
				{
					return -1;
				}
				else if (layerX.GetRenderPriority() < layerY.GetRenderPriority())
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}
	}
}


