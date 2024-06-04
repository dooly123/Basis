// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Input;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
using PoseControl = UnityEngine.InputSystem.XR.PoseControl;
#else
using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;
#endif

namespace VIVE.OpenXR
{
	/// <summary>
	/// This <see cref="OpenXRInteractionFeature"/> enables the use of HTC VIVE Focus 3 interaction profiles in OpenXR.
	/// </summary>
#if UNITY_EDITOR
	[OpenXRFeature(UiName = "VIVE Focus 3 Controller Interaction",
		BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
		Company = "HTC",
		Desc = "Allows for mapping input to the VIVE Focus 3 interaction profile.",
		DocumentationLink = "https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_HTC_vive_focus3_controller_interaction",
		OpenxrExtensionStrings = kOpenxrExtensionString,
		Version = "1.0.0",
		Category = FeatureCategory.Interaction,
		FeatureId = featureId)]
#endif
	public class VIVEFocus3Profile : OpenXRInteractionFeature
	{
		const string LOG_TAG = "VIVE.OpenXR.VIVEFocus3Profile ";
		StringBuilder m_sb = null;
		StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Debug.Log(msg); }
		void ERROR(StringBuilder msg) { Debug.LogError(msg); }

		private static VIVEFocus3Profile m_Instance = null;

		public const string kOpenxrExtensionString = "XR_HTC_vive_focus3_controller_interaction";

		/// <summary>
		/// The feature id string. This is used to give the feature a well known id for reference.
		/// </summary>
		public const string featureId = "vive.openxr.feature.focus3controller";

