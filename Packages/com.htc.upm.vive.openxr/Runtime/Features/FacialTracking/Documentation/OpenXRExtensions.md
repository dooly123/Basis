# 12.68. XR_HTC_facial_tracking
## Name String
    XR_HTC_facial_tracking
## Revision
    1
## Overview
    This extension allows an application to track and integrate users' eye and lip movements, empowering developers to read intention and model facial expressions.

## VIVE Plugin

Through feeding the blend shape values of eye expression to an avatar, its facial expression can be animated with the player¡¦s eye movement. The following enumerations show the facial expression of eye blend shape.

    public enum XrEyeExpressionHTC
    {
        XR_EYE_EXPRESSION_LEFT_BLINK_HTC = 0,
        XR_EYE_EXPRESSION_LEFT_WIDE_HTC = 1,
        XR_EYE_EXPRESSION_RIGHT_BLINK_HTC = 2,
        XR_EYE_EXPRESSION_RIGHT_WIDE_HTC = 3,
        XR_EYE_EXPRESSION_LEFT_SQUEEZE_HTC = 4,
        XR_EYE_EXPRESSION_RIGHT_SQUEEZE_HTC = 5,
        XR_EYE_EXPRESSION_LEFT_DOWN_HTC = 6,
        XR_EYE_EXPRESSION_RIGHT_DOWN_HTC = 7,
        XR_EYE_EXPRESSION_LEFT_OUT_HTC = 8,
        XR_EYE_EXPRESSION_RIGHT_IN_HTC = 9,
        XR_EYE_EXPRESSION_LEFT_IN_HTC = 10,
        XR_EYE_EXPRESSION_RIGHT_OUT_HTC = 11,
        XR_EYE_EXPRESSION_LEFT_UP_HTC = 12,
        XR_EYE_EXPRESSION_RIGHT_UP_HTC = 13,
        XR_EYE_EXPRESSION_MAX_ENUM_HTC = 14
    };

You can use the following API to retrieve the array of eye expression values if the return value is true.

    using VIVE.OpenXR.FacialTracking;
    
    bool ViveFacialTracking.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC, out float[] expressionWeightings);

Through feeding the blend shape values of lip expression to an avatar, its facial expression can be animated with the player¡¦s lip movement. The following enumerations show the facial expression of lip blend shape values.

    public enum XrLipExpressionHTC
    {
        XR_LIP_EXPRESSION_JAW_RIGHT_HTC = 0,
        XR_LIP_EXPRESSION_JAW_LEFT_HTC = 1,
        XR_LIP_EXPRESSION_JAW_FORWARD_HTC = 2,
        XR_LIP_EXPRESSION_JAW_OPEN_HTC = 3,
        XR_LIP_EXPRESSION_MOUTH_APE_SHAPE_HTC = 4,
        XR_LIP_EXPRESSION_MOUTH_UPPER_RIGHT_HTC = 5,
        XR_LIP_EXPRESSION_MOUTH_UPPER_LEFT_HTC = 6,
        XR_LIP_EXPRESSION_MOUTH_LOWER_RIGHT_HTC = 7,
        XR_LIP_EXPRESSION_MOUTH_LOWER_LEFT_HTC = 8,
        XR_LIP_EXPRESSION_MOUTH_UPPER_OVERTURN_HTC = 9,
        XR_LIP_EXPRESSION_MOUTH_LOWER_OVERTURN_HTC = 10,
        XR_LIP_EXPRESSION_MOUTH_POUT_HTC = 11,
        XR_LIP_EXPRESSION_MOUTH_SMILE_RIGHT_HTC = 12,
        XR_LIP_EXPRESSION_MOUTH_SMILE_LEFT_HTC = 13,
        XR_LIP_EXPRESSION_MOUTH_SAD_RIGHT_HTC = 14,
        XR_LIP_EXPRESSION_MOUTH_SAD_LEFT_HTC = 15,
        XR_LIP_EXPRESSION_CHEEK_PUFF_RIGHT_HTC = 16,
        XR_LIP_EXPRESSION_CHEEK_PUFF_LEFT_HTC = 17,
        XR_LIP_EXPRESSION_CHEEK_SUCK_HTC = 18,
        XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC = 19,
        XR_LIP_EXPRESSION_MOUTH_UPPER_UPLEFT_HTC = 20,
        XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNRIGHT_HTC = 21,
        XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNLEFT_HTC = 22,
        XR_LIP_EXPRESSION_MOUTH_UPPER_INSIDE_HTC = 23,
        XR_LIP_EXPRESSION_MOUTH_LOWER_INSIDE_HTC = 24,
        XR_LIP_EXPRESSION_MOUTH_LOWER_OVERLAY_HTC = 25,
        XR_LIP_EXPRESSION_TONGUE_LONGSTEP1_HTC = 26,
        XR_LIP_EXPRESSION_TONGUE_LEFT_HTC = 27,
        XR_LIP_EXPRESSION_TONGUE_RIGHT_HTC = 28,
        XR_LIP_EXPRESSION_TONGUE_UP_HTC = 29,
        XR_LIP_EXPRESSION_TONGUE_DOWN_HTC = 30,
        XR_LIP_EXPRESSION_TONGUE_ROLL_HTC = 31,
        XR_LIP_EXPRESSION_TONGUE_LONGSTEP2_HTC = 32,
        XR_LIP_EXPRESSION_TONGUE_UPRIGHT_MORPH_HTC = 33,
        XR_LIP_EXPRESSION_TONGUE_UPLEFT_MORPH_HTC = 34,
        XR_LIP_EXPRESSION_TONGUE_DOWNRIGHT_MORPH_HTC = 35,
        XR_LIP_EXPRESSION_TONGUE_DOWNLEFT_MORPH_HTC = 36,
        XR_LIP_EXPRESSION_MAX_ENUM_HTC = 37
    };

You can use the following API to retrieve the array of eye expression values if the return value is true.

    using VIVE.OpenXR.FacialTracking;
    
    bool ViveFacialTracking.GetFacialExpressions(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, out float[] expressionWeightings);

