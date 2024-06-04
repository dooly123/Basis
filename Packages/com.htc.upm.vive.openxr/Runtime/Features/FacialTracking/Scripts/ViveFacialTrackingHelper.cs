// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Runtime.InteropServices;

namespace VIVE.OpenXR.FacialTracking
{
    /// <summary>
    /// The XrFacialTrackerHTC handle represents the resources for an facial tracker of the specific facial tracking type.
    /// </summary>
    public struct XrFacialTrackerHTC : IEquatable<UInt64>
    {
        private readonly UInt64 value;

        public XrFacialTrackerHTC(UInt64 u)
        {
            value = u;
        }

        public static implicit operator UInt64(XrFacialTrackerHTC equatable)
        {
            return equatable.value;
        }
        public static implicit operator XrFacialTrackerHTC(UInt64 u)
        {
            return new XrFacialTrackerHTC(u);
        }

        public bool Equals(XrFacialTrackerHTC other)
        {
            return value == other.value;
        }
        public bool Equals(UInt64 other)
        {
            return value == other;
        }
        public override bool Equals(object obj)
        {
            return obj is XrFacialTrackerHTC && Equals((XrFacialTrackerHTC)obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static bool operator ==(XrFacialTrackerHTC a, XrFacialTrackerHTC b) { return a.Equals(b); }
        public static bool operator !=(XrFacialTrackerHTC a, XrFacialTrackerHTC b) { return !a.Equals(b); }
        public static bool operator >=(XrFacialTrackerHTC a, XrFacialTrackerHTC b) { return a.value >= b.value; }
        public static bool operator <=(XrFacialTrackerHTC a, XrFacialTrackerHTC b) { return a.value <= b.value; }
        public static bool operator >(XrFacialTrackerHTC a, XrFacialTrackerHTC b) { return a.value > b.value; }
        public static bool operator <(XrFacialTrackerHTC a, XrFacialTrackerHTC b) { return a.value < b.value; }
        public static XrFacialTrackerHTC operator +(XrFacialTrackerHTC a, XrFacialTrackerHTC b) { return a.value + b.value; }
        public static XrFacialTrackerHTC operator -(XrFacialTrackerHTC a, XrFacialTrackerHTC b) { return a.value - b.value; }
        public static XrFacialTrackerHTC operator *(XrFacialTrackerHTC a, XrFacialTrackerHTC b) { return a.value * b.value; }
        public static XrFacialTrackerHTC operator /(XrFacialTrackerHTC a, XrFacialTrackerHTC b)
        {
            if (b.value == 0)
            {
                throw new DivideByZeroException();
            }
            return a.value / b.value;
        }

    }
    /// <summary>
    /// The XrFacialTrackingTypeHTC describes which type of tracking the <see cref="XrFacialTrackerHTC">XrFacialTrackerHTC</see> is using.
    /// </summary>
    public enum XrFacialTrackingTypeHTC
    {
        /// <summary>
        /// Specifies this handle will observe eye expressions, with values indexed by <see cref="XrEyeExpressionHTC">XrEyeExpressionHTC</see> whose count is <see cref="ViveFacialTrackingHelper.XR_FACIAL_EXPRESSION_EYE_COUNT_HTC">XR_FACIAL_EXPRESSION_EYE_COUNT_HTC</see>.
        /// </summary>
        XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC = 1,
        /// <summary>
        /// Specifies this handle will observe lip expressions, with values indexed by <see cref="XrLipExpressionHTC">XrLipExpressionHTC</see> whose count is <see cref="ViveFacialTrackingHelper.XR_FACIAL_EXPRESSION_LIP_COUNT_HTC">XR_FACIAL_EXPRESSION_LIP_COUNT_HTC</see>.
        /// </summary>
        XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC = 2,
    };
    /// <summary>
    /// Indicates the eye expressions. Refer to <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrEyeExpressionHTC">XrEyeExpressionHTC</see> about the illustrations. 
    /// </summary>
    public enum XrEyeExpressionHTC
    {
        /// <summary>
        /// This blend shape influences blinking of the right eye. When this value goes higher, left eye approaches close.
        /// </summary>
        XR_EYE_EXPRESSION_LEFT_BLINK_HTC = 0,
        /// <summary>
        /// This blend shape keeps left eye wide and at that time XR_EYE_EXPRESSION_LEFT_BLINK_HTC value is 0.
        /// </summary>
        XR_EYE_EXPRESSION_LEFT_WIDE_HTC = 1,
        /// <summary>
        /// This blend shape influences blinking of the right eye. When this value goes higher, right eye approaches close.
        /// </summary>
        XR_EYE_EXPRESSION_RIGHT_BLINK_HTC = 2,
        /// <summary>
        /// This blend shape keeps right eye wide and at that time XR_EYE_EXPRESSION_RIGHT_BLINK_HTC value is 0.
        /// </summary>
        XR_EYE_EXPRESSION_RIGHT_WIDE_HTC = 3,
        /// <summary>
        /// The blend shape closes eye tightly and at that time XR_EYE_EXPRESSION_LEFT_BLINK_HTC value is 1.
        /// </summary>
        XR_EYE_EXPRESSION_LEFT_SQUEEZE_HTC = 4,
        /// <summary>
        /// The blend shape closes eye tightly and at that time XR_EYE_EXPRESSION_RIGHT_BLINK_HTC value is 1.
        /// </summary>
        XR_EYE_EXPRESSION_RIGHT_SQUEEZE_HTC = 5,
        /// <summary>
        /// This blendShape influences the muscles around the left eye, moving these muscles further downward with a higher value.
        /// </summary>
        XR_EYE_EXPRESSION_LEFT_DOWN_HTC = 6,
        /// <summary>
        /// This blendShape influences the muscles around the right eye, moving these muscles further downward with a higher value.
        /// </summary>
        XR_EYE_EXPRESSION_RIGHT_DOWN_HTC = 7,
        /// <summary>
        /// This blendShape influences the muscles around the left eye, moving these muscles further leftward with a higher value.
        /// </summary>
        XR_EYE_EXPRESSION_LEFT_OUT_HTC = 8,
        /// <summary>
        /// This blendShape influences the muscles around the right eye, moving these muscles further leftward with a higher value.
        /// </summary>
        XR_EYE_EXPRESSION_RIGHT_IN_HTC = 9,
        /// <summary>
        /// This blendShape influences the muscles around the left eye, moving these muscles further rightward with a higher value.
        /// </summary>
        XR_EYE_EXPRESSION_LEFT_IN_HTC = 10,
        /// <summary>
        /// This blendShape influences the muscles around the right eye, moving these muscles further rightward with a higher value.
        /// </summary>
        XR_EYE_EXPRESSION_RIGHT_OUT_HTC = 11,
        /// <summary>
        /// This blendShape influences the muscles around the left eye, moving these muscles further upward with a higher value.
        /// </summary>
        XR_EYE_EXPRESSION_LEFT_UP_HTC = 12,
        /// <summary>
        /// This blendShape influences the muscles around the right eye, moving these muscles further upward with a higher value.
        /// </summary>
        XR_EYE_EXPRESSION_RIGHT_UP_HTC = 13,
        XR_EYE_EXPRESSION_MAX_ENUM_HTC = 14
    };
    /// <summary>
    /// Indicates the lip expressions. Refer to <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrLipExpressionHTC">XrLipExpressionHTC</see> about the illustrations.
    /// </summary>
    public enum XrLipExpressionHTC
    {
        /// <summary>
        /// This blend shape moves the jaw further rightward with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_JAW_RIGHT_HTC = 0,
        /// <summary>
        /// This blend shape moves the jaw further leftward with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_JAW_LEFT_HTC = 1,
        /// <summary>
        /// This blend shape moves the jaw forward with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_JAW_FORWARD_HTC = 2,
        /// <summary>
        /// This blend shape opens the mouth further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_JAW_OPEN_HTC = 3,
        /// <summary>
        /// This blend shape stretches the jaw further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_APE_SHAPE_HTC = 4,
        /// <summary>
        /// This blend shape moves your upper lip rightward.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_UPPER_RIGHT_HTC = 5,
        /// <summary>
        /// This blend shape moves your upper lip leftward.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_UPPER_LEFT_HTC = 6,
        /// <summary>
        /// This blend shape moves your lower lip rightward.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_LOWER_RIGHT_HTC = 7,
        /// <summary>
        /// This blend shape moves your lower lip leftward.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_LOWER_LEFT_HTC = 8,
        /// <summary>
        /// This blend shape pouts your upper lip. Can be used with <see cref="XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC">XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC</see> and <see cref="XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPLEFT_HTC">XR_LIP_EXPRESSION_MOUTH_UPPER_UPLEFT_HTC</see> to complete upper O mouth shape.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_UPPER_OVERTURN_HTC = 9,
        /// <summary>
        /// This blend shape pouts your lower lip. Can be used with <see cref="XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC">XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC</see> and <see cref="XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNRIGHT_HTC">XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNRIGHT_HTC</see> to complete upper O mouth shape.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_LOWER_OVERTURN_HTC = 10,
        /// <summary>
        /// This blend shape allows the lips to pout more with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_POUT_HTC = 11,
        /// <summary>
        /// This blend shape raises the right side of the mouth further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_SMILE_RIGHT_HTC = 12,
        /// <summary>
        /// This blend shape raises the left side of the mouth further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_SMILE_LEFT_HTC = 13,
        /// <summary>
        /// This blend shape lowers the right side of the mouth further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_SAD_RIGHT_HTC = 14,
        /// <summary>
        /// This blend shape lowers the left side of the mouth further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_SAD_LEFT_HTC = 15,
        /// <summary>
        /// This blend shape puffs up the right side of the cheek further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_CHEEK_PUFF_RIGHT_HTC = 16,
        /// <summary>
        /// This blend shape puffs up the left side of the cheek further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_CHEEK_PUFF_LEFT_HTC = 17,
        /// <summary>
        /// This blend shape sucks in the cheeks on both sides further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_CHEEK_SUCK_HTC = 18,
        /// <summary>
        /// This blend shape raises the right upper lip further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC = 19,
        /// <summary>
        /// This blend shape raises the left upper lip further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_UPPER_UPLEFT_HTC = 20,
        /// <summary>
        /// This blend shape lowers the right lower lip further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNRIGHT_HTC = 21,
        /// <summary>
        /// This blend shape lowers the left lower lip further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNLEFT_HTC = 22,
        /// <summary>
        /// This blend shape rolls in the upper lip further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_UPPER_INSIDE_HTC = 23,
        /// <summary>
        /// This blend shape rolls in the lower lip further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_LOWER_INSIDE_HTC = 24,
        /// <summary>
        /// This blend shape stretches the lower lip further and lays it on the upper lip further with a higher value.
        /// </summary>
        XR_LIP_EXPRESSION_MOUTH_LOWER_OVERLAY_HTC = 25,
        /// <summary>
        /// This blend shape sticks the tongue out slightly.
        /// 
        /// In step 1 of extending the tongue, the main action of the tongue is to lift up, and the elongated length only extends to a little bit beyond the teeth.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_LONGSTEP1_HTC = 26,
        /// <summary>
        /// This blend shape sticks the tongue out and left extremely.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_LEFT_HTC = 27,
        /// <summary>
        /// This blend shape sticks the tongue out and right extremely.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_RIGHT_HTC = 28,
        /// <summary>
        /// This blend shape sticks the tongue out and up extremely.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_UP_HTC = 29,
        /// <summary>
        /// This blend shape sticks the tongue out and down extremely.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_DOWN_HTC = 30,
        /// <summary>
        /// This blend shape sticks the tongue out with roll type.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_ROLL_HTC = 31,
        /// <summary>
        /// This blend shape sticks the tongue out extremely.
        /// 
        /// Continuing the step 1, it extends the tongue to the longest.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_LONGSTEP2_HTC = 32,
        /// <summary>
        /// This blend shape doesn’t make sense. When both the right and up blend shapes appear at the same time, the tongue will be deformed.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_UPRIGHT_MORPH_HTC = 33,
        /// <summary>
        /// This blend shape doesn’t make sense. When both the left and up blend shapes appear at the same time, the tongue will be deformed.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_UPLEFT_MORPH_HTC = 34,
        /// <summary>
        /// This blend shape doesn’t make sense. When both the right and down blend shapes appear at the same time, the tongue will be deformed.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_DOWNRIGHT_MORPH_HTC = 35,
        /// <summary>
        /// This blend shape doesn’t make sense. When both the left and down blend shapes appear at the same time, the tongue will be deformed.
        /// </summary>
        XR_LIP_EXPRESSION_TONGUE_DOWNLEFT_MORPH_HTC = 36,
        XR_LIP_EXPRESSION_MAX_ENUM_HTC = 37
    };

    /// <summary>
    /// An application can inspect whether the system is capable of two of the facial tracking by extending the <see cref="XrSystemProperties">XrSystemProperties</see> with <see cref="XrSystemFacialTrackingPropertiesHTC">XrSystemFacialTrackingPropertiesHTC</see> structure when calling <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetSystemProperties">xrGetSystemProperties</see>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XrSystemFacialTrackingPropertiesHTC
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// Indicates if the current system is capable of generating eye expressions.
        /// </summary>
        public XrBool32 supportEyeFacialTracking;
        /// <summary>
        /// Indicates if the current system is capable of generating lip expressions.
        /// </summary>
        public XrBool32 supportLipFacialTracking;
    };

