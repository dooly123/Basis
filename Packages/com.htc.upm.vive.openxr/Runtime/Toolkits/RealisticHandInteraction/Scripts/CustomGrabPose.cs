// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;

namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
	/// <summary>
	/// This class is designed to edit grab gestures.
	/// </summary>
	[RequireComponent(typeof(HandMeshManager))]
	public class CustomGrabPose : MonoBehaviour
	{
	#if UNITY_EDITOR
		[SerializeField]
		private HandGrabGesture m_GrabGesture;

		private HandMeshManager jointManager;
		private Transform palmTransform;
		private HandGrabGesture currentGesture;
		private HandGrabInteractable candidate = null;
		private readonly float k_GrabDistance = 0.1f;

		private Pose[] fingerTipPoses => new Pose[5];

		private void OnEnable()
		{
			if (jointManager == null)
			{
				jointManager = transform.GetComponent<HandMeshManager>();
			}
		}

		private void Update()
		{
			// Non-DirectPreview mode.
			if (m_GrabGesture != currentGesture)
			{
				currentGesture = m_GrabGesture;
				jointManager.SetJointsFromGrabGesture(currentGesture);
			}
		}

		/// <summary>
		/// Finds the nearest interactable object to the hand.
		/// </summary>
		public void FindNearInteractable()
		{
			if (jointManager.GetJointTransform(JointType.Palm, out palmTransform))
			{
				float maxScore = 0;
				foreach (HandGrabInteractable interactable in GrabManager.handGrabbables)
				{
					float distanceScore = interactable.CalculateDistanceScore(palmTransform.position, k_GrabDistance);
					if (distanceScore > maxScore)
					{
						maxScore = distanceScore;
						candidate = interactable;
					}
				}
			}
		}

		/// <summary>
		/// Save the position and rotation offset with the candidate.
		/// </summary>
		public void SavePoseWithCandidate()
		{
			if (candidate != null &&
				jointManager.GetJointTransform(JointType.Palm, out palmTransform))
			{
				Vector3 posOffset = candidate.transform.position - palmTransform.position;
				Quaternion rotOffset = palmTransform.rotation;
				GrabPose grabPose = GrabPose.Identity;
				grabPose.Update($"Grab Pose {candidate.grabPoses.Count + 1}", currentGesture, jointManager.IsLeft); 
				grabPose.grabOffset = new GrabOffset(candidate.transform.position, candidate.transform.rotation, posOffset, rotOffset);
				if (!candidate.grabPoses.Contains(grabPose))
				{
					candidate.grabPoses.Add(grabPose);
				}
				GrabbablePoseRecorder.SaveChanges();
			}
		}
	#endif
	}
}
