// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

using VIVE.OpenXR.Hand;

using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

namespace VIVE.OpenXR.Toolkits.BodyTracking
{
	public enum BodyTrackingResult : Byte
	{
		SUCCESS = 0,
		ERROR_IK_NOT_UPDATED = 1,
		ERROR_INVALID_ARGUMENT = 2,
		ERROR_IK_NOT_DESTROYED = 3,

		ERROR_BODYTRACKINGMODE_NOT_FOUND = 100,
		ERROR_TRACKER_AMOUNT_FAILED = 101,
		ERROR_SKELETONID_NOT_FOUND = 102,
		ERROR_INPUTPOSE_NOT_VALID = 103,
		ERROR_NOT_CALIBRATED = 104,
		ERROR_BODYTRACKINGMODE_NOT_ALIGNED = 105,
		ERROR_AVATAR_INIT_FAILED = 106,
		ERROR_CALIBRATE_FAILED = 107,
		ERROR_COMPUTE_FAILED = 108,
		ERROR_TABLE_STATIC = 109,
		ERROR_SOLVER_NOT_FOUND = 110,
		ERROR_NOT_INITIALIZATION = 111,
		ERROR_JOINT_NOT_FOUND = 112,

		ERROR_FATAL_ERROR = 255,
	}
	public enum DeviceExtRole : UInt64
	{
		Unknown = 0,

		Arm_Wrist = (UInt64)(1 << (Int32)TrackedDeviceRole.ROLE_HEAD
			| 1 << (Int32)TrackedDeviceRole.ROLE_LEFTWRIST | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTWRIST),
		UpperBody_Wrist = (UInt64)(Arm_Wrist | 1 << (Int32)TrackedDeviceRole.ROLE_HIP),
		FullBody_Wrist_Ankle = (UInt64)(UpperBody_Wrist | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTANKLE | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTANKLE),
		FullBody_Wrist_Foot = (UInt64)(UpperBody_Wrist | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTFOOT | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTFOOT),

		Arm_Handheld_Hand = (UInt64)(1 << (Int32)TrackedDeviceRole.ROLE_HEAD
			| 1 << (Int32)TrackedDeviceRole.ROLE_LEFTHAND | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTHAND
			| 1 << (Int32)TrackedDeviceRole.ROLE_LEFTHANDHELD | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTHANDHELD),
		UpperBody_Handheld_Hand = (UInt64)(Arm_Handheld_Hand | 1 << (Int32)TrackedDeviceRole.ROLE_HIP),
		FullBody_Handheld_Hand_Ankle = (UInt64)(UpperBody_Handheld_Hand | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTANKLE | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTANKLE),
		FullBody_Handheld_Hand_Foot = (UInt64)(UpperBody_Handheld_Hand | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTFOOT | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTFOOT),

		UpperBody_Handheld_Hand_Knee_Ankle = (UInt64)(UpperBody_Handheld_Hand
			| 1 << (Int32)TrackedDeviceRole.ROLE_LEFTKNEE | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTKNEE
			| 1 << (Int32)TrackedDeviceRole.ROLE_LEFTANKLE | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTANKLE),

		// Total 9 Device Extrinsic Roles.
	}
	public enum BodyPoseRole : UInt64
	{
		Unknown = 0,

		// Using Tracker
		Arm_Wrist = (UInt64)(1 << (Int32)TrackedDeviceRole.ROLE_HEAD | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTWRIST | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTWRIST),
		UpperBody_Wrist = (UInt64)(Arm_Wrist | 1 << (Int32)TrackedDeviceRole.ROLE_HIP),
		FullBody_Wrist_Ankle = (UInt64)(UpperBody_Wrist | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTANKLE | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTANKLE),
		FullBody_Wrist_Foot = (UInt64)(UpperBody_Wrist | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTFOOT | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTFOOT),

		// Using Controller
		Arm_Handheld = (UInt64)(1 << (Int32)TrackedDeviceRole.ROLE_HEAD | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTHANDHELD | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTHANDHELD),
		UpperBody_Handheld = (UInt64)(Arm_Handheld | 1 << (Int32)TrackedDeviceRole.ROLE_HIP),
		FullBody_Handheld_Ankle = (UInt64)(UpperBody_Handheld | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTANKLE | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTANKLE),
		FullBody_Handheld_Foot = (UInt64)(UpperBody_Handheld | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTFOOT | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTFOOT),

		// Using Natural Hand
		Arm_Hand = (UInt64)(1 << (Int32)TrackedDeviceRole.ROLE_HEAD | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTHAND | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTHAND),
		UpperBody_Hand = (UInt64)(Arm_Hand | 1 << (Int32)TrackedDeviceRole.ROLE_HIP),
		FullBody_Hand_Ankle = (UInt64)(UpperBody_Hand | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTANKLE | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTANKLE),
		FullBody_Hand_Foot = (UInt64)(UpperBody_Hand | 1 << (Int32)TrackedDeviceRole.ROLE_LEFTFOOT | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTFOOT),

		// Head + Controller/Hand + Hip + Knee + Ankle
		UpperBody_Handheld_Knee_Ankle = (UInt64)(UpperBody_Handheld
			| 1 << ((Int32)TrackedDeviceRole.ROLE_LEFTKNEE) | 1 << ((Int32)TrackedDeviceRole.ROLE_RIGHTKNEE)
			| 1 << (Int32)TrackedDeviceRole.ROLE_LEFTANKLE | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTANKLE),
		UpperBody_Hand_Knee_Ankle = (UInt64)(UpperBody_Hand
			| 1 << ((Int32)TrackedDeviceRole.ROLE_LEFTKNEE) | 1 << ((Int32)TrackedDeviceRole.ROLE_RIGHTKNEE)
			| 1 << (Int32)TrackedDeviceRole.ROLE_LEFTANKLE | 1 << (Int32)TrackedDeviceRole.ROLE_RIGHTANKLE),

		// Total 14 Body Pose Roles.
	}

	public struct TransformData
	{
		public Vector3 position;
		public Vector3 localPosition;
		public Quaternion rotation;
		public Quaternion localRotation;
		public Vector3 localScale;

