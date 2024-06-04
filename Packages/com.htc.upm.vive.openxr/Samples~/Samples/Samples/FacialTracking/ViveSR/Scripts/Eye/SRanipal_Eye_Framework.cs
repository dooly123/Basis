//========= Copyright 2018, HTC Corporation. All rights reserved. ===========

using System;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    public class SRanipal_Eye_Framework : MonoBehaviour
    {
        public enum FrameworkStatus { STOP, START, WORKING, ERROR, NOT_SUPPORT }
        /// <summary>
        /// The status of the anipal engine.
        /// </summary>
        public static FrameworkStatus Status { get; protected set; }

        /// <summary>
        /// Currently supported lip motion prediction engine's version.
        /// </summary>
        public enum SupportedEyeVersion { version1, version2 }

        /// <summary>
        /// Whether to enable anipal's Eye module.
        /// </summary>
        public bool EnableEye = true;

        /// <summary>
        /// Which version of eye prediction engine will be used, default is version 1.
        /// </summary>
        public SupportedEyeVersion EnableEyeVersion = SupportedEyeVersion.version2;
        private static SRanipal_Eye_Framework Mgr = null;
        public static SRanipal_Eye_Framework Instance
        {
            get
            {
                if (Mgr == null)
                {
                    Mgr = FindObjectOfType<SRanipal_Eye_Framework>();
                }
                if (Mgr == null)
                {
                    Debug.LogError("SRanipal_Eye_Framework not found");
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

        public void StartFramework()
        {
            if (!EnableEye) return;
            if (Status == FrameworkStatus.WORKING || Status == FrameworkStatus.NOT_SUPPORT) return;
            if (EnableEyeVersion == SupportedEyeVersion.version1)
            {
                Debug.LogError("[SRanipal] Initial Version 1 Eye not supported now : ");
            }
            else
            {
                var feature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
                if (feature && feature.CreateFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC))
                {
                    Debug.Log("[SRanipal] Initial Eye v2 success!");
                    Status = FrameworkStatus.WORKING;
                }
                else
                {
                    Debug.LogError("[SRanipal] Initial Eye v2 failed!");
                    Status = FrameworkStatus.ERROR;
                }

            }
        }

        public void StopFramework()
        {
            if (Status != FrameworkStatus.NOT_SUPPORT)
            {
                if (Status != FrameworkStatus.STOP)
                {
                    if (EnableEyeVersion == SupportedEyeVersion.version1)
                    {
                        Debug.LogError("[SRanipal] Initial Version 1 Eye not supported now : ");
                    }
                    else
                    {
                        var feature = OpenXRSettings.Instance.GetFeature<ViveFacialTracking>();
                        if (feature && feature.DestroyFacialTracker(XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_EYE_DEFAULT_HTC))
                        {
                            Debug.Log("[SRanipal] Release Version 2 Eye success!");

                        }
                        else
                        {
                            Debug.LogError("[SRanipal] Release Version 2 Eye failed!");
                        }
                    }
                }
                else
                {
                    Debug.Log("[SRanipal] Stop Framework : module not on");
                }
            }
            Status = FrameworkStatus.STOP;
        }
    }
}
