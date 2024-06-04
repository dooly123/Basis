// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using System.Runtime.InteropServices;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

namespace VIVE.OpenXR
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "VIVE XR Foveation",
		Desc = "Support the HTC foveation extension.",
		Company = "HTC",
		DocumentationLink = "..\\Documentation",
		OpenxrExtensionStrings = "XR_HTC_foveation",
		Version = "1.0.0",
		BuildTargetGroups = new[] { BuildTargetGroup.Android },
        FeatureId = featureId
	)]
#endif
    public class ViveFoveation : OpenXRFeature
    {
		/// <summary>
		/// Flag bits for XrFoveationDynamicFlagsHTC
		/// </summary>
		public const UInt64 XR_FOVEATION_DYNAMIC_LEVEL_ENABLED_BIT_HTC = 0x00000001;
		public const UInt64 XR_FOVEATION_DYNAMIC_CLEAR_FOV_ENABLED_BIT_HTC = 0x00000002;
		public const UInt64 XR_FOVEATION_DYNAMIC_FOCAL_CENTER_OFFSET_ENABLED_BIT_HTC = 0x00000004;

		/// <summary>
		/// The feature id string. This is used to give the feature a well known id for reference.
		/// </summary>
		public const string featureId = "vive.openxr.feature.foveation";

		protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
		{
			Debug.Log("EXT: registering our own xrGetInstanceProcAddr");
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
            {
                Debug.Log("Vulkan no hook foveation");
                return func;
            }
            return intercept_xrGetInstanceProcAddr(func);
		}

		private const string ExtLib = "viveopenxr";
		[DllImport(ExtLib, EntryPoint = "intercept_xrGetInstanceProcAddr")]
		private static extern IntPtr intercept_xrGetInstanceProcAddr(IntPtr func);

		[DllImport(ExtLib, EntryPoint = "applyFoveationHTC")]
		private static extern XrResult applyFoveationHTC(XrFoveationModeHTC mode, UInt32 configCount, XrFoveationConfigurationHTC[] configs, UInt64 flags);

		/// <summary>
		/// function to apply HTC Foveation
		/// </summary>
		public static XrResult ApplyFoveationHTC(XrFoveationModeHTC mode, UInt32 configCount, XrFoveationConfigurationHTC[] configs, UInt64 flags = 0)
		{
			//Debug.Log("Unity HTCFoveat:configCount " + configCount);
			//if (configCount >=2) {
				//Debug.Log("Unity HTCFoveat:configs[0].clearFovDegree " + configs[0].clearFovDegree);
				//Debug.Log("Unity HTCFoveat:configs[0].level " + configs[0].level);
				//Debug.Log("Unity HTCFoveat:configs[1].clearFovDegree " + configs[1].clearFovDegree);
				//Debug.Log("Unity HTCFoveat:configs[1].level " + configs[1].level);
			//}
			return applyFoveationHTC(mode, configCount, configs, flags);
		}
    }
}