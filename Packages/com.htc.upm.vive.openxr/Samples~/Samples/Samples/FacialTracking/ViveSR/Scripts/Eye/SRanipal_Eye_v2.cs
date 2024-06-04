//========= Copyright 2019, HTC Corporation. All rights reserved. ===========

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    public static class SRanipal_Eye_v2
    {
        public const int ANIPAL_TYPE_EYE_V2 = 2;
        public const int WeightingCount = (int)XrEyeShapeHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC;
        private static XrFacialExpressionsHTC EyeExpression_;
        private static int LastUpdateFrame = -1;
        private static Error LastUpdateResult = Error.FAILED;
        private static Dictionary<XrEyeShapeHTC, float> Weightings;
        private static float[] blendshapes = new float[60];
        static SRanipal_Eye_v2()
        {
            Weightings = new Dictionary<XrEyeShapeHTC, float>();
            for (int i = 0; i < WeightingCount; ++i) Weightings.Add((XrEyeShapeHTC)i, 0.0f);
        }
        private static bool UpdateData()
        {
            if (Time.frameCount == LastUpdateFrame) return LastUpdateResult == Error.WORK;
            else LastUpdateFrame = Time.frameCount;

            var feature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
            if (feature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out blendshapes))
            {
                LastUpdateResult = Error.WORK;
            }
            else
            {
                LastUpdateResult = Error.FAILED;
            }
            return LastUpdateResult == Error.WORK;
        }
        public static bool GetEyeWeightings(out Dictionary<XrEyeShapeHTC, float> shapes, XrFacialExpressionsHTC expression)
        {
            for (int i = 0; i < WeightingCount; ++i)
            {
                if (i < blendshapes.Length)
                    Weightings[(XrEyeShapeHTC)(i)] = blendshapes[i];
            }
            shapes = Weightings;
            return true;
        }


        /// <summary>
        /// Gets weighting values from anipal's Eye module.
        /// </summary>
        /// <param name="shapes">Weighting values obtained from anipal's Eye module.</param>
        /// <returns>Indicates whether the values received are new.</returns>\
        public static bool GetEyeWeightings(out Dictionary<XrEyeShapeHTC, float> shapes)
        {
            UpdateData();
            return GetEyeWeightings(out shapes, EyeExpression_);
        }

    }
}
