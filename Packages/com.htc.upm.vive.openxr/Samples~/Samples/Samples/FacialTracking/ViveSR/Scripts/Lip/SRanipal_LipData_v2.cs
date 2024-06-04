//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System;
using System.Runtime.InteropServices;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    public enum XrLipShapeHTC
    {
        XR_LIP_SHAPE_NONE_HTC = -1,
        XR_LIP_SHAPE_JAW_RIGHT_HTC = 0,
        XR_LIP_SHAPE_JAW_LEFT_HTC = 1,
        XR_LIP_SHAPE_JAW_FORWARD_HTC = 2,
        XR_LIP_SHAPE_JAW_OPEN_HTC = 3,
        XR_LIP_SHAPE_MOUTH_APE_SHAPE_HTC = 4,
        XR_LIP_SHAPE_MOUTH_UPPER_RIGHT_HTC = 5,
        XR_LIP_SHAPE_MOUTH_UPPER_LEFT_HTC = 6,
        XR_LIP_SHAPE_MOUTH_LOWER_RIGHT_HTC = 7,
        XR_LIP_SHAPE_MOUTH_LOWER_LEFT_HTC = 8,
        XR_LIP_SHAPE_MOUTH_UPPER_OVERTURN_HTC = 9,
        XR_LIP_SHAPE_MOUTH_LOWER_OVERTURN_HTC = 10,
        XR_LIP_SHAPE_MOUTH_POUT_HTC = 11,
        XR_LIP_SHAPE_MOUTH_SMILE_RIGHT_HTC = 12,
        XR_LIP_SHAPE_MOUTH_SMILE_LEFT_HTC = 13,
        XR_LIP_SHAPE_MOUTH_SAD_RIGHT_HTC = 14,
        XR_LIP_SHAPE_MOUTH_SAD_LEFT_HTC = 15,
        XR_LIP_SHAPE_CHEEK_PUFF_RIGHT_HTC = 16,
        XR_LIP_SHAPE_CHEEK_PUFF_LEFT_HTC = 17,
        XR_LIP_SHAPE_CHEEK_SUCK_HTC = 18,
        XR_LIP_SHAPE_MOUTH_UPPER_UPRIGHT_HTC = 19,
        XR_LIP_SHAPE_MOUTH_UPPER_UPLEFT_HTC = 20,
        XR_LIP_SHAPE_MOUTH_LOWER_DOWNRIGHT_HTC = 21,
        XR_LIP_SHAPE_MOUTH_LOWER_DOWNLEFT_HTC = 22,
        XR_LIP_SHAPE_MOUTH_UPPER_INSIDE_HTC = 23,
        XR_LIP_SHAPE_MOUTH_LOWER_INSIDE_HTC = 24,
        XR_LIP_SHAPE_MOUTH_LOWER_OVERLAY_HTC = 25,
        XR_LIP_SHAPE_TONGUE_LONGSTEP1_HTC = 26,
        XR_LIP_SHAPE_TONGUE_LEFT_HTC = 27,
        XR_LIP_SHAPE_TONGUE_RIGHT_HTC = 28,
        XR_LIP_SHAPE_TONGUE_UP_HTC = 29,
        XR_LIP_SHAPE_TONGUE_DOWN_HTC = 30,
        XR_LIP_SHAPE_TONGUE_ROLL_HTC = 31,
        XR_LIP_SHAPE_TONGUE_LONGSTEP2_HTC = 32,
        XR_LIP_SHAPE_TONGUE_UPRIGHT_MORPH_HTC = 33,
        XR_LIP_SHAPE_TONGUE_UPLEFT_MORPH_HTC = 34,
        XR_LIP_SHAPE_TONGUE_DOWNRIGHT_MORPH_HTC = 35,
        XR_LIP_SHAPE_TONGUE_DOWNLEFT_MORPH_HTC = 36,
        XR_LIP_SHAPE_MAX_ENUM_HTC = 37
    }
    public enum LipShape_v2
    {
        None = -1,
        Jaw_Right = 0,
        Jaw_Left = 1,
        Jaw_Forward = 2,
        Jaw_Open = 3,
        Mouth_Ape_Shape = 4,
        Mouth_Upper_Right = 5,
        Mouth_Upper_Left = 6,
        Mouth_Lower_Right = 7,
        Mouth_Lower_Left = 8,
        Mouth_Upper_Overturn = 9,
        Mouth_Lower_Overturn = 10,
        Mouth_Pout = 11,
        Mouth_Smile_Right = 12,
        Mouth_Smile_Left = 13,
        Mouth_Sad_Right = 14,
        Mouth_Sad_Left = 15,
        Cheek_Puff_Right = 16,
        Cheek_Puff_Left = 17,
        Cheek_Suck = 18,
        Mouth_Upper_UpRight = 19,
        Mouth_Upper_UpLeft = 20,
        Mouth_Lower_DownRight = 21,
        Mouth_Lower_DownLeft = 22,
        Mouth_Upper_Inside = 23,
        Mouth_Lower_Inside = 24,
        Mouth_Lower_Overlay = 25,
        Tongue_LongStep1 = 26,
        Tongue_LongStep2 = 32,
        Tongue_Down = 30,
        Tongue_Up = 29,
        Tongue_Right = 28,
        Tongue_Left = 27,
        Tongue_Roll = 31,
        Tongue_UpLeft_Morph = 34,
        Tongue_UpRight_Morph = 33,
        Tongue_DownLeft_Morph = 36,
        Tongue_DownRight_Morph = 35,
        Max = 37,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PredictionData_v2
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public float[] blend_shape_weight;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct LipData_v2
    {
        public int frame;
        public int time;
        public IntPtr image;
        public PredictionData_v2 prediction_data;
    };
}
