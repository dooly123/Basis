// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEngine;

namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
	/// <summary>
	/// This class is designed to implement IHandGrabbable, allowing objects to be grabbed.
	/// </summary>
	public class HandGrabInteractable : MonoBehaviour, IHandGrabbable
	{
		#region Interface Implement
		private HandGrabInteractor m_Grabber = null;
		public IGrabber grabber => m_Grabber;

		public bool isGrabbed => m_Grabber != null;

		[SerializeField]
		private bool m_IsGrabbable = true;
		public bool isGrabbable { get { return m_IsGrabbable; } set { m_IsGrabbable = value; } }

		[SerializeField]
		private bool m_ForceMovable = true;
		public bool forceMovable { get { return m_ForceMovable; } set { m_ForceMovable = value; } }

		[SerializeField]
		private FingerRequirement m_FingerRequirement;
		public FingerRequirement fingerRequirement => m_FingerRequirement;
		#endregion

		#region Public State
		[SerializeField]
		private List<GrabPose> m_GrabPoses = new List<GrabPose>();
		public List<GrabPose> grabPoses => m_GrabPoses;

		public GrabPose bestGrabPose => bestGrabPoseId != -1 ? m_GrabPoses[bestGrabPoseId] : GrabPose.Identity;
		#endregion

		[SerializeField]
		private bool m_ShowAllIndicator = false;
		private List<Collider> allColliders = new List<Collider>();
		private HandGrabInteractor closestGrabber = null;
		private int bestGrabPoseId = -1;
		private OnBeginGrabbed onBeginGrabbed;
		private OnEndGrabbed onEndGrabbed;

		#region MonoBehaviour
		private void Awake()
		{
			allColliders.AddRange(transform.GetComponentsInChildren<Collider>(true));
		}

		private void OnEnable()
		{
			GrabManager.RegisterGrabbable(this);
			Initialize();
		}

		private void OnDisable()
		{
			GrabManager.UnregisterGrabbable(this);
		}
		#endregion

		#region Public Interface
		/// <summary>
		/// Set the grabber for the hand grabbable object.
		/// </summary>
		/// <param name="grabber">The grabber to set.</param>
		public void SetGrabber(IGrabber grabber)
		{
			if (grabber is HandGrabInteractor)
			{
				HandGrabInteractor handGrabber = grabber as HandGrabInteractor;
				m_Grabber = handGrabber;
				UpdateBestGrabPose(handGrabber.isLeft, handGrabber.handGrabState.GetJointPose(JointType.Palm));
				onBeginGrabbed?.Invoke(this);
			}
			else
			{
				m_Grabber = null;
				bestGrabPoseId = -1;
				onEndGrabbed?.Invoke(this);
			}
		}

		/// <summary>
		/// Enable/Disable indicators. If enabled, display the closest indicator based on grabber position.
		/// </summary>
		/// <param name="enable">True to show the indicator, false to hide it.</param>
		/// <param name="grabber">The grabber for which to show or hide this indicator.</param>
		public void ShowIndicator(bool enable, HandGrabInteractor grabber)
		{
			if (enable)
			{
				closestGrabber = grabber;
				if (m_ShowAllIndicator)
				{
					ShowAllIndicator(grabber.isLeft);
				}
				else
				{
					int index = FindBestGrabPose(grabber.isLeft, grabber.handGrabState.GetJointPose((int)JointType.Palm));
					ShowIndicatorByIndex(index);
				}
			}
			else
			{
				if (closestGrabber == grabber)
				{
					closestGrabber = null;
					ShowIndicatorByIndex(-1);
				}
			}
		}

		/// <summary>
		/// Calculate the shortest distance between the grabber and the grabbable and convert it into a score based on grabDistance.
		/// </summary>
		/// <param name="grabberPos">The current pose of grabber.</param>
		/// <param name="grabDistance">The maximum grab distance between the grabber and the grabbable object.</param>
		/// <returns>The score represents the distance between the grabber and the grabbable.</returns>
		public float CalculateDistanceScore(Vector3 grabberPos, float grabDistance = 0.03f)
		{
			if (!isGrabbable || isGrabbed) { return 0; }
			Vector3 closestPoint = GetClosestPoint(grabberPos);
			float distacne = Vector3.Distance(grabberPos, closestPoint);
			return distacne > grabDistance ? 0 : 1 - (distacne / grabDistance);
		}

		/// <summary>
		/// Add a listener for the event triggered when the grabbable object is grabbed.
		/// </summary>
		/// <param name="handler">The method to be called when the grabbable object is grabbed.</param>
		public void AddBeginGrabbedListener(OnBeginGrabbed handler)
		{
			onBeginGrabbed += handler;
		}

		/// <summary>
		/// Remove a listener for the event triggered when the grabbable object is grabbed.
		/// </summary>
		/// <param name="handler">The method to be removed from the event listeners.</param>
		public void RemoveBeginGrabbedListener(OnBeginGrabbed handler)
		{
			onBeginGrabbed -= handler;
		}

		/// <summary>
		/// Add a listener for the event triggered when the grabbable object is released.
		/// </summary>
		/// <param name="handler">The method to be called when the grabbable object is released.</param>
		public void AddEndGrabbedListener(OnEndGrabbed handler)
		{
			onEndGrabbed += handler;
		}

		/// <summary>
		/// Remove a listener for the event triggered when the grabbable object is released.
		/// </summary>
		/// <param name="handler">The method to be removed from the event listeners.</param>
		public void RemoveEndGrabbedListener(OnEndGrabbed handler)
		{
			onEndGrabbed -= handler;
		}
		#endregion

		/// <summary>
		/// Generate all indicators and calculate grab offsets.
		/// </summary>
		private void Initialize()
		{
			for (int i = 0; i < m_GrabPoses.Count; i++)
			{
				if (m_GrabPoses[i].indicator.enableIndicator || m_ShowAllIndicator)
				{
					if (m_GrabPoses[i].indicator.NeedGenerateIndicator())
					{
						AutoGenerateIndicator(i);
					}
					else
					{
						GrabPose grabPose = m_GrabPoses[i];
						grabPose.indicator.CalculateGrabOffset(transform);
						m_GrabPoses[i] = grabPose;
					}
				}
			}
			ShowIndicatorByIndex(-1);
		}

		/// <summary>
		/// Automatically generate an indicator by the index of the grab pose.
		/// </summary>
		/// <param name="index">The index of the grab pose.</param>
		private void AutoGenerateIndicator(int index)
		{
			AutoGenIndicator autoGenIndicator = new GameObject($"Indicator {index}", typeof(AutoGenIndicator)).GetComponent<AutoGenIndicator>();

			GrabPose grabPose = m_GrabPoses[index];
			// The grabPose.grabOffset was calculated as the position of the object minus the position of the hand,
			// so inverse calculation is needed here to infer the position of the hand.
			Vector3 offset = transform.rotation * Quaternion.Inverse(grabPose.grabOffset.targetRotation) * -grabPose.grabOffset.position;
			Vector3 defaultPosition = transform.position + offset;
			Vector3 closestPoint = GetClosestPoint(defaultPosition);
			autoGenIndicator.SetPose(closestPoint, offset.normalized);
			grabPose.indicator.Update(true, true, autoGenIndicator.gameObject);
			grabPose.indicator.CalculateGrabOffset(transform);
			m_GrabPoses[index] = grabPose;
		}

		/// <summary>
		/// Calculate the point closest to the source position.
		/// </summary>
		/// <param name="sourcePos">The position of source.</param>
		/// <returns>The position which closest to the source position.</returns>
		private Vector3 GetClosestPoint(Vector3 sourcePos)
		{
			Vector3 closestPoint = Vector3.zero;
			float shortDistance = float.MaxValue;
			foreach (var collider in allColliders)
			{
				Vector3 closePoint = collider.ClosestPointOnBounds(sourcePos);
				float distance = Vector3.Distance(sourcePos, closePoint);
				if (collider.bounds.Contains(closePoint))
				{
					Vector3 direction = (closePoint - sourcePos).normalized;
					RaycastHit[] hits = Physics.RaycastAll(sourcePos, direction, distance);
					foreach (var hit in hits)
					{
						if (hit.collider == collider)
						{
							float hitDistnace = Vector3.Distance(sourcePos, hit.point);
							if (distance > hitDistnace)
							{
								distance = hitDistnace;
								closePoint = hit.point;
							}
						}
					}
				}

				if (shortDistance > distance)
				{
					shortDistance = distance;
					closestPoint = closePoint;
				}
			}
			return closestPoint;
		}

		/// <summary>
		/// Find the best grab pose for the grabber and updates the bestGrabPoseId.
		/// </summary>
		/// <param name="isLeft">Whether the grabber is the left hand.</param>
		/// <param name="grabberPose">The pose of the grabber.</param>
		/// <returns>True if a best grab pose is found; otherwise, false.</returns>
		private bool UpdateBestGrabPose(bool isLeft, Pose grabberPose)
		{
			int index = FindBestGrabPose(isLeft, grabberPose);
			if (index != -1 && index < m_GrabPoses.Count)
			{
				bestGrabPoseId = index;
				return true;
			}
			index = -1;
			return false;
		}

		/// <summary>
		/// Find the best grab pose for the grabber.
		/// </summary>
		/// <param name="isLeft">Whether the grabber is the left hand.</param>
		/// <param name="grabberPose">The pose of the grabber.</param>
		/// <returns>The index of the best grab pose among the grab poses.</returns>
		private int FindBestGrabPose(bool isLeft, Pose grabberPose)
		{
			int index = -1;
			float maxDot = float.MinValue;
			Vector3 currentDirection = grabberPose.position - transform.position;
			for (int i = 0; i < m_GrabPoses.Count; i++)
			{
				if (m_GrabPoses[i].isLeft == isLeft)
				{
					Vector3 grabDirection = transform.rotation * Quaternion.Inverse(m_GrabPoses[i].grabOffset.targetRotation) * -m_GrabPoses[i].grabOffset.position;
					float dot = Vector3.Dot(currentDirection.normalized, grabDirection.normalized);
					if (dot > maxDot)
					{
						maxDot = dot;
						index = i;
					}
				}
			}
			return index;
		}

		/// <summary>
		/// Show the indicator corresponding to the specified index and hides others.
		/// </summary>
		/// <param name="index">The index of the indicator to show.</param>
		private void ShowIndicatorByIndex(int index)
		{
			foreach (var grabPose in m_GrabPoses)
			{
				grabPose.indicator.SetActive(false);
			}
			if (index >= 0 && index < m_GrabPoses.Count &&
				m_GrabPoses[index].indicator.enableIndicator)
			{
				m_GrabPoses[index].indicator.UpdatePositionAndRotation(transform);
				m_GrabPoses[index].indicator.SetActive(true);
			}
		}

		/// <summary>
		/// Show all indicators corresponding to the specified hand side and hides others.
		/// </summary>
		/// <param name="isLeft">Whether the hand side is left.</param>
		private void ShowAllIndicator(bool isLeft)
		{
			foreach (var grabPose in m_GrabPoses)
			{
				grabPose.indicator.SetActive(false);
			}
			foreach (var grabPose in m_GrabPoses)
			{
				if (grabPose.isLeft == isLeft)
				{
					grabPose.indicator.UpdatePositionAndRotation(transform);
					grabPose.indicator.SetActive(true);
				}
			}
		}
	}
}