    /// <summary>
    /// The XrFacialTrackerCreateInfoHTC structure describes the information to create an <see cref="XrFacialTrackerHTC">XrFacialTrackerHTC</see> handle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XrFacialTrackerCreateInfoHTC
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// An XrFacialTrackingTypeHTC which describes which type of facial tracking should be used for this handle.
        /// </summary>
        public XrFacialTrackingTypeHTC facialTrackingType;

        /// <param name="in_type">The <see cref="XrStructureType">XrStructureType</see> of this structure.</param>
        /// <param name="in_next">NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.</param>
        /// <param name="in_facialTrackingType">An XrFacialTrackingTypeHTC which describes which type of facial tracking should be used for this handle.</param>
        public XrFacialTrackerCreateInfoHTC(XrStructureType in_type, IntPtr in_next, XrFacialTrackingTypeHTC in_facialTrackingType)
        {
            type = in_type;
            next = in_next;
            facialTrackingType = in_facialTrackingType;
        }
    };

    /// <summary>
    /// XrFacialExpressionsHTC structure returns data of a lip facial expression or an eye facial expression.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XrFacialExpressionsHTC
    {
        /// <summary>The XrStructureType of this structure.</summary>
        public XrStructureType type;
        /// <summary>NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.</summary>
        public IntPtr next;
        /// <summary>An XrBool32 indicating if the facial tracker is active.</summary>
        public XrBool32 isActive;
        /// <summary>When in time the expression is expressed.</summary>
        public XrTime sampleTime;
        /// <summary>A uint32_t describing the count of elements in expressionWeightings array.</summary>
        public UInt32 expressionCount;
        /// <summary>A float array filled in by the runtime, specifying the weightings for each blend shape.</summary>
        public IntPtr expressionWeightings;

        /// <param name="in_type">The XrStructureType of this structure.</param>
        /// <param name="in_next">NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.</param>
        /// <param name="in_isActive">An XrBool32 indicating if the facial tracker is active.</param>
        /// <param name="in_sampleTime">When in time the expression is expressed.</param>
        /// <param name="in_expressionCount">>A uint32_t describing the count of elements in expressionWeightings array.</param>
        /// <param name="in_expressionWeightings">A float array filled in by the runtime, specifying the weightings for each blend shape.</param>
        public XrFacialExpressionsHTC(
            XrStructureType in_type,
            IntPtr in_next,
            XrBool32 in_isActive,
            XrTime in_sampleTime,
            UInt32 in_expressionCount,
            IntPtr in_expressionWeightings)
        {
            type = in_type;
            next = in_next;
            isActive = in_isActive;
            sampleTime = in_sampleTime;
            expressionCount = in_expressionCount;
            expressionWeightings = in_expressionWeightings;
        }
    };

