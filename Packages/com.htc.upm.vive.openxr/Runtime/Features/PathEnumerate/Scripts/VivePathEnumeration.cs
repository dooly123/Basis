// Copyright HTC Corporation All Rights Reserved.

using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine;
using System.Text;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace VIVE.OpenXR
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "VIVE XR Path Enumeration",
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = "HTC",
        Desc = "The extension provides more flexibility for the user paths and input/output source paths related to an interaction profile. Developers can use this extension to obtain the path that the user has decided on.",
        DocumentationLink = "..\\Documentation",
        Version = "1.0.6",
        OpenxrExtensionStrings = kOpenxrExtensionString,
        FeatureId = featureId)]
#endif
    public class VivePathEnumeration : OpenXRFeature
    {
		const string LOG_TAG = "VIVE.OpenXR.VivePathEnumeration ";
        StringBuilder m_sb = null;
        StringBuilder sb {
            get {
                if (m_sb == null) { m_sb = new StringBuilder(); }
                return m_sb;
            }
        }
		void DEBUG(StringBuilder msg) { Debug.Log(msg); }
        void WARNING(StringBuilder msg) { Debug.LogWarning(msg); }
        void ERROR(StringBuilder msg) { Debug.LogError(msg); }
        /// <summary>
        /// OpenXR specification <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_HTC_path_enumeration">12.1. XR_HTC_path_enumeration</see>.
        /// </summary>
        public const string kOpenxrExtensionString = "XR_HTC_path_enumeration";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "vive.wave.openxr.feature.pathenumeration";

        #region OpenXR Life Cycle
#pragma warning disable
        private bool m_XrInstanceCreated = false;
#pragma warning enable
        private XrInstance m_XrInstance = 0;
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateInstance">xrCreateInstance</see> is done.
        /// </summary>
        /// <param name="xrInstance">The created instance.</param>
        /// <returns>True for valid <see cref="XrInstance">XrInstance</see></returns>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(kOpenxrExtensionString))
            {
                sb.Clear().Append(LOG_TAG).Append("OnInstanceCreate() ").Append(kOpenxrExtensionString).Append(" is NOT enabled."); WARNING(sb);
                return false;
            }

            m_XrInstanceCreated = true;
            m_XrInstance = xrInstance;
            sb.Clear().Append(LOG_TAG).Append("OnInstanceCreate() ").Append(m_XrInstance); DEBUG(sb);
            GetXrFunctionDelegates(m_XrInstance);
            return base.OnInstanceCreate(xrInstance);
        }

        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroyInstance">xrDestroyInstance</see> is done.
        /// </summary>
        /// <param name="xrInstance">The instance to destroy.</param>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            if (m_XrInstance == xrInstance)
            {
                m_XrInstanceCreated = false;
                m_XrInstance = 0;
            }
            sb.Clear().Append(LOG_TAG).Append("OnInstanceDestroy() ").Append(xrInstance); DEBUG(sb);
        }

        private XrSystemId m_XrSystemId = 0;
        /// <summary>
        /// Called when the <see cref="XrSystemId">XrSystemId</see> retrieved by <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetSystem">xrGetSystem</see> is changed.
        /// </summary>
        /// <param name="xrSystem">The system id.</param>
        protected override void OnSystemChange(ulong xrSystem)
        {
            m_XrSystemId = xrSystem;
            sb.Clear().Append(LOG_TAG).Append("OnSystemChange() ").Append(m_XrSystemId); DEBUG(sb);
        }

        private bool m_XrSessionCreated = false;
        private XrSession m_XrSession = 0;
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see> is done.
        /// </summary>
        /// <param name="xrSession">The created session ID.</param>
        protected override void OnSessionCreate(ulong xrSession)
        {
            m_XrSession = xrSession;
            m_XrSessionCreated = true;
            sb.Clear().Append(LOG_TAG).Append("OnSessionCreate() ").Append(m_XrSession); DEBUG(sb);
        }
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroySession">xrDestroySession</see> is done.
        /// </summary>
        /// <param name="xrSession">The session ID to destroy.</param>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            sb.Clear().Append(LOG_TAG).Append("OnSessionDestroy() ").Append(xrSession); DEBUG(sb);
            m_XrSession = 0;
            m_XrSessionCreated = false;
        }
        #endregion
        #region OpenXR function delegates
        /// xrGetInstanceProcAddr
        OpenXRHelper.xrGetInstanceProcAddrDelegate XrGetInstanceProcAddr;
        VivePathEnumerationHelper.xrEnumeratePathsForInteractionProfileHTCDelegate xrEnumeratePathsForInteractionProfileHTC;
        private bool GetXrFunctionDelegates(XrInstance xrInstance)
        {
            // xrGetInstanceProcAddr
            if (xrGetInstanceProcAddr != null && xrGetInstanceProcAddr != IntPtr.Zero)
            {
                sb.Clear().Append(LOG_TAG).Append("Get function pointer of xrGetInstanceProcAddr."); DEBUG(sb);
                XrGetInstanceProcAddr = Marshal.GetDelegateForFunctionPointer(
                    xrGetInstanceProcAddr,
                    typeof(OpenXRHelper.xrGetInstanceProcAddrDelegate)) as OpenXRHelper.xrGetInstanceProcAddrDelegate;
            }
            else
            {
                sb.Clear().Append(LOG_TAG).Append("No function pointer of xrGetInstanceProcAddr"); ERROR(sb);
                return false;
            }

            IntPtr funcPtr = IntPtr.Zero;
            /// xrEnumeratePathsForInteractionProfileHTC
            if (XrGetInstanceProcAddr(xrInstance, "xrEnumeratePathsForInteractionProfileHTC", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    sb.Clear().Append(LOG_TAG).Append("Get function pointer of xrEnumeratePathsForInteractionProfileHTC."); DEBUG(sb);
                    xrEnumeratePathsForInteractionProfileHTC = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(VivePathEnumerationHelper.xrEnumeratePathsForInteractionProfileHTCDelegate)) as VivePathEnumerationHelper.xrEnumeratePathsForInteractionProfileHTCDelegate;
                }
                else
                {
                    sb.Clear().Append(LOG_TAG).Append("No function pointer of xrEnumeratePathsForInteractionProfileHTC.");
                    ERROR(sb);
                    return false;
                }
            }
            else
            {
                sb.Clear().Append(LOG_TAG).Append("No function pointer of xrEnumeratePathsForInteractionProfileHTC");
                ERROR(sb);
                return false;
            }
            return true;
        }
        private List<T> CreateList<T>(UInt32 count, T initialValue)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < count; i++)
                list.Add(initialValue);

            return list;
        }

        public XrResult EnumeratePathsForInteractionProfileHTC(
            ref XrPathsForInteractionProfileEnumerateInfoHTC createInfo,
            UInt32 pathCapacityInput,
            ref UInt32 pathCountOutput,
            [In, Out] XrPath[] paths)
        {
            if (!m_XrInstanceCreated)
            {
                sb.Clear().Append(LOG_TAG).Append("EnumeratePathsForInteractionProfileHTC() XR_ERROR_INSTANCE_LOST."); ERROR(sb);
                paths = null;
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }
            return xrEnumeratePathsForInteractionProfileHTC(m_XrInstance, 
                ref createInfo, pathCapacityInput,ref pathCountOutput, paths);
        }

        public bool GetUserPaths(string interactionProfileString, out XrPath[] userPaths)
        {
            XrPathsForInteractionProfileEnumerateInfoHTC enumerateInfo;
            if (!m_XrInstanceCreated) { userPaths = null; return false; }

            string func = "GetUserPaths() ";

            if (xrEnumeratePathsForInteractionProfileHTC == null)
            {
                sb.Clear().Append(LOG_TAG).Append(func)
                    .Append("No function pointer of xrEnumeratePathsForInteractionProfileHTC"); WARNING(sb);
                userPaths = null;
                return false;
            }
            // 1. Get user path count of sepecified profile.
            UInt32 trackerCount = 0;
            enumerateInfo.type = (XrStructureType)1000319000;//Todo : update openxr spec to prevent hot code.
            enumerateInfo.next = IntPtr.Zero;
            enumerateInfo.interactionProfile = StringToPath(interactionProfileString); 
            enumerateInfo.userPath = OpenXRHelper.XR_NULL_PATH;

            XrResult result = xrEnumeratePathsForInteractionProfileHTC(m_XrInstance, ref enumerateInfo, 0, ref trackerCount, null);
            if (result != XrResult.XR_SUCCESS)
            {
                sb.Clear().Append(LOG_TAG).Append(func)
                    .Append("Retrieves trackerCount failed."); ERROR(sb);
                userPaths = null;
                return false;
            }
            //sb.Clear().Append(LOG_TAG).Append(func)
            //    .Append("Get profile ").Append(interactionProfileString).Append(" user path count: ").Append(trackerCount); DEBUG(sb);
            if (trackerCount > 0)
            {
                // 2. Get user paths of sepecified profile.
                List<XrPath> trackerList = CreateList<XrPath>(trackerCount, OpenXRHelper.XR_NULL_PATH);
                XrPath[] trackers = trackerList.ToArray();
                result = xrEnumeratePathsForInteractionProfileHTC(
                    m_XrInstance,
                    ref enumerateInfo,
                    pathCapacityInput: (UInt32)(trackers.Length & 0x7FFFFFFF),
                    pathCountOutput: ref trackerCount,
                    paths: trackers);
                if (result != XrResult.XR_SUCCESS)
                {
                    sb.Clear().Append(LOG_TAG).Append(func)
                        .Append("Retrieves trackers failed."); ERROR(sb);
                    userPaths = null;
                    return false;
                }
                userPaths = trackers;
                return true;
            }
            else
            {
                userPaths = null;
                return false;
            }
        }

        public bool GetInputPathsWithUserPath(string interactionProfileString, XrPath userPath, out XrPath[] inputPaths)
        {
            string func = "GetInputPathsWithUserPath() ";
            if (!m_XrInstanceCreated) { inputPaths = null; return false; }
            UInt32 trackerCount = 0;
            XrPathsForInteractionProfileEnumerateInfoHTC enumerateInfo;
            enumerateInfo.type = (XrStructureType)1000319000;//Todo : update openxr spec and prevent hard-code.
            enumerateInfo.next = IntPtr.Zero;
            enumerateInfo.interactionProfile = StringToPath(interactionProfileString);
            enumerateInfo.userPath = userPath;
            UInt32 Count = 0;
            xrEnumeratePathsForInteractionProfileHTC(
                m_XrInstance,
                ref enumerateInfo,
                0,
                pathCountOutput: ref Count,
                paths: null);
            if (Count > 0)
            {
                List<XrPath> pathlist = CreateList<XrPath>(Count, OpenXRHelper.XR_NULL_PATH);
                inputPaths = pathlist.ToArray();
                XrResult result = xrEnumeratePathsForInteractionProfileHTC(
                    m_XrInstance,
                    ref enumerateInfo,
                    pathCapacityInput: (UInt32)(inputPaths.Length & 0x7FFFFFFF),
                    pathCountOutput: ref Count,
                    paths: inputPaths);
                UnityEngine.Debug.Log("Get inputpath str : "+PathToString(inputPaths[0]));
                return true;
            }
            else
            {
                inputPaths = null;
                return false;
            }
            

        }
        public string xrPathToString(ulong path)
        {
            return PathToString(path);
        }

        public ulong xrStringToPath(string str)
        {
            return StringToPath(str);
        }
        #endregion
    }
}
