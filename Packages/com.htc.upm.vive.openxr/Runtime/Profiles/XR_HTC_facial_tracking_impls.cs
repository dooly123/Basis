// ===================== 2022 HTC Corporation. All Rights Reserved. ===================

using UnityEngine;

using UnityEngine.XR.OpenXR;

using VIVE.OpenXR.FacialTracking;


namespace VIVE.OpenXR
{
    public class XR_HTC_facial_tracking_impls : XR_HTC_facial_tracking_defs
    {
        const string LOG_TAG = "VIVE.OpenXR.Android.XR_HTC_facial_tracking_impls";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }

        private ViveFacialTracking feature = null;
        private void ASSERT_FEATURE() {
            if (feature == null) { feature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>(); }
        }

        public override XrResult xrCreateFacialTrackerHTC(XrFacialTrackerCreateInfoHTC createInfo, out ulong facialTracker)
        {
            DEBUG("xrCreateFacialTrackerHTC");
            XrResult result = XrResult.XR_ERROR_VALIDATION_FAILURE;
            facialTracker = 0;

            ASSERT_FEATURE();
            if (feature)
            {
                result = (XrResult)feature.CreateFacialTracker(createInfo, out XrFacialTrackerHTC value);
                if (result == XrResult.XR_SUCCESS) { facialTracker = value; }
            }

            return result;
        }
        public override XrResult xrDestroyFacialTrackerHTC(ulong facialTracker)
        {
            DEBUG("xrDestroyFacialTrackerHTC");

            ASSERT_FEATURE();
            if (feature) { return (XrResult)feature.DestroyFacialTracker(facialTracker); }

            return XrResult.XR_ERROR_VALIDATION_FAILURE;
        }
        public override XrResult xrGetFacialExpressionsHTC(ulong facialTracker, out XrFacialExpressionsHTC facialExpressions)
        {
            InitializeFacialExpressions();
            facialExpressions = m_FacialExpressions;
            XrResult result = XrResult.XR_ERROR_VALIDATION_FAILURE;

            ASSERT_FEATURE();
            if (feature)
            {
                result = (XrResult)feature.GetFacialExpressions(facialTracker, out XrFacialExpressionsHTC exps);
                if (result == XrResult.XR_SUCCESS) { facialExpressions = exps; }
            }

            return result;
        }

        public override bool GetExpressionWeightings(XrFacialTrackingTypeHTC facialTrackingType, out float[] expressionWeightings)
        {
            ASSERT_FEATURE();
            if (feature) { return feature.GetFacialExpressions((XrFacialTrackingTypeHTC)facialTrackingType, out expressionWeightings); }

            expressionWeightings = s_ExpressionWeightings[facialTrackingType];
            return false;
        }
    }
}
