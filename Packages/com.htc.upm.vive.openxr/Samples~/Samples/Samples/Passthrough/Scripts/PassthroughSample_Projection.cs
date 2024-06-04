using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.CompositionLayer.Passthrough;

using VIVE.OpenXR.Samples;

namespace VIVE.OpenXR.CompositionLayer.Samples.Passthrough
{
    public class PassthroughSample_Projection : MonoBehaviour
    {
        public Mesh passthroughMesh = null;
        public Transform passthroughMeshTransform = null;

        public GameObject hmd = null;

        public Text scaleValueText;
        public Slider scaleSlider = null;
        private Vector3 scale = Vector3.one;
        private float scaleModifier = 1f;

        private int activePassthroughID = 0;
        private LayerType currentActiveLayerType = LayerType.Underlay;
        private ProjectedPassthroughSpaceType currentActiveSpaceType = ProjectedPassthroughSpaceType.Worldlock;

        private void Start()
        {
            if (hmd == null) hmd = Camera.main.gameObject;

            if (scaleSlider != null) scaleSlider.value = scaleModifier;
        }

        private void Update()
        {
			if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.B)) //Set Passthrough as Overlay
			{
				SetPassthroughToOverlay();
				if (activePassthroughID != 0)  SetPassthroughMesh();
			}
			if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.A)) //Set Passthrough as Underlay
			{
				SetPassthroughToUnderlay();
				if (activePassthroughID != 0) SetPassthroughMesh();
			}
			if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.X)) //Switch to world lock
			{
				SetWorldLock();
				if (activePassthroughID != 0) SetPassthroughMesh();
			}
			if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.Y)) //Switch to head lock
			{
				SetHeadLock();
				if (activePassthroughID != 0) SetPassthroughMesh();
			}

			if (passthroughMesh != null && passthroughMeshTransform != null)
            {
                if (activePassthroughID == 0)
                {
                    StartPassthrough();
                }
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

        public void SetHeadLock()
        {
            if (activePassthroughID != 0)
            {
                if (CompositionLayerPassthroughAPI.SetProjectedPassthroughSpaceType(activePassthroughID, ProjectedPassthroughSpaceType.Headlock))
                {
                    passthroughMeshTransform.SetParent(hmd.transform);

                    currentActiveSpaceType = ProjectedPassthroughSpaceType.Headlock;
                }
            }
        }

        public void SetWorldLock()
        {
            if (activePassthroughID != 0)
            {
                if (CompositionLayerPassthroughAPI.SetProjectedPassthroughSpaceType(activePassthroughID, ProjectedPassthroughSpaceType.Worldlock))
                {
                    passthroughMeshTransform.SetParent(null);

                    currentActiveSpaceType = ProjectedPassthroughSpaceType.Worldlock;
                }
            }
        }

        public void OnScaleSliderValueChange(float newScaleModifier)
        {
            scaleValueText.text = newScaleModifier.ToString();
            if (activePassthroughID != 0)
            {
                scaleModifier = newScaleModifier;
				SetPassthroughMesh();
			}
        }

        void StartPassthrough()
        {
            activePassthroughID = CompositionLayerPassthroughAPI.CreateProjectedPassthrough(currentActiveLayerType, OnDestroyPassthroughFeatureSession);
            SetPassthroughMesh();
        }

        void SetPassthroughMesh()
        {
            CompositionLayerPassthroughAPI.SetProjectedPassthroughMesh(activePassthroughID, passthroughMesh.vertices, passthroughMesh.triangles);
            switch (currentActiveSpaceType)
            {
                case ProjectedPassthroughSpaceType.Headlock: //Apply HMD offset
                    Vector3 relativePosition = hmd.transform.InverseTransformPoint(passthroughMeshTransform.transform.position);
                    Quaternion relativeRotation = Quaternion.Inverse(hmd.transform.rotation).normalized * passthroughMeshTransform.transform.rotation.normalized;
                    CompositionLayerPassthroughAPI.SetProjectedPassthroughMeshTransform(activePassthroughID, currentActiveSpaceType, relativePosition, relativeRotation, scale * scaleModifier, false);
                    break;
                case ProjectedPassthroughSpaceType.Worldlock:
                default:
                    CompositionLayerPassthroughAPI.SetProjectedPassthroughMeshTransform(activePassthroughID, currentActiveSpaceType, passthroughMeshTransform.transform.position, passthroughMeshTransform.transform.rotation, scale * scaleModifier);
                    break;
            }
        }

        void OnDestroyPassthroughFeatureSession(int passthroughID)
        {
            CompositionLayerPassthroughAPI.DestroyPassthrough(passthroughID);
            activePassthroughID = 0;
        }
    }
}
