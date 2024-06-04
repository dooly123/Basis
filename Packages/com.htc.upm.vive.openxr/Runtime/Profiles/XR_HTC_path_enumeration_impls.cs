// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.OpenXR;
namespace VIVE.OpenXR
{
    public class XR_HTC_path_enumeration_impls : XR_HTC_path_enumeration_defs
    {
        const string LOG_TAG = "VIVE.OpenXR.XR_HTC_path_enumeration_impls";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
        public XR_HTC_path_enumeration_impls() { DEBUG("XR_HTC_path_enumeration_impls()"); }
        private VivePathEnumeration feature = null;

        private void ASSERT_FEATURE()
        {
            if (feature == null) { feature = OpenXRSettings.Instance.GetFeature<VivePathEnumeration>(); }
        }

        public override XrResult xrEnumeratePathsForInteractionProfileHTC(
            ref XrPathsForInteractionProfileEnumerateInfoHTC createInfo,
            UInt32 pathCapacityInput,
            ref UInt32 pathCountOutput,
            [In, Out] XrPath[] paths)
        {
            DEBUG("xrEnumeratePathsForInteractionProfileHTC");
            XrResult result = XrResult.XR_ERROR_VALIDATION_FAILURE;
            ASSERT_FEATURE();
            if (feature)
            {
                result = feature.EnumeratePathsForInteractionProfileHTC(ref createInfo, pathCapacityInput,
                    ref pathCountOutput, paths);
                if (result != XrResult.XR_SUCCESS) { paths = null; }
            }
            paths = null;
            return result;
        }

        public override bool GetUserPaths(string interactionProfileString, out XrPath[] userPaths)
        {
            ASSERT_FEATURE();
            if (feature)
            {
                if (feature.GetUserPaths(interactionProfileString, out userPaths))
                {
                    return true;
                }
                else
                {
                    userPaths = null;
                    return false;
                }
            }
            else
            {
                userPaths = null;
                return false;
            }
        }

        public override bool GetInputPathsWithUserPath(string interactionProfileString, XrPath userPath, out XrPath[] inputPaths)
        {
            ASSERT_FEATURE();
            if (feature)
            {
                if (feature.GetInputPathsWithUserPath(interactionProfileString, userPath,out inputPaths))
                {
                    return true;
                }
                else
                {
                    inputPaths = null;
                    return false;
                }
            }
            else
            {
                inputPaths = null;
                return false;
            }
        }

        public override string PathToString(ulong path) 
        {
            ASSERT_FEATURE();
            if (feature)
                return feature.xrPathToString(path);
            else
                return null;
        }
        public override ulong StringToPath(string str)
        {
            ASSERT_FEATURE();
            if (feature)
                return feature.xrStringToPath(str);
            else
                return 0;
        }
    }
}
 
