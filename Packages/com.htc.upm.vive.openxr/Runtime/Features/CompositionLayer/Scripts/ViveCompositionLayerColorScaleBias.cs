// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using AOT;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

namespace VIVE.OpenXR.CompositionLayer
{
#if UNITY_EDITOR
	[OpenXRFeature(UiName = "VIVE XR Composition Layer (Color Scale Bias)",
		Desc = "Enable this feature to enable the Composition Layer Color Scale Bias Extension",
		Company = "HTC",
		DocumentationLink = "..\\Documentation",
		OpenxrExtensionStrings = kOpenXRColorScaleBiasExtensionString,
		Version = "1.0.0",
		BuildTargetGroups = new[] { BuildTargetGroup.Android },
		FeatureId = featureId
	)]
#endif
	public class ViveCompositionLayerColorScaleBias : OpenXRFeature
	{
		const string LOG_TAG = "VIVE.OpenXR.ViveCompositionLayer.ColorScaleBias";
		static void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
		static void WARNING(string msg) { Debug.LogWarning(LOG_TAG + " " + msg); }
		static void ERROR(string msg) { Debug.LogError(LOG_TAG + " " + msg); }

		/// <summary>
		/// The feature id string. This is used to give the feature a well known id for reference.
		/// </summary>
		public const string featureId = "vive.openxr.feature.compositionlayer.colorscalebias";

		private const string kOpenXRColorScaleBiasExtensionString = "XR_KHR_composition_layer_color_scale_bias";

		private bool m_ColorScaleBiasExtensionEnabled = true;
		public bool ColorScaleBiasExtensionEnabled
		{
			get { return m_ColorScaleBiasExtensionEnabled; }
		}


		#region OpenXR Life Cycle
		protected override bool OnInstanceCreate(ulong xrInstance)
		{
			if (!OpenXRRuntime.IsExtensionEnabled(kOpenXRColorScaleBiasExtensionString))
			{
				WARNING("OnInstanceCreate() " + kOpenXRColorScaleBiasExtensionString + " is NOT enabled.");

				m_ColorScaleBiasExtensionEnabled = false;
				return false;
			}

			return true;
		}
		#endregion

		#region Wrapper Functions
		private const string ExtLib = "viveopenxr";

		[DllImportAttribute(ExtLib, EntryPoint = "submit_CompositionLayerColorBias")]
		public static extern void VIVEOpenXR_Submit_CompositionLayerColorBias(XrCompositionLayerColorScaleBiasKHR colorBias, int layerID);
		public void Submit_CompositionLayerColorBias(XrCompositionLayerColorScaleBiasKHR colorBias, int layerID)
		{
			if (!ColorScaleBiasExtensionEnabled)
			{
				ERROR("Submit_CompositionLayerColorBias: " + kOpenXRColorScaleBiasExtensionString + " is not enabled.");
				return;
			}

			VIVEOpenXR_Submit_CompositionLayerColorBias(colorBias, layerID);
		}
		#endregion
	}
}