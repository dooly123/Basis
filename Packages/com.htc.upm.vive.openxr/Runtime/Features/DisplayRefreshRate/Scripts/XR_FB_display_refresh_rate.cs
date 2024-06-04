// ===================== 2022 HTC Corporation. All Rights Reserved. ===================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VIVE.OpenXR.DisplayRefreshRate;

namespace VIVE.OpenXR
{
    public class XR_FB_display_refresh_rate_defs
    {
		public virtual XrResult RequestDisplayRefreshRate(float displayRefreshRate)
		{
			return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
		}

		public virtual XrResult GetDisplayRefreshRate(out float displayRefreshRate)
		{
			displayRefreshRate = 90.0f;
			return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
		}

		public virtual XrResult EnumerateDisplayRefreshRates(UInt32 displayRefreshRateCapacityInput, out UInt32 displayRefreshRateCountOutput, out float displayRefreshRates)
		{
			displayRefreshRateCountOutput = 0;
			displayRefreshRates = 90.0f;
			return XrResult.XR_ERROR_FEATURE_UNSUPPORTED;
		}
    }

    public class XR_FB_display_refresh_rate
    {
        static XR_FB_display_refresh_rate_defs m_Instance = null;
        public static XR_FB_display_refresh_rate_defs Interop
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new XR_FB_display_refresh_rate_impls();
                }
                return m_Instance;
            }
        }

		/// <summary>
		/// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrRequestDisplayRefreshRateFB">xrRequestDisplayRefreshRateFB</see>.
		/// </summary>
		/// <param name="displayRefreshRate"></param>
		/// <returns></returns>
		public static XrResult RequestDisplayRefreshRate(float displayRefreshRate)
		{
			return Interop.RequestDisplayRefreshRate(displayRefreshRate);
		}

		/// <summary>
		/// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetDisplayRefreshRateFB">xrGetDisplayRefreshRateFB</see>.
		/// </summary>
		/// <param name="displayRefreshRate"></param>
		/// <returns></returns>
		public static XrResult GetDisplayRefreshRate(out float displayRefreshRate)
		{
			return Interop.GetDisplayRefreshRate(out displayRefreshRate);
		}

		/// <summary>
		/// Refer to OpenXR <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrEnumerateDisplayRefreshRatesFB">xrEnumerateDisplayRefreshRatesFB</see>.
		/// </summary>
		/// <param name="displayRefreshRateCapacityInput"></param>
		/// <param name="displayRefreshRateCountOutput"></param>
		/// <param name="displayRefreshRates"></param>
		/// <returns></returns>
		public static XrResult EnumerateDisplayRefreshRates(UInt32 displayRefreshRateCapacityInput, out UInt32 displayRefreshRateCountOutput, out float displayRefreshRates)
		{
			return Interop.EnumerateDisplayRefreshRates(displayRefreshRateCapacityInput, out displayRefreshRateCountOutput, out displayRefreshRates);
		}
    }
}
