// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Input;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
#endif

#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
using PoseControl = UnityEngine.InputSystem.XR.PoseControl;
#else
using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;
#endif

namespace VIVE.OpenXR.Tracker
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of tracker interaction profiles in OpenXR. It enables XR_HTC_vive_xr_tracker_interaction in the underyling runtime.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "VIVE XR Tracker (Beta)",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "HTC",
        Desc = "Support for enabling the vive xr tracker interaction profile. Will register the controller map for xr tracker if enabled.",
        DocumentationLink = "..\\Documentation",
        Version = "1.0.6",
        OpenxrExtensionStrings = kOpenxrExtensionString,
        Category = FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class ViveXRTracker : OpenXRInteractionFeature
    {
        const string LOG_TAG = "VIVE.OpenXR.Tracker.ViveXRTracker ";
        static StringBuilder m_sb = null;
        static StringBuilder sb {
            get {
                if (m_sb == null) { m_sb = new StringBuilder(); }
                return m_sb;
            }
        }
        static void DEBUG(StringBuilder msg) { Debug.Log(msg); }
        static void WARNING(StringBuilder msg) { Debug.LogWarning(msg); }
        static void ERROR(StringBuilder msg) { Debug.LogError(msg); }

        private static ViveXRTracker m_Instance = null;

        /// <summary>
        /// OpenXR specification.
        /// </summary>
        public const string kOpenxrExtensionString = "XR_HTC_vive_xr_tracker_interaction";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "vive.wave.openxr.feature.xrtracker";

        #region Tracker Product Name
        public const string kProductUltimateTracker = "VIVE Ultimate Tracker";
        public const string kProductUltimateTracker0 = "VIVE Ultimate Tracker 0";
        public const string kProductUltimateTracker1 = "VIVE Ultimate Tracker 1";
        public const string kProductUltimateTracker2 = "VIVE Ultimate Tracker 2";
        public const string kProductUltimateTracker3 = "VIVE Ultimate Tracker 3";
        public const string kProductUltimateTracker4 = "VIVE Ultimate Tracker 4";
        const string kProductTrackingTag = "VIVE Tracking Tag";
        private const string kProducts = "^(" + kProductUltimateTracker
            + ")|^(" + kProductUltimateTracker0 + ")|^(" + kProductUltimateTracker1 + ")|^(" + kProductUltimateTracker2 + ")|^(" + kProductUltimateTracker3 + ")|^(" + kProductUltimateTracker4
            + ")|^(" + kProductTrackingTag + ")";
        private readonly string[] s_UltimateTrackerProduct = { kProductUltimateTracker0, kProductUltimateTracker1, kProductUltimateTracker2, kProductUltimateTracker3, kProductUltimateTracker4 };
        private bool IsUltimateTracker(string product)
        {
            for (int i = 0; i < s_UltimateTrackerProduct.Length; i++)
            {
                if (s_UltimateTrackerProduct[i].Equals(product))
                    return true;
            }
            return false;
        }
        #endregion

        #region Tracker Action Map Name
        const string kUltimateTrackerActionMap0 = "viveultimatetracker0";
        const string kUltimateTrackerActionMap1 = "viveultimatetracker1";
        const string kUltimateTrackerActionMap2 = "viveultimatetracker2";
        const string kUltimateTrackerActionMap3 = "viveultimatetracker3";
        const string kUltimateTrackerActionMap4 = "viveultimatetracker4";
        #endregion

        #region Tracker Usage
        const string kUltimateTrackerUsage0 = "Ultimate Tracker 0";
        const string kUltimateTrackerUsage1 = "Ultimate Tracker 1";
        const string kUltimateTrackerUsage2 = "Ultimate Tracker 2";
        const string kUltimateTrackerUsage3 = "Ultimate Tracker 3";
        const string kUltimateTrackerUsage4 = "Ultimate Tracker 4";
        #endregion

        #region Tracker User Path
        public const string kUltimateTrackerPath0 = "/user/xr_tracker_htc/vive_ultimate_tracker_0";
        public const string kUltimateTrackerPath1 = "/user/xr_tracker_htc/vive_ultimate_tracker_1";
        public const string kUltimateTrackerPath2 = "/user/xr_tracker_htc/vive_ultimate_tracker_2";
        public const string kUltimateTrackerPath3 = "/user/xr_tracker_htc/vive_ultimate_tracker_3";
        public const string kUltimateTrackerPath4 = "/user/xr_tracker_htc/vive_ultimate_tracker_4";
        #endregion

        #region Tracker Serial Number
        public const string kUltimateTrackerSN0 = "VIVE_Ultimate_Tracker_0";
        public const string kUltimateTrackerSN1 = "VIVE_Ultimate_Tracker_1";
        public const string kUltimateTrackerSN2 = "VIVE_Ultimate_Tracker_2";
        public const string kUltimateTrackerSN3 = "VIVE_Ultimate_Tracker_3";
        public const string kUltimateTrackerSN4 = "VIVE_Ultimate_Tracker_4";
        const string kTrackingTagSN0 = "VIVE_Tracking_Tag_0";
        const string kTrackingTagSN1 = "VIVE_Tracking_Tag_1";
        const string kTrackingTagSN2 = "VIVE_Tracking_Tag_2";
        const string kTrackingTagSN3 = "VIVE_Tracking_Tag_3";
        const string kTrackingTagSN4 = "VIVE_Tracking_Tag_4";
        const string k6DoFTrackerSN0 = "VIVE_6DoF_Tracker_a_0";
        const string k6DoFTrackerSN1 = "VIVE_6DoF_Tracker_a_1";
        #endregion

        #region Tracker Product Maps
        private Dictionary<string, string> m_UltimateTrackerActionMap = new Dictionary<string, string>() {
            { kProductUltimateTracker0, kUltimateTrackerActionMap0 },
            { kProductUltimateTracker1, kUltimateTrackerActionMap1 },
            { kProductUltimateTracker2, kUltimateTrackerActionMap2 },
            { kProductUltimateTracker3, kUltimateTrackerActionMap3 },
            { kProductUltimateTracker4, kUltimateTrackerActionMap4 },
        };
        /// <summary> Mapping from product to tracker usage. </summary>
        private static Dictionary<string, string> m_UltimateTrackerUsageMap = new Dictionary<string, string>() {
            { kProductUltimateTracker0, kUltimateTrackerUsage0 },
            { kProductUltimateTracker1, kUltimateTrackerUsage1 },
            { kProductUltimateTracker2, kUltimateTrackerUsage2 },
            { kProductUltimateTracker3, kUltimateTrackerUsage3 },
            { kProductUltimateTracker4, kUltimateTrackerUsage4 },
        };
        /// <summary> Mapping from product to user path. </summary>
        private Dictionary<string, string> m_UltimateTrackerPathMap = new Dictionary<string, string>() {
            { kProductUltimateTracker0, kUltimateTrackerPath0 },
            { kProductUltimateTracker1, kUltimateTrackerPath1 },
            { kProductUltimateTracker2, kUltimateTrackerPath2 },
            { kProductUltimateTracker3, kUltimateTrackerPath3 },
            { kProductUltimateTracker4, kUltimateTrackerPath4 },
        };
        /// <summary> Mapping from product to serial number. </summary>
        private Dictionary<string, string> m_UltimateTrackerSerialMap = new Dictionary<string, string>() {
            { kProductUltimateTracker0, kUltimateTrackerSN0 },
            { kProductUltimateTracker1, kUltimateTrackerSN1 },
            { kProductUltimateTracker2, kUltimateTrackerSN2 },
            { kProductUltimateTracker3, kUltimateTrackerSN3 },
            { kProductUltimateTracker4, kUltimateTrackerSN4 },
        };
        #endregion

        [Preserve, InputControlLayout(displayName = "VIVE XR Tracker (OpenXR)", commonUsages = new[] {
            kUltimateTrackerUsage0, kUltimateTrackerUsage1, kUltimateTrackerUsage2, kUltimateTrackerUsage3, kUltimateTrackerUsage4,
        }, isGenericTypeOfDevice = true)]
        public class XrTrackerDevice : OpenXRDevice//, IInputUpdateCallbackReceiver
        {
            #region Log
            const string LOG_TAG = "VIVE.OpenXR.Tracker.ViveXRTracker.XrTrackerDevice ";
            StringBuilder m_sb = null;
            StringBuilder sb {
                get {
                    if (m_sb == null) { m_sb = new StringBuilder(); }
                    return m_sb;
                }
            }
            void DEBUG(StringBuilder msg) { Debug.Log(msg); }
            #endregion

            #region Interactions
            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="entityPose"/> OpenXR binding. The entity pose represents the location of the tracker.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "entityPose" }, usage = "Device")]
            public PoseControl devicePose { get; private set; }

            /// <summary>
            /// A <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Controls.ButtonControl.html">ButtonControl</see> required for backwards compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping devicePose/isTracked.
            /// </summary>
            [Preserve, InputControl(offset = 0, usage = "IsTracked")]
            public ButtonControl isTracked { get; private set; }

            /// <summary>
            /// A <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Controls.IntegerControl.html">IntegerControl</see> required for backwards compatibility with the XRSDK layouts. This represents the bit flag set to indicate what data is valid. This value is equivalent to mapping devicePose/trackingState.
            /// </summary>
            [Preserve, InputControl(offset = 4, usage = "TrackingState")]
            public IntegerControl trackingState { get; private set; }

            /// <summary>
            /// A <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@0.1/api/UnityEngine.Experimental.Input.Controls.Vector3Control.html">Vector3Control</see> required for backwards compatibility with the XRSDK layouts. This is the device position. For the VIVE XR device, this is both the device and the pointer position. This value is equivalent to mapping devicePose/position.
            /// </summary>
            [Preserve, InputControl(offset = 8, alias = "gripPosition")]
            public Vector3Control devicePosition { get; private set; }

            /// <summary>
            /// A <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Controls.QuaternionControl.html">QuaternionControl</see> required for backwards compatibility with the XRSDK layouts. This is the device orientation. For the VIVE XR device, this is both the device and the pointer rotation. This value is equivalent to mapping devicePose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 20, alias = "gripOrientation")]
            public QuaternionControl deviceRotation { get; private set; }
            #endregion

