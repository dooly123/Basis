// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_XR_HANDS
using UnityEngine.XR.Hands;
#endif

namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
	#region Basic Hand Data

	/// <summary>
	/// This enum corresponds to HandManager.HandJoint and is used for categorizing joint types.
	/// </summary>
	public enum JointType : Int32
	{
		Palm = 0,
		Wrist = 1,
		Thumb_Joint0 = 2,
		Thumb_Joint1 = 3,
		Thumb_Joint2 = 4,
		Thumb_Tip = 5,
		Index_Joint0 = 6,
		Index_Joint1 = 7,
		Index_Joint2 = 8,
		Index_Joint3 = 9,
		Index_Tip = 10,
		Middle_Joint0 = 11,
		Middle_Joint1 = 12,
		Middle_Joint2 = 13,
		Middle_Joint3 = 14,
		Middle_Tip = 15,
		Ring_Joint0 = 16,
		Ring_Joint1 = 17,
		Ring_Joint2 = 18,
		Ring_Joint3 = 19,
		Ring_Tip = 20,
		Pinky_Joint0 = 21,
		Pinky_Joint1 = 22,
		Pinky_Joint2 = 23,
		Pinky_Joint3 = 24,
		Pinky_Tip = 25,
		Count = Pinky_Tip + 1,
	}

	/// <summary>
	/// This class is designed to update hand tracking data.
	/// </summary>
