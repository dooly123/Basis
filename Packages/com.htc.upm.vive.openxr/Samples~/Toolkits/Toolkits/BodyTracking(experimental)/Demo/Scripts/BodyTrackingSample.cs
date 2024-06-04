// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace VIVE.OpenXR.Toolkits.BodyTracking.Demo
{
	public class BodyTrackingSample : MonoBehaviour
	{
		#region Log
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.Demo.BodyTrackingSample";
		private StringBuilder m_sb = null;
		private StringBuilder sb
		{
			get
			{
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Debug.Log(LOG_TAG + " " + msg); }
		int logFrame = -1;
		bool printIntervalLog = false;
		void ERROR(StringBuilder msg) { Debug.LogError(LOG_TAG + msg); }
		#endregion

		#region Inspector
		public Body inputBody;
		private Body m_InitialBody = null;
		private Vector3 m_InitialScale = Vector3.one;
		#endregion

		private BTDemoHelper.TrackingMode m_TrackingMode = BTDemoHelper.TrackingMode.FullBody;
		public BTDemoHelper.TrackingMode TrackingMode { get { return m_TrackingMode; } }
		public void SetArmMode()
		{
			m_TrackingMode = BTDemoHelper.TrackingMode.Arm;
			sb.Clear().Append("SetArmMode() m_TrackingMode: ").Append(m_TrackingMode.Name()); DEBUG(sb);
		}
		public void SetUpperMode()
		{
			m_TrackingMode = BTDemoHelper.TrackingMode.UpperBody;
			sb.Clear().Append("SetUpperMode() m_TrackingMode: ").Append(m_TrackingMode.Name()); DEBUG(sb);
		}
		public void SetFullMode()
		{
			m_TrackingMode = BTDemoHelper.TrackingMode.FullBody;
			sb.Clear().Append("SetFullMode() m_TrackingMode: ").Append(m_TrackingMode.Name()); DEBUG(sb);
		}
		public void SetUpperBodyAndLegMode()
		{
			m_TrackingMode = BTDemoHelper.TrackingMode.UpperBodyAndLeg;
			sb.Clear().Append("SetUpperBodyAndLegMode() m_TrackingMode: ").Append(m_TrackingMode.Name()); DEBUG(sb);
		}

		private void Awake()
		{
			if (m_InitialBody == null) { m_InitialBody = new Body(); }
			sb.Clear().Append("Awake() Records the initial standard pose and body scale."); DEBUG(sb);
			m_InitialBody.UpdateData(inputBody);
			m_InitialScale = transform.localScale;
		}
		private void Update()
		{
			logFrame++;
			logFrame %= 300;
			printIntervalLog = (logFrame == 0);
		}
		private void OnDisable()
		{
			StopTracking();
		}

		bool updateTrackingData = false;
		public void BeginTracking()
		{
			if (BodyManager.Instance == null) { return; }

			sb.Clear().Append("BeginTracking() ").Append(m_TrackingMode.Name()); DEBUG(sb);
			updateTrackingData = true;
			StartCoroutine(StartBodyTracking());
		}
		public void StopTracking()
		{
			updateTrackingData = false;

			sb.Clear().Append("StopTracking() Recovers the body scale."); DEBUG(sb);
			transform.localScale = m_InitialScale;
		}

		private Vector3 ApplyBodyScale(float scale)
		{
			sb.Clear().Append("ApplyBodyScale() ")
				.Append(" old scale (").Append(transform.localScale.x).Append(", ").Append(transform.localScale.y).Append(", ").Append(transform.localScale.z).Append(")")
				.Append(", apply scale ").Append(scale);
			DEBUG(sb);

			transform.localScale *= scale;

			sb.Clear().Append("ApplyBodyScale() ")
				.Append(" new scale (").Append(transform.localScale.x).Append(", ").Append(transform.localScale.y).Append(", ").Append(transform.localScale.z).Append(")");
			DEBUG(sb);

			return transform.localScale;
		}
		private int m_TrackerId = -1;
		private IEnumerator StartBodyTracking()
		{
			if (BodyManager.Instance == null) { yield return null; }

			yield return new WaitForSeconds(3f);
			sb.Clear(); sb.Append("StartBodyTracking() ").Append(m_TrackingMode.Name()); DEBUG(sb);

			// Sets the standard pose for calibration.
			var result = BodyManager.Instance.SetStandardPose((BodyTrackingMode)m_TrackingMode);
			sb.Clear(); sb.Append("StartBodyTracking() SetStandardPose result: ").Append(result.Name()); DEBUG(sb);
			if (result != BodyTrackingResult.SUCCESS) { yield break; }

			// Creates a body tracker.
			result = BodyManager.Instance.CreateBodyTracking(ref m_TrackerId, (BodyTrackingMode)m_TrackingMode);
			sb.Clear(); sb.Append("StartBodyTracking() CreateBodyTracking(").Append(m_TrackerId).Append(") result: ").Append(result.Name()); DEBUG(sb);
			if (result != BodyTrackingResult.SUCCESS) { yield break; }

			// Retrieves the default rotation spaces.
			result = BodyManager.Instance.GetDefaultRotationSpace(m_TrackerId, out RotateSpace[] rotationSpaces, out UInt32 rotationSpaceCount);
			sb.Clear(); sb.Append("StartBodyTracking() GetBodyRotationSpaces result: ").Append(result.Name()).Append(", jointCount: ").Append(rotationSpaceCount);
			DEBUG(sb);
			if (result != BodyTrackingResult.SUCCESS || rotationSpaceCount == 0) { yield break; }

			for (UInt32 i = 0; i < rotationSpaceCount; i++)
			{
				sb.Clear();
				sb.Append("StartBodyTracking() rotationSpaces[").Append(i).Append("]")
					.Append(", rotation(").Append(rotationSpaces[i].rotation.x)
					.Append(", ").Append(rotationSpaces[i].rotation.y)
					.Append(", ").Append(rotationSpaces[i].rotation.z)
					.Append(", ").Append(rotationSpaces[i].rotation.w).Append(")");
				DEBUG(sb);
			}

			result = BodyManager.Instance.GetBodyTrackingInfo(m_TrackerId, out float avatarHeight, out float avatarScale);
			sb.Clear().Append("StartBodyTracking() GetBodyTrackingInfo result ").Append(result.Name()).Append(", avatarHeight ").Append(avatarHeight).Append(", avatarScale ").Append(avatarScale); DEBUG(sb);
			if (result != BodyTrackingResult.SUCCESS) { yield break; }

			// Due to the pose from GetBodyTrackingPoseOnce is "scaled pose", we need to change the avatar mesh size first.
			// The avatarBody.height is user's height in calibration.
			// The m_InitialBody.height is the height of avatar used in this content.
			// Due to the avatarScale in Body Tracking is always 1, so we calculate the scale with (user height / avatar height).
			float scale = avatarHeight / m_InitialBody.height;
			sb.Clear().Append("StartBodyTracking() Apply avatar scale with ").Append(scale); DEBUG(sb);
			ApplyBodyScale(scale);

			while (updateTrackingData)
			{
				result = BodyManager.Instance.GetBodyTrackingPoseOnce(m_TrackerId, out BodyAvatar avatarBody);
				if (result == BodyTrackingResult.SUCCESS) { UpdateBodyPosesInOrder(avatarBody, rotationSpaces, rotationSpaceCount); }
				yield return new WaitForEndOfFrame();
			}

			result = BodyManager.Instance.DestroyBodyTracking(m_TrackerId);
			sb.Clear(); sb.Append("StartBodyTracking() DestroyBodyTracking(").Append(m_TrackerId).Append(") result: ").Append(result.Name()); DEBUG(sb);
			yield return null;
		}

		/// <summary>
		/// Update the body joints poses according to the avatar joint order.
		/// If your avatar joint order is different, you have to modify this function.
		/// </summary>
		/// <param name="avatarBody">The avatar IK pose from plugin.</param>
		private void UpdateBodyPosesInOrder(BodyAvatar avatarBody, RotateSpace[] rotationSpaces, UInt32 rotationSpaceCount)
		{
			if (inputBody == null || m_InitialBody == null || avatarBody == null || rotationSpaces == null) { return; }
			if (printIntervalLog)
			{
				sb.Clear().Append("UpdateBodyPosesInOrder() new avatar height ").Append(avatarBody.height)
					.Append(", original avatar height ").Append(m_InitialBody.height)
					.Append(", scale: ").Append(avatarBody.scale);
				DEBUG(sb);
			}

			if (inputBody.root != null)
			{
				avatarBody.Update(JointType.HIP, ref inputBody.root);
				UpdateBodyTransform(JointType.HIP, rotationSpaces, rotationSpaceCount, m_InitialBody.HipData.rotation, ref inputBody.root);
			}

			if (inputBody.leftThigh != null)
			{
				inputBody.leftThigh.rotation = avatarBody.leftThigh.rotation;
				UpdateBodyTransform(JointType.LEFTTHIGH, rotationSpaces, rotationSpaceCount, m_InitialBody.LeftThighData.rotation, ref inputBody.leftThigh);
			}
			if (inputBody.leftLeg != null)
			{
				inputBody.leftLeg.rotation = avatarBody.leftLeg.rotation;
				UpdateBodyTransform(JointType.LEFTLEG, rotationSpaces, rotationSpaceCount, m_InitialBody.LeftLegData.rotation, ref inputBody.leftLeg);
			}
			if (inputBody.leftAnkle != null)
			{
				inputBody.leftAnkle.rotation = avatarBody.leftAnkle.rotation;
				UpdateBodyTransform(JointType.LEFTANKLE, rotationSpaces, rotationSpaceCount, m_InitialBody.LeftAnkleData.rotation, ref inputBody.leftAnkle);
			}
			if (inputBody.leftFoot != null)
			{
				inputBody.leftFoot.rotation = avatarBody.leftFoot.rotation;
				UpdateBodyTransform(JointType.LEFTFOOT, rotationSpaces, rotationSpaceCount, m_InitialBody.LeftFootData.rotation, ref inputBody.leftFoot);
			}

			if (inputBody.rightThigh != null)
			{
				inputBody.rightThigh.rotation = avatarBody.rightThigh.rotation;
				UpdateBodyTransform(JointType.RIGHTTHIGH, rotationSpaces, rotationSpaceCount, m_InitialBody.RightThighData.rotation, ref inputBody.rightThigh);
			}
			if (inputBody.rightLeg != null)
			{
				inputBody.rightLeg.rotation = avatarBody.rightLeg.rotation;
				UpdateBodyTransform(JointType.RIGHTLEG, rotationSpaces, rotationSpaceCount, m_InitialBody.RightLegData.rotation, ref inputBody.rightLeg);
			}
			if (inputBody.rightAnkle != null)
			{
				inputBody.rightAnkle.rotation = avatarBody.rightAnkle.rotation;
				UpdateBodyTransform(JointType.RIGHTANKLE, rotationSpaces, rotationSpaceCount, m_InitialBody.RightAnkleData.rotation, ref inputBody.rightAnkle);
			}
			if (inputBody.rightFoot != null)
			{
				inputBody.rightFoot.rotation = avatarBody.rightFoot.rotation;
				UpdateBodyTransform(JointType.RIGHTFOOT, rotationSpaces, rotationSpaceCount, m_InitialBody.RightFootData.rotation, ref inputBody.rightFoot);
			}

			if (inputBody.waist != null)
			{
				inputBody.waist.rotation = avatarBody.waist.rotation;
				UpdateBodyTransform(JointType.WAIST, rotationSpaces, rotationSpaceCount, m_InitialBody.WaistData.rotation, ref inputBody.waist);
			}

			if (inputBody.spineLower != null)
			{
				inputBody.spineLower.rotation = avatarBody.spineLower.rotation;
				UpdateBodyTransform(JointType.SPINELOWER, rotationSpaces, rotationSpaceCount, m_InitialBody.SpineLowerData.rotation, ref inputBody.spineLower);
			}
			if (inputBody.spineMiddle != null)
			{
				inputBody.spineMiddle.rotation = avatarBody.spineMiddle.rotation;
				UpdateBodyTransform(JointType.SPINEMIDDLE, rotationSpaces, rotationSpaceCount, m_InitialBody.SpineMiddleData.rotation, ref inputBody.spineMiddle);
			}
			if (inputBody.spineHigh != null)
			{
				inputBody.spineHigh.rotation = avatarBody.spineHigh.rotation;
				UpdateBodyTransform(JointType.SPINEHIGH, rotationSpaces, rotationSpaceCount, m_InitialBody.SpineHighData.rotation, ref inputBody.spineHigh);
			}

			if (inputBody.chest != null)
			{
				inputBody.chest.rotation = avatarBody.chest.rotation;
				UpdateBodyTransform(JointType.CHEST, rotationSpaces, rotationSpaceCount, m_InitialBody.ChestData.rotation, ref inputBody.chest);
			}
			if (inputBody.neck != null)
			{
				inputBody.neck.rotation = avatarBody.neck.rotation;
				UpdateBodyTransform(JointType.NECK, rotationSpaces, rotationSpaceCount, m_InitialBody.NeckData.rotation, ref inputBody.neck);
			}
			if (inputBody.head != null)
			{
				inputBody.head.rotation = avatarBody.head.rotation;
				UpdateBodyTransform(JointType.HEAD, rotationSpaces, rotationSpaceCount, m_InitialBody.HeadData.rotation, ref inputBody.head);
			}

			if (inputBody.leftClavicle != null)
			{
				inputBody.leftClavicle.rotation = avatarBody.leftClavicle.rotation;
				UpdateBodyTransform(JointType.LEFTCLAVICLE, rotationSpaces, rotationSpaceCount, m_InitialBody.LeftClavicleData.rotation, ref inputBody.leftClavicle);
			}
			if (inputBody.leftScapula != null)
			{
				inputBody.leftScapula.rotation = avatarBody.leftScapula.rotation;
				UpdateBodyTransform(JointType.LEFTSCAPULA, rotationSpaces, rotationSpaceCount, m_InitialBody.LeftScapulaData.rotation, ref inputBody.leftScapula);
			}
			if (inputBody.leftUpperarm != null)
			{
				inputBody.leftUpperarm.rotation = avatarBody.leftUpperarm.rotation;
				UpdateBodyTransform(JointType.LEFTUPPERARM, rotationSpaces, rotationSpaceCount, m_InitialBody.LeftUpperarmData.rotation, ref inputBody.leftUpperarm);
			}
			if (inputBody.leftForearm != null)
			{
				inputBody.leftForearm.rotation = avatarBody.leftForearm.rotation;
				UpdateBodyTransform(JointType.LEFTFOREARM, rotationSpaces, rotationSpaceCount, m_InitialBody.LeftForearmData.rotation, ref inputBody.leftForearm);
			}
			if (inputBody.leftHand != null)
			{
				inputBody.leftHand.rotation = avatarBody.leftHand.rotation;
				UpdateBodyTransform(JointType.LEFTHAND, rotationSpaces, rotationSpaceCount, m_InitialBody.LeftHandData.rotation, ref inputBody.leftHand);
			}

			if (inputBody.rightClavicle != null)
			{
				inputBody.rightClavicle.rotation = avatarBody.rightClavicle.rotation;
				UpdateBodyTransform(JointType.RIGHTCLAVICLE, rotationSpaces, rotationSpaceCount, m_InitialBody.RightClavicleData.rotation, ref inputBody.rightClavicle);
			}
			if (inputBody.rightScapula != null)
			{
				inputBody.rightScapula.rotation = avatarBody.rightScapula.rotation;
				UpdateBodyTransform(JointType.RIGHTSCAPULA, rotationSpaces, rotationSpaceCount, m_InitialBody.RightScapulaData.rotation, ref inputBody.rightScapula);
			}
			if (inputBody.rightUpperarm != null)
			{
				inputBody.rightUpperarm.rotation = avatarBody.rightUpperarm.rotation;
				UpdateBodyTransform(JointType.RIGHTUPPERARM, rotationSpaces, rotationSpaceCount, m_InitialBody.RightUpperarmData.rotation, ref inputBody.rightUpperarm);
			}
			if (inputBody.rightForearm != null)
			{
				inputBody.rightForearm.rotation = avatarBody.rightForearm.rotation;
				UpdateBodyTransform(JointType.RIGHTFOREARM, rotationSpaces, rotationSpaceCount, m_InitialBody.RightForearmData.rotation, ref inputBody.rightForearm);
			}
			if (inputBody.rightHand != null)
			{
				inputBody.rightHand.rotation = avatarBody.rightHand.rotation;
				UpdateBodyTransform(JointType.RIGHTHAND, rotationSpaces, rotationSpaceCount, m_InitialBody.RightHandData.rotation, ref inputBody.rightHand);
			}
		}
		private void UpdateBodyTransform(JointType type, RotateSpace[] rotationSpaces, UInt32 rotationSpaceCount, Quaternion customRot, ref Transform joint)
		{
			if (joint == null) { return; }

			if (printIntervalLog)
			{
				sb.Clear().Append("UpdateJoint() ").Append(type.Name())
					.Append(", rotTPose(").Append(customRot.x).Append(", ").Append(customRot.y).Append(", ").Append(customRot.z).Append(", ").Append(customRot.w).Append(")");
				DEBUG(sb);
				sb.Clear().Append(", rotation before (").Append(joint.rotation.x).Append(", ").Append(joint.rotation.y).Append(", ").Append(joint.rotation.z).Append(", ").Append(joint.rotation.w).Append(")");
				DEBUG(sb);
			}

			// and apply the rotation space to body tracking avatar joint rotation.
			UInt32 index = 0;
			Quaternion diff = Quaternion.identity;
			for (index = 0; index < rotationSpaceCount; index++)
			{
				if (rotationSpaces[index].jointType == type)
				{
					// Calculate the rotation diff from default rotation space to custom avatar standard rotation.
					if (BodyTrackingUtils.GetQuaternionDiff(rotationSpaces[index].rotation, customRot, out diff))
					{
						// Apply the joint rotation with rotation diff.
						joint.rotation *= diff;
					}
					break;
				}
			}

			if (printIntervalLog)
			{
				sb.Clear().Append(", rotation diff (").Append(diff.x).Append(", ").Append(diff.y).Append(", ").Append(diff.z).Append(", ").Append(diff.w).Append(")");
				DEBUG(sb);
				sb.Clear().Append(", rotation after (").Append(joint.rotation.x).Append(", ").Append(joint.rotation.y).Append(", ").Append(joint.rotation.z).Append(", ").Append(joint.rotation.w).Append(")");
				DEBUG(sb);
			}
		}
	}
}
