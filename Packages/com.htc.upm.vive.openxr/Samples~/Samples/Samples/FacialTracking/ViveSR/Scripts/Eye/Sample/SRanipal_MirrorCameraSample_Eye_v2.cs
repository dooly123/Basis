//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System.Runtime.InteropServices;
using UnityEngine;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    /// <summary>
    /// A very basic mirror.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SRanipal_MirrorCameraSample_Eye_v2 : MonoBehaviour
    {
        private const float Distance = 0.6f;
        //private static EyeData_v2 eyeData = new EyeData_v2();
        private void Update()
        {
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING) return;

        }

        private void Release()
        {
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