		/// <summary>
		/// An Input System device based on the hand interaction profile in the <a href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_HTC_vive_focus3_controller_interaction">Interaction Profile</a>.
		/// </summary>
		[Preserve, InputControlLayout(displayName = "VIVE Focus 3 Controller (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" })]
		public class VIVEFocus3Controller : XRControllerWithRumble, IInputUpdateCallbackReceiver
		{
			const string LOG_TAG = "VIVE.OpenXR.VIVEFocus3Profile.VIVEFocus3Controller ";
			StringBuilder m_sb = null;
			StringBuilder sb {
				get {
					if (m_sb == null) { m_sb = new StringBuilder(); }
					return m_sb;
				}
			}
			void DEBUG(StringBuilder msg) { Debug.Log(msg); }
			void ERROR(StringBuilder msg) { Debug.LogError(msg); }

			/// <summary>
			/// A [Vector2Control](xref:UnityEngine.InputSystem.Controls.Vector2Control) that represents the <see cref="VIVEFocus3Profile.thumbstick"/> OpenXR binding.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "Primary2DAxis", "joystick", "joystickAxis", "thumbstickAxis" }, usage = "Primary2DAxis")]
			public Vector2Control thumbstick { get; private set; }

			/// <summary>
			/// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="VIVEFocus3Profile.grip"/> OpenXR binding.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "GripAxis", "squeeze" }, usage = "Grip")]
			public AxisControl grip { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="gripPress"/> OpenXR binding.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "GripButton", "squeezeClick" }, usage = "GripButton")]
			public ButtonControl gripPressed { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="gripTouch"/> OpenXR binding.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "GripTouch", "squeezeTouch" }, usage = "GripTouch")]
			public ButtonControl gripTouched { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="VIVEFocus3Profile.menu"/> OpenXR bindings, depending on handedness.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "menuButton" }, usage = "MenuButton")]
			public ButtonControl menu { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="buttonA"/> <see cref="buttonX"/> OpenXR bindings, depending on handedness.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "A", "X", "buttonA", "buttonX" }, usage = "PrimaryButton")]
			public ButtonControl primaryButton { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="buttonB"/> <see cref="buttonY"/> OpenXR bindings, depending on handedness.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "B", "Y", "buttonB", "buttonY" }, usage = "SecondaryButton")]
			public ButtonControl secondaryButton { get; private set; }

			/// <summary>
			/// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="VIVEFocus3Profile.trigger"/> OpenXR binding.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "triggerAxis" }, usage = "Trigger")]
			public AxisControl trigger { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="triggerClick"/> OpenXR binding.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "triggerButton", "triggerClick" }, usage = "TriggerButton")]
			public ButtonControl triggerPressed { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="triggerTouch"/> OpenXR binding.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "triggerTouch", "indexTouch", "indexNearTouched" }, usage = "TriggerTouch")]
			public ButtonControl triggerTouched { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="thumbstickClick"/> OpenXR binding.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "Primary2DAxisClick", "joystickPress", "joystickClick" }, usage = "Primary2DAxisClick")]
			public ButtonControl thumbstickClicked { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="thumbstickTouch"/> OpenXR binding.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "Primary2DAxisTouch", "joystickTouch" }, usage = "Primary2DAxisTouch")]
			public ButtonControl thumbstickTouched { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="thumbrest"/> OpenXR binding.
			/// </summary>
			[Preserve, InputControl(aliases = new[] { "ParkingTouched" })]
			public ButtonControl parkingTouched { get; private set; }

			/// <summary>
			/// A <see cref="PoseControl"/> that represents the <see cref="gripPose"/> OpenXR binding. The grip pose represents the location of the user's palm or holding a motion controller.
			/// </summary>
			[Preserve, InputControl(offset = 0, aliases = new[] { "device", "gripPose" }, usage = "Device")]
			public PoseControl devicePose { get; private set; }

			/// <summary>
			/// A <see cref="PoseControl"/> that represents the <see cref="VIVEFocus3Profile.pointerPose"/> OpenXR binding. The pointer pose represents the tip of the controller pointing forward.
			/// </summary>
			[Preserve, InputControl(offset = 0, alias = "aimPose", usage = "Pointer")]
			public PoseControl pointerPose { get; private set; }

			/// <summary>
			/// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) required for backwards compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping devicePose/isTracked.
			/// </summary>
			[Preserve, InputControl(offset = 24, usage = "IsTracked")]
			new public ButtonControl isTracked { get; private set; }

			/// <summary>
			/// A [IntegerControl](xref:UnityEngine.InputSystem.Controls.IntegerControl) required for backwards compatibility with the XRSDK layouts. This represents the bit flag set to indicate what data is valid. This value is equivalent to mapping devicePose/trackingState.
			/// </summary>
			[Preserve, InputControl(offset = 28, usage = "TrackingState")]
			new public IntegerControl trackingState { get; private set; }

			/// <summary>
			/// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the device position. For the VIVE Focus 3 device, this is both the device and the pointer position. This value is equivalent to mapping devicePose/position.
			/// </summary>
			[Preserve, InputControl(offset = 32, alias = "gripPosition")]
			new public Vector3Control devicePosition { get; private set; }

			/// <summary>
			/// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the device orientation. For the VIVE Focus 3 device, this is both the device and the pointer rotation. This value is equivalent to mapping devicePose/rotation.
			/// </summary>
			[Preserve, InputControl(offset = 44, alias = "gripOrientation")]
			new public QuaternionControl deviceRotation { get; private set; }

			/// <summary>
			/// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for back compatibility with the XRSDK layouts. This is the pointer position. This value is equivalent to mapping pointerPose/position.
			/// </summary>
			[Preserve, InputControl(offset = 92)]
			public Vector3Control pointerPosition { get; private set; }

			/// <summary>
			/// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the pointer rotation. This value is equivalent to mapping pointerPose/rotation.
			/// </summary>
			[Preserve, InputControl(offset = 104, alias = "pointerOrientation")]
			public QuaternionControl pointerRotation { get; private set; }

			/// <summary>
			/// A <see cref="HapticControl"/> that represents the <see cref="VIVEFocus3Profile.haptic"/> binding.
			/// </summary>
			[Preserve, InputControl(usage = "Haptic")]
			public HapticControl haptic { get; private set; }

			private bool UpdateInputDeviceInRuntime = false;

			/// <summary>
			/// Internal call used to assign controls to the the correct element.
			/// </summary>
			protected override void FinishSetup()
			{
				base.FinishSetup();

				thumbstick = GetChildControl<Vector2Control>("thumbstick");
				trigger = GetChildControl<AxisControl>("trigger");
				triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
				triggerTouched = GetChildControl<ButtonControl>("triggerTouched");
				grip = GetChildControl<AxisControl>("grip");
				gripPressed = GetChildControl<ButtonControl>("gripPressed");
				gripTouched = GetChildControl<ButtonControl>("gripTouched");
				menu = GetChildControl<ButtonControl>("menu");
				primaryButton = GetChildControl<ButtonControl>("primaryButton");
				secondaryButton = GetChildControl<ButtonControl>("secondaryButton");
				thumbstickClicked = GetChildControl<ButtonControl>("thumbstickClicked");
				thumbstickTouched = GetChildControl<ButtonControl>("thumbstickTouched");
				parkingTouched = GetChildControl<ButtonControl>("parkingTouched");

				devicePose = GetChildControl<PoseControl>("devicePose");
				pointerPose = GetChildControl<PoseControl>("pointerPose");

				isTracked = GetChildControl<ButtonControl>("isTracked");
				trackingState = GetChildControl<IntegerControl>("trackingState");
				devicePosition = GetChildControl<Vector3Control>("devicePosition");
				deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
				pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
				pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
				haptic = GetChildControl<HapticControl>("haptic");

				sb.Clear().Append(LOG_TAG)
					.Append("FinishSetup() device interfaceName: ").Append(description.interfaceName)
					.Append(", deviceClass: ").Append(description.deviceClass)
					.Append(", product: ").Append(description.product)
					.Append(", serial: ").Append(description.serial)
					.Append(", capabilities: ").Append(description.capabilities);
				DEBUG(sb);
			}

			private bool bRoleUpdatedLeft = false, bRoleUpdatedRight = false;
			public void OnUpdate()
			{
				if (!UpdateInputDeviceInRuntime) { return; }
				if (m_Instance == null) { return; }

				string func = "OnUpdate() ";
				if (leftHand.isTracked.ReadValue() > 0 && !bRoleUpdatedLeft)
				{
					sb.Clear().Append(LOG_TAG).Append(func)
						.Append("product: ").Append(description.product)
						.Append(" with user path: ").Append(UserPaths.leftHand).Append(" is_tracked."); DEBUG(sb);

					XrPath path = StringToPath(UserPaths.leftHand);

					if (m_Instance.GetInputSourceName(path, XrInputSourceLocalizedNameFlags.XR_INPUT_SOURCE_LOCALIZED_NAME_USER_PATH_BIT, out string role) != XrResult.XR_SUCCESS)
					{
						sb.Clear().Append(LOG_TAG).Append(func)
							.Append("GetInputSourceName XR_INPUT_SOURCE_LOCALIZED_NAME_USER_PATH_BIT failed."); ERROR(sb);
					}
					else
					{
						sb.Clear().Append(LOG_TAG).Append(func)
							.Append("product: ").Append(description.product)
							.Append(" with user path: ").Append(UserPaths.leftHand).Append(" has role: ").Append(role); DEBUG(sb);
					}

					if (m_Instance.GetInputSourceName(path, XrInputSourceLocalizedNameFlags.XR_INPUT_SOURCE_LOCALIZED_NAME_SERIAL_NUMBER_BIT_HTC, out string sn) != XrResult.XR_SUCCESS)
					{
						sb.Clear().Append(LOG_TAG).Append(func)
							.Append("GetInputSourceName XR_INPUT_SOURCE_LOCALIZED_NAME_SERIAL_NUMBER_BIT_HTC failed."); ERROR(sb);
					}
					else
					{
						sb.Clear().Append(LOG_TAG).Append(func)
							.Append("product: ").Append(description.product)
							.Append(" with user path: ").Append(UserPaths.leftHand).Append(" has serial number: ").Append(role); DEBUG(sb);
					}

					bRoleUpdatedLeft = true;
				}
				if (rightHand.isTracked.ReadValue() > 0 && !bRoleUpdatedRight)
				{
					sb.Clear().Append(LOG_TAG).Append(func)
						.Append("product: ").Append(description.product)
						.Append(" with user path: ").Append(UserPaths.rightHand).Append(" is_tracked."); DEBUG(sb);

					XrPath path = StringToPath(UserPaths.rightHand);

					if (m_Instance.GetInputSourceName(path, XrInputSourceLocalizedNameFlags.XR_INPUT_SOURCE_LOCALIZED_NAME_USER_PATH_BIT, out string role) != XrResult.XR_SUCCESS)
					{
						sb.Clear().Append(LOG_TAG).Append(func)
							.Append("GetInputSourceName XR_INPUT_SOURCE_LOCALIZED_NAME_USER_PATH_BIT failed."); ERROR(sb);
					}
					else
					{
						sb.Clear().Append(LOG_TAG).Append(func)
							.Append("product: ").Append(description.product)
							.Append(" with user path: ").Append(UserPaths.rightHand).Append(" has role: ").Append(role); DEBUG(sb);
					}

					if (m_Instance.GetInputSourceName(path, XrInputSourceLocalizedNameFlags.XR_INPUT_SOURCE_LOCALIZED_NAME_SERIAL_NUMBER_BIT_HTC, out string sn) != XrResult.XR_SUCCESS)
					{
						sb.Clear().Append(LOG_TAG).Append(func)
							.Append("GetInputSourceName XR_INPUT_SOURCE_LOCALIZED_NAME_SERIAL_NUMBER_BIT_HTC failed."); ERROR(sb);
					}
					else
					{
						sb.Clear().Append(LOG_TAG).Append(func)
							.Append("product: ").Append(description.product)
							.Append(" with user path: ").Append(UserPaths.leftHand).Append(" has serial number: ").Append(role); DEBUG(sb);
					}

					bRoleUpdatedRight = true;
				}
			}
		}

		/// <summary>
		/// The interaction profile string used to reference the <a href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_HTC_vive_focus3_controller_interaction">Interaction Profile</a>.
		/// </summary>
		public const string profile = "/interaction_profiles/htc/vive_focus3_controller";

		// Available Bindings
		// Left Hand Only
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/x/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.leftHand"/> user path.
		/// </summary>
		public const string buttonX = "/input/x/click";
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/y/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.leftHand"/> user path.
		/// </summary>
		public const string buttonY = "/input/y/click";
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/menu/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.leftHand"/> user path.
		/// </summary>
		public const string menu = "/input/menu/click";

		// Right Hand Only
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/a/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.rightHand"/> user path.
		/// </summary>
		public const string buttonA = "/input/a/click";
		/// <summary>
		/// Constant for a boolean interaction binding '..."/input/b/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.rightHand"/> user path.
		/// </summary>
		public const string buttonB = "/input/b/click";
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/system/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.rightHand"/> user path.
		/// </summary>
		public const string system = "/input/system/click";

		// Both Hands
		/// <summary>
		/// Constant for a float interaction binding '.../input/squeeze/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string grip = "/input/squeeze/value";
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/squeeze/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string gripPress = "/input/squeeze/click";
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/squeeze/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string gripTouch = "/input/squeeze/touch";
		/// <summary>
		/// Constant for a float interaction binding '.../input/trigger/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string trigger = "/input/trigger/value";
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/trigger/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string triggerClick = "/input/trigger/click";
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/trigger/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string triggerTouch = "/input/trigger/touch";
		/// <summary>
		/// Constant for a Vector2 interaction binding '.../input/thumbstick' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string thumbstick = "/input/thumbstick";
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/thumbstick/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string thumbstickClick = "/input/thumbstick/click";
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/thumbstick/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string thumbstickTouch = "/input/thumbstick/touch";
		/// <summary>
		/// Constant for a boolean interaction binding '.../input/thumbrest/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string thumbrest = "/input/thumbrest/touch";
		/// <summary>
		/// Constant for a hand grip pose interaction binding '.../input/grip/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string gripPose = "/input/grip/pose";
		/// <summary>
		/// Constant for a hand point pose interaction binding '.../input/aim/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
		/// </summary>
		public const string pointerPose = "/input/aim/pose";
		/// <summary>
		/// Constant for a haptic interaction binding '.../output/haptic' OpenXR Input Binding. Used by input subsystem to bind actions to physical outputs.
		/// </summary>
		public const string haptic = "/output/haptic";


		private const string kDeviceLocalizedName = "VIVE Focus 3 Controller OpenXR";

		/// <summary>
		/// Registers the <see cref="VIVEFocus3Controller"/> layout with the Input System.
		/// </summary>
		protected override void RegisterDeviceLayout()
		{
			InputSystem.RegisterLayout(typeof(VIVEFocus3Controller),
						matches: new InputDeviceMatcher()
						.WithInterface(XRUtilities.InterfaceMatchAnyVersion)
						.WithProduct(kDeviceLocalizedName));
		}

		/// <summary>
		/// Removes the <see cref="VIVEFocus3Controller"/> layout from the Input System.
		/// </summary>
		protected override void UnregisterDeviceLayout()
		{
			InputSystem.RemoveLayout(typeof(VIVEFocus3Controller).Name);
		}

		/// <inheritdoc/>
		protected override void RegisterActionMapsWithRuntime()
		{
			ActionMapConfig actionMap = new ActionMapConfig()
			{
				name = "vivefocus3controller",
				localizedName = kDeviceLocalizedName,
				desiredInteractionProfile = profile,
				manufacturer = "HTC",
				serialNumber = "",
				deviceInfos = new List<DeviceConfig>()
				{
					new DeviceConfig()
					{
						characteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left,
						userPath = UserPaths.leftHand // "/user/hand/left"
					},
					new DeviceConfig()
					{
						characteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right,
						userPath = UserPaths.rightHand // "/user/hand/right"
					}
				},
				actions = new List<ActionConfig>()
				{
					// Thumbstick Axis
					new ActionConfig()
					{
						name = "thumbstick",
						localizedName = "Thumbstick Axis",
						type = ActionType.Axis2D,
						usages = new List<string>()
						{
							"Primary2DAxis"
						},
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = thumbstick,
								interactionProfileName = profile,
							}
						}
					},
					// Grip Axis
					new ActionConfig()
					{
						name = "grip",
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
								interactionPath = grip,
								interactionProfileName = profile,
							}
						}
					},
					// Grip Press
					new ActionConfig()
					{
						name = "gripPressed",
						localizedName = "Grip Pressed",
						type = ActionType.Binary,
						usages = new List<string>()
						{
							"GripButton"
						},
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = gripPress,
								interactionProfileName = profile,
							}
						}
					},
					// Grip Touch
					// Known issue: Registering gripTouched will cause Controller Interaction Profile not work.
					/*new ActionConfig()
					{
						name = "gripTouched",
						localizedName = "Grip Touched",
						type = ActionType.Binary,
						usages = new List<string>()
						{
							"GripTouch"
						},
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = gripTouch,
								interactionProfileName = profile,
							}
						}
					},*/
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
								userPaths = new List<string>() { UserPaths.leftHand }
							},
							new ActionBinding()
							{
								interactionPath = system,
								interactionProfileName = profile,
								userPaths = new List<string>() { UserPaths.rightHand }
							},
						}
					},
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
								userPaths = new List<string>() { UserPaths.leftHand }
							},
							new ActionBinding()
							{
								interactionPath = buttonA,
								interactionProfileName = profile,
								userPaths = new List<string>() { UserPaths.rightHand }
							},
						}
					},


					// Y / B Press
					new ActionConfig()
					{
						name = "secondaryButton",
						localizedName = "Secondary Pressed",
						type = ActionType.Binary,
						usages = new List<string>()
						{
							"SecondaryButton"
						},
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = buttonY,
								interactionProfileName = profile,
								userPaths = new List<string>() { UserPaths.leftHand }
							},
							new ActionBinding()
							{
								interactionPath = buttonB,
								interactionProfileName = profile,
								userPaths = new List<string>() { UserPaths.rightHand }
							},
						}
					},


					// Trigger Axis
					new ActionConfig()
					{
						name = "trigger",
						localizedName = "Trigger Axis",
						type = ActionType.Axis1D,
						usages = new List<string>()
						{
							"Trigger"
						},
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = trigger,
								interactionProfileName = profile,
							}
						}
					},
					// Trigger Press
					new ActionConfig()
					{
						name = "triggerPressed",
						localizedName = "Trigger Pressed",
						type = ActionType.Binary,
						usages = new List<string>()
						{
							"TriggerButton"
						},
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = triggerClick,
								interactionProfileName = profile,
							}
						}
					},
					// Trigger Touch
					new ActionConfig()
					{
						name = "triggerTouched",
						localizedName = "Trigger Touched",
						type = ActionType.Binary,
						usages = new List<string>()
						{
							"TriggerTouch"
						},
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = triggerTouch,
								interactionProfileName = profile,
							}
						}
					},
					// Thumbstick Click
					new ActionConfig()
					{
						name = "thumbstickClicked",
						localizedName = "Thumbstick Pressed",
						type = ActionType.Binary,
						usages = new List<string>()
						{
							"Primary2DAxisClick"
						},
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = thumbstickClick,
								interactionProfileName = profile,
							}
						}
					},
					// Thumbstick Touch
					new ActionConfig()
					{
						name = "thumbstickTouched",
						localizedName = "Thumbstick Touched",
						type = ActionType.Binary,
						usages = new List<string>()
						{
							"Primary2DAxisTouch"
						},
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = thumbstickTouch,
								interactionProfileName = profile,
							}
						}
					},
					// Thumbrest Touch
					new ActionConfig()
					{
						name = "parkingTouched",
						localizedName = "Parking Touch",
						type = ActionType.Binary,
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = thumbrest,
								interactionProfileName = profile,
							}
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
								interactionPath = gripPose,
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
					},
					// Haptics
					new ActionConfig()
					{
						name = "haptic",
						localizedName = "Haptic Output",
						type = ActionType.Vibrate,
						usages = new List<string>() { "Haptic" },
						bindings = new List<ActionBinding>()
						{
							new ActionBinding()
							{
								interactionPath = haptic,
								interactionProfileName = profile,
							}
						}
					}
				}
			};

			AddActionMap(actionMap);
		}

		#region OpenXR function delegates
		/// xrGetInstanceProcAddr
		OpenXRHelper.xrGetInstanceProcAddrDelegate XrGetInstanceProcAddr;
		/// xrEnumerateDisplayRefreshRatesFB
		OpenXRHelper.xrGetInputSourceLocalizedNameDelegate xrGetInputSourceLocalizedName = null;
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

		private XrResult GetInputSourceName(XrPath path, XrInputSourceLocalizedNameFlags sourceType, out string sourceName)
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
				if (result == XrResult.XR_SUCCESS)
				{
					sourceName = new string(buffer).TrimEnd('\0');
					sb.Clear().Append(LOG_TAG).Append(func)
						.Append("xrGetInputSourceLocalizedName(").Append(userPath).Append(")")
						.Append(", flag: ").Append((UInt64)sourceType)
						.Append(", bufferCapacityInput: ").Append(nameSizeIn)
						.Append(", bufferCountOutput: ").Append(nameSizeOut)
						.Append(", sourceName: ").Append(sourceName);
					DEBUG(sb);
				}
				else
				{
					sb.Clear().Append(LOG_TAG).Append(func)
						.Append("2.xrGetInputSourceLocalizedName(").Append(userPath).Append(")")
						.Append(", flag: ").Append((UInt64)sourceType)
						.Append(", bufferCapacityInput: ").Append(nameSizeIn)
						.Append(", bufferCountOutput: ").Append(nameSizeOut)
						.Append(" result: ").Append(result);
					ERROR(sb);
				}
			}
			else
			{
				sb.Clear().Append(LOG_TAG).Append(func)
					.Append("1.xrGetInputSourceLocalizedName(").Append(userPath).Append(")")
					.Append(", flag: ").Append((UInt64)sourceType)
					.Append(", bufferCapacityInput: ").Append(nameSizeIn)
					.Append(", bufferCountOutput: ").Append(nameSizeOut)
					.Append(" result: ").Append(result);
				ERROR(sb);
			}

			return result;
		}

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
			if (m_XrSession == xrSession)
			{
				m_XrSession = 0;
				m_XrSessionCreated = false;
			}
		}
		#endregion
	}
}
