// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using System.Threading;
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
    [OpenXRFeature(UiName = "VIVE XR Support",
		Desc = "Necessary to deploy an VIVE XR compatible app.",
		Company = "HTC",
		DocumentationLink = "https://developer.vive.com/resources/openxr/openxr-mobile/tutorials/how-install-vive-wave-openxr-plugin/",
		OpenxrExtensionStrings = kOpenxrExtensionStrings,
		Version = "1.0.0",
		BuildTargetGroups = new[] { BuildTargetGroup.Android },
		CustomRuntimeLoaderBuildTargets = new[] { BuildTarget.Android },
        FeatureId = featureId
	)]
#endif
    public class VIVEFocus3Feature : OpenXRFeature
    {
		const string LOG_TAG = "VIVE.OpenXR.VIVEFocus3Feature";
		static void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
		static void WARNING(string msg) { Debug.LogWarning(LOG_TAG + " " + msg); }
		static void ERROR(string msg) { Debug.LogError(LOG_TAG + " " + msg); }

		/// <summary>
		/// The feature id string. This is used to give the feature a well known id for reference.
		/// </summary>
		public const string featureId = "com.unity.openxr.feature.vivefocus3";

		/// <summary>
		/// OpenXR specification <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_KHR_android_surface_swapchain">12.2. XR_KHR_android_surface_swapchain</see>.
		/// </summary>
		public const string kOpenxrExtensionStrings = "";

		/// <summary>
		/// Enable Hand Tracking or Not.
		/// </summary>
		//public bool enableHandTracking = false;

		/// <summary>
		/// Enable Tracker or Not.
		/// </summary>
		//public bool enableTracker = false;

		/// <inheritdoc />
		//protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
		//{
		//	Debug.Log("EXT: registering our own xrGetInstanceProcAddr");
		//	return intercept_xrGetInstanceProcAddr(func);
		//}

		//private const string ExtLib = "viveopenxr";
		//[DllImport(ExtLib, EntryPoint = "intercept_xrGetInstanceProcAddr")]
		//private static extern IntPtr intercept_xrGetInstanceProcAddr(IntPtr func);

		private XrInstance m_XrInstance = 0;

		protected override bool OnInstanceCreate(ulong xrInstance)
		{
			/*
			foreach (string kOpenxrExtensionString in kOpenxrExtensionStrings.Split(' '))
			{
				if (!OpenXRRuntime.IsExtensionEnabled(kOpenxrExtensionString))
				{
					WARNING("OnInstanceCreate() " + kOpenxrExtensionString + " is NOT enabled.");
				}
				else
				{
					DEBUG("OnInstanceCreate() " + kOpenxrExtensionString + " is enabled.");
				}
			}
			*/

			m_XrInstance = xrInstance;
			Debug.Log("OnInstanceCreate() " + m_XrInstance);

			return GetXrFunctionDelegates(xrInstance);
		}

		private static XrSession m_XrSession = 0;
		protected override void OnSessionCreate(ulong xrSession)
		{
			m_XrSession = xrSession;
			DEBUG("OnSessionCreate() " + m_XrSession);
		}

		protected override void OnSessionDestroy(ulong xrSession)
		{
			DEBUG("OnSessionDestroy() " + xrSession);
			m_XrSession = 0;
		}

		/// xrGetInstanceProcAddr
		OpenXRHelper.xrGetInstanceProcAddrDelegate XrGetInstanceProcAddr;

		private bool GetXrFunctionDelegates(XrInstance xrInstance)
		{
			/// xrGetInstanceProcAddr
			if (xrGetInstanceProcAddr != null && xrGetInstanceProcAddr != IntPtr.Zero)
			{
				DEBUG("Get function pointer of xrGetInstanceProcAddr.");
				XrGetInstanceProcAddr = Marshal.GetDelegateForFunctionPointer(
					xrGetInstanceProcAddr,
					typeof(OpenXRHelper.xrGetInstanceProcAddrDelegate)) as OpenXRHelper.xrGetInstanceProcAddrDelegate;
			}
			else
			{
				ERROR("xrGetInstanceProcAddr failed.");
				return false;
			}

			IntPtr funcPtr = IntPtr.Zero;

			return true;
		}

#if UNITY_EDITOR
		protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.Add(new ValidationRule(this)
            {
                message = "Only the Focus 3 Interaction Profile is supported right now.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (null == settings)
                        return false;

                    bool touchFeatureEnabled = false;
                    foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
                    {
                        if (feature.enabled)
                        {
                            if (feature is VIVEFocus3Profile)
                                touchFeatureEnabled = true;
                        }
                    }
                    return touchFeatureEnabled;
                },
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (null == settings)
                        return;

                    foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
                    {
                        if (feature is VIVEFocus3Profile)
                            feature.enabled = true;
                    }
                },
                error = true,
            });
        }
#endif
    }
}