#if DEBUG_CODE
            // Unity action binding path: <ViveXRTracker>{Tracker 0}/isTracked
            private InputAction inputActionIsTracked = null;
#endif

            /// <summary>
            /// Internal call used to assign controls to the the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                base.FinishSetup();

                devicePose = GetChildControl<PoseControl>("devicePose");
                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");

                sb.Clear().Append(LOG_TAG)
                    .Append("FinishSetup() device interfaceName: ").Append(description.interfaceName)
                    .Append(", deviceClass: ").Append(description.deviceClass)
                    .Append(", product: ").Append(description.product)
                    .Append(", serial: ").Append(description.serial)
                    .Append(", current profile: ").Append(profile);
                DEBUG(sb);

                if (m_UltimateTrackerUsageMap.ContainsKey(description.product))
                {
                    /// After RegisterActionMapsWithRuntime finished, each XrTrackerDevice will have a product name (e.g. kProductUltimateTracker0)
                    /// We have to assign the XrTrackerDeivce to a commonUsage (e.g. kUltimateTrackerUsage0)
                    /// 
                    /// Since we already established the m_UltimateTrackerUsageMap (kProductUltimateTracker0, kUltimateTrackerUsage0),
                    /// we can simply call SetDeviceUsage(m_UltimateTrackerUsageMap[description.product])
                    /// to set assign the XrTrackerDevice with product name kProductUltimateTracker0 to the commonUsage kUltimateTrackerUsage0.
                    InputSystem.SetDeviceUsage(this, m_UltimateTrackerUsageMap[description.product]);
                    sb.Clear().Append(LOG_TAG).Append("FinishSetup() usage: ").Append(m_UltimateTrackerUsageMap[description.product]); DEBUG(sb);
#if DEBUG_CODE
                    /// We cannot update the ActionMap outside the RegisterActionMapsWithRuntime method so ignore this code.
                    if (inputActionIsTracked == null)
                    {
                        //string actionBindingIsTracked = "<XRController>{LeftHand}/isTracked";
                        string actionBindingIsTracked = "<" + kLayoutName + ">{" + m_UltimateTrackerUsageMap[description.product] + "}/isTracked";
                        sb.Clear().Append(LOG_TAG).Append("FinishSetup() ").Append(m_UltimateTrackerUsageMap[description.product]).Append(", action binding of IsTracked: ").Append(actionBindingIsTracked);
                        DEBUG(sb);

                        inputActionIsTracked = new InputAction(
                            type: InputActionType.Value,
                            binding: actionBindingIsTracked);

                        inputActionIsTracked.Enable();
                    }
#endif
                }
            }

