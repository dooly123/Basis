// "VIVE SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the VIVE SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace VIVE.OpenXR.DisplayRefreshRate
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "VIVE XR Display Refresh Rate",
        BuildTargetGroups = new[] { BuildTargetGroup.Android},
        Company = "HTC",
        Desc = "Support the display refresh rate.",
        DocumentationLink = "..\\Documentation",
        OpenxrExtensionStrings = kOpenxrExtensionString,
        Version = "1.0.0",
        FeatureId = featureId)]
#endif
    public class ViveDisplayRefreshRate : OpenXRFeature
	{
        const string LOG_TAG = "VIVE.OpenXR.DisplayRefreshRate";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
		void WARNING(string msg) { Debug.LogWarning(LOG_TAG + " " + msg); }
		void ERROR(string msg) { Debug.LogError(LOG_TAG + " " + msg); }

        /// <summary>
        /// OpenXR specification <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_display_refresh_rate">12.54. XR_FB_display_refresh_rate</see>.
        /// </summary>
        public const string kOpenxrExtensionString = "XR_FB_display_refresh_rate";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "vive.openxr.feature.displayrefreshrate";

        #region OpenXR Life Cycle
        private XrInstance m_XrInstance = 0;
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateInstance">xrCreateInstance</see> is done.
        /// </summary>
        /// <param name="xrInstance">The created instance.</param>
        /// <returns>True for valid <see cref="XrInstance">XrInstance</see></returns>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(kOpenxrExtensionString))
            {
                WARNING("OnInstanceCreate() " + kOpenxrExtensionString + " is NOT enabled.");
                return false;
            }

            m_XrInstance = xrInstance;
            DEBUG("OnInstanceCreate() " + m_XrInstance);

            return GetXrFunctionDelegates(m_XrInstance);
        }
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroyInstance">xrDestroyInstance</see> is done.
        /// </summary>
        /// <param name="xrInstance">The instance to destroy.</param>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            m_XrInstance = 0;
            DEBUG("OnInstanceDestroy() " + xrInstance);
        }

        private XrSystemId m_XrSystemId = 0;
        /// <summary>
        /// Called when the <see cref="XrSystemId">XrSystemId</see> retrieved by <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetSystem">xrGetSystem</see> is changed.
        /// </summary>
        /// <param name="xrSystem">The system id.</param>
        protected override void OnSystemChange(ulong xrSystem)
        {
            m_XrSystemId = xrSystem;
            DEBUG("OnSystemChange() " + m_XrSystemId);
        }

		private XrSession m_XrSession = 0;

        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see> is done.
        /// </summary>
        /// <param name="xrSession">The created session ID.</param>
        protected override void OnSessionCreate(ulong xrSession)
        {
            m_XrSession = xrSession;
            DEBUG("OnSessionCreate() " + m_XrSession);

        }
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroySession">xrDestroySession</see> is done.
        /// </summary>
        /// <param name="xrSession">The session ID to destroy.</param>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            DEBUG("OnSessionDestroy() " + xrSession);

            m_XrSession = 0;
        }
        #endregion

        #region OpenXR function delegates
        /// xrGetInstanceProcAddr
        OpenXRHelper.xrGetInstanceProcAddrDelegate XrGetInstanceProcAddr;

		//xrRequestDisplayRefreshRateFB
		OpenXRHelper.xrRequestDisplayRefreshRateFBDelegate xrRequestDisplayRefreshRateFB;

		//xrGetDisplayRefreshRateFB
		OpenXRHelper.xrGetDisplayRefreshRateFBDelegate xrGetDisplayRefreshRateFB;

		//xrEnumerateDisplayRefreshRatesFB
		OpenXRHelper.xrEnumerateDisplayRefreshRatesFBDelegate xrEnumerateDisplayRefreshRatesFB;


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
                ERROR("xrGetInstanceProcAddr");
                return false;
            }

            IntPtr funcPtr = IntPtr.Zero;
			DEBUG("Try Get function pointer of xrRequestDisplayRefreshRateFB.");

			/// xrRequestDisplayRefreshRateFB
			if (XrGetInstanceProcAddr(xrInstance, "xrRequestDisplayRefreshRateFB", out funcPtr) == XrResult.XR_SUCCESS)
			{
				if (funcPtr != IntPtr.Zero)
				{
					DEBUG("Get function pointer of xrRequestDisplayRefreshRateFB.");
					xrRequestDisplayRefreshRateFB = Marshal.GetDelegateForFunctionPointer(
						funcPtr,
						typeof(OpenXRHelper.xrRequestDisplayRefreshRateFBDelegate)) as OpenXRHelper.xrRequestDisplayRefreshRateFBDelegate;
				}
				else
				{
					ERROR("0. Get function pointer of xrRequestDisplayRefreshRateFB failed.");
				}
			}
			else
			{
				ERROR("1. Get function pointer of xrRequestDisplayRefreshRateFB failed.");
			}

			/// xrGetDisplayRefreshRateFB
			if (XrGetInstanceProcAddr(xrInstance, "xrGetDisplayRefreshRateFB", out funcPtr) == XrResult.XR_SUCCESS)
			{
				if (funcPtr != IntPtr.Zero)
				{
					DEBUG("Get function pointer of xrGetDisplayRefreshRateFB.");
					xrGetDisplayRefreshRateFB = Marshal.GetDelegateForFunctionPointer(
						funcPtr,
						typeof(OpenXRHelper.xrGetDisplayRefreshRateFBDelegate)) as OpenXRHelper.xrGetDisplayRefreshRateFBDelegate;
				}
				else
				{
					ERROR("0. Get function pointer of xrGetDisplayRefreshRateFB failed.");
				}
			}
			else
			{
				ERROR("1. Get function pointer of xrGetDisplayRefreshRateFB failed.");
			}

			/// xrEnumerateDisplayRefreshRatesFB
			if (XrGetInstanceProcAddr(xrInstance, "xrEnumerateDisplayRefreshRatesFB", out funcPtr) == XrResult.XR_SUCCESS)
			{
				if (funcPtr != IntPtr.Zero)
				{
					DEBUG("Get function pointer of xrEnumerateDisplayRefreshRatesFB.");
					xrEnumerateDisplayRefreshRatesFB = Marshal.GetDelegateForFunctionPointer(
						funcPtr,
						typeof(OpenXRHelper.xrEnumerateDisplayRefreshRatesFBDelegate)) as OpenXRHelper.xrEnumerateDisplayRefreshRatesFBDelegate;
				}
				else
				{
					ERROR("0. Get function pointer of xrEnumerateDisplayRefreshRatesFB failed.");
				}
			}
			else
			{
				ERROR("1. Get function pointer of xrEnumerateDisplayRefreshRatesFB failed.");
			}

			return true;
        }

		/// <summary>
		/// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrRequestDisplayRefreshRateFB">xrRequestDisplayRefreshRateFB</see>.
		/// </summary>
		/// <param name="displayRefreshRate"></param>
		/// <returns></returns>
		public XrResult RequestDisplayRefreshRate(float displayRefreshRate)
		{
			if (!OpenXRRuntime.IsExtensionEnabled("XR_FB_display_refresh_rate"))
			{
				WARNING("RequestDisplayRefreshRate: XR_FB_display_refresh_rate is NOT enabled.");
				return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
			}
			return xrRequestDisplayRefreshRateFB(m_XrSession, displayRefreshRate);
		}

		/// <summary>
		/// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetDisplayRefreshRateFB">xrGetDisplayRefreshRateFB</see>.
		/// </summary>
		/// <param name="displayRefreshRate"></param>
		/// <returns></returns>
		public XrResult GetDisplayRefreshRate(out float displayRefreshRate)
		{
			if (!OpenXRRuntime.IsExtensionEnabled("XR_FB_display_refresh_rate"))
			{
				WARNING("GetDisplayRefreshRate: XR_FB_display_refresh_rate is NOT enabled.");
				displayRefreshRate = 90.0f;
				return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
			}
			return xrGetDisplayRefreshRateFB(m_XrSession, out displayRefreshRate);
		}

		/// <summary>
		/// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrEnumerateDisplayRefreshRatesFB">xrEnumerateDisplayRefreshRatesFB</see>.
		/// </summary>
		/// <param name="displayRefreshRateCapacityInput"></param>
		/// <param name="displayRefreshRateCountOutput"></param>
		/// <param name="displayRefreshRates"></param>
		/// <returns></returns>
		public XrResult EnumerateDisplayRefreshRates(UInt32 displayRefreshRateCapacityInput, out UInt32 displayRefreshRateCountOutput, out float displayRefreshRates)
		{
			if (!OpenXRRuntime.IsExtensionEnabled("XR_FB_display_refresh_rate"))
			{
				WARNING("EnumerateDisplayRefreshRates: XR_FB_display_refresh_rate is NOT enabled.");
				displayRefreshRateCountOutput = 0;
				displayRefreshRates = 90.0f;
				return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
			}
			return xrEnumerateDisplayRefreshRatesFB(m_XrSession, displayRefreshRateCapacityInput, out displayRefreshRateCountOutput, out displayRefreshRates);
		}
        #endregion
    }
}
