// ===================== 2022 HTC Corporation. All Rights Reserved. ===================

using System;
using UnityEngine;

using UnityEngine.XR.OpenXR;

using VIVE.OpenXR.DisplayRefreshRate;


namespace VIVE.OpenXR
{
    public class XR_FB_display_refresh_rate_impls : XR_FB_display_refresh_rate_defs
    {
        const string LOG_TAG = "VIVE.OpenXR.XR_FB_display_refresh_rate_impls";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }

        private ViveDisplayRefreshRate feature = null;
        private void ASSERT_FEATURE() {
            if (feature == null) { feature = OpenXRSettings.Instance.GetFeature<ViveDisplayRefreshRate>(); }
        }

		public override XrResult RequestDisplayRefreshRate(float displayRefreshRate)
		{
			DEBUG("RequestDisplayRefreshRate");
			XrResult result = XrResult.XR_ERROR_VALIDATION_FAILURE;

			ASSERT_FEATURE();
			if (feature)
			{
				result = (XrResult)feature.RequestDisplayRefreshRate(displayRefreshRate);
			}

			return result;
		}

		public override XrResult GetDisplayRefreshRate(out float displayRefreshRate)
		{
			//DEBUG("GetDisplayRefreshRate");
			XrResult result = XrResult.XR_ERROR_VALIDATION_FAILURE;
			displayRefreshRate = 0.0f;

			ASSERT_FEATURE();
			if (feature)
			{
				result = (XrResult)feature.GetDisplayRefreshRate(out displayRefreshRate);
			}

			return result;
		}

		public override XrResult EnumerateDisplayRefreshRates(UInt32 displayRefreshRateCapacityInput, out UInt32 displayRefreshRateCountOutput, out float displayRefreshRates)
		{
			DEBUG("EnumerateDisplayRefreshRates");
			XrResult result = XrResult.XR_ERROR_VALIDATION_FAILURE;
			displayRefreshRateCountOutput = 0;
			displayRefreshRates = 90.0f;

			ASSERT_FEATURE();
			if (feature)
			{
				result = (XrResult)feature.EnumerateDisplayRefreshRates(displayRefreshRateCapacityInput, out displayRefreshRateCountOutput, out displayRefreshRates);
			}

			return result;
		}
    }
}
