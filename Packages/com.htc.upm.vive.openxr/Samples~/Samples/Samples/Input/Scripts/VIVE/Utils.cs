// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.XR;
using System;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VIVE.OpenXR.Samples.OpenXRInput
{
	public static class Utils
	{
		public enum DeviceTypes : UInt32
		{
			HMD = 0,
			ControllerLeft = 1,
			ControllerRight = 2,

			Tracker0 = 10,
			Tracker1 = 11,
			Tracker2 = 12,
			Tracker3 = 13,
			Tracker4 = 14,
			Tracker5 = 15,
			Tracker6 = 16,
			Tracker7 = 17,

			Eye = 3,
		}

		public enum BinaryButtons : UInt32
		{
			menuButton,
			gripButton,
			primaryButton,
			primaryTouch,
			secondaryButton,
			secondaryTouch,
			primary2DAxisClick,
			primary2DAxisTouch,
			triggerButton,
			secondary2DAxisClick,
			secondary2DAxisTouch,
		}
		public static InputFeatureUsage<bool> InputFeature(this BinaryButtons button)
		{
			if (button == BinaryButtons.menuButton) { return UnityEngine.XR.CommonUsages.menuButton; }
			if (button == BinaryButtons.gripButton) { return UnityEngine.XR.CommonUsages.gripButton; }
			if (button == BinaryButtons.primaryButton) { return UnityEngine.XR.CommonUsages.primaryButton; }
			if (button == BinaryButtons.primaryTouch) { return UnityEngine.XR.CommonUsages.primaryTouch; }
			if (button == BinaryButtons.secondaryButton) { return UnityEngine.XR.CommonUsages.secondaryButton; }
			if (button == BinaryButtons.secondaryTouch) { return UnityEngine.XR.CommonUsages.secondaryTouch; }
			if (button == BinaryButtons.primary2DAxisClick) { return UnityEngine.XR.CommonUsages.primary2DAxisClick; }
			if (button == BinaryButtons.secondary2DAxisClick) { return UnityEngine.XR.CommonUsages.secondary2DAxisClick; }
			if (button == BinaryButtons.triggerButton) { return UnityEngine.XR.CommonUsages.triggerButton; }
			if (button == BinaryButtons.primary2DAxisTouch) { return UnityEngine.XR.CommonUsages.primary2DAxisTouch; }

			return UnityEngine.XR.CommonUsages.secondary2DAxisTouch;
		}

		public enum Vector2Buttons : UInt32
		{
			primary2DAxis,
			secondary2DAxis,
		}
		public static InputFeatureUsage<Vector2> InputFeature(this Vector2Buttons button)
		{
			if (button == Vector2Buttons.secondary2DAxis) { return UnityEngine.XR.CommonUsages.secondary2DAxis; }
			return UnityEngine.XR.CommonUsages.primary2DAxis;
		}

		public enum FloatButtons : UInt32
		{
			trigger,
			grip
		}
		public static InputFeatureUsage<float> InputFeature(this FloatButtons button)
		{
			if (button == FloatButtons.grip) { return UnityEngine.XR.CommonUsages.grip; }
			return UnityEngine.XR.CommonUsages.trigger;
		}

#if ENABLE_INPUT_SYSTEM
		public enum ActionRefError : UInt32
		{
			NONE = 0,
			REFERENCE_NULL = 1,
			ACTION_NULL = 2,
			DISABLED = 3,
			ACTIVECONTROL_NULL = 4,
			NO_CONTROLS_COUNT = 5,
		}
		public static string Name(this ActionRefError error)
		{
			if (error == ActionRefError.REFERENCE_NULL) { return "Null reference."; }
			if (error == ActionRefError.ACTION_NULL) { return "Null reference action."; }
			if (error == ActionRefError.DISABLED) { return "Reference action disabled."; }
			if (error == ActionRefError.ACTIVECONTROL_NULL) { return "No active control of the reference action."; }
			if (error == ActionRefError.NO_CONTROLS_COUNT) { return "No action control count."; }

			return "";
		}
		private static ActionRefError VALIDATE(InputActionReference actionReference)
		{
			if (actionReference == null) { return ActionRefError.REFERENCE_NULL; }
			if (actionReference.action == null) { return ActionRefError.ACTION_NULL; }
			if (!actionReference.action.enabled) { return ActionRefError.DISABLED; }
			if (actionReference.action.activeControl == null) { return ActionRefError.ACTIVECONTROL_NULL; }
			else if (actionReference.action.controls.Count <= 0) { return ActionRefError.NO_CONTROLS_COUNT; }

			return ActionRefError.NONE;
		}
		public static bool GetButton(InputActionReference actionReference, out bool value, out string msg)
		{
			var result = VALIDATE(actionReference);

			value = false;
			msg = result.Name();

			if (result == ActionRefError.NONE)
			{
				if (actionReference.action.activeControl.valueType == typeof(float))
					value = actionReference.action.ReadValue<float>() > 0;
				if (actionReference.action.activeControl.valueType == typeof(bool))
					value = actionReference.action.ReadValue<bool>();

				return true;
			}

			return false;
		}
		public static bool GetAnalog(InputActionReference actionReference, out float value, out string msg)
		{
			var result = VALIDATE(actionReference);

			value = 0;
			msg = result.Name();

			if (result == ActionRefError.NONE)
			{
				if (actionReference.action.activeControl.valueType == typeof(float))
					value = actionReference.action.ReadValue<float>();

				return true;
			} else if (result == ActionRefError.ACTIVECONTROL_NULL)
			{
				value = 0;
				return true;
			}

			return false;
		}
		public static bool GetInteger(InputActionReference actionReference, out InputTrackingState value, out string msg)
		{
			var result = VALIDATE(actionReference);

			value = 0;
			msg = result.Name();

			if (result == ActionRefError.NONE)
			{
				if (actionReference.action.activeControl.valueType == typeof(int))
				{
					int diff = 0;
					int i = actionReference.action.ReadValue<int>();

					diff = i & ((int)InputTrackingState.Position);
					if (diff != 0) { value |= InputTrackingState.Position; }

					diff = i & ((int)InputTrackingState.Rotation);
					if (diff != 0) { value |= InputTrackingState.Rotation; }

					diff = i & ((int)InputTrackingState.Velocity);
					if (diff != 0) { value |= InputTrackingState.Velocity; }

					diff = i & ((int)InputTrackingState.AngularVelocity);
					if (diff != 0) { value |= InputTrackingState.AngularVelocity; }

					diff = i & ((int)InputTrackingState.Acceleration);
					if (diff != 0) { value |= InputTrackingState.Acceleration; }

					diff = i & ((int)InputTrackingState.AngularAcceleration);
					if (diff != 0) { value |= InputTrackingState.AngularAcceleration; }
				}

				return true;
			}

			return false;
		}
		public static bool GetVector3(InputActionReference actionReference, out Vector3 value, out string msg)
		{
			var result = VALIDATE(actionReference);

			value = Vector3.zero;
			msg = result.Name();

			if (result == ActionRefError.NONE)
			{
				if (actionReference.action.activeControl.valueType == typeof(Vector3))
					value = actionReference.action.ReadValue<Vector3>();

				return true;
			}

			return false;
		}
		public static bool GetQuaternion(InputActionReference actionReference, out Quaternion value, out string msg)
		{
			var result = VALIDATE(actionReference);

			value = Quaternion.identity;
			msg = result.Name();

			if (result == ActionRefError.NONE)
			{
				if (actionReference.action.activeControl.valueType == typeof(Quaternion))
					value = actionReference.action.ReadValue<Quaternion>();

				Vector3 direction = value * Vector3.forward;
				return true;
			}

			return false;
		}
		public static bool GetPoseIsTracked(InputActionReference actionReference, out bool value, out string msg)
		{
			var result = VALIDATE(actionReference);

			value = false;
			msg = result.Name();

			if (result == ActionRefError.NONE)
			{
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
				if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
				{
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    value = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().isTracked;
#else
					value = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().isTracked;
#endif
					return true;
				}
			}

			return false;
		}
		public static bool GetPoseTrackingState(InputActionReference actionReference, out InputTrackingState value, out string msg)
		{
			var result = VALIDATE(actionReference);

			value = InputTrackingState.None;
			msg = result.Name();

			if (result == ActionRefError.NONE)
			{
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
				if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
				{
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    value = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().trackingState;
#else
					value = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().trackingState;
#endif
					return true;
				}
			}

			return false;
		}
		public static bool GetPosePosition(InputActionReference actionReference, out Vector3 value, out string msg)
		{
			var result = VALIDATE(actionReference);

			value = Vector3.zero;
			msg = result.Name();

			if (result == ActionRefError.NONE)
			{
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
				if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
				{
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    value = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().position;
#else
					value = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().position;
#endif
					return true;
				}
			}

			return false;
		}
		public static bool GetPoseRotation(InputActionReference actionReference, out Quaternion value, out string msg)
		{
			var result = VALIDATE(actionReference);

			value = Quaternion.identity;
			msg = result.Name();

			if (result == ActionRefError.NONE)
			{
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                if (actionReference.action.activeControl.valueType == typeof(UnityEngine.InputSystem.XR.PoseState))
#else
				if (actionReference.action.activeControl.valueType == typeof(UnityEngine.XR.OpenXR.Input.Pose))
#endif
				{
#if USE_INPUT_SYSTEM_POSE_CONTROL // Scripting Define Symbol added by using OpenXR Plugin 1.6.0.
                    value = actionReference.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>().rotation;
#else
					value = actionReference.action.ReadValue<UnityEngine.XR.OpenXR.Input.Pose>().rotation;
#endif
					return true;
				}
			}

			return false;
		}
#endif
	}
}
