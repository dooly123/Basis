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

namespace VIVE.OpenXR.Tracker
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of wrist tracker interaction profiles in OpenXR. It enables XR_HTC_vive_wrist_tracker_interaction in the underyling runtime.
    /// This creates a new <see cref="InputDevice"/> with the <see cref="InputDeviceCharacteristics.TrackedDevice"/> characteristic.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "VIVE XR Wrist Tracker",
        BuildTargetGroups = new[] { BuildTargetGroup.Android , BuildTargetGroup.Standalone},
        Company = "HTC",
        Desc = "Support for enabling the wrist tracker interaction profile. Will register the controller map for wrist tracker if enabled.",
        DocumentationLink = "..\\Documentation",
        Version = "1.0.0",
        OpenxrExtensionStrings = kOpenxrExtensionString,
        Category = FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class ViveWristTracker : OpenXRInteractionFeature
    {
        const string LOG_TAG = "VIVE.OpenXR.Tracker.ViveWristTracker ";
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
        /// OpenXR specification <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_HTC_vive_wrist_tracker_interaction">12.72. XR_HTC_vive_wrist_tracker_interaction</see>.
        /// </summary>
        public const string kOpenxrExtensionString = "XR_HTC_vive_wrist_tracker_interaction";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "vive.openxr.feature.tracker";

        /// <summary>The interaction profile string used to reference the wrist tracker interaction input device.</summary>
        private const string profile = "/interaction_profiles/htc/vive_wrist_tracker";

        private const string leftWrist = "/user/wrist_htc/left";
        private const string rightWrist = "/user/wrist_htc/right";

		// Available Bindings
		// Left Hand Only
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/x/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="leftWrist"/> user path.
		/// </summary>
		public const string buttonX = "/input/x/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/menu/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="leftWrist"/> user path.
        /// </summary>
        public const string menu = "/input/menu/click";

        // Right Hand Only
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/a/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="rightWrist"/> user path.
        /// </summary>
        public const string buttonA = "/input/a/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/system/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="rightWrist"/> user path.
        /// </summary>
        public const string system = "/input/system/click";

        // Both Hands
        /// <summary>
        /// Constant for a pose interaction binding '.../input/entity_htc/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string entityPose = "/input/entity_htc/pose";

        [Preserve, InputControlLayout(displayName = "VIVE Wrist Tracker (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = true)]
        public class WristTrackerDevice : OpenXRDevice
        {
            const string LOG_TAG = "VIVE.OpenXR.Tracker.ViveWristTracker.WristTrackerDevice ";
            StringBuilder m_sb = null;
            StringBuilder sb {
                get {
                    if (m_sb == null) { m_sb = new StringBuilder(); }
                    return m_sb;
                }
            }
            void DEBUG(StringBuilder msg) { Debug.Log(msg); }

            /// <summary>
            /// A <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Controls.ButtonControl.html">ButtonControl</see> that represents the <see cref="buttonA"/> <see cref="buttonX"/> OpenXR bindings, depending on handedness.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "A", "X", "buttonA", "buttonX" }, usage = "PrimaryButton")]
            public ButtonControl primaryButton { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="entityPose"/> OpenXR binding. The entity pose represents the location of the tracker.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "entityPose" }, usage = "Device")]
            public PoseControl devicePose { get; private set; }

            /// <summary>
            /// A <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Controls.ButtonControl.html">ButtonControl</see> required for backwards compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping devicePose/isTracked.
            /// </summary>
            [Preserve, InputControl(offset = 24, usage = "IsTracked")]
            public ButtonControl isTracked { get; private set; }

            /// <summary>
            /// A <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Controls.IntegerControl.html">IntegerControl</see> required for backwards compatibility with the XRSDK layouts. This represents the bit flag set to indicate what data is valid. This value is equivalent to mapping devicePose/trackingState.
            /// </summary>
            [Preserve, InputControl(offset = 28, usage = "TrackingState")]
            public IntegerControl trackingState { get; private set; }

            /// <summary>
            /// A <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@0.1/api/UnityEngine.Experimental.Input.Controls.Vector3Control.html">Vector3Control</see> required for backwards compatibility with the XRSDK layouts. This is the device position. For the VIVE XR device, this is both the device and the pointer position. This value is equivalent to mapping devicePose/position.
            /// </summary>
            [Preserve, InputControl(offset = 32, alias = "gripPosition")]
            public Vector3Control devicePosition { get; private set; }

            /// <summary>
            /// A <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Controls.QuaternionControl.html">QuaternionControl</see> required for backwards compatibility with the XRSDK layouts. This is the device orientation. For the VIVE XR device, this is both the device and the pointer rotation. This value is equivalent to mapping devicePose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 44, alias = "gripOrientation")]
            public QuaternionControl deviceRotation { get; private set; }

            /// <summary>
            /// A <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Controls.ButtonControl.html">ButtonControl</see> that represents the <see cref="ViveWristTracker.menu"/> OpenXR bindings, depending on handedness.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "menuButton" }, usage = "MenuButton")]
            public ButtonControl menu { get; private set; }

            /// <summary>
            /// Internal call used to assign controls to the the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                base.FinishSetup();

                primaryButton = GetChildControl<ButtonControl>("primaryButton");
                menu = GetChildControl<ButtonControl>("menu");
                devicePose = GetChildControl<PoseControl>("devicePose");
                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");

                sb.Clear().Append(LOG_TAG)
                    .Append(" FinishSetup() device interfaceName: ").Append(description.interfaceName)
                    .Append(", deviceClass: ").Append(description.deviceClass)
                    .Append(", product: ").Append(description.product)
                    .Append(", serial: ").Append(description.serial);
                DEBUG(sb);
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
            m_XrInstanceCreated = true;
            m_XrInstance = xrInstance;
            sb.Clear().Append(LOG_TAG).Append(" OnInstanceCreate() " + m_XrInstance); DEBUG(sb);

            return base.OnInstanceCreate(xrInstance);
        }

        private const string kLayoutName = "ViveWristTracker";
        private const string kDeviceLocalizedName = "Vive Wrist Tracker OpenXR";
        /// <summary>
        /// Registers the <see cref="WristTrackerDevice"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            InputSystem.RegisterLayout(typeof(WristTrackerDevice),
                        kLayoutName,
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="WristTrackerDevice"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            InputSystem.RemoveLayout(kLayoutName);
        }

        /// <summary>
        /// Registers action maps to Unity XR.
        /// </summary>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "vivewristtracker",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "HTC",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left,
                        userPath = leftWrist // "/user/wrist_htc/left"
                    },
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right,
                        userPath = rightWrist // "/user/wrist_htc/right"
                    }
                },
                actions = new List<ActionConfig>()
                {
					// X / A Press
					new ActionConfig()
					{
						name = "primaryButton",
						localizedName = "Primary Pressed",
						type = ActionType.Binary,
						usages = new List<string>()
						{
							"PrimaryButton"
						},
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = buttonX,
								interactionProfileName = profile,
								userPaths = new List<string>() { leftWrist }
							},
							new ActionBinding()
							{
								interactionPath = buttonA,
								interactionProfileName = profile,
								userPaths = new List<string>() { rightWrist }
							},
						}
					},
					// Menu
					new ActionConfig()
                    {
                        name = "menu",
                        localizedName = "Menu",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "MenuButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = menu,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { leftWrist }
                            },
                            new ActionBinding()
                            {
                                interactionPath = system,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { rightWrist }
                            },
                        }
                    },
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
    }
}