		public TransformData(Vector3 in_pos, Vector3 in_localPos, Quaternion in_rot, Quaternion in_localRot, Vector3 in_scale)
		{
			position = in_pos;
			localPosition = in_localPos;

			rotation = in_rot;
			BodyTrackingUtils.Validate(ref rotation);
			localRotation = in_localRot;
			BodyTrackingUtils.Validate(ref localRotation);

			localScale = in_scale;
		}
		public TransformData(Transform trans)
		{
			position = trans.position;
			localPosition = trans.localPosition;

			rotation = trans.rotation;
			BodyTrackingUtils.Validate(ref rotation);
			localRotation = trans.localRotation;
			BodyTrackingUtils.Validate(ref localRotation);

			localScale = trans.localScale;
		}
		public static TransformData identity {
			get {
				return new TransformData(Vector3.zero, Vector3.zero, Quaternion.identity, Quaternion.identity, Vector3.zero);
			}
		}
		public void Update(Transform trans)
		{
			if (trans == null) { return; }
			position = trans.position;
			localPosition = trans.localPosition;
			BodyTrackingUtils.Update(ref rotation, trans.rotation);
			BodyTrackingUtils.Update(ref localRotation, trans.localRotation);
			localScale = trans.localScale;
		}
		public void Update(ref Transform trans)
		{
			if (trans == null) { return; }
			trans.position = position;
			trans.localPosition = localPosition;
			trans.rotation = rotation;
			trans.localRotation = localRotation;
			trans.localScale = localScale;
		}
	}
	public class BodyAvatar
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.BodyAvatar";
		StringBuilder m_sb = null;
		StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Debug.Log(msg); }

		public float height = 0;

		public Joint hip = Joint.identity;

		public Joint leftThigh = Joint.identity;
		public Joint leftLeg = Joint.identity;
		public Joint leftAnkle = Joint.identity;
		public Joint leftFoot = Joint.identity;

		public Joint rightThigh = Joint.identity;
		public Joint rightLeg = Joint.identity;
		public Joint rightAnkle = Joint.identity;
		public Joint rightFoot = Joint.identity;

		public Joint waist = Joint.identity;

		public Joint spineLower = Joint.identity;
		public Joint spineMiddle = Joint.identity;
		public Joint spineHigh = Joint.identity;

		public Joint chest = Joint.identity;
		public Joint neck = Joint.identity;
		public Joint head = Joint.identity;

		public Joint leftClavicle = Joint.identity;
		public Joint leftScapula = Joint.identity;
		public Joint leftUpperarm = Joint.identity;
		public Joint leftForearm = Joint.identity;
		public Joint leftHand = Joint.identity;

		public Joint rightClavicle = Joint.identity;
		public Joint rightScapula = Joint.identity;
		public Joint rightUpperarm = Joint.identity;
		public Joint rightForearm = Joint.identity;
		public Joint rightHand = Joint.identity;

		public float scale = 1;

		private Joint[] s_AvatarJoints = null;
		private void UpdateJoints()
		{
			if (s_AvatarJoints == null || s_AvatarJoints.Length <= 0) { return; }

			int jointCount = 0;
			s_AvatarJoints[jointCount++].Update(hip);

			s_AvatarJoints[jointCount++].Update(leftThigh);
			s_AvatarJoints[jointCount++].Update(leftLeg);
			s_AvatarJoints[jointCount++].Update(leftAnkle);
			s_AvatarJoints[jointCount++].Update(leftFoot);

			s_AvatarJoints[jointCount++].Update(rightThigh);
			s_AvatarJoints[jointCount++].Update(rightLeg);
			s_AvatarJoints[jointCount++].Update(rightAnkle);
			s_AvatarJoints[jointCount++].Update(rightFoot);

			s_AvatarJoints[jointCount++].Update(waist);

			s_AvatarJoints[jointCount++].Update(spineLower);
			s_AvatarJoints[jointCount++].Update(spineMiddle);
			s_AvatarJoints[jointCount++].Update(spineHigh);

			s_AvatarJoints[jointCount++].Update(chest);
			s_AvatarJoints[jointCount++].Update(neck);
			s_AvatarJoints[jointCount++].Update(head);

			s_AvatarJoints[jointCount++].Update(leftClavicle);
			s_AvatarJoints[jointCount++].Update(leftScapula);
			s_AvatarJoints[jointCount++].Update(leftUpperarm);
			s_AvatarJoints[jointCount++].Update(leftForearm);
			s_AvatarJoints[jointCount++].Update(leftHand);

			s_AvatarJoints[jointCount++].Update(rightClavicle);
			s_AvatarJoints[jointCount++].Update(rightScapula);
			s_AvatarJoints[jointCount++].Update(rightUpperarm);
			s_AvatarJoints[jointCount++].Update(rightForearm);
			s_AvatarJoints[jointCount++].Update(rightHand);
		}
		public BodyAvatar()
		{
			int jointCount = 0;

			height = 0;
			// Joint initialization
			{
				hip.jointType = JointType.HIP; jointCount++;

				leftThigh.jointType = JointType.LEFTTHIGH; jointCount++;
				leftLeg.jointType = JointType.LEFTLEG; jointCount++;
				leftAnkle.jointType = JointType.LEFTANKLE; jointCount++;
				leftFoot.jointType = JointType.LEFTFOOT; jointCount++; // 5

				rightThigh.jointType = JointType.RIGHTTHIGH; jointCount++;
				rightLeg.jointType = JointType.RIGHTLEG; jointCount++;
				rightAnkle.jointType = JointType.RIGHTANKLE; jointCount++;
				rightFoot.jointType = JointType.RIGHTFOOT; jointCount++;

				waist.jointType = JointType.WAIST; jointCount++; // 10

				spineLower.jointType = JointType.SPINELOWER; jointCount++;
				spineMiddle.jointType = JointType.SPINEMIDDLE; jointCount++;
				spineHigh.jointType = JointType.SPINEHIGH; jointCount++;

				chest.jointType = JointType.CHEST; jointCount++;
				neck.jointType = JointType.NECK; jointCount++; // 15
				head.jointType = JointType.HEAD; jointCount++;

				leftClavicle.jointType = JointType.LEFTCLAVICLE; jointCount++;
				leftScapula.jointType = JointType.LEFTSCAPULA; jointCount++;
				leftUpperarm.jointType = JointType.LEFTUPPERARM; jointCount++;
				leftForearm.jointType = JointType.LEFTFOREARM; jointCount++; // 20
				leftHand.jointType = JointType.LEFTHAND; jointCount++;

				rightClavicle.jointType = JointType.RIGHTCLAVICLE; jointCount++;
				rightScapula.jointType = JointType.RIGHTSCAPULA; jointCount++;
				rightUpperarm.jointType = JointType.RIGHTUPPERARM; jointCount++;
				rightForearm.jointType = JointType.RIGHTFOREARM; jointCount++; // 25
				rightHand.jointType = JointType.RIGHTHAND; jointCount++;
			}
			scale = 1;

			s_AvatarJoints = new Joint[jointCount];
		}
		public void Update(BodyAvatar in_avatar)
		{
			if (in_avatar == null) { return; }

			height = in_avatar.height;

			head.Update(in_avatar.head);
			neck.Update(in_avatar.neck);
			chest.Update(in_avatar.chest);
			waist.Update(in_avatar.waist);
			hip.Update(in_avatar.hip);

			leftClavicle.Update(in_avatar.leftClavicle);
			leftScapula.Update(in_avatar.leftScapula);
			leftUpperarm.Update(in_avatar.leftUpperarm);
			leftForearm.Update(in_avatar.leftForearm);
			leftHand.Update(in_avatar.leftHand);

			leftThigh.Update(in_avatar.leftThigh);
			leftLeg.Update(in_avatar.leftLeg);
			leftAnkle.Update(in_avatar.leftAnkle);
			leftFoot.Update(in_avatar.leftFoot);

			rightClavicle.Update(in_avatar.rightClavicle);
			rightScapula.Update(in_avatar.rightScapula);
			rightUpperarm.Update(in_avatar.rightUpperarm);
			rightForearm.Update(in_avatar.rightForearm);
			rightHand.Update(in_avatar.rightHand);

			rightThigh.Update(in_avatar.rightThigh);
			rightLeg.Update(in_avatar.rightLeg);
			rightAnkle.Update(in_avatar.rightAnkle);
			rightFoot.Update(in_avatar.rightFoot);

			scale = in_avatar.scale;
		}
		public void Update(Joint joint)
		{
			if (joint.jointType == JointType.HIP) { hip.Update(joint); }

			if (joint.jointType == JointType.LEFTTHIGH) { leftThigh.Update(joint); }
			if (joint.jointType == JointType.LEFTLEG) { leftLeg.Update(joint); }
			if (joint.jointType == JointType.LEFTANKLE) { leftAnkle.Update(joint); }
			if (joint.jointType == JointType.LEFTFOOT) { leftFoot.Update(joint); } // 5

			if (joint.jointType == JointType.RIGHTTHIGH) { rightThigh.Update(joint); }
			if (joint.jointType == JointType.RIGHTLEG) { rightLeg.Update(joint); }
			if (joint.jointType == JointType.RIGHTANKLE) { rightAnkle.Update(joint); }
			if (joint.jointType == JointType.RIGHTFOOT) { rightFoot.Update(joint); }

			if (joint.jointType == JointType.WAIST) { waist.Update(joint); } // 10

			if (joint.jointType == JointType.SPINELOWER) { spineLower.Update(joint); }
			if (joint.jointType == JointType.SPINEMIDDLE) { spineMiddle.Update(joint); }
			if (joint.jointType == JointType.SPINEHIGH) { spineHigh.Update(joint); }

			if (joint.jointType == JointType.CHEST) { chest.Update(joint); }
			if (joint.jointType == JointType.NECK) { neck.Update(joint); } // 15
			if (joint.jointType == JointType.HEAD) { head.Update(joint); }

			if (joint.jointType == JointType.LEFTCLAVICLE) { leftClavicle.Update(joint); }
			if (joint.jointType == JointType.LEFTSCAPULA) { leftScapula.Update(joint); }
			if (joint.jointType == JointType.LEFTUPPERARM) { leftUpperarm.Update(joint); }
			if (joint.jointType == JointType.LEFTFOREARM) { leftForearm.Update(joint); } // 20
			if (joint.jointType == JointType.LEFTHAND) { leftHand.Update(joint); }

			if (joint.jointType == JointType.RIGHTCLAVICLE) { rightClavicle.Update(joint); }
			if (joint.jointType == JointType.RIGHTSCAPULA) { rightScapula.Update(joint); }
			if (joint.jointType == JointType.RIGHTUPPERARM) { rightUpperarm.Update(joint); }
			if (joint.jointType == JointType.RIGHTFOREARM) { rightForearm.Update(joint); } // 25
			if (joint.jointType == JointType.RIGHTHAND) { rightHand.Update(joint); }
		}

		private void Update([In] Transform trans, [In] Vector3 velocity, [In] Vector3 angularVelocity, ref Joint joint)
		{
			if (trans == null) { return; }
			joint.poseState = (PoseState.ROTATION | PoseState.TRANSLATION);
			joint.translation = trans.position;
			BodyTrackingUtils.Update(ref joint.rotation, trans.rotation);
			joint.velocity = velocity;
			joint.angularVelocity = angularVelocity;
			//sb.Clear().Append(LOG_TAG).Append("Update() ").Append(joint.Log()); DEBUG(sb);
		}
		public void Update(JointType jointType, Transform trans, Vector3 velocity, Vector3 angularVelocity)
		{
			if (trans == null) { return; }
			if (jointType == JointType.HEAD) { Update(trans, velocity, angularVelocity, ref head); }
			if (jointType == JointType.NECK) { Update(trans, velocity, angularVelocity, ref neck); }
			if (jointType == JointType.CHEST) { Update(trans, velocity, angularVelocity, ref chest); }
			if (jointType == JointType.WAIST) { Update(trans, velocity, angularVelocity, ref waist); }
			if (jointType == JointType.HIP) { Update(trans, velocity, angularVelocity, ref hip); }

			if (jointType == JointType.LEFTCLAVICLE) { Update(trans, velocity, angularVelocity, ref leftClavicle); }
			if (jointType == JointType.LEFTSCAPULA) { Update(trans, velocity, angularVelocity, ref leftScapula); }
			if (jointType == JointType.LEFTUPPERARM) { Update(trans, velocity, angularVelocity, ref leftUpperarm); }
			if (jointType == JointType.LEFTFOREARM) { Update(trans, velocity, angularVelocity, ref leftForearm); }
			if (jointType == JointType.LEFTHAND) { Update(trans, velocity, angularVelocity, ref leftHand); }

			if (jointType == JointType.LEFTTHIGH) { Update(trans, velocity, angularVelocity, ref leftThigh); }
			if (jointType == JointType.LEFTLEG) { Update(trans, velocity, angularVelocity, ref leftLeg); }
			if (jointType == JointType.LEFTANKLE) { Update(trans, velocity, angularVelocity, ref leftAnkle); }
			if (jointType == JointType.LEFTFOOT) { Update(trans, velocity, angularVelocity, ref leftFoot); }

			if (jointType == JointType.RIGHTCLAVICLE) { Update(trans, velocity, angularVelocity, ref rightClavicle); }
			if (jointType == JointType.RIGHTSCAPULA) { Update(trans, velocity, angularVelocity, ref rightScapula); }
			if (jointType == JointType.RIGHTUPPERARM) { Update(trans, velocity, angularVelocity, ref rightUpperarm); }
			if (jointType == JointType.RIGHTFOREARM) { Update(trans, velocity, angularVelocity, ref rightForearm); }
			if (jointType == JointType.RIGHTHAND) { Update(trans, velocity, angularVelocity, ref rightHand); }

			if (jointType == JointType.RIGHTTHIGH) { Update(trans, velocity, angularVelocity, ref rightThigh); }
			if (jointType == JointType.RIGHTLEG) { Update(trans, velocity, angularVelocity, ref rightLeg); }
			if (jointType == JointType.RIGHTANKLE) { Update(trans, velocity, angularVelocity, ref rightAnkle); }
			if (jointType == JointType.RIGHTFOOT) { Update(trans, velocity, angularVelocity, ref rightFoot); }

			if (jointType == JointType.SPINELOWER) { Update(trans, velocity, angularVelocity, ref spineLower); }
			if (jointType == JointType.SPINEMIDDLE) { Update(trans, velocity, angularVelocity, ref spineMiddle); }
			if (jointType == JointType.SPINEHIGH) { Update(trans, velocity, angularVelocity, ref spineHigh); }
		}
		private void Update([In] Transform trans, [In] Vector3 velocity, ref Joint joint)
		{
			if (trans == null) { return; }
			joint.translation = trans.transform.position;
			BodyTrackingUtils.Update(ref joint.rotation, trans.transform.rotation);
			joint.poseState = (PoseState.ROTATION | PoseState.TRANSLATION);
			joint.velocity = velocity;
			//sb.Clear().Append(LOG_TAG).Append("Update() ").Append(joint.Log()); DEBUG(sb);
		}
		public void Update(JointType jointType, Transform trans, Vector3 velocity)
		{
			if (trans == null) { return; }
			if (jointType == JointType.HEAD) { Update(trans, velocity, ref head); }
			if (jointType == JointType.NECK) { Update(trans, velocity, ref neck); }
			if (jointType == JointType.CHEST) { Update(trans, velocity, ref chest); }
			if (jointType == JointType.WAIST) { Update(trans, velocity, ref waist); }
			if (jointType == JointType.HIP) { Update(trans, velocity, ref hip); }

			if (jointType == JointType.LEFTCLAVICLE) { Update(trans, velocity, ref leftClavicle); }
			if (jointType == JointType.LEFTSCAPULA) { Update(trans, velocity, ref leftScapula); }
			if (jointType == JointType.LEFTUPPERARM) { Update(trans, velocity, ref leftUpperarm); }
			if (jointType == JointType.LEFTFOREARM) { Update(trans, velocity, ref leftForearm); }
			if (jointType == JointType.LEFTHAND) { Update(trans, velocity, ref leftHand); }

			if (jointType == JointType.LEFTTHIGH) { Update(trans, velocity, ref leftThigh); }
			if (jointType == JointType.LEFTLEG) { Update(trans, velocity, ref leftLeg); }
			if (jointType == JointType.LEFTANKLE) { Update(trans, velocity, ref leftAnkle); }
			if (jointType == JointType.LEFTFOOT) { Update(trans, velocity, ref leftFoot); }

			if (jointType == JointType.RIGHTCLAVICLE) { Update(trans, velocity, ref rightClavicle); }
			if (jointType == JointType.RIGHTSCAPULA) { Update(trans, velocity, ref rightScapula); }
			if (jointType == JointType.RIGHTUPPERARM) { Update(trans, velocity, ref rightUpperarm); }
			if (jointType == JointType.RIGHTFOREARM) { Update(trans, velocity, ref rightForearm); }
			if (jointType == JointType.RIGHTHAND) { Update(trans, velocity, ref rightHand); }

			if (jointType == JointType.RIGHTTHIGH) { Update(trans, velocity, ref rightThigh); }
			if (jointType == JointType.RIGHTLEG) { Update(trans, velocity, ref rightLeg); }
			if (jointType == JointType.RIGHTANKLE) { Update(trans, velocity, ref rightAnkle); }
			if (jointType == JointType.RIGHTFOOT) { Update(trans, velocity, ref rightFoot); }

			if (jointType == JointType.SPINELOWER) { Update(trans, velocity, ref spineLower); }
			if (jointType == JointType.SPINEMIDDLE) { Update(trans, velocity, ref spineMiddle); }
			if (jointType == JointType.SPINEHIGH) { Update(trans, velocity, ref spineHigh); }
		}
		private void Update([In] Transform trans, ref Joint joint)
		{
			if (trans == null) { return; }
			joint.translation = trans.position;
			BodyTrackingUtils.Update(ref joint.rotation, trans.rotation);
			joint.poseState = (PoseState.ROTATION | PoseState.TRANSLATION);
			//sb.Clear().Append(LOG_TAG).Append("Update() ").Append(joint.Log()); DEBUG(sb);
		}
		public void Update(JointType jointType, Transform trans)
		{
			if (trans == null) { return; }
			if (jointType == JointType.HEAD) { Update(trans, ref head); }
			if (jointType == JointType.NECK) { Update(trans, ref neck); }
			if (jointType == JointType.CHEST) { Update(trans, ref chest); }
			if (jointType == JointType.WAIST) { Update(trans, ref waist); }
			if (jointType == JointType.HIP) { Update(trans, ref hip); }

			if (jointType == JointType.LEFTCLAVICLE) { Update(trans, ref leftClavicle); }
			if (jointType == JointType.LEFTSCAPULA) { Update(trans, ref leftScapula); }
			if (jointType == JointType.LEFTUPPERARM) { Update(trans, ref leftUpperarm); }
			if (jointType == JointType.LEFTFOREARM) { Update(trans, ref leftForearm); }
			if (jointType == JointType.LEFTHAND) { Update(trans, ref leftHand); }

			if (jointType == JointType.LEFTTHIGH) { Update(trans, ref leftThigh); }
			if (jointType == JointType.LEFTLEG) { Update(trans, ref leftLeg); }
			if (jointType == JointType.LEFTANKLE) { Update(trans, ref leftAnkle); }
			if (jointType == JointType.LEFTFOOT) { Update(trans, ref leftFoot); }

			if (jointType == JointType.RIGHTCLAVICLE) { Update(trans, ref rightClavicle); }
			if (jointType == JointType.RIGHTSCAPULA) { Update(trans, ref rightScapula); }
			if (jointType == JointType.RIGHTUPPERARM) { Update(trans, ref rightUpperarm); }
			if (jointType == JointType.RIGHTFOREARM) { Update(trans, ref rightForearm); }
			if (jointType == JointType.RIGHTHAND) { Update(trans, ref rightHand); }

			if (jointType == JointType.RIGHTTHIGH) { Update(trans, ref rightThigh); }
			if (jointType == JointType.RIGHTLEG) { Update(trans, ref rightLeg); }
			if (jointType == JointType.RIGHTANKLE) { Update(trans, ref rightAnkle); }
			if (jointType == JointType.RIGHTFOOT) { Update(trans, ref rightFoot); }

			if (jointType == JointType.SPINELOWER) { Update(trans, ref spineLower); }
			if (jointType == JointType.SPINEMIDDLE) { Update(trans, ref spineMiddle); }
			if (jointType == JointType.SPINEHIGH) { Update(trans, ref spineHigh); }
		}
		private void Update([In] Joint joint, ref Transform trans, float scale = 1)
		{
			if (trans == null) { return; }
			if (joint.poseState.HasFlag(PoseState.TRANSLATION)) { trans.position = joint.translation * scale; }
			if (joint.poseState.HasFlag(PoseState.ROTATION))
			{
				trans.rotation = joint.rotation;
				if (trans.rotation.isZero()) { trans.rotation = Quaternion.identity; }
			}
		}
		public void Update([In] JointType jointType, ref Transform trans, float scale = 1)
		{
			if (trans == null) { return; }
			if (jointType == JointType.HEAD) { Update(head, ref trans, scale); }
			if (jointType == JointType.NECK) { Update(neck, ref trans, scale); }
			if (jointType == JointType.CHEST) { Update(chest, ref trans, scale); }
			if (jointType == JointType.WAIST) { Update(waist, ref trans, scale); }
			if (jointType == JointType.HIP) { Update(hip, ref trans, scale); }

			if (jointType == JointType.LEFTCLAVICLE) { Update(leftClavicle, ref trans, scale); }
			if (jointType == JointType.LEFTSCAPULA) { Update(leftScapula, ref trans, scale); }
			if (jointType == JointType.LEFTUPPERARM) { Update(leftUpperarm, ref trans, scale); }
			if (jointType == JointType.LEFTFOREARM) { Update(leftForearm, ref trans, scale); }
			if (jointType == JointType.LEFTHAND) { Update(leftHand, ref trans, scale); }

			if (jointType == JointType.LEFTTHIGH) { Update(leftThigh, ref trans, scale); }
			if (jointType == JointType.LEFTLEG) { Update(leftLeg, ref trans, scale); }
			if (jointType == JointType.LEFTANKLE) { Update(leftAnkle, ref trans, scale); }
			if (jointType == JointType.LEFTFOOT) { Update(leftFoot, ref trans, scale); }

			if (jointType == JointType.RIGHTCLAVICLE) { Update(rightClavicle, ref trans, scale); }
			if (jointType == JointType.RIGHTSCAPULA) { Update(rightScapula, ref trans, scale); }
			if (jointType == JointType.RIGHTUPPERARM) { Update(rightUpperarm, ref trans, scale); }
			if (jointType == JointType.RIGHTFOREARM) { Update(rightForearm, ref trans, scale); }
			if (jointType == JointType.RIGHTHAND) { Update(rightHand, ref trans, scale); }

			if (jointType == JointType.RIGHTTHIGH) { Update(rightThigh, ref trans, scale); }
			if (jointType == JointType.RIGHTLEG) { Update(rightLeg, ref trans, scale); }
			if (jointType == JointType.RIGHTANKLE) { Update(rightAnkle, ref trans, scale); }
			if (jointType == JointType.RIGHTFOOT) { Update(rightFoot, ref trans, scale); }

			if (jointType == JointType.SPINELOWER) { Update(spineLower, ref trans, scale); }
			if (jointType == JointType.SPINEMIDDLE) { Update(spineMiddle, ref trans, scale); }
			if (jointType == JointType.SPINEHIGH) { Update(spineHigh, ref trans, scale); }
		}

		public void Update([In] Body body)
		{
			if (body == null) { return; }

			Update(JointType.HIP, body.root); // 0

			Update(JointType.LEFTTHIGH, body.leftThigh);
			Update(JointType.LEFTLEG, body.leftLeg);
			Update(JointType.LEFTANKLE, body.leftAnkle);
			Update(JointType.LEFTFOOT, body.leftFoot);

			Update(JointType.RIGHTTHIGH, body.rightThigh); // 5
			Update(JointType.RIGHTLEG, body.rightLeg);
			Update(JointType.RIGHTANKLE, body.rightAnkle);
			Update(JointType.RIGHTFOOT, body.rightFoot);

			Update(JointType.WAIST, body.waist);

			Update(JointType.SPINELOWER, body.spineLower); // 10
			Update(JointType.SPINEMIDDLE, body.spineMiddle);
			Update(JointType.SPINEHIGH, body.spineHigh);

			Update(JointType.CHEST, body.chest);
			Update(JointType.NECK, body.neck);
			Update(JointType.HEAD, body.head); // 15

			Update(JointType.LEFTCLAVICLE, body.leftClavicle);
			Update(JointType.LEFTSCAPULA, body.leftScapula);
			Update(JointType.LEFTUPPERARM, body.leftUpperarm);
			Update(JointType.LEFTFOREARM, body.leftForearm);
			Update(JointType.LEFTHAND, body.leftHand); // 20

			Update(JointType.RIGHTCLAVICLE, body.rightClavicle);
			Update(JointType.RIGHTSCAPULA, body.rightScapula);
			Update(JointType.RIGHTUPPERARM, body.rightUpperarm);
			Update(JointType.RIGHTFOREARM, body.rightForearm);
			Update(JointType.RIGHTHAND, body.rightHand);

			height = body.height;
		}
		/// <summary>
		/// Update full body poses. Note that your avatar should have joints in specified order.
		/// E.g. You avatar's toe should be the child of foot and the foot should be the child of leg.
		/// </summary>
		/// <param name="body">Reference to the avatar body.</param>
		public void Update(ref Body body)
		{
			if (body == null) { return; }

			body.height = height;

			if (body.root != null) Update(JointType.HIP, ref body.root); // 0

			if (body.leftThigh != null) Update(JointType.LEFTTHIGH, ref body.leftThigh);
			if (body.leftLeg != null) Update(JointType.LEFTLEG, ref body.leftLeg);
			if (body.leftAnkle != null) Update(JointType.LEFTANKLE, ref body.leftAnkle);
			if (body.leftFoot != null) Update(JointType.LEFTFOOT, ref body.leftFoot);

			if (body.rightThigh != null) Update(JointType.RIGHTTHIGH, ref body.rightThigh); // 5
			if (body.rightLeg != null) Update(JointType.RIGHTLEG, ref body.rightLeg);
			if (body.rightAnkle != null) Update(JointType.RIGHTANKLE, ref body.rightAnkle);
			if (body.rightFoot != null) Update(JointType.RIGHTFOOT, ref body.rightFoot);

			if (body.waist != null) Update(JointType.WAIST, ref body.waist);

			if (body.spineLower != null) Update(JointType.SPINELOWER, ref body.spineLower); // 10
			if (body.spineMiddle != null) Update(JointType.SPINEMIDDLE, ref body.spineMiddle);
			if (body.spineHigh != null) Update(JointType.SPINEHIGH, ref body.spineHigh);

			if (body.chest != null) Update(JointType.CHEST, ref body.chest);
			if (body.neck != null) Update(JointType.NECK, ref body.neck);
			if (body.head != null) Update(JointType.HEAD, ref body.head); // 15

			if (body.leftClavicle != null) Update(JointType.LEFTCLAVICLE, ref body.leftClavicle);
			if (body.leftScapula != null) Update(JointType.LEFTSCAPULA, ref body.leftScapula);
			if (body.leftUpperarm != null) Update(JointType.LEFTUPPERARM, ref body.leftUpperarm);
			if (body.leftForearm != null) Update(JointType.LEFTFOREARM, ref body.leftForearm);
			if (body.leftHand != null) Update(JointType.LEFTHAND, ref body.leftHand); // 20

			if (body.rightClavicle != null) Update(JointType.RIGHTCLAVICLE, ref body.rightClavicle);
			if (body.rightScapula != null) Update(JointType.RIGHTSCAPULA, ref body.rightScapula);
			if (body.rightUpperarm != null) Update(JointType.RIGHTUPPERARM, ref body.rightUpperarm);
			if (body.rightForearm != null) Update(JointType.RIGHTFOREARM, ref body.rightForearm);
			if (body.rightHand != null) Update(JointType.RIGHTHAND, ref body.rightHand); // 25
		}

		private List<Joint> joints = new List<Joint>();
		public bool GetJoints(out Joint[] avatarJoints, out UInt32 avatarJointCount, bool is6DoF = false)
		{
			if (!is6DoF) // including NODATA joints.
			{
				UpdateJoints();
				avatarJoints = s_AvatarJoints;
				avatarJointCount = (UInt32)(avatarJoints.Length & 0x7FFFFFFF);
				return true;
			}

			avatarJoints = null;
			avatarJointCount = 0;

			joints.Clear();
			if (hip.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(hip); }

			if (leftThigh.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(leftThigh); }
			if (leftLeg.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(leftLeg); }
			if (leftAnkle.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(leftAnkle); }
			if (leftFoot.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(leftFoot); }

			if (rightThigh.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(rightThigh); }
			if (rightLeg.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(rightLeg); }
			if (rightAnkle.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(rightAnkle); }
			if (rightFoot.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(rightFoot); }

			if (waist.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(waist); }

			if (spineLower.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(spineLower); }
			if (spineMiddle.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(spineMiddle); }
			if (spineHigh.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(spineHigh); }

			if (chest.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(chest); }
			if (neck.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(neck); }
			if (head.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(head); }

			if (leftClavicle.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(leftClavicle); }
			if (leftScapula.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(leftScapula); }
			if (leftUpperarm.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(leftUpperarm); }
			if (leftForearm.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(leftForearm); }
			if (leftHand.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(leftHand); }

			if (rightClavicle.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(rightClavicle); }
			if (rightScapula.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(rightScapula); }
			if (rightUpperarm.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(rightUpperarm); }
			if (rightForearm.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(rightForearm); }
			if (rightHand.poseState == (PoseState.ROTATION | PoseState.TRANSLATION)) { joints.Add(rightHand); }

			if (joints.Count > 0)
			{
				avatarJoints = joints.ToArray();
				avatarJointCount = (UInt32)(joints.Count & 0x7FFFFFFF);
				return true;
			}

			return false;
		}
		public void Set6DoFJoints(Joint[] avatarJoints, UInt32 avatarJointCount)
		{
			for (UInt32 i = 0; i < avatarJointCount; i++)
			{
				Update(avatarJoints[i]);
			}
		}
	}

	[Serializable]
	public struct ExtrinsicVector4_t
	{
		public Vector3 translation;
		[SerializeField]
		private Vector4 m_rotation;
		public Vector4 rotation {
			get {
				if (m_rotation == Vector4.zero) { m_rotation.w = 1; }
				return m_rotation;
			}
			set { m_rotation = value; }
		}

		private Extrinsic ext;
		private void UpdateExtrinsic()
		{
			ext.translation = translation;
			BodyTrackingUtils.Update(rotation, ref ext.rotation);
		}
		public Extrinsic GetExtrinsic()
		{
			UpdateExtrinsic();
			return ext;
		}

		public ExtrinsicVector4_t(Vector3 in_tra, Vector4 in_rot)
		{
			translation = in_tra;
			m_rotation = in_rot;

			ext = Extrinsic.identity;
			UpdateExtrinsic();
		}
		public static ExtrinsicVector4_t identity {
			get {
				return new ExtrinsicVector4_t(Vector3.zero, new Vector4(0, 0, 0, 1));
			}
		}
		public void Update(ExtrinsicVector4_t in_ext)
		{
			translation = in_ext.translation;
			rotation = in_ext.rotation;
		}
		public void Update(Extrinsic in_ext)
		{
			translation = in_ext.translation;
			BodyTrackingUtils.Update(in_ext.rotation, ref m_rotation);
		}

		public string Log()
		{
			string log = "position(" + translation.x.ToString() + ", " + translation.y.ToString() + ", " + translation.z.ToString() + ")";
			log += ", rotation(" + rotation.x.ToString() + ", " + rotation.y.ToString() + ", " + rotation.z.ToString() + ", " + rotation.w.ToString() + ")";
			return log;
		}
	}
	[Serializable]
	public struct ExtrinsicInfo_t
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.ExtrinsicInfo_t";
		static StringBuilder m_sb = null;
		static StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Debug.Log(msg); }

		public bool isTracking;
		public ExtrinsicVector4_t extrinsic;
		public ExtrinsicInfo_t(bool in_isTracking, ExtrinsicVector4_t in_extrinsic)
		{
			isTracking = in_isTracking;
			extrinsic = in_extrinsic;
		}
		public static ExtrinsicInfo_t identity {
			get {
				return new ExtrinsicInfo_t(false, ExtrinsicVector4_t.identity);
			}
		}
		public void Init()
		{
			isTracking = false;
			extrinsic = ExtrinsicVector4_t.identity;
		}
		public void Update(ExtrinsicInfo_t in_info)
		{
			isTracking = in_info.isTracking;
			extrinsic.Update(in_info.extrinsic);
		}
		public void Update(ExtrinsicVector4_t in_ext)
		{
			isTracking = true;
			extrinsic.Update(in_ext);
		}
		public void Update(Extrinsic in_ext)
		{
			isTracking = true;
			extrinsic.Update(in_ext);
		}
		public void printLog(string prefix)
		{
			sb.Clear().Append(LOG_TAG).Append(prefix)
				.Append(", position(").Append(extrinsic.translation.x).Append(", ").Append(extrinsic.translation.y).Append(", ").Append(extrinsic.translation.z).Append(")")
				.Append(", rotation(").Append(extrinsic.rotation.x).Append(", ").Append(extrinsic.rotation.y).Append(", ").Append(extrinsic.rotation.z).Append(", ").Append(extrinsic.rotation.w).Append(")");
			DEBUG(sb);
		}
	}

	public struct TrackedDeviceExtrinsicState_t
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.TrackedDeviceExtrinsicState_t";
		static StringBuilder m_sb = null;
		static StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		static void DEBUG(StringBuilder msg) { Debug.Log(msg); }

		public bool isTracking;
		public TrackedDeviceExtrinsic deviceExtrinsic;

		public TrackedDeviceExtrinsicState_t(bool in_isTracking, TrackedDeviceExtrinsic in_deviceExtrinsic)
		{
			isTracking = in_isTracking;
			deviceExtrinsic = in_deviceExtrinsic;
		}
		public static TrackedDeviceExtrinsicState_t identity {
			get {
				return new TrackedDeviceExtrinsicState_t(false, TrackedDeviceExtrinsic.identity);
			}
		}
		public void Update(TrackedDeviceExtrinsicState_t in_info)
		{
			isTracking = in_info.isTracking;
			deviceExtrinsic.Update(in_info.deviceExtrinsic);
		}
		public void Update(ExtrinsicInfo_t extInfo)
		{
			isTracking = extInfo.isTracking;
			deviceExtrinsic.extrinsic.Update(extInfo.extrinsic.GetExtrinsic());

			sb.Clear().Append(LOG_TAG).Append(deviceExtrinsic.trackedDeviceRole.Name())
				.Append(", isTracking: ").Append(isTracking)
				.Append(", position(")
					.Append(deviceExtrinsic.extrinsic.translation.x).Append(", ")
					.Append(deviceExtrinsic.extrinsic.translation.y).Append(", ")
					.Append(deviceExtrinsic.extrinsic.translation.z)
				.Append(")")
				.Append(", rotation(")
					.Append(deviceExtrinsic.extrinsic.rotation.x).Append(", ")
					.Append(deviceExtrinsic.extrinsic.rotation.y).Append(", ")
					.Append(deviceExtrinsic.extrinsic.rotation.z).Append(", ")
					.Append(deviceExtrinsic.extrinsic.rotation.w)
				.Append(")");
			DEBUG(sb);
		}
	}
	/// <summary>
	/// A class records the developer's choices of tracking devices.
	/// The developer selects which devices to be tracked in the Inspector of BodyManager.Body.
	/// The selections will be imported as a BodyTrackedDevice instance.
	/// </summary>
	public class BodyTrackedDevice
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.BodyTrackedDevice";
		static StringBuilder m_sb = null;
		static StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Debug.Log(msg); }
		static int logFrame = 0;
		bool printIntervalLog = false;
		void ERROR(StringBuilder msg) { Debug.LogError(msg); }

		public TrackedDeviceExtrinsicState_t hip = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t chest = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t head = TrackedDeviceExtrinsicState_t.identity;

		public TrackedDeviceExtrinsicState_t leftElbow = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t leftWrist = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t leftHand = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t leftHandheld = TrackedDeviceExtrinsicState_t.identity;

		public TrackedDeviceExtrinsicState_t rightElbow = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t rightWrist = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t rightHand = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t rightHandheld = TrackedDeviceExtrinsicState_t.identity;

		public TrackedDeviceExtrinsicState_t leftKnee = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t leftAnkle = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t leftFoot = TrackedDeviceExtrinsicState_t.identity;

		public TrackedDeviceExtrinsicState_t rightKnee = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t rightAnkle = TrackedDeviceExtrinsicState_t.identity;
		public TrackedDeviceExtrinsicState_t rightFoot = TrackedDeviceExtrinsicState_t.identity;

		private Dictionary<DeviceExtRole, List<TrackedDeviceExtrinsic>> s_TrackedDeviceExtrinsics = new Dictionary<DeviceExtRole, List<TrackedDeviceExtrinsic>>();
		private Dictionary<DeviceExtRole, TrackedDeviceExtrinsic[]> trackedDevicesArrays = new Dictionary<DeviceExtRole, TrackedDeviceExtrinsic[]>();

		private bool getDeviceExtrinsicsFirstTime = true;
		private void UpdateTrackedDevicesArray()
		{
			if (s_TrackedDeviceExtrinsics == null) { s_TrackedDeviceExtrinsics = new Dictionary<DeviceExtRole, List<TrackedDeviceExtrinsic>>(); }
			s_TrackedDeviceExtrinsics.Clear();

			s_TrackedDeviceExtrinsics.Add(DeviceExtRole.Arm_Wrist, new List<TrackedDeviceExtrinsic>());
			s_TrackedDeviceExtrinsics.Add(DeviceExtRole.UpperBody_Wrist, new List<TrackedDeviceExtrinsic>());
			s_TrackedDeviceExtrinsics.Add(DeviceExtRole.FullBody_Wrist_Ankle, new List<TrackedDeviceExtrinsic>());
			s_TrackedDeviceExtrinsics.Add(DeviceExtRole.FullBody_Wrist_Foot, new List<TrackedDeviceExtrinsic>());

			s_TrackedDeviceExtrinsics.Add(DeviceExtRole.Arm_Handheld_Hand, new List<TrackedDeviceExtrinsic>());
			s_TrackedDeviceExtrinsics.Add(DeviceExtRole.UpperBody_Handheld_Hand, new List<TrackedDeviceExtrinsic>());
			s_TrackedDeviceExtrinsics.Add(DeviceExtRole.FullBody_Handheld_Hand_Ankle, new List<TrackedDeviceExtrinsic>());
			s_TrackedDeviceExtrinsics.Add(DeviceExtRole.FullBody_Handheld_Hand_Foot, new List<TrackedDeviceExtrinsic>());

			s_TrackedDeviceExtrinsics.Add(DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle, new List<TrackedDeviceExtrinsic>());

			// 7 roles use hip.
			if (hip.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(hip.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Wrist].Add(hip.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Ankle].Add(hip.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Foot].Add(hip.deviceExtrinsic);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand].Add(hip.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Ankle].Add(hip.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Foot].Add(hip.deviceExtrinsic);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].Add(hip.deviceExtrinsic);
			}
			if (chest.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(chest.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);
			}
			// 9 roles use head.
			if (head.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(head.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.Arm_Wrist].Add(head.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Wrist].Add(head.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Ankle].Add(head.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Foot].Add(head.deviceExtrinsic);

				s_TrackedDeviceExtrinsics[DeviceExtRole.Arm_Handheld_Hand].Add(head.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand].Add(head.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Ankle].Add(head.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Foot].Add(head.deviceExtrinsic);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].Add(head.deviceExtrinsic);
			}

			if (leftElbow.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(leftElbow.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);
			}
			// 4 roles use leftWrist.
			if (leftWrist.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(leftWrist.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.Arm_Wrist].Add(leftWrist.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Wrist].Add(leftWrist.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Ankle].Add(leftWrist.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Foot].Add(leftWrist.deviceExtrinsic);
			}
			// 5 roles use leftHand
			if (leftHand.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(leftHand.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.Arm_Handheld_Hand].Add(leftHand.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand].Add(leftHand.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Ankle].Add(leftHand.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Foot].Add(leftHand.deviceExtrinsic);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].Add(leftHand.deviceExtrinsic);
			}
			// 5 roles use leftHandheld
			if (leftHandheld.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(leftHandheld.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.Arm_Handheld_Hand].Add(leftHandheld.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand].Add(leftHandheld.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Ankle].Add(leftHandheld.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Foot].Add(leftHandheld.deviceExtrinsic);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].Add(leftHandheld.deviceExtrinsic);
			}

			if (rightElbow.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(rightElbow.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);
			}
			// 4 roles use rightWrist.
			if (rightWrist.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(rightWrist.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.Arm_Wrist].Add(rightWrist.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Wrist].Add(rightWrist.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Ankle].Add(rightWrist.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Foot].Add(rightWrist.deviceExtrinsic);
			}
			// 5 roles use rightHand
			if (rightHand.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(rightHand.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.Arm_Handheld_Hand].Add(rightHand.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand].Add(rightHand.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Ankle].Add(rightHand.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Foot].Add(rightHand.deviceExtrinsic);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].Add(rightHand.deviceExtrinsic);
			}
			// 5 roles use rightHandheld
			if (rightHandheld.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(rightHandheld.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.Arm_Handheld_Hand].Add(rightHandheld.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand].Add(rightHandheld.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Ankle].Add(rightHandheld.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Foot].Add(rightHandheld.deviceExtrinsic);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].Add(rightHandheld.deviceExtrinsic);
			}

			// Only 1 role uses leftKnee.
			if (leftKnee.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(leftKnee.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].Add(leftKnee.deviceExtrinsic);
			}
			// 3 roles use leftAnkle
			if (leftAnkle.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(leftAnkle.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Ankle].Add(leftAnkle.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Ankle].Add(leftAnkle.deviceExtrinsic);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].Add(leftAnkle.deviceExtrinsic);
			}
			// 2 roles use leftFoot
			if (leftFoot.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(leftFoot.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Foot].Add(leftFoot.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Foot].Add(leftFoot.deviceExtrinsic);
			}

			// Only 1 role uses rightKnee.
			if (rightKnee.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(rightKnee.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].Add(rightKnee.deviceExtrinsic);
			}
			// 3 roles use rightAnkle
			if (rightAnkle.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(rightAnkle.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Ankle].Add(rightAnkle.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Ankle].Add(rightAnkle.deviceExtrinsic);

				s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].Add(rightAnkle.deviceExtrinsic);
			}
			// 2 roles use rightFoot
			if (rightFoot.isTracking)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateTrackedDevicesArray() Uses extrinsic of ").Append(rightFoot.deviceExtrinsic.trackedDeviceRole.Name()); DEBUG(sb);

				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Foot].Add(rightFoot.deviceExtrinsic);
				s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Foot].Add(rightFoot.deviceExtrinsic);
			}

			if (trackedDevicesArrays == null) { trackedDevicesArrays = new Dictionary<DeviceExtRole, TrackedDeviceExtrinsic[]>(); }
			trackedDevicesArrays.Clear();

			trackedDevicesArrays.Add(DeviceExtRole.Arm_Wrist, s_TrackedDeviceExtrinsics[DeviceExtRole.Arm_Wrist].ToArray());
			trackedDevicesArrays.Add(DeviceExtRole.UpperBody_Wrist, s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Wrist].ToArray());
			trackedDevicesArrays.Add(DeviceExtRole.FullBody_Wrist_Ankle, s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Ankle].ToArray());
			trackedDevicesArrays.Add(DeviceExtRole.FullBody_Wrist_Foot, s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Wrist_Foot].ToArray());

			trackedDevicesArrays.Add(DeviceExtRole.Arm_Handheld_Hand, s_TrackedDeviceExtrinsics[DeviceExtRole.Arm_Handheld_Hand].ToArray());
			trackedDevicesArrays.Add(DeviceExtRole.UpperBody_Handheld_Hand, s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand].ToArray());
			trackedDevicesArrays.Add(DeviceExtRole.FullBody_Handheld_Hand_Ankle, s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Ankle].ToArray());
			trackedDevicesArrays.Add(DeviceExtRole.FullBody_Handheld_Hand_Foot, s_TrackedDeviceExtrinsics[DeviceExtRole.FullBody_Handheld_Hand_Foot].ToArray());

			trackedDevicesArrays.Add(DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle, s_TrackedDeviceExtrinsics[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle].ToArray());

			getDeviceExtrinsicsFirstTime = true;
		}

		public BodyTrackedDevice()
		{
			hip.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_HIP;
			chest.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_CHEST;
			head.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_HEAD;

			leftElbow.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTELBOW;
			leftWrist.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTWRIST;
			leftHand.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTHAND;
			leftHandheld.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTHANDHELD;

			rightElbow.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTELBOW;
			rightWrist.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTWRIST;
			rightHand.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTHAND;
			rightHandheld.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTHANDHELD;

			leftKnee.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTKNEE;
			leftAnkle.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTANKLE;
			leftFoot.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTFOOT;

			rightKnee.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTKNEE;
			rightAnkle.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTANKLE;
			rightFoot.deviceExtrinsic.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTFOOT;
		}
		public void Update(BodyTrackedDevice in_device)
		{
			if (in_device == null) { return; }

			hip.Update(in_device.hip);
			chest.Update(in_device.chest);
			head.Update(in_device.head);

			leftElbow.Update(in_device.leftElbow);
			leftWrist.Update(in_device.leftWrist);
			leftHand.Update(in_device.leftHand);
			leftHandheld.Update(in_device.leftHandheld);

			rightElbow.Update(in_device.rightElbow);
			rightWrist.Update(in_device.rightWrist);
			rightHand.Update(in_device.rightHand);
			rightHandheld.Update(in_device.rightHandheld);

			leftKnee.Update(in_device.leftKnee);
			leftAnkle.Update(in_device.leftAnkle);
			leftFoot.Update(in_device.leftFoot);

			rightKnee.Update(in_device.rightKnee);
			rightAnkle.Update(in_device.rightAnkle);
			rightFoot.Update(in_device.rightFoot);

			UpdateTrackedDevicesArray();
		}
		/// <summary> Should only be called in CreateBodyTracking() </summary>
		public void Update([In] TrackerExtrinsic in_ext)
		{
			if (in_ext == null) { return; }
			sb.Clear().Append(LOG_TAG).Append("Update() TrackerExtrinsic of each device."); DEBUG(sb);

			hip.Update(in_ext.hip); // 0
			chest.Update(in_ext.chest);
			head.Update(in_ext.head);

			leftElbow.Update(in_ext.leftElbow);
			leftWrist.Update(in_ext.leftWrist);
			leftHand.Update(in_ext.leftHand); // 5
			leftHandheld.Update(in_ext.leftHandheld);

			rightElbow.Update(in_ext.rightElbow);
			rightWrist.Update(in_ext.rightWrist);
			rightHand.Update(in_ext.rightHand);
			rightHandheld.Update(in_ext.rightHandheld); // 10

			leftKnee.Update(in_ext.leftKnee);
			leftAnkle.Update(in_ext.leftAnkle);
			leftFoot.Update(in_ext.leftFoot);

			rightKnee.Update(in_ext.rightKnee);
			rightAnkle.Update(in_ext.rightAnkle); // 15
			rightFoot.Update(in_ext.rightFoot);

			UpdateTrackedDevicesArray();
		}

		/// <summary> The device extrinsics for use depends on the calibration pose role. </summary>
		public bool GetDevicesExtrinsics(BodyPoseRole calibRole, out TrackedDeviceExtrinsic[] bodyTrackedDevices, out UInt32 bodyTrackedDeviceCount)
		{
			logFrame++;
			logFrame %= 500;
			printIntervalLog = (logFrame == 0);

			// Upper Body + Leg FK
			if (calibRole == BodyPoseRole.UpperBody_Handheld_Knee_Ankle || calibRole == BodyPoseRole.UpperBody_Hand_Knee_Ankle)
			{
				bodyTrackedDevices = trackedDevicesArrays[DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle];
				bodyTrackedDeviceCount = (UInt32)(bodyTrackedDevices.Length & 0x7FFFFFFF);

				if (printIntervalLog || getDeviceExtrinsicsFirstTime)
				{
					sb.Clear().Append(LOG_TAG).Append("GetDevicesExtrinsics() of extrinsic role ").Append(DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle.Name()); DEBUG(sb);
					sb.Clear().Append(LOG_TAG);
					for (int i = 0; i < bodyTrackedDeviceCount; i++)
					{
						sb.Append("GetDevicesExtrinsics() Add extrinsic[").Append(i).Append("] ")
							.Append(bodyTrackedDevices[i].trackedDeviceRole.Name()).Append("\n");
					}
					DEBUG(sb);
				}
				return bodyTrackedDeviceCount > 0;
			}

			// Full Body
			if (calibRole == BodyPoseRole.FullBody_Wrist_Ankle)
			{
				bodyTrackedDevices = trackedDevicesArrays[DeviceExtRole.FullBody_Wrist_Ankle];
				bodyTrackedDeviceCount = (UInt32)(bodyTrackedDevices.Length & 0x7FFFFFFF);

				if (printIntervalLog || getDeviceExtrinsicsFirstTime)
				{
					sb.Clear().Append(LOG_TAG).Append("GetDevicesExtrinsics() of extrinsic role ").Append(DeviceExtRole.FullBody_Wrist_Ankle.Name()); DEBUG(sb);
					sb.Clear().Append(LOG_TAG);
					for (int i = 0; i < bodyTrackedDeviceCount; i++)
					{
						sb.Append("GetDevicesExtrinsics() Add extrinsic[").Append(i).Append("] ")
							.Append(bodyTrackedDevices[i].trackedDeviceRole.Name()).Append("\n");
					}
					DEBUG(sb);
				}
				return bodyTrackedDeviceCount > 0;
			}
			if (calibRole == BodyPoseRole.FullBody_Wrist_Foot)
			{
				bodyTrackedDevices = trackedDevicesArrays[DeviceExtRole.FullBody_Wrist_Foot];
				bodyTrackedDeviceCount = (UInt32)(bodyTrackedDevices.Length & 0x7FFFFFFF);

				if (printIntervalLog || getDeviceExtrinsicsFirstTime)
				{
					sb.Clear().Append(LOG_TAG).Append("GetDevicesExtrinsics() of extrinsic role ").Append(DeviceExtRole.FullBody_Wrist_Foot.Name()); DEBUG(sb);
					sb.Clear().Append(LOG_TAG);
					for (int i = 0; i < bodyTrackedDeviceCount; i++)
					{
						sb.Append("GetDevicesExtrinsics() Add extrinsic[").Append(i).Append("] ")
							.Append(bodyTrackedDevices[i].trackedDeviceRole.Name()).Append("\n");
					}
					DEBUG(sb);
				}
				return bodyTrackedDeviceCount > 0;
			}
			if (calibRole == BodyPoseRole.FullBody_Handheld_Ankle || calibRole == BodyPoseRole.FullBody_Hand_Ankle)
			{
				bodyTrackedDevices = trackedDevicesArrays[DeviceExtRole.FullBody_Handheld_Hand_Ankle];
				bodyTrackedDeviceCount = (UInt32)(bodyTrackedDevices.Length & 0x7FFFFFFF);

				if (printIntervalLog || getDeviceExtrinsicsFirstTime)
				{
					sb.Clear().Append(LOG_TAG).Append("GetDevicesExtrinsics() of extrinsic role ").Append(DeviceExtRole.FullBody_Handheld_Hand_Ankle.Name()); DEBUG(sb);
					sb.Clear().Append(LOG_TAG);
					for (int i = 0; i < bodyTrackedDeviceCount; i++)
					{
						sb.Append("GetDevicesExtrinsics() Add extrinsic[").Append(i).Append("] ")
							.Append(bodyTrackedDevices[i].trackedDeviceRole.Name()).Append("\n");
					}
					DEBUG(sb);
				}
				return bodyTrackedDeviceCount > 0;
			}
			if (calibRole == BodyPoseRole.FullBody_Handheld_Foot || calibRole == BodyPoseRole.FullBody_Hand_Foot)
			{
				bodyTrackedDevices = trackedDevicesArrays[DeviceExtRole.FullBody_Handheld_Hand_Foot];
				bodyTrackedDeviceCount = (UInt32)(bodyTrackedDevices.Length & 0x7FFFFFFF);

				if (printIntervalLog || getDeviceExtrinsicsFirstTime)
				{
					sb.Clear().Append(LOG_TAG).Append("GetDevicesExtrinsics() of extrinsic role ").Append(DeviceExtRole.FullBody_Handheld_Hand_Foot.Name()); DEBUG(sb);
					sb.Clear().Append(LOG_TAG);
					for (int i = 0; i < bodyTrackedDeviceCount; i++)
					{
						sb.Append("GetDevicesExtrinsics() Add extrinsic[").Append(i).Append("] ")
							.Append(bodyTrackedDevices[i].trackedDeviceRole.Name()).Append("\n");
					}
					DEBUG(sb);
				}
				return bodyTrackedDeviceCount > 0;
			}

			// Upper Body
			if (calibRole == BodyPoseRole.UpperBody_Wrist)
			{
				bodyTrackedDevices = trackedDevicesArrays[DeviceExtRole.UpperBody_Wrist];
				bodyTrackedDeviceCount = (UInt32)(bodyTrackedDevices.Length & 0x7FFFFFFF);

				if (printIntervalLog || getDeviceExtrinsicsFirstTime)
				{
					sb.Clear().Append(LOG_TAG).Append("GetDevicesExtrinsics() of extrinsic role ").Append(DeviceExtRole.UpperBody_Wrist.Name()); DEBUG(sb);
					sb.Clear().Append(LOG_TAG);
					for (int i = 0; i < bodyTrackedDeviceCount; i++)
					{
						sb.Append("GetDevicesExtrinsics() Add extrinsic[").Append(i).Append("] ")
							.Append(bodyTrackedDevices[i].trackedDeviceRole.Name()).Append("\n");
					}
					DEBUG(sb);
				}
				return bodyTrackedDeviceCount > 0;
			}
			if (calibRole == BodyPoseRole.UpperBody_Handheld || calibRole == BodyPoseRole.UpperBody_Hand)
			{
				bodyTrackedDevices = trackedDevicesArrays[DeviceExtRole.UpperBody_Handheld_Hand];
				bodyTrackedDeviceCount = (UInt32)(bodyTrackedDevices.Length & 0x7FFFFFFF);

				if (printIntervalLog || getDeviceExtrinsicsFirstTime)
				{
					sb.Clear().Append(LOG_TAG).Append("GetDevicesExtrinsics() of extrinsic role ").Append(DeviceExtRole.UpperBody_Handheld_Hand.Name()); DEBUG(sb);
					sb.Clear().Append(LOG_TAG);
					for (int i = 0; i < bodyTrackedDeviceCount; i++)
					{
						sb.Append("GetDevicesExtrinsics() Add extrinsic[").Append(i).Append("] ")
							.Append(bodyTrackedDevices[i].trackedDeviceRole.Name()).Append("\n");
					}
					DEBUG(sb);
				}
				return bodyTrackedDeviceCount > 0;
			}

			// Arm
			if (calibRole == BodyPoseRole.Arm_Wrist)
			{
				bodyTrackedDevices = trackedDevicesArrays[DeviceExtRole.Arm_Wrist];
				bodyTrackedDeviceCount = (UInt32)(bodyTrackedDevices.Length & 0x7FFFFFFF);

				if (printIntervalLog || getDeviceExtrinsicsFirstTime)
				{
					sb.Clear().Append(LOG_TAG).Append("GetDevicesExtrinsics() of extrinsic role ").Append(DeviceExtRole.Arm_Wrist.Name()); DEBUG(sb);
					sb.Clear().Append(LOG_TAG);
					for (int i = 0; i < bodyTrackedDeviceCount; i++)
					{
						sb.Append("GetDevicesExtrinsics() Add extrinsic[").Append(i).Append("] ")
							.Append(bodyTrackedDevices[i].trackedDeviceRole.Name()).Append("\n");
					}
					DEBUG(sb);
				}
				return bodyTrackedDeviceCount > 0;
			}
			if (calibRole == BodyPoseRole.Arm_Handheld || calibRole == BodyPoseRole.Arm_Hand)
			{
				bodyTrackedDevices = trackedDevicesArrays[DeviceExtRole.Arm_Handheld_Hand];
				bodyTrackedDeviceCount = (UInt32)(bodyTrackedDevices.Length & 0x7FFFFFFF);

				if (printIntervalLog || getDeviceExtrinsicsFirstTime)
				{
					sb.Clear().Append(LOG_TAG).Append("GetDevicesExtrinsics() of extrinsic role ").Append(DeviceExtRole.Arm_Handheld_Hand.Name()); DEBUG(sb);
					sb.Clear().Append(LOG_TAG);
					for (int i = 0; i < bodyTrackedDeviceCount; i++)
					{
						sb.Append("GetDevicesExtrinsics() Add extrinsic[").Append(i).Append("] ")
							.Append(bodyTrackedDevices[i].trackedDeviceRole.Name()).Append("\n");
					}
					DEBUG(sb);
				}
				return bodyTrackedDeviceCount > 0;
			}

			bodyTrackedDevices = null;
			bodyTrackedDeviceCount = 0;
			return false;
		}

		private int ikFrame = -1;
		private DeviceExtRole m_IKRoles = DeviceExtRole.Unknown;
		public DeviceExtRole GetIKRoles(BodyPoseRole calibRole)
		{
			if (printIntervalLog || getDeviceExtrinsicsFirstTime) { sb.Clear().Append(LOG_TAG).Append("GetIKRoles()"); DEBUG(sb); }

			if (ikFrame == Time.frameCount) { return m_IKRoles; }
			else { m_IKRoles = DeviceExtRole.Unknown; ikFrame = Time.frameCount; }

			if (GetDevicesExtrinsics(calibRole, out TrackedDeviceExtrinsic[] bodyTrackedDevices, out UInt32 bodyTrackedDeviceCount))
				m_IKRoles = BodyTrackingUtils.GetDeviceExtRole(calibRole, bodyTrackedDevices, bodyTrackedDeviceCount);

			return m_IKRoles;
		}
	}
	public class BodyIKInfo
	{
		public BodyTrackingMode mode = BodyTrackingMode.UNKNOWNMODE;
		public BodyAvatar avatar = new BodyAvatar();
		public BodyTrackedDevice trackedDevice = new BodyTrackedDevice();

		public BodyIKInfo(BodyTrackingMode in_mode)
		{
			mode = in_mode;
		}
		public BodyIKInfo(BodyTrackingMode in_mode, BodyAvatar in_avatar, BodyTrackedDevice in_device)
		{
			mode = in_mode;
			avatar = in_avatar;
			trackedDevice = in_device;
		}
		public void Update(BodyIKInfo in_info)
		{
			if (in_info == null) { return; }

			mode = in_info.mode;
			avatar.Update(in_info.avatar);
			trackedDevice.Update(in_info.trackedDevice);
		}
	}
	
	internal struct TrackingInfo
	{
		public bool[] initRoles;
		public bool[] trackedRoles;

		public TrackingInfo(bool[] in_initRoles, bool[] in_trackedRoles)
		{
			initRoles = in_initRoles;
			trackedRoles = in_trackedRoles;
		}
		public void Init()
		{
			initRoles = new bool[(Int32)TrackedDeviceRole.NUMS_OF_ROLE];
			trackedRoles = new bool[(Int32)TrackedDeviceRole.NUMS_OF_ROLE];
		}
		public void Update(TrackingInfo info)
		{
			if (initRoles == null || initRoles.Length != info.initRoles.Length)
				initRoles = new bool[info.initRoles.Length];
			for (int i = 0; i < initRoles.Length; i++)
				initRoles[i] = info.initRoles[i];

			if (trackedRoles == null || trackedRoles.Length != info.trackedRoles.Length)
				trackedRoles = new bool[info.trackedRoles.Length];
			for (int i = 0; i < trackedRoles.Length; i++)
				trackedRoles[i] = info.trackedRoles[i];
		}
		public void ResetInitRoles()
		{
			if (initRoles == null) { return; }
			for (int i = 0; i < initRoles.Length; i++)
				initRoles[i] = false;
		}
		public void ResetTrackedRoles()
		{
			if (trackedRoles == null) { return; }
			for (int i = 0; i < trackedRoles.Length; i++)
				trackedRoles[i] = false;
		}
		public void Reset()
		{
			ResetInitRoles();
			ResetTrackedRoles();
		}
	}
	public class BodyPose
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.BodyPose ";
		StringBuilder m_sb = null;
		StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Debug.Log(LOG_TAG + msg); }
		static int logFrame = 0;
		bool printIntervalLog = false;
		void WARNING(StringBuilder msg) { Debug.LogWarning(msg); }
		void ERROR(StringBuilder msg) { Debug.LogError(LOG_TAG + msg); }

		public TrackedDevicePose hip = TrackedDevicePose.identity;
		public TrackedDevicePose chest = TrackedDevicePose.identity;
		public TrackedDevicePose head = TrackedDevicePose.identity;

		public TrackedDevicePose leftElbow = TrackedDevicePose.identity;
		public TrackedDevicePose leftWrist = TrackedDevicePose.identity;
		public TrackedDevicePose leftHand = TrackedDevicePose.identity;
		public TrackedDevicePose leftHandheld = TrackedDevicePose.identity;

		public TrackedDevicePose rightElbow = TrackedDevicePose.identity;
		public TrackedDevicePose rightWrist = TrackedDevicePose.identity;
		public TrackedDevicePose rightHand = TrackedDevicePose.identity;
		public TrackedDevicePose rightHandheld = TrackedDevicePose.identity;

		public TrackedDevicePose leftKnee = TrackedDevicePose.identity;
		public TrackedDevicePose leftAnkle = TrackedDevicePose.identity;
		public TrackedDevicePose leftFoot = TrackedDevicePose.identity;

		public TrackedDevicePose rightKnee = TrackedDevicePose.identity;
		public TrackedDevicePose rightAnkle = TrackedDevicePose.identity;
		public TrackedDevicePose rightFoot = TrackedDevicePose.identity;

		private InputActionReference m_HipPose = null;
		private InputActionReference m_LeftWristPose = null;
		private InputActionReference m_RightWristPose = null;

		private InputActionReference m_LeftKneePose = null;
		private InputActionReference m_LeftAnklePose = null;
		private InputActionReference m_LeftFootPose = null;

		private InputActionReference m_RightKneePose = null;
		private InputActionReference m_RightAnklePose = null;
		private InputActionReference m_RightFootPose = null;

		internal TrackingInfo trackingInfos;

		public BodyPose(
			InputActionReference Hip, InputActionReference LeftWrist, InputActionReference RightWrist,
			InputActionReference LeftKnee, InputActionReference LeftAnkle, InputActionReference LeftFoot,
			InputActionReference RightKnee, InputActionReference RightAnkle, InputActionReference RightFoot
		)
		{
			hip.trackedDeviceRole = TrackedDeviceRole.ROLE_HIP;
			chest.trackedDeviceRole = TrackedDeviceRole.ROLE_CHEST;
			head.trackedDeviceRole = TrackedDeviceRole.ROLE_HEAD;

			leftElbow.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTELBOW;
			leftWrist.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTWRIST;
			leftHand.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTHAND;
			leftHandheld.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTHANDHELD;

			rightElbow.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTELBOW;
			rightWrist.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTWRIST;
			rightHand.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTHAND;
			rightHandheld.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTHANDHELD;

			leftKnee.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTKNEE;
			leftAnkle.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTANKLE;
			leftFoot.trackedDeviceRole = TrackedDeviceRole.ROLE_LEFTFOOT;

			rightKnee.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTKNEE;
			rightAnkle.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTANKLE;
			rightFoot.trackedDeviceRole = TrackedDeviceRole.ROLE_RIGHTFOOT;

			trackingInfos.Init();

			Reset(
				Hip: Hip,
				LeftWrist: LeftWrist,
				RightWrist: RightWrist,

				LeftKnee: LeftKnee,
				LeftAnkle: LeftAnkle,
				LeftFoot: LeftFoot,

				RightKnee: RightKnee,
				RightAnkle: RightAnkle,
				RightFoot: RightFoot
			);
		}
		public void Update([In] BodyPose in_body)
		{
			hip.Update(in_body.hip);
			chest.Update(in_body.chest);
			head.Update(in_body.head);

			leftElbow.Update(in_body.leftElbow);
			leftWrist.Update(in_body.leftWrist);
			leftHand.Update(in_body.leftHand);
			leftHandheld.Update(in_body.leftHandheld);

			rightElbow.Update(in_body.rightElbow);
			rightWrist.Update(in_body.rightWrist);
			rightHand.Update(in_body.rightHand);
			rightHandheld.Update(in_body.rightHandheld);

			leftKnee.Update(in_body.leftKnee);
			leftAnkle.Update(in_body.leftAnkle);
			leftFoot.Update(in_body.leftFoot);

			rightKnee.Update(in_body.rightKnee);
			rightAnkle.Update(in_body.rightAnkle);
			rightFoot.Update(in_body.rightFoot);

			trackingInfos.Update(in_body.trackingInfos);
		}
		private void ResetPose()
		{
			hip.poseState = PoseState.NODATA;
			chest.poseState = PoseState.NODATA;
			head.poseState = PoseState.NODATA;

			leftElbow.poseState = PoseState.NODATA;
			leftWrist.poseState = PoseState.NODATA;
			leftHand.poseState = PoseState.NODATA;
			leftHandheld.poseState = PoseState.NODATA;

			rightElbow.poseState = PoseState.NODATA;
			rightWrist.poseState = PoseState.NODATA;
			rightHand.poseState = PoseState.NODATA;
			rightHandheld.poseState = PoseState.NODATA;

			leftKnee.poseState = PoseState.NODATA;
			leftAnkle.poseState = PoseState.NODATA;
			leftFoot.poseState = PoseState.NODATA;

			rightKnee.poseState = PoseState.NODATA;
			rightAnkle.poseState = PoseState.NODATA;
			rightFoot.poseState = PoseState.NODATA;

			trackingInfos.ResetTrackedRoles();
		}
		private bool getDevicePosesFirstTime = true;
		public void Reset(
			InputActionReference Hip, InputActionReference LeftWrist, InputActionReference RightWrist,
			InputActionReference LeftKnee, InputActionReference LeftAnkle, InputActionReference LeftFoot,
			InputActionReference RightKnee, InputActionReference RightAnkle, InputActionReference RightFoot
		)
		{
			getDevicePosesFirstTime = true;

			ResetPose();

			m_HipPose = Hip;
			if (m_HipPose == null) { sb.Clear().Append(LOG_TAG).Append("Reset() m_HipPose is null."); WARNING(sb); }
			m_LeftWristPose = LeftWrist;
			if (m_LeftWristPose == null) { sb.Clear().Append(LOG_TAG).Append("Reset() m_LeftWristPose is null."); WARNING(sb); }
			m_RightWristPose = RightWrist;
			if (m_RightWristPose == null) { sb.Clear().Append(LOG_TAG).Append("Reset() m_RightWristPose is null."); WARNING(sb); }

			m_LeftKneePose = LeftKnee;
			if (m_LeftKneePose == null) { sb.Clear().Append(LOG_TAG).Append("Reset() m_LeftKneePose is null."); WARNING(sb); }
			m_LeftAnklePose = LeftAnkle;
			if (m_LeftAnklePose == null) { sb.Clear().Append(LOG_TAG).Append("Reset() m_LeftAnklePose is null."); WARNING(sb); }
			m_LeftFootPose = LeftFoot;
			if (m_LeftFootPose == null) { sb.Clear().Append(LOG_TAG).Append("Reset() m_LeftFootPose is null."); WARNING(sb); }

			m_RightKneePose = RightKnee;
			if (m_RightKneePose == null) { sb.Clear().Append(LOG_TAG).Append("Reset() m_RightKneePose is null."); WARNING(sb); }
			m_RightAnklePose = RightAnkle;
			if (m_RightAnklePose == null) { sb.Clear().Append(LOG_TAG).Append("Reset() m_RightAnklePose is null."); WARNING(sb); }
			m_RightFootPose = RightFoot;
			if (m_RightFootPose == null) { sb.Clear().Append(LOG_TAG).Append("Reset() m_RightFootPose is null."); WARNING(sb); }
		}

		#region Update Tracking Infos and Standard Pose in content.
		/// <summary>
		/// Called from BodyManager.UpdateBodyTrackingOnce() to update Body Poses every frame.
		/// <br></br>
		/// Called time: 1. After InitTrackingInfos. 2. Each frame from UpdateBodyTrackingOnce().
		/// </summary>
		internal BodyTrackingResult UpdatePose()
		{
			logFrame++;
			logFrame %= 500;
			printIntervalLog = (logFrame == 0);

			if (trackingInfos.initRoles == null || trackingInfos.initRoles.Length <= 0)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdatePose() No pose to update."); ERROR(sb);
				return BodyTrackingResult.ERROR_IK_NOT_UPDATED;
			}
			ResetPose();

			for (int i = 0; i < trackingInfos.initRoles.Length; i++)
			{
				if (!trackingInfos.initRoles[i]) { continue; }

				TrackedDeviceRole role = BodyTrackingUtils.GetTrackedDeviceRole(i);
				if (role == TrackedDeviceRole.ROLE_HIP && BodyTrackingUtils.UpdateTrackedDevicePose(role, m_HipPose, ref hip) == BodyTrackingResult.SUCCESS)
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("UpdatePose() ").Append(role.Name()).Append(", poseState: ").Append(hip.poseState);
						DEBUG(sb);
					}
					trackingInfos.trackedRoles[i] = true;
				}
				if (role == TrackedDeviceRole.ROLE_HEAD && BodyTrackingUtils.UpdateTrackedDevicePose(role, null, ref head) == BodyTrackingResult.SUCCESS)
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("UpdatePose() ").Append(role.Name()).Append(", poseState: ").Append(head.poseState);
						DEBUG(sb);
					}
					trackingInfos.trackedRoles[i] = true;
				}

				if (role == TrackedDeviceRole.ROLE_LEFTWRIST && BodyTrackingUtils.UpdateTrackedDevicePose(role, m_LeftWristPose, ref leftWrist) == BodyTrackingResult.SUCCESS)
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("UpdatePose() ").Append(role.Name()).Append(", poseState: ").Append(leftWrist.poseState);
						DEBUG(sb);
					}
					trackingInfos.trackedRoles[i] = true;
				}
				if (role == TrackedDeviceRole.ROLE_LEFTHANDHELD || role == TrackedDeviceRole.ROLE_LEFTHAND)
				{
					if (BodyTrackingUtils.UpdateTrackedDevicePose(TrackedDeviceRole.ROLE_LEFTHANDHELD, null, ref leftHandheld) == BodyTrackingResult.SUCCESS)
					{
						if (printIntervalLog || getDevicePosesFirstTime)
						{
							sb.Clear().Append(LOG_TAG).Append("UpdatePose() ROLE_LEFTHANDHELD, poseState: ").Append(leftHandheld.poseState);
							DEBUG(sb);
						}
						trackingInfos.trackedRoles[(Int32)TrackedDeviceRole.ROLE_LEFTHANDHELD] = true;
					}
					else
					{
						BodyTrackingUtils.UpdateTrackedDevicePose(TrackedDeviceRole.ROLE_LEFTHAND, null, ref leftHand);
						if (printIntervalLog || getDevicePosesFirstTime)
						{
							sb.Clear().Append(LOG_TAG).Append("UpdatePose() ROLE_LEFTHAND poseState: ").Append(leftHand.poseState);
							DEBUG(sb);
						}
						trackingInfos.trackedRoles[(Int32)TrackedDeviceRole.ROLE_LEFTHAND] = true;
					}
				}

				if (role == TrackedDeviceRole.ROLE_RIGHTWRIST && BodyTrackingUtils.UpdateTrackedDevicePose(role, m_RightWristPose, ref rightWrist) == BodyTrackingResult.SUCCESS)
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("UpdatePose() ").Append(role.Name()).Append(", poseState: ").Append(rightWrist.poseState);
						DEBUG(sb);
					}
					trackingInfos.trackedRoles[i] = true;
				}
				if (role == TrackedDeviceRole.ROLE_RIGHTHANDHELD || role == TrackedDeviceRole.ROLE_RIGHTHAND)
				{
					if (BodyTrackingUtils.UpdateTrackedDevicePose(TrackedDeviceRole.ROLE_RIGHTHANDHELD, null, ref rightHandheld) == BodyTrackingResult.SUCCESS)
					{
						if (printIntervalLog || getDevicePosesFirstTime)
						{
							sb.Clear().Append(LOG_TAG).Append("UpdatePose() ROLE_RIGHTHANDHELD, poseState: ").Append(rightHandheld.poseState);
							DEBUG(sb);
						}
						trackingInfos.trackedRoles[(Int32)TrackedDeviceRole.ROLE_RIGHTHANDHELD] = true;
					}
					else
					{
						BodyTrackingUtils.UpdateTrackedDevicePose(TrackedDeviceRole.ROLE_RIGHTHAND, null, ref rightHand);
						if (printIntervalLog || getDevicePosesFirstTime)
						{
							sb.Clear().Append(LOG_TAG).Append("UpdatePose() ROLE_RIGHTHAND, poseState: ").Append(rightHand.poseState);
							DEBUG(sb);
						}
						trackingInfos.trackedRoles[(Int32)TrackedDeviceRole.ROLE_RIGHTHAND] = true;
					}
				}

				if (role == TrackedDeviceRole.ROLE_LEFTKNEE && BodyTrackingUtils.UpdateTrackedDevicePose(role, m_LeftKneePose, ref leftKnee) == BodyTrackingResult.SUCCESS)
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("UpdatePose() ").Append(role.Name()).Append(", poseState: ").Append(leftKnee.poseState);
						DEBUG(sb);
					}
					trackingInfos.trackedRoles[i] = true;
				}
				if (role == TrackedDeviceRole.ROLE_LEFTANKLE && BodyTrackingUtils.UpdateTrackedDevicePose(role, m_LeftAnklePose, ref leftAnkle) == BodyTrackingResult.SUCCESS)
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("UpdatePose() ").Append(role.Name()).Append(", poseState: ").Append(leftAnkle.poseState);
						DEBUG(sb);
					}
					trackingInfos.trackedRoles[i] = true;
				}
				if (role == TrackedDeviceRole.ROLE_LEFTFOOT && BodyTrackingUtils.UpdateTrackedDevicePose(role, m_LeftFootPose, ref leftFoot) == BodyTrackingResult.SUCCESS)
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("UpdatePose() ").Append(role.Name()).Append(", poseState: ").Append(leftFoot.poseState);
						DEBUG(sb);
					}
					trackingInfos.trackedRoles[i] = true;
				}

				if (role == TrackedDeviceRole.ROLE_RIGHTKNEE && BodyTrackingUtils.UpdateTrackedDevicePose(role, m_RightKneePose, ref rightKnee) == BodyTrackingResult.SUCCESS)
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("UpdatePose() ").Append(role.Name()).Append(", poseState: ").Append(rightKnee.poseState);
						DEBUG(sb);
					}
					trackingInfos.trackedRoles[i] = true;
				}
				if (role == TrackedDeviceRole.ROLE_RIGHTANKLE && BodyTrackingUtils.UpdateTrackedDevicePose(role, m_RightAnklePose, ref rightAnkle) == BodyTrackingResult.SUCCESS)
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("UpdatePose() ").Append(role.Name()).Append(", poseState: ").Append(rightAnkle.poseState);
						DEBUG(sb);
					}
					trackingInfos.trackedRoles[i] = true;
				}
				if (role == TrackedDeviceRole.ROLE_RIGHTFOOT && BodyTrackingUtils.UpdateTrackedDevicePose(role, m_RightFootPose, ref rightFoot) == BodyTrackingResult.SUCCESS)
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("UpdatePose() ").Append(role.Name()).Append(", poseState: ").Append(rightFoot.poseState);
						DEBUG(sb);
					}
					trackingInfos.trackedRoles[i] = true;
				}
			}

			return BodyTrackingResult.SUCCESS;
		}
		private readonly Dictionary<BodyTrackingMode, List<TrackedDeviceRole>> s_TrackingModeRole = new Dictionary<BodyTrackingMode, List<TrackedDeviceRole>>()
		{
			{
				BodyTrackingMode.ARMIK, new List<TrackedDeviceRole>() {
				TrackedDeviceRole.ROLE_HEAD,
				TrackedDeviceRole.ROLE_LEFTWRIST, TrackedDeviceRole.ROLE_RIGHTWRIST,
				TrackedDeviceRole.ROLE_LEFTHANDHELD, TrackedDeviceRole.ROLE_RIGHTHANDHELD,
				TrackedDeviceRole.ROLE_LEFTHAND, TrackedDeviceRole.ROLE_RIGHTHAND }
			},
			{
				BodyTrackingMode.UPPERBODYIK, new List<TrackedDeviceRole>() {
				TrackedDeviceRole.ROLE_HEAD,
				TrackedDeviceRole.ROLE_LEFTWRIST, TrackedDeviceRole.ROLE_RIGHTWRIST,
				TrackedDeviceRole.ROLE_LEFTHANDHELD, TrackedDeviceRole.ROLE_RIGHTHANDHELD,
				TrackedDeviceRole.ROLE_LEFTHAND, TrackedDeviceRole.ROLE_RIGHTHAND,
				TrackedDeviceRole.ROLE_HIP }
			},
			{
				BodyTrackingMode.FULLBODYIK, new List<TrackedDeviceRole>() {
				TrackedDeviceRole.ROLE_HEAD,
				TrackedDeviceRole.ROLE_LEFTWRIST, TrackedDeviceRole.ROLE_RIGHTWRIST,
				TrackedDeviceRole.ROLE_LEFTHANDHELD, TrackedDeviceRole.ROLE_RIGHTHANDHELD,
				TrackedDeviceRole.ROLE_LEFTHAND, TrackedDeviceRole.ROLE_RIGHTHAND,
				TrackedDeviceRole.ROLE_HIP,
				TrackedDeviceRole.ROLE_LEFTANKLE, TrackedDeviceRole.ROLE_RIGHTANKLE,
				TrackedDeviceRole.ROLE_LEFTFOOT, TrackedDeviceRole.ROLE_RIGHTFOOT }
			},
			{
				BodyTrackingMode.UPPERIKANDLEGFK, new List<TrackedDeviceRole>() {
				TrackedDeviceRole.ROLE_HEAD,
				TrackedDeviceRole.ROLE_LEFTWRIST, TrackedDeviceRole.ROLE_RIGHTWRIST,
				TrackedDeviceRole.ROLE_LEFTHANDHELD, TrackedDeviceRole.ROLE_RIGHTHANDHELD,
				TrackedDeviceRole.ROLE_LEFTHAND, TrackedDeviceRole.ROLE_RIGHTHAND,
				TrackedDeviceRole.ROLE_HIP,
				TrackedDeviceRole.ROLE_LEFTKNEE, TrackedDeviceRole.ROLE_RIGHTKNEE,
				TrackedDeviceRole.ROLE_LEFTANKLE, TrackedDeviceRole.ROLE_RIGHTANKLE }
			},
		};

		internal BodyTrackingResult InitTrackingInfos(BodyTrackingMode mode)
		{
			trackingInfos.ResetInitRoles();
			sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() ").Append(mode.Name()); DEBUG(sb);

			bool isTracked = false;
			string error = "";
			if (InputDeviceControl.IsTracked(InputDeviceControl.ControlDevice.Head))
			{
				sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_HEAD"); DEBUG(sb);
				trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_HEAD] = true;
			}

			// Checks Wrist first.
			bool hasLeftHand = false, hasRightHand = false;
			if (!hasLeftHand && m_LeftWristPose != null && BodyTrackingUtils.GetPoseIsTracked(m_LeftWristPose, out isTracked, out error) && isTracked)
			{
				sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_LEFTWRIST"); DEBUG(sb);
				trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_LEFTWRIST] = true;
				hasLeftHand = true;
			}
			if (!hasRightHand && m_RightWristPose != null && BodyTrackingUtils.GetPoseIsTracked(m_RightWristPose, out isTracked, out error) && isTracked)
			{
				sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_RIGHTWRIST"); DEBUG(sb);
				trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_RIGHTWRIST] = true;
				hasRightHand = true;
			}
			// Checks Handheld next.
			if (!hasLeftHand && InputDeviceControl.IsTracked(InputDeviceControl.ControlDevice.Left))
			{
				sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_LEFTHANDHELD"); DEBUG(sb);
				trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_LEFTHANDHELD] = true;
				hasLeftHand = true;
			}
			if (!hasRightHand && InputDeviceControl.IsTracked(InputDeviceControl.ControlDevice.Right))
			{
				sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_RIGHTHANDHELD"); DEBUG(sb);
				trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_RIGHTHANDHELD] = true;
				hasRightHand = true;
			}
			// Checks Hand last.
			if (!hasLeftHand)
			{
				sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() FORCE add ROLE_LEFTHAND"); DEBUG(sb);
				trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_LEFTHAND] = true;
			}
			if (!hasRightHand)
			{
				sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() FORCE add ROLE_RIGHTHAND"); DEBUG(sb);
				trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_RIGHTHAND] = true;
			}

			// UpperBody mode uses HIP.
			if (s_TrackingModeRole[mode].Contains(TrackedDeviceRole.ROLE_HIP))
			{
				if (m_HipPose != null && BodyTrackingUtils.GetPoseIsTracked(m_HipPose, out isTracked, out error) && isTracked)
				{
					sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_HIP"); DEBUG(sb);
					trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_HIP] = true;
				}
			}

			// UpperBodyLeg mode uses KNEE.
			if (s_TrackingModeRole[mode].Contains(TrackedDeviceRole.ROLE_LEFTKNEE) && s_TrackingModeRole[mode].Contains(TrackedDeviceRole.ROLE_RIGHTKNEE))
			{
				if (m_LeftKneePose != null && BodyTrackingUtils.GetPoseIsTracked(m_LeftKneePose, out isTracked, out error) && isTracked)
				{
					sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_LEFTKNEE"); DEBUG(sb);
					trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_LEFTKNEE] = true;
				}
				if (m_RightKneePose != null && BodyTrackingUtils.GetPoseIsTracked(m_RightKneePose, out isTracked, out error) && isTracked)
				{
					sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_RIGHTKNEE"); DEBUG(sb);
					trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_RIGHTKNEE] = true;
				}
			}

			// FullBody & UpperBodyLeg modes use either ANKLE or FOOT.
			bool hasLeftLeg = false, hasRightLeg = false;
			if (s_TrackingModeRole[mode].Contains(TrackedDeviceRole.ROLE_LEFTANKLE) && s_TrackingModeRole[mode].Contains(TrackedDeviceRole.ROLE_RIGHTANKLE))
			{
				if (!hasLeftLeg && m_LeftAnklePose != null && BodyTrackingUtils.GetPoseIsTracked(m_LeftAnklePose, out isTracked, out error) && isTracked)
				{
					sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_LEFTANKLE"); DEBUG(sb);
					trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_LEFTANKLE] = true;
					hasLeftLeg = true;
				}
				if (!hasRightLeg && m_RightAnklePose != null && BodyTrackingUtils.GetPoseIsTracked(m_RightAnklePose, out isTracked, out error) && isTracked)
				{
					sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_RIGHTANKLE"); DEBUG(sb);
					trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_RIGHTANKLE] = true;
					hasRightLeg = true;
				}
			}
			if (s_TrackingModeRole[mode].Contains(TrackedDeviceRole.ROLE_LEFTFOOT) && s_TrackingModeRole[mode].Contains(TrackedDeviceRole.ROLE_RIGHTFOOT))
			{
				if (!hasLeftLeg && m_LeftFootPose != null && BodyTrackingUtils.GetPoseIsTracked(m_LeftFootPose, out isTracked, out error) && isTracked)
				{
					sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_LEFTFOOT"); DEBUG(sb);
					trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_LEFTFOOT] = true;
				}
				if (!hasRightLeg && m_RightFootPose != null && BodyTrackingUtils.GetPoseIsTracked(m_RightFootPose, out isTracked, out error) && isTracked)
				{
					sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() add ROLE_RIGHTFOOT"); DEBUG(sb);
					trackingInfos.initRoles[(UInt32)TrackedDeviceRole.ROLE_RIGHTFOOT] = true;
				}
			}

			BodyTrackingResult result = UpdatePose();
			if (result == BodyTrackingResult.SUCCESS)
			{
				BodyPoseRole ikRoles = GetIKRoles(mode);
				sb.Clear().Append(LOG_TAG).Append("InitTrackingInfos() ikRoles: ").Append(ikRoles.Name()); DEBUG(sb);
			}

			return result;
		}
		#endregion

		private List<TrackedDevicePose> poses = new List<TrackedDevicePose>();
		private TrackedDevicePose[] poseArray = null;
		private void UpdatePoseArray()
		{
			if (poses == null || poses.Count <= 0) { return; }
			if (poseArray == null || poseArray.Length != poses.Count) { poseArray = new TrackedDevicePose[poses.Count]; }
			for (int i = 0; i < poseArray.Length; i++) { poseArray[i] = poses[i]; }
		}
		public bool GetTrackedDevicePoses(BodyTrackingMode mode, out TrackedDevicePose[] trackedDevicePoses, out UInt32 trackedDevicePoseCount)
		{
			poses.Clear();

			for (int i = 0; i < trackingInfos.trackedRoles.Length; i++)
			{
				if (!trackingInfos.trackedRoles[i]) { continue; }

				TrackedDeviceRole role = BodyTrackingUtils.GetTrackedDeviceRole(i);
				if (role == TrackedDeviceRole.ROLE_HEAD && BodyTrackingUtils.PoseStateAvailable(mode, head))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add head with poseState ").Append(head.poseState);
						DEBUG(sb);
					}
					poses.Add(head);
				}
				if (role == TrackedDeviceRole.ROLE_CHEST && BodyTrackingUtils.PoseStateAvailable(mode, chest)) { poses.Add(chest); }
				if (role == TrackedDeviceRole.ROLE_HIP && BodyTrackingUtils.PoseStateAvailable(mode, hip))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add hip with poseState ").Append(hip.poseState);
						DEBUG(sb);
					}
					poses.Add(hip);
				}

				if (role == TrackedDeviceRole.ROLE_LEFTELBOW && BodyTrackingUtils.PoseStateAvailable(mode, leftElbow)) { poses.Add(leftElbow); }
				if (role == TrackedDeviceRole.ROLE_LEFTWRIST && BodyTrackingUtils.PoseStateAvailable(mode, leftWrist))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add leftWrist with poseState ").Append(leftWrist.poseState);
						DEBUG(sb);
					}
					poses.Add(leftWrist);
				}
				// LeftHand uses Natural Hand pose which can be ignored in calibration.
				if (role == TrackedDeviceRole.ROLE_LEFTHAND && BodyTrackingUtils.PoseStateAvailable(mode, leftHand, true))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add leftHand with poseState ").Append(leftHand.poseState);
						DEBUG(sb);
					}
					poses.Add(leftHand);
				}
				if (role == TrackedDeviceRole.ROLE_LEFTHANDHELD && BodyTrackingUtils.PoseStateAvailable(mode, leftHandheld))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add leftHandheld with poseState ").Append(leftHandheld.poseState);
						DEBUG(sb);
					}
					poses.Add(leftHandheld);
				}

				if (role == TrackedDeviceRole.ROLE_RIGHTELBOW && BodyTrackingUtils.PoseStateAvailable(mode, rightElbow)) { poses.Add(rightElbow); }
				if (role == TrackedDeviceRole.ROLE_RIGHTWRIST && BodyTrackingUtils.PoseStateAvailable(mode, rightWrist))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add rightWrist with poseState ").Append(rightWrist.poseState);
						DEBUG(sb);
					}
					poses.Add(rightWrist);
				}
				// RightHand uses Natural Hand pose which can be ignored in calibration.
				if (role == TrackedDeviceRole.ROLE_RIGHTHAND && BodyTrackingUtils.PoseStateAvailable(mode, rightHand, true))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add rightHand with poseState ").Append(rightHand.poseState);
						DEBUG(sb);
					}
					poses.Add(rightHand);
				}
				if (role == TrackedDeviceRole.ROLE_RIGHTHANDHELD && BodyTrackingUtils.PoseStateAvailable(mode, rightHandheld))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add rightHandheld with poseState ").Append(rightHandheld.poseState);
						DEBUG(sb);
					}
					poses.Add(rightHandheld);
				}

				// LeftKnee uses SelfTrackerIM which has rotation only.
				if (role == TrackedDeviceRole.ROLE_LEFTKNEE && BodyTrackingUtils.PoseStateAvailable(mode, leftKnee))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add leftKnee with poseState ").Append(leftKnee.poseState);
						DEBUG(sb);
					}
					poses.Add(leftKnee);
				}
				// LeftAnkle uses SelfTracker 6DoF pose or SelfTrackerIM pose which has rotation only.
				if (role == TrackedDeviceRole.ROLE_LEFTANKLE && BodyTrackingUtils.PoseStateAvailable(mode, leftAnkle))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add leftAnkle with poseState ").Append(leftAnkle.poseState);
						DEBUG(sb);
					}
					poses.Add(leftAnkle);
				}
				if (role == TrackedDeviceRole.ROLE_LEFTFOOT && BodyTrackingUtils.PoseStateAvailable(mode, leftFoot))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add leftFoot with poseState ").Append(leftFoot.poseState);
						DEBUG(sb);
					}
					poses.Add(leftFoot);
				}

				// RightKnee uses SelfTrackerIM which has rotation only.
				if (role == TrackedDeviceRole.ROLE_RIGHTKNEE && BodyTrackingUtils.PoseStateAvailable(mode, rightKnee))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add rightKnee with poseState ").Append(rightKnee.poseState);
						DEBUG(sb);
					}
					poses.Add(rightKnee);
				}
				// RightAnkle uses SelfTracker 6DoF pose or SelfTrackerIM pose which has rotation only.
				if (role == TrackedDeviceRole.ROLE_RIGHTANKLE && BodyTrackingUtils.PoseStateAvailable(mode, rightAnkle))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add rightAnkle with poseState ").Append(rightAnkle.poseState);
						DEBUG(sb);
					}
					poses.Add(rightAnkle);
				}
				if (role == TrackedDeviceRole.ROLE_RIGHTFOOT && BodyTrackingUtils.PoseStateAvailable(mode, rightFoot))
				{
					if (printIntervalLog || getDevicePosesFirstTime)
					{
						sb.Clear().Append(LOG_TAG).Append("GetTrackedDevicePoses() Add rightFoot with poseState ").Append(rightFoot.poseState);
						DEBUG(sb);
					}
					poses.Add(rightFoot);
				}
			}

			getDevicePosesFirstTime = false;
			if (poses.Count > 0)
			{
				UpdatePoseArray();
				trackedDevicePoses = poseArray;
				trackedDevicePoseCount = (UInt32)(poseArray.Length & 0x7FFFFFFF);
				return true;
			}

			trackedDevicePoses = null;
			trackedDevicePoseCount = 0;
			return false;
		}

		private int ikFrame = -1;
		private BodyPoseRole m_BodyPoseRole = BodyPoseRole.Unknown;
		public BodyPoseRole GetIKRoles(BodyTrackingMode mode)
		{
			if (printIntervalLog || getDevicePosesFirstTime)
			{
				sb.Clear().Append(LOG_TAG).Append("GetIKRoles() mode: ").Append(mode.Name())
					.Append(", last m_BodyPoseRole: ").Append(m_BodyPoseRole.Name());
				DEBUG(sb);
			}

			if (ikFrame == Time.frameCount) { return m_BodyPoseRole; }
			else { m_BodyPoseRole = BodyPoseRole.Unknown; ikFrame = Time.frameCount; }

			if (GetTrackedDevicePoses(mode, out TrackedDevicePose[] trackedDevicePoses, out UInt32 trackedDevicePoseCount))
				m_BodyPoseRole = BodyTrackingUtils.GetBodyPoseRole(trackedDevicePoses, trackedDevicePoseCount);

			return m_BodyPoseRole;
		}
	}

	internal struct BodyRotationSpace_t
	{
		public RotateSpace[] spaces;
		public UInt32 count;

		public BodyRotationSpace_t(RotateSpace[] in_spaces, UInt32 in_count)
		{
			spaces = in_spaces;
			count = in_count;
		}
		public void Update(BodyRotationSpace_t in_brt)
		{
			if (count != in_brt.count && in_brt.count > 0)
			{
				count = in_brt.count;
				spaces = new RotateSpace[count];
			}
			for (UInt32 i = 0; i < count; i++)
				spaces[i] = in_brt.spaces[i];
		}
	}

	#region API v1.0.0.1
	public enum JointType : Int32
	{
		UNKNOWN = -1,
		HIP = 0,

		LEFTTHIGH = 1,
		LEFTLEG = 2,
		LEFTANKLE = 3,
		LEFTFOOT = 4,

		RIGHTTHIGH = 5,
		RIGHTLEG = 6,
		RIGHTANKLE = 7,
		RIGHTFOOT = 8,

		WAIST = 9,

		SPINELOWER = 10,
		SPINEMIDDLE = 11,
		SPINEHIGH = 12,

		CHEST = 13,
		NECK = 14,
		HEAD = 15,

		LEFTCLAVICLE = 16,
		LEFTSCAPULA = 17,
		LEFTUPPERARM = 18,
		LEFTFOREARM = 19,
		LEFTHAND = 20,

		RIGHTCLAVICLE = 21,
		RIGHTSCAPULA = 22,
		RIGHTUPPERARM = 23,
		RIGHTFOREARM = 24,
		RIGHTHAND = 25,

		NUMS_OF_JOINT,
		MAX_ENUM = 0x7fffffff
	}
	[Flags]
	public enum PoseState : UInt32
	{
		NODATA = 0,
		ROTATION = 1 << 0,
		TRANSLATION = 1 << 1
	}
	public enum BodyTrackingMode : Int32
	{
		UNKNOWNMODE = -1,
		ARMIK = 0,
		UPPERBODYIK = 1,
		FULLBODYIK = 2,

		UPPERIKANDLEGFK = 3, // controller or hand + hip tracker + leg fk
		SPINEIK = 4,    // used internal
		LEGIK = 5,    // used internal
		LEGFK = 6,    // used internal
		SPINEIKANDLEGFK = 7, // hip tracker + leg fk
		MAX = 0x7fffffff
	}
	public enum TrackedDeviceRole : Int32
	{
		ROLE_UNDEFINED = -1,
		ROLE_HIP = 0,
		ROLE_CHEST = 1,
		ROLE_HEAD = 2,

		ROLE_LEFTELBOW = 3,
		ROLE_LEFTWRIST = 4,
		ROLE_LEFTHAND = 5,
		ROLE_LEFTHANDHELD = 6,

		ROLE_RIGHTELBOW = 7,
		ROLE_RIGHTWRIST = 8,
		ROLE_RIGHTHAND = 9,
		ROLE_RIGHTHANDHELD = 10,

		ROLE_LEFTKNEE = 11,
		ROLE_LEFTANKLE = 12,
		ROLE_LEFTFOOT = 13,

		ROLE_RIGHTKNEE = 14,
		ROLE_RIGHTANKLE = 15,
		ROLE_RIGHTFOOT = 16,

		NUMS_OF_ROLE,
		ROLE_MAX = 0x7fffffff
	}
	public enum Result : Int32
	{
		SUCCESS = 0,
		ERROR_BODYTRACKINGMODE_NOT_FOUND = 100,
		ERROR_TRACKER_AMOUNT_FAILED = 101,
		ERROR_SKELETONID_NOT_FOUND = 102,
		ERROR_INPUTPOSE_NOT_VALID = 103,
		ERROR_NOT_CALIBRATED = 104,
		ERROR_BODYTRACKINGMODE_NOT_ALIGNED = 105,
		ERROR_AVATAR_INIT_FAILED = 200,
		ERROR_CALIBRATE_FAILED = 300,
		ERROR_COMPUTE_FAILED = 400,
		ERROR_TABLE_STATIC = 401,
		ERROR_SOLVER_NOT_FOUND = 402,
		ERROR_NOT_INITIALIZATION = 403,
		ERROR_JOINT_NOT_FOUND = 404,
		ERROR_FATAL_ERROR = 499,
		ERROR_MAX = 0x7fffffff,
	}
	public enum TrackerDirection : Int32
	{
		NODIRECTION = -1,
		FORWARD = 0,
		BACKWARD = 1,
		RIGHT = 2,
		LEFT = 3
	}
	public enum AvatarType : UInt32
	{
		TPOSE = 0,    // T-pose, pre-processing in SDK
		STANDARD_VRM = 1,    // any pose, standard vrm model (all joints' coordinate is identity)
		OTHERS = 2     // any pose, but need to meet the constraint defined by library
	}
	public enum CalibrationType : UInt32
	{
		DEFAULTCALIBRATION =
			0,    // User stands L-pose. Use tracked device poses to calibrate. Need tracked device pose.
		TOFFSETCALIBRATION =
			1,    // User stands straight. Only do translation offset calibration. Need tracked device pose.
		HEIGHTCALIBRATION = 2,    // Set user height directly. No need tracked device pose.
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Joint
	{
		[FieldOffset(0)] public JointType jointType;
		[FieldOffset(4)] public PoseState poseState;
		[FieldOffset(8)] public Vector3 translation;
		[FieldOffset(20)] public Vector3 velocity;
		[FieldOffset(32)] public Vector3 angularVelocity;
		[FieldOffset(44)] public Quaternion rotation;

		public Joint(JointType in_jointType, PoseState in_poseState, Vector3 in_translation, Vector3 in_velocity, Vector3 in_angularVelocity, Quaternion in_rotation)
		{
			jointType = in_jointType;
			poseState = in_poseState;
			translation = in_translation;
			velocity = in_velocity;
			angularVelocity = in_angularVelocity;
			rotation = in_rotation;
			BodyTrackingUtils.Validate(ref rotation);
		}
		public static Joint identity {
			get {
				return new Joint(JointType.UNKNOWN, PoseState.NODATA, Vector3.zero, Vector3.zero, Vector3.zero, Quaternion.identity);
			}
		}
		public void Update(Joint in_joint)
		{
			jointType = in_joint.jointType;
			poseState = in_joint.poseState;
			translation = in_joint.translation;
			velocity = in_joint.velocity;
			angularVelocity = in_joint.angularVelocity;
			BodyTrackingUtils.Update(ref rotation, in_joint.rotation);
		}
		public static Joint init(JointType type)
		{
			return new Joint(type, PoseState.NODATA, Vector3.zero, Vector3.zero, Vector3.zero, Quaternion.identity);
		}
		public string Log()
		{
			string log = "jointType: " + jointType;
			log += ", poseState: " + poseState;
			log += ", position(" + translation.x.ToString() + ", " + translation.y.ToString() + ", " + translation.z.ToString() + ")";
			log += ", rotation(" + rotation.x.ToString() + ", " + rotation.y.ToString() + ", " + rotation.z.ToString() + ", " + rotation.w.ToString() + ")";
			log += ", velocity(" + velocity.x.ToString() + ", " + velocity.y.ToString() + ", " + velocity.z.ToString() + ")";
			log += ", angularVelocity(" + angularVelocity.x.ToString() + ", " + angularVelocity.y.ToString() + ", " + angularVelocity.z.ToString() + ")";
			return log;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	[Serializable]
	public struct Extrinsic
	{
		[FieldOffset(0)] public Vector3 translation;
		[FieldOffset(12)] public Quaternion rotation;

		public Extrinsic(Vector3 in_translation, Quaternion in_rotation)
		{
			translation = in_translation;
			rotation = in_rotation;
			BodyTrackingUtils.Validate(ref rotation);
		}
		public static Extrinsic identity { 
			get {
				return new Extrinsic(Vector3.zero, Quaternion.identity);
			}
		}
		public void Update(Extrinsic in_ext)
		{
			translation = in_ext.translation;
			BodyTrackingUtils.Update(ref rotation, in_ext.rotation);
		}

		public string Log()
		{
			string log = "position(" + translation.x.ToString() + ", " + translation.y.ToString() + ", " + translation.z.ToString() + ")";
			log += ", rotation(" + rotation.x.ToString() + ", " + rotation.y.ToString() + ", " + rotation.z.ToString() + ", " + rotation.w.ToString() + ")";
			return log;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct TrackedDeviceExtrinsic
	{
		[FieldOffset(0)] public TrackedDeviceRole trackedDeviceRole;
		[FieldOffset(4)] public Extrinsic extrinsic;

		public TrackedDeviceExtrinsic(TrackedDeviceRole in_trackedDeviceRole, Extrinsic in_extrinsic)
		{
			trackedDeviceRole = in_trackedDeviceRole;
			extrinsic = in_extrinsic;
		}
		public static TrackedDeviceExtrinsic identity {
			get {
				return new TrackedDeviceExtrinsic(TrackedDeviceRole.ROLE_UNDEFINED, Extrinsic.identity);
			}
		}
		public static TrackedDeviceExtrinsic init(TrackedDeviceRole role)
		{
			return new TrackedDeviceExtrinsic(role, Extrinsic.identity);
		}
		public void Update(TrackedDeviceExtrinsic in_ext)
		{
			trackedDeviceRole = in_ext.trackedDeviceRole;
			extrinsic.Update(in_ext.extrinsic);
		}
		public string Log()
		{
			string log = "Role: " + trackedDeviceRole;
			log += ", extrinsic: " + extrinsic.Log();
			return log;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct TrackedDevicePose
	{
		[FieldOffset(0)] public TrackedDeviceRole trackedDeviceRole;
		[FieldOffset(4)] public PoseState poseState;
		[FieldOffset(8)] public Vector3 translation;
		[FieldOffset(20)] public Vector3 velocity;
		[FieldOffset(32)] public Vector3 angularVelocity;
		[FieldOffset(44)] public Vector3 acceleration;
		[FieldOffset(56)] public Quaternion rotation;

		public TrackedDevicePose(TrackedDeviceRole in_trackedDeviceRole, PoseState in_poseState, Vector3 in_translation, Vector3 in_velocity, Vector3 in_angularVelocity, Vector3 in_acceleration, Quaternion in_rotation)
		{
			trackedDeviceRole = in_trackedDeviceRole;
			poseState = in_poseState;
			translation = in_translation;
			velocity = in_velocity;
			angularVelocity = in_angularVelocity;
			acceleration = in_acceleration;
			rotation = in_rotation;
			BodyTrackingUtils.Validate(ref rotation);
		}
		public static TrackedDevicePose identity {
			get {
				return new TrackedDevicePose(TrackedDeviceRole.ROLE_UNDEFINED, PoseState.NODATA, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Quaternion.identity);
			}
		}
		public void Update([In] TrackedDevicePose in_pose)
		{
			trackedDeviceRole = in_pose.trackedDeviceRole;
			poseState = in_pose.poseState;
			translation = in_pose.translation;
			velocity = in_pose.velocity;
			angularVelocity = in_pose.angularVelocity;
			acceleration = in_pose.acceleration;
			BodyTrackingUtils.Update(ref rotation, in_pose.rotation);
		}
		public string Log()
		{
			string log = "trackedDeviceRole: " + trackedDeviceRole.Name();
			log += ", poseState: " + poseState;
			log += ", translation(" + translation.x.ToString() + ", " + translation.y.ToString() + ", " + translation.z.ToString() + ")";
			log += ", velocity(" + velocity.x.ToString() + ", " + velocity.y.ToString() + ", " + velocity.z.ToString() + ")";
			log += ", angularVelocity(" + angularVelocity.x.ToString() + ", " + angularVelocity.y.ToString() + ", " + angularVelocity.z.ToString() + ")";
			log += ", acceleration(" + acceleration.x.ToString() + ", " + acceleration.y.ToString() + ", " + acceleration.z.ToString() + ")";
			log += ", rotation(" + rotation.x.ToString() + ", " + rotation.y.ToString() + ", " + rotation.z.ToString() + ", " + rotation.w.ToString() + ")";
			return log;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct RotateSpace
	{
		[FieldOffset(0)] public JointType jointType;
		[FieldOffset(4)] public Quaternion rotation;

		public RotateSpace(JointType in_jointType, Quaternion in_rotation)
		{
			jointType = in_jointType;
			rotation = in_rotation;
			BodyTrackingUtils.Validate(ref rotation);
		}
		public static RotateSpace identity {
			get {
				return new RotateSpace(JointType.UNKNOWN, Quaternion.identity);
			}
		}
	}

	public class fbt
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.fbt";
		static StringBuilder m_sb = null;
		static StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		static void DEBUG(StringBuilder msg) { Debug.Log(msg); }
		static int logFrame = -1;
		static bool fbtIntervalLog = false;

		[DllImport("bodytracking")]
		/**
		 *  @brief Initial body tracking algorithm with custom skeleton
		 *  @param[in] timestamp (unit:us)
		 *  @param[in] bodyTrackingMode. The body tracking mode which developer wants to use
		 *  @param[in] trackedDeviceExt. The tracked device extrinsic from avatar to tracked device
		 *  @param[in] deviceCount. The amount of tracked devices
		 *  @param[in] avatarJoints. The avatar's joints
		 *  @param[in] avatarJointCount. The amount of the avatar's joints
		 *  @param[in] avatarHeight. The avatar's height
		 *  @param[out] skeleton id.
		 *  @param[in] avatarType. The avatar's type (This paramenter is only for internal use. The default value is TPOSE.)
		 *  @param[out] success or not.
		 **/
		public static extern Result InitBodyTracking(UInt64 ts, BodyTrackingMode bodyTrackingMode,
			TrackedDeviceExtrinsic[] trackedDeviceExt, UInt32 deviceCount,
			Joint[] avatarJoints, UInt32 avatarJointCount, float avatarHeight,
			ref int skeletonId,
			AvatarType avatarType = AvatarType.TPOSE);
		public static Result InitBodyTrackingLog(UInt64 ts, BodyTrackingMode bodyTrackingMode,
			TrackedDeviceExtrinsic[] trackedDeviceExt, UInt32 deviceCount,
			Joint[] avatarJoints, UInt32 avatarJointCount, float avatarHeight,
			ref int skeletonId,
			AvatarType avatarType = AvatarType.TPOSE)
		{
			sb.Clear().Append(LOG_TAG).Append("InitBodyTracking() ").Append(ts).Append(" bodyTrackingMode: ").Append(bodyTrackingMode.Name()); DEBUG(sb);

			sb.Clear().Append(LOG_TAG).Append("deviceCount: ").Append(deviceCount); DEBUG(sb);
			for (UInt32 i = 0; i < deviceCount; i++)
			{
				sb.Clear().Append(LOG_TAG).Append("InitBodyTracking() trackedDeviceExt[").Append(i).Append("] role: ").Append(trackedDeviceExt[i].trackedDeviceRole.Name())
					.Append(", position (").Append(trackedDeviceExt[i].extrinsic.translation.x.ToString("N2")).Append(", ").Append(trackedDeviceExt[i].extrinsic.translation.y.ToString("N2")).Append(", ").Append(trackedDeviceExt[i].extrinsic.translation.z.ToString("N2")).Append(")")
					.Append(", rotation (").Append(trackedDeviceExt[i].extrinsic.rotation.x.ToString("N2")).Append(", ").Append(trackedDeviceExt[i].extrinsic.rotation.y.ToString("N2")).Append(", ").Append(trackedDeviceExt[i].extrinsic.rotation.z.ToString("N2")).Append(", ").Append(trackedDeviceExt[i].extrinsic.rotation.w.ToString("N2")).Append(")");
				DEBUG(sb);
			}

			sb.Append("InitBodyTracking() avatarJointCount: ").Append(avatarJointCount); DEBUG(sb);
			for (UInt32 i = 0; i < avatarJointCount; i++)
			{
				sb.Clear().Append(LOG_TAG).Append("InitBodyTracking() avatarJoints[").Append(i).Append("] jointType: ").Append(avatarJoints[i].jointType.Name())
					.Append(", poseState: ").Append(avatarJoints[i].poseState)
					.Append(", position (").Append(avatarJoints[i].translation.x.ToString("N2")).Append(", ").Append(avatarJoints[i].translation.y.ToString("N2")).Append(", ").Append(avatarJoints[i].translation.z.ToString("N2")).Append(")")
					.Append(", rotation (").Append(avatarJoints[i].rotation.x.ToString("N2")).Append(", ").Append(avatarJoints[i].rotation.y.ToString("N2")).Append(", ").Append(avatarJoints[i].rotation.z.ToString("N2")).Append(", ").Append(avatarJoints[i].rotation.w.ToString("N2")).Append(")")
					.Append(", velocity (").Append(avatarJoints[i].velocity.x.ToString("N2")).Append(", ").Append(avatarJoints[i].velocity.y.ToString("N2")).Append(", ").Append(avatarJoints[i].velocity.z.ToString("N2")).Append(")")
					.Append(", angularVelocity (").Append(avatarJoints[i].angularVelocity.x.ToString("N2")).Append(", ").Append(avatarJoints[i].angularVelocity.y.ToString("N2")).Append(", ").Append(avatarJoints[i].angularVelocity.z.ToString("N2")).Append(")");
				DEBUG(sb);
			}

			sb.Clear().Append(LOG_TAG).Append("InitBodyTracking() avatarHeight: ").Append(avatarHeight)
				.Append("skeletonId: ").Append(skeletonId)
				.Append("avatarType: ").Append(avatarType);
			DEBUG(sb);

			return InitBodyTracking(ts, bodyTrackingMode, trackedDeviceExt, deviceCount, avatarJoints, avatarJointCount, avatarHeight, ref skeletonId);
		}

		[DllImport("bodytracking")]
		/**
		 *  @brief Initial body trahcking algorithm with default skeleton
		 *  @param[in] timestamp (unit:us)
		 *  @param[in] bodyTrackingMode. The body tracking mode which developer wants to use
		 *  @param[in] trackedDeviceExt. The tracked device extrinsic from avatar to tracked device
		 *  @param[in] deviceCount. The amount of tracked devices
		 *  @param[out] skeleton id.
		 *  @param[out] success or not.
		 **/
		public static extern Result InitDefaultBodyTracking(UInt64 ts, BodyTrackingMode bodyTrackingMode,
			TrackedDeviceExtrinsic[] trackedDeviceExt, UInt32 deviceCount,
			ref int skeletonId);

		[DllImport("bodytracking")]
		/**
		 *  @brief Calibrate Body Tracking. Must be called after initail. User needs to stand L pose(stand straight, two arms straight forward and let the palm down)
		 *  @param[in] timestamp (unit:us)
		 *  @param[in] skeletonId.
		 *  @param[in] userHeight. The user height.
		 *  @param[in] bodyTrackingMode. The body tracking mode which developer wants to use
		 *  @param[in] trackedDevicePose. The tracked device poses.(Left-Handed coordinate sytstem. x right, y up, z forward. unit: m)
		 *  @param[in] deviceCount. The amount of tracked devices
		 *  @param[out] scale. If used custom skeleton, this value will be the scale of custom skeleton. Otherwise, the value will be 1.
		 *  @param[out] success or not.
		 **/
		public static extern Result CalibrateBodyTracking(UInt64 ts, int skeletonId, float userHeight,
			BodyTrackingMode bodyTrackingMode,
			TrackedDevicePose[] trackedDevicePose, UInt32 deviceCount,
			ref float scale, CalibrationType calibrationType = CalibrationType.DEFAULTCALIBRATION);
		public static Result CalibrateBodyTrackingLog(UInt64 ts, int skeletonId, float userHeight,
			BodyTrackingMode bodyTrackingMode,
			TrackedDevicePose[] trackedDevicePose, UInt32 deviceCount,
			ref float scale, CalibrationType calibrationType = CalibrationType.DEFAULTCALIBRATION)
		{
			sb.Clear().Append(LOG_TAG).Append("CalibrateBodyTracking() ").Append(ts).Append(", id: ").Append(skeletonId)
				.Append("bodyTrackingMode: ").Append(bodyTrackingMode.Name());
			DEBUG(sb);

			sb.Clear().Append(LOG_TAG).Append("deviceCount: ").Append(deviceCount); DEBUG(sb);
			for (UInt32 i = 0; i < deviceCount; i++)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateBodyTracking() trackedDevicePose[").Append(i).Append("] role: ").Append(trackedDevicePose[i].trackedDeviceRole.Name())
					.Append(", position (").Append(trackedDevicePose[i].translation.x.ToString("N2")).Append(", ").Append(trackedDevicePose[i].translation.y.ToString("N2")).Append(", ").Append(trackedDevicePose[i].translation.z.ToString("N2")).Append(")")
					.Append(", rotation (").Append(trackedDevicePose[i].rotation.x.ToString("N2")).Append(", ").Append(trackedDevicePose[i].rotation.y.ToString("N2")).Append(", ").Append(trackedDevicePose[i].rotation.z.ToString("N2")).Append(", ").Append(trackedDevicePose[i].rotation.w.ToString("N2")).Append(")")
					.Append(", velocity (").Append(trackedDevicePose[i].velocity.x.ToString("N2")).Append(", ").Append(trackedDevicePose[i].velocity.y.ToString("N2")).Append(", ").Append(trackedDevicePose[i].velocity.z.ToString("N2")).Append(")")
					.Append(", acceleration (").Append(trackedDevicePose[i].acceleration.x.ToString("N2")).Append(", ").Append(trackedDevicePose[i].acceleration.y.ToString("N2")).Append(", ").Append(trackedDevicePose[i].acceleration.z.ToString("N2")).Append(")")
					.Append(", angularVelocity (").Append(trackedDevicePose[i].angularVelocity.x.ToString("N2")).Append(", ").Append(trackedDevicePose[i].angularVelocity.y.ToString("N2")).Append(", ").Append(trackedDevicePose[i].angularVelocity.z.ToString("N2")).Append(")");
				DEBUG(sb);
			}

			sb.Clear().Append(LOG_TAG).Append("scale: ").Append(scale).Append(", calibrationType: ").Append(calibrationType.Name());
			DEBUG(sb);

			return CalibrateBodyTracking(ts, skeletonId, userHeight, bodyTrackingMode, trackedDevicePose, deviceCount, ref scale, calibrationType);
		}

		[DllImport("bodytracking")]
		/**
		 *  @brief Get the amount of output skeleton joints.
		 *  @param[in] timestamp (unit:us)
		 *  @param[in] skeleton id.
		 *  @param[out] the amount of output skeleton joints.
		 *  @param[out] success or not.
		 **/
		public static extern Result GetOutputJointCount(UInt64 ts, int skeletonId, ref UInt32 jointCount);

		[DllImport("bodytracking")]
		/**
		 *  @brief Update and get skeleton joints pose every frame. Must be called after calibrate.
		 *  @param[in] timestamp (unit:us)
		 *  @param[in] skeleton id.
		 *  @param[in] trackedDevicePose. The tracked device poses.(Left-Handed coordinate sytstem. x right, y up, z forward. unit: m)
		 *  @param[in] deviceCount. The amount of tracked devices
		 *  @param[out] output joints of skeleton. If the pose state of joint equals to 3(Translation|Rotation), it means the joint's pose is valid.
		 *  @param[in] jointCount. The amount of joints.
		 *  @param[out] success or not.
		 **/
		public static extern Result UpdateBodyTracking(UInt64 ts, int skeletonId,
			TrackedDevicePose[] trackedDevicePose, UInt32 deviceCount,
			[In, Out] Joint[] outJoint, UInt32 jointCount);
		public static Result UpdateBodyTrackingLog(UInt64 ts, int skeletonId,
			TrackedDevicePose[] trackedDevicePose, UInt32 deviceCount,
			[In, Out] Joint[] outJoint, UInt32 jointCount)
		{
			sb.Clear().Append(LOG_TAG).Append("UpdateBodyTracking() ").Append(ts).Append(", id: ").Append(skeletonId).Append(", deviceCount: ").Append(deviceCount);
			DEBUG(sb);
			for (UInt32 i = 0; i < deviceCount; i++)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateBodyTracking() trackedDevicePose[").Append(i).Append("] role: ").Append(trackedDevicePose[i].trackedDeviceRole.Name())
					.Append(", position (").Append(trackedDevicePose[i].translation.x.ToString("N2")).Append(", ").Append(trackedDevicePose[i].translation.y.ToString("N2")).Append(", ").Append(trackedDevicePose[i].translation.z.ToString("N2")).Append(")")
					.Append(", rotation (").Append(trackedDevicePose[i].rotation.x.ToString("N2")).Append(", ").Append(trackedDevicePose[i].rotation.y.ToString("N2")).Append(", ").Append(trackedDevicePose[i].rotation.z.ToString("N2")).Append(", ").Append(trackedDevicePose[i].rotation.w.ToString("N2")).Append(")")
					.Append(", velocity (").Append(trackedDevicePose[i].velocity.x.ToString("N2")).Append(", ").Append(trackedDevicePose[i].velocity.y.ToString("N2")).Append(", ").Append(trackedDevicePose[i].velocity.z.ToString("N2")).Append(")")
					.Append(", acceleration (").Append(trackedDevicePose[i].acceleration.x.ToString("N2")).Append(", ").Append(trackedDevicePose[i].acceleration.y.ToString("N2")).Append(", ").Append(trackedDevicePose[i].acceleration.z.ToString("N2")).Append(")")
					.Append(", angularVelocity (").Append(trackedDevicePose[i].angularVelocity.x.ToString("N2")).Append(", ").Append(trackedDevicePose[i].angularVelocity.y.ToString("N2")).Append(", ").Append(trackedDevicePose[i].angularVelocity.z.ToString("N2")).Append(")");
				DEBUG(sb);
			}

			Result result = UpdateBodyTracking(ts, skeletonId, trackedDevicePose, deviceCount, outJoint, jointCount);
			if (result == Result.SUCCESS)
			{
				sb.Clear().Append(LOG_TAG).Append("UpdateBodyTracking() jointCount: ").Append(jointCount);
				DEBUG(sb);
				for (UInt32 i = 0; i < jointCount; i++)
				{
					sb.Clear().Append(LOG_TAG).Append("UpdateBodyTracking() outJoint[").Append(i).Append("] jointType: ").Append(outJoint[i].jointType.Name())
						.Append(", position (").Append(outJoint[i].translation.x.ToString("N2")).Append(", ").Append(outJoint[i].translation.y.ToString("N2")).Append(", ").Append(outJoint[i].translation.z.ToString("N2")).Append(")")
						.Append(", rotation (").Append(outJoint[i].rotation.x.ToString("N2")).Append(", ").Append(outJoint[i].rotation.y.ToString("N2")).Append(", ").Append(outJoint[i].rotation.z.ToString("N2")).Append(", ").Append(outJoint[i].rotation.w.ToString("N2")).Append(")")
						.Append(", velocity (").Append(outJoint[i].velocity.x.ToString("N2")).Append(", ").Append(outJoint[i].velocity.y.ToString("N2")).Append(", ").Append(outJoint[i].velocity.z.ToString("N2")).Append(")")
						.Append(", angularVelocity (").Append(outJoint[i].angularVelocity.x.ToString("N2")).Append(", ").Append(outJoint[i].angularVelocity.y.ToString("N2")).Append(", ").Append(outJoint[i].angularVelocity.z.ToString("N2")).Append(")");
					DEBUG(sb);
				}
			}

			return result;
		}

		[DllImport("bodytracking")]
		/**
		 *  @brief Destroy body tracking.
		 *  @param[in] timestamp (unit:us)
		 *  @param[in] skeleton id.
		 *  @param[out] success or not.
		 **/
		public static extern Result DestroyBodyTracking(UInt64 ts, int skeletonId);

		[DllImport("bodytracking")]
		/**
		 *  @brief Get the amount of default skeleton joints.
		 *  @param[out] the amount of default skeleton joints.
		 *  @param[out] success or not.
		 **/
		public static extern Result GetDefaultSkeletonJointCount(ref UInt32 jointCount);

		[DllImport("bodytracking")]
		/**
		 *  @brief Get default skeleton rotate space.
		 *  @param[out] the rotate space of default skeleton.
		 *  @param[out] success or not.
		 * */
		public static extern Result GetDefaultSkeletonRotateSpace([In, Out] RotateSpace[] rotateSpace, UInt32 jointCount);
	}
	#endregion

	public static class BodyTrackingUtils
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.BodyTrackingUtils";
		static StringBuilder m_sb = null;
		static StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		static void DEBUG(StringBuilder msg) { Debug.Log(msg); }
		static void ERROR(StringBuilder msg) { Debug.LogError(msg); }

		#region Input System
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
		#endregion

		public static bool isZero(this Quaternion qua)
		{
			if (qua.x == 0 &&
				qua.y == 0 &&
				qua.z == 0 &&
				qua.w == 0)
			{
				return true;
			}

			return false;
		}
		public static void Validate(ref Quaternion qua)
		{
			if (qua.isZero()) { qua = Quaternion.identity; }
		}
		public static bool IsValid(this Quaternion quat)
		{
			if (quat.x > 1 || quat.x < -1 || float.IsNaN(quat.x)) { return false; }
			if (quat.y > 1 || quat.y < -1 || float.IsNaN(quat.y)) { return false; }
			if (quat.z > 1 || quat.z < -1 || float.IsNaN(quat.z)) { return false; }
			if (quat.w > 1 || quat.w < -1 || float.IsNaN(quat.w)) { return false; }
			return true;
		}
		public static void Update(ref Quaternion qua, Quaternion in_qua)
		{
			qua = in_qua;
			Validate(ref qua);
		}
		public static bool GetQuaternionDiff(Quaternion src, Quaternion dst, out Quaternion diff)
		{
			if (src.IsValid() && dst.IsValid())
			{
				diff = Quaternion.Inverse(src) * dst;
				Validate(ref diff);
				return true;
			}

			diff = Quaternion.identity;
			return false;
		}
		public static void Update(Quaternion qua, ref Vector4 vec)
		{
			vec.x = qua.x;
			vec.y = qua.y;
			vec.z = qua.z;
			vec.w = qua.w;
		}
		public static void Update(Vector4 vec, ref Quaternion qua)
		{
			qua.x = vec.x;
			qua.y = vec.y;
			qua.z = vec.z;
			qua.w = vec.w;
			Validate(ref qua);
		}

		public static string Name(this TrackedDeviceRole role)
		{
			if (role == TrackedDeviceRole.ROLE_CHEST) { return "CHEST"; }
			if (role == TrackedDeviceRole.ROLE_HEAD) { return "HEAD"; }
			if (role == TrackedDeviceRole.ROLE_HIP) { return "HIP"; }

			if (role == TrackedDeviceRole.ROLE_LEFTANKLE) { return "LEFTANKLE"; }
			if (role == TrackedDeviceRole.ROLE_LEFTELBOW) { return "LEFTELBOW"; }
			if (role == TrackedDeviceRole.ROLE_LEFTFOOT) { return "LEFTFOOT"; }
			if (role == TrackedDeviceRole.ROLE_LEFTHAND) { return "LEFTHAND"; }
			if (role == TrackedDeviceRole.ROLE_LEFTHANDHELD) { return "LEFTHANDHELD"; }
			if (role == TrackedDeviceRole.ROLE_LEFTKNEE) { return "LEFTKNEE"; }
			if (role == TrackedDeviceRole.ROLE_LEFTWRIST) { return "LEFTWRIST"; }

			if (role == TrackedDeviceRole.ROLE_RIGHTANKLE) { return "RIGHTANKLE"; }
			if (role == TrackedDeviceRole.ROLE_RIGHTELBOW) { return "RIGHTELBOW"; }
			if (role == TrackedDeviceRole.ROLE_RIGHTFOOT) { return "RIGHTFOOT"; }
			if (role == TrackedDeviceRole.ROLE_RIGHTHAND) { return "RIGHTHAND"; }
			if (role == TrackedDeviceRole.ROLE_RIGHTHANDHELD) { return "RIGHTHANDHELD"; }
			if (role == TrackedDeviceRole.ROLE_RIGHTKNEE) { return "RIGHTKNEE"; }
			if (role == TrackedDeviceRole.ROLE_RIGHTWRIST) { return "RIGHTWRIST"; }

			if (role == TrackedDeviceRole.ROLE_UNDEFINED) { return "UNDEFINED"; }

			sb.Clear().Append(LOG_TAG).Append("TrackedDeviceRole = ").Append(role); DEBUG(sb);
			return "";
		}
		public static string Name(this DeviceExtRole role)
		{
			if (role == DeviceExtRole.Arm_Wrist) { return "Arm_Wrist"; }
			if (role == DeviceExtRole.Arm_Handheld_Hand) { return "Arm_Handheld_Hand"; }

			if (role == DeviceExtRole.UpperBody_Wrist) { return "UpperBody_Wrist"; }
			if (role == DeviceExtRole.UpperBody_Handheld_Hand) { return "UpperBody_Handheld_Hand"; }

			if (role == DeviceExtRole.FullBody_Wrist_Ankle) { return "FullBody_Wrist_Ankle"; }
			if (role == DeviceExtRole.FullBody_Wrist_Foot) { return "FullBody_Wrist_Foot"; }
			if (role == DeviceExtRole.FullBody_Handheld_Hand_Ankle) { return "FullBody_Handheld_Hand_Ankle"; }
			if (role == DeviceExtRole.FullBody_Handheld_Hand_Foot) { return "FullBody_Handheld_Hand_Foot"; }

			if (role == DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle) { return "UpperBody_Handheld_Hand_Knee_Ankle"; }

			if (role == DeviceExtRole.Unknown) { return "Unknown"; }

			sb.Clear().Append(LOG_TAG).Append("DeviceExtRole = ").Append(role); DEBUG(sb);
			return "";
		}
		public static string Name(this BodyPoseRole role)
		{
			if (role == BodyPoseRole.Arm_Wrist) { return "Arm_Wrist"; }
			if (role == BodyPoseRole.Arm_Handheld) { return "Arm_Handheld"; }
			if (role == BodyPoseRole.Arm_Hand) { return "Arm_Hand"; }

			if (role == BodyPoseRole.UpperBody_Wrist) { return "UpperBody_Wrist"; }
			if (role == BodyPoseRole.UpperBody_Handheld) { return "UpperBody_Handheld"; }
			if (role == BodyPoseRole.UpperBody_Hand) { return "UpperBody_Hand"; }

			if (role == BodyPoseRole.FullBody_Wrist_Ankle) { return "FullBody_Wrist_Ankle"; }
			if (role == BodyPoseRole.FullBody_Wrist_Foot) { return "FullBody_Wrist_Foot"; }
			if (role == BodyPoseRole.FullBody_Handheld_Ankle) { return "FullBody_Handheld_Ankle"; }
			if (role == BodyPoseRole.FullBody_Handheld_Foot) { return "FullBody_Handheld_Foot"; }
			if (role == BodyPoseRole.FullBody_Hand_Ankle) { return "FullBody_Hand_Ankle"; }
			if (role == BodyPoseRole.FullBody_Hand_Foot) { return "FullBody_Hand_Foot"; }

			if (role == BodyPoseRole.UpperBody_Handheld_Knee_Ankle) { return "UpperBody_Handheld_Knee_Ankle"; }
			if (role == BodyPoseRole.UpperBody_Hand_Knee_Ankle) { return "UpperBody_Hand_Knee_Ankle"; }

			if (role == BodyPoseRole.Unknown) { return "Unknown"; }

			sb.Clear().Append(LOG_TAG).Append("BodyPoseRole = ").Append(role); DEBUG(sb);
			return "";
		}
		public static string Name(this BodyTrackingMode mode)
		{
			if (mode == BodyTrackingMode.ARMIK) { return "ARMIK"; }
			if (mode == BodyTrackingMode.FULLBODYIK) { return "FULLBODYIK"; }
			if (mode == BodyTrackingMode.LEGFK) { return "LEGFK"; }
			if (mode == BodyTrackingMode.LEGIK) { return "LEGIK"; }
			if (mode == BodyTrackingMode.SPINEIK) { return "SPINEIK"; }
			if (mode == BodyTrackingMode.UNKNOWNMODE) { return "Unknown"; }
			if (mode == BodyTrackingMode.UPPERBODYIK) { return "UPPERBODYIK"; }
			if (mode == BodyTrackingMode.UPPERIKANDLEGFK) { return "UPPERIKANDLEGFK"; }

			sb.Clear().Append(LOG_TAG).Append("BodyTrackingMode = ").Append(mode); DEBUG(sb);
			return "";
		}
		public static string Name(this BodyTrackingResult result)
		{
			if (result == BodyTrackingResult.ERROR_AVATAR_INIT_FAILED) { return "ERROR_AVATAR_INIT_FAILED"; }
			if (result == BodyTrackingResult.ERROR_BODYTRACKINGMODE_NOT_ALIGNED) { return "ERROR_BODYTRACKINGMODE_NOT_ALIGNED"; }
			if (result == BodyTrackingResult.ERROR_BODYTRACKINGMODE_NOT_FOUND) { return "ERROR_BODYTRACKINGMODE_NOT_FOUND"; }
			if (result == BodyTrackingResult.ERROR_CALIBRATE_FAILED) { return "ERROR_CALIBRATE_FAILED"; }
			if (result == BodyTrackingResult.ERROR_COMPUTE_FAILED) { return "ERROR_COMPUTE_FAILED"; }
			if (result == BodyTrackingResult.ERROR_FATAL_ERROR) { return "ERROR_FATAL_ERROR"; }
			if (result == BodyTrackingResult.ERROR_IK_NOT_DESTROYED) { return "ERROR_IK_NOT_DESTROYED"; }
			if (result == BodyTrackingResult.ERROR_IK_NOT_UPDATED) { return "ERROR_IK_NOT_UPDATED"; }
			if (result == BodyTrackingResult.ERROR_INPUTPOSE_NOT_VALID) { return "ERROR_INPUTPOSE_NOT_VALID"; }
			if (result == BodyTrackingResult.ERROR_INVALID_ARGUMENT) { return "ERROR_INVALID_ARGUMENT"; }
			if (result == BodyTrackingResult.ERROR_JOINT_NOT_FOUND) { return "ERROR_JOINT_NOT_FOUND"; }
			if (result == BodyTrackingResult.ERROR_NOT_CALIBRATED) { return "ERROR_NOT_CALIBRATED"; }
			if (result == BodyTrackingResult.ERROR_SKELETONID_NOT_FOUND) { return "ERROR_SKELETONID_NOT_FOUND"; }
			if (result == BodyTrackingResult.ERROR_SOLVER_NOT_FOUND) { return "ERROR_SOLVER_NOT_FOUND"; }
			if (result == BodyTrackingResult.ERROR_TABLE_STATIC) { return "ERROR_TABLE_STATIC"; }
			if (result == BodyTrackingResult.ERROR_TRACKER_AMOUNT_FAILED) { return "ERROR_TRACKER_AMOUNT_FAILED"; }
			if (result == BodyTrackingResult.SUCCESS) { return "SUCCESS"; }

			sb.Clear().Append(LOG_TAG).Append("BodyTrackingResult = ").Append(result); DEBUG(sb);
			return "";
		}
		public static string Name(this JointType type)
		{
			if (type == JointType.HIP) { return "HIP"; }

			if (type == JointType.LEFTTHIGH) { return "LEFTTHIGH"; }
			if (type == JointType.LEFTLEG) { return "LEFTLEG"; }
			if (type == JointType.LEFTANKLE) { return "LEFTANKLE"; }
			if (type == JointType.LEFTFOOT) { return "LEFTFOOT"; }

			if (type == JointType.RIGHTTHIGH) { return "RIGHTTHIGH"; }
			if (type == JointType.RIGHTLEG) { return "RIGHTLEG"; }
			if (type == JointType.RIGHTANKLE) { return "RIGHTANKLE"; }
			if (type == JointType.RIGHTFOOT) { return "RIGHTFOOT"; }

			if (type == JointType.WAIST) { return "WAIST"; }

			if (type == JointType.SPINELOWER) { return "SPINELOWER"; }
			if (type == JointType.SPINEMIDDLE) { return "SPINEMIDDLE"; }
			if (type == JointType.SPINEHIGH) { return "SPINEHIGH"; }

			if (type == JointType.CHEST) { return "CHEST"; }
			if (type == JointType.NECK) { return "NECK"; }
			if (type == JointType.HEAD) { return "HEAD"; }

			if (type == JointType.LEFTCLAVICLE) { return "LEFTCLAVICLE"; }
			if (type == JointType.LEFTSCAPULA) { return "LEFTSCAPULA"; }
			if (type == JointType.LEFTUPPERARM) { return "LEFTUPPERARM"; }
			if (type == JointType.LEFTFOREARM) { return "LEFTFOREARM"; }
			if (type == JointType.LEFTHAND) { return "LEFTHAND"; }

			if (type == JointType.RIGHTCLAVICLE) { return "RIGHTCLAVICLE"; }
			if (type == JointType.RIGHTSCAPULA) { return "RIGHTSCAPULA"; }
			if (type == JointType.RIGHTUPPERARM) { return "RIGHTUPPERARM"; }
			if (type == JointType.RIGHTFOREARM) { return "RIGHTFOREARM"; }
			if (type == JointType.RIGHTHAND) { return "RIGHTHAND"; }

			sb.Clear().Append(LOG_TAG).Append("JointType = ").Append(type); DEBUG(sb);
			return "";
		}
		public static string Name(this CalibrationType type)
		{
			if (type == CalibrationType.DEFAULTCALIBRATION) { return "DEFAULTCALIBRATION"; }
			if (type == CalibrationType.HEIGHTCALIBRATION) { return "HEIGHTCALIBRATION"; }
			if (type == CalibrationType.TOFFSETCALIBRATION) { return "TOFFSETCALIBRATION"; }

			sb.Clear().Append(LOG_TAG).Append("CalibrationType = ").Append(type); DEBUG(sb);
			return "";
		}

		public static BodyTrackingResult Type(this Result result)
		{
			if (result == Result.ERROR_AVATAR_INIT_FAILED) { return BodyTrackingResult.ERROR_AVATAR_INIT_FAILED; }
			if (result == Result.ERROR_BODYTRACKINGMODE_NOT_ALIGNED) { return BodyTrackingResult.ERROR_BODYTRACKINGMODE_NOT_ALIGNED; }
			if (result == Result.ERROR_BODYTRACKINGMODE_NOT_FOUND) { return BodyTrackingResult.ERROR_BODYTRACKINGMODE_NOT_FOUND; }
			if (result == Result.ERROR_CALIBRATE_FAILED) { return BodyTrackingResult.ERROR_CALIBRATE_FAILED; }
			if (result == Result.ERROR_COMPUTE_FAILED) { return BodyTrackingResult.ERROR_COMPUTE_FAILED; }
			if (result == Result.ERROR_FATAL_ERROR) { return BodyTrackingResult.ERROR_FATAL_ERROR; }
			if (result == Result.ERROR_INPUTPOSE_NOT_VALID) { return BodyTrackingResult.ERROR_INPUTPOSE_NOT_VALID; }
			if (result == Result.ERROR_JOINT_NOT_FOUND) { return BodyTrackingResult.ERROR_JOINT_NOT_FOUND; }
			if (result == Result.ERROR_NOT_CALIBRATED) { return BodyTrackingResult.ERROR_NOT_CALIBRATED; }
			if (result == Result.ERROR_NOT_INITIALIZATION) { return BodyTrackingResult.ERROR_NOT_INITIALIZATION; }
			if (result == Result.ERROR_SKELETONID_NOT_FOUND) { return BodyTrackingResult.ERROR_SKELETONID_NOT_FOUND; }
			if (result == Result.ERROR_SOLVER_NOT_FOUND) { return BodyTrackingResult.ERROR_SOLVER_NOT_FOUND; }
			if (result == Result.ERROR_TABLE_STATIC) { return BodyTrackingResult.ERROR_TABLE_STATIC; }
			if (result == Result.ERROR_TRACKER_AMOUNT_FAILED) { return BodyTrackingResult.ERROR_TRACKER_AMOUNT_FAILED; }

			return BodyTrackingResult.SUCCESS;
		}

		public static void Update([In] Joint joint, ref Extrinsic ext)
		{
			ext.translation = joint.translation;
			BodyTrackingUtils.Update(ref ext.rotation, joint.rotation);
		}

		readonly static DateTime kBeginTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		public static UInt64 GetTimeStamp(bool bflag = true)
		{
			TimeSpan ts = DateTime.UtcNow - kBeginTime;
			return Convert.ToUInt64(ts.TotalMilliseconds);
		}

		/// <summary> Retrieves the body pose role according to the currently tracked device poses. </summary>
		public static BodyPoseRole GetBodyPoseRole([In] TrackedDevicePose[] trackedDevicePoses, [In] UInt32 trackedDevicePoseCount)
		{
			UInt64 ikRoles = 0;
			sb.Clear().Append(LOG_TAG);
			for (UInt32 i = 0; i < trackedDevicePoseCount; i++)
			{
				sb.Append("GetBodyPoseRole() pose ").Append(i)
					.Append(" role ").Append(trackedDevicePoses[i].trackedDeviceRole.Name())
					.Append("\n");
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
				ikRoles |= (UInt64)(1 << (Int32)trackedDevicePoses[i].trackedDeviceRole);
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
			}
			DEBUG(sb);

			BodyPoseRole m_IKRoles = BodyPoseRole.Unknown;

			// Upper Body + Leg FK
			if ((ikRoles & (UInt64)BodyPoseRole.UpperBody_Handheld_Knee_Ankle) == (UInt64)BodyPoseRole.UpperBody_Handheld_Knee_Ankle)
				m_IKRoles = BodyPoseRole.UpperBody_Handheld_Knee_Ankle;
			else if ((ikRoles & (UInt64)BodyPoseRole.UpperBody_Hand_Knee_Ankle) == (UInt64)BodyPoseRole.UpperBody_Hand_Knee_Ankle)
				m_IKRoles = BodyPoseRole.UpperBody_Hand_Knee_Ankle;

			// ToDo: else if {Hybrid mode}

			// Full body
			else if ((ikRoles & (UInt64)BodyPoseRole.FullBody_Wrist_Ankle) == (UInt64)BodyPoseRole.FullBody_Wrist_Ankle)
				m_IKRoles = BodyPoseRole.FullBody_Wrist_Ankle;
			else if ((ikRoles & (UInt64)BodyPoseRole.FullBody_Wrist_Foot) == (UInt64)BodyPoseRole.FullBody_Wrist_Foot)
				m_IKRoles = BodyPoseRole.FullBody_Wrist_Foot;
			else if ((ikRoles & (UInt64)BodyPoseRole.FullBody_Handheld_Ankle) == (UInt64)BodyPoseRole.FullBody_Handheld_Ankle)
				m_IKRoles = BodyPoseRole.FullBody_Handheld_Ankle;
			else if ((ikRoles & (UInt64)BodyPoseRole.FullBody_Handheld_Foot) == (UInt64)BodyPoseRole.FullBody_Handheld_Foot)
				m_IKRoles = BodyPoseRole.FullBody_Handheld_Foot;
			else if ((ikRoles & (UInt64)BodyPoseRole.FullBody_Hand_Ankle) == (UInt64)BodyPoseRole.FullBody_Hand_Ankle)
				m_IKRoles = BodyPoseRole.FullBody_Hand_Ankle;
			else if ((ikRoles & (UInt64)BodyPoseRole.FullBody_Hand_Foot) == (UInt64)BodyPoseRole.FullBody_Hand_Foot)
				m_IKRoles = BodyPoseRole.FullBody_Hand_Foot;

			// Upper body
			else if ((ikRoles & (UInt64)BodyPoseRole.UpperBody_Wrist) == (UInt64)BodyPoseRole.UpperBody_Wrist)
				m_IKRoles = BodyPoseRole.UpperBody_Wrist;
			else if ((ikRoles & (UInt64)BodyPoseRole.UpperBody_Handheld) == (UInt64)BodyPoseRole.UpperBody_Handheld)
				m_IKRoles = BodyPoseRole.UpperBody_Handheld;
			else if ((ikRoles & (UInt64)BodyPoseRole.UpperBody_Hand) == (UInt64)BodyPoseRole.UpperBody_Hand)
				m_IKRoles = BodyPoseRole.UpperBody_Hand;

			// Arm
			else if ((ikRoles & (UInt64)BodyPoseRole.Arm_Wrist) == (UInt64)BodyPoseRole.Arm_Wrist)
				m_IKRoles = BodyPoseRole.Arm_Wrist;
			else if ((ikRoles & (UInt64)BodyPoseRole.Arm_Handheld) == (UInt64)BodyPoseRole.Arm_Handheld)
				m_IKRoles = BodyPoseRole.Arm_Handheld;
			else if ((ikRoles & (UInt64)BodyPoseRole.Arm_Hand) == (UInt64)BodyPoseRole.Arm_Hand)
				m_IKRoles = BodyPoseRole.Arm_Hand;

			sb.Clear().Append(LOG_TAG).Append("GetBodyPoseRole() role: ").Append(m_IKRoles.Name()); DEBUG(sb);
			return m_IKRoles;
		}
		/// <summary> Checks if the body pose role and body tracking mode are matched./// </summary>
		public static bool MatchBodyTrackingMode(BodyTrackingMode mode, BodyPoseRole poseRole)
		{
			sb.Clear().Append(LOG_TAG).Append("MatchBodyTrackingMode() mode: ").Append(mode.Name()).Append(", poseRole: ").Append(poseRole.Name()); DEBUG(sb);

			if (poseRole == BodyPoseRole.UpperBody_Handheld_Knee_Ankle || poseRole == BodyPoseRole.UpperBody_Hand_Knee_Ankle)
			{
				if (mode == BodyTrackingMode.UPPERIKANDLEGFK)
					return true;
			}
			if (poseRole == BodyPoseRole.FullBody_Wrist_Ankle || poseRole == BodyPoseRole.FullBody_Wrist_Foot ||
				poseRole == BodyPoseRole.FullBody_Handheld_Ankle || poseRole == BodyPoseRole.FullBody_Handheld_Foot ||
				poseRole == BodyPoseRole.FullBody_Hand_Ankle || poseRole == BodyPoseRole.FullBody_Hand_Foot)
			{
				if (mode == BodyTrackingMode.FULLBODYIK || mode == BodyTrackingMode.UPPERBODYIK || mode == BodyTrackingMode.ARMIK)
					return true;
			}
			if (poseRole == BodyPoseRole.UpperBody_Wrist || poseRole == BodyPoseRole.UpperBody_Handheld || poseRole == BodyPoseRole.UpperBody_Hand)
			{
				if (mode == BodyTrackingMode.UPPERBODYIK || mode == BodyTrackingMode.ARMIK)
					return true;
			}
			if (poseRole == BodyPoseRole.Arm_Wrist || poseRole == BodyPoseRole.Arm_Handheld || poseRole == BodyPoseRole.Arm_Hand)
			{
				if (mode == BodyTrackingMode.ARMIK)
					return true;
			}

			return false;
		}

		/// <summary> Retrievs the device extrinsic role according to the calibration pose role and tracked device extrinsics in use. </summary>
		public static DeviceExtRole GetDeviceExtRole(BodyPoseRole calibRole, [In] TrackedDeviceExtrinsic[] bodyTrackedDevices, [In] UInt32 bodyTrackedDeviceCount)
		{
			sb.Clear().Append(LOG_TAG).Append("GetDeviceExtRole() calibRole: ").Append(calibRole.Name()); DEBUG(sb);

			UInt64 ikRoles = 0;
			sb.Clear().Append(LOG_TAG);
			for (UInt32 i = 0; i < bodyTrackedDeviceCount; i++)
			{
				sb.Append("GetDeviceExtRole() device ").Append(i)
					.Append(" role ").Append(bodyTrackedDevices[i].trackedDeviceRole.Name())
					.Append("\n");
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
				ikRoles |= (UInt64)(1 << (Int32)bodyTrackedDevices[i].trackedDeviceRole);
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
			}
			DEBUG(sb);

			DeviceExtRole m_IKRoles = DeviceExtRole.Unknown;

			// Upper Body + Leg FK
			if (calibRole == BodyPoseRole.UpperBody_Handheld_Knee_Ankle || calibRole == BodyPoseRole.UpperBody_Hand_Knee_Ankle)
			{
				if ((ikRoles & (UInt64)DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle) == (UInt64)DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle)
					m_IKRoles = DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle;
			}

			// Full Body
			if (calibRole == BodyPoseRole.FullBody_Wrist_Ankle)
			{
				if ((ikRoles & (UInt64)DeviceExtRole.FullBody_Wrist_Ankle) == (UInt64)DeviceExtRole.FullBody_Wrist_Ankle)
					m_IKRoles = DeviceExtRole.FullBody_Wrist_Ankle;
			}
			if (calibRole == BodyPoseRole.FullBody_Wrist_Foot)
			{
				if ((ikRoles & (UInt64)DeviceExtRole.FullBody_Wrist_Foot) == (UInt64)DeviceExtRole.FullBody_Wrist_Foot)
					m_IKRoles = DeviceExtRole.FullBody_Wrist_Foot;
			}
			if (calibRole == BodyPoseRole.FullBody_Handheld_Ankle || calibRole == BodyPoseRole.FullBody_Hand_Ankle)
			{
				if ((ikRoles & (UInt64)DeviceExtRole.FullBody_Handheld_Hand_Ankle) == (UInt64)DeviceExtRole.FullBody_Handheld_Hand_Ankle)
					m_IKRoles = DeviceExtRole.FullBody_Handheld_Hand_Ankle;
			}
			if (calibRole == BodyPoseRole.FullBody_Handheld_Foot || calibRole == BodyPoseRole.FullBody_Hand_Foot)
			{
				if ((ikRoles & (UInt64)DeviceExtRole.FullBody_Handheld_Hand_Foot) == (UInt64)DeviceExtRole.FullBody_Handheld_Hand_Foot)
					m_IKRoles = DeviceExtRole.FullBody_Handheld_Hand_Foot;
			}

			// Upper Body
			if (calibRole == BodyPoseRole.UpperBody_Wrist)
			{
				if ((ikRoles & (UInt64)DeviceExtRole.UpperBody_Wrist) == (UInt64)DeviceExtRole.UpperBody_Wrist)
					m_IKRoles = DeviceExtRole.UpperBody_Wrist;
			}
			if (calibRole == BodyPoseRole.UpperBody_Handheld || calibRole == BodyPoseRole.UpperBody_Hand)
			{
				if ((ikRoles & (UInt64)DeviceExtRole.UpperBody_Handheld_Hand) == (UInt64)DeviceExtRole.UpperBody_Handheld_Hand)
					m_IKRoles = DeviceExtRole.UpperBody_Handheld_Hand;
			}

			// Arm
			if (calibRole == BodyPoseRole.Arm_Wrist)
			{
				if ((ikRoles & (UInt64)DeviceExtRole.Arm_Wrist) == (UInt64)DeviceExtRole.Arm_Wrist)
					m_IKRoles = DeviceExtRole.Arm_Wrist;
			}
			if (calibRole == BodyPoseRole.Arm_Handheld || calibRole == BodyPoseRole.Arm_Hand)
			{
				if ((ikRoles & (UInt64)DeviceExtRole.Arm_Handheld_Hand) == (UInt64)DeviceExtRole.Arm_Handheld_Hand)
					m_IKRoles = DeviceExtRole.Arm_Handheld_Hand;
			}

			sb.Clear().Append(LOG_TAG).Append("GetDeviceExtRole() role: ").Append(m_IKRoles.Name()); DEBUG(sb);
			return m_IKRoles;
		}
		/// <summary> Checks if the device extrinsic role and body tracking mode are matched./// </summary>
		public static bool MatchBodyTrackingMode(BodyTrackingMode mode, DeviceExtRole extRole)
		{
			sb.Clear().Append(LOG_TAG).Append("MatchBodyTrackingMode() mode: ").Append(mode.Name()).Append(", extRole: ").Append(extRole.Name()); DEBUG(sb);

			if (mode == BodyTrackingMode.ARMIK)
			{
				if (extRole == DeviceExtRole.Arm_Wrist || extRole == DeviceExtRole.Arm_Handheld_Hand)
					return true;
			}
			if (mode == BodyTrackingMode.UPPERBODYIK)
			{
				if (extRole == DeviceExtRole.UpperBody_Wrist || extRole == DeviceExtRole.UpperBody_Handheld_Hand)
					return true;
			}
			if (mode == BodyTrackingMode.FULLBODYIK)
			{
				if (extRole == DeviceExtRole.FullBody_Wrist_Ankle ||
					extRole == DeviceExtRole.FullBody_Wrist_Foot ||
					extRole == DeviceExtRole.FullBody_Handheld_Hand_Ankle ||
					extRole == DeviceExtRole.FullBody_Handheld_Hand_Foot)
					return true;
			}
			if (mode == BodyTrackingMode.UPPERIKANDLEGFK)
			{
				if (extRole == DeviceExtRole.UpperBody_Handheld_Hand_Knee_Ankle)
					return true;
			}

			return false;
		}

		private static TrackedDeviceRole[] s_DeviceRoleTracked =
		{
			TrackedDeviceRole.ROLE_HIP,
			TrackedDeviceRole.ROLE_CHEST,
			TrackedDeviceRole.ROLE_HEAD,

			TrackedDeviceRole.ROLE_LEFTELBOW,
			TrackedDeviceRole.ROLE_LEFTWRIST,
			TrackedDeviceRole.ROLE_LEFTHAND, // 5
			TrackedDeviceRole.ROLE_LEFTHANDHELD,

			TrackedDeviceRole.ROLE_RIGHTELBOW,
			TrackedDeviceRole.ROLE_RIGHTWRIST,
			TrackedDeviceRole.ROLE_RIGHTHAND,
			TrackedDeviceRole.ROLE_RIGHTHANDHELD, // 10

			TrackedDeviceRole.ROLE_LEFTKNEE,
			TrackedDeviceRole.ROLE_LEFTANKLE,
			TrackedDeviceRole.ROLE_LEFTFOOT,

			TrackedDeviceRole.ROLE_RIGHTKNEE,
			TrackedDeviceRole.ROLE_RIGHTANKLE, // 15
			TrackedDeviceRole.ROLE_RIGHTFOOT,
		};
		public static TrackedDeviceRole GetTrackedDeviceRole(int id)
		{
			if (id >= 0) { return s_DeviceRoleTracked[id]; }
			return TrackedDeviceRole.ROLE_UNDEFINED;
		}

		public static void GetQuaternion(this XrQuaternionf xrQuat, out Quaternion quat)
		{
			quat.x = xrQuat.x;
			quat.y = xrQuat.y;
			quat.z = -xrQuat.z;
			quat.w = -xrQuat.w;
			Validate(ref quat);
		}
		public static void GetVector3(this XrVector3f xrVec3, out Vector3 vec3)
		{
			vec3.x = xrVec3.x;
			vec3.y = xrVec3.y;
			vec3.z = -xrVec3.z;
		}
		public static BodyTrackingResult UpdateTrackedDevicePose(TrackedDeviceRole role, InputActionReference action, ref TrackedDevicePose pose)
		{
			if (role == TrackedDeviceRole.ROLE_UNDEFINED) { return BodyTrackingResult.ERROR_INVALID_ARGUMENT; }

			string func = "UpdateTrackedDevicePose() ";
			if (action != null)
			{
				if (GetPoseIsTracked(action, out bool isTracked, out string error) && isTracked &&
					GetPoseTrackingState(action, out InputTrackingState state, out error))
				{
					if (state.HasFlag(InputTrackingState.Rotation))
					{
						if (!GetPoseRotation(action, out pose.rotation, out error))
						{
							sb.Clear().Append(LOG_TAG).Append(func).Append("Invalid ").Append(role.Name()).Append(" rotation, error: ").Append(error); ERROR(sb);
							return BodyTrackingResult.ERROR_INPUTPOSE_NOT_VALID;
						}
						pose.poseState |= PoseState.ROTATION;
					}
					if (state.HasFlag(InputTrackingState.Position))
					{
						if (!GetPosePosition(action, out pose.translation, out error))
						{
							sb.Clear().Append(LOG_TAG).Append(func).Append("Invalid ").Append(role.Name()).Append(" position, error: ").Append(error); ERROR(sb);
							return BodyTrackingResult.ERROR_INPUTPOSE_NOT_VALID;
						}
						pose.poseState |= PoseState.TRANSLATION;
					}
				}
			}
			else
			{
				if (role == TrackedDeviceRole.ROLE_HEAD || role == TrackedDeviceRole.ROLE_LEFTHANDHELD || role == TrackedDeviceRole.ROLE_RIGHTHANDHELD)
				{
					InputDeviceControl.ControlDevice device = InputDeviceControl.ControlDevice.Head;
					if (role == TrackedDeviceRole.ROLE_LEFTHANDHELD) { device = InputDeviceControl.ControlDevice.Left; }
					if (role == TrackedDeviceRole.ROLE_RIGHTHANDHELD) { device = InputDeviceControl.ControlDevice.Right; }

					if (!InputDeviceControl.IsTracked(device) ||
						!InputDeviceControl.GetRotation(device, out Quaternion rotation) ||
						!InputDeviceControl.GetPosition(device, out Vector3 position))
					{
						return BodyTrackingResult.ERROR_INPUTPOSE_NOT_VALID;
					}

					pose.rotation = rotation;
					pose.translation = position;
					pose.poseState |= PoseState.ROTATION | PoseState.TRANSLATION;
				}
				if (role == TrackedDeviceRole.ROLE_LEFTHAND || role == TrackedDeviceRole.ROLE_RIGHTHAND)
				{
					bool isLeft = (role == TrackedDeviceRole.ROLE_LEFTHAND);
					if (XR_EXT_hand_tracking.Interop.GetJointLocations(isLeft, out XrHandJointLocationEXT[] handJointLocation))
					{
						int palm = (int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT;
						if (((UInt64)handJointLocation[palm].locationFlags & (UInt64)XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT) != 0)
						{
							handJointLocation[palm].pose.orientation.GetQuaternion(out pose.rotation);
							pose.poseState |= PoseState.ROTATION;
						}
						if (((UInt64)handJointLocation[palm].locationFlags & (UInt64)XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_TRACKED_BIT) != 0)
						{
							handJointLocation[(int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT].pose.position.GetVector3(out pose.translation);
							pose.poseState |= PoseState.TRANSLATION;
						}
					}
					// Hand pose can be NODATA.
				}
			}
			return BodyTrackingResult.SUCCESS;
		}
		public static bool PoseStateAvailable(BodyTrackingMode mode, [In] TrackedDevicePose pose, bool canIgnorePose = false)
		{
			// Only Controller and Hand pose can be ignored.
			if (pose.trackedDeviceRole == TrackedDeviceRole.ROLE_LEFTHANDHELD ||
				pose.trackedDeviceRole == TrackedDeviceRole.ROLE_RIGHTHANDHELD ||
				pose.trackedDeviceRole == TrackedDeviceRole.ROLE_LEFTHAND ||
				pose.trackedDeviceRole == TrackedDeviceRole.ROLE_RIGHTHAND)
			{
				if (canIgnorePose)
					return true;
			}
			// 6DoF pose is allowed.
			if (pose.poseState == (PoseState.ROTATION | PoseState.TRANSLATION))
				return true;
			// Tracker can be rotation-only in UPPERIKANDLEGFK mode.
			if (mode == BodyTrackingMode.UPPERIKANDLEGFK)
			{
				if (pose.trackedDeviceRole == TrackedDeviceRole.ROLE_LEFTKNEE ||
					pose.trackedDeviceRole == TrackedDeviceRole.ROLE_RIGHTKNEE ||
					pose.trackedDeviceRole == TrackedDeviceRole.ROLE_LEFTANKLE ||
					pose.trackedDeviceRole == TrackedDeviceRole.ROLE_RIGHTANKLE)
				{
					if ((pose.poseState & PoseState.ROTATION) == PoseState.ROTATION)
						return true;
				}
			}

			return false;
		}
	}

	public static class InputDeviceControl
	{
		public enum ControlDevice
		{
			Head = 1,
			Right = 2,
			Left = 3,
		}

		/// <summary> Wave Head Mounted Device Characteristics </summary>
		public const InputDeviceCharacteristics kHMDCharacteristics = (
			InputDeviceCharacteristics.HeadMounted |
			InputDeviceCharacteristics.TrackedDevice
		);
		public const uint kHMDCharacteristicsValue = (uint)kHMDCharacteristics;
		/// <summary> Wave Left Controller Characteristics </summary>
		public const InputDeviceCharacteristics kControllerLeftCharacteristics = (
			InputDeviceCharacteristics.Left |
			InputDeviceCharacteristics.TrackedDevice |
			InputDeviceCharacteristics.Controller |
			InputDeviceCharacteristics.HeldInHand
		);
		public const uint kControllerLeftCharacteristicsValue = (uint)kControllerLeftCharacteristics;
		/// <summary> Wave Right Controller Characteristics </summary>
		public const InputDeviceCharacteristics kControllerRightCharacteristics = (
			InputDeviceCharacteristics.Right |
			InputDeviceCharacteristics.TrackedDevice |
			InputDeviceCharacteristics.Controller |
			InputDeviceCharacteristics.HeldInHand
		);
		public const uint kControllerRightCharacteristicsValue = (uint)kControllerRightCharacteristics;
		private static uint GetIndex(uint value)
		{
			if (value == kHMDCharacteristicsValue) { return 1; }
			if (value == kControllerLeftCharacteristicsValue) { return 2; }
			if (value == kControllerRightCharacteristicsValue) { return 3; }
			return 0;
		}

		public static InputDeviceCharacteristics characteristic(this ControlDevice cd)
		{
			return (
				cd == ControlDevice.Head ? kHMDCharacteristics : (
					cd == ControlDevice.Right ? kControllerRightCharacteristics : kControllerLeftCharacteristics
				)
			);
		}

		internal static List<InputDevice> m_InputDevices = new List<InputDevice>();
		internal static int inputDeviceFrame = -1;
		private static void UpdateInputDevices()
		{
			if (inputDeviceFrame != Time.frameCount)
			{
				inputDeviceFrame = Time.frameCount;
				InputDevices.GetDevices(m_InputDevices);
			}
		}

		/// Tracking state
		private static bool[] s_IsConnected = new bool[4] { false, false, false, false };
		private static int[] isConnectedFrame = new int[4] { -1, -1, -1, -1 };
		private static bool UpdateConnectedDevice(InputDeviceCharacteristics device, out uint index)
		{
			index = GetIndex((uint)device);
			if (index == 0) { return false; }

			if (isConnectedFrame[index] != Time.frameCount)
			{
				isConnectedFrame[index] = Time.frameCount;
				return true;
			}
			return false;
		}
		public static bool IsConnected(InputDeviceCharacteristics device)
		{
			if (!UpdateConnectedDevice(device, out uint index)) { return s_IsConnected[index]; }

			UpdateInputDevices();
			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						s_IsConnected[index] = true;
						return s_IsConnected[index];
					}
				}
			}

			s_IsConnected[index] = false;
			return s_IsConnected[index];
		}
		public static bool IsConnected(ControlDevice device) { return IsConnected(device.characteristic()); }

		private static bool[] s_IsTracked = new bool[4] { false, false, false, false };
		private static int[] isTrackedFrame = new int[4] { -1, -1, -1, -1 };
		private static bool UpdateTrackedDevice(InputDeviceCharacteristics device, out uint index)
		{
			index = GetIndex((uint)device);
			if (index == 0) { return false; }

			if (isTrackedFrame[index] != Time.frameCount)
			{
				isTrackedFrame[index] = Time.frameCount;
				return true;
			}
			return false;
		}
		public static bool IsTracked(InputDeviceCharacteristics device)
		{
			if (!UpdateTrackedDevice(device, out uint index)) { return s_IsTracked[index]; }
			if (!IsConnected(device)) { return false; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetFeatureValue(CommonUsages.isTracked, out s_IsTracked[index]))
							return s_IsTracked[index];
					}
				}
			}

			return false;
		}
		public static bool IsTracked(ControlDevice device) { return IsTracked(device.characteristic()); }

		/// Button
		public static bool KeyDown(InputDeviceCharacteristics device, InputFeatureUsage<bool> button)
		{
			if (!IsConnected(device)) { return false; }

			bool isDown = false;
			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetFeatureValue(button, out bool value))
							isDown = value;
					}
				}
			}

			return isDown;
		}
		public static bool KeyDown(ControlDevice device, InputFeatureUsage<bool> button) { return KeyDown(device.characteristic(), button); }
		public static bool KeyAxis1D(InputDeviceCharacteristics device, InputFeatureUsage<float> button, out float axis1d)
		{
			axis1d = 0;
			if (!IsConnected(device)) { return false; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetFeatureValue(button, out float value))
						{
							axis1d = value;
							return true;
						}
					}
				}
			}

			return false;
		}
		public static bool KeyAxis1D(ControlDevice device, InputFeatureUsage<float> button, out float axis1d) { return KeyAxis1D(device.characteristic(), button, out axis1d); }
		public static bool KeyAxis2D(InputDeviceCharacteristics device, InputFeatureUsage<Vector2> button, out Vector2 axis2d)
		{
			axis2d = Vector2.zero;
			if (!IsConnected(device)) { return false; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetFeatureValue(button, out Vector2 value))
						{
							axis2d = value;
							return true;
						}
					}
				}
			}

			return false;
		}
		public static bool KeyAxis2D(ControlDevice device, InputFeatureUsage<Vector2> button, out Vector2 axis2d) { return KeyAxis2D(device.characteristic(), button, out axis2d); }

		/// Haptic
		static readonly HapticCapabilities emptyHapticCapabilities = new HapticCapabilities();
		private static HapticCapabilities[] s_HapticCaps = new HapticCapabilities[4] { emptyHapticCapabilities, emptyHapticCapabilities, emptyHapticCapabilities, emptyHapticCapabilities };
		private static int[] hapticCapFrame = new int[4] { -1, -1, -1, -1 };
		private static bool UpdateHapticCapabilities(InputDeviceCharacteristics device, out uint index)
		{
			index = GetIndex((uint)device);
			if (index == 0) { return false; }

			if (hapticCapFrame[index] != Time.frameCount)
			{
				hapticCapFrame[index] = Time.frameCount;
				return true;
			}
			return false;
		}
		public static bool TryGetHapticCapabilities(InputDeviceCharacteristics device, out HapticCapabilities hapticCaps)
		{
			hapticCaps = emptyHapticCapabilities;
			if (!IsConnected(device)) { return false; }
			if (!UpdateHapticCapabilities(device, out uint index)) { hapticCaps = s_HapticCaps[index]; return true; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetHapticCapabilities(out s_HapticCaps[index]))
						{
							hapticCaps = s_HapticCaps[index];
							return true;
						}
					}
				}
			}

			return false;
		}
		public static bool TryGetHapticCapabilities(ControlDevice device, out HapticCapabilities hapticCaps) { return TryGetHapticCapabilities(device.characteristic(), out hapticCaps); }
		public static bool SendHapticImpulse(InputDeviceCharacteristics device, float amplitude, float duration)
		{
			if (!IsConnected(device)) { return false; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetHapticCapabilities(out HapticCapabilities value))
						{
							if (value.supportsImpulse)
							{
								amplitude = Mathf.Clamp(amplitude, 0, 1);
								return m_InputDevices[i].SendHapticImpulse(0, amplitude, duration);
							}
						}
					}
				}
			}

			return false;
		}
		public static bool SendHapticImpulse(ControlDevice device, float amplitude, float duration) { return SendHapticImpulse(device.characteristic(), amplitude, duration); }

		/// Pose
		private static Vector3[] s_Positions = new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
		private static int[] positionFrame = new int[4] { -1, -1, -1, -1 };
		private static bool UpdatePosition(InputDeviceCharacteristics device, out uint index)
		{
			index = GetIndex((uint)device);
			if (index == 0) { return false; }

			if (positionFrame[index] != Time.frameCount)
			{
				positionFrame[index] = Time.frameCount;
				return true;
			}
			return false;
		}
		public static bool GetPosition(InputDeviceCharacteristics device, out Vector3 position)
		{
			position = Vector3.zero;
			if (!IsTracked(device)) { return false; }
			if (!UpdatePosition(device, out uint index)) { position = s_Positions[index]; return true; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetFeatureValue(CommonUsages.devicePosition, out s_Positions[index]))
						{
							position = s_Positions[index];
							return true;
						}
					}
				}
			}

			return false;
		}
		public static bool GetPosition(ControlDevice device, out Vector3 position) { return GetPosition(device.characteristic(), out position); }

		private static Quaternion[] s_Rotations = new Quaternion[4] { Quaternion.identity, Quaternion.identity, Quaternion.identity, Quaternion.identity };
		private static int[] rotationFrame = new int[4] { -1, -1, -1, -1 };
		private static bool UpdateRotation(InputDeviceCharacteristics device, out uint index)
		{
			index = GetIndex((uint)device);
			if (index == 0) { return false; }

			if (rotationFrame[index] != Time.frameCount)
			{
				rotationFrame[index] = Time.frameCount;
				return true;
			}
			return false;
		}
		public static bool GetRotation(InputDeviceCharacteristics device, out Quaternion rotation)
		{
			rotation = Quaternion.identity;
			if (!IsTracked(device)) { return false; }
			if (!UpdateRotation(device, out uint index)) { rotation = s_Rotations[index]; return true; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetFeatureValue(CommonUsages.deviceRotation, out s_Rotations[index]))
						{
							rotation = s_Rotations[index];
							return true;
						}
					}
				}
			}

			return false;
		}
		public static bool GetRotation(ControlDevice device, out Quaternion rotation) { return GetRotation(device.characteristic(), out rotation); }

		private static Vector3[] s_Velocity = new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
		private static int[] velocityFrame = new int[4] { -1, -1, -1, -1 };
		private static bool UpdateVelocity(InputDeviceCharacteristics device, out uint index)
		{
			index = GetIndex((uint)device);
			if (index == 0) { return false; }

			if (velocityFrame[index] != Time.frameCount)
			{
				velocityFrame[index] = Time.frameCount;
				return true;
			}
			return false;
		}
		public static bool GetVelocity(InputDeviceCharacteristics device, out Vector3 velocity)
		{
			velocity = Vector3.zero;
			if (!IsTracked(device)) { return false; }
			if (!UpdateVelocity(device, out uint index)) { velocity = s_Velocity[index]; return true; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetFeatureValue(CommonUsages.deviceVelocity, out s_Velocity[index]))
						{
							velocity = s_Velocity[index];
							return true;
						}
					}
				}
			}

			return false;
		}
		public static bool GetVelocity(ControlDevice device, out Vector3 velocity) { return GetVelocity(device.characteristic(), out velocity); }

		private static Vector3[] s_AngularVelocity = new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
		private static int[] angularVelocityFrame = new int[4] { -1, -1, -1, -1 };
		private static bool UpdateAngularVelocity(InputDeviceCharacteristics device, out uint index)
		{
			index = GetIndex((uint)device);
			if (index == 0) { return false; }

			if (angularVelocityFrame[index] != Time.frameCount)
			{
				angularVelocityFrame[index] = Time.frameCount;
				return true;
			}
			return false;
		}
		public static bool GetAngularVelocity(InputDeviceCharacteristics device, out Vector3 angularVelocity)
		{
			angularVelocity = Vector3.zero;
			if (!IsTracked(device)) { return false; }
			if (!UpdateAngularVelocity(device, out uint index)) { angularVelocity = s_AngularVelocity[index]; return true; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out s_AngularVelocity[index]))
						{
							angularVelocity = s_AngularVelocity[index];
							return true;
						}
					}
				}
			}

			return false;
		}
		public static bool GetAngularVelocity(ControlDevice device, out Vector3 angularVelocity) { return GetAngularVelocity(device.characteristic(), out angularVelocity); }

		private static Vector3[] s_Acceleration = new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
		private static int[] accelerationFrame = new int[4] { -1, -1, -1, -1 };
		private static bool UpdateAcceleration(InputDeviceCharacteristics device, out uint index)
		{
			index = GetIndex((uint)device);
			if (index == 0) { return false; }

			if (accelerationFrame[index] != Time.frameCount)
			{
				accelerationFrame[index] = Time.frameCount;
				return true;
			}
			return false;
		}
		public static bool GetAcceleration(InputDeviceCharacteristics device, out Vector3 acceleration)
		{
			acceleration = Vector3.zero;
			if (!IsTracked(device)) { return false; }
			if (!UpdateAcceleration(device, out uint index)) { acceleration = s_Acceleration[index]; return true; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetFeatureValue(CommonUsages.deviceAcceleration, out s_Acceleration[index]))
						{
							acceleration = s_Acceleration[index];
							return true;
						}
					}
				}
			}

			return false;
		}
		public static bool GetAcceleration(ControlDevice device, out Vector3 acceleration) { return GetAcceleration(device.characteristic(), out acceleration); }

		/// Battery
		private static float[] s_BatteryLevels = new float[4] { 0, 0, 0, 0 };
		private static int[] batteryFrame = new int[4] { -1, -1, -1, -1 };
		private static bool UpdateBatteryLevel(InputDeviceCharacteristics device, out uint index)
		{
			index = GetIndex((uint)device);
			if (index == 0) { return false; }

			if (batteryFrame[index] != Time.frameCount)
			{
				batteryFrame[index] = Time.frameCount;
				return true;
			}
			return false;
		}
		public static float GetBatteryLevel(InputDeviceCharacteristics device)
		{
			if (!UpdateBatteryLevel(device, out uint index)) { return s_BatteryLevels[index]; }
			if (!IsConnected(device)) { return 0; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(device))
					{
						if (m_InputDevices[i].TryGetFeatureValue(CommonUsages.batteryLevel, out s_BatteryLevels[index]))
							return s_BatteryLevels[index];
					}
				}
			}

			return 0;
		}
		public static float GetBatteryLevel(ControlDevice device) { return GetBatteryLevel(device.characteristic()); }

		private static bool m_UserPresence = false;
		private static int userPresenceFrame = -1;
		private static bool UpdateUserPresence()
		{
			if (userPresenceFrame != Time.frameCount)
			{
				userPresenceFrame = Time.frameCount;
				return true;
			}
			return false;
		}
		public static bool IsUserPresence()
		{
			if (!UpdateUserPresence()) { return m_UserPresence; }
			if (!IsConnected(kHMDCharacteristics)) { return false; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					// The device is connected.
					if (m_InputDevices[i].characteristics.HasFlag(kHMDCharacteristics))
					{
						if (m_InputDevices[i].TryGetFeatureValue(CommonUsages.userPresence, out m_UserPresence))
							return m_UserPresence;
					}
				}
			}

			return false;
		}

		private static string[] s_Names = new string[4] { "", "", "", "" };
		private static int[] nameFrame = new int[4] { -1, -1, -1, -1 };
		private static bool UpdateName(InputDeviceCharacteristics device, out uint index)
		{
			index = GetIndex((uint)device);
			if (index == 0) { return false; }

			if (nameFrame[index] != Time.frameCount)
			{
				nameFrame[index] = Time.frameCount;
				return true;
			}
			return false;
		}
		public static bool Name(ControlDevice device, out string name)
		{
			name = "";
			if (!IsConnected(kHMDCharacteristics)) { return false; }
			if (!UpdateName(device.characteristic(), out uint index)) { name = s_Names[index]; return true; }

			if (m_InputDevices != null && m_InputDevices.Count > 0)
			{
				for (int i = 0; i < m_InputDevices.Count; i++)
				{
					if (m_InputDevices[i].characteristics.HasFlag(device.characteristic()))
					{
						s_Names[index] = m_InputDevices[i].name;
						name = s_Names[index];
						return true;
					}
				}
			}

			return false;
		}
	}
}
