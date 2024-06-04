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
using UnityEngine.UI;
using VIVE.OpenXR.FacialTracking;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    [RequireComponent(typeof(Text))]
    public class EyeExpText : MonoBehaviour
    {
        public enum EyeExpressions
        {
            LeftBlink = XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC,
            LeftWide = XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC,
            RightBlink = XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC,
            RightWide = XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_WIDE_HTC,
            LeftSqueeze = XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_SQUEEZE_HTC,
            RightSqueeze = XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_SQUEEZE_HTC,
            LeftDown = XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_DOWN_HTC,
            RightDown = XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_DOWN_HTC,
            LeftOut = XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_OUT_HTC,
            RightIn = XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_IN_HTC,
            LeftIn = XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_IN_HTC,
            RightOut = XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_OUT_HTC,
            LeftUp = XrEyeExpressionHTC.XR_EYE_EXPRESSION_LEFT_UP_HTC,
            RightUp = XrEyeExpressionHTC.XR_EYE_EXPRESSION_RIGHT_UP_HTC,
        }

        [SerializeField]
        private EyeExpressions m_EyeExpression = EyeExpressions.LeftBlink;
        public EyeExpressions EyeExpression { get { return m_EyeExpression; } set { m_EyeExpression = value; } }

        Text m_Text = null;
        private void Start()
        {
            m_Text = GetComponent<Text>();
        }

        void Update()
        {
            if (m_Text == null) { return; }

            m_Text.text = m_EyeExpression.ToString() + ": " + FacialTrackingData.EyeExpression((XrEyeExpressionHTC)m_EyeExpression).ToString("N5");
        }
    }
}
