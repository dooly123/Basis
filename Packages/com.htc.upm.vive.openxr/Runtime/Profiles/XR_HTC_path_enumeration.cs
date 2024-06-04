// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VIVE.OpenXR
{
    public class XR_HTC_path_enumeration_defs
    {
        public virtual XrResult xrEnumeratePathsForInteractionProfileHTC(
            ref XrPathsForInteractionProfileEnumerateInfoHTC createInfo,
            UInt32 pathCapacityInput,
            ref UInt32 pathCountOutput,
            [In, Out] XrPath[] paths)
        {
            paths = null;
            return XrResult.XR_ERROR_RUNTIME_FAILURE;
        }

        public virtual bool GetUserPaths(string interactionProfileString, out XrPath[] userPaths)
        {
            userPaths = null;
            return false;
        }
        public virtual bool GetInputPathsWithUserPath(string interactionProfileString, XrPath userPath, out XrPath[] inputPaths)
        {
            inputPaths = null;
            return false;
        }

        public virtual string PathToString(ulong path) { return null; }
        public virtual ulong StringToPath(string str) {
            return 0; }
    }

    public static class XR_HTC_path_enumeration
    {
        static XR_HTC_path_enumeration_defs m_Instance = null;
        public static XR_HTC_path_enumeration_defs Interop
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new XR_HTC_path_enumeration_impls();
                }
                return m_Instance;
            }
        }

        public static XrResult xrEnumeratePathsForInteractionProfileHTC(
            ref XrPathsForInteractionProfileEnumerateInfoHTC createInfo,
            UInt32 pathCapacityInput,
            ref UInt32 pathCountOutput,
            [In, Out] XrPath[] paths)
        {
            return Interop.xrEnumeratePathsForInteractionProfileHTC(ref createInfo,
                pathCapacityInput,
                ref pathCountOutput,
                paths);
        }

        public static string PathToString(ulong path) { return Interop.PathToString(path); }
        public static ulong StringToPath(string str) 
        {
            return Interop.StringToPath(str); }

    }
}

