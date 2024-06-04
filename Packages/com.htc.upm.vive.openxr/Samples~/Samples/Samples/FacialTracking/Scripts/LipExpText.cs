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
    public class LipExpText : MonoBehaviour
    {
        public enum LipExpressions
        {
            JawRight = XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_RIGHT_HTC, // 0
            JawLeft = XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_LEFT_HTC,
            JawForward = XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_FORWARD_HTC,
            JawOpen = XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC,
            MouthApeShape = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_APE_SHAPE_HTC,
            MouthUpperRight = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_RIGHT_HTC, // 5
            MouthUpperLeft = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_LEFT_HTC,
            MouthLowerRight = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_RIGHT_HTC,
            MouthLowerLeft = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_LEFT_HTC,
            MouthUpperOverturn = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_OVERTURN_HTC,
            MouthLowerOverturn = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_OVERTURN_HTC, // 10
            MouthPout = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC,
            MouthSmileRight = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_SMILE_RIGHT_HTC,
            MouthSmileLeft = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_SMILE_LEFT_HTC,
            MouthSadRight = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_SAD_RIGHT_HTC,
            MouthSadLeft = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_SAD_LEFT_HTC, // 15
            CheekPuffRight = XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_PUFF_RIGHT_HTC,
            CheekPuffLeft = XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_PUFF_LEFT_HTC,
            CheekSuck = XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_SUCK_HTC,
            MouthUpperUpright = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC,
            MouthUpperUpleft = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPLEFT_HTC, // 20
            MouthLowerDownright = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNRIGHT_HTC,
            MouthLowerDownleft = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNLEFT_HTC,
            MouthUpperInside = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_INSIDE_HTC,
            MouthLowerInside = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_INSIDE_HTC,
            MouthLowerOverlay = XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_OVERLAY_HTC, // 25
            TongueLongstep1 = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_LONGSTEP1_HTC,
            TongueLeft = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_LEFT_HTC,
            TongueRight = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_RIGHT_HTC,
            TongueUp = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_UP_HTC,
            TongueDown = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_DOWN_HTC, // 30
            TongueRoll = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_ROLL_HTC,
            TongueLongstep2 = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_LONGSTEP2_HTC,
            TongueUprightMorph = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_UPRIGHT_MORPH_HTC,
            TongueUpleftMorph = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_UPLEFT_MORPH_HTC,
            TongueDownrightMorph = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_DOWNRIGHT_MORPH_HTC, // 35
            TongueDownleftMorph = XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_DOWNLEFT_MORPH_HTC,
        }

        [SerializeField]
        private LipExpressions m_LipExpression = LipExpressions.JawRight;
        public LipExpressions LipExpression { get { return m_LipExpression; } set { m_LipExpression = value; } }

        Text m_Text = null;
        private void Start()
        {
            m_Text = GetComponent<Text>();
        }

        void Update()
        {
            if (m_Text == null) { return; }

            m_Text.text = m_LipExpression.ToString() + ": " + FacialTrackingData.LipExpression((XrLipExpressionHTC)m_LipExpression).ToString("N5");
        }
    }
}
