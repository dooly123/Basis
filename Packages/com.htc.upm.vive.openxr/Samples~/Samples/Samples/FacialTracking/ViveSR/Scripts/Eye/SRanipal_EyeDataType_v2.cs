//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    public enum XrEyeShapeHTC
    {
        XR_EYE_SHAPE_NONE_HTC = -1,
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
        XR_EYE_EXPRESSION_MAX_ENUM_HTC = 14,
    }

    #region EyeShape_v2
    public enum EyeShape_v2
    {
        None = -1,
        Eye_Left_Blink = 0,
        Eye_Left_Wide,
        Eye_Left_Right,
        Eye_Left_Left,
        Eye_Left_Up,
        Eye_Left_Down,
        Eye_Right_Blink = 6,
        Eye_Right_Wide,
        Eye_Right_Right,
        Eye_Right_Left,
        Eye_Right_Up,
        Eye_Right_Down,
        Eye_Frown = 12,
        Eye_Left_Squeeze,
        Eye_Right_Squeeze,
        Max = 15,
    }



    [Serializable]
    public class EyeShapeTable_v2
    {
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public EyeShape_v2[] eyeShapes;
    }
    #endregion

    [StructLayout(LayoutKind.Sequential)]
    public struct SingleEyeExpression
    {
        public float eye_wide; /*!<A value representing how open eye widely.*/
        public float eye_squeeze; /*!<A value representing how the eye is closed tightly.*/
        public float eye_frown; /*!<A value representing user's frown.*/
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct EyeExpression
    {
        public SingleEyeExpression left;
        public SingleEyeExpression right;
    };

    [StructLayout(LayoutKind.Sequential)]
    /** @struct EyeData
    * A struct containing all data listed below.
    */
    public struct EyeData_v2
    {
        /** Indicate if there is a user in front of HMD. */
        public bool no_user;
        /** The frame sequence.*/
        public int frame_sequence;
        /** The time when the frame was capturing. in millisecond.*/
        public int timestamp;
        public VerboseData verbose_data;
        public EyeExpression expression_data;
    }
}
