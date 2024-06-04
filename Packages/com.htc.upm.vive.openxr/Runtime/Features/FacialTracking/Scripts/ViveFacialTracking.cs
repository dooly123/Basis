// Copyright HTC Corporation All Rights Reserved.

using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace VIVE.OpenXR.FacialTracking
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "VIVE XR Facial Tracking",
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = "HTC",
        Desc = "Support the facial tracking extension.",
        DocumentationLink = "..\\Documentation",
        OpenxrExtensionStrings = kOpenxrExtensionString,
        Version = "1.0.0",
        FeatureId = featureId)]
#endif
    public class ViveFacialTracking : OpenXRFeature
	{
        const string LOG_TAG = "VIVE.OpenXR.FacialTracking.ViveFacialTracking";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
        void WARNING(string msg) { Debug.LogWarning(LOG_TAG + " " + msg); }
        void ERROR(string msg) { Debug.LogError(LOG_TAG + " " + msg); }

        /// <summary>
        /// OpenXR specification <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_HTC_facial_tracking">12.68. XR_HTC_facial_tracking</see>.
        /// </summary>
        public const string kOpenxrExtensionString = "XR_HTC_facial_tracking";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "vive.openxr.feature.facial.tracking";

        #region OpenXR Life Cycle
        private bool m_XrInstanceCreated = false;
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

            m_XrInstanceCreated = true;
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
            m_XrInstanceCreated = false;
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

        private bool m_XrSessionCreated = false;
        private XrSession m_XrSession = 0;
        private bool hasEyeTracker = false, hasLipTracker = false;
        private XrFacialTrackerHTC m_EyeTracker = 0, m_LipTracker = 0;
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see> is done.
        /// </summary>
        /// <param name="xrSession">The created session ID.</param>
        protected override void OnSessionCreate(ulong xrSession)
        {
            m_XrSession = xrSession;
            m_XrSessionCreated = true;
            DEBUG("OnSessionCreate() " + m_XrSession);

            if (CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC)) { DEBUG("OnSessionCreate() m_EyeTracker " + m_EyeTracker); }
            if (CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC)) { DEBUG("OnSessionCreate() m_LipTracker " + m_LipTracker); }
        }
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroySession">xrDestroySession</see> is done.
        /// </summary>
        /// <param name="xrSession">The session ID to destroy.</param>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            DEBUG("OnSessionDestroy() " + xrSession);

            // Facial Tracking is binding with xrSession so we destroy the trackers when xrSession is destroyed.
            DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC);
            DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC);

            m_XrSession = 0;
            m_XrSessionCreated = false;
        }
        #endregion

        #region OpenXR function delegates
        /// xrGetInstanceProcAddr
        OpenXRHelper.xrGetInstanceProcAddrDelegate XrGetInstanceProcAddr;

        /// xrGetSystemProperties
        OpenXRHelper.xrGetSystemPropertiesDelegate xrGetSystemProperties;
        private XrResult GetSystemProperties(ref XrSystemProperties properties)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("GetSystemProperties() XR_ERROR_SESSION_LOST.");
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("GetSystemProperties() XR_ERROR_INSTANCE_LOST.");
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrGetSystemProperties(m_XrInstance, m_XrSystemId, ref properties);
        }

        /// xrDestroySpace
        OpenXRHelper.xrDestroySpaceDelegate xrDestroySpace;
        private XrResult DestroySpace(XrSpace space)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("DestroySpace() XR_ERROR_SESSION_LOST.");
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("DestroySpace() XR_ERROR_INSTANCE_LOST.");
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrDestroySpace(space);
        }

        ViveFacialTrackingHelper.xrCreateFacialTrackerHTCDelegate xrCreateFacialTrackerHTC;
        private XrResult CreateFacialTrackerHTC(XrFacialTrackerCreateInfoHTC createInfo, out XrFacialTrackerHTC facialTracker)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("CreateFacialTrackerHTC() XR_ERROR_SESSION_LOST.");
                facialTracker = 0;
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
			{
                ERROR("CreateFacialTrackerHTC() XR_ERROR_INSTANCE_LOST.");
                facialTracker = 0;
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrCreateFacialTrackerHTC(m_XrSession, createInfo, out facialTracker);
        }

        ViveFacialTrackingHelper.xrDestroyFacialTrackerHTCDelegate xrDestroyFacialTrackerHTC;
        private XrResult DestroyFacialTrackerHTC(XrFacialTrackerHTC facialTracker)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("DestroyFacialTrackerHTC() XR_ERROR_SESSION_LOST.");
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("DestroyFacialTrackerHTC() XR_ERROR_INSTANCE_LOST.");
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrDestroyFacialTrackerHTC(facialTracker);
        }

        ViveFacialTrackingHelper.xrGetFacialExpressionsHTCDelegate xrGetFacialExpressionsHTC;
        private XrResult GetFacialExpressionsHTC(XrFacialTrackerHTC facialTracker, ref XrFacialExpressionsHTC facialExpressions)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("GetFacialExpressionsHTC() XR_ERROR_SESSION_LOST.");
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("GetFacialExpressionsHTC() XR_ERROR_INSTANCE_LOST.");
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrGetFacialExpressionsHTC(facialTracker, ref facialExpressions);
        }

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
            /// xrGetSystemProperties
            if (XrGetInstanceProcAddr(xrInstance, "xrGetSystemProperties", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrGetSystemProperties.");
                    xrGetSystemProperties = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(OpenXRHelper.xrGetSystemPropertiesDelegate)) as OpenXRHelper.xrGetSystemPropertiesDelegate;
                }
            }
            else
            {
                ERROR("xrGetSystemProperties");
                return false;
            }
            /// xrDestroySpace
            if (XrGetInstanceProcAddr(xrInstance, "xrDestroySpace", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrDestroySpace.");
                    xrDestroySpace = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(OpenXRHelper.xrDestroySpaceDelegate)) as OpenXRHelper.xrDestroySpaceDelegate;
                }
            }
            else
            {
                ERROR("xrDestroySpace");
                return false;
            }

            /// xrCreateFacialTrackerHTC
            if (XrGetInstanceProcAddr(xrInstance, "xrCreateFacialTrackerHTC", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrCreateFacialTrackerHTC.");
                    xrCreateFacialTrackerHTC = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(ViveFacialTrackingHelper.xrCreateFacialTrackerHTCDelegate)) as ViveFacialTrackingHelper.xrCreateFacialTrackerHTCDelegate;
                }
            }
            else
            {
                ERROR("xrCreateFacialTrackerHTC");
                return false;
            }
            /// xrDestroyFacialTrackerHTC
            if (XrGetInstanceProcAddr(xrInstance, "xrDestroyFacialTrackerHTC", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrDestroyFacialTrackerHTC.");
                    xrDestroyFacialTrackerHTC = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(ViveFacialTrackingHelper.xrDestroyFacialTrackerHTCDelegate)) as ViveFacialTrackingHelper.xrDestroyFacialTrackerHTCDelegate;
                }
            }
            else
            {
                ERROR("xrDestroyFacialTrackerHTC");
                return false;
            }
            /// xrGetFacialExpressionsHTC
            if (XrGetInstanceProcAddr(xrInstance, "xrGetFacialExpressionsHTC", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrGetFacialExpressionsHTC.");
                    xrGetFacialExpressionsHTC = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(ViveFacialTrackingHelper.xrGetFacialExpressionsHTCDelegate)) as ViveFacialTrackingHelper.xrGetFacialExpressionsHTCDelegate;
                }
            }
            else
            {
                ERROR("xrGetFacialExpressionsHTC");
                return false;
            }

            return true;
        }
        #endregion

        XrSystemFacialTrackingPropertiesHTC facialTrackingSystemProperties;
        XrSystemProperties systemProperties;
        private bool IsFacialTrackingSupported(XrFacialTrackingTypeHTC facialTrackingType)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("IsFacialTrackingSupported() session is not created.");
                return false;
            }

            facialTrackingSystemProperties.type = XrStructureType.XR_TYPE_SYSTEM_FACIAL_TRACKING_PROPERTIES_HTC;
            systemProperties.type = XrStructureType.XR_TYPE_SYSTEM_PROPERTIES;
            systemProperties.next = Marshal.AllocHGlobal(Marshal.SizeOf(facialTrackingSystemProperties));

            long offset = 0;
            if (IntPtr.Size == 4)
                offset = systemProperties.next.ToInt32();
            else
                offset = systemProperties.next.ToInt64();

            IntPtr sys_facial_tracking_prop_ptr = new IntPtr(offset);
            Marshal.StructureToPtr(facialTrackingSystemProperties, sys_facial_tracking_prop_ptr, false);

            if (GetSystemProperties(ref systemProperties) == XrResult.XR_SUCCESS)
            {
                if (IntPtr.Size == 4)
                    offset = systemProperties.next.ToInt32();
                else
                    offset = systemProperties.next.ToInt64();

                sys_facial_tracking_prop_ptr = new IntPtr(offset);
                facialTrackingSystemProperties = (XrSystemFacialTrackingPropertiesHTC)Marshal.PtrToStructure(sys_facial_tracking_prop_ptr, typeof(XrSystemFacialTrackingPropertiesHTC));

                DEBUG("IsFacialTrackingSupported() XrSystemFacialTrackingPropertiesHTC.supportEyeFacialTracking: "
                    + facialTrackingSystemProperties.supportEyeFacialTracking
                    + ", supportLipFacialTracking: "
                    + facialTrackingSystemProperties.supportLipFacialTracking);

                return (facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC ?
                    (facialTrackingSystemProperties.supportEyeFacialTracking > 0) : (facialTrackingSystemProperties.supportLipFacialTracking > 0));
            }
            else
            {
                ERROR("IsFacialTrackingSupported() GetSystemProperties failed.");
            }

            return false;
        }

        /// <summary>
        /// An application can create an <see cref="XrFacialTrackingTypeHTC">XrFacialTrackerHTC</see> handle using CreateFacialTracker.
        /// </summary>
        /// <param name="facialTrackingType">The XrFacialTrackingTypeHTC describes which type of tracking the <see cref="XrFacialTrackerHTC">XrFacialTrackerHTC</see> is using.</param>
        /// <param name="facialTracker">The returned XrFacialTrackerHTC handle.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public XrResult CreateFacialTracker(XrFacialTrackerCreateInfoHTC createInfo, out XrFacialTrackerHTC facialTracker)
        {
            if (createInfo.facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC && hasEyeTracker)
			{
                facialTracker = m_EyeTracker;
                DEBUG("CreateFacialTracker() m_EyeTracker: " + facialTracker + " already created before.");
                return XrResult.XR_SUCCESS;
			}

            if (createInfo.facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC && hasLipTracker)
			{
                facialTracker = m_LipTracker;
                DEBUG("CreateFacialTracker() m_LipTracker: " + facialTracker + " already created before.");
                return XrResult.XR_SUCCESS;
            }

            if (!IsFacialTrackingSupported(createInfo.facialTrackingType))
            {
                ERROR("CreateFacialTracker() " + createInfo.facialTrackingType + " is NOT supported.");
                facialTracker = 0;
                return XrResult.XR_ERROR_VALIDATION_FAILURE;
            }

            var result = CreateFacialTrackerHTC(createInfo, out facialTracker);
            DEBUG("CreateFacialTracker() " + createInfo.facialTrackingType + ", CreateFacialTrackerHTC = " + result + ", facialTracker: " + facialTracker);

            if (result == XrResult.XR_SUCCESS)
            {
                if (createInfo.facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC)
                {
                    hasEyeTracker = true;
                    m_EyeTracker = facialTracker;
                    DEBUG("CreateFacialTracker() m_EyeTracker " + m_EyeTracker);
                }
                if (createInfo.facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC)
				{
                    hasLipTracker = true;
                    m_LipTracker = facialTracker;
                    DEBUG("CreateFacialTracker() m_LipTracker " + m_LipTracker);
                }
            }

            return result;
        }
        /// <summary>
        /// An application can create an <see cref="XrFacialTrackingTypeHTC">XrFacialTrackerHTC</see> handle using CreateFacialTracker.
        /// </summary>
        /// <param name="facialTrackingType">The XrFacialTrackingTypeHTC describes which type of tracking the <see cref="XrFacialTrackerHTC">XrFacialTrackerHTC</see> is using.</param>
        /// <returns>True for success.</returns>
        public bool CreateFacialTracker(XrFacialTrackingTypeHTC facialTrackingType)
		{
            XrFacialTrackerCreateInfoHTC createInfo = new XrFacialTrackerCreateInfoHTC(
                in_type: XrStructureType.XR_TYPE_FACIAL_TRACKER_CREATE_INFO_HTC,
                in_next: IntPtr.Zero,
                in_facialTrackingType: facialTrackingType);

            var result = CreateFacialTracker(createInfo, out XrFacialTrackerHTC value);
            DEBUG("CreateFacialTracker() " + createInfo.facialTrackingType + " tracker: " + value);

            return result == XrResult.XR_SUCCESS;
		}

        /// <summary>
        /// Releases the facial tracker and the underlying resources of the <see cref="XrFacialTrackingTypeHTC">facial tracking type</see> when finished with facial tracking experiences.
        /// </summary>
        /// <param name="facialTracker">Facial tracker in <see cref="XrFacialTrackerHTC">XrFacialTrackerHTC</see>.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public XrResult DestroyFacialTracker(XrFacialTrackerHTC facialTracker)
        {
            XrResult result = DestroyFacialTrackerHTC(facialTracker);
            DEBUG("DestroyFacialTracker() " + facialTracker + ", result: " + result);

            return result;
        }
        /// <summary>
        /// Releases the facial tracker and the underlying resources of the <see cref="XrFacialTrackingTypeHTC">facial tracking type</see> when finished with facial tracking experiences.
        /// </summary>
        /// <param name="facialTrackingType">The <see cref="XrFacialTrackingTypeHTC">XrFacialTrackingTypeHTC</see> describes which type of tracking the <see cref="XrFacialTrackerHTC">XrFacialTrackerHTC</see> is using.</param>
        /// <returns>True for success.</returns>
        public bool DestroyFacialTracker(XrFacialTrackingTypeHTC facialTrackingType)
        {
            if (facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC && !hasEyeTracker)
            {
                DEBUG("DestroyFacialTracker() no " + facialTrackingType + "tracker.");
                return true;
            }
            if (facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC && !hasLipTracker)
            {
                DEBUG("DestroyFacialTracker() no " + facialTrackingType + "tracker.");
                return true;
            }

            XrResult ret = XrResult.XR_ERROR_VALIDATION_FAILURE;

            if (facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC)
            {
                ret = DestroyFacialTracker(m_EyeTracker);
                hasEyeTracker = false;
                m_EyeTracker = 0;
            }
            if (facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC)
            {
                ret = DestroyFacialTracker(m_LipTracker);
                hasLipTracker = false;
                m_LipTracker = 0;
            }

            return ret == XrResult.XR_SUCCESS;
        }

        private int eyeUpdateFrame = -1, lipUpdateFrame = -1;
        private float[] defExpressionData = new float[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC];
        private float[] s_EyeExpressionData = new float[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC];
        private float[] s_LipExpressionData = new float[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC];
        XrFacialExpressionsHTC facialExpressionsDef = new XrFacialExpressionsHTC(XrStructureType.XR_TYPE_FACIAL_EXPRESSIONS_HTC, IntPtr.Zero, false, 0, 0, IntPtr.Zero);
        XrFacialExpressionsHTC m_FacialExpressionsEye = new XrFacialExpressionsHTC(XrStructureType.XR_TYPE_FACIAL_EXPRESSIONS_HTC, IntPtr.Zero, false, 0, 0, IntPtr.Zero);
        XrFacialExpressionsHTC m_FacialExpressionsLip = new XrFacialExpressionsHTC(XrStructureType.XR_TYPE_FACIAL_EXPRESSIONS_HTC, IntPtr.Zero, false, 0, 0, IntPtr.Zero);
        /// <summary>
        /// Retrieves an array of values of blend shapes for a facial expression on a given time.
        /// </summary>
        /// <param name="facialTrackingType">The <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrFacialTrackingTypeHTC">XrFacialTrackingTypeHTC</see> describes which type of tracking the <see cref="XrFacialTrackerHTC">XrFacialTrackerHTC</see> is using.</param>
        /// <param name="expressionWeightings">A float array filled in by the runtime, specifying the weightings for each blend shape. The array size is <see cref="XrEyeExpressionHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC">XR_EYE_EXPRESSION_MAX_ENUM_HTC</see> for eye expression and <see cref="XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC">XR_LIP_EXPRESSION_MAX_ENUM_HTC</see> for lip expression.</param>
        /// <returns>True for success.</returns>
        public bool GetFacialExpressions(XrFacialTrackingTypeHTC facialTrackingType, out float[] expressionWeightings)
		{
            expressionWeightings = defExpressionData;

            if (facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC)
            {
                if (GetFacialExpressions(m_EyeTracker, out XrFacialExpressionsHTC facialExpressions) == XrResult.XR_SUCCESS)
                {
                    if (facialExpressions.isActive)
                    {
                        Marshal.Copy(facialExpressions.expressionWeightings, s_EyeExpressionData, 0, (int)facialExpressions.expressionCount);
                        expressionWeightings = s_EyeExpressionData;
                        return true;
                    }
                }
            }
            if (facialTrackingType == XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC)
            {
                if (GetFacialExpressions(m_LipTracker, out XrFacialExpressionsHTC facialExpressions) == XrResult.XR_SUCCESS)
                {
                    if (facialExpressions.isActive)
                    {
                        Marshal.Copy(facialExpressions.expressionWeightings, s_LipExpressionData, 0, (int)facialExpressions.expressionCount);
                        expressionWeightings = s_LipExpressionData;
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Retrieves the <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrFacialExpressionsHTC">XrFacialExpressionsHTC</see> data of a <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrFacialTrackerHTC">XrFacialTrackerHTC</see>.
        /// </summary>
        /// <param name="facialTracker">The <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrFacialTrackerHTC">XrFacialTrackerHTC</see> handle represents the resources for an facial tracker of the specific facial tracking type.</param>
        /// <param name="facialExpressions">Structure returns data of a lip facial expression or an eye facial expression.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public XrResult GetFacialExpressions(XrFacialTrackerHTC facialTracker, out XrFacialExpressionsHTC facialExpressions)
        {
            facialExpressions = facialExpressionsDef;
            XrResult result = XrResult.XR_ERROR_VALIDATION_FAILURE;

            if (facialTracker == m_EyeTracker)
            {
                if (eyeUpdateFrame == Time.frameCount)
                {
                    facialExpressions = m_FacialExpressionsEye;
                    return XrResult.XR_SUCCESS;
                }
                eyeUpdateFrame = Time.frameCount;

                // Initialize the XrFacialExpressionsHTC struct of Eye.
                if (m_FacialExpressionsEye.expressionCount == 0)
				{
                    m_FacialExpressionsEye.type = XrStructureType.XR_TYPE_FACIAL_EXPRESSIONS_HTC;
                    m_FacialExpressionsEye.next = IntPtr.Zero;
                    m_FacialExpressionsEye.isActive = false;
                    m_FacialExpressionsEye.sampleTime = 0;
                    m_FacialExpressionsEye.expressionCount = (UInt32)XrEyeExpressionHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC;
                    m_FacialExpressionsEye.expressionWeightings = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * (int)m_FacialExpressionsEye.expressionCount);
                }

                result = GetFacialExpressionsHTC(facialTracker, ref m_FacialExpressionsEye);
                if (result == XrResult.XR_SUCCESS) { facialExpressions = m_FacialExpressionsEye; }
            }
            if (facialTracker == m_LipTracker)
            {
                if (lipUpdateFrame == Time.frameCount)
                {
                    facialExpressions = m_FacialExpressionsLip;
                    return XrResult.XR_SUCCESS;
                }
                lipUpdateFrame = Time.frameCount;

                // Initialize the XrFacialExpressionsHTC struct of Lip.
                if (m_FacialExpressionsLip.expressionCount == 0)
				{
                    m_FacialExpressionsLip.type = XrStructureType.XR_TYPE_FACIAL_EXPRESSIONS_HTC;
                    m_FacialExpressionsLip.next = IntPtr.Zero;
                    m_FacialExpressionsLip.isActive = false;
                    m_FacialExpressionsLip.sampleTime = 0;
                    m_FacialExpressionsLip.expressionCount = (UInt32)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC;
                    m_FacialExpressionsLip.expressionWeightings = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * (int)m_FacialExpressionsLip.expressionCount);
                }

                result = GetFacialExpressionsHTC(facialTracker, ref m_FacialExpressionsLip);
                if (result == XrResult.XR_SUCCESS) { facialExpressions = m_FacialExpressionsLip; }
            }

            return result;
        }
    }
}
