// Copyright HTC Corporation All Rights Reserved.

using System.Text;
using UnityEngine;

namespace VIVE.OpenXR.Toolkits.BodyTracking.Demo
{
	public class BodyJointCoordinate : MonoBehaviour
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.Demo.BodyJointCoordinate";
		private StringBuilder m_sb = null;
		private StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Debug.Log(LOG_TAG + " " + msg); }
		int logFrame = 0;
		bool printIntervalLog = false;

		public Body inputBody;

		private void Update()
		{
			logFrame++;
			logFrame %= 300;
			printIntervalLog = (logFrame == 0);

			if (BodyManager.Instance.GetBodyTrackingPoses(0, out BodyAvatar avatarBody) == BodyTrackingResult.SUCCESS)
				UpdateBodyPosesInOrder(avatarBody);
		}

		private void UpdateBodyPosesInOrder(BodyAvatar avatarBody)
		{
			if (inputBody == null || avatarBody == null) { return; }

			inputBody.height = avatarBody.height;

			if (inputBody.root != null) avatarBody.Update(JointType.HIP, ref inputBody.root); // 0

			if (inputBody.leftThigh != null) avatarBody.Update(JointType.LEFTTHIGH, ref inputBody.leftThigh);
			if (inputBody.leftLeg != null) avatarBody.Update(JointType.LEFTLEG, ref inputBody.leftLeg);
			if (inputBody.leftAnkle != null) avatarBody.Update(JointType.LEFTANKLE, ref inputBody.leftAnkle);
			if (inputBody.leftFoot != null) avatarBody.Update(JointType.LEFTFOOT, ref inputBody.leftFoot);

			if (inputBody.rightThigh != null) avatarBody.Update(JointType.RIGHTTHIGH, ref inputBody.rightThigh); // 5
			if (inputBody.rightLeg != null) avatarBody.Update(JointType.RIGHTLEG, ref inputBody.rightLeg);
			if (inputBody.rightAnkle != null) avatarBody.Update(JointType.RIGHTANKLE, ref inputBody.rightAnkle);
			if (inputBody.rightFoot != null) avatarBody.Update(JointType.RIGHTFOOT, ref inputBody.rightFoot);

			if (inputBody.waist != null) avatarBody.Update(JointType.WAIST, ref inputBody.waist);

			if (inputBody.spineLower != null) avatarBody.Update(JointType.SPINELOWER, ref inputBody.spineLower); // 10
			if (inputBody.spineMiddle != null) avatarBody.Update(JointType.SPINEMIDDLE, ref inputBody.spineMiddle);
			if (inputBody.spineHigh != null) avatarBody.Update(JointType.SPINEHIGH, ref inputBody.spineHigh);

			if (inputBody.chest != null) avatarBody.Update(JointType.CHEST, ref inputBody.chest);
			if (inputBody.neck != null) avatarBody.Update(JointType.NECK, ref inputBody.neck);
			if (inputBody.head != null) avatarBody.Update(JointType.HEAD, ref inputBody.head); // 15

			if (inputBody.leftClavicle != null) avatarBody.Update(JointType.LEFTCLAVICLE, ref inputBody.leftClavicle);
			if (inputBody.leftScapula != null) avatarBody.Update(JointType.LEFTSCAPULA, ref inputBody.leftScapula);
			if (inputBody.leftUpperarm != null) avatarBody.Update(JointType.LEFTUPPERARM, ref inputBody.leftUpperarm);
			if (inputBody.leftForearm != null) avatarBody.Update(JointType.LEFTFOREARM, ref inputBody.leftForearm);
			if (inputBody.leftHand != null) avatarBody.Update(JointType.LEFTHAND, ref inputBody.leftHand); // 20

			if (inputBody.rightClavicle != null) avatarBody.Update(JointType.RIGHTCLAVICLE, ref inputBody.rightClavicle);
			if (inputBody.rightScapula != null) avatarBody.Update(JointType.RIGHTSCAPULA, ref inputBody.rightScapula);
			if (inputBody.rightUpperarm != null) avatarBody.Update(JointType.RIGHTUPPERARM, ref inputBody.rightUpperarm);
			if (inputBody.rightForearm != null) avatarBody.Update(JointType.RIGHTFOREARM, ref inputBody.rightForearm);
			if (inputBody.rightHand != null) avatarBody.Update(JointType.RIGHTHAND, ref inputBody.rightHand); // 25
		}
	}
}