#if DEBUG_CODE
            /// We cannot update the ActionMap outside the RegisterActionMapsWithRuntime method so ignore this code.
            private bool UpdateInputDeviceInRuntime = true;
            private bool bRoleUpdated = false;
            public void OnUpdate()
            {
                if (!UpdateInputDeviceInRuntime) { return; }
                if (m_Instance == null) { return; }
                if (inputActionIsTracked == null) { return; }

                /// Updates the Usage (tracker role) when IsTracked becomes true.
                if (inputActionIsTracked.ReadValue<float>() > 0 && !bRoleUpdated)
                {
                    sb.Clear().Append(LOG_TAG).Append("OnUpdate() Update the InputDevice with product: ").Append(description.product); DEBUG(sb);

                    if (m_UltimateTrackerUsageMap.ContainsKey(description.product)) { m_Instance.UpdateInputDeviceUltimateTracker(description.product); }

                    bRoleUpdated = true;
                }
            }
#endif
        }

        /// <summary>The interaction profile string used to reference the wrist tracker interaction input device.</summary>
        private const string profile = "/interaction_profiles/htc/vive_xr_tracker";

        #region OpenXR Life Cycle
#pragma warning disable
        private bool m_XrInstanceCreated = false;
#pragma warning restore
        private XrInstance m_XrInstance = 0;
        /// <summary>
        /// Called when <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#xrCreateInstance">xrCreateInstance</see> is done.
        /// </summary>
        /// <param name="xrInstance">The created instance.</param>
        /// <returns>True for valid <see cref="XrInstance">XrInstance</see></returns>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            m_XrInstanceCreated = true;
            m_XrInstance = xrInstance;
            m_Instance = this;
            sb.Clear().Append(LOG_TAG).Append("OnInstanceCreate() ").Append(m_XrInstance); DEBUG(sb);

            GetXrFunctionDelegates(m_XrInstance);
            return true;
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

        private static bool m_XrSessionCreated = false;
        private static XrSession m_XrSession = 0;
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
            if (m_XrSession == xrSession)
            {
                m_XrSession = 0;
                m_XrSessionCreated = false;
            }
        }

        // "<" + kLayoutName + ">{" + m_UltimateTrackerUsageMap[description.product] + "}/isTracked"
        private const string kLayoutName = "ViveXRTracker";

        /// <summary>
        /// Registers the <see cref="XrTrackerDevice"/> layout with product name to the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            sb.Clear().Append(LOG_TAG).Append("RegisterDeviceLayout() ").Append(kLayoutName).Append(", product: ").Append(@kProducts); DEBUG(sb);
            InputSystem.RegisterLayout(typeof(XrTrackerDevice),
                        kLayoutName,
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(@kProducts));
        }

        /// <summary>
        /// Removes the <see cref="XrTrackerDevice"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            sb.Clear().Append(LOG_TAG).Append("UnregisterDeviceLayout() ").Append(kLayoutName); DEBUG(sb);
            InputSystem.RemoveLayout(kLayoutName);
        }
        #endregion

        #region OpenXR function delegates
        /// xrGetInstanceProcAddr
        OpenXRHelper.xrGetInstanceProcAddrDelegate XrGetInstanceProcAddr;
        /// xrGetInputSourceLocalizedName
        static OpenXRHelper.xrGetInputSourceLocalizedNameDelegate xrGetInputSourceLocalizedName = null;
        /// xrEnumerateInstanceExtensionProperties
        OpenXRHelper.xrEnumerateInstanceExtensionPropertiesDelegate xrEnumerateInstanceExtensionProperties = null;
        private bool GetXrFunctionDelegates(XrInstance xrInstance)
        {
            /// xrGetInstanceProcAddr
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

            /// xrEnumerateInstanceExtensionProperties
            if (XrGetInstanceProcAddr(xrInstance, "xrEnumerateInstanceExtensionProperties", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    sb.Clear().Append(LOG_TAG).Append("Get function pointer of xrEnumerateInstanceExtensionProperties."); DEBUG(sb);
                    xrEnumerateInstanceExtensionProperties = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(OpenXRHelper.xrEnumerateInstanceExtensionPropertiesDelegate)) as OpenXRHelper.xrEnumerateInstanceExtensionPropertiesDelegate;
                }
                else
                {
                    sb.Clear().Append(LOG_TAG).Append("No function pointer of xrEnumerateInstanceExtensionProperties.");
                    ERROR(sb);
                }
            }
            else
            {
                sb.Clear().Append(LOG_TAG).Append("No function pointer of xrEnumerateInstanceExtensionProperties");
                ERROR(sb);
            }

            /// xrGetInputSourceLocalizedName
            if (XrGetInstanceProcAddr(xrInstance, "xrGetInputSourceLocalizedName", out funcPtr) == XrResult.XR_SUCCESS)
            {
                if (funcPtr != IntPtr.Zero)
                {
                    sb.Clear().Append(LOG_TAG).Append("Get function pointer of xrGetInputSourceLocalizedName."); DEBUG(sb);
                    xrGetInputSourceLocalizedName = Marshal.GetDelegateForFunctionPointer(
                        funcPtr,
                        typeof(OpenXRHelper.xrGetInputSourceLocalizedNameDelegate)) as OpenXRHelper.xrGetInputSourceLocalizedNameDelegate;
                }
                else
                {
                    sb.Clear().Append(LOG_TAG).Append("No function pointer of xrGetInputSourceLocalizedName.");
                    ERROR(sb);
                }
            }
            else
            {
                sb.Clear().Append(LOG_TAG).Append("No function pointer of xrGetInputSourceLocalizedName");
                ERROR(sb);
            }

            return true;
        }
