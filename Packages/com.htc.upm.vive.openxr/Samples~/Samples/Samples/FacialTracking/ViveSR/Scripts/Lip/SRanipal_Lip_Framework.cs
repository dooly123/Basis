//========= Copyright 2019, HTC Corporation. All rights reserved. ===========

using System;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    public class SRanipal_Lip_Framework : MonoBehaviour
    {
        public enum FrameworkStatus { STOP, START, WORKING, ERROR }
        /// <summary>
        /// The status of the anipal engine.
        /// </summary>
        public static FrameworkStatus Status { get; protected set; }
        /// <summary>
        /// Whether to enable anipal's Lip module.
        /// </summary>
        public bool EnableLip = true;

        /// <summary>
        /// Currently supported lip motion prediction engine's version.
        /// </summary>
        public enum SupportedLipVersion { version1, version2 }
        /// <summary>
        /// Which version of lip motion prediction engine will be used, default is version 1.
        /// </summary>
        public SupportedLipVersion EnableLipVersion = SupportedLipVersion.version2;

        private static SRanipal_Lip_Framework Mgr = null;
        public static SRanipal_Lip_Framework Instance
        {
            get
            {
                if (Mgr == null)
                {
                    Mgr = FindObjectOfType<SRanipal_Lip_Framework>();
                }
                if (Mgr == null)
                {
                    Debug.LogError("SRanipal_Lip_Framework not found");
                }
                return Mgr;
            }
        }

        void Start()
        {
            StartFramework();
        }

        void OnDestroy()
        {
            StopFramework();
        }

        private void StartFramework()
        {
            if (!EnableLip) return;
            if (Status == FrameworkStatus.WORKING) return;
            Status = FrameworkStatus.START;

            if (EnableLipVersion == SupportedLipVersion.version2)
            {
                Debug.Log("[SRanipal] Starting to Initial Version 2 Lip");
                var feature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
                if (feature && feature.CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC))
                {
                    Debug.Log("[SRanipal] Initial Version 2 Lip success!");
                    Status = FrameworkStatus.WORKING;
                }
                else
                {
                    Debug.LogError("[SRanipal] Initial Version 2 Lip failed!");
                    Status = FrameworkStatus.ERROR;
                }
            }
        }

        public void StopFramework()
        {
            if (Status != FrameworkStatus.STOP)
            {
                if (EnableLipVersion == SupportedLipVersion.version2)
                {
                    var feature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
                    if (feature && feature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC))
                    {
                        Debug.Log("[SRanipal] Release Version 2 Lip success!");

                    }
                    else
                    {
                        Debug.LogError("[SRanipal] Release Version 2 Lip failed!");
                    }
                    //Error result = SRanipal_API.Release(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2);
                    //if (result == Error.WORK) Debug.Log("[SRanipal] Release Version 2 Lip : " + result);
                    //else Debug.LogError("[SRanipal] Release Version 2 Lip : " + result);
                }
            }
            else
            {
                Debug.Log("[SRanipal] Stop Framework : module not on");
            }
            Status = FrameworkStatus.STOP;
        }
    }
}
