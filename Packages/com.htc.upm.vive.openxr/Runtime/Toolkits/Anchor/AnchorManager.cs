// Copyright HTC Corporation All Rights Reserved.
using System;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.Feature;
using VIVE.OpenXR.Anchor;
using static VIVE.OpenXR.Anchor.ViveAnchor;

namespace VIVE.OpenXR.Toolkits.Anchor
{
    public static class AnchorManager
    {
        static ViveAnchor feature = null;
        static bool isSupported = false;

        static void CheckFeature()
        {
            if (feature != null) return;

            feature = OpenXRSettings.Instance.GetFeature<ViveAnchor>();
            if (feature == null)
                throw new NotSupportedException("ViveAnchor feature is not enabled");
        }

        /// <summary>
        /// Helper to get the extention feature instance.
        /// </summary>
        /// <returns></returns>
        public static ViveAnchor GetFeature()
        {
            try
            {
                CheckFeature();
            }
            catch (NotSupportedException)
            {
                Debug.LogWarning("ViveAnchor feature is not enabled");
                return null;
            }
            return feature;
        }

        /// <summary>
        /// Check if the extension is supported.
        /// </summary>
        /// <returns></returns>
        public static bool IsSupported()
        {
            if (GetFeature() == null) return false;
            if (isSupported) return true;

            var ret = false;
            if (feature.GetProperties(out XrSystemAnchorPropertiesHTC properties) == XrResult.XR_SUCCESS)
            {
                Debug.Log("Anchor: IsSupported() properties.supportedFeatures: " + properties.supportsAnchor);
                ret = properties.supportsAnchor;
                isSupported = ret;
            }
            else
            {
                Debug.Log("Anchor: IsSupported() GetSystemProperties failed.");
            }

            return ret;
        }

        /// <summary>
        /// Create a spatial anchor at tracking space (Camera Rig).
        /// </summary>
        /// <param name="pose">The related pose to the tracking space (Camera Rig)</param>
        /// <returns>Anchor container</returns>
        public static Anchor CreateAnchor(Pose pose, string name)
        {
            try
            {
                CheckFeature();
                XrSpace baseSpace = feature.GetTrackingSpace();
                XrSpatialAnchorCreateInfoHTC createInfo = new XrSpatialAnchorCreateInfoHTC();
                createInfo.type = XrStructureType.XR_TYPE_SPATIAL_ANCHOR_CREATE_INFO_HTC;
                createInfo.poseInSpace = new XrPosef();
                createInfo.poseInSpace.position = pose.position.ToOpenXRVector();
                createInfo.poseInSpace.orientation = pose.rotation.ToOpenXRQuaternion();
                createInfo.name.name = name;
                createInfo.space = baseSpace;

                if (feature.CreateSpatialAnchor(createInfo, out XrSpace anchor) == XrResult.XR_SUCCESS)
                {
                    return new Anchor(anchor, name);
                }
            } catch (Exception) { }
            return null;
        }

        public static bool GetSpatialAnchorName(Anchor anchor, out string name)
        {
            return GetSpatialAnchorName(anchor.GetXrSpace(), out name);
        }

        public static bool GetSpatialAnchorName(XrSpace anchor, out string name)
        {
            name = "";
            CheckFeature();
            XrResult ret = feature.GetSpatialAnchorName(anchor, out XrSpatialAnchorNameHTC xrName);
            if (ret == XrResult.XR_SUCCESS)
                name = xrName.name;
            return ret == XrResult.XR_SUCCESS;
        }

        /// <summary>
        /// Get the XrSpace stand for current tracking space.
        /// </summary>
        /// <returns></returns>
        public static XrSpace GetTrackingSpace()
        {
            CheckFeature();
            return feature.GetTrackingSpace();
        }

        /// <summary>
        /// Get the pose related to current tracking space.  Only when position and orientation are both valid, the pose is valid.
        /// </summary>
        /// <param name="anchor"></param>
        /// <param name="pose"></param>
        /// <returns>true if both position and rotation are valid.</returns>
        public static bool GetTrackingSpacePose(Anchor anchor, out Pose pose)
        {
            var sw = SpaceWrapper.Instance;
            return anchor.GetRelatedPose(feature.GetTrackingSpace(), ViveInterceptors.Instance.GetPredictTime(), out pose);
        }

        /// <summary>
        /// Anchor is a named Space.  It can be used to create a spatial anchor, or get the anchor's name.
        /// After use it, you should call Dispose() to release the anchor.
        /// </summary>
        public class Anchor : VIVE.OpenXR.Feature.Space
        {
            /// <summary>
            /// The anchor's name
            /// </summary>
            string name;

            /// <summary>
            /// The anchor's name
            /// </summary>
            public string Name
            {
                get
                {
                    if (string.IsNullOrEmpty(name))
                        name = GetSpatialAnchorName();
                    return name;
                }
            }

            internal Anchor(XrSpace anchor, string name) : base(anchor)
            {
                // Get the current tracking space.
                this.name = name;
            }

            internal Anchor(Anchor other) : base(other.space)
            {
                // Get the current tracking space.
                name = other.name;
            }

            /// <summary>
            /// Get the anchor's name by using this anchor's handle, instead of the anchor's Name.  This will update the anchor's Name.
            /// </summary>
            /// <returns></returns>
            public string GetSpatialAnchorName()
            {
                AnchorManager.CheckFeature();
                if (AnchorManager.GetSpatialAnchorName(this, out string name))
                    return name;
                return null;
            }
        }
    }
}