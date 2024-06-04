using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace VIVE.OpenXR.Samples
{

    public enum VRSHandFlag
    {
        None = 0,
        Left = 1,
        Right = 2,
    }

    public enum VRSButtonReference
    {
        GripR = 0,
        TriggerR = 1,
        JoyStickR = 2,
        A = 3,
        B = 4,
        GripL = 5,
        TriggerL = 6,
        JoyStickL = 7,
        X = 8,
        Y = 9,
    }

    public enum VRSPositionReference
    {
        Head = 0,
        AimR = 1,
        GripR = 2,
        AimL = 3,
        GripL = 4,
    }

    public enum VRSRotationReference
    {
        Head = 0,
        AimR = 1,
        GripR = 2,
        AimL = 3,
        GripL = 4,
        HandAim = 5,
    }

    public enum VRSDevice
    {
        HMD = 0,
        LeftController = 1,
        RightController = 2,
    }
}
