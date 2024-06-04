// "VIVE SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the VIVE SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    public class FacialTrackingData : MonoBehaviour
    {
        private static FacialTrackingData instance = null;
        private static void CheckInstance()
        {
            if (instance == null)
            {
                var instanceObj = new GameObject("FacialTrackingData");
                instance = instanceObj.AddComponent<FacialTrackingData>();
                DontDestroyOnLoad(instance);
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private static float[] eyeExps = new float[(int)XrEyeExpressionHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC];
        private static float[] lipExps = new float[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC];
        void Update()
        {
            var feature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
            if (feature != null)
            {
                // Eye expressions
                {
                    if (feature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out float[] exps))
                    {
                        eyeExps = exps;
                    }
                }
                // Lip expressions
                {
                    if (feature.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out float[] exps))
                    {
                        lipExps = exps;
                    }
                }
            }
        }

        public static float EyeExpression(XrEyeExpressionHTC exp)
        {
            CheckInstance();
            if ((int)exp < eyeExps.Length) { return eyeExps[(int)exp]; }
            return 0;
        }
        public static float LipExpression(XrLipExpressionHTC exp)
        {
            CheckInstance();
            if ((int)exp < lipExps.Length) { return lipExps[(int)exp]; }
            return 0;
        }
    }
}
