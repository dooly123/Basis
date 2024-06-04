// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace VIVE.OpenXR
{
    public class VisibilityMaskDisabler : MonoBehaviour
    {
        const string TAG = "VisibilityMaskDisabler";
        void Enable()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (NeedWorkAround() && XRSettings.occlusionMaskScale != 0.0f)
            {
                Debug.Log(TAG + "Try set scale to 0");
                XRSettings.occlusionMaskScale = 0.0f;
            }
        }

        bool NeedWorkAround()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Vulkan && (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePass || XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassMultiview))
            {
                return true;
            }
            return false;
        }
    }


}

