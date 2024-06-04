// Copyright HTC Corporation All Rights Reserved.

using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.OpenXR;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Input;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
using PoseControl = UnityEngine.InputSystem.XR.PoseControl;
#else
using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;
#endif

namespace VIVE.OpenXR.Hand
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of hand interaction profiles in OpenXR. It enables <see cref="ViveHandInteraction.kOpenxrExtensionString">XR_HTC_hand_interaction</see> in the underyling runtime.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "VIVE XR Hand Interaction",
        BuildTargetGroups = new[] { BuildTargetGroup.Android , BuildTargetGroup.Standalone},
        Company = "HTC",
        Desc = "Support for enabling the hand interaction profile. Will register the controller map for hand interaction if enabled.",
        DocumentationLink = "..\\Documentation",
        Version = "1.0.0",
        OpenxrExtensionStrings = kOpenxrExtensionString,
        Category = FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class ViveHandInteraction : OpenXRInteractionFeature
    {
        const string LOG_TAG = "VIVE.OpenXR.Hand.ViveHandInteraction ";
        StringBuilder m_sb = null;
        StringBuilder sb {
            get {
                if (m_sb == null) { m_sb = new StringBuilder(); }
                return m_sb;
            }
        }
        void DEBUG(StringBuilder msg) { Debug.Log(msg); }
        void WARNING(StringBuilder msg) { Debug.LogWarning(msg); }

        /// <summary>
        /// OpenXR specification <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_HTC_hand_interaction">12.69. XR_HTC_hand_interaction</see>.
        /// </summary>
        public const string kOpenxrExtensionString = "XR_HTC_hand_interaction";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "vive.openxr.feature.hand.interaction";

        /// <summary>
        /// The interaction profile string used to reference the hand interaction input device.
        /// </summary>
        private const string profile = "/interaction_profiles/htc/hand_interaction";

        private const string leftHand = "/user/hand_htc/left";
        private const string rightHand = "/user/hand_htc/right";

        /// <summary>
        /// Constant for a float interaction binding '.../input/select/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string selectValue = "/input/select/value";

        /// <summary>
        /// Constant for a float interaction binding '.../input/squeeze/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string gripValue = "/input/squeeze/value";


        /// <summary>
        /// Constant for a pose interaction binding '.../input/aim/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string pointerPose = "/input/aim/pose";

        /// <summary>
        /// Constant for a pose interaction binding '.../input/grip/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string devicePose = "/input/grip/pose";

        [Preserve, InputControlLayout(displayName = "VIVE Hand Interaction (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = true)]
        public class HandInteractionDevice : OpenXRDevice
        {
            const string LOG_TAG = "VIVE.OpenXR.Hand.ViveHandInteraction.HandInteractionDevice";
            void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="ViveHandInteraction.selectValue"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "selectAxis, pinchStrength" }, usage = "Select")]
            public AxisControl selectValue { get; private set; }

            /// <summary>
            /// A <see cref="AxisControl"/> representing information from the <see cref="ViveHandInteraction.squeeze"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "GripAxis" }, usage = "Grip")]
            public AxisControl gripValue { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> representing information from the <see cref="ViveHandInteraction.devicePose"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "gripPose" }, usage = "Device")]
            public PoseControl devicePose { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> representing the <see cref="ViveHandInteraction.pointerPose"/> OpenXR binding.
            /// </summary>
			[Preserve, InputControl(offset = 0, alias = "aimPose", usage = "Pointer")]
            public PoseControl pointerPose { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) required for backwards compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping devicePose/isTracked.
			/// </summary>
			[Preserve, InputControl(offset = 8, usage = "IsTracked")]
			public ButtonControl isTracked { get; private set; }

			/// <summary>
			/// A [IntegerControl](xref:UnityEngine.InputSystem.Controls.IntegerControl) required for backwards compatibility with the XRSDK layouts. This represents the bit flag set to indicate what data is valid. This value is equivalent to mapping devicePose/trackingState.
			/// </summary>
			[Preserve, InputControl(offset = 12, usage = "TrackingState")]
			public IntegerControl trackingState { get; private set; }

			/// <summary>
			/// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the device position. For the VIVE Focus 3 device, this is both the device and the pointer position. This value is equivalent to mapping devicePose/position.
			/// </summary>
			[Preserve, InputControl(offset = 16, alias = "gripPosition")]
			public Vector3Control devicePosition { get; private set; }

			/// <summary>
			/// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the device orientation. For the VIVE Focus 3 device, this is both the device and the pointer rotation. This value is equivalent to mapping devicePose/rotation.
			/// </summary>
			[Preserve, InputControl(offset = 28, alias = "gripOrientation")]
			public QuaternionControl deviceRotation { get; private set; }


            /// <summary>
            /// Internal call used to assign controls to the the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                DEBUG("FinishSetup() interfaceName: " + description.interfaceName
                    + ", deviceClass: " + description.deviceClass
                    + ", product: " + description.product
                    + ", serial: " + description.serial
                    + ", version: " + description.version);

                base.FinishSetup();

                selectValue = GetChildControl<AxisControl>("selectValue");
                gripValue = GetChildControl<AxisControl>("gripValue");

                devicePose = GetChildControl<PoseControl>("devicePose");
                pointerPose = GetChildControl<PoseControl>("pointerPose");

                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
            }
        }

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
            // Requires the eye tracking extension
            if (!OpenXRRuntime.IsExtensionEnabled(kOpenxrExtensionString))
            {
                sb.Clear().Append(LOG_TAG).Append("OnInstanceCreate() ").Append(kOpenxrExtensionString).Append(" is NOT enabled."); WARNING(sb);
                return false;
            }

            m_XrInstanceCreated = true;
            m_XrInstance = xrInstance;
            sb.Clear().Append(LOG_TAG).Append("OnInstanceCreate() " + m_XrInstance); DEBUG(sb);

            return base.OnInstanceCreate(xrInstance);
        }

        private const string kLayoutName = "ViveHandInteraction";
        private const string kDeviceLocalizedName = "Vive Hand Interaction OpenXR";
        /// <summary>
        /// Registers the <see cref="HandInteractionDevice"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            sb.Clear().Append(LOG_TAG).Append("RegisterDeviceLayout() Layout: ").Append(kLayoutName)
                .Append(", Product: ").Append(kDeviceLocalizedName);
            DEBUG(sb);
            InputSystem.RegisterLayout(typeof(HandInteractionDevice),
                        kLayoutName,
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="HandInteractionDevice"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            sb.Clear().Append(LOG_TAG).Append("UnregisterDeviceLayout() ").Append(kLayoutName); DEBUG(sb);
            InputSystem.RemoveLayout(kLayoutName);
        }

        /// <summary>
        /// Registers action maps to Unity XR.
        /// </summary>
        protected override void RegisterActionMapsWithRuntime()
        {
            sb.Clear().Append(LOG_TAG).Append("RegisterActionMapsWithRuntime() Action map vivehandinteraction")
                .Append(", localizedName: ").Append(kDeviceLocalizedName)
                .Append(", desiredInteractionProfile").Append(profile);
            DEBUG(sb);

            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "vivehandinteraction",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "HTC",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Left,
                        userPath = leftHand // "/user/hand_htc/left"
                    },
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Right,
                        userPath = rightHand // "/user/hand_htc/right"
                    }
                },
                actions = new List<ActionConfig>()
                {
                    // Grip Axis
                    new ActionConfig()
                    {
                        name = "gripValue",
                        localizedName = "Grip Axis",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Grip"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = gripValue,
                                interactionProfileName = profile,
                            }
                        }
                    },
					// Select Axis
					new ActionConfig()
                    {
                        name = "selectValue",
                        localizedName = "Select Axis",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Select"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = selectValue,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Grip pose
                    new ActionConfig()
                    {
                        name = "devicePose",
                        localizedName = "Device Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Device"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = devicePose,
                                interactionProfileName = profile,
                            }
                        }
                    },
					// Pointer Pose
					new ActionConfig()
                    {
                        name = "pointerPose",
                        localizedName = "Pointer Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Pointer"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = pointerPose,
                                interactionProfileName = profile,
                            }
                        }
                    }
                }
            };

            AddActionMap(actionMap);
        }
    }
}