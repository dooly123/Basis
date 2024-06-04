using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.CompositionLayer.Passthrough;

using VIVE.OpenXR.Samples;

namespace VIVE.OpenXR.CompositionLayer.Samples.Passthrough
{
    public class PassthroughSample_Planar : MonoBehaviour
    {
        private int activePassthroughID = 0;
        private LayerType currentActiveLayerType = LayerType.Underlay;

        private void Update()
        {
            if (activePassthroughID == 0)
            {
                StartPassthrough();
            }

            if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.B)) //Set Passthrough as Overlay
            {
                SetPassthroughToOverlay();
            }
            if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.A)) //Set Passthrough as Underlay
            {
                SetPassthroughToUnderlay();
            }
        }

        public void SetPassthroughToOverlay()
        {
            if (activePassthroughID != 0)
            {
                CompositionLayerPassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Overlay);
                currentActiveLayerType = LayerType.Overlay;
            }
        }

        public void SetPassthroughToUnderlay()
        {
            if (activePassthroughID != 0)
            {
                CompositionLayerPassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Underlay);
                currentActiveLayerType = LayerType.Underlay;
            }
        }

        void StartPassthrough()
        {
            activePassthroughID = CompositionLayerPassthroughAPI.CreatePlanarPassthrough(currentActiveLayerType, OnDestroyPassthroughFeatureSession);
        }

        void OnDestroyPassthroughFeatureSession(int passthroughID)
        {
            CompositionLayerPassthroughAPI.DestroyPassthrough(passthroughID);
            activePassthroughID = 0;
        }
    }
}
