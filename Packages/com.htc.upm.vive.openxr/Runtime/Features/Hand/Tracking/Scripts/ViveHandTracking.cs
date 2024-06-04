// Copyright HTC Corporation All Rights Reserved.

using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using UnityEngine.XR;
using System.Collections.Generic;
using AOT;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace VIVE.OpenXR.Hand
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "VIVE XR Hand Tracking",
        BuildTargetGroups = new[] { BuildTargetGroup.Android , BuildTargetGroup.Standalone },
        Company = "HTC",
        Desc = "Support the Hand Tracking extension.",
        DocumentationLink = "..\\Documentation",
        OpenxrExtensionStrings = kOpenxrExtensionString,
        Version = "4.0.0",
        FeatureId = featureId)]
#endif
    public class ViveHandTracking : OpenXRFeature
    {
        const string LOG_TAG = "VIVE.OpenXR.Hand.ViveHandTracking";
        void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
        void WARNING(string msg) { Debug.LogWarning(LOG_TAG + " " + msg); }
        void ERROR(string msg) { Debug.LogError(LOG_TAG + " " + msg); }

        /// <summary>
        /// OpenXR specification <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_hand_tracking">12.29 XR_EXT_hand_tracking</see>.
        /// </summary>
        public const string kOpenxrExtensionString = "XR_EXT_hand_tracking";
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "vive.openxr.feature.hand.tracking";

        #region OpenXR Life Cycle
        private bool m_XrInstanceCreated = false;
        private XrInstance m_XrInstance = 0;
        private static IntPtr xrGetInstanceProcAddr_prev;
        private static IntPtr WaitFrame_prev;
        private static XrFrameWaitInfo m_frameWaitInfo;
        private static XrFrameState m_frameState;
        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            UnityEngine.Debug.Log("EXT: registering our own xrGetInstanceProcAddr");
            xrGetInstanceProcAddr_prev = func;
            return Marshal.GetFunctionPointerForDelegate(m_intercept_xrWaitFrame_xrGetInstanceProcAddr);
        }
        [MonoPInvokeCallback(typeof(OpenXRHelper.xrGetInstanceProcAddrDelegate))]
        private static XrResult intercept_xrWaitFrame_xrGetInstanceProcAddr(XrInstance instance, string name, out IntPtr function)
        {
            if (xrGetInstanceProcAddr_prev == null || xrGetInstanceProcAddr_prev == IntPtr.Zero)
            {
                UnityEngine.Debug.LogError("xrGetInstanceProcAddr_prev is null");
                function = IntPtr.Zero;
                return XrResult.XR_ERROR_VALIDATION_FAILURE;
            }

            // Get delegate of old xrGetInstanceProcAddr.
            var xrGetProc = Marshal.GetDelegateForFunctionPointer<OpenXRHelper.xrGetInstanceProcAddrDelegate>(xrGetInstanceProcAddr_prev);
            XrResult result = xrGetProc(instance, name, out function);
            if (name == "xrWaitFrame")
            {
                WaitFrame_prev = function;
                m_intercept_xrWaitFrame = intercepted_xrWaitFrame;
                function = Marshal.GetFunctionPointerForDelegate(m_intercept_xrWaitFrame); ;
                UnityEngine.Debug.Log("Getting xrWaitFrame func");
            }

            return result;

        }
        [MonoPInvokeCallback(typeof(OpenXRHelper.xrWaitFrameDelegate))]
        private static int intercepted_xrWaitFrame(ulong session, ref XrFrameWaitInfo frameWaitInfo, ref XrFrameState frameState)
        {
            // Get delegate of prev xrWaitFrame.
            var xrWaitFrame = Marshal.GetDelegateForFunctionPointer<OpenXRHelper.xrWaitFrameDelegate>(WaitFrame_prev);
            int res = xrWaitFrame(session, ref frameWaitInfo, ref frameState);
            m_frameWaitInfo = frameWaitInfo;
            m_frameState = frameState;
            return res;
        }

        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateInstance">xrCreateInstance</see> is done.
        /// </summary>
        /// <param name="xrInstance">The created instance.</param>
        /// <returns>True for valid <see cref="XrInstance">XrInstance</see></returns>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(kOpenxrExtensionString))
            {
                WARNING("OnInstanceCreate() " + kOpenxrExtensionString + " is NOT enabled.");
                return false;
            }

            m_XrInstanceCreated = true;
            m_XrInstance = xrInstance;
            DEBUG("OnInstanceCreate() " + m_XrInstance);

            return GetXrFunctionDelegates(m_XrInstance);
        }
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroyInstance">xrDestroyInstance</see> is done.
        /// </summary>
        /// <param name="xrInstance">The instance to destroy.</param>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            m_XrInstanceCreated = false;
            m_XrInstance = 0;
            DEBUG("OnInstanceDestroy() " + xrInstance);
        }

        private XrSystemId m_XrSystemId = 0;
        /// <summary>
        /// Called when the <see cref="XrSystemId">XrSystemId</see> retrieved by <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrGetSystem">xrGetSystem</see> is changed.
        /// </summary>
        /// <param name="xrSystem">The system id.</param>
        protected override void OnSystemChange(ulong xrSystem)
        {
            m_XrSystemId = xrSystem;
            DEBUG("OnSystemChange() " + m_XrSystemId);
        }

        private bool m_XrSessionCreated = false;
        private XrSession m_XrSession = 0;
        private bool hasReferenceSpaceLocal = false, hasReferenceSpaceStage = false;
        private XrSpace m_ReferenceSpaceLocal = 0, m_ReferenceSpaceStage = 0;

        private bool hasLeftHandTracker = false, hasRightHandTracker = false;
        private XrHandTrackerEXT leftHandTracker = 0, rightHandTracker = 0;
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession">xrCreateSession</see> is done.
        /// </summary>
        /// <param name="xrSession">The created session ID.</param>
        protected override void OnSessionCreate(ulong xrSession)
        {
            m_XrSession = xrSession;
            m_XrSessionCreated = true;
            DEBUG("OnSessionCreate() " + m_XrSession);

            // Enumerate supported reference space types and create the XrSpace.
            XrReferenceSpaceType[] spaces = new XrReferenceSpaceType[Enum.GetNames(typeof(XrReferenceSpaceType)).Count()];
            UInt32 spaceCountOutput;
#pragma warning disable 0618
            if (EnumerateReferenceSpaces(
                spaceCapacityInput: 0,
                spaceCountOutput: out spaceCountOutput,
                spaces: out spaces[0]) == XrResult.XR_SUCCESS)
#pragma warning restore 0618
            {
                DEBUG("OnSessionCreate() spaceCountOutput: " + spaceCountOutput);

                Array.Resize(ref spaces, (int)spaceCountOutput);
#pragma warning disable 0618
                if (EnumerateReferenceSpaces(
                    spaceCapacityInput: spaceCountOutput,
                    spaceCountOutput: out spaceCountOutput,
                    spaces: out spaces[0]) == XrResult.XR_SUCCESS)
#pragma warning restore 0618
                {
                    XrReferenceSpaceCreateInfo createInfo;

                    /// Create m_ReferenceSpaceLocal
                    if (IsReferenceSpaceTypeSupported(spaceCountOutput, spaces, XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_LOCAL))
                    {
                        createInfo.type = XrStructureType.XR_TYPE_REFERENCE_SPACE_CREATE_INFO;
                        createInfo.next = IntPtr.Zero;
                        createInfo.referenceSpaceType = XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_LOCAL;//referenceSpaceType;
                        createInfo.poseInReferenceSpace.orientation = new XrQuaternionf(0, 0, 0, 1);
                        createInfo.poseInReferenceSpace.position = new XrVector3f(0, 0, 0);

#pragma warning disable 0618
                        if (CreateReferenceSpace(
                            createInfo: ref createInfo,
                            space: out m_ReferenceSpaceLocal) == XrResult.XR_SUCCESS)
#pragma warning restore 0618
                        {
                            hasReferenceSpaceLocal = true;
                            DEBUG("OnSessionCreate() CreateReferenceSpace LOCAL: " + m_ReferenceSpaceLocal);
                        }
                        else
                        {
                            ERROR("OnSessionCreate() CreateReferenceSpace LOCAL failed.");
                        }
                    }

                    /// Create m_ReferenceSpaceStage
                    if (IsReferenceSpaceTypeSupported(spaceCountOutput, spaces, XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_STAGE))
                    {
                        createInfo.type = XrStructureType.XR_TYPE_REFERENCE_SPACE_CREATE_INFO;
                        createInfo.next = IntPtr.Zero;
                        createInfo.referenceSpaceType = XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_STAGE;
                        createInfo.poseInReferenceSpace.orientation = new XrQuaternionf(0, 0, 0, 1);
                        createInfo.poseInReferenceSpace.position = new XrVector3f(0, 0, 0);

#pragma warning disable 0618
                        if (CreateReferenceSpace(
                            createInfo: ref createInfo,
                            space: out m_ReferenceSpaceStage) == XrResult.XR_SUCCESS)
#pragma warning restore 0618
                        {
                            hasReferenceSpaceStage = true;
                            DEBUG("OnSessionCreate() CreateReferenceSpace STAGE: " + m_ReferenceSpaceStage);
                        }
                        else
                        {
                            ERROR("OnSessionCreate() CreateReferenceSpace STAGE failed.");
                        }
                    }
                }
                else
                {
                    ERROR("OnSessionCreate() EnumerateReferenceSpaces(" + spaceCountOutput + ") failed.");
                }
            }
            else
            {
                ERROR("OnSessionCreate() EnumerateReferenceSpaces(0) failed.");
            }

            { // left hand tracker
                if (CreateHandTrackers(true, out XrHandTrackerEXT value))
                {
                    hasLeftHandTracker = true;
                    leftHandTracker = value;
                    DEBUG("OnSessionCreate() leftHandTracker " + leftHandTracker);
                }
            }
            { // right hand tracker
                if (CreateHandTrackers(false, out XrHandTrackerEXT value))
                {
                    hasRightHandTracker = true;
                    rightHandTracker = value;
                    DEBUG("OnSessionCreate() rightHandTracker " + rightHandTracker);
                }
            }
        }

        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrDestroySession">xrDestroySession</see> is done.
        /// </summary>
        /// <param name="xrSession">The session ID to destroy.</param>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            DEBUG("OnSessionDestroy() " + xrSession);

            // Reference Space is binding with xrSession so we destroy the xrSpace when xrSession is destroyed.
            if (hasReferenceSpaceLocal)
            {
#pragma warning disable 0618
                if (DestroySpace(m_ReferenceSpaceLocal) == XrResult.XR_SUCCESS)
#pragma warning restore 0618
                {
                    DEBUG("OnSessionDestroy() DestroySpace LOCAL " + m_ReferenceSpaceLocal);
                    m_ReferenceSpaceLocal = 0;
                }
                else
                {
                    ERROR("OnSessionDestroy() DestroySpace LOCAL " + m_ReferenceSpaceLocal + " failed.");
                }
                hasReferenceSpaceLocal = false;
            }
            if (hasReferenceSpaceStage)
            {
#pragma warning disable 0618
                if (DestroySpace(m_ReferenceSpaceStage) == XrResult.XR_SUCCESS)
#pragma warning restore 0618
                {
                    DEBUG("OnSessionDestroy() DestroySpace STAGE " + m_ReferenceSpaceStage);
                    m_ReferenceSpaceStage = 0;
                }
                else
                {
                    ERROR("OnSessionDestroy() DestroySpace STAGE " + m_ReferenceSpaceStage + " failed.");
                }
                hasReferenceSpaceStage = false;
            }

            // Hand Tracking is binding with xrSession so we destroy the hand trackers when xrSession is destroyed.
            if (hasLeftHandTracker)
            {
                if (DestroyHandTrackerEXT(leftHandTracker) == XrResult.XR_SUCCESS)
                {
                    DEBUG("OnSessionDestroy() Left DestroyHandTrackerEXT " + leftHandTracker);
                }
                else
                {
                    ERROR("OnSessionDestroy() Left DestroyHandTrackerEXT " + leftHandTracker + " failed.");
                }
                hasLeftHandTracker = false;
            }
            if (hasRightHandTracker)
            {
                if (DestroyHandTrackerEXT(rightHandTracker) == XrResult.XR_SUCCESS)
                {
                    DEBUG("OnSessionDestroy() Right DestroyHandTrackerEXT " + rightHandTracker);
                }
                else
                {
                    ERROR("OnSessionDestroy() Right DestroyHandTrackerEXT " + rightHandTracker + " failed.");
                }
                hasRightHandTracker = false;
            }

            if (m_XrSession == xrSession)
            {
                m_XrSession = 0;
                m_XrSessionCreated = false;
            }
        }
        #endregion

        #region OpenXR function delegates
        private static readonly OpenXRHelper.xrGetInstanceProcAddrDelegate m_intercept_xrWaitFrame_xrGetInstanceProcAddr
            = new OpenXRHelper.xrGetInstanceProcAddrDelegate(intercept_xrWaitFrame_xrGetInstanceProcAddr);
        private static OpenXRHelper.xrWaitFrameDelegate m_intercept_xrWaitFrame;
        /// xrGetInstanceProcAddr
        OpenXRHelper.xrGetInstanceProcAddrDelegate XrGetInstanceProcAddr;

        /// xrGetSystemProperties
        OpenXRHelper.xrGetSystemPropertiesDelegate xrGetSystemProperties;
        /// <summary>
        /// An application can call GetSystemProperties to retrieve information about the system such as vendor ID, system name, and graphics and tracking properties.
        /// </summary>
        /// <param name="properties">Points to an instance of the XrSystemProperties structure, that will be filled with returned information.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        [Obsolete("This function will become private in next release")]
        public XrResult GetSystemProperties(ref XrSystemProperties properties)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("GetSystemProperties() XR_ERROR_SESSION_LOST.");
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("GetSystemProperties() XR_ERROR_INSTANCE_LOST.");
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrGetSystemProperties(m_XrInstance, m_XrSystemId, ref properties);
        }

        /// xrEnumerateReferenceSpaces
        OpenXRHelper.xrEnumerateReferenceSpacesDelegate xrEnumerateReferenceSpaces;
        /// <summary>
        /// Enumerates the set of reference space types that this runtime supports for a given session. Runtimes must always return identical buffer contents from this enumeration for the lifetime of the session.
        /// </summary>
        /// <param name="spaceCapacityInput">The capacity of the spaces array, or 0 to indicate a request to retrieve the required capacity.</param>
        /// <param name="spaceCountOutput">A pointer to the count of spaces written, or a pointer to the required capacity in the case that spaceCapacityInput is insufficient.</param>
        /// <param name="spaces">A pointer to an application-allocated array that will be filled with the enumerant of each supported reference space. It can be NULL if spaceCapacityInput is 0.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        [Obsolete("This function will become private in next release")]
        public XrResult EnumerateReferenceSpaces(UInt32 spaceCapacityInput, out UInt32 spaceCountOutput, out XrReferenceSpaceType spaces)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("EnumerateReferenceSpaces() XR_ERROR_SESSION_LOST.");
                spaceCountOutput = 0;
                spaces = XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_UNBOUNDED_MSFT;
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("EnumerateReferenceSpaces() XR_ERROR_SESSION_LOST.");
                spaceCountOutput = 0;
                spaces = XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_UNBOUNDED_MSFT;
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrEnumerateReferenceSpaces(m_XrSession, spaceCapacityInput, out spaceCountOutput, out spaces);
        }

        /// xrCreateReferenceSpace
        OpenXRHelper.xrCreateReferenceSpaceDelegate xrCreateReferenceSpace;
        /// <summary>
        /// Creates an <see cref="XrSpace">XrSpace</see> handle based on a chosen reference space. Application can provide an <see cref="XrPosef">XrPosef</see> to define the position and orientation of the new space’s origin within the natural reference frame of the reference space.
        /// </summary>
        /// <param name="createInfo">The XrReferenceSpaceCreateInfo used to specify the space.</param>
        /// <param name="space">The returned XrSpace handle.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        [Obsolete("This function will become private in next release")]
        public XrResult CreateReferenceSpace(ref XrReferenceSpaceCreateInfo createInfo, out XrSpace space)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("CreateReferenceSpace() XR_ERROR_SESSION_LOST.");
                space = 0;
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("CreateReferenceSpace() XR_ERROR_INSTANCE_LOST.");
                space = 0;
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrCreateReferenceSpace(m_XrSession, ref createInfo, out space);
        }

        /// xrDestroySpace
        OpenXRHelper.xrDestroySpaceDelegate xrDestroySpace;
        /// <summary>
        /// <see cref="XrSpace">XrSpace</see> handles are destroyed using DestroySpace. The runtime may still use this space if there are active dependencies (e.g, compositions in progress).
        /// </summary>
        /// <param name="space">Must be a valid XrSpace handle.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        [Obsolete("This function will become private in next release")]
        public XrResult DestroySpace(XrSpace space)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("DestroySpace() XR_ERROR_SESSION_LOST.");
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("DestroySpace() XR_ERROR_INSTANCE_LOST.");
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrDestroySpace(space);
        }

        /// xrCreateHandTrackerEXT
        ViveHandTrackingHelper.xrCreateHandTrackerEXTDelegate xrCreateHandTrackerEXT;
        /// <summary>
        /// An application can create an <see cref="XrHandTrackerEXT">XrHandTrackerEXT</see> handle using CreateHandTrackerEXT function.
        /// </summary>
        /// <param name="createInfo">The XrHandTrackerCreateInfoEXT used to specify the hand tracker.</param>
        /// <param name="handTracker">The returned XrHandTrackerEXT handle.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public XrResult CreateHandTrackerEXT(ref XrHandTrackerCreateInfoEXT createInfo, out XrHandTrackerEXT handTracker)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("CreateHandTrackerEXT() XR_ERROR_SESSION_LOST.");
                handTracker = 0;
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("CreateHandTrackerEXT() XR_ERROR_INSTANCE_LOST.");
                handTracker = 0;
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            if (createInfo.hand == XrHandEXT.XR_HAND_LEFT_EXT && hasLeftHandTracker)
            {
                DEBUG("CreateHandTrackerEXT() Left tracker " + leftHandTracker + " already created.");
                handTracker = leftHandTracker;
                return XrResult.XR_SUCCESS;
            }
            if (createInfo.hand == XrHandEXT.XR_HAND_RIGHT_EXT && hasRightHandTracker)
            {
                DEBUG("CreateHandTrackerEXT() Right tracker " + rightHandTracker + " already created.");
                handTracker = rightHandTracker;
                return XrResult.XR_SUCCESS;
            }

            return xrCreateHandTrackerEXT(m_XrSession, ref createInfo, out handTracker);
        }

        /// xrDestroyHandTrackerEXT
        ViveHandTrackingHelper.xrDestroyHandTrackerEXTDelegate xrDestroyHandTrackerEXT;
        /// <summary>
        /// Releases the handTracker and the underlying resources when finished with hand tracking experiences.
        /// </summary>
        /// <param name="handTracker">An XrHandTrackerEXT previously created by <see cref="CreateHandTrackerEXT">CreateHandTrackerEXT</see>.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public XrResult DestroyHandTrackerEXT(XrHandTrackerEXT handTracker)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("DestroyHandTrackerEXT() XR_ERROR_SESSION_LOST.");
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("DestroyHandTrackerEXT() XR_ERROR_INSTANCE_LOST.");
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrDestroyHandTrackerEXT(handTracker);
        }

        /// xrLocateHandJointsEXT
        ViveHandTrackingHelper.xrLocateHandJointsEXTDelegate xrLocateHandJointsEXT;
        /// <summary>
        /// The LocateHandJointsEXT function locates an array of hand joints to a base space at given time.
        /// </summary>
        /// <param name="handTracker">An <see cref="XrHandTrackerEXT">XrHandTrackerEXT</see> previously created by <see cref="ViveHandTracking.CreateHandTrackerEXT(ref XrHandTrackerCreateInfoEXT, out XrHandTrackerEXT)">CreateHandTrackerEXT</see>.</param>
        /// <param name="locateInfo">A pointer to <see cref="XrHandJointsLocateInfoEXT">XrHandJointsLocateInfoEXT</see> describing information to locate hand joints.</param>
        /// <param name="locations">A pointer to <see cref="XrHandJointLocationsEXT">XrHandJointLocationsEXT</see> receiving the returned hand joint locations.</param>
        /// <returns></returns>
        public XrResult LocateHandJointsEXT(XrHandTrackerEXT handTracker, XrHandJointsLocateInfoEXT locateInfo, ref XrHandJointLocationsEXT locations)
        {
            if (!m_XrSessionCreated)
            {
                ERROR("LocateHandJointsEXT() XR_ERROR_SESSION_LOST.");
                return XrResult.XR_ERROR_SESSION_LOST;
            }
            if (!m_XrInstanceCreated)
            {
                ERROR("LocateHandJointsEXT() XR_ERROR_INSTANCE_LOST.");
                return XrResult.XR_ERROR_INSTANCE_LOST;
            }

            return xrLocateHandJointsEXT(handTracker, locateInfo, ref locations);
        }

        private bool GetXrFunctionDelegates(XrInstance xrInstance)
        {
            /// xrGetInstanceProcAddr
            if (xrGetInstanceProcAddr != null && xrGetInstanceProcAddr != IntPtr.Zero)
            {
                DEBUG("Get function pointer of xrGetInstanceProcAddr.");
                XrGetInstanceProcAddr = Marshal.GetDelegateForFunctionPointer(
                    xrGetInstanceProcAddr,
                    typeof(OpenXRHelper.xrGetInstanceProcAddrDelegate)) as OpenXRHelper.xrGetInstanceProcAddrDelegate;
            }
            else
            {
                ERROR("xrGetInstanceProcAddr");
                return false;
            }

            IntPtr funcPtr = IntPtr.Zero;
            /// xrGetSystemProperties
            if (XrGetInstanceProcAddr(xrInstance, "xrGetSystemProperties", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrGetSystemProperties.");
                    xrGetSystemProperties = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(OpenXRHelper.xrGetSystemPropertiesDelegate)) as OpenXRHelper.xrGetSystemPropertiesDelegate;
                }
            }
            else
            {
                ERROR("xrGetSystemProperties");
                return false;
            }
            /// xrEnumerateReferenceSpaces
            if (XrGetInstanceProcAddr(xrInstance, "xrEnumerateReferenceSpaces", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrEnumerateReferenceSpaces.");
                    xrEnumerateReferenceSpaces = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(OpenXRHelper.xrEnumerateReferenceSpacesDelegate)) as OpenXRHelper.xrEnumerateReferenceSpacesDelegate;
                }
            }
            else
            {
                ERROR("xrEnumerateReferenceSpaces");
                return false;
            }
            /// xrCreateReferenceSpace
            if (XrGetInstanceProcAddr(xrInstance, "xrCreateReferenceSpace", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrCreateReferenceSpace.");
                    xrCreateReferenceSpace = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(OpenXRHelper.xrCreateReferenceSpaceDelegate)) as OpenXRHelper.xrCreateReferenceSpaceDelegate;
                }
            }
            else
            {
                ERROR("xrCreateReferenceSpace");
                return false;
            }
            /// xrDestroySpace
            if (XrGetInstanceProcAddr(xrInstance, "xrDestroySpace", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrDestroySpace.");
                    xrDestroySpace = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(OpenXRHelper.xrDestroySpaceDelegate)) as OpenXRHelper.xrDestroySpaceDelegate;
                }
            }
            else
            {
                ERROR("xrDestroySpace");
                return false;
            }

            /// xrCreateHandTrackerEXT
            if (XrGetInstanceProcAddr(xrInstance, "xrCreateHandTrackerEXT", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrCreateHandTrackerEXT.");
                    xrCreateHandTrackerEXT = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(ViveHandTrackingHelper.xrCreateHandTrackerEXTDelegate)) as ViveHandTrackingHelper.xrCreateHandTrackerEXTDelegate;
                }
            }
            else
            {
                ERROR("xrCreateHandTrackerEXT");
                return false;
            }
            /// xrDestroyHandTrackerEXT
            if (XrGetInstanceProcAddr(xrInstance, "xrDestroyHandTrackerEXT", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrDestroyHandTrackerEXT.");
                    xrDestroyHandTrackerEXT = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(ViveHandTrackingHelper.xrDestroyHandTrackerEXTDelegate)) as ViveHandTrackingHelper.xrDestroyHandTrackerEXTDelegate;
                }
            }
            else
            {
                ERROR("xrDestroyHandTrackerEXT");
                return false;
            }
            /// xrLocateHandJointsEXT
            if (XrGetInstanceProcAddr(xrInstance, "xrLocateHandJointsEXT", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    DEBUG("Get function pointer of xrLocateHandJointsEXT.");
                    xrLocateHandJointsEXT = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(ViveHandTrackingHelper.xrLocateHandJointsEXTDelegate)) as ViveHandTrackingHelper.xrLocateHandJointsEXTDelegate;
                }
            }
            else
            {
                ERROR("xrLocateHandJointsEXT");
                return false;
            }

            return true;
        }
        #endregion

        static List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();
        /// <summary>
        /// Retrieves the current tracking origin in Unity XR.
        /// </summary>
        /// <returns>The tracking origin in <see href="https://docs.unity3d.com/ScriptReference/XR.TrackingOriginModeFlags.html">TrackingOriginModeFlags</see></returns>
        public TrackingOriginModeFlags GetTrackingOriginMode()
        {
            XRInputSubsystem subsystem = null;

            SubsystemManager.GetSubsystems(s_InputSubsystems);
            if (s_InputSubsystems.Count > 0)
            {
                subsystem = s_InputSubsystems[0];
            }

            if (subsystem != null)
            {
                return subsystem.GetTrackingOriginMode();
            }

            return TrackingOriginModeFlags.Unknown;
        }
        private bool IsReferenceSpaceTypeSupported(UInt32 spaceCountOutput, XrReferenceSpaceType[] spaces, XrReferenceSpaceType space)
        {
            bool support = false;
            for (int i = 0; i < spaceCountOutput; i++)
            {
                DEBUG("IsReferenceSpaceTypeSupported() supported space[" + i + "]: " + spaces[i]);
                if (spaces[i] == space) { support = true; }
            }

            return support;
        }

        XrSystemHandTrackingPropertiesEXT handTrackingSystemProperties;
        XrSystemProperties systemProperties;
        private bool IsHandTrackingSupported()
        {
            bool ret = false;
            if (!m_XrSessionCreated)
            {
                ERROR("IsHandTrackingSupported() session is not created.");
                return ret;
            }

            handTrackingSystemProperties.type = XrStructureType.XR_TYPE_SYSTEM_HAND_TRACKING_PROPERTIES_EXT;
            systemProperties.type = XrStructureType.XR_TYPE_SYSTEM_PROPERTIES;
            systemProperties.next = Marshal.AllocHGlobal(Marshal.SizeOf(handTrackingSystemProperties));

            long offset = 0;
            if (IntPtr.Size == 4)
                offset = systemProperties.next.ToInt32();
            else
                offset = systemProperties.next.ToInt64();

            IntPtr sys_hand_tracking_prop_ptr = new IntPtr(offset);
            Marshal.StructureToPtr(handTrackingSystemProperties, sys_hand_tracking_prop_ptr, false);

#pragma warning disable 0618
            if (GetSystemProperties(ref systemProperties) == XrResult.XR_SUCCESS)
#pragma warning restore 0618
            {
                if (IntPtr.Size == 4)
                    offset = systemProperties.next.ToInt32();
                else
                    offset = systemProperties.next.ToInt64();

                sys_hand_tracking_prop_ptr = new IntPtr(offset);
                handTrackingSystemProperties = (XrSystemHandTrackingPropertiesEXT)Marshal.PtrToStructure(sys_hand_tracking_prop_ptr, typeof(XrSystemHandTrackingPropertiesEXT));

                DEBUG("IsHandTrackingSupported() XrSystemHandTrackingPropertiesEXT.supportsHandTracking: " + handTrackingSystemProperties.supportsHandTracking);
                ret = handTrackingSystemProperties.supportsHandTracking > 0;
            }
            else
            {
                ERROR("IsHandTrackingSupported() GetSystemProperties failed.");
            }

            Marshal.FreeHGlobal(systemProperties.next);
            return ret;
        }

        private bool CreateHandTrackers(bool isLeft, out XrHandTrackerEXT handTracker)
        {
            if (!IsHandTrackingSupported())
            {
                ERROR("CreateHandTrackers() " + (isLeft ? "Left" : "Right") + " hand tracking is NOT supported.");
                handTracker = 0;
                return false;
            }

            XrHandTrackerCreateInfoEXT createInfo;
            createInfo.type = XrStructureType.XR_TYPE_HAND_TRACKER_CREATE_INFO_EXT;
            createInfo.next = IntPtr.Zero;
            createInfo.hand = isLeft ? XrHandEXT.XR_HAND_LEFT_EXT : XrHandEXT.XR_HAND_RIGHT_EXT;
            createInfo.handJointSet = XrHandJointSetEXT.XR_HAND_JOINT_SET_DEFAULT_EXT;

            var ret = CreateHandTrackerEXT(ref createInfo, out handTracker);
            DEBUG("CreateHandTrackers() " + (isLeft ? "Left" : "Right") + " CreateHandTrackerEXT = " + ret);

            return ret == XrResult.XR_SUCCESS;
        }

        private XrHandJointLocationEXT[] jointLocationsL = new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
        private XrHandJointLocationEXT[] jointLocationsR = new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
        private XrHandJointLocationsEXT locations = new XrHandJointLocationsEXT(XrStructureType.XR_TYPE_HAND_JOINT_LOCATIONS_EXT, IntPtr.Zero, false, 0, IntPtr.Zero);

        public bool GetHandTrackingSpace(out XrSpace space)
        {
            space = 0;

            TrackingOriginModeFlags origin = GetTrackingOriginMode();
            if (origin == TrackingOriginModeFlags.Unknown || origin == TrackingOriginModeFlags.Unbounded) { return false; }

            space = (origin == TrackingOriginModeFlags.Device ? m_ReferenceSpaceLocal : m_ReferenceSpaceStage);
            return true;
        }

        /// <summary>
        /// Retrieves the <see cref="XrHandJointLocationEXT"> XrHandJointLocationEXT </see> data.
        /// </summary>
        /// <param name="isLeft">Left or right hand.</param>
        /// <param name="handJointLocation">Output parameter to retrieve <see cref="XrHandJointLocationEXT"> XrHandJointLocationEXT </see> data.</param>
        /// <returns>True for valid data.</returns>
        public bool GetJointLocations(bool isLeft, out XrHandJointLocationEXT[] handJointLocation)
        {
            bool ret = false;
            handJointLocation = isLeft ? jointLocationsL : jointLocationsR;

            if (isLeft && !hasLeftHandTracker) { return ret; }
            if (!isLeft && !hasRightHandTracker) { return ret; }

            TrackingOriginModeFlags origin = GetTrackingOriginMode();
            if (origin == TrackingOriginModeFlags.Unknown || origin == TrackingOriginModeFlags.Unbounded) { return ret; }
            XrSpace baseSpace = (origin == TrackingOriginModeFlags.Device ? m_ReferenceSpaceLocal : m_ReferenceSpaceStage);

            /// Configures XrHandJointsLocateInfoEXT
            XrHandJointsLocateInfoEXT locateInfo = new XrHandJointsLocateInfoEXT(
                in_type: XrStructureType.XR_TYPE_HAND_JOINTS_LOCATE_INFO_EXT,
                in_next: IntPtr.Zero,
                in_baseSpace: baseSpace,
                in_time: m_frameState.predictedDisplayTime);

            /// Configures XrHandJointLocationsEXT
            locations.type = XrStructureType.XR_TYPE_HAND_JOINT_LOCATIONS_EXT;
            locations.next = IntPtr.Zero;
            locations.isActive = false;
            locations.jointCount = (uint)(isLeft ? jointLocationsL.Length : jointLocationsR.Length);

            XrHandJointLocationEXT joint_location_ext_type = default(XrHandJointLocationEXT);
            int jointLocationsLength = isLeft ? jointLocationsL.Length : jointLocationsR.Length;
            locations.jointLocations = Marshal.AllocHGlobal(Marshal.SizeOf(joint_location_ext_type) * jointLocationsLength);

            long offset = 0;
            /*if (IntPtr.Size == 4)
                offset = locations.jointLocations.ToInt32();
            else
                offset = locations.jointLocations.ToInt64();

            for (int i = 0; i < jointLocationsLength; i++)
            {
                IntPtr joint_location_ext_ptr = new IntPtr(offset);

                if (isLeft)
                    Marshal.StructureToPtr(jointLocationsL[i], joint_location_ext_ptr, false);
                else
                    Marshal.StructureToPtr(jointLocationsR[i], joint_location_ext_ptr, false);

                offset += Marshal.SizeOf(joint_location_ext_type);
            }*/

            if (LocateHandJointsEXT(
                handTracker: (isLeft ? leftHandTracker : rightHandTracker),
                locateInfo: locateInfo,
                locations: ref locations) == XrResult.XR_SUCCESS)
            {
                if (locations.isActive)
                {
                    if (IntPtr.Size == 4)
                        offset = locations.jointLocations.ToInt32();
                    else
                        offset = locations.jointLocations.ToInt64();

                    for (int i = 0; i < locations.jointCount; i++)
                    {
                        IntPtr joint_location_ext_ptr = new IntPtr(offset);

                        if (isLeft)
                            jointLocationsL[i] = (XrHandJointLocationEXT)Marshal.PtrToStructure(joint_location_ext_ptr, typeof(XrHandJointLocationEXT));
                        else
                            jointLocationsR[i] = (XrHandJointLocationEXT)Marshal.PtrToStructure(joint_location_ext_ptr, typeof(XrHandJointLocationEXT));

                        offset += Marshal.SizeOf(joint_location_ext_type);
                    }

                    // ToDo: locationFlags?
                    handJointLocation = isLeft ? jointLocationsL : jointLocationsR;

                    ret = true;
                }
            }

            Marshal.FreeHGlobal(locations.jointLocations);
            return ret;
        }
    }
}
