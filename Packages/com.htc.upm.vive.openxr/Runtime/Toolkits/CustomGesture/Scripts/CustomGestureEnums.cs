// Copyright HTC Corporation All Rights Reserved.

namespace VIVE.OpenXR.Toolkits.CustomGesture
{

    public class CGEnums
    {
        public enum HandFlag
        {
            /// <summary>
            /// The flag indicating no hand
            /// </summary>
            None = 0,
            /// <summary>
            /// The flag indicating the left hand
            /// </summary>
            Left = 1,
            /// <summary>
            /// The flag indicating the right hand
            /// </summary>
            Right = 2,
            Either = 3,
            Dual = 4,
            Num = 5,
        }

        public enum FingerStatus
        {
            None = 0,
            Straight = 1,
            Bending = 2,
            Bended = 3,
            Num = 4,
        }

        public enum FingerFlag
        {
            None = 0,
            Thumb = 1,
            Index = 2,
            Middle = 3,
            Ring = 4,
            Pinky = 5,
            Num = 6,
        }
    }
}
