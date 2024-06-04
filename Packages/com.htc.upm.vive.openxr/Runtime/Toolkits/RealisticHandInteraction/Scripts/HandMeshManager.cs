using System;
using UnityEngine;

namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
	/// <summary>
	/// This class is designed to manage the positions of various joint nodes in the hand model.
	/// </summary>
	public class HandMeshManager : MonoBehaviour
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.RealisticHandInteraction.HandMeshManager";
		private void DEBUG(string msg) { Debug.Log($"{LOG_TAG}, {msg}"); }
		private void WARNING(string msg) { Debug.LogWarning($"{LOG_TAG}, {msg}"); }
		private void ERROR(string msg) { Debug.LogError($"{LOG_TAG}, {msg}"); }

		private enum RootType
		{
			Palm = JointType.Palm,
			Wrist = JointType.Wrist,
		}

		[SerializeField]
		private Handedness m_Handedness;
		public bool IsLeft { get { return m_Handedness == Handedness.Left; } }
		[SerializeField]
		private HandGrabInteractor m_HandGrabber;
		public HandGrabInteractor HandGrabber { get { return m_HandGrabber; } }
		[SerializeField]
		private RootType m_RootJointType;
		public JointType RootJointType { get { return (JointType)m_RootJointType; } }
		[SerializeField]
		private Transform m_HandRootJoint;
		public Transform HandRootJoint { get { return m_HandRootJoint; } }
		[SerializeField]
		private Transform[] m_HandJoints = new Transform[(int)JointType.Count];
		public Transform[] HandJoints { get { return m_HandJoints; } }

		private const int k_JointCount = 26;
		private const int k_JointChildCount = 6;

		private void OnEnable()
		{
			if (m_HandGrabber == null)
			{
				WARNING("Not to set HandGrabInteractor so it won't stop when colliding with Immovable objects.");
			}
		}

		private void Update()
		{
			HandData hand = CachedHand.Get(IsLeft);
			if (!hand.isTracked)
			{
				return;
			}

			var parentTransform = m_HandRootJoint.parent;

			int rootId = m_RootJointType == RootType.Palm ? 0 : 1;
			Pose jointPose = GetJointPose(hand, rootId);
			m_HandJoints[rootId].rotation = parentTransform.rotation * jointPose.rotation;
			m_HandJoints[rootId].localPosition = jointPose.position;
			m_HandJoints[rootId].localRotation = jointPose.rotation;

			for (int i = 0; i < m_HandJoints.Length; i++)
			{
				if (m_HandJoints[i] == null || i == rootId) { continue; }

				jointPose = GetJointPose(hand, i);
				m_HandJoints[i].rotation = parentTransform.rotation * jointPose.rotation;
				if (m_HandGrabber != null && m_HandGrabber.isGrabbing &&
					m_HandGrabber.GetGrabPoseJointRotation(i, out Quaternion localStaticRot))
				{
					Quaternion currentRotation = m_HandJoints[i].rotation;
					Quaternion maxRotation = m_HandJoints[i].parent.rotation * localStaticRot;
					if (m_HandGrabber.IsRequiredJoint((JointType)i) ||
						OverFlex(currentRotation, maxRotation) >= 0 ||
						FlexAngle(currentRotation, maxRotation) >= 100)
					{
						m_HandJoints[i].rotation = maxRotation;
					}
				}
			}
		}

		/// <summary>
		/// Calculate whether the current rotation exceeds the maximum rotation.
		/// If the product is greater than 0, it exceeds.
		/// </summary>
		/// <param name="currentRot">Current rotation</param>
		/// <param name="maxRot">Maximum rotation</param>
		/// <returns>The return value represents the dot product between the cross product of two rotations and the -x axis direction of the current rotation.</returns>
		private float OverFlex(Quaternion currentRot, Quaternion maxRot)
		{
			Vector3 currFwd = currentRot * Vector3.forward;
			Vector3 maxFwd = maxRot * Vector3.forward;
			return Vector3.Dot(currentRot * Vector3.left, Vector3.Cross(currFwd, maxFwd));
		}

		/// <summary>
		/// Calculate the angle between the y-axis directions of two rotations.
		/// </summary>
		/// <param name="currentRot">Current rotation</param>
		/// <param name="maxRot">Maximum rotation</param>
		/// <returns>The return value represents the angle between the up directions of the two rotation</returns>
		private float FlexAngle(Quaternion currentRot, Quaternion maxRot)
		{
			Vector3 currFwd = currentRot * Vector3.up;
			Vector3 maxFwd = maxRot * Vector3.up;
			return Mathf.Acos(Vector3.Dot(currFwd, maxFwd) / (currFwd.magnitude * maxFwd.magnitude)) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// Get the pose of the joint based on the joint ID.
		/// </summary>
		/// <param name="hand">The current result of hand tracking.</param>
		/// <param name="jointId">ID of the specified joint.</param>
		/// <returns>Return the pose of the specified joint.</returns>
		private Pose GetJointPose(HandData hand, int jointId)
		{
			if (m_HandGrabber != null)
			{
				return m_HandGrabber.GetCurrentJointPose(jointId);
			}

			Vector3 jointPosition = Vector3.zero;
			Quaternion jointRotation = Quaternion.identity;
			hand.GetJointPosition((JointType)jointId, ref jointPosition);
			hand.GetJointRotation((JointType)jointId, ref jointRotation);
			return new Pose(jointPosition, jointRotation);
		}

		/// <summary>
		/// Get the transform of the joint.
		/// </summary>
		/// <param name="joint">JointType of the specified joint.</param>
		/// <param name="jointTransform">Output the transform of the joint.</param>
		/// <returns>Return true if successfully get the transform, otherwise false.</returns>
		public bool GetJointTransform(JointType joint, out Transform jointTransform)
		{
			jointTransform = null;
			int id = (int)joint;
			if (id >= 0 && id < m_HandJoints.Length)
			{
				if (m_HandJoints[id] != null)
				{
					jointTransform = m_HandJoints[id];
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Set all joints through gesture.
		/// </summary>
		/// <param name="handGrabGesture">The gesture of grabbing.</param>
		/// <returns>Return true if successfully set the gesture, otherwise false.</returns>
		public bool SetJointsFromGrabGesture(HandGrabGesture handGrabGesture)
		{
			if (m_HandGrabber == null) { return false; }

			for (int i = 0; i < m_HandJoints.Length; i++)
			{
				m_HandJoints[i].localRotation = m_HandGrabber.handGrabState.GetDefaultJointRotationInGesture(handGrabGesture, i);
			}
			return true;
		}

#if UNITY_EDITOR

		public void FindJoints()
		{
			if (m_HandRootJoint != null)
			{
				int fingerJoint = (int)JointType.Thumb_Joint0;
				if (m_HandRootJoint.childCount == k_JointChildCount)
				{
					for (int i = 0; i < m_HandRootJoint.childCount; i++)
					{
						Transform child = m_HandRootJoint.GetChild(i);
						switch (child.childCount)
						{
							case 0:
								if (m_RootJointType == RootType.Palm)
								{
									m_HandJoints[(int)JointType.Palm] = m_HandRootJoint;
									m_HandJoints[(int)JointType.Wrist] = child;
								}
								else
								{
									m_HandJoints[(int)JointType.Palm] = child;
									m_HandJoints[(int)JointType.Wrist] = m_HandRootJoint;
								}
								break;
							case 4:
							case 5:
								for (int j = 0; j < child.childCount; j++)
								{
									m_HandJoints[fingerJoint] = child.GetChild(j);
									fingerJoint++;
								}
								break;
							default:
								fingerJoint = RecursiveFind(fingerJoint, child);
								break;
						}
					}
				}
				else if (m_HandRootJoint.childCount == k_JointCount - 1)
				{
					Transform child = m_HandRootJoint.GetChild(0);
					if (m_RootJointType == RootType.Palm)
					{
						m_HandJoints[(int)JointType.Palm] = m_HandRootJoint;
						m_HandJoints[(int)JointType.Wrist] = child;
					}
					else
					{
						m_HandJoints[(int)JointType.Palm] = child;
						m_HandJoints[(int)JointType.Wrist] = m_HandRootJoint;
					}
					for (int i = 1; i < m_HandRootJoint.childCount; i++)
					{
						m_HandJoints[i + 1] = m_HandRootJoint.GetChild(i);
					}
				}
			}
		}

		private int RecursiveFind(int jointId, Transform transform)
		{
			m_HandJoints[jointId] = transform;
			jointId++;
			if (transform.childCount > 0)
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					jointId = RecursiveFind(jointId, transform.GetChild(i));
				}
			}
			return jointId;
		}

		public void ClearJoints()
		{
			Array.Clear(m_HandJoints, 0, m_HandJoints.Length);
		}
#endif
	}
}
