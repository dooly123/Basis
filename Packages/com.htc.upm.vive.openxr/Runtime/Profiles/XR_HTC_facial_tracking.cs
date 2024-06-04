// ===================== 2022 HTC Corporation. All Rights Reserved. ===================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VIVE.OpenXR.FacialTracking;

namespace VIVE.OpenXR
{
    public class XR_HTC_facial_tracking_defs
    {
        public virtual XrResult xrCreateFacialTrackerHTC(XrFacialTrackerCreateInfoHTC createInfo, out ulong facialTracker)
        {
            facialTracker = 0;
            return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
        }
        public virtual XrResult xrDestroyFacialTrackerHTC(ulong facialTracker)
        {
            return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
        }
        const int kMaxExpressionCount = 64;
        protected XrFacialExpressionsHTC m_FacialExpressions = new XrFacialExpressionsHTC(XrStructureType.XR_TYPE_FACIAL_EXPRESSIONS_HTC, IntPtr.Zero, 0, 0, 0, IntPtr.Zero);
        protected void InitializeFacialExpressions()
        {
            if (m_FacialExpressions.expressionCount != 0) { return; }

            m_FacialExpressions.type = XrStructureType.XR_TYPE_FACIAL_EXPRESSIONS_HTC;
            m_FacialExpressions.next = IntPtr.Zero;
            m_FacialExpressions.isActive = 0;
            m_FacialExpressions.sampleTime = 0;
            m_FacialExpressions.expressionCount = kMaxExpressionCount;
            m_FacialExpressions.expressionWeightings = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * kMaxExpressionCount);
        }
        public virtual XrResult xrGetFacialExpressionsHTC(ulong facialTracker, out XrFacialExpressionsHTC facialExpressions)
        {
            facialExpressions = m_FacialExpressions;
            return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
        }

        protected Dictionary<XrFacialTrackingTypeHTC, float[]> s_ExpressionWeightings = new Dictionary<XrFacialTrackingTypeHTC, float[]>()
        {
            { XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, new float[kMaxExpressionCount] },
            { XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, new float[kMaxExpressionCount] }
        };
        /// <summary>
        /// Retrieves an array of values of blend shapes for a facial expression on a given time.
        /// </summary>
        /// <param name="facialTrackingType">The <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrFacialTrackingTypeHTC">XrFacialTrackingTypeHTC</see> describes which type of tracking the <see cref="XrFacialTrackerHTC">XrFacialTrackerHTC</see> is using.</param>
        /// <param name="expressionWeightings">A float array filled in by the runtime, specifying the weightings for each blend shape. The array size is <see cref="XrEyeExpressionHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC">XR_EYE_EXPRESSION_MAX_ENUM_HTC</see> for eye expression and <see cref="XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC">XR_LIP_EXPRESSION_MAX_ENUM_HTC</see> for lip expression.</param>
        /// <returns>True for success.</returns>
        public virtual bool GetExpressionWeightings(XrFacialTrackingTypeHTC facialTrackingType, out float[] expressionWeightings)
        {
            expressionWeightings = s_ExpressionWeightings[facialTrackingType];
            int expressionCount = s_ExpressionWeightings[facialTrackingType].Length;

            if (m_FacialExpressions.isActive == 1 && expressionCount <= kMaxExpressionCount)
            {
                Marshal.Copy(m_FacialExpressions.expressionWeightings, s_ExpressionWeightings[facialTrackingType], 0, s_ExpressionWeightings[facialTrackingType].Length);
                expressionWeightings = s_ExpressionWeightings[facialTrackingType];
                return true;
            }

            return false;
        }
    }

    public class XR_HTC_facial_tracking
    {
        static XR_HTC_facial_tracking_defs m_Instance = null;
        public static XR_HTC_facial_tracking_defs Interop
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new XR_HTC_facial_tracking_impls();
                }
                return m_Instance;
            }
        }

        /// <summary>
        /// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateFacialTrackerHTC">xrCreateFacialTrackerHTC</see>.
        /// </summary>
        /// <param name="createInfo"></param>
        /// <param name="facialTracker"></param>
        /// <returns></returns>
        public static XrResult xrCreateFacialTrackerHTC(XrFacialTrackerCreateInfoHTC createInfo, out ulong facialTracker)
        {
            return Interop.xrCreateFacialTrackerHTC(createInfo, out facialTracker);
        }
        /// <summary>
        /// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroyFacialTrackerHTC">xrDestroyFacialTrackerHTC</see>.
        /// </summary>
        /// <param name="facialTracker"></param>
        /// <returns></returns>
        public static XrResult xrDestroyFacialTrackerHTC(ulong facialTracker)
        {
            return Interop.xrDestroyFacialTrackerHTC(facialTracker);
        }
        /// <summary>
        /// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetFacialExpressionsHTC">xrGetFacialExpressionsHTC</see>
        /// </summary>
        /// <param name="facialTracker"></param>
        /// <param name="facialExpressions"></param>
        /// <returns></returns>
        public static XrResult xrGetFacialExpressionsHTC(ulong facialTracker, out XrFacialExpressionsHTC facialExpressions)
        {
            return Interop.xrGetFacialExpressionsHTC(facialTracker, out facialExpressions);
        }
    }
}
