// Copyright HTC Corporation All Rights Reserved.

//#define TRACKING_LOG

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VIVE.OpenXR.Toolkits.BodyTracking
{
	[Serializable]
	public class Body
	{
		public Transform root; // hip
		private TransformData hipData = TransformData.identity;
		public TransformData HipData { get { return hipData; } private set { hipData = value; } }

		public Transform leftThigh;
		private TransformData leftThighData = TransformData.identity;
		public TransformData LeftThighData { get { return leftThighData; } private set { leftThighData = value; } }
		public Transform leftLeg;
		private TransformData leftLegData = TransformData.identity;
		public TransformData LeftLegData { get { return leftLegData; } private set { leftLegData = value; } }
		public Transform leftAnkle;
		private TransformData leftAnkleData = TransformData.identity;
		public TransformData LeftAnkleData { get { return leftAnkleData; } private set { leftAnkleData = value; } }
		public Transform leftFoot;
		private TransformData leftFootData = TransformData.identity;
		public TransformData LeftFootData { get { return leftFootData; } private set { leftFootData = value; } }

		public Transform rightThigh;
		private TransformData rightThighData = TransformData.identity;
		public TransformData RightThighData { get { return rightThighData; } private set { rightThighData = value; } }
		public Transform rightLeg;
		private TransformData rightLegData = TransformData.identity;
		public TransformData RightLegData { get { return rightLegData; } private set { rightLegData = value; } }
		public Transform rightAnkle;
		private TransformData rightAnkleData = TransformData.identity;
		public TransformData RightAnkleData { get { return rightAnkleData; } private set { rightAnkleData = value; } }
		public Transform rightFoot;
		private TransformData rightFootData = TransformData.identity;
		public TransformData RightFootData { get { return rightFootData; } private set { rightFootData = value; } }

		public Transform waist;
		private TransformData waistData = TransformData.identity;
		public TransformData WaistData { get { return waistData; } private set { waistData = value; } }

		public Transform spineLower;
		private TransformData spineLowerData = TransformData.identity;
		public TransformData SpineLowerData { get { return spineLowerData; } private set { spineLowerData = value; } }
		public Transform spineMiddle;
		private TransformData spineMiddleData = TransformData.identity;
		public TransformData SpineMiddleData { get { return spineMiddleData; } private set { spineMiddleData = value; } }
		public Transform spineHigh;
		private TransformData spineHighData = TransformData.identity;
		public TransformData SpineHighData { get { return spineHighData; } private set { spineHighData = value; } }

		public Transform chest;
		private TransformData chestData = TransformData.identity;
		public TransformData ChestData { get { return chestData; } private set { chestData = value; } }
		public Transform neck;
		private TransformData neckData = TransformData.identity;
		public TransformData NeckData { get { return neckData; } private set { neckData = value; } }
		public Transform head;
		private TransformData headData = TransformData.identity;
		public TransformData HeadData { get { return headData; } private set { headData = value; } }

		public Transform leftClavicle;
		private TransformData leftClavicleData = TransformData.identity;
		public TransformData LeftClavicleData { get { return leftClavicleData; } private set { leftClavicleData = value; } }
		public Transform leftScapula;
		private TransformData leftScapulaData = TransformData.identity;
		public TransformData LeftScapulaData { get { return leftScapulaData; } private set { leftScapulaData = value; } }
		public Transform leftUpperarm;
		private TransformData leftUpperarmData = TransformData.identity;
		public TransformData LeftUpperarmData { get { return leftUpperarmData; } private set { leftUpperarmData = value; } }
		public Transform leftForearm;
		private TransformData leftForearmData = TransformData.identity;
		public TransformData LeftForearmData { get { return leftForearmData; } private set { leftForearmData = value; } }
		public Transform leftHand;
		private TransformData leftHandData = TransformData.identity;
		public TransformData LeftHandData { get { return leftHandData; } private set { leftHandData = value; } }

		public Transform rightClavicle;
		private TransformData rightClavicleData = TransformData.identity;
		public TransformData RightClavicleData { get { return rightClavicleData; } private set { rightClavicleData = value; } }
		public Transform rightScapula;
		private TransformData rightScapulaData = TransformData.identity;
		public TransformData RightScapulaData { get { return rightScapulaData; } private set { rightScapulaData = value; } }
		public Transform rightUpperarm;
		private TransformData rightUpperarmData = TransformData.identity;
		public TransformData RightUpperarmData { get { return rightUpperarmData; } private set { rightUpperarmData = value; } }
		public Transform rightForearm;
		private TransformData rightForearmData = TransformData.identity;
		public TransformData RightForearmData { get { return rightForearmData; } private set { rightForearmData = value; } }
		public Transform rightHand;
		private TransformData rightHandData = TransformData.identity;
		public TransformData RightHandData { get { return rightHandData; } private set { rightHandData = value; } }

		public float height = 0;

		public void UpdateData(Body in_body)
		{
			height = in_body.height;

			hipData.Update(in_body.root);

			leftThighData.Update(in_body.leftThigh);
			leftLegData.Update(in_body.leftLeg);
			leftAnkleData.Update(in_body.leftAnkle);
			leftFootData.Update(in_body.leftFoot);

			rightThighData.Update(in_body.rightThigh);
			rightLegData.Update(in_body.rightLeg);
			rightAnkleData.Update(in_body.rightAnkle);
			rightFootData.Update(in_body.rightFoot);

			waistData.Update(in_body.waist);

			spineLowerData.Update(in_body.spineLower);
			spineMiddleData.Update(in_body.spineMiddle);
			spineHighData.Update(in_body.spineHigh);

			chestData.Update(in_body.chest);
			neckData.Update(in_body.neck);
			headData.Update(in_body.head);

			leftClavicleData.Update(in_body.leftClavicle);
			leftScapulaData.Update(in_body.leftScapula);
			leftUpperarmData.Update(in_body.leftUpperarm);
			leftForearmData.Update(in_body.leftForearm);
			leftHandData.Update(in_body.leftHand);

			rightClavicleData.Update(in_body.rightClavicle);
			rightScapulaData.Update(in_body.rightScapula);
			rightUpperarmData.Update(in_body.rightUpperarm);
			rightForearmData.Update(in_body.rightForearm);
			rightHandData.Update(in_body.rightHand);
		}
		public void UpdateBody(ref Body body)
		{
			body.height = height;

			hipData.Update(ref body.root);

			leftThighData.Update(ref body.leftThigh);
			leftLegData.Update(ref body.leftLeg);
			leftAnkleData.Update(ref body.leftAnkle);
			leftFootData.Update(ref body.leftFoot);

			rightThighData.Update(ref body.rightThigh);
			rightLegData.Update(ref body.rightLeg);
			rightAnkleData.Update(ref body.rightAnkle);
			rightFootData.Update(ref body.rightFoot);

			waistData.Update(ref body.waist);

			spineLowerData.Update(ref body.spineLower);
			spineMiddleData.Update(ref body.spineMiddle);
			spineHighData.Update(ref body.spineHigh);

			chestData.Update(ref body.chest);
			neckData.Update(ref body.neck);
			headData.Update(ref body.head);

			leftClavicleData.Update(ref body.leftClavicle);
			leftScapulaData.Update(ref body.leftScapula);
			leftUpperarmData.Update(ref body.leftUpperarm);
			leftForearmData.Update(ref body.leftForearm);
			leftHandData.Update(ref body.leftHand);

			rightClavicleData.Update(ref body.rightClavicle);
			rightScapulaData.Update(ref body.rightScapula);
			rightUpperarmData.Update(ref body.rightUpperarm);
			rightForearmData.Update(ref body.rightForearm);
			rightHandData.Update(ref body.rightHand);
		}
	}

	[Serializable]
	public class TrackerExtrinsic
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.TrackerExtrinsic";
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

		public ExtrinsicInfo_t hip = ExtrinsicInfo_t.identity; // 0
		public ExtrinsicInfo_t chest = ExtrinsicInfo_t.identity; // 1
		public ExtrinsicInfo_t head = ExtrinsicInfo_t.identity; // 2

		public ExtrinsicInfo_t leftElbow = ExtrinsicInfo_t.identity; // 3
		public ExtrinsicInfo_t leftWrist = ExtrinsicInfo_t.identity; // 4
		public ExtrinsicInfo_t leftHand = ExtrinsicInfo_t.identity; // 5
		public ExtrinsicInfo_t leftHandheld = ExtrinsicInfo_t.identity; // 6

		public ExtrinsicInfo_t rightElbow = ExtrinsicInfo_t.identity; // 7
		public ExtrinsicInfo_t rightWrist = ExtrinsicInfo_t.identity; // 8
		public ExtrinsicInfo_t rightHand = ExtrinsicInfo_t.identity; // 9
		public ExtrinsicInfo_t rightHandheld = ExtrinsicInfo_t.identity; // 10

		public ExtrinsicInfo_t leftKnee = ExtrinsicInfo_t.identity; // 11
		public ExtrinsicInfo_t leftAnkle = ExtrinsicInfo_t.identity; // 12
		public ExtrinsicInfo_t leftFoot = ExtrinsicInfo_t.identity; // 13

		public ExtrinsicInfo_t rightKnee = ExtrinsicInfo_t.identity; // 14
		public ExtrinsicInfo_t rightAnkle = ExtrinsicInfo_t.identity; // 15
		public ExtrinsicInfo_t rightFoot = ExtrinsicInfo_t.identity; // 16

		public void Init()
		{
			hip.Init();
			chest.Init();
			head.Init();

			leftElbow.Init();
			leftWrist.Init();
			leftHand.Init();
			leftHandheld.Init();

			rightElbow.Init();
			rightWrist.Init();
			rightHand.Init();
			rightHandheld.Init();

			leftKnee.Init();
			leftAnkle.Init();
			leftFoot.Init();

			rightKnee.Init();
			rightAnkle.Init();
			rightFoot.Init();
		}
		public void Update(TrackedDeviceRole role, ExtrinsicInfo_t ext)
		{
			if (role == TrackedDeviceRole.ROLE_HIP) { hip.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_CHEST) { chest.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_HEAD) { head.Update(ext); }

			if (role == TrackedDeviceRole.ROLE_LEFTELBOW) { leftElbow.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_LEFTWRIST) { leftWrist.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_LEFTHANDHELD) { leftHandheld.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_LEFTHAND) { leftHand.Update(ext); }

			if (role == TrackedDeviceRole.ROLE_RIGHTELBOW) { rightElbow.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_RIGHTWRIST) { rightWrist.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_RIGHTHANDHELD) { rightHandheld.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_RIGHTHAND) { rightHand.Update(ext); }

			if (role == TrackedDeviceRole.ROLE_LEFTKNEE) { leftKnee.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_LEFTANKLE) { leftAnkle.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_LEFTFOOT) { leftFoot.Update(ext); }

			if (role == TrackedDeviceRole.ROLE_RIGHTKNEE) { rightKnee.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_RIGHTANKLE) { rightAnkle.Update(ext); }
			if (role == TrackedDeviceRole.ROLE_RIGHTFOOT) { rightFoot.Update(ext); }
		}
	}

	[DisallowMultipleComponent]
	public sealed class BodyManager : MonoBehaviour
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.BodyManager";
		private StringBuilder m_sb = null;
		internal StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}

		void DEBUG(StringBuilder sb) { Debug.Log(LOG_TAG + " " + sb); }
		int logFrame = 0;
		bool printIntervalLog = false;
		void ERROR(StringBuilder sb) { Debug.LogError(LOG_TAG + " " + sb); }

		#region Inspector
		[Tooltip("The DevicePose of Tracker 0 in ViveXRTracker.")]
		[SerializeField]
		private InputActionReference m_TrackerPose0 = null;
		public InputActionReference TrackerPose0 => m_TrackerPose0;
		[Tooltip("The DevicePose of Tracker 1 in ViveXRTracker.")]
		[SerializeField]
		private InputActionReference m_TrackerPose1 = null;
		public InputActionReference TrackerPose1 => m_TrackerPose1;
		[Tooltip("The DevicePose of Tracker 2 in ViveXRTracker.")]
		[SerializeField]
		private InputActionReference m_TrackerPose2 = null;
		public InputActionReference TrackerPose2 => m_TrackerPose2;
		[Tooltip("The DevicePose of Tracker 3 in ViveXRTracker.")]
		[SerializeField]
		private InputActionReference m_TrackerPose3 = null;
		public InputActionReference TrackerPose3 => m_TrackerPose3;
		[Tooltip("The DevicePose of Tracker 4 in ViveXRTracker.")]
		[SerializeField]
		private InputActionReference m_TrackerPose4 = null;
		public InputActionReference TrackerPose4 => m_TrackerPose4;
		#endregion

		private static BodyManager m_Instance = null;
		public static BodyManager Instance { get { return m_Instance; } }

		// Role of TrackerPose.
		private int[] s_InputIndex = new int[(int)TrackedDeviceRole.NUMS_OF_ROLE]
		{
			-1,-1,-1,-1,-1,
			-1,-1,-1,-1,-1,
			-1,-1,-1,-1,-1,
			-1,-1
		};
		private InputActionReference[] s_InputTrackedDevice = new InputActionReference[(int)TrackedDeviceRole.NUMS_OF_ROLE]
		{
			null, null, null, null, null,
			null, null, null, null, null,
			null, null, null, null, null,
			null, null
		};
		public void SetTrackerRole(uint index, TrackedDeviceRole role)
		{
			if (index < 0 || index >= 5) { return; }
			if (role <= TrackedDeviceRole.ROLE_UNDEFINED || role >= TrackedDeviceRole.NUMS_OF_ROLE) { return; }
			if (s_InputIndex == null		 || s_InputIndex.Length != (int)TrackedDeviceRole.NUMS_OF_ROLE)
			{
				s_InputIndex = new int[(int)TrackedDeviceRole.NUMS_OF_ROLE]
				{
					-1,-1,-1,-1,-1,
					-1,-1,-1,-1,-1,
					-1,-1,-1,-1,-1,
					-1,-1
				};
			}
			if (s_InputTrackedDevice == null || s_InputTrackedDevice.Length != (int)TrackedDeviceRole.NUMS_OF_ROLE)
			{
				s_InputTrackedDevice = new InputActionReference[(int)TrackedDeviceRole.NUMS_OF_ROLE]
				{
					null, null, null, null, null,
					null, null, null, null, null,
					null, null, null, null, null,
					null, null
				};
			}

			for (int i = 0; i < s_InputIndex.Length; i++)
			{
				if (s_InputIndex[i] == index)
				{
					sb.Clear().Append("SetTrackerRole() Reset s_InputTrackedDevice[").Append(i).Append("] from TrackerPose").Append(s_InputIndex[i]).Append(" to null "); DEBUG(sb);
					s_InputIndex[i] = -1;
					s_InputTrackedDevice[i] = null;
				}
			}

			s_InputIndex[(int)role] = (int)(index & 0x7FFFFFFF);
			if (index == 0) { s_InputTrackedDevice[(int)role] = m_TrackerPose0; }
			if (index == 1) { s_InputTrackedDevice[(int)role] = m_TrackerPose1; }
			if (index == 2) { s_InputTrackedDevice[(int)role] = m_TrackerPose2; }
			if (index == 3) { s_InputTrackedDevice[(int)role] = m_TrackerPose3; }
			if (index == 4) { s_InputTrackedDevice[(int)role] = m_TrackerPose4; }
			sb.Clear().Append("SetTrackerRole() Set s_InputTrackedDevice[").Append((int)role).Append("] ").Append(role.Name()).Append(" to TrackerPose").Append(s_InputIndex[(int)role]); DEBUG(sb);
		}

		private void Awake()
		{
			m_Instance = this;
		}

		private List<int> s_SkeletonIds = new List<int>();
		private void Update()
		{
			logFrame++;
			logFrame %= 300;
			printIntervalLog = (logFrame == 0);

			for (int i = 0; i < s_SkeletonIds.Count; i++)
			{
				int skeletonId = s_SkeletonIds[i];
				if (printIntervalLog)
				{
					sb.Clear().Append("Update() body tracking with id ").Append(skeletonId);
					DEBUG(sb);
				}
				UpdateBodyTrackingOnce(skeletonId);
			}
		}

		#region Standard Pose
		// ------ Calculate Standard Pose ------
		private BodyPose m_StandardPose = null;
		private BodyTrackingResult GetStandardPose(ref BodyPose bodyPose, BodyTrackingMode mode = BodyTrackingMode.FULLBODYIK)
		{
			if (bodyPose == null)
			{
				sb.Clear().Append("GetStandardPose() Invalid BodyPose."); ERROR(sb);
				return BodyTrackingResult.ERROR_INVALID_ARGUMENT;
			}

			BodyTrackingResult result = bodyPose.InitTrackingInfos(mode);
			sb.Clear().Append("GetStandardPose() InitTrackingInfos result: ").Append(result.Name()); DEBUG(sb);

			return result;
		}
		public BodyTrackingResult SetStandardPose(BodyTrackingMode mode = BodyTrackingMode.FULLBODYIK)
		{
			sb.Clear().Append("SetStandardPose() ").Append(mode.Name()); DEBUG(sb);

			if (m_StandardPose == null)
			{
				m_StandardPose = new BodyPose(
					Hip: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_HIP],
					LeftWrist: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTWRIST],
					RightWrist: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTWRIST],

					LeftKnee: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTKNEE],
					LeftAnkle: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTANKLE],
					LeftFoot: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTFOOT],

					RightKnee: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTKNEE],
					RightAnkle: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTANKLE],
					RightFoot: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTFOOT]
				);
			}
			m_StandardPose.Reset(
				Hip: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_HIP],
				LeftWrist: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTWRIST],
				RightWrist: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTWRIST],

				LeftKnee: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTKNEE],
				LeftAnkle: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTANKLE],
				LeftFoot: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTFOOT],

				RightKnee: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTKNEE],
				RightAnkle: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTANKLE],
				RightFoot: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTFOOT]
			);
			BodyTrackingResult result = GetStandardPose(ref m_StandardPose, mode);
			sb.Clear().Append("SetStandardPose() ").Append(mode.Name()).Append(", result: ").Append(result.Name()); DEBUG(sb);

			return result;
		}
		#endregion

		#region Extrinsics
		// ------ Get Standard Tracked Device Extrinsics ------
		private readonly ExtrinsicInfo_t extHead = new ExtrinsicInfo_t(true, new ExtrinsicVector4_t(new Vector3(0, -0.08f, -0.1f), new Vector4(0, 0, 0, 1)));
		private readonly ExtrinsicInfo_t extLeftWrist_SelfTracker = new ExtrinsicInfo_t(true, new ExtrinsicVector4_t(new Vector3(0.0f, -0.035f, 0.043f), new Vector4(0.0f, 0.707f, 0.0f, 0.707f)));
		private readonly ExtrinsicInfo_t extRightWrist_SelfTracker = new ExtrinsicInfo_t(true, new ExtrinsicVector4_t(new Vector3(0.0f, -0.035f, 0.043f), new Vector4(0.0f, -0.707f, 0.0f, 0.707f)));
		private readonly ExtrinsicInfo_t extLeftHandheld = new ExtrinsicInfo_t(true, new ExtrinsicVector4_t(new Vector3(-0.03f, -0.035f, -0.13f), new Vector4(-0.345273f, 0.639022f, 0.462686f, 0.508290f)));
		private readonly ExtrinsicInfo_t extRightHandheld = new ExtrinsicInfo_t(true, new ExtrinsicVector4_t(new Vector3(0.03f, -0.035f, -0.13f), new Vector4(-0.345273f, -0.639022f, -0.462686f, 0.508290f)));
		private readonly ExtrinsicInfo_t extLeftHand = new ExtrinsicInfo_t(true, new ExtrinsicVector4_t(Vector3.zero, new Vector4(0.094802f, 0.641923f, -0.071626f, 0.757508f)));
		private readonly ExtrinsicInfo_t extRightHand = new ExtrinsicInfo_t(true, new ExtrinsicVector4_t(Vector3.zero, new Vector4(0.094802f, -0.641923f, -0.071626f, 0.757508f)));
		private readonly ExtrinsicInfo_t extHips = new ExtrinsicInfo_t(true, ExtrinsicVector4_t.identity);
		private readonly ExtrinsicInfo_t extLeftKnee_SelfTrackerIM = new ExtrinsicInfo_t(true, ExtrinsicVector4_t.identity);
		private readonly ExtrinsicInfo_t extRightKnee_SelfTrackerIM = new ExtrinsicInfo_t(true, ExtrinsicVector4_t.identity);
		private readonly ExtrinsicInfo_t extLeftAnkle_SelfTracker = new ExtrinsicInfo_t(true, new ExtrinsicVector4_t(new Vector3(0.0f, -0.05f, 0.0f), new Vector4(-0.5f, 0.5f, 0.5f, -0.5f)));
		private readonly ExtrinsicInfo_t extRightAnkle_SelfTracker = new ExtrinsicInfo_t(true, new ExtrinsicVector4_t(new Vector3(0.0f, -0.05f, 0.0f), new Vector4(0.5f, 0.5f, 0.5f, 0.5f)));
		private readonly ExtrinsicInfo_t extLeftAnkle_SelfTrackerIM = new ExtrinsicInfo_t(true, ExtrinsicVector4_t.identity);
		private readonly ExtrinsicInfo_t extRightAnkle_SelfTrackerIM = new ExtrinsicInfo_t(true, ExtrinsicVector4_t.identity);
		private BodyTrackingResult UpdateTrackerExtrinsic(ref TrackerExtrinsic trackerExtrinsic)
		{
			if (trackerExtrinsic == null) { return BodyTrackingResult.ERROR_INVALID_ARGUMENT; }

			// Head
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_HEAD.Name())
				.Append(", pos (").Append(extHead.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extHead.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extHead.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extHead.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extHead.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extHead.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extHead.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_HEAD, extHead);
			// Left wrist uses self tracker
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_LEFTWRIST.Name())
				.Append(", pos (").Append(extLeftWrist_SelfTracker.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extLeftWrist_SelfTracker.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extLeftWrist_SelfTracker.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extLeftWrist_SelfTracker.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extLeftWrist_SelfTracker.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extLeftWrist_SelfTracker.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extLeftWrist_SelfTracker.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_LEFTWRIST, extLeftWrist_SelfTracker);
			// Left handheld
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_LEFTHANDHELD.Name())
				.Append(", pos (").Append(extLeftHandheld.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extLeftHandheld.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extLeftHandheld.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extLeftHandheld.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extLeftHandheld.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extLeftHandheld.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extLeftHandheld.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_LEFTHANDHELD, extLeftHandheld);
			// Left hand
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_LEFTHAND.Name())
				.Append(", pos (").Append(extLeftHand.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extLeftHand.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extLeftHand.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extLeftHand.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extLeftHand.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extLeftHand.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extLeftHand.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_LEFTHAND, extLeftHand);
			// Right wrist uses self tracker
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_RIGHTWRIST.Name())
				.Append(", pos (").Append(extRightWrist_SelfTracker.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extRightWrist_SelfTracker.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extRightWrist_SelfTracker.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extRightWrist_SelfTracker.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extRightWrist_SelfTracker.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extRightWrist_SelfTracker.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extRightWrist_SelfTracker.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_RIGHTWRIST, extRightWrist_SelfTracker);
			// Right handheld
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_RIGHTHANDHELD.Name())
				.Append(", pos (").Append(extRightHandheld.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extRightHandheld.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extRightHandheld.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extRightHandheld.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extRightHandheld.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extRightHandheld.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extRightHandheld.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_RIGHTHANDHELD, extRightHandheld);
			// Right hand
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_RIGHTHAND.Name())
				.Append(", pos (").Append(extRightHand.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extRightHand.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extRightHand.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extRightHand.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extRightHand.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extRightHand.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extRightHand.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_RIGHTHAND, extRightHand);
			// Hip
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_HIP.Name())
				.Append(", pos (").Append(extHips.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extHips.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extHips.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extHips.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extHips.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extHips.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extHips.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_HIP, extHips);
			/* Left knee uses self tracker im
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_LEFTKNEE.Name())
				.Append(", pos (").Append(extLeftKnee_SelfTrackerIM.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extLeftKnee_SelfTrackerIM.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extLeftKnee_SelfTrackerIM.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extLeftKnee_SelfTrackerIM.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extLeftKnee_SelfTrackerIM.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extLeftKnee_SelfTrackerIM.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extLeftKnee_SelfTrackerIM.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_LEFTKNEE, extLeftKnee_SelfTrackerIM);*/
			// Left ankle uses self tracker im
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_LEFTANKLE.Name())
				.Append(", pos (").Append(extLeftAnkle_SelfTracker.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extLeftAnkle_SelfTracker.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extLeftAnkle_SelfTracker.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extLeftAnkle_SelfTracker.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extLeftAnkle_SelfTracker.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extLeftAnkle_SelfTracker.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extLeftAnkle_SelfTracker.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_LEFTANKLE, extLeftAnkle_SelfTracker);
			/* Right knee uses self tracker im
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_RIGHTKNEE.Name())
				.Append(", pos (").Append(extRightKnee_SelfTrackerIM.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extRightKnee_SelfTrackerIM.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extRightKnee_SelfTrackerIM.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extRightKnee_SelfTrackerIM.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extRightKnee_SelfTrackerIM.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extRightKnee_SelfTrackerIM.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extRightKnee_SelfTrackerIM.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_RIGHTKNEE, extRightKnee_SelfTrackerIM);*/
			// Right ankle uses self tracker im
			sb.Clear().Append("UpdateTrackerExtrinsic() ").Append(TrackedDeviceRole.ROLE_RIGHTANKLE.Name())
				.Append(", pos (").Append(extRightAnkle_SelfTracker.extrinsic.translation.x.ToString("N2")).Append(", ").Append(extRightAnkle_SelfTracker.extrinsic.translation.y.ToString("N2")).Append(", ").Append(extRightAnkle_SelfTracker.extrinsic.translation.z.ToString("N2")).Append(")")
				.Append(", rot (").Append(extRightAnkle_SelfTracker.extrinsic.rotation.x.ToString("N2")).Append(", ").Append(extRightAnkle_SelfTracker.extrinsic.rotation.y.ToString("N2")).Append(", ").Append(extRightAnkle_SelfTracker.extrinsic.rotation.z.ToString("N2")).Append(", ").Append(extRightAnkle_SelfTracker.extrinsic.rotation.w.ToString("N2")).Append(")");
			DEBUG(sb);
			trackerExtrinsic.Update(TrackedDeviceRole.ROLE_RIGHTANKLE, extRightAnkle_SelfTracker);

			return BodyTrackingResult.SUCCESS;
		}
		#endregion

		#region Init IK
		// ------ Init IK: Avatar IK or Default Body Tracking.
		private Dictionary<int, BodyIKInfo> s_BodyIKInfos = new Dictionary<int, BodyIKInfo>();

		// ------ Init Avatar IK: Init BodyIKInfo (from Body & TrackerExtrinsic) ------
		/// <summary>
		/// Creates Body Tracking IK with the custom avatar (<see cref="Body">Body</see>) and tracker extrinsics (<see cref="TrackerExtrinsic">TrackerExtrinsic</see>).
		/// <br></br>
		/// Before initializing calibration:
		/// <br></br>
		/// 1. Checks if specified <see cref="BodyTrackingMode">BodyTrackingMode</see> is supported by the calibration pose.
		/// <br></br>
		/// 2. Checks if tracked device extrinsics support the <see cref="BodyTrackingMode">BodyTrackingMode</see> and calibration pose.
		/// </summary>
		/// <param name="skeletonId">Output ID for the IK.</param>
		/// <param name="avatarBody">Custom avatar.</param>
		/// <param name="trackerExtrinsic">Custom tracked device extrinsics.</param>
		/// <param name="mode">Body tracking mode which is full body tracking by default.</param>
		/// <returns>SUCCESS for creating custom body tracking IK successfully.</returns>
		public BodyTrackingResult CreateBodyTracking(ref int skeletonId, Body avatarBody, TrackerExtrinsic trackerExtrinsic, BodyTrackingMode mode = BodyTrackingMode.FULLBODYIK)
		{
			sb.Clear().Append("CreateBodyTracking() with Avatar and Tracker Extrinsics, mode: ").Append(mode.Name()); DEBUG(sb);

			if (avatarBody == null)
			{
				sb.Clear().Append("CreateBodyTracking() Invalid Avatar Body.");
				ERROR(sb);
				return BodyTrackingResult.ERROR_INVALID_ARGUMENT;
			}
			if (trackerExtrinsic == null)
			{
				sb.Clear().Append("CreateBodyTracking() Invalid TrackerExtrinsic.");
				ERROR(sb);
				return BodyTrackingResult.ERROR_INVALID_ARGUMENT;
			}

			// 1-1. Retrieves the calibration pose.
			BodyPose calibPose = new BodyPose(
				Hip: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_HIP],
				LeftWrist: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTWRIST],
				RightWrist: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTWRIST],

				LeftKnee: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTKNEE],
				LeftAnkle: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTANKLE],
				LeftFoot: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTFOOT],

				RightKnee: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTKNEE],
				RightAnkle: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTANKLE],
				RightFoot: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTFOOT]
			);
			if (m_StandardPose != null)
			{
				// The user calibrates standard pose in runtime.
				sb.Clear().Append("CreateBodyTracking() updates standard pose in content."); DEBUG(sb);
				calibPose.Update(m_StandardPose);
			}
			else
			{
				sb.Clear().Append("CreateBodyTracking() updates standard pose from runtime."); DEBUG(sb);
				if (GetStandardPose(ref calibPose, mode) != BodyTrackingResult.SUCCESS)
				{
					sb.Clear().Append("CreateBodyTracking() failed to retrieve calibration pose from runtime."); ERROR(sb);
					return BodyTrackingResult.ERROR_NOT_CALIBRATED;
				}
			}

			BodyPoseRole calibRole = calibPose.GetIKRoles(mode);
			sb.Clear().Append("CreateBodyTracking() retrieves calibration pose successfully, mode: ").Append(mode.Name()).Append(", calibration role: ").Append(calibRole.Name()); DEBUG(sb);

			// 1-2. Checks if the body tracking mode is available.
			if (!BodyTrackingUtils.MatchBodyTrackingMode(mode, calibRole))
			{
				sb.Clear().Append("CreateBodyTracking() The body tracking mode ").Append(mode.Name()).Append(" and calibration role ").Append(calibRole.Name()).Append(" are NOT matched."); ERROR(sb);
				return BodyTrackingResult.ERROR_BODYTRACKINGMODE_NOT_ALIGNED;
			}

			// No need to initialize the Tracked Device Extrinsic.


			// Initializes BodyIKInfo.
			BodyIKInfo ikInfo = new BodyIKInfo(mode);
			ikInfo.avatar.Update(avatarBody);
			ikInfo.trackedDevice.Update(trackerExtrinsic);

			// 2-1. Retrieves the tracked device extrinsics according to the calibration role.
			if (!ikInfo.trackedDevice.GetDevicesExtrinsics(calibRole, out TrackedDeviceExtrinsic[] bodyTrackedDevices, out UInt32 bodyTrackedDeviceCount))
			{
				sb.Clear().Append("CreateBodyTracking() Cannot get tracked device extrinsics."); ERROR(sb);
				return BodyTrackingResult.ERROR_AVATAR_INIT_FAILED;
			}
			sb.Clear().Append("CreateBodyTracking() Init IK uses ").Append(bodyTrackedDeviceCount).Append(" tracked devices: \n");
			for (UInt32 u = 0; u < bodyTrackedDeviceCount; u++)
			{
				sb.Append(u).Append(": ").Append(bodyTrackedDevices[u].trackedDeviceRole.Name()).Append("\n");
			}
			DEBUG(sb);

			DeviceExtRole extRole = BodyTrackingUtils.GetDeviceExtRole(calibRole, bodyTrackedDevices, bodyTrackedDeviceCount);
			sb.Clear().Append("CreateBodyTracking() retrieves tracked device extrinsics successfully, mode: ").Append(mode.Name()).Append(", extrinsic role: ").Append(extRole.Name()); DEBUG(sb);

			// 2-2. Checks if the body tracking mode is available.
			if (!BodyTrackingUtils.MatchBodyTrackingMode(mode, extRole))
			{
				sb.Clear().Append("CreateBodyTracking() The body tracking mode ").Append(mode.Name()).Append(" and device extrinsic role ").Append(extRole.Name()).Append(" are NOT matched."); ERROR(sb);
				return BodyTrackingResult.ERROR_INPUTPOSE_NOT_VALID;
			}

			// 3. Retrieves the Avatar T-Pose joints.
			if (!ikInfo.avatar.GetJoints(out Joint[] avatarJoints, out UInt32 avatarJointCount, true))
			{
				sb.Clear().Append("CreateBodyTracking() Cannot get 6DoF joints.");
				ERROR(sb);
				return BodyTrackingResult.ERROR_AVATAR_INIT_FAILED;
			}
			sb.Clear().Append("CreateBodyTracking() Init IK uses ").Append(avatarJointCount).Append(" avatar joints: \n");
			for (UInt32 u = 0; u < avatarJointCount; u++)
			{
				sb.Append(u).Append(": ").Append(avatarJoints[u].jointType.Name()).Append("\n");
			}
			DEBUG(sb);

			// Initializes IK
			UInt64 ts = BodyTrackingUtils.GetTimeStamp();