    public static class ViveFacialTrackingHelper
    {
        /// <summary> The number of blend shapes in an expression of type <see cref="XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC">XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC</see>. </summary>
        public const UInt32 XR_FACIAL_EXPRESSION_EYE_COUNT_HTC = 14;
        /// <summary> The number of blend shapes in an expression of type <see cref="XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC">XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC.</see> </summary>
        public const UInt32 XR_FACIAL_EXPRESSION_LIP_COUNT_HTC = 37;

        /// <summary>
        /// The delegate function of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateFacialTrackerHTC">xrCreateFacialTrackerHTC</see>.
        /// </summary>
        /// <param name="session">An XrSession in which the facial expression will be active.</param>
        /// <param name="createInfo">The <see cref="XrFacialTrackerCreateInfoHTC">XrFacialTrackerCreateInfoHTC</see> used to specify the facial tracking type.</param>
        /// <param name="facialTracker">The returned <see cref="XrFacialTrackerHTC">XrFacialTrackerHTC</see> handle.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public delegate XrResult xrCreateFacialTrackerHTCDelegate(
            XrSession session,
            XrFacialTrackerCreateInfoHTC createInfo,
            out XrFacialTrackerHTC facialTracker);

        /// <summary>
        /// The delegate function of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroyFacialTrackerHTC">xrDestroyFacialTrackerHTC</see>.
        /// </summary>
        /// <param name="facialTracker">An XrFacialTrackerHTC previously created by xrCreateFacialTrackerHTC.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public delegate XrResult xrDestroyFacialTrackerHTCDelegate(
            XrFacialTrackerHTC facialTracker);

        /// <summary>
        /// The delegate function of <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetFacialExpressionsHTC">xrGetFacialExpressionsHTC</see>.
        /// </summary>
        /// <param name="facialTracker">An <see cref="XrFacialTrackerHTC">XrFacialTrackerHTC</see> previously created by <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateFacialTrackerHTC">xrCreateFacialTrackerHTC</see>.</param>
        /// <param name="facialExpressions">A pointer to <see cref="XrFacialExpressionsHTC">XrFacialExpressionsHTC</see> receiving the returned facial expressions.</param>
        /// <returns></returns>
        public delegate XrResult xrGetFacialExpressionsHTCDelegate(
            XrFacialTrackerHTC facialTracker,
            ref XrFacialExpressionsHTC facialExpressions);
    }
}
