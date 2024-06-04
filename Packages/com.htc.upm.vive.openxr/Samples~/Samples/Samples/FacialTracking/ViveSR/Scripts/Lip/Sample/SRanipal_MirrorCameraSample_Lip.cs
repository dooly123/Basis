//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using UnityEngine;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    /// <summary>
    /// A very basic mirror.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SRanipal_MirrorCameraSample_Lip : MonoBehaviour
    {
        private const float Distance = 0.6f;

        private void Update()
        {
            if (SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING) return;
            SetMirroTransform();
        }

        private void SetMirroTransform()
        {
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * Distance;
            transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, transform.position.z);
            transform.LookAt(Camera.main.transform);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }
}