#endregion

        // Available Bindings
        /// <summary>
        /// Constant for a pose interaction binding '.../input/entity_htc/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string entityPose = "/input/entity_htc/pose";

        private void RegisterActionMap(string in_name, string product, string in_sn, string in_path)
        {
            sb.Clear().Append(LOG_TAG).Append("RegisterActionMap() Added ActionMapConfig of ").Append(in_path)
                .Append(", localizedName = ").Append(product)
                .Append(" { name = ").Append(in_name)
                .Append(", desiredInteractionProfile = ").Append(profile)
                .Append(", serialNumber = ").Append(in_sn);
            DEBUG(sb);

            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = in_name,
                localizedName = product,
                desiredInteractionProfile = profile,
                manufacturer = "HTC",
                serialNumber = in_sn,
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.TrackedDevice,
                        userPath = in_path
                    }
                },
                actions = new List<ActionConfig>()
                {
                    // Device Pose
                    new ActionConfig()
                    {
                        name = "devicePose",
                        localizedName = "Grip Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Device"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = entityPose,
                                interactionProfileName = profile,
                            }
                        }
                    }
                }
            };

            AddActionMap(actionMap);
        }
        /// <summary>
        /// Registers action maps to Unity XR.
        /// </summary>
        protected override void RegisterActionMapsWithRuntime()
        {
            if (OpenXRHelper.IsExtensionSupported(xrEnumerateInstanceExtensionProperties, kOpenxrExtensionString) != XrResult.XR_SUCCESS)
            {
                sb.Clear().Append(LOG_TAG).Append("RegisterActionMapsWithRuntime() ").Append(kOpenxrExtensionString).Append(" is NOT supported."); ERROR(sb);
                return;
            }

            /// Updates m_UltimateTrackerPathMap.
            if (!EnumeratePath())
            {
                sb.Clear().Append(LOG_TAG).Append("RegisterActionMapsWithRuntime() EnumeratePath failed.");
                ERROR(sb);
            }

            for (int userIndex = 0; userIndex < s_UltimateTrackerProduct.Length; userIndex++)
            {
                string product = s_UltimateTrackerProduct[userIndex];
                RegisterActionMap(
                    product: product,
                    in_name: m_UltimateTrackerActionMap[product],
                    in_sn: m_UltimateTrackerSerialMap[product],
                    in_path: m_UltimateTrackerPathMap[product]
                );
            }
        }

        XrPathsForInteractionProfileEnumerateInfoHTC enumerateInfo;
        private bool EnumeratePath()
        {
            if (!m_XrInstanceCreated) { return false; }

            string func = "EnumeratePath() ";

            // 1. Get user path count of /interaction_profiles/htc/vive_xr_tracker profile.
            UInt32 trackerCount = 0;
            enumerateInfo.type = XrStructureType.XR_TYPE_UNKNOWN;
            enumerateInfo.next = IntPtr.Zero;
            enumerateInfo.interactionProfile = StringToPath(profile); // /interaction_profiles/htc/vive_xr_tracker
            enumerateInfo.userPath = OpenXRHelper.XR_NULL_PATH;

            XrResult result = XR_HTC_path_enumeration.xrEnumeratePathsForInteractionProfileHTC(ref enumerateInfo, 0, ref trackerCount, null);
            sb.Clear().Append(LOG_TAG).Append(func).Append("xrEnumeratePathsForInteractionProfileHTC result: ").Append(result)
                .Append(", profile: ").Append(profile)
                .Append(", trackerCount: ").Append(trackerCount);
            DEBUG(sb);
            if (result != XrResult.XR_SUCCESS) { return false; }

            if (trackerCount > 0)
            {
                // 2. Get user paths of /interaction_profiles/htc/vive_xr_tracker profile.
                XrPath[] trackerPaths = new XrPath[trackerCount];
                for (int i = 0; i < trackerPaths.Length; i++) { trackerPaths[i] = OpenXRHelper.XR_NULL_PATH; }

                result = XR_HTC_path_enumeration.xrEnumeratePathsForInteractionProfileHTC(
                    ref enumerateInfo,
                    pathCapacityInput: (UInt32)(trackerPaths.Length & 0x7FFFFFFF),
                    pathCountOutput: ref trackerCount,
                    paths: trackerPaths);
                if (result != XrResult.XR_SUCCESS)
                {
                    sb.Clear().Append(LOG_TAG).Append(func).Append("xrEnumeratePathsForInteractionProfileHTC result: ").Append(result); ERROR(sb);
                    return false;
                }

                int ultimate_tracker_index = 0;
                for (int i = 0; i < trackerCount; i++)
                {
                    string userPath = PathToString(trackerPaths[i]);
                    sb.Clear().Append(LOG_TAG).Append(func).Append("xrEnumeratePathsForInteractionProfileHTC[").Append(i).Append("] ").Append(userPath); DEBUG(sb);

                    if (userPath.Contains("ultimate", StringComparison.OrdinalIgnoreCase) && ultimate_tracker_index < s_UltimateTrackerProduct.Length)
                    {
                        string product = s_UltimateTrackerProduct[ultimate_tracker_index];

                        m_UltimateTrackerPathMap[product] = userPath;
                        sb.Clear().Append(LOG_TAG).Append(func).Append("Updates ").Append(product).Append(" path to ").Append(m_UltimateTrackerPathMap[product]); DEBUG(sb);

                        m_UltimateTrackerSerialMap[product] = ConvertUserPathToSerialNumber(userPath);
                        sb.Clear().Append(LOG_TAG).Append(func).Append("Updates ").Append(product).Append(" serial number to ").Append(m_UltimateTrackerSerialMap[product]); DEBUG(sb);

                        ultimate_tracker_index++;
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// For example, user path "/user/xr_tracker_htc/vive_ultimate_tracker_0" will become serial number "VIVE_Ultimate_Tracker_0".
        /// </summary>
        /// <param name="userPath">The user path from <see cref="EnumeratePath"> EnumeratePath </see>.</param>
        /// <returns>Serial Number in string.</returns>
        private string ConvertUserPathToSerialNumber(string userPath)
        {
            string result = "";

            int lastSlashIndex = userPath.LastIndexOf('/');
            if (lastSlashIndex >= 0)
            {
                string[] parts = userPath.Substring(lastSlashIndex + 1).Split('_');
                parts[0] = parts[0].ToUpper();
                for (int i = 1; i < parts.Length - 1; i++)
                {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
                }
                result = string.Join("_", parts);
            }

            return result;
        }

        [Obsolete("This enumeration is deprecated. Please use XrInputSourceLocalizedNameFlags instead.")]
        public enum InputSourceType : UInt64
        {
            SerialNumber = XrInputSourceLocalizedNameFlags.XR_INPUT_SOURCE_LOCALIZED_NAME_SERIAL_NUMBER_BIT_HTC,
            TrackerRole = XrInputSourceLocalizedNameFlags.XR_INPUT_SOURCE_LOCALIZED_NAME_USER_PATH_BIT,
        }
        [Obsolete("This function is deprecated. Please use OpenXRHelper.GetInputSourceName instead.")]
        public static XrResult GetInputSourceName(XrPath path, InputSourceType sourceType, out string sourceName)
        {
            string func = "GetInputSourceName() ";

            sourceName = "";
            if (!m_XrSessionCreated || xrGetInputSourceLocalizedName == null) { return XrResult.XR_ERROR_VALIDATION_FAILURE; }

            string userPath = PathToString(path);
            sb.Clear().Append(LOG_TAG).Append(func)
                .Append("userPath: ").Append(userPath).Append(", flag: ").Append((UInt64)sourceType); DEBUG(sb);

            XrInputSourceLocalizedNameGetInfo nameInfo = new XrInputSourceLocalizedNameGetInfo(
                XrStructureType.XR_TYPE_INPUT_SOURCE_LOCALIZED_NAME_GET_INFO,
                IntPtr.Zero, path, (XrInputSourceLocalizedNameFlags)sourceType);

            UInt32 nameSizeIn = 0;
            UInt32 nameSizeOut = 0;
            char[] buffer = new char[0];

            XrResult result = xrGetInputSourceLocalizedName(m_XrSession, ref nameInfo, nameSizeIn, ref nameSizeOut, buffer);
            sb.Clear().Append(LOG_TAG).Append(func)
                .Append("1.xrGetInputSourceLocalizedName(").Append(userPath).Append(") result: ").Append(result)
                .Append(", flag: ").Append((UInt64)nameInfo.whichComponents)
                .Append(", bufferCapacityInput: ").Append(nameSizeIn)
                .Append(", bufferCountOutput: ").Append(nameSizeOut);
            DEBUG(sb);
            if (result == XrResult.XR_SUCCESS)
            {
                if (nameSizeOut < 1)
                {
                    sb.Clear().Append(LOG_TAG).Append(func)
                        .Append("xrGetInputSourceLocalizedName(").Append(userPath).Append(")")
                        .Append(", flag: ").Append((UInt64)sourceType)
                        .Append("bufferCountOutput size is invalid!");
                    ERROR(sb);
                    return XrResult.XR_ERROR_VALIDATION_FAILURE;
                }

                nameSizeIn = nameSizeOut;
                buffer = new char[nameSizeIn];

                result = xrGetInputSourceLocalizedName(m_XrSession, ref nameInfo, nameSizeIn, ref nameSizeOut, buffer);
                sb.Clear().Append(LOG_TAG).Append(func)
                    .Append("2.xrGetInputSourceLocalizedName(").Append(userPath).Append(") result: ").Append(result)
                    .Append(", flag: ").Append((UInt64)nameInfo.whichComponents)
                    .Append(", bufferCapacityInput: ").Append(nameSizeIn)
                    .Append(", bufferCountOutput: ").Append(nameSizeOut);
                DEBUG(sb);
                if (result == XrResult.XR_SUCCESS)
                {
                    sourceName = new string(buffer).TrimEnd('\0');
                }
            }

            return result;
        }

#if DEBUG_CODE
        XrInputSourceLocalizedNameGetInfo nameInfo;
        private bool updateSerialNumber = true, updateUsage = false;
        private bool UpdateTrackerMaps(string product, XrPath path, ref Dictionary<string, string> serialMap, ref Dictionary<string, string> roleMap)
        {
            string func = "UpdateTrackerMaps() ";
            string s_path = PathToString(path);
            sb.Clear().Append(LOG_TAG).Append(func).Append("product: ").Append(product).Append(", path: ").Append(s_path); DEBUG(sb);

            // -------------------- Tracker Serial Number --------------------
            if (updateSerialNumber)
            {
                nameInfo.type = XrStructureType.XR_TYPE_INPUT_SOURCE_LOCALIZED_NAME_GET_INFO;
                nameInfo.next = IntPtr.Zero;
                nameInfo.sourcePath = path;
                nameInfo.whichComponents = XrInputSourceLocalizedNameFlags.XR_INPUT_SOURCE_LOCALIZED_NAME_SERIAL_NUMBER_BIT_HTC;

                XrResult result = OpenXRHelper.GetInputSourceName(
                    xrGetInputSourceLocalizedName,
                    m_XrSession,
                    ref nameInfo,
                    out string sn);

                sb.Clear().Append(LOG_TAG).Append(func).Append("GetInputSourceName(").Append(s_path).Append(")")
                    .Append(", sourceType: ").Append(nameInfo.whichComponents)
                    .Append(", serial number: ").Append(sn)
                    .Append(", result: ").Append(result);
                DEBUG(sb);

                /// For sample:
                /// A user path (e.g. "/user/tracker_htc/index0") has the "sourceName" kUltimateTrackerSN0 ("VIVE_Ultimate_Tracker_0") which means
                /// the corresponding product (e.g. kProductUltimateTracker0 = "VIVE Ultimate Tracker 0") has the "sourceName" kUltimateTrackerSN0.
                /// So we have to set the serial of the product name kProductUltimateTracker0 to kUltimateTrackerSN0.
                if (result == XrResult.XR_SUCCESS)
                {
                    if (!serialMap.ContainsKey(product))
                        serialMap.Add(product, sn);
                    else
                        serialMap[product] = sn;

                    sb.Clear().Append(LOG_TAG).Append(func).Append("Sets product ").Append(product).Append(" with serial number ").Append(serialMap[product]); DEBUG(sb);
                }
            }
            // -------------------- Tracker Role --------------------
            if (updateUsage)
            {
                nameInfo.type = XrStructureType.XR_TYPE_INPUT_SOURCE_LOCALIZED_NAME_GET_INFO;
                nameInfo.next = IntPtr.Zero;
                nameInfo.sourcePath = path;
                nameInfo.whichComponents = XrInputSourceLocalizedNameFlags.XR_INPUT_SOURCE_LOCALIZED_NAME_USER_PATH_BIT;

                XrResult result = OpenXRHelper.GetInputSourceName(
                    xrGetInputSourceLocalizedName,
                    m_XrSession,
                    ref nameInfo,
                    out string role);

                sb.Clear().Append(LOG_TAG).Append(func).Append("GetInputSourceName(").Append(s_path).Append(")")
                    .Append(", sourceType: ").Append(nameInfo.whichComponents)
                    .Append(", role: ").Append(role)
                    .Append(", result: ").Append(result);
                DEBUG(sb);

                if (result == XrResult.XR_SUCCESS)
                {
                    /// For sample:
                    /// A user path (e.g. "/user/tracker_htc/index0") has the "sourceName" kTrackerRoleLeftWrist ("Left Wrist") which means
                    /// the corresponding product (e.g. kProductUltimateTracker0 = "VIVE Ultimate Tracker 0") has the "sourceName" kTrackerRoleLeftWrist.
                    /// So we have to set the usage of the product name kProductUltimateTracker0 to kTrackerRoleLeftWrist.
                    if (!roleMap.ContainsKey(product))
                        roleMap.Add(product, role);
                    else
                        roleMap[product] = role;

                    sb.Clear().Append(LOG_TAG).Append(func).Append("Sets product ").Append(product).Append(" with usage ").Append(roleMap[product]); DEBUG(sb);
                }
            }

            return true;
        }

        /// <summary>
        /// Due to "ActionMap must be added from within the RegisterActionMapsWithRuntime method", this function is unusable.
        /// </summary>
        /// <param name="product">A tracker's product name.</param>
        private void UpdateInputDeviceUltimateTracker(string product)
        {
            if (!IsUltimateTracker(product)) { return; }
            string func = "UpdateInputDeviceUltimateTracker() ";

            sb.Clear().Append(LOG_TAG).Append(func)
                .Append("product: ").Append(product)
                .Append(", serial number: ").Append(m_UltimateTrackerSerialMap[product])
                .Append(", user path: ").Append(m_UltimateTrackerPathMap[product])
                .Append(", usage: ").Append(m_UltimateTrackerUsageMap[product]);
            DEBUG(sb);

            XrPath path = StringToPath(m_UltimateTrackerPathMap[product]);
            /// Updates tracker serial number (m_UltimateTrackerSerialMap) and role (m_UltimateTrackerUsageMap)
            if (UpdateTrackerMaps(product, path, ref m_UltimateTrackerSerialMap, ref m_UltimateTrackerUsageMap))
            {
                sb.Clear().Append(LOG_TAG).Append(func)
                    .Append("Maps of ").Append(product)
                    .Append(" with user path ").Append(m_UltimateTrackerPathMap[product]).Append(" are updated.");
                DEBUG(sb);

                bool foundProduct = false;
                for (int i = 0; i < InputSystem.devices.Count; i++)
                {
                    if (InputSystem.devices[i] is XrTrackerDevice &&
                        InputSystem.devices[i].description.product.Equals(product))
                    {
                        sb.Clear().Append(LOG_TAG).Append(func)
                            .Append("Removes the XrTrackerDevice product ").Append(product);
                        DEBUG(sb);

                        InputSystem.RemoveDevice(InputSystem.devices[i]);
                        foundProduct = true;
                        break;
                    }
                }
                if (foundProduct)
                {
                    sb.Clear().Append(LOG_TAG).Append(func).Append("Adds a XrTrackerDevice product ").Append(product); DEBUG(sb);
                    RegisterActionMap(
                        product: product,
                        in_name: m_UltimateTrackerActionMap[product],
                        in_sn: m_UltimateTrackerSerialMap[product],
                        in_path: m_UltimateTrackerPathMap[product]
                    );
                }
            }
        }
#endif
    }
}