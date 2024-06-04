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
	/// This class is designed to implement IHandGrabber, allowing objects to grab grabbable objects.
	/// </summary>
	public class HandGrabInteractor : MonoBehaviour, IHandGrabber
	{
		private enum GrabState
		{
			None,
			Hover,
			Grabbing,
		};

		#region Public States 
		private HandGrabInteractable m_Grabbable = null;
		public IGrabbable grabbable => m_Grabbable;
		public bool isGrabbing => m_Grabbable != null;

		[SerializeField]
		private Handedness m_Handedness = Handedness.Left;
		public Handedness handedness => m_Handedness;

		private HandGrabState m_HandGrabState = null;
		public HandGrabState handGrabState => m_HandGrabState;

		[SerializeField]
		private float m_GrabDistance = 0.03f;
		public float grabDistance { get { return m_GrabDistance; } set { m_GrabDistance = value; } }

		[SerializeField]
		private bool m_EnableCollider = true;
		public bool enableCollider
		{
			get { return m_EnableCollider; }
			set
			{
				m_EnableCollider = value;
				if (colliderManager != null)
				{
					colliderManager.EnableCollider(m_EnableCollider);
				}
			}
		}

		public bool isLeft => handedness == Handedness.Left;
		#endregion

		[SerializeField]
		private GrabColliderManager colliderManager;

		private readonly float MinGrabScore = 0.25f;
		private readonly float MinDistanceScore = 0.25f;
		private HandGrabInteractable currentCaidate = null;
		private GrabPose grabPose = GrabPose.Identity;
		private GrabState m_State = GrabState.None;
		private Pose[] fingerTipPoses => new Pose[]
		{
			m_HandGrabState.GetJointPose(JointType.Thumb_Tip),
			m_HandGrabState.GetJointPose(JointType.Index_Tip),
			m_HandGrabState.GetJointPose(JointType.Middle_Tip),
			m_HandGrabState.GetJointPose(JointType.Ring_Tip),
			m_HandGrabState.GetJointPose(JointType.Pinky_Tip),
		};
		private Pose palmPose => m_HandGrabState.GetJointPose(JointType.Palm);
		private Pose[] frozenPoses = new Pose[(int)JointType.Count];
		private bool isFrozen = false;
		private OnBeginGrab beginGrabHandler;
		private OnEndGrab endGrabHandler;

		#region MonoBehaviour
		private void Awake()
		{
			m_HandGrabState = new HandGrabState(isLeft);
		}

		private void Start()
		{
			if (colliderManager != null)
			{
				colliderManager.EnableCollider(m_EnableCollider);
			}
		}

		private void OnEnable()
		{
			GrabManager.RegisterGrabber(this);
			if (colliderManager != null)
			{
				colliderManager.AddImmovableCollisionListener(FreezeHandPose);
			}
		}

		private void OnDisable()
		{
			GrabManager.UnregisterGrabber(this);
			if (colliderManager != null)
			{
				colliderManager.RemoveImmovableCollisionListener(FreezeHandPose);
			}
		}

		private void Update()
		{
			m_HandGrabState.UpdateState();
			if (isFrozen)
			{
				return;
			}

			if (m_State != GrabState.Grabbing)
			{
				FindCandidate();
			}

			switch (m_State)
			{
				case GrabState.None:
					NoneUpdate();
					break;
				case GrabState.Hover:
					HoverUpdate();
					break;
				case GrabState.Grabbing:
					GrabbingUpdate();
					break;
			}
		}
		#endregion

		#region Public Interface
		/// <summary>
		/// Get the current joint pose of the grabber.
		/// </summary>
		/// <param name="jointId">The id of the joint for which to get the pose.</param>
		/// <returns>The current pose of the specified joint.</returns>
		public Pose GetCurrentJointPose(int jointId)
		{
			if (isFrozen)
			{
				return frozenPoses[jointId];
			}
			return m_HandGrabState.GetJointPose(jointId);
		}

		/// <summary>
		/// Get the rotation of the joint in the grab pose.
		/// </summary>
		/// <param name="jointId">The id of the joint for which to get the rotation.</param>
		/// <returns>The rotation of the joint in the grab pose.</returns>
		public bool GetGrabPoseJointRotation(int jointId, out Quaternion rotation)
		{
			rotation = Quaternion.identity;
			if (m_Grabbable == null) { return false; }
			if (jointId >= 0 && grabPose.recordedGrabRotations.Length > jointId)
			{
				rotation = grabPose.recordedGrabRotations[jointId];
				return true;
			}
			else if (grabPose.handGrabGesture != HandGrabGesture.Identity)
			{
				rotation = m_HandGrabState.GetDefaultJointRotationInGesture(grabPose.handGrabGesture, jointId);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Check if the specific joint is necessary for grabbing.
		/// </summary>
		/// <param name="joint">JointType of the specified joint.</param>
		/// <returns>Return true if this joint is needed for grabbing, otherwise false.</returns>
		public bool IsRequiredJoint(JointType joint)
		{
			if (m_Grabbable != null)
			{
				GetJointIndex(joint, out int group, out _);
				switch (group)
				{
					case 2: return m_Grabbable.fingerRequirement.thumb == GrabRequirement.Required;
					case 3: return m_Grabbable.fingerRequirement.index == GrabRequirement.Required;
					case 4: return m_Grabbable.fingerRequirement.middle == GrabRequirement.Required;
					case 5: return m_Grabbable.fingerRequirement.ring == GrabRequirement.Required;
					case 6: return m_Grabbable.fingerRequirement.pinky == GrabRequirement.Required;
				}
			}
			return false;
		}

		/// <summary>
		/// Add a listener for the event triggered when the grabber begins grabbing.
		/// </summary>
		/// <param name="handler">The method to be called when the grabber begins grabbing.</param>
		public void AddBeginGrabListener(OnBeginGrab handler)
		{
			beginGrabHandler += handler;
		}

		/// <summary>
		/// Remove a listener for the event triggered when the grabber begins grabbing.
		/// </summary>
		/// <param name="handler">The method to be removed from the event listeners.</param>
		public void RemoveBeginGrabListener(OnBeginGrab handler)
		{
			beginGrabHandler -= handler;
		}

		/// <summary>
		/// Add a listener for the event triggered when the grabber ends grabbing.
		/// </summary>
		/// <param name="handler">The method to be called when the grabber ends grabbing.</param>
		public void AddEndGrabListener(OnEndGrab handler)
		{
			endGrabHandler += handler;
		}

		/// <summary>
		/// Remove a listener for the event triggered when the grabber ends grabbing.
		/// </summary>
		/// <param name="handler">The method to be removed from the event listeners.</param>
		public void RemoveEndGrabListener(OnEndGrab handler)
		{
			endGrabHandler -= handler;
		}
		#endregion

		/// <summary>
		/// Find the candidate grabbable object for grabber.
		/// </summary>
		private void FindCandidate()
		{
			float distanceScore = float.MinValue;
			if (GetClosestGrabbable(m_GrabDistance, out HandGrabInteractable grabbable, out float score) && score > distanceScore)
			{
				distanceScore = score;
				currentCaidate = grabbable;
			}

			if (currentCaidate != null)
			{
				float grabScore = Grab.CalculateHandGrabScore(this, currentCaidate);
				if (distanceScore < MinDistanceScore || grabScore < MinGrabScore)
				{
					currentCaidate = null;
				}
			}
		}

		/// <summary>
		/// Get the closest grabbable object for grabber.
		/// </summary>
		/// <param name="grabDistance">The maximum grab distance between the grabber and the grabbable object.</param>
		/// <param name="grabbable">The closest grabbable object.</param>
		/// <param name="maxScore">The maximum score indicating the closeness of the grabbable object.</param>
		/// <returns>True if a grabbable object is found within the grab distance; otherwise, false.</returns>
		private bool GetClosestGrabbable(float grabDistance, out HandGrabInteractable grabbable, out float maxScore)
		{
			grabbable = null;
			maxScore = 0f;
			foreach (HandGrabInteractable interactable in GrabManager.handGrabbables)
			{
				interactable.ShowIndicator(false, this);
				foreach (Pose fingerTipPose in fingerTipPoses)
				{
					float distanceScore = interactable.CalculateDistanceScore(fingerTipPose.position, grabDistance);
					if (distanceScore > maxScore)
					{
						maxScore = distanceScore;
						grabbable = interactable;
					}
				}
			}
			if (grabbable != null)
			{
				grabbable.ShowIndicator(true, this);
			}
			return grabbable != null;
		}

		/// <summary>
		/// Set the state to GrabState.Hover if a candidate is found.
		/// </summary>
		private void NoneUpdate()
		{
			if (currentCaidate != null)
			{
				m_State = GrabState.Hover;
			}
		}

		/// <summary>
		/// Update the state and related information when the grabber begins grabbing the grabbable.
		/// </summary>
		private void HoverUpdate()
		{
			if (currentCaidate == null)
			{
				m_State = GrabState.None;
				return;
			}

			if (Grab.HandBeginGrab(this, currentCaidate))
			{
				m_State = GrabState.Grabbing;

				m_Grabbable = currentCaidate;
				m_Grabbable.SetGrabber(this);
				m_Grabbable.ShowIndicator(false, this);
				grabPose = m_Grabbable.bestGrabPose;
				if (grabPose == GrabPose.Identity)
				{
					Vector3 posOffset = m_Grabbable.transform.position - palmPose.position;
					Quaternion rotOffset = palmPose.rotation;
					grabPose.grabOffset = new GrabOffset(m_Grabbable.transform.position, m_Grabbable.transform.rotation, posOffset, rotOffset);
				}
				beginGrabHandler?.Invoke(this);
			}
		}

		/// <summary>
		/// Update the position of grabbable object according to the movement of the grabber.
		/// </summary>
		private void GrabbingUpdate()
		{
			if (Grab.HandDoneGrab(this, m_Grabbable) || !Grab.HandIsGrabbing(this, m_Grabbable))
			{
				m_Grabbable.SetGrabber(null);
				m_Grabbable = null;
				m_State = GrabState.Hover;
				endGrabHandler?.Invoke(this);
				return;
			}

			Quaternion handRotOffset = palmPose.rotation * Quaternion.Inverse(grabPose.grabOffset.rotation);
			Vector3 currentPos = palmPose.position + handRotOffset * grabPose.grabOffset.position;
			Quaternion currentRot = handRotOffset * grabPose.grabOffset.targetRotation;
			m_Grabbable.transform.SetPositionAndRotation(currentPos, currentRot);
		}

		/// <summary>
		/// Freezes or unfreezes the hand pose.
		/// </summary>
		/// <param name="enable">True to freeze the hand pose; False to unfreeze.</param>
		private void FreezeHandPose(bool enable)
		{
			isFrozen = enable;
			if (isFrozen)
			{
				for (int i = 0; i < frozenPoses.Length; i++)
				{
					frozenPoses[i] = m_HandGrabState.GetJointPose(i);
				}
			}
		}

		/// <summary>
		/// Get the position of a specific joint.
		/// </summary>
		/// <param name="joint">The type of joint to get.</param>
		/// <param name="position">The reference to store the position of the joint.</param>
		/// <returns>True if the joint position is successfully retrieved; otherwise, false.</returns>
		private void GetJointIndex(JointType joint, out int group, out int index)
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
}
