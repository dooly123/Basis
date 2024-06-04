// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using VIVE.OpenXR.FacialTracking;

namespace VIVE.OpenXR.Samples
{
    public class FacialTrackingData : MonoBehaviour
    {
        private static FacialTrackingData m_Instance = null;
        private static void CheckInstance()
        {
            if (m_Instance == null)
            {
                var instanceObj = new GameObject("FacialTrackingData");
                m_Instance = instanceObj.AddComponent<FacialTrackingData>();
                DontDestroyOnLoad(m_Instance);
            }
        }

        private void Awake()
        {
            m_Instance = this;
        }

        private static float[] s_EyeExps = new float[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC];
        private static float[] s_LipExps = new float[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC];
        void Update()
        {
            // Eye expressions
            {
                if (XR_HTC_facial_tracking.Interop.GetExpressionWeightings(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out float[] exps))
                {
                    s_EyeExps = exps;
                }
            }
            // Lip expressions
            {
                if (XR_HTC_facial_tracking.Interop.GetExpressionWeightings(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out float[] exps))
                {
                    s_LipExps = exps;
                }
            }
        }

        public static float EyeExpression(XrEyeExpressionHTC exp)
        {
            CheckInstance();
            if ((int)exp < s_EyeExps.Length) { return s_EyeExps[(int)exp]; }
            return 0;
        }

        public static float LipExpression(XrLipExpressionHTC exp)
        {
            CheckInstance();
            if ((int)exp < s_LipExps.Length) { return s_LipExps[(int)exp]; }
            return 0;
        }
    }
}