#if TRACKING_LOG
			Result result = fbt.InitBodyTrackingLog(
				ts: ts,
				bodyTrackingMode: mode,
				trackedDeviceExt: bodyTrackedDevices,
				deviceCount: bodyTrackedDeviceCount,
				avatarJoints: avatarJoints,
				avatarJointCount: avatarJointCount,
				avatarHeight: ikInfo.avatar.height, ref skeletonId);
#else
			Result result = fbt.InitBodyTracking(
				ts: ts,
				bodyTrackingMode: mode,
				trackedDeviceExt: bodyTrackedDevices,
				deviceCount: bodyTrackedDeviceCount,
				avatarJoints: avatarJoints,
				avatarJointCount: avatarJointCount,
				avatarHeight: ikInfo.avatar.height, ref skeletonId);
#endif
			if (result != Result.SUCCESS)
			{
				sb.Clear().Append("CreateBodyTracking() InitBodyTracking failed, result: ").Append(result.Type().Name());
				ERROR(sb);
				return result.Type();
			}

			sb.Clear().Append("CreateBodyTracking() Initial IK successfully, mode: ").Append(mode.Name()); DEBUG(sb);
			s_BodyIKInfos.Add(skeletonId, ikInfo);

			// Calibrates IK.
			if (CalibrateBodyTracking(skeletonId, ts, mode, calibPose, false) != BodyTrackingResult.SUCCESS)
			{
				sb.Clear().Append("CreateBodyTracking() CalibrateBodyTracking failed."); ERROR(sb);
				return BodyTrackingResult.ERROR_CALIBRATE_FAILED;
			}
			sb.Clear().Append("CreateBodyTracking() Calibrate IK successfully, mode: ").Append(mode.Name()); DEBUG(sb);

			return BodyTrackingResult.SUCCESS;
		}

		// ------ Init Avatar IK: Init BodyIKInfo (from Body & default TrackerExtrinsic) ------
		/// <summary>
		/// Creates Body Tracking IK with the custom avatar (<see cref="Body">Body</see>).
		/// <br></br>
		/// Before initializing calibration:
		/// <br></br>
		/// 1. Checks if specified <see cref="BodyTrackingMode">BodyTrackingMode</see> is supported by the calibration pose.
		/// <br></br>
		/// 2. Checks if default tracked device extrinsics support the <see cref="BodyTrackingMode">BodyTrackingMode</see> and calibration pose.
		/// </summary>
		/// <param name="skeletonId">Output ID for the IK.</param>
		/// <param name="avatarBody">Custom avatar.</param>
		/// <param name="mode">Body tracking mode which is full body tracking by default.</param>
		/// <returns>SUCCESS for creating custom body tracking IK successfully.</returns>
		public BodyTrackingResult CreateBodyTracking(ref int skeletonId, Body avatarBody, BodyTrackingMode mode = BodyTrackingMode.FULLBODYIK)
		{
			sb.Clear().Append("CreateBodyTracking() with Avatar, mode: ").Append(mode.Name()); DEBUG(sb);

			// 1-1. Retrieves the calibration pose.
			BodyPose calibPose = new BodyPose(
				Hip: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_HIP],
				LeftWrist: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTWRIST],
				RightWrist: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTWRIST],

				LeftKnee: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTKNEE],
				LeftAnkle: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTANKLE],
				LeftFoot: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTFOOT],

				RightKnee: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTKNEE],
				RightAnkle: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTANKLE],
				RightFoot: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTFOOT]
			);
			if (m_StandardPose != null)
			{
				// The user calibrates standard pose in runtime.
				sb.Clear().Append("CreateBodyTracking() updates standard pose in content."); DEBUG(sb);
				calibPose.Update(m_StandardPose);
			}
			else
			{
				sb.Clear().Append("CreateBodyTracking() updates standard pose from runtime."); DEBUG(sb);
				if (GetStandardPose(ref calibPose, mode) != BodyTrackingResult.SUCCESS)
				{
					sb.Clear().Append("CreateBodyTracking() failed to retrieve calibration pose from runtime."); ERROR(sb);
					return BodyTrackingResult.ERROR_NOT_CALIBRATED;
				}
			}

			BodyPoseRole calibRole = calibPose.GetIKRoles(mode);
			sb.Clear().Append("CreateBodyTracking() retrieves calibration pose successfully, mode: ").Append(mode.Name()).Append(", calibration role: ").Append(calibRole.Name()); DEBUG(sb);

			// 1-2. Checks if the body tracking mode is available.
			if (!BodyTrackingUtils.MatchBodyTrackingMode(mode, calibRole))
			{
				sb.Clear().Append("CreateBodyTracking() The body tracking mode ").Append(mode.Name()).Append(" and calibration role ").Append(calibRole.Name()).Append(" are NOT matched."); ERROR(sb);
				return BodyTrackingResult.ERROR_BODYTRACKINGMODE_NOT_ALIGNED;
			}

			// Initializes the Tracked Device Extrinsic.
			TrackerExtrinsic trackerExtrinsic = new TrackerExtrinsic();
			if (UpdateTrackerExtrinsic(ref trackerExtrinsic) != BodyTrackingResult.SUCCESS)
			{
				sb.Clear().Append("CreateBodyTracking() mode: ").Append(mode.Name()).Append(", can't retrieve correct tracker extrinsics."); ERROR(sb);
				return BodyTrackingResult.ERROR_INVALID_ARGUMENT;
			}

			return CreateBodyTracking(ref skeletonId, avatarBody, trackerExtrinsic, mode);
		}

		// ------ Init Default Body Tracking: Init BodyIKInfo and retrieve the Default Rotation Spaces ------
		private Dictionary<int, BodyRotationSpace_t> s_BodyRotationSpaces = new Dictionary<int, BodyRotationSpace_t>();
		/// <summary>
		/// Creates Body Tracking IK with all default configurations.
		/// <br></br>
		/// Before initializing calibration:
		/// <br></br>
		/// 1. Checks if specified <see cref="BodyTrackingMode">BodyTrackingMode</see> is supported by the calibration pose.
		/// <br></br>
		/// 2. Checks if default tracked device extrinsics support the <see cref="BodyTrackingMode">BodyTrackingMode</see> and calibration pose.
		/// </summary>
		/// <param name="skeletonId">Output ID for the IK.</param>
		/// <param name="mode">Body tracking mode which is full body tracking by default.</param>
		/// <returns>SUCCESS for creating default body tracking IK successfully.</returns>
		public BodyTrackingResult CreateBodyTracking(ref int skeletonId, BodyTrackingMode mode = BodyTrackingMode.FULLBODYIK)
		{
			sb.Clear().Append("CreateBodyTracking() with body tracking mode: ").Append(mode.Name()); DEBUG(sb);

			// 1-1. Retrieves the calibration pose.
			BodyPose calibPose = new BodyPose(
				Hip: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_HIP],
				LeftWrist: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTWRIST],
				RightWrist: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTWRIST],

				LeftKnee: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTKNEE],
				LeftAnkle: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTANKLE],
				LeftFoot: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_LEFTFOOT],

				RightKnee: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTKNEE],
				RightAnkle: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTANKLE],
				RightFoot: s_InputTrackedDevice[(int)TrackedDeviceRole.ROLE_RIGHTFOOT]
			);
			if (m_StandardPose != null)
			{
				// The user calibrates standard pose in runtime.
				sb.Clear().Append("CreateBodyTracking() updates standard pose in content."); DEBUG(sb);
				calibPose.Update(m_StandardPose);
			}
			else
			{
				sb.Clear().Append("CreateBodyTracking() updates standard pose from runtime."); DEBUG(sb);
				if (GetStandardPose(ref calibPose, mode) != BodyTrackingResult.SUCCESS)
				{
					sb.Clear().Append("CreateBodyTracking() failed to retrieve calibration pose from runtime."); ERROR(sb);
					return BodyTrackingResult.ERROR_NOT_CALIBRATED;
				}
			}

			BodyPoseRole calibRole = calibPose.GetIKRoles(mode);
			sb.Clear().Append("CreateBodyTracking() retrieves calibration pose successfully, mode: ").Append(mode.Name()).Append(", calibration role: ").Append(calibRole.Name()); DEBUG(sb);

			// 1-2. Checks if the body tracking mode is available.
			if (!BodyTrackingUtils.MatchBodyTrackingMode(mode, calibRole))
			{
				sb.Clear().Append("CreateBodyTracking() The body tracking mode ").Append(mode.Name()).Append(" and calibration role ").Append(calibRole.Name()).Append(" are NOT matched."); ERROR(sb);
				return BodyTrackingResult.ERROR_BODYTRACKINGMODE_NOT_ALIGNED;
			}

			// Initializes the Tracked Device Extrinsic.
			TrackerExtrinsic trackerExtrinsic = new TrackerExtrinsic();
			if (UpdateTrackerExtrinsic(ref trackerExtrinsic) != BodyTrackingResult.SUCCESS)
			{
				sb.Clear().Append("CreateBodyTracking() mode: ").Append(mode.Name()).Append(", can't retrieve the tracker extrinsics."); ERROR(sb);
				return BodyTrackingResult.ERROR_INVALID_ARGUMENT;
			}

			// Initializes BodyIKInfo.
			BodyIKInfo ikInfo = new BodyIKInfo(mode);
			ikInfo.trackedDevice.Update(trackerExtrinsic);

			// 2-1. Retrieves the tracked device extrinsics according to the calibration role.
			if (!ikInfo.trackedDevice.GetDevicesExtrinsics(calibRole, out TrackedDeviceExtrinsic[] bodyTrackedDevices, out UInt32 bodyTrackedDeviceCount))
			{
				sb.Clear().Append("CreateBodyTracking() Cannot get tracked device extrinsics."); ERROR(sb);
				return BodyTrackingResult.ERROR_AVATAR_INIT_FAILED;
			}
			sb.Clear().Append("CreateBodyTracking() Init IK uses ").Append(bodyTrackedDeviceCount).Append(" tracked devices: \n");
			for (UInt32 u = 0; u < bodyTrackedDeviceCount; u++)
			{
				sb.Append(u).Append(": ").Append(bodyTrackedDevices[u].trackedDeviceRole.Name()).Append("\n");
			}
			DEBUG(sb);

			DeviceExtRole extRole = BodyTrackingUtils.GetDeviceExtRole(calibRole, bodyTrackedDevices, bodyTrackedDeviceCount);
			sb.Clear().Append("CreateBodyTracking retrieves tracked device extrinsics successfully, mode: ").Append(mode.Name()).Append(", extrinsic role: ").Append(extRole.Name()); DEBUG(sb);

			// 2-2. Checks if the body tracking mode is available.
			if (!BodyTrackingUtils.MatchBodyTrackingMode(mode, extRole))
			{
				sb.Clear().Append("CreateBodyTracking() The body tracking mode ").Append(mode.Name()).Append(" and device extrinsic role ").Append(extRole.Name()).Append(" are NOT matched."); ERROR(sb);
				return BodyTrackingResult.ERROR_INPUTPOSE_NOT_VALID;
			}

			// Initializes IK
			UInt64 ts = BodyTrackingUtils.GetTimeStamp();
			Result result = fbt.InitDefaultBodyTracking(
				ts: ts,
				bodyTrackingMode: mode,
				trackedDeviceExt: bodyTrackedDevices,
				deviceCount: bodyTrackedDeviceCount,
				ref skeletonId);
			if (result != Result.SUCCESS)
			{
				sb.Clear().Append("CreateBodyTracking() InitDefaultBodyTracking failed, result: ").Append(result.Type().Name());
				ERROR(sb);
				return result.Type();
			}

			sb.Clear().Append("CreateBodyTracking() Initial IK successfully, tracking mode: ").Append(mode.Name()); DEBUG(sb);
			s_BodyIKInfos.Add(skeletonId, ikInfo);

			// Retrieves Default Rotation Spaces
			UInt32 jointCount = 0;
			result = fbt.GetDefaultSkeletonJointCount(ref jointCount);
			if (result != Result.SUCCESS || jointCount == 0 || jointCount > (UInt32)JointType.MAX_ENUM)
			{
				sb.Clear().Append("CreateBodyTracking() GetDefaultSkeletonJointCount failed, count: ").Append(jointCount).Append(", result: ").Append(result.Type().Name()); ERROR(sb);
				return BodyTrackingResult.ERROR_AVATAR_INIT_FAILED;
			}

			RotateSpace[] rotateSpaces = new RotateSpace[jointCount];
			result = fbt.GetDefaultSkeletonRotateSpace(rotateSpaces, jointCount);
			if (result != Result.SUCCESS || rotateSpaces.Length <= 0)
			{
				sb.Clear().Append("CreateBodyTracking() GetDefaultSkeletonRotateSpace failed, count: ").Append(jointCount).Append(", result: ").Append(result.Type().Name());
				ERROR(sb);
				return BodyTrackingResult.ERROR_AVATAR_INIT_FAILED;
			}

			s_BodyRotationSpaces.Add(skeletonId, new BodyRotationSpace_t(rotateSpaces, jointCount));

			// Calibrates IK.
			if (CalibrateBodyTracking(skeletonId, ts, mode, calibPose, true) != BodyTrackingResult.SUCCESS)
			{
				sb.Clear().Append("CreateBodyTracking() CalibrateBodyTracking failed."); ERROR(sb);
				return BodyTrackingResult.ERROR_CALIBRATE_FAILED;
			}
			sb.Clear().Append("CreateBodyTracking() Calibrate IK successfully, tracking mode: ").Append(mode.Name()); DEBUG(sb);

			return BodyTrackingResult.SUCCESS;
		}
		public BodyTrackingResult GetDefaultRotationSpace(int skeletonId, out RotateSpace[] spaces, out UInt32 count)
		{
			if (s_BodyRotationSpaces != null && s_BodyRotationSpaces.ContainsKey(skeletonId))
			{
				spaces = s_BodyRotationSpaces[skeletonId].spaces;
				count = s_BodyRotationSpaces[skeletonId].count;
				return BodyTrackingResult.SUCCESS;
			}
			spaces = null;
			count = 0;
			return BodyTrackingResult.ERROR_AVATAR_INIT_FAILED;
		}
		#endregion

		// ------ Calibrate IK: Calculate BodyPose (from standard pose) ------
		private Dictionary<int, BodyPose> s_BodyPoses = new Dictionary<int, BodyPose>();
		private Dictionary<int, BodyAvatar> s_BodyAvatars = new Dictionary<int, BodyAvatar>();
		private const float kHeightMin = 0.5f;
		/// <summary>
		/// Calibrate Default/Custom Body Tracking IK.
		/// </summary>
		/// <param name="id">Skeleton ID from Init IK.</param>
		/// <param name="ts">Timestamp in UInt64.</param>
		/// <param name="mode">Tracking mode from Init IK.</param>
		/// <param name="isDefault">True for default Body Tracking IK.</param>
		/// <returns>SUCCESS for successful calibration.</returns>
		private BodyTrackingResult CalibrateBodyTracking(int id, UInt64 ts, BodyTrackingMode mode, BodyPose calibPose, bool isDefault)
		{
			sb.Clear().Append("CalibrateBodyTracking() ").Append(id)
				.Append(", timestamp: ").Append(ts)
				.Append(", mode: ").Append(mode.Name())
				.Append(", isDefault: ").Append(isDefault);
			DEBUG(sb);

			if (!calibPose.GetTrackedDevicePoses(mode, out TrackedDevicePose[] trackedDevicePoses, out UInt32 trackedDevicePoseCount))
			{
				sb.Clear().Append("UpdateBodyTrackingOnce() Cannot tracked device poses."); ERROR(sb);
				return BodyTrackingResult.ERROR_CALIBRATE_FAILED;
			}
			BodyPoseRole calibRole = BodyTrackingUtils.GetBodyPoseRole(trackedDevicePoses, trackedDevicePoseCount);
			sb.Clear().Append("CalibrateBodyTracking() ").Append(id).Append(", calibRole: ").Append(calibRole.Name()); DEBUG(sb);

			// Checks if the body tracking mode is available.
			if (!BodyTrackingUtils.MatchBodyTrackingMode(mode, calibRole))
			{
				sb.Clear().Append("CalibrateBodyTracking() Can not figure out the body tracking initial mode ").Append(mode.Name()); ERROR(sb);
				return BodyTrackingResult.ERROR_CALIBRATE_FAILED;
			}

			// Calibrates IK with the L-Pose.
			if (calibPose.head.translation.y <= kHeightMin)
			{
				sb.Clear().Append("CalibrateBodyTracking() Invalid HMD height ").Append(calibPose.head.translation.y).Append(", make sure your camera in Tracking Origin - Floor mode.");
				ERROR(sb);
				return BodyTrackingResult.ERROR_INVALID_ARGUMENT;
			}

			// scale = user_height / avatar_height
			float scale = isDefault ? 1 : (calibPose.head.translation.y / s_BodyIKInfos[id].avatar.height);
			sb.Clear().Append("CalibrateBodyTracking() Head height: ").Append(calibPose.head.translation.y).Append(", avatar height: ").Append(s_BodyIKInfos[id].avatar.height).Append(", scale: ").Append(scale);
			DEBUG(sb);

#if TRACKING_LOG
			Result result = fbt.CalibrateBodyTrackingLog(
				ts: ts,
				skeletonId: id,
				userHeight: calibPose.head.translation.y,
				bodyTrackingMode: mode,
				trackedDevicePose: trackedDevicePoses,
				deviceCount: trackedDevicePoseCount, ref scale);
#else
			Result result = fbt.CalibrateBodyTracking(
				ts: ts,
				skeletonId: id,
				userHeight: calibPose.head.translation.y,
				bodyTrackingMode: mode,
				trackedDevicePose: trackedDevicePoses,
				deviceCount: trackedDevicePoseCount, ref scale);
#endif
			if (result != Result.SUCCESS)
			{
				sb.Clear().Append("CalibrateBodyTracking() CalibrateBodyTracking failed, result: ").Append(result.Type().Name());
				ERROR(sb);
				return result.Type();
			}

			// Save the calibration pose as cached device pose and will be updated at UpdateBodyTrackingOnce().
			s_BodyPoses.Add(id, calibPose);

			s_BodyAvatars.Add(id, new BodyAvatar());
			s_BodyAvatars[id].Update(s_BodyIKInfos[id].avatar);
			s_BodyAvatars[id].height = (calibPose.head.translation.y / scale);
			s_BodyAvatars[id].scale = scale;

			sb.Clear().Append("CalibrateBodyTracking() Calibrate ").Append(isDefault ? "Default" : "Custom").Append(" IK successfully")
				.Append(", avatar height: ").Append(s_BodyAvatars[id].height)
				.Append(", scale: ").Append(s_BodyAvatars[id].scale);
			DEBUG(sb);

			return BodyTrackingResult.SUCCESS;
		}

		/// <summary>
		/// After <see cref="CreateBodyTracking">CreateBodyTracking</see> you can retrieve the calibrated avatar height and scale.
		/// </summary>
		/// <param name="skeletonId"> The skeleton ID from <see cref="CreateBodyTracking">CreateBodyTracking</see>.</param>
		/// <param name="avatarHeight">The calibrated avatar height which is usually the HMD's y-axis of position.</param>
		/// <param name="avatarScale">The calibrated avatar scale which is 1 when using default Body Tracking. Otherwise scale = {HMD y-axis} / your avatar's height.</param>
		/// <returns>True if the calibration completes successfully.</returns>
		public BodyTrackingResult GetBodyTrackingInfo(int skeletonId, out float avatarHeight, out float avatarScale)
		{
			avatarHeight = 1.5f;
			avatarScale = 1;

			if (s_BodyAvatars.ContainsKey(skeletonId))
			{
				avatarHeight = s_BodyAvatars[skeletonId].height;
				avatarScale = s_BodyAvatars[skeletonId].scale;
				return BodyTrackingResult.SUCCESS;
			}

			return BodyTrackingResult.ERROR_NOT_CALIBRATED;
		}

		// ------ Update IK: Calculate BodyAvatar according to BodyIKInfo & BodyPose ------
		private BodyTrackingResult IsValidSkeletonId(int id)
		{
			if (!s_BodyIKInfos.ContainsKey(id)) { return BodyTrackingResult.ERROR_AVATAR_INIT_FAILED; }
			if (!s_BodyPoses.ContainsKey(id)) { return BodyTrackingResult.ERROR_NOT_CALIBRATED; }
			return BodyTrackingResult.SUCCESS;
		}
		private UInt32 m_OutputJointCount = 0;
		private Joint[] s_OutputJoints = null;
		private BodyTrackingResult UpdateBodyTrackingOnce(int id)
		{
			// Checks if the initialization succeeds.
			var btResult = IsValidSkeletonId(id);
			if (btResult != BodyTrackingResult.SUCCESS)
			{
				if (printIntervalLog)
				{
					sb.Clear().Append("UpdateBodyTrackingOnce() invalid id ").Append(id).Append(" btResult ").Append(btResult.Name());
					ERROR(sb);
				}
				return btResult;
			}

			// Updates the device pose.
			if (s_BodyPoses[id].UpdatePose() != BodyTrackingResult.SUCCESS)
			{
				sb.Clear().Append("UpdateBodyTrackingOnce() UpdateBodyTrackingOnce poses failed for ID ").Append(id);
				ERROR(sb);
				return BodyTrackingResult.ERROR_IK_NOT_UPDATED;
			}

			// Updates IK with the calibrate pose.
			UInt64 ts = BodyTrackingUtils.GetTimeStamp();

			UInt32 jointCount = 0;
			Result result = fbt.GetOutputJointCount(ts, id, ref jointCount);
			if (result != Result.SUCCESS || jointCount == 0)
			{
				sb.Clear().Append("UpdateBodyTrackingOnce() GetOutputJointCount failed, jointCount: ").Append(jointCount).Append(", result: ").Append(result.Type().Name());
				ERROR(sb);
				return BodyTrackingResult.ERROR_IK_NOT_UPDATED;
			}
			if (m_OutputJointCount != jointCount)
			{
				m_OutputJointCount = jointCount;
				s_OutputJoints = new Joint[m_OutputJointCount];
			}
			if (!s_BodyPoses[id].GetTrackedDevicePoses(s_BodyIKInfos[id].mode, out TrackedDevicePose[] trackedDevicePoses, out UInt32 trackedDevicePoseCount))
			{
				sb.Clear().Append("UpdateBodyTrackingOnce() Cannot tracked device poses."); ERROR(sb);
				return BodyTrackingResult.ERROR_IK_NOT_UPDATED;
			}

#if TRACKING_LOG
			result = fbt.UpdateBodyTrackingLog(ts, id, trackedDevicePoses, trackedDevicePoseCount, s_OutputJoints, m_OutputJointCount);
#else
			result = fbt.UpdateBodyTracking(ts, id, trackedDevicePoses, trackedDevicePoseCount, s_OutputJoints, m_OutputJointCount);
#endif
			if (result != Result.SUCCESS)
			{
				sb.Clear().Append("UpdateBodyTrackingOnce() UpdateBodyTracking failed, result: ").Append(result.Type().Name());
				ERROR(sb);
				return result.Type();
			}

			// Comes out the bodyAvatar.
			if (s_BodyAvatars.ContainsKey(id))
			{
				s_BodyAvatars[id].Set6DoFJoints(s_OutputJoints, m_OutputJointCount);
			}

			return BodyTrackingResult.SUCCESS;
		}
		public BodyTrackingResult GetBodyTrackingPoses(int skeletonId, out BodyAvatar avatarBody)
		{
			if (s_BodyAvatars.ContainsKey(skeletonId))
			{
				avatarBody = s_BodyAvatars[skeletonId];
				return BodyTrackingResult.SUCCESS;
			}

			avatarBody = null;
			return BodyTrackingResult.ERROR_IK_NOT_UPDATED;
		}
		public BodyTrackingResult GetBodyTrackingPoseOnce(int skeletonId, out BodyAvatar avatarBody)
		{
			var result = UpdateBodyTrackingOnce(skeletonId);
			if (result == BodyTrackingResult.SUCCESS)
			{
				return GetBodyTrackingPoses(skeletonId, out avatarBody);
			}

			avatarBody = null;
			return result;
		}

		// ------ Destroy IK: Destroy IK according to BodyIKInfo ------
		public BodyTrackingResult DestroyBodyTracking(int skeletonId)
		{
			BodyTrackingResult btResult = BodyTrackingResult.ERROR_IK_NOT_DESTROYED;

			UInt64 ts = BodyTrackingUtils.GetTimeStamp();
			// In CreateBodyTracking, we collected the Avatar Joints and Extrinsics to s_BodyIKInfos.
			if (s_BodyIKInfos.ContainsKey(skeletonId))
			{
				btResult = fbt.DestroyBodyTracking(ts, skeletonId).Type();

				sb.Clear().Append("DestroyBodyTracking() DestroyBodyTracking ").Append(skeletonId).Append(" result ").Append(btResult.Name()); DEBUG(sb);
				s_BodyIKInfos.Remove(skeletonId);
			}
			// After InitDefaultBodyTracking, we collected the default rotation spaces to s_BodyRotationSpaces.
			if (s_BodyRotationSpaces.ContainsKey(skeletonId)) { s_BodyRotationSpaces.Remove(skeletonId); }
			// After CalibrateBodyTracking, we collected the initial pose to s_BodyPoses. s_BodyPoses will be updated in UpdateBodyTrackingOnce().
			if (s_BodyPoses.ContainsKey(skeletonId)) { s_BodyPoses.Remove(skeletonId); }
			// In UpdateBodyTrackingOnce, we collected the Body Avatar information in s_BodyAvatars
			if (s_BodyAvatars.ContainsKey(skeletonId)) { s_BodyAvatars.Remove(skeletonId); }

			return btResult;
		}

		public BodyTrackingResult StartUpdatingBodyTracking(List<int> skeletonIds)
		{
			if (skeletonIds == null || skeletonIds.Count <= 0)
			{
				sb.Clear().Append("StartUpdatingBodyTracking() Invalid input.");
				ERROR(sb);
				return BodyTrackingResult.ERROR_INVALID_ARGUMENT;
			}

			if (s_SkeletonIds == null) { s_SkeletonIds = new List<int>(); }
			s_SkeletonIds.Clear();

			for (int i = 0; i < skeletonIds.Count; i++)
			{
				if (IsValidSkeletonId(skeletonIds[i]) == BodyTrackingResult.SUCCESS)
				{
					if (!s_SkeletonIds.Contains(skeletonIds[i])) { s_SkeletonIds.Add(skeletonIds[i]); }
					sb.Clear().Append("StartUpdatingBodyTracking() id: ").Append(skeletonIds[i]);
					DEBUG(sb);
				}
			}

			if (s_SkeletonIds.Count <= 0)
			{
				sb.Clear().Append("StartUpdatingBodyTracking() Invalid skeleton IDs.");
				ERROR(sb);
				return BodyTrackingResult.ERROR_IK_NOT_UPDATED;
			}

			return BodyTrackingResult.SUCCESS;
		}
		public BodyTrackingResult StartUpdatingBodyTracking(int skeletonId)
		{
			if (s_SkeletonIds == null) { s_SkeletonIds = new List<int>(); }

			BodyTrackingResult result = IsValidSkeletonId(skeletonId);
			if (result != BodyTrackingResult.SUCCESS)
			{
				sb.Clear().Append("StartUpdatingBodyTracking() invalid id ").Append(skeletonId);
				ERROR(sb);
				return result;
			}

			if (!s_SkeletonIds.Contains(skeletonId)) { s_SkeletonIds.Add(skeletonId); }

			sb.Clear().Append("StartUpdatingBodyTracking() id: ").Append(skeletonId);
			DEBUG(sb);
			return BodyTrackingResult.SUCCESS;
		}
		public BodyTrackingResult StopUpdatingBodyTracking(List<int> skeletonIds)
		{
			if (skeletonIds == null || skeletonIds.Count <= 0)
			{
				sb.Clear().Append("StopUpdatingBodyTracking() Invalid input.");
				ERROR(sb);
				return BodyTrackingResult.ERROR_INVALID_ARGUMENT;
			}

			if (s_SkeletonIds == null || s_SkeletonIds.Count <= 0)
			{
				sb.Clear().Append("StopUpdatingBodyTracking() No available IK.");
				ERROR(sb);
				return BodyTrackingResult.ERROR_IK_NOT_DESTROYED;
			}

			sb.Clear();
			for (int i = 0; i < skeletonIds.Count; i++)
			{
				if (s_SkeletonIds.Contains(skeletonIds[i])) { s_SkeletonIds.Remove(skeletonIds[i]); }
				sb.Append("StopUpdatingBodyTracking() id: ").Append(skeletonIds[i]).Append("\n");
			}
			DEBUG(sb);

			return BodyTrackingResult.SUCCESS;
		}
		public BodyTrackingResult StopUpdatingBodyTracking(int skeletonId)
		{
			if (s_SkeletonIds == null || s_SkeletonIds.Count <= 0)
			{
				sb.Clear().Append("StopUpdatingBodyTracking() No available IK.");
				ERROR(sb);
				return BodyTrackingResult.ERROR_IK_NOT_DESTROYED;
			}

			if (s_SkeletonIds.Contains(skeletonId)) { s_SkeletonIds.Remove(skeletonId); }

			sb.Clear().Append("StopUpdatingBodyTracking() id: ").Append(skeletonId);
			DEBUG(sb);
			return BodyTrackingResult.SUCCESS;
		}
	}
}