#if UNITY_XR_HANDS
	public static class DataWrapper
	{
		private static XRHandSubsystem handSubsystem = null;
		private static List<XRHandSubsystem> s_XRHandSubsystems = new List<XRHandSubsystem>();

		/// <summary>
		/// Validate whether the hand tracking is active.
		/// </summary>
		/// <returns>True if the hand tracking is active; otherwise, false.</returns>
		public static bool Validate()
		{
			if (handSubsystem == null || !handSubsystem.running)
			{
				SubsystemManager.GetSubsystems(s_XRHandSubsystems);
				for (int i = 0; i < s_XRHandSubsystems.Count; i++)
				{
					if (handSubsystem != null)
					{
						handSubsystem = null;
					}
					handSubsystem = s_XRHandSubsystems[i];
				}
			}
			return handSubsystem != null && handSubsystem.running;
		}

		/// <summary>
		/// Validate whether the hand tracking is successfully tracking.
		/// </summary>
		/// <param name="isLeft">True if the hand is left; otherwise, false.</param>
		/// <returns>True if the hand tracking is successfully tracking; otherwise, false.</returns>
		public static bool IsHandTracked(bool isLeft)
		{
			if (handSubsystem != null)
			{
				return isLeft ? handSubsystem.leftHand.isTracked : handSubsystem.rightHand.isTracked;
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="jointType"></param>
		/// <param name="rotation">The reference to store the position of the joint.</param>
		/// <param name="rotation">The reference to store the rotation of the joint.</param>
		/// <param name="isLeft"></param>
		/// <returns></returns>
		public static bool GetJointPose(JointType jointType, ref Vector3 position, ref Quaternion rotation, bool isLeft)
		{
			if (IsHandTracked(isLeft))
			{
				XRHand hand = isLeft ? handSubsystem.leftHand : handSubsystem.rightHand;
				XRHandJoint xrHandJoint = hand.GetJoint(ConvertToXRHandJointID(jointType));
				if (xrHandJoint.TryGetPose(out Pose pose))
				{
					position = pose.position;
					rotation = pose.rotation;
					return true;
				}
			}
			return false;
		}

		private static XRHandJointID ConvertToXRHandJointID(JointType jointType)
		{
			int id = (int)jointType;
			switch (id)
			{
				case 0:
					return XRHandJointID.Palm;
				case 1:
					return XRHandJointID.Wrist;
				default:
					return (XRHandJointID)(id + 1);
			}
		}
	}
#else
	public static class DataWrapper
	{
		public static bool Validate() 
		{ 
			return false;
		}

		public static bool IsHandTracked(bool isLeft)
		{
			return false;
		}

		public static bool GetJointPose(JointType jointType, ref Vector3 position, ref Quaternion rotation, bool isLeft)
		{
			return false;
		}
	}
#endif

	/// <summary>
	/// The enum is designed to define the IDs of joints.
	/// </summary>
	public enum JointId : Int32
	{
		Invalid = -1,
		Joint0 = 0,
		Joint1 = 1,
		Joint2 = 2,
		Joint3 = 3,
		Tip = 4,
		Count = 5,
	}

	/// <summary>
	/// The struct is designed to record data for each joint.
	/// </summary>
	public struct JointData
	{
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 velocity;

		public JointData(Vector3 in_pos, Quaternion in_rot, Vector3 in_vel)
		{
			position = in_pos;
			rotation = in_rot;
			velocity = in_vel;
		}

		public static JointData identity => new JointData(Vector3.zero, Quaternion.identity, Vector3.zero);

		public void Update(Vector3 in_pos, Quaternion in_rot, Vector3 in_vel)
		{
			position = in_pos;
			rotation = in_rot;
			velocity = in_vel;
		}

		public void Reset()
		{
			position = Vector3.zero;
			rotation = Quaternion.identity;
			velocity = Vector3.zero;
		}
	}

	/// <summary>
	/// The enum is designed to define the IDs of fingers.
	/// </summary>
	public enum FingerId : Int32
	{
		Invalid = -1,
		Thumb = 0,
		Index = 1,
		Middle = 2,
		Ring = 3,
		Pinky = 4,
		Count = 5
	}

	/// <summary>
	/// The enum is designed to define the flags of FingerId.
	/// </summary>
	[Flags]
	public enum FingerFlags
	{
		None = 0,
		Thumb = 1 << FingerId.Thumb,
		Index = 1 << FingerId.Index,
		Middle = 1 << FingerId.Middle,
		Ring = 1 << FingerId.Ring,
		Pinky = 1 << FingerId.Pinky,
		All = Thumb | Index | Middle | Ring | Pinky
	}

	/// <summary>
	/// The class is designed to record each joint data of finger.
	/// </summary>
	public class FingerData
	{
		public Vector3 direction = Vector3.zero;
		public JointData[] joints = null;
		public JointData joint0 {
			get {
				return joints[(Int32)JointId.Joint0];
			}
		}
		public JointData joint1 {
			get {
				return joints[(Int32)JointId.Joint1];
			}
		}
		public JointData joint2 {
			get {
				return joints[(Int32)JointId.Joint2];
			}
		}
		public JointData joint3 {
			get {
				return joints[(Int32)JointId.Joint3];
			}
		}
		public JointData tip {
			get {
				return joints[(Int32)JointId.Tip];
			}
		}

		public FingerData()
		{
			joints = new JointData[(Int32)JointId.Count];
		}
	}

	/// <summary>
	/// The enum is designed to define the IDs of hands.
	/// </summary>
	public enum HandId : Int32
	{
		Right = 0,
		Left = 1,
		Both = 2
	}

	/// <summary>
	/// This enum corresponds to HandId and is used for categorizing hand types.
	/// </summary>
	public enum Handedness
	{
		Right = HandId.Right,
		Left = HandId.Left,
	}

	/// <summary>
	/// The class is designed to record each finger data of hand and tracking state.
	/// </summary>
	public class HandData
	{
		// Local rotation of the fingers at different degrees of bending.
		private static readonly Dictionary<bool, Dictionary<FingerBendingLevel, Quaternion[]>> s_FingersBending = new Dictionary<bool, Dictionary<FingerBendingLevel, Quaternion[]>>()
		{
			{
				true, new Dictionary<FingerBendingLevel, Quaternion[]>()
				{
					{
						FingerBendingLevel.Level0, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(-0.11262f, 0.25416f, -0.69724f, 0.66074f),
							new Quaternion(-0.03846f, -0.00148f, 0.00000f, 0.99926f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.00743f, 0.00133f, 0.00060f, 0.99997f),
							new Quaternion(0.00000f, 0.04362f, 0.00000f, 0.99905f),
							new Quaternion(0.13198f, 0.09994f, -0.06426f, 0.98410f),
							new Quaternion(-0.10418f, -0.00017f, 0.00022f, 0.99456f),
							new Quaternion(0.00037f, -0.00007f, 0.00003f, 1.00000f),
							new Quaternion(-0.01159f, 0.00267f, -0.00007f, 0.99993f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.09351f, 0.06177f, -0.00584f, 0.99368f),
							new Quaternion(-0.10136f, 0.00105f, 0.00001f, 0.99485f),
							new Quaternion(0.00014f, 0.00000f, 0.00021f, 1.00000f),
							new Quaternion(-0.01364f, 0.00797f, 0.00001f, 0.99988f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, 0.99619f),
							new Quaternion(0.06461f, 0.07685f, 0.10882f, 0.98898f),
							new Quaternion(-0.10454f, -0.00005f, 0.00001f, 0.99452f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(-0.01527f, 0.00108f, 0.00021f, 0.99988f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, 0.99144f),
							new Quaternion(-0.00779f, -0.04296f, 0.17214f, 0.98410f),
							new Quaternion(0.00005f, 0.00004f, 0.00006f, 1.00000f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(-0.01093f, -0.00441f, -0.00067f, 0.99993f),
						}
					},
					{
						FingerBendingLevel.Level1, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(0.08584f, 0.41049f, -0.74290f, 0.52175f),
							new Quaternion(-0.01511f, 0.00187f, -0.00105f, 0.99988f),
							new Quaternion(0.00122f, 0.00035f, 0.00112f, 1.00000f),
							new Quaternion(0.00952f, 0.00195f, -0.00441f, 0.99994f),
							new Quaternion(0.00000f, 0.04362f, 0.00000f, 0.99905f),
							new Quaternion(-0.07049f, -0.04325f, -0.05602f, 0.99500f),
							new Quaternion(0.30987f, -0.00018f, 0.00052f, 0.95078f),
							new Quaternion(0.08450f, -0.00161f, 0.00108f, 0.99642f),
							new Quaternion(0.00826f, 0.00448f, -0.00361f, 0.99995f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(-0.08271f, -0.12642f, -0.01109f, 0.98846f),
							new Quaternion(0.35610f, -0.00018f, 0.00067f, 0.93445f),
							new Quaternion(0.06270f, -0.00060f, 0.00049f, 0.99803f),
							new Quaternion(-0.00183f, 0.00981f, -0.00822f, 0.99992f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, 0.99619f),
							new Quaternion(-0.06405f, -0.11035f, 0.09828f, 0.98695f),
							new Quaternion(0.23558f, 0.00047f, 0.00041f, 0.97186f),
							new Quaternion(0.26601f, 0.00557f, -0.00305f, 0.96395f),
							new Quaternion(-0.02288f, 0.00326f, -0.00459f, 0.99972f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, 0.99144f),
							new Quaternion(-0.00587f, -0.12947f, 0.15966f, 0.97863f),
							new Quaternion(0.21401f, -0.00210f, 0.00028f, 0.97683f),
							new Quaternion(0.19785f, -0.00033f, 0.00033f, 0.98023f),
							new Quaternion(0.00340f, 0.00377f, -0.00316f, 0.99998f),
						}
					},
					{
						FingerBendingLevel.Level2, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(0.13107f, 0.34075f, -0.75125f, 0.54985f),
							new Quaternion(0.02612f, 0.00033f, 0.00102f, 0.99966f),
							new Quaternion(0.13555f, 0.00348f, -0.00306f, 0.99076f),
							new Quaternion(-0.00298f, -0.00081f, 0.00466f, 0.99998f),
							new Quaternion(0.00000f, 0.04362f, 0.00000f, 0.99905f),
							new Quaternion(0.18517f, 0.06679f, -0.06457f, 0.97831f),
							new Quaternion(0.42630f, 0.00348f, -0.00361f, 0.90457f),
							new Quaternion(0.01349f, -0.00133f, 0.00466f, 0.99990f),
							new Quaternion(0.01817f, -0.00123f, 0.00384f, 0.99983f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.19922f, 0.06582f, -0.01357f, 0.97765f),
							new Quaternion(0.41682f, 0.00047f, -0.00043f, 0.90899f),
							new Quaternion(0.09295f, -0.00016f, 0.00089f, 0.99567f),
							new Quaternion(0.01949f, -0.00126f, 0.01275f, 0.99973f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, 0.99619f),
							new Quaternion(0.19167f, 0.06709f, 0.10157f, 0.97388f),
							new Quaternion(0.44857f, -0.00059f, 0.00108f, 0.89375f),
							new Quaternion(0.04810f, -0.00002f, 0.00029f, 0.99884f),
							new Quaternion(0.01730f, -0.00086f, 0.00529f, 0.99984f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, 0.99144f),
							new Quaternion(0.30474f, 0.01742f, 0.17554f, 0.93596f),
							new Quaternion(0.21203f, -0.00469f, 0.00512f, 0.97724f),
							new Quaternion(0.07055f, 0.00188f, -0.00330f, 0.99750f),
							new Quaternion(-0.01002f, -0.00507f, 0.00883f, 0.99990f),
						}
					},
					{
						FingerBendingLevel.Level3, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(0.19528f, 0.35744f, -0.76287f, 0.50212f),
							new Quaternion(-0.04099f, 0.00351f, 0.00061f, 0.99915f),
							new Quaternion(0.31070f, 0.00152f, -0.00356f, 0.95050f),
							new Quaternion(0.02296f, 0.00583f, -0.01160f, 0.99965f),
							new Quaternion(0.00000f, 0.04362f, 0.00000f, 0.99905f),
							new Quaternion(0.37185f, 0.08682f, -0.08873f, 0.91995f),
							new Quaternion(0.48089f, -0.00220f, 0.00500f, 0.87676f),
							new Quaternion(0.00267f, 0.00023f, -0.00010f, 1.00000f),
							new Quaternion(0.01532f, -0.00006f, -0.00193f, 0.99988f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.35511f, 0.03925f, -0.01449f, 0.93389f),
							new Quaternion(0.47741f, -0.00149f, 0.00245f, 0.87867f),
							new Quaternion(0.09010f, 0.00021f, 0.00067f, 0.99593f),
							new Quaternion(0.02853f, -0.00213f, -0.00656f, 0.99957f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, 0.99619f),
							new Quaternion(0.31547f, -0.03149f, 0.12452f, 0.94020f),
							new Quaternion(0.50233f, -0.00093f, 0.00163f, 0.86467f),
							new Quaternion(0.00506f, -0.00058f, 0.00043f, 0.99999f),
							new Quaternion(0.00671f, 0.00015f, -0.00312f, 0.99997f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, 0.99144f),
							new Quaternion(0.34884f, -0.07124f, 0.19724f, 0.91342f),
							new Quaternion(0.37271f, 0.00005f, 0.00052f, 0.92795f),
							new Quaternion(0.00246f, -0.00112f, 0.00072f, 1.00000f),
							new Quaternion(0.02218f, 0.00207f, -0.00514f, 0.99974f),
						}
					},
					{
						FingerBendingLevel.Level4, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(0.10211f, 0.44288f, -0.74597f, 0.48678f),
							new Quaternion(0.19065f, 0.00005f, 0.00039f, 0.98166f),
							new Quaternion(0.62796f, -0.00156f, -0.00539f, 0.77822f),
							new Quaternion(-0.01918f, 0.01012f, 0.00302f, 0.99976f),
							new Quaternion(0.00000f, 0.04362f, 0.00000f, 0.99905f),
							new Quaternion(0.52214f, -0.02285f, -0.04694f, 0.85126f),
							new Quaternion(0.67094f, 0.00003f, 0.00042f, 0.74152f),
							new Quaternion(0.29652f, 0.00077f, 0.00024f, 0.95503f),
							new Quaternion(0.00114f, -0.00613f, 0.00019f, 0.99998f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.51402f, -0.06984f, 0.04232f, 0.85388f),
							new Quaternion(0.67124f, -0.00060f, -0.00234f, 0.74124f),
							new Quaternion(0.40244f, 0.00146f, 0.00035f, 0.91544f),
							new Quaternion(-0.00075f, 0.00310f, -0.00062f, 0.99999f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, 0.99619f),
							new Quaternion(0.42444f, -0.01220f, 0.12585f, 0.89659f),
							new Quaternion(0.70429f, 0.00002f, 0.00069f, 0.70991f),
							new Quaternion(0.44565f, 0.00261f, 0.00074f, 0.89520f),
							new Quaternion(0.00448f, 0.00155f, -0.00006f, 0.99999f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, 0.99144f),
							new Quaternion(0.33697f, 0.02717f, 0.17449f, 0.92481f),
							new Quaternion(0.70669f, 0.00004f, -0.00011f, 0.70752f),
							new Quaternion(0.22958f, -0.00104f, -0.00111f, 0.97329f),
							new Quaternion(-0.02959f, 0.00321f, 0.00209f, 0.99955f),
						}
					},
					{
						FingerBendingLevel.Level5, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(0.06759f, 0.29327f, -0.73738f, 0.60473f),
							new Quaternion(-0.00881f, 0.00852f, -0.00361f, 0.99992f),
							new Quaternion(0.60423f, -0.00304f, 0.00496f, 0.79679f),
							new Quaternion(0.03855f, 0.00866f, 0.00562f, 0.99920f),
							new Quaternion(0.00000f, 0.04362f, 0.00000f, 0.99905f),
							new Quaternion(0.64690f, -0.02744f, -0.04492f, 0.76076f),
							new Quaternion(0.70685f, 0.00011f, 0.00020f, 0.70736f),
							new Quaternion(-0.34253f, 0.00102f, -0.00017f, -0.93950f),
							new Quaternion(-0.00715f, -0.00086f, 0.00053f, 0.99997f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.69063f, -0.05554f, 0.05341f, 0.71909f),
							new Quaternion(0.70712f, -0.00009f, -0.00005f, 0.70710f),
							new Quaternion(-0.41333f, 0.00119f, -0.00041f, -0.91058f),
							new Quaternion(-0.00444f, -0.00008f, 0.00022f, 0.99999f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, 0.99619f),
							new Quaternion(0.64441f, 0.03540f, 0.11369f, 0.75535f),
							new Quaternion(0.70712f, 0.00000f, -0.00001f, 0.70710f),
							new Quaternion(-0.44200f, 0.00351f, -0.00085f, -0.89701f),
							new Quaternion(0.00568f, -0.00249f, 0.00231f, 0.99998f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, 0.99144f),
							new Quaternion(0.57291f, 0.02400f, 0.18783f, 0.79744f),
							new Quaternion(0.70707f, -0.00001f, 0.00015f, 0.70714f),
							new Quaternion(0.44752f, -0.00011f, -0.00001f, 0.89427f),
							new Quaternion(0.01201f, -0.00357f, 0.00210f, 0.99992f),
						}
					},
				}
			},
			{
				false, new Dictionary<FingerBendingLevel, Quaternion[]>()
				{
					{
						FingerBendingLevel.Level0, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(0.11262f, 0.25416f, -0.69724f, -0.66074f),
							new Quaternion(-0.03846f, 0.00148f, 0.00000f, 0.99926f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.00743f, -0.00133f, -0.00060f, 0.99997f),
							new Quaternion(0.00000f, -0.04362f, 0.00000f, 0.99905f),
							new Quaternion(-0.13198f, 0.09994f, -0.06426f, -0.98410f),
							new Quaternion(-0.10418f, 0.00017f, -0.00022f, 0.99456f),
							new Quaternion(-0.00037f, -0.00007f, 0.00003f, -1.00000f),
							new Quaternion(-0.01159f, -0.00267f, 0.00007f, 0.99993f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.09351f, -0.06177f, 0.00584f, 0.99368f),
							new Quaternion(-0.10136f, -0.00105f, -0.00001f, 0.99485f),
							new Quaternion(0.00014f, 0.00000f, -0.00021f, 1.00000f),
							new Quaternion(-0.01364f, -0.00797f, -0.00001f, 0.99988f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, -0.99619f),
							new Quaternion(0.06461f, -0.07685f, -0.10882f, 0.98898f),
							new Quaternion(-0.10454f, 0.00005f, -0.00001f, 0.99452f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.01527f, 0.00108f, 0.00021f, -0.99988f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, -0.99144f),
							new Quaternion(-0.00779f, 0.04296f, -0.17214f, 0.98410f),
							new Quaternion(0.00005f, -0.00004f, -0.00006f, 1.00000f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.01093f, -0.00441f, -0.00067f, -0.99993f),
						}
					},
					{
						FingerBendingLevel.Level1, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(-0.08584f, 0.41049f, -0.74290f, -0.52175f),
							new Quaternion(-0.01511f, -0.00187f, 0.00105f, 0.99988f),
							new Quaternion(0.00122f, -0.00035f, -0.00112f, 1.00000f),
							new Quaternion(-0.00952f, 0.00195f, -0.00441f, -0.99994f),
							new Quaternion(0.00000f, -0.04362f, 0.00000f, 0.99905f),
							new Quaternion(0.07049f, -0.04325f, -0.05602f, -0.99500f),
							new Quaternion(0.30987f, 0.00018f, -0.00052f, 0.95078f),
							new Quaternion(-0.08450f, -0.00161f, 0.00108f, -0.99642f),
							new Quaternion(-0.00826f, 0.00448f, -0.00361f, -0.99995f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.08271f, -0.12642f, -0.01109f, -0.98846f),
							new Quaternion(0.35610f, 0.00018f, -0.00067f, 0.93445f),
							new Quaternion(-0.06270f, -0.00060f, 0.00049f, -0.99803f),
							new Quaternion(-0.00183f, -0.00981f, 0.00822f, 0.99992f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, -0.99619f),
							new Quaternion(-0.06405f, 0.11035f, -0.09828f, 0.98695f),
							new Quaternion(0.23558f, -0.00047f, -0.00041f, 0.97186f),
							new Quaternion(-0.26601f, 0.00557f, -0.00305f, -0.96395f),
							new Quaternion(-0.02288f, -0.00326f, 0.00459f, 0.99972f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, -0.99144f),
							new Quaternion(0.00587f, -0.12947f, 0.15966f, -0.97863f),
							new Quaternion(0.21401f, 0.00210f, -0.00028f, 0.97683f),
							new Quaternion(-0.19785f, -0.00033f, 0.00033f, -0.98023f),
							new Quaternion(-0.00340f, 0.00377f, -0.00316f, -0.99998f),
						}
					},
					{
						FingerBendingLevel.Level2, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(-0.13107f, 0.34075f, -0.75125f, -0.54985f),
							new Quaternion(0.02612f, -0.00033f, -0.00102f, 0.99966f),
							new Quaternion(-0.13555f, 0.00348f, -0.00306f, -0.99076f),
							new Quaternion(-0.00298f, 0.00081f, -0.00466f, 0.99998f),
							new Quaternion(0.00000f, -0.04362f, 0.00000f, 0.99905f),
							new Quaternion(-0.18517f, 0.06679f, -0.06457f, -0.97831f),
							new Quaternion(-0.42630f, 0.00348f, -0.00361f, -0.90457f),
							new Quaternion(-0.01349f, -0.00133f, 0.00466f, -0.99990f),
							new Quaternion(-0.01817f, -0.00123f, 0.00384f, -0.99983f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(-0.19922f, 0.06582f, -0.01357f, -0.97765f),
							new Quaternion(-0.41682f, 0.00047f, -0.00043f, -0.90899f),
							new Quaternion(-0.09295f, -0.00016f, 0.00089f, -0.99567f),
							new Quaternion(-0.01949f, -0.00126f, 0.01275f, -0.99973f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, -0.99619f),
							new Quaternion(0.19167f, -0.06709f, -0.10157f, 0.97388f),
							new Quaternion(-0.44857f, -0.00059f, 0.00108f, -0.89375f),
							new Quaternion(0.04810f, 0.00002f, -0.00029f, 0.99884f),
							new Quaternion(-0.01730f, -0.00086f, 0.00529f, -0.99984f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, -0.99144f),
							new Quaternion(0.30474f, -0.01742f, -0.17554f, 0.93596f),
							new Quaternion(-0.21203f, -0.00469f, 0.00512f, -0.97724f),
							new Quaternion(-0.07055f, 0.00188f, -0.00330f, -0.99750f),
							new Quaternion(-0.01002f, 0.00507f, -0.00883f, 0.99990f),
						}
					},
					{
						FingerBendingLevel.Level3, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(-0.19528f, 0.35744f, -0.76287f, -0.50212f),
							new Quaternion(0.04099f, 0.00351f, 0.00061f, -0.99915f),
							new Quaternion(-0.31070f, 0.00152f, -0.00356f, -0.95050f),
							new Quaternion(-0.02296f, 0.00583f, -0.01160f, -0.99965f),
							new Quaternion(0.00000f, -0.04362f, 0.00000f, 0.99905f),
							new Quaternion(-0.37185f, 0.08682f, -0.08873f, -0.91995f),
							new Quaternion(0.48089f, 0.00220f, -0.00500f, 0.87676f),
							new Quaternion(-0.00267f, 0.00023f, -0.00010f, -1.00000f),
							new Quaternion(0.01532f, 0.00006f, 0.00193f, 0.99988f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(0.35511f, -0.03925f, 0.01449f, 0.93389f),
							new Quaternion(-0.47741f, -0.00149f, 0.00245f, -0.87867f),
							new Quaternion(0.09010f, -0.00021f, -0.00067f, 0.99593f),
							new Quaternion(0.02853f, 0.00213f, 0.00656f, 0.99957f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, -0.99619f),
							new Quaternion(0.31547f, 0.03149f, -0.12452f, 0.94020f),
							new Quaternion(0.50233f, 0.00093f, -0.00163f, 0.86467f),
							new Quaternion(-0.00506f, -0.00058f, 0.00043f, -0.99999f),
							new Quaternion(-0.00671f, 0.00015f, -0.00312f, -0.99997f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, -0.99144f),
							new Quaternion(0.34884f, 0.07124f, -0.19724f, 0.91342f),
							new Quaternion(0.37271f, -0.00005f, -0.00052f, 0.92795f),
							new Quaternion(-0.00246f, -0.00112f, 0.00072f, -1.00000f),
							new Quaternion(-0.02218f, 0.00207f, -0.00514f, -0.99974f),
						}
					},
					{
						FingerBendingLevel.Level4, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(-0.10211f, 0.44288f, -0.74597f, -0.48678f),
							new Quaternion(0.19065f, -0.00005f, -0.00039f, 0.98166f),
							new Quaternion(0.62796f, 0.00156f, 0.00539f, 0.77822f),
							new Quaternion(0.01918f, 0.01012f, 0.00302f, -0.99976f),
							new Quaternion(0.00000f, -0.04362f, 0.00000f, 0.99905f),
							new Quaternion(0.52214f, 0.02285f, 0.04694f, 0.85126f),
							new Quaternion(0.67094f, -0.00003f, -0.00042f, 0.74152f),
							new Quaternion(0.29652f, -0.00077f, -0.00024f, 0.95503f),
							new Quaternion(-0.00114f, -0.00613f, 0.00019f, -0.99998f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(-0.51402f, -0.06984f, 0.04232f, -0.85388f),
							new Quaternion(0.67124f, 0.00060f, 0.00234f, 0.74124f),
							new Quaternion(0.40244f, -0.00146f, -0.00035f, 0.91544f),
							new Quaternion(-0.00075f, -0.00310f, 0.00062f, 0.99999f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, -0.99619f),
							new Quaternion(0.42444f, 0.01220f, -0.12585f, 0.89659f),
							new Quaternion(0.70429f, -0.00002f, -0.00069f, 0.70991f),
							new Quaternion(0.44565f, -0.00261f, -0.00074f, 0.89520f),
							new Quaternion(-0.00448f, 0.00155f, -0.00007f, -0.99999f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, -0.99144f),
							new Quaternion(0.33697f, -0.02717f, -0.17449f, 0.92481f),
							new Quaternion(0.70670f, -0.00004f, 0.00004f, 0.70751f),
							new Quaternion(0.22958f, 0.00104f, 0.00111f, 0.97329f),
							new Quaternion(0.02959f, 0.00321f, 0.00209f, -0.99955f),
						}
					},
					{
						FingerBendingLevel.Level5, new Quaternion[(int)JointType.Count]
						{
							Quaternion.identity,
							Quaternion.identity,
							new Quaternion(-0.06759f, 0.29327f, -0.73738f, -0.60473f),
							new Quaternion(-0.00881f, -0.00852f, 0.00361f, 0.99992f),
							new Quaternion(0.60423f, 0.00304f, -0.00496f, 0.79679f),
							new Quaternion(0.03855f, -0.00866f, -0.00562f, 0.99920f),
							new Quaternion(0.00000f, -0.04362f, 0.00000f, 0.99905f),
							new Quaternion(0.64690f, 0.02744f, 0.04492f, 0.76076f),
							new Quaternion(0.70686f, -0.00011f, 0.00011f, 0.70735f),
							new Quaternion(0.34253f, 0.00102f, -0.00016f, 0.93950f),
							new Quaternion(-0.00715f, 0.00086f, -0.00053f, 0.99997f),
							new Quaternion(0.00000f, 0.00000f, 0.00000f, 1.00000f),
							new Quaternion(-0.69063f, -0.05554f, 0.05341f, -0.71909f),
							new Quaternion(-0.70711f, -0.00009f, 0.00009f, -0.70711f),
							new Quaternion(0.41333f, 0.00119f, -0.00041f, 0.91058f),
							new Quaternion(-0.00444f, 0.00008f, -0.00022f, 0.99999f),
							new Quaternion(0.00000f, -0.08716f, 0.00000f, -0.99619f),
							new Quaternion(0.64441f, -0.03540f, -0.11369f, 0.75535f),
							new Quaternion(0.70711f, 0.00000f, 0.00000f, 0.70711f),
							new Quaternion(0.44200f, 0.00351f, -0.00085f, 0.89701f),
							new Quaternion(-0.00568f, -0.00249f, 0.00231f, -0.99998f),
							new Quaternion(0.00000f, -0.13053f, 0.00000f, -0.99144f),
							new Quaternion(0.57291f, -0.02400f, -0.18783f, 0.79744f),
							new Quaternion(0.70711f, 0.00001f, -0.00001f, 0.70711f),
							new Quaternion(0.44752f, 0.00011f, 0.00001f, 0.89427f),
							new Quaternion(-0.01201f, -0.00357f, 0.00210f, -0.99992f),
						}
					},
				}
			},
		};

		public bool valid = false;
		public bool isTracked = false;
		public Vector3 normal = Vector3.zero;
		public Vector3 direction = Vector3.zero;
		public JointData palm;
		public JointData wrist;
		public FingerData[] fingers = null; // size: FingerId.Count
		public FingerData thumb {
			get {
				return fingers[(Int32)FingerId.Thumb];
			}
		}
		public FingerData index {
			get {
				return fingers[(Int32)FingerId.Index];
			}
		}
		public FingerData middle {
			get {
				return fingers[(Int32)FingerId.Middle];
			}
		}
		public FingerData ring {
			get {
				return fingers[(Int32)FingerId.Ring];
			}
		}
		public FingerData pinky {
			get {
				return fingers[(Int32)FingerId.Pinky];
			}
		}

		public HandData()
		{
			fingers = new FingerData[(Int32)FingerId.Count];
			for (int i = 0; i < fingers.Length; i++)
			{
				fingers[i] = new FingerData();
			}
		}

		/// <summary>
		/// Get the position of a specific joint.
		/// </summary>
		/// <param name="joint">The type of joint to get.</param>
		/// <param name="position">The reference to store the position of the joint.</param>
		/// <returns>True if the joint position is successfully retrieved; otherwise, false.</returns>
		public bool GetJointPosition(JointType joint, ref Vector3 position)
		{
			GetJointIndex(joint, out int handId, out int jointId);
			switch (handId)
			{
				case 0: position = palm.position; break;
				case 1: position = wrist.position; break;
				case 2: position = thumb.joints[jointId].position; break;
				case 3: position = index.joints[jointId].position; break;
				case 4: position = middle.joints[jointId].position; break;
				case 5: position = ring.joints[jointId].position; break;
				case 6: position = pinky.joints[jointId].position; break;
				default: return false;
			}
			return true;
		}

		/// <summary>
		/// Get the rotation of a specific joint.
		/// </summary>
		/// <param name="joint">The type of joint to get.</param>
		/// <param name="rotation">The reference to store the rotation of the joint.</param>
		/// <returns>True if the joint rotation is successfully retrieved; otherwise, false.</returns>
		public bool GetJointRotation(JointType joint, ref Quaternion rotation)
		{
			GetJointIndex(joint, out int handId, out int jointId);
			switch (handId)
			{
				case 0: rotation = palm.rotation; break;
				case 1: rotation = wrist.rotation; break;
				case 2: rotation = thumb.joints[jointId].rotation; break;
				case 3: rotation = index.joints[jointId].rotation; break;
				case 4: rotation = middle.joints[jointId].rotation; break;
				case 5: rotation = ring.joints[jointId].rotation; break;
				case 6: rotation = pinky.joints[jointId].rotation; break;
				default: return false;
			}
			return true;
		}

		/// <summary>
		/// Get the default joint rotation for a specific HandGrabGesture and joint type.
		/// </summary>
		/// <param name="isLeft">True if the hand is left; otherwise, false.</param>
		/// <param name="handGrabGesture">The HandGrabGesture is a gesture where a hand grabs.</param>
		/// <param name="joint">The type of joint.</param>
		/// <param name="rotation">The reference to store the default joint rotation.</param>
		/// <returns>True if the default joint rotation is successfully retrieved; otherwise, false.</returns>
		public bool GetDefaultJointRotationInGesture(bool isLeft, HandGrabGesture handGrabGesture, JointType joint, ref Quaternion rotation)
		{
			FingerBendingLevel bendingLevel = FingerBendingLevel.Level0;
			GetJointIndex(joint, out int group, out int index);
			switch (group)
			{
				case 2: bendingLevel = handGrabGesture.thumbPose; break;
				case 3: bendingLevel = handGrabGesture.indexPose; break;
				case 4: bendingLevel = handGrabGesture.middlePose; break;
				case 5: bendingLevel = handGrabGesture.ringPose; break;
				case 6: bendingLevel = handGrabGesture.pinkyPose; break;
				default: return false;
			}
			rotation = s_FingersBending[isLeft][bendingLevel][(int)joint];
			return true;
		}

		/// <summary>
		/// Determine the group and index of a given joint type.
		/// </summary>
		/// <param name="joint">The type of joint.</param>
		/// <param name="group">The group to which the joint belongs.</param>
		/// <param name="index">The index of the joint within its group.</param>
		private static void GetJointIndex(JointType joint, out int group, out int index)
		{
			int jointId = (int)joint + 1;
			group = 0;
			index = jointId;

			// palm, wrist, thumb, index, middle, ring, pinky
			int[] fingerGroup = { 1, 1, 4, 5, 5, 5, 5 };
			while (index > fingerGroup[group])
			{
				index -= fingerGroup[group];
				group += 1;
			}
			index -= 1;
		}
	}

	/// <summary>
	/// The class is designed to temporarily store all hand data for this frame to avoid redundant updates.
	/// </summary>
	public static class CachedHand
	{
		public static readonly JointType[] s_ThumbJoints = new JointType[4] {
			JointType.Thumb_Joint0, JointType.Thumb_Joint1, JointType.Thumb_Joint2, JointType.Thumb_Tip
		};
		public static readonly JointType[] s_IndexJoints = new JointType[5] {
			JointType.Index_Joint0, JointType.Index_Joint1, JointType.Index_Joint2, JointType.Index_Joint3, JointType.Index_Tip
		};
		public static readonly JointType[] s_MiddleJoints = new JointType[5] {
			JointType.Middle_Joint0, JointType.Middle_Joint1, JointType.Middle_Joint2, JointType.Middle_Joint3, JointType.Middle_Tip
		};
		public static readonly JointType[] s_RingJoints = new JointType[5] {
			JointType.Ring_Joint0, JointType.Ring_Joint1, JointType.Ring_Joint2, JointType.Ring_Joint3, JointType.Ring_Tip
		};
		public static readonly JointType[] s_PinkyJoints = new JointType[5] {
			JointType.Pinky_Joint0, JointType.Pinky_Joint1, JointType.Pinky_Joint2, JointType.Pinky_Joint3, JointType.Pinky_Tip
		};

		private static int updateFrameCountLeft = 0, updateFrameCountRight = 0, handCount = 2;
		private static HandData[] hands = new HandData[handCount];

		/// <summary>
		/// Determine whether to allow updating hand data based on the handedness of the hand and frame count.
		/// </summary>
		/// <param name="isLeft">True if the hand is left; otherwise, false.</param>
		/// <returns>True if updating hand data is allowed; otherwise, false.</returns>
		private static bool AllowUpdateData(bool isLeft)
		{
			if (isLeft && (updateFrameCountLeft != Time.frameCount))
			{
				updateFrameCountLeft = Time.frameCount;
				return true;
			}
			if (!isLeft && (updateFrameCountRight != Time.frameCount))
			{
				updateFrameCountRight = Time.frameCount;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Get data for a specific joint including position, rotation, and velocity.
		/// </summary>
		/// <param name="id">The type of joint.</param>
		/// <param name="data">The reference to store the joint data.</param>
		/// <param name="isLeft">True if the hand is left; otherwise, false.</param>
		private static void GetJointData(JointType id, ref JointData data, bool isLeft)
		{
			Vector3 lastPosition = data.position;
			DataWrapper.GetJointPose(id, ref data.position, ref data.rotation, isLeft);
			Vector3 poositionOffset = data.position - lastPosition;
			data.velocity = poositionOffset / Time.deltaTime;
		}

		/// <summary>
		/// Get data for a specific finger.
		/// </summary>
		/// <param name="id">The type of finger.</param>
		/// <param name="finger">The reference to store the finger data.</param>
		/// <param name="isLeft">True if the hand is left; otherwise, false.</param>
		private static void GetFingerData(FingerId id, ref FingerData finger, bool isLeft)
		{
			JointType[] jointTypes = { };
			switch (id)
			{
				case FingerId.Thumb: jointTypes = s_ThumbJoints; break;
				case FingerId.Index: jointTypes = s_IndexJoints; break;
				case FingerId.Middle: jointTypes = s_MiddleJoints; break;
				case FingerId.Ring: jointTypes = s_RingJoints; break;
				case FingerId.Pinky: jointTypes = s_PinkyJoints; break;
				default: return;
			}
			for (int i = 0; i < jointTypes.Length; i++)
			{
				Vector3 parentVel = i == 0 ? Vector3.zero : finger.joints[i - 1].velocity;
				JointData lastJoint = finger.joints[i];
				GetJointData(jointTypes[i], ref finger.joints[i], isLeft);

				//As the velocity of child node should not be lower than the parent node.
				//Add the current parent node's velocity multiplied by time to the last position of child node, obtaining the new simulated position.
				if (parentVel.magnitude > finger.joints[i].velocity.magnitude)
				{
					lastJoint.position += parentVel * Time.deltaTime;
					finger.joints[i] = lastJoint;
				}
			}

			// Since the thumb does not have joint3, it is replaced by joint2.
			if (id == FingerId.Thumb)
			{
				finger.joints[(int)JointId.Tip] = finger.joint3;
				finger.direction = finger.tip.position - finger.joint2.position;
			}
			else
			{
				finger.direction = finger.tip.position - finger.joint3.position;
			}
		}

		/// <summary>
		/// Update the data for the left or right hand.
		/// </summary>
		/// <param name="isLeft">True if the hand is left; otherwise, false.</param>
		private static void UpdateData(bool isLeft)
		{
			if (!AllowUpdateData(isLeft)) { return; }

			HandData hand = hands[isLeft ? (Int32)HandId.Left : (Int32)HandId.Right];
			if (hand == null)
			{
				hand = new HandData();
				hands[isLeft ? (Int32)HandId.Left : (Int32)HandId.Right] = hand;
			}

			hand.valid = DataWrapper.Validate();
			if (!hand.valid)
			{
				hand.isTracked = false;
				return;
			}
			hand.isTracked = DataWrapper.IsHandTracked(isLeft);

			// Palm
			GetJointData(JointType.Palm, ref hand.palm, isLeft);
			hand.direction = hand.palm.rotation * Vector3.up;
			hand.normal = hand.palm.rotation * Vector3.forward;
			// Wrist
			GetJointData(JointType.Wrist, ref hand.wrist, isLeft);
			// Thumb
			GetFingerData(FingerId.Thumb, ref hand.fingers[(Int32)FingerId.Thumb], isLeft);
			// Index
			GetFingerData(FingerId.Index, ref hand.fingers[(Int32)FingerId.Index], isLeft);
			// Middle
			GetFingerData(FingerId.Middle, ref hand.fingers[(Int32)FingerId.Middle], isLeft);
			// Ring
			GetFingerData(FingerId.Ring, ref hand.fingers[(Int32)FingerId.Ring], isLeft);
			// Pinky
			GetFingerData(FingerId.Pinky, ref hand.fingers[(Int32)FingerId.Pinky], isLeft);
		}

		/// <summary>
		/// Get the complete data for the left or right hand.
		/// </summary>
		/// <param name="isLeft">True if the hand is left; otherwise, false.</param>
		/// <returns>The hand data for the specified hand.</returns>
		public static HandData Get(bool isLeft)
		{
			UpdateData(isLeft);
			return hands[isLeft ? (Int32)HandId.Left : (Int32)HandId.Right];
		}
	}
	#endregion

	/// <summary>
	/// The abstract class is designed to defines the necessary members or functions for grabbing.
	/// </summary>
	public abstract class GrabState
	{
		public abstract void UpdateState();
	}

	/// <summary>
	/// The class is designed to record the grab state of a hand.
	/// </summary>
	public class HandGrabState : GrabState
	{
		/// <summary>
		/// The class is designed to record the pinch state of each finger.
		/// </summary>
		private class FingerPinchState
		{
			private struct FingerPinchThreshold
			{
				public float pinchOn; // pinch on when distance < threshold.
				public float pinchOff; // pinch off when distance > threshold.
				public float pinchOffset; // pinch off if distance > mini distance + offset threshold.

				public FingerPinchThreshold(float in_PinchOn, float in_PinchOff, float in_PinchOffset)
				{
					pinchOn = in_PinchOn;
					pinchOff = in_PinchOff;
					pinchOffset = in_PinchOffset;
				}

				public FingerPinchThreshold Identity => new FingerPinchThreshold(0f, 0f, 0f);

				public void Update(float in_PinchOn, float in_PinchOff, float in_PinchOffset)
				{
					pinchOn = in_PinchOn;
					pinchOff = in_PinchOff;
					pinchOffset = in_PinchOffset;
				}

				public void Reset()
				{
					this = Identity;
				}
			}

			const string LOG_TAG = "VIVE.OpenXR.Toolkits.RealisticHandInteraction.HandGrabState.FingerPinchState ";
			StringBuilder m_sb = null;
			StringBuilder sb {
				get {
					if (m_sb == null) { m_sb = new StringBuilder(); }
					return m_sb;
				}
			}
			void DEBUG(StringBuilder msg) { Debug.Log(msg); }

			private bool isLeft = false;
			private FingerData thumbData;
			private FingerData fingerData;

			#region Public States
			private FingerId m_finger = FingerId.Invalid; // should not be Thumb
			public FingerId finger { get { return m_finger; } }

			private bool m_isPinching = false;
			public bool isPinching { get { return m_isPinching; } }

			[Range(0, 1)]
			private float m_pinchStrength = 0;
			public float pinchStrength { get { return m_pinchStrength; } }

			private bool m_isChanged = false;
			public bool isChanged { get { return m_isChanged; } }
			#endregion

			private readonly Dictionary<FingerId, FingerPinchThreshold> fingersPinchThreshold = new Dictionary<FingerId, FingerPinchThreshold>()
			{
				{
					FingerId.Index,
					new FingerPinchThreshold(0.02f, 0.08f, 0.045f)
				},
				{
					FingerId.Middle,
					new FingerPinchThreshold(0.02f, 0.10f, 0.055f)
				},
				{
					FingerId.Ring,
					new FingerPinchThreshold(0.02f, 0.10f, 0.055f)
				},
				{
					FingerId.Pinky,
					new FingerPinchThreshold(0.035f, 0.10f, 0.065f)
				},
			};
			private const float kPinchStrengthOnThreshold = 0.7f;
			private const float kPinchStrengthOffThreshold = 0.3f;
			private float minDistance = float.PositiveInfinity;

			public FingerPinchState(bool in_isLeft, FingerId in_finger)
			{
				isLeft = in_isLeft;
				m_finger = in_finger;
			}

			/// <summary>
			/// Checks if the finger is valid.
			/// </summary>
			/// <returns>True if the finger ID is valid.</returns>
			private bool Validate()
			{
				return (m_finger != FingerId.Invalid && m_finger != FingerId.Count && m_finger != FingerId.Thumb);
			}

			/// <summary>
			/// Calculates the shortest distance from the finger, including tip, joint3, and joint2, to the thumb, including tip, joint2, and joint1.
			/// </summary>
			/// <returns>The value for the shortest distance from the finger to the thumb.</returns>
			private float GetFingerToThumbDistance()
			{
				if (!Validate()) { return float.PositiveInfinity; }

				Vector3 thumbTip = thumbData.tip.position;
				Vector3 thumbJoint2 = thumbData.joint2.position;
				Vector3 thumbJoint1 = thumbData.joint1.position;
				Vector3[] fingerPos = { fingerData.tip.position,
										fingerData.joint3.position,
										fingerData.joint2.position};

				float distance = float.PositiveInfinity;
				foreach (var fingerJointPos in fingerPos)
				{
					distance = Mathf.Min(distance, CalculateShortestDistance(fingerJointPos, thumbTip, thumbJoint2));
					distance = Mathf.Min(distance, CalculateShortestDistance(fingerJointPos, thumbJoint2, thumbJoint1));
				}
				return distance;
			}

			/// <summary>
			/// Calculates the shortest distance from a point to a line segment defined by two points.
			/// </summary>
			/// <param name="point">The point from which the distance is measured.</param>
			/// <param name="start">The start point of the line segment.</param>
			/// <param name="end">The end point of the line segment.</param>
			/// <returns>The shortest distance from the point to the line segment.</returns>
			private float CalculateShortestDistance(Vector3 point, Vector3 start, Vector3 end)
			{
				Vector3 v1 = end - start;
				Vector3 v2 = point - start;

				float dot = Vector3.Dot(v1, v2);

				if (dot <= 0)
				{
					return Vector3.Distance(point, start);
				}

				float lengthSquared = v1.sqrMagnitude;

				if (dot >= lengthSquared)
				{
					return Vector3.Distance(point, end);
				}

				float distance = Vector3.Cross(v1, v2).magnitude / Mathf.Sqrt(lengthSquared);

				return distance;
			}

			/// <summary>
			/// Update the pinch states based on the calculated distance.
			/// </summary>
			/// <param name="distance">The distance from the finger to the thumb.</param>
			/// <returns>True if the states are updated; otherwise, false.</returns>
			private bool UpdateStates(float distance)
			{
				if (!Validate()) { return false; }

				// We use Clamp01 for
				// value > 1 if distance > kPinchOffThreshold
				// value < 0 if distance < kPinchOnThreshold
				FingerPinchThreshold threshold = fingersPinchThreshold[m_finger];
				m_pinchStrength = 1f - Mathf.Clamp01((distance - threshold.pinchOn) / (threshold.pinchOff - threshold.pinchOn));

				bool updated = false;
				if (!m_isPinching)
				{
					if (m_pinchStrength >= kPinchStrengthOnThreshold)
					{
						m_isPinching = true;
						minDistance = distance;

						sb.Clear().Append(LOG_TAG).Append(isLeft ? "Left " : "Right ").Append(m_finger.Name())
							.Append(" UpdateState() pinch strength: ").Append(m_pinchStrength)
							.Append(", pinch on threshold: ").Append(kPinchStrengthOnThreshold)
							.Append(", is pinching: ").Append(m_isPinching)
							.Append(", pinch distance: ").Append(minDistance);
						DEBUG(sb);

						updated = true;
					}
				}
				else
				{
					minDistance = Mathf.Min(minDistance, distance);
					if (m_pinchStrength < kPinchStrengthOffThreshold ||
						distance > minDistance + threshold.pinchOffset)
					{
						m_isPinching = false;

						sb.Clear().Append(LOG_TAG).Append(isLeft ? "Left " : "Right ").Append(m_finger.Name())
							.Append(" UpdateState() pinch strength: ").Append(m_pinchStrength)
							.Append(", pinch off threshold: ").Append(kPinchStrengthOffThreshold)
							.Append(", is pinching: ").Append(m_isPinching)
							.Append(", pinch distance: ").Append(minDistance);
						DEBUG(sb);

						updated = true;
					}
				}
				return updated;
			}

			/// <summary>
			/// Update the FingerPinchState with the data of the thumb and finger.
			/// </summary>
			/// <param name="thumb">The FingerData of the thumb.</param>
			/// <param name="finger">The FingerData of the finger.</param>
			public void Update(FingerData thumb, FingerData finger)
			{
				if (!Validate()) { return; }

				this.thumbData = thumb;
				this.fingerData = finger;
				float distance = GetFingerToThumbDistance();
				m_isChanged = UpdateStates(distance);
			}

			/// <summary>
			/// Manually updates the FingerPinchState with specified pinch parameters.
			/// </summary>
			/// <param name="isPinch">True if the finger is pinching; otherwise, false.</param>
			/// <param name="pinchStrength">The strength of the pinch.</param>
			/// <param name="stateChange">True if the state has changed; otherwise, false.</param>
			public void ManualUpdate(bool isPinch, float pinchStrength, bool stateChange)
			{
				m_isPinching = isPinch;
				m_pinchStrength = pinchStrength;
				m_isChanged = stateChange;
			}

			public bool IsUsed(FingerFlags flags)
			{
				return ((Int32)flags & (1 << (Int32)m_finger)) != 0;
			}
		}

		private static readonly FingerId[] s_FingerIds = {
			FingerId.Thumb,
			FingerId.Index,
			FingerId.Middle,
			FingerId.Ring,
			FingerId.Pinky,
		};

		private bool m_IsLeft = false;
		public bool isLeft { get { return m_IsLeft; } }

		private HandData hand { get { return CachedHand.Get(m_IsLeft); } }
		private FingerPinchState[] s_PinchStates = null;
		private FingerFlags pinchFingers = FingerFlags.None;
		private int updateFrame = -1;

		public HandGrabState(bool isLeft)
		{
			m_IsLeft = isLeft;
			s_PinchStates = new FingerPinchState[] {
				new FingerPinchState(m_IsLeft, FingerId.Thumb),
				new FingerPinchState(m_IsLeft, FingerId.Index),
				new FingerPinchState(m_IsLeft, FingerId.Middle),
				new FingerPinchState(m_IsLeft, FingerId.Ring),
				new FingerPinchState(m_IsLeft, FingerId.Pinky),
			};
		}

		/// <summary>
		/// Update all FingerPinchStates of a hand.
		/// </summary>
		public override void UpdateState()
		{
			pinchFingers = FingerFlags.None;
			if (updateFrame == Time.frameCount || s_PinchStates == null || !hand.isTracked) { return; }
			updateFrame = Time.frameCount;

			float bestPinchStrength = float.MinValue;
			int bestFingerId = -1;
			for (int i = s_PinchStates.Length - 1; i >= 0; i--)
			{
				if (s_PinchStates[i].finger != FingerId.Thumb)
				{
					s_PinchStates[i].Update(hand.fingers[0], hand.fingers[i]);
					if (s_PinchStates[i].pinchStrength > bestPinchStrength)
					{
						bestPinchStrength = s_PinchStates[i].pinchStrength;
						bestFingerId = i;
					}
				}
				else if (bestFingerId != -1)
				{
					bool lastPinching = s_PinchStates[i].isPinching;
					s_PinchStates[i].ManualUpdate(s_PinchStates[bestFingerId].isPinching,
												  s_PinchStates[bestFingerId].pinchStrength,
												  lastPinching != s_PinchStates[bestFingerId].isPinching);
				}
				if (s_PinchStates[i].isPinching)
					pinchFingers |= s_PinchStates[i].finger.Flag();
			}
		}

		/// <summary>
		/// Checks if the state of the FingerPinchState for a specific finger has changed as expected.
		/// </summary>
		/// <param name="finger">The FingerId of the finger.</param>
		/// <param name="expectState">The expected state of the finger pinch.</param>
		/// <returns>True if the state has changed as expected; otherwise, false.</returns>
		private bool FingerPinchStateIsChanged(FingerId finger, bool expectState)
		{
			if (s_PinchStates == null) { return false; }

			for (int i = 0; i < s_PinchStates.Length; i++)
			{
				if (s_PinchStates[i].finger == finger &&
					s_PinchStates[i].isPinching == expectState &&
					s_PinchStates[i].isChanged)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if the state of the FingerPinchState for each finger has changed as expected.
		/// </summary>
		/// <param name="fingerRequirement">The finger requirements for grabbing.</param>
		/// <param name="expectState">The expected state of the hand grab.</param>
		/// <returns>True if the requirement is met as expected; otherwise, false.</returns>
		public bool HandGrabStateIsChanged(FingerRequirement fingerRequirement, bool expectState)
		{
			Int32 pinchedValue = (Int32)pinchFingers;
			Int32 requiredValue = (Int32)fingerRequirement.Required();
			Int32 optionalValue = (Int32)fingerRequirement.Optional();

			// All required fingers have the expected state.
			// Checks the state change of each required finger.
			for (int i = 0; i < s_FingerIds.Length; i++)
			{
				if (fingerRequirement[s_FingerIds[i]] == GrabRequirement.Required &&
					FingerPinchStateIsChanged(s_FingerIds[i], expectState))
				{
					return true;
				}
			}

			// All required fingers have the expected state but no required finger state is changed.
			// Checks the state change of each optional finger if no required finger.
			if (requiredValue == 0)
			{
				for (int i = 0; i < s_FingerIds.Length; i++)
				{
					if (fingerRequirement[s_FingerIds[i]] == GrabRequirement.Optional &&
						FingerPinchStateIsChanged(s_FingerIds[i], expectState))
					{
						if (expectState && (pinchedValue & optionalValue) != 0)
						{
							// State changed to grabbed if any optional finger is pinching.
							return true;
						}
						if (!expectState && (pinchedValue & optionalValue) == 0)
						{
							// State changed to released if all optional fingers are unpinched.
							return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determine whether the hand is keeping grabbing based on finger requirements.
		/// </summary>
		/// <param name="fingerRequirement">The finger requirements for grabbing.</param>
		/// <returns>True if the hand is keeping grabbing; otherwise, false.</returns>
		public bool HandKeepGrabbing(FingerRequirement fingerRequirement)
		{
			Int32 pinchedValue = (Int32)pinchFingers;
			Int32 requiredValue = (Int32)fingerRequirement.Required();
			Int32 optionalValue = (Int32)fingerRequirement.Optional();

			// Keep grabbing if all required fingers are pinching.
			if (requiredValue != 0 && (pinchedValue & requiredValue) >= requiredValue) { return true; }

			// No required fingers and at least one optional finger is pinching.
			if (requiredValue == 0 && (pinchedValue & optionalValue) != 0) { return true; }

			return false;
		}

		/// <summary>
		/// Calculates the score of the current hand grab state based on FingerRequirement.
		/// </summary>
		/// <param name="requirements">The finger requirements for grabbing.</param>
		/// <returns>The score of the current hand grab state based on FingerRequirement.</returns>
		public float HandGrabNearStrength(FingerRequirement fingerRequirement)
		{
			/// Since the hand is not grabbed, the grab state depends on "all" required fingers.
			/// The hand will become grabbed from the frame of the "last" required finger become pinched.
			/// E.g. Index, Middle and Ring are required.
			/// If Index and Middle are already pinched, the hand will become grabbed when Ring become pinched.
			/// So the requiredStrength depends on the "last" required finger which has the minimum pinch strength.
			float requiredStrength = float.MaxValue;
			/// Since the hand is not grabbed, the grab state depends on "one of" the optional fingers.
			/// The hand will become grabbed from the frame of the "first" optional finger become pinched.
			/// So the optionalStrength depends on the "first" optional finger which has the maximum pinch strength.
			float optionalStrength = float.MinValue;

			for (int i = 0; i < s_FingerIds.Length; i++)
			{
				if (fingerRequirement[s_FingerIds[i]] == GrabRequirement.Required)
				{
					requiredStrength = Mathf.Min(requiredStrength, s_PinchStates[i].pinchStrength);
				}
				if (fingerRequirement[s_FingerIds[i]] == GrabRequirement.Optional)
				{
					optionalStrength = Mathf.Max(optionalStrength, s_PinchStates[i].pinchStrength);
				}
			}
			requiredStrength = requiredStrength == float.MaxValue ? 0 : requiredStrength;
			optionalStrength = optionalStrength == float.MinValue ? 0 : optionalStrength;

			/// Uses optionalStrength if no required finger.
			return (fingerRequirement.Required() == FingerFlags.None ? optionalStrength : requiredStrength);
		}

		/// <summary>
		/// Get the pose of the specified joint.
		/// </summary>
		/// <param name="jointType">The type of joint.</param>
		/// <returns>The pose of the specified joint. </returns>
		public Pose GetJointPose(JointType jointType)
		{
			return GetJointPose((int)jointType);
		}

		/// <summary>
		/// Get the pose of the specified joint.
		/// </summary>
		/// <param name="jointId">The index of joint.</param>
		/// <returns>The pose of the specified joint.</returns>
		public Pose GetJointPose(int jointId)
		{
			if (jointId >= (int)JointType.Count) { return Pose.identity; }

			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;

			hand.GetJointPosition((JointType)jointId, ref pos);
			hand.GetJointRotation((JointType)jointId, ref rot);
			return new Pose(pos, rot);
		}

		/// <summary>
		/// Get the default joint rotation for a specific HandGrabGesture and joint type.
		/// </summary>
		/// <param name="handGrabGesture">The HandGrabGesture is a gesture where a hand grabs.</param>
		/// <param name="jointId">The index of joint.</param>
		/// <returns>The default rotation of the specified joint in local coordinates.</returns>
		public Quaternion GetDefaultJointRotationInGesture(HandGrabGesture handGrabGesture, int jointId)
		{
			Quaternion rotation = Quaternion.identity;
			hand.GetDefaultJointRotationInGesture(m_IsLeft, handGrabGesture, (JointType)jointId, ref rotation);
			return rotation;
		}
	}

	#region Grab Framework
	/// <summary>
	/// The enum is designed to define the requirement level for grabbing.
	/// </summary>
	public enum GrabRequirement : Int32
	{
		Ignored = 0,
		Required = 1,
		Optional = 2,
	}

	/// <summary>
	/// The struct is designed to represent the requirement level for each finger during grabbing.
	/// </summary>
	[Serializable]
	public struct FingerRequirement
	{
		[SerializeField]
		public GrabRequirement thumb;
		[SerializeField]
		public GrabRequirement index;
		[SerializeField]
		public GrabRequirement middle;
		[SerializeField]
		public GrabRequirement ring;
		[SerializeField]
		public GrabRequirement pinky;

		public FingerRequirement(GrabRequirement in_Thumb, GrabRequirement in_Index, GrabRequirement in_Middle, GrabRequirement in_Ring, GrabRequirement in_Pinky)
		{
			thumb = in_Thumb;
			index = in_Index;
			middle = in_Middle;
			ring = in_Ring;
			pinky = in_Pinky;
		}

		public FingerRequirement Identity => new FingerRequirement(GrabRequirement.Ignored, GrabRequirement.Ignored, GrabRequirement.Ignored, GrabRequirement.Ignored, GrabRequirement.Ignored);

		public void Update(GrabRequirement in_Thumb, GrabRequirement in_Index, GrabRequirement in_Middle, GrabRequirement in_Ring, GrabRequirement in_Pinky)
		{
			thumb = in_Thumb;
			index = in_Index;
			middle = in_Middle;
			ring = in_Ring;
			pinky = in_Pinky;
		}

		public void Update(FingerId fingerID, GrabRequirement grabRequirement)
		{
			switch (fingerID)
			{
				case FingerId.Thumb: thumb = grabRequirement; break;
				case FingerId.Index: index = grabRequirement; break;
				case FingerId.Middle: middle = grabRequirement; break;
				case FingerId.Ring: ring = grabRequirement; break;
				case FingerId.Pinky: pinky = grabRequirement; break;
			}
		}

		public void Reset()
		{
			this = Identity;
		}

		/// <summary>
		/// Gets or sets the grab requirement for a specific finger.
		/// </summary>
		/// <param name="fingerID">The ID of the finger.</param>
		/// <returns>The grab requirement for the specified finger.</returns>
		public GrabRequirement this[FingerId fingerID]
		{
			get
			{
				switch (fingerID)
				{
					case FingerId.Thumb: return thumb;
					case FingerId.Index: return index;
					case FingerId.Middle: return middle;
					case FingerId.Ring: return ring;
					case FingerId.Pinky: return pinky;
				}
				return GrabRequirement.Ignored;
			}
			set
			{
				switch (fingerID)
				{
					case FingerId.Thumb: thumb = value; break;
					case FingerId.Index: index = value; break;
					case FingerId.Middle: middle = value; break;
					case FingerId.Ring: ring = value; break;
					case FingerId.Pinky: pinky = value; break;
				}
			}
		}

		public FingerFlags Required()
		{
			FingerFlags flag = FingerFlags.None;

			if (thumb == GrabRequirement.Required) { flag |= FingerFlags.Thumb; }
			if (index == GrabRequirement.Required) { flag |= FingerFlags.Index; }
			if (middle == GrabRequirement.Required) { flag |= FingerFlags.Middle; }
			if (ring == GrabRequirement.Required) { flag |= FingerFlags.Ring; }
			if (pinky == GrabRequirement.Required) { flag |= FingerFlags.Pinky; }

			return flag;
		}

		public FingerFlags Optional()
		{
			FingerFlags flag = FingerFlags.None;

			if (thumb == GrabRequirement.Optional) { flag |= FingerFlags.Thumb; }
			if (index == GrabRequirement.Optional) { flag |= FingerFlags.Index; }
			if (middle == GrabRequirement.Optional) { flag |= FingerFlags.Middle; }
			if (ring == GrabRequirement.Optional) { flag |= FingerFlags.Ring; }
			if (pinky == GrabRequirement.Optional) { flag |= FingerFlags.Pinky; }

			return flag;
		}
	}

	/// <summary>
	/// The struct is designed to represent the bending level of a finger.
	/// </summary>
	public enum FingerBendingLevel : UInt32
	{
		Level0 = 0,
		Level1 = 1,
		Level2 = 2,
		Level3 = 3,
		Level4 = 4,
		Level5 = 5,
	}

	/// <summary>
	/// The struct is designed to represent the bending level of each finger in a hand grab gesture.
	/// </summary>
	[Serializable]
	public struct HandGrabGesture
	{
		[SerializeField]
		public FingerBendingLevel thumbPose;
		[SerializeField]
		public FingerBendingLevel indexPose;
		[SerializeField]
		public FingerBendingLevel middlePose;
		[SerializeField]
		public FingerBendingLevel ringPose;
		[SerializeField]
		public FingerBendingLevel pinkyPose;

		public HandGrabGesture(FingerBendingLevel in_Thumb, FingerBendingLevel in_Index, FingerBendingLevel in_Middle, FingerBendingLevel in_Ring, FingerBendingLevel in_Pinky)
		{
			thumbPose = in_Thumb;
			indexPose = in_Index;
			middlePose = in_Middle;
			ringPose = in_Ring;
			pinkyPose = in_Pinky;
		}

		public static HandGrabGesture Identity => new HandGrabGesture(FingerBendingLevel.Level0, FingerBendingLevel.Level0, FingerBendingLevel.Level0, FingerBendingLevel.Level0, FingerBendingLevel.Level0);

		public void Update(FingerBendingLevel in_Thumb, FingerBendingLevel in_Index, FingerBendingLevel in_Middle, FingerBendingLevel in_Ring, FingerBendingLevel in_Pinky)
		{
			thumbPose = in_Thumb;
			indexPose = in_Index;
			middlePose = in_Middle;
			ringPose = in_Ring;
			pinkyPose = in_Pinky;
		}

		public void Reset()
		{
			this = Identity;
		}

		public override bool Equals(object obj)
		{
			return obj is HandGrabGesture pose &&
				   thumbPose == pose.thumbPose &&
				   indexPose == pose.indexPose &&
				   middlePose == pose.middlePose &&
				   ringPose == pose.ringPose &&
				   pinkyPose == pose.pinkyPose;
		}
		public override int GetHashCode()
		{
			return thumbPose.GetHashCode() ^ indexPose.GetHashCode() ^ middlePose.GetHashCode() ^ ringPose.GetHashCode() ^ pinkyPose.GetHashCode();
		}
		public static bool operator ==(HandGrabGesture source, HandGrabGesture target) => source.Equals(target);
		public static bool operator !=(HandGrabGesture source, HandGrabGesture target) => !(source == (target));
	}

	/// <summary>
	/// The struct is designed to record the position and rotation offsets between the grabber and grabbable objects.
	/// </summary>
	[Serializable]
	public struct GrabOffset
	{
		[SerializeField]
		public Vector3 targetPosition;
		[SerializeField]
		public Quaternion targetRotation;
		[SerializeField]
		public Vector3 position;
		[SerializeField]
		public Quaternion rotation;

		public GrabOffset(Vector3 in_TargetPosition, Quaternion in_targetRotation, Vector3 in_Position, Quaternion in_Rotation)
		{
			targetPosition = in_TargetPosition;
			targetRotation = in_targetRotation;
			position = in_Position;
			rotation = in_Rotation;
		}

		public static GrabOffset Identity => new GrabOffset(Vector3.zero, Quaternion.identity, Vector3.zero, Quaternion.identity);

		public void Update(Pose OffsetPose)
		{
			position = OffsetPose.position;
			rotation = OffsetPose.rotation;
		}

		public void Update(Vector3 pos, Quaternion rot)
		{
			position = pos;
			rotation = rot;
		}

		public void Reset()
		{
			this = Identity;
		}

		public override bool Equals(object obj)
		{
			return obj is GrabOffset grabOffset &&
				   position == grabOffset.position &&
				   rotation == grabOffset.rotation;
		}
		public override int GetHashCode()
		{
			return position.GetHashCode() ^ rotation.GetHashCode();
		}
		public static bool operator ==(GrabOffset source, GrabOffset target) => source.Equals(target);
		public static bool operator !=(GrabOffset source, GrabOffset target) => !(source == target);

	}

	/// <summary>
	/// The struct is designed to represent an indicator for grabbing.
	/// </summary>
	[Serializable]
	public struct Indicator
	{
		[SerializeField]
		public bool enableIndicator;
		[SerializeField]
		public bool autoIndicator;
		[SerializeField]
		public GameObject target;
		[SerializeField]
		public GrabOffset grabOffSet;

		public Indicator(bool in_EnableIndicator, bool in_AutoIndicator, GameObject in_Target)
		{
			enableIndicator = in_EnableIndicator;
			autoIndicator = in_AutoIndicator;
			target = in_Target;
			grabOffSet = GrabOffset.Identity;
		}

		public static Indicator Identity => new Indicator(true, true, null);

		public void Update(bool enableIndicator, bool autoIndicator, GameObject target)
		{
			this.enableIndicator = enableIndicator;
			this.autoIndicator = autoIndicator;
			this.target = target;
		}

		public void Reset()
		{
			this = Identity;
		}

		/// <summary>
		/// Enable or disable the indicator if the target of indicator is not null.
		/// </summary>
		/// <param name="enable">True to enable the indicator, false to deactivate it.</param>
		public void SetActive(bool enable)
		{
			if (target != null)
			{
				target.SetActive(enable);
			}
		}

		/// <summary>
		/// Check if there is a need to generate a new indicator.
		/// </summary>
		/// <returns>True if there is a need to generate a new indicator; otherwise, false. </returns>
		public bool NeedGenerateIndicator()
		{
			return autoIndicator || target == null;
		}

		/// <summary>
		/// Calculates the grab offset based on the reference transform.
		/// </summary>
		/// <param name="refTransform">The reference transform used for calculation.</param>
		public void CalculateGrabOffset(Transform refTransform)
		{
			if (target != null)
			{
				Vector3 pos = target.transform.position - refTransform.position;
				Quaternion rot = Quaternion.Inverse(refTransform.rotation) * target.transform.rotation;
				grabOffSet.Update(pos, rot);
			}
		}

		/// <summary>
		/// Update the position and rotation of the target GameObject based on the reference transform.
		/// </summary>
		/// <param name="refTransform">The reference transform used for updating position and rotation.</param>
		public void UpdatePositionAndRotation(Transform refTransform)
		{
			if (target != null)
			{
				target.transform.position = refTransform.position + refTransform.rotation * grabOffSet.position;
				target.transform.rotation = refTransform.rotation * grabOffSet.rotation;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is Indicator indicator &&
				   enableIndicator == indicator.enableIndicator &&
				   autoIndicator == indicator.autoIndicator &&
				   target == indicator.target &&
				   grabOffSet == indicator.grabOffSet;
		}
		public override int GetHashCode()
		{
			return enableIndicator.GetHashCode() ^ autoIndicator.GetHashCode() ^ target.GetHashCode() ^ grabOffSet.GetHashCode();
		}
		public static bool operator ==(Indicator source, Indicator target) => source.Equals(target);
		public static bool operator !=(Indicator source, Indicator target) => !(source == target);
	}

	/// <summary>
	/// The struct is designed to define the information about hand gestures when grabbing
	/// </summary>
	[Serializable]
	public struct GrabPose
	{
		[SerializeField]
		public string grabPoseName;
		[SerializeField]
		public HandGrabGesture handGrabGesture;
		[SerializeField]
		public Quaternion[] recordedGrabRotations;
		[SerializeField]
		public bool isLeft;
		[SerializeField]
		public Indicator indicator;
		[SerializeField]
		public GrabOffset grabOffset;

		public GrabPose(string in_GrabPoseName, HandGrabGesture in_GrabGesture, bool in_IsLeft)
		{
			grabPoseName = in_GrabPoseName;
			handGrabGesture = in_GrabGesture;
			recordedGrabRotations = Array.Empty<Quaternion>();
			isLeft = in_IsLeft;
			indicator = Indicator.Identity;
			grabOffset = GrabOffset.Identity;
		}

		public GrabPose(string in_GrabPoseName, Quaternion[] in_RecordedGrabRotations, bool in_IsLeft)
		{
			grabPoseName = in_GrabPoseName;
			handGrabGesture = HandGrabGesture.Identity;
			recordedGrabRotations = in_RecordedGrabRotations;
			isLeft = in_IsLeft;
			indicator = Indicator.Identity;
			grabOffset = GrabOffset.Identity;
		}

		public static GrabPose Identity => new GrabPose(string.Empty, HandGrabGesture.Identity, true);

		public void Update(string grabPoseName, HandGrabGesture grabGesture, bool isLeft)
		{
			this.grabPoseName = grabPoseName;
			this.handGrabGesture = grabGesture;
			this.isLeft = isLeft;
		}

		public void Update(string grabPoseName, Quaternion[] recordedGrabRotations, bool isLeft)
		{
			this.grabPoseName = grabPoseName;
			this.recordedGrabRotations = recordedGrabRotations;
			this.isLeft = isLeft;
		}

		public void Reset()
		{
			this = Identity;
		}

		public override bool Equals(object obj)
		{
			return obj is GrabPose grabPose &&
				   grabPoseName == grabPose.grabPoseName &&
				   handGrabGesture == grabPose.handGrabGesture &&
				   recordedGrabRotations == grabPose.recordedGrabRotations &&
				   isLeft == grabPose.isLeft &&
				   indicator == grabPose.indicator &&
				   grabOffset == grabPose.grabOffset;
		}
		public override int GetHashCode()
		{
			return grabPoseName.GetHashCode() ^ handGrabGesture.GetHashCode() ^ recordedGrabRotations.GetHashCode()
				^ isLeft.GetHashCode() ^ indicator.GetHashCode() ^ grabOffset.GetHashCode();
		}
		public static bool operator ==(GrabPose source, GrabPose target) => source.Equals(target);
		public static bool operator !=(GrabPose source, GrabPose target) => !(source == target);
	}

	public delegate void OnBeginGrab(IGrabber grabber);
	public delegate void OnEndGrab(IGrabber grabber);
	public delegate void OnBeginGrabbed(IGrabbable grabbable);
	public delegate void OnEndGrabbed(IGrabbable grabbable);

	/// <summary>
	/// Interface for objects capable of grabbing.
	/// </summary>
	public interface IGrabber
	{
		IGrabbable grabbable { get; }
		bool isGrabbing { get; }
		void AddBeginGrabListener(OnBeginGrab handler);
		void RemoveBeginGrabListener(OnBeginGrab handler);
		void AddEndGrabListener(OnEndGrab handler);
		void RemoveEndGrabListener(OnEndGrab handler);
	}

	/// <summary>
	/// Interface for hands capable of grabbing.
	/// </summary>
	public interface IHandGrabber : IGrabber
	{
		Handedness handedness { get; }
		HandGrabState handGrabState { get; }
	}

	/// <summary>
	///  Interface for objects capable of being grabbed.
	/// </summary>
	public interface IGrabbable
	{
		IGrabber grabber { get; }
		bool isGrabbed { get; }
		bool isGrabbable { get; }
		bool forceMovable { get; }
		void SetGrabber(IGrabber grabber);
		void AddBeginGrabbedListener(OnBeginGrabbed handler);
		void RemoveBeginGrabbedListener(OnBeginGrabbed handler);
		void AddEndGrabbedListener(OnEndGrabbed handler);
		void RemoveEndGrabbedListener(OnEndGrabbed handler);
	}

	/// <summary>
	/// Interface for objects capable of being grabbed by hands.
	/// </summary>
	public interface IHandGrabbable : IGrabbable
	{
		FingerRequirement fingerRequirement { get; }
	}

	/// <summary>
	/// The class is designed to serve as the Hand Grab API.
	/// </summary>
	public static class Grab
	{
		/// <summary>
		/// Checks if the hand grabber is beginning to grab grabbable object.
		/// </summary>
		/// <param name="grabber">The hand grabber capable of grabbing.</param>
		/// <param name="grabbable">The object being grabbed.</param>
		/// <returns>True if the hand is beginning to grab; otherwise, false.</returns>
		public static bool HandBeginGrab(IHandGrabber grabber, IHandGrabbable grabbable)
		{
			if (grabbable == null || grabber == null) { return false; }

			return grabber.handGrabState.HandGrabStateIsChanged(grabbable.fingerRequirement, true);
		}

		/// <summary>
		/// Check if the hand grabber is grabbing the grabbable object.
		/// </summary>
		/// <param name="grabber">The hand grabber capable of grabbing.</param>
		/// <param name="grabbable">The object being grabbed.</param>
		/// <returns>True if the hand is grabbing the grabbable object; otherwise, false.</returns>
		public static bool HandIsGrabbing(IHandGrabber grabber, IHandGrabbable grabbable)
		{
			if (grabbable == null || grabber == null) { return false; }

			return grabber.handGrabState.HandKeepGrabbing(grabbable.fingerRequirement);
		}

		/// <summary>
		/// Check if the hand grabber has just finished grabbing the grabbable.
		/// </summary>
		/// <param name="grabber">The hand grabber capable of grabbing.</param>
		/// <param name="grabbable">The object being grabbed.</param>
		/// <returns>True if the hand has finished grabbing the grabbable object; otherwise, false.</returns>
		public static bool HandDoneGrab(IHandGrabber grabber, IHandGrabbable grabbable)
		{
			if (grabbable == null || grabber == null) { return true; }

			return grabber.handGrabState.HandGrabStateIsChanged(grabbable.fingerRequirement, false);
		}

		/// <summary>
		/// Calculate the grab score between the grabber and the grabbable object.
		/// </summary>
		/// <param name="grabber">The hand grabber capable of grabbing.</param>
		/// <param name="grabbable">The object being grabbed.</param>
		/// <returns>The value representing the grab score between the grabber and the grabbable object.</returns>
		public static float CalculateHandGrabScore(IHandGrabber grabber, IHandGrabbable grabbable)
		{
			if (grabbable == null || grabber == null) { return -1; }

			return grabber.handGrabState.HandGrabNearStrength(grabbable.fingerRequirement);
		}
	}
	#endregion

	public static class HandInteractionHelper
	{
		public static string Name(this FingerId id)
		{
			if (id == FingerId.Thumb) { return "Thumb"; }
			if (id == FingerId.Index) { return "Index"; }
			if (id == FingerId.Middle) { return "Middle"; }
			if (id == FingerId.Ring) { return "Ring"; }
			if (id == FingerId.Pinky) { return "Pinky"; }

			return "";
		}

		public static FingerFlags Flag(this FingerId id)
		{
			if (id == FingerId.Thumb) { return FingerFlags.Thumb; }
			if (id == FingerId.Index) { return FingerFlags.Index; }
			if (id == FingerId.Middle) { return FingerFlags.Middle; }
			if (id == FingerId.Ring) { return FingerFlags.Ring; }
			if (id == FingerId.Pinky) { return FingerFlags.Pinky; }

			return FingerFlags.None;
		}
	}
}
