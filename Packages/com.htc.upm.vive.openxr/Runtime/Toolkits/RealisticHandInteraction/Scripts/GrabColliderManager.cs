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
using System.Linq;
using UnityEngine;

namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
	public class GrabColliderManager : MonoBehaviour
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.RealisticHandInteraction.GrabColliderManager";
		private void DEBUG(string msg) { Debug.Log($"{LOG_TAG}, {msg}"); }
		private void WARNING(string msg) { Debug.LogWarning($"{LOG_TAG}, {msg}"); }
		private void ERROR(string msg) { Debug.LogError($"{LOG_TAG}, {msg}"); }

		/// <summary>
		/// The struct is designed to record movable colliders, 
		/// including which grabbable they belong to, which joints collisioned with, and whether they have been grabbed.
		/// </summary>
		private struct MovableHitInfo
		{
			public struct JointHitInfo
			{
				public JointType joint;
				public Vector3 hitOffset;
				public int hitTime { get; private set; }

				public JointHitInfo(JointType in_JointType, Vector3 in_HitOffset)
				{
					joint = in_JointType;
					hitOffset = in_HitOffset;
					hitTime = Time.frameCount;
				}

				public void Update()
				{
					hitTime = Time.frameCount;
				}
			}

			public HandGrabInteractable grabbable;
			public List<JointHitInfo> jointHitInfos;
			public bool grabbed;
			public bool stopMove;

			public MovableHitInfo(HandGrabInteractable in_Grabbable, JointType in_Joint, Vector3 in_Offset)
			{
				grabbable = in_Grabbable;
				jointHitInfos = new List<JointHitInfo>()
				{
					new JointHitInfo(in_Joint, in_Offset)
				};
				grabbed = false;
				stopMove = false;
			}

			public void Update(bool in_Grabbed, bool in_StopMove)
			{
				grabbed = in_Grabbed;
				stopMove = in_StopMove;
			}

			public void Reset()
			{
				grabbed = false;
				stopMove = false;
			}

			/// <summary>
			/// Add a JointType. If it's already in the dictionary, update the time.
			/// </summary>
			/// <param name="joint">The joint which needs to be added.</param>
			public void AddJoint(JointType joint, Vector3 offset)
			{
				int hitId = jointHitInfos.FindIndex(x => x.joint == joint);
				if (hitId == -1)
				{
					jointHitInfos.Add(new JointHitInfo(joint, offset));
				}
				else
				{
					JointHitInfo jointHitInfo = jointHitInfos[hitId];
					jointHitInfo.Update();
					jointHitInfos[hitId] = jointHitInfo;
				}
			}

			/// <summary>
			/// Remove a JointType and check if it has been grabbed.
			/// </summary>
			/// <param name="joint">The joint which needs to be removed.</param>
			public void RemoveJoint(JointType joint)
			{
				int hitId = jointHitInfos.FindIndex(x => x.joint == joint);
				if (hitId != -1)
				{
					jointHitInfos.RemoveAt(hitId);
				}
			}
		}

		[SerializeField]
		private HandMeshManager jointManager;

		private GrabCollider[] jointsCollider = new GrabCollider[(int)JointType.Count];
		private Pose[] jointsPrevFramePose = new Pose[(int)JointType.Count];
		private bool isImmovableCollision = false;
		private bool isGrabbing = false;
		private List<MovableHitInfo> movableHits = new List<MovableHitInfo>();
		private Dictionary<GrabCollider, Collider> immovableHits = new Dictionary<GrabCollider, Collider>();
		public delegate void OnImmovableCollision(bool enable);
		private OnImmovableCollision immovableCollisionHandler;

		private void Awake()
		{
			if (jointManager == null)
			{
				jointManager = transform.GetComponent<HandMeshManager>();
				if (jointManager == null)
				{
					ERROR("Failed to find HandJointManager.");
				}
			}
		}

		private void OnEnable()
		{
			if (jointManager != null)
			{
				jointManager.HandGrabber.AddBeginGrabListener(OnGrabberBeginGrab);
				jointManager.HandGrabber.AddEndGrabListener(OnGrabberEndGrab);
			}

			CreateJointsCollider();
		}

		private void OnDisable()
		{
			if (jointManager != null)
			{
				jointManager.HandGrabber.RemoveBeginGrabListener(OnGrabberBeginGrab);
				jointManager.HandGrabber.RemoveEndGrabListener(OnGrabberEndGrab);
			}

			foreach (var collider in jointsCollider)
			{
				collider.RemoveListener(CollisionEvent);
				Destroy(collider);
			}
			Array.Clear(jointsCollider, 0, jointsCollider.Length);
		}

		private void Update()
		{
			if (jointManager == null) {  return; }

			UpdateColliderPose();
			if (!isGrabbing)
			{
				UpdateImmovable();
				UpdateMovable();
			}

			for (int i = 0; i < jointsCollider.Length; i++)
			{
				jointsPrevFramePose[i] = jointsCollider[i] == null ? Pose.identity : new Pose(jointsCollider[i].transform.position, jointsCollider[i].transform.rotation);
			}
		}

		/// <summary>
		/// Create colliders for each joint and set them do not collide with each other.
		/// </summary>
		private void CreateJointsCollider()
		{
			if (jointManager != null)
			{
				var cloneRoot = Instantiate(jointManager.HandRootJoint, jointManager.HandRootJoint.parent);
				cloneRoot.name = jointManager.HandRootJoint.name;
				List<GameObject> children = new List<GameObject>() { cloneRoot.gameObject };
				GetChildren(cloneRoot, children);

				foreach (var child in children)
				{
					Transform target = jointManager.HandJoints.FirstOrDefault(x => x.name == child.name);
					if (target != null)
					{
						int index = Array.IndexOf(jointManager.HandJoints, target);

						GrabCollider grabCollider = child.AddComponent<GrabCollider>();
						grabCollider.AddListener(CollisionEvent);
						grabCollider.SetJointId(index);
						jointsCollider[index] = grabCollider;
					}
				}
			}

			for (int i = 0; i < jointsCollider.Length; i++)
			{
				if (jointsCollider[i] == null)
				{
					continue;
				}

				for (int j = i + 1; j < jointsCollider.Length; j++)
				{
					if (jointsCollider[j] != null)
					{
						Physics.IgnoreCollision(jointsCollider[i].Collider, jointsCollider[j].Collider, true);
					}
				}
			}
		}

		private void GetChildren(Transform parent, List<GameObject> children)
		{
			foreach (Transform child in parent)
			{
				children.Add(child.gameObject);
				GetChildren(child, children);
			}
		}

		/// <summary>
		/// Update the position of the collider using the position of the joint.
		/// </summary>
		private void UpdateColliderPose()
		{
			HandData hand = CachedHand.Get(jointManager.IsLeft);
			bool isTracked = hand.isTracked;
			if (!isTracked)
			{
				return;
			}

			var parentTransform = jointManager.HandRootJoint.parent;
			var parentRotation = Matrix4x4.Rotate(parentTransform.rotation);
			Vector3 jointPosition = Vector3.zero;
			Quaternion jointRotation = Quaternion.identity;
			for (int i = 0; i < jointsCollider.Length; i++)
			{
				if (jointsCollider[i] == null) { continue; }

				hand.GetJointPosition((JointType)i, ref jointPosition);
				hand.GetJointRotation((JointType)i, ref jointRotation);

				if ((JointType)i == JointType.Wrist)
				{
					jointsCollider[i].transform.localPosition = jointPosition;
					jointsCollider[i].transform.localRotation = jointRotation;
				}
				jointsCollider[i].transform.rotation = (parentRotation * Matrix4x4.Rotate(jointRotation)).rotation;
			}
		}

		/// <summary>
		/// Save the hand pose if a collision has already occurred with a joint.
		/// </summary>
		private void UpdateImmovable()
		{
			bool isCollision = jointsCollider.Any(x => x != null && x.IsCollision);
			foreach (var jointCollider in jointsCollider)
			{
				jointCollider.Collider.enabled = isCollision ? jointCollider.IsCollision : true;
			}

			if (isImmovableCollision != isCollision)
			{
				isImmovableCollision = isCollision;
				immovableCollisionHandler?.Invoke(isImmovableCollision);
			}
		}

		/// <summary>
		/// Check all movableHits and move the object relative to the movement of the collisioned joint.
		/// </summary>
		private void UpdateMovable()
		{
			if (isImmovableCollision) { return; }

			const int k_MinCollisionTimeDiff = 5;
			const int k_MaxCollisionTimeDiff = 50;

			for (int i = movableHits.Count - 1; i >= 0; i--)
			{
				MovableHitInfo hit = movableHits[i];
				if (hit.stopMove) { continue; }

				Vector3 totalPosition = Vector3.zero;
				Vector3 totalOffset = Vector3.zero;
				int validCount = 0;
				for (int j = hit.jointHitInfos.Count - 1; j >= 0; j--)
				{
					MovableHitInfo.JointHitInfo jointHit = hit.jointHitInfos[j];
					int frameCountDiff = Time.frameCount - jointHit.hitTime;
					if (frameCountDiff > k_MinCollisionTimeDiff)
					{
						if (frameCountDiff > k_MaxCollisionTimeDiff)
						{
							hit.RemoveJoint(jointHit.joint);
						}
						continue;
					}

					int jointId = (int)jointHit.joint;
					Vector3 currentPose = jointsCollider[jointId].transform.position;
					Vector3 prevPose = jointsPrevFramePose[jointId].position;

					// Condition 1: Calculate the displacement between consecutive frames of joints, it should greater than 1E-6f as significant.
					// Condition 2: Calculate distance score relative to grabbable; the score of current pose should be greater than the previous pose.
					// Condition 3: The dot product of the vector between the current pose and the grabbable object,
					// and the vector representing finger movement direction should be less than 0.
					if (Vector3.Distance(prevPose, currentPose) > 1E-6f &&
						movableHits[i].grabbable.CalculateDistanceScore(currentPose) >= movableHits[i].grabbable.CalculateDistanceScore(prevPose) &&
						Vector3.Dot((currentPose - prevPose).normalized, (movableHits[i].grabbable.transform.position - prevPose).normalized) > 0)
					{
						validCount++;
						totalPosition += currentPose;
						totalOffset += jointHit.hitOffset;
					}
				}

				if (validCount > 0)
				{
					movableHits[i].grabbable.transform.position = (totalPosition - totalOffset) / validCount;
				}

				if (hit.jointHitInfos.Count == 0)
				{
					movableHits.RemoveAt(i);
				}
				else
				{
					movableHits[i] = hit;
				}
			}
		}

		/// <summary>
		/// Enable or disable the collider of joints.
		/// </summary>
		/// <param name="enable">Enable (true) or disable (false) the colliders.</param>
		public void EnableCollider(bool enable)
		{
			for (int i = 0; i < jointsCollider.Length; i++)
			{
				if (jointsCollider[i] != null)
				{
					jointsCollider[i].gameObject.SetActive(enable);
				}
			}
		}

		#region Collision Event
		/// <summary>
		/// Adds a listener for immovable collision events.
		/// </summary>
		/// <param name="handler">The method to be called when an immovable collision occurs.</param>
		public void AddImmovableCollisionListener(OnImmovableCollision handler)
		{
			immovableCollisionHandler += handler;
		}

		/// <summary>
		/// Removes a listener for immovable collision events.
		/// </summary>
		/// <param name="handler">The method to be removed from the immovable collision event listeners.</param>
		public void RemoveImmovableCollisionListener(OnImmovableCollision handler)
		{
			immovableCollisionHandler -= handler;
		}

		/// <summary>
		/// Event handler for when the grabber begins grabbing.
		/// </summary>
		/// <param name="grabber">The grabber of IGrabber.</param>
		private void OnGrabberBeginGrab(IGrabber grabber)
		{
			isGrabbing = true;
			for (int i = 0; i < movableHits.Count; i++)
			{
				if (grabber.grabbable is HandGrabInteractable &&
					(HandGrabInteractable)grabber.grabbable == movableHits[i].grabbable)
				{
					MovableHitInfo movableHit = movableHits[i];
					movableHit.Update(true, true);
					movableHits[i] = movableHit;
				}
			}
		}

		private void OnGrabberEndGrab(IGrabber grabber)
		{
			isGrabbing = false;
		}

		/// <summary>
		/// Filter all collision events, check for grabbables, and update collision data.
		/// </summary>
		/// <param name="joint">The joint which has been collision.</param>
		/// <param name="collision">The data of Collision.</param>
		/// <param name="isColliding">True when the collision event is OnCollisionEnter or OnCollisionStay.</param>
		private void CollisionEvent(JointType joint, Collision collision, GrabCollider.CollisionState state)
		{
			bool isCollision = state != GrabCollider.CollisionState.end;
			Rigidbody rigidbody = collision.rigidbody;
			GrabManager.GetFirstHandGrabbableFromParent(collision.collider.gameObject, out HandGrabInteractable grabbable);
			if (collision.rigidbody == null && (grabbable == null || grabbable != null && !grabbable.enabled)) { return; }

			if ((rigidbody == null || rigidbody.isKinematic) && grabbable != null && grabbable.forceMovable)
			{
				if (isCollision)
				{
					UpdateMovableHits(joint, grabbable);
				}
				else
				{
					RemoveMovableHits(joint, grabbable);
				}
			}
			else if ((rigidbody != null && rigidbody.isKinematic) || (grabbable != null && !grabbable.forceMovable))
			{
				UpdateImmovableHIts(joint, collision.collider, isCollision);
			}

		}

		private void UpdateMovableHits(JointType joint, HandGrabInteractable grabbable)
		{
			int index = movableHits.FindIndex(x => x.grabbable == grabbable);
			if (index != -1)
			{
				MovableHitInfo moveable = movableHits[index];
				moveable.AddJoint(joint, jointsCollider[(int)joint].transform.position - grabbable.transform.position);
				movableHits[index] = moveable;
			}
			else
			{
				MovableHitInfo moveable = new MovableHitInfo(grabbable, joint, jointsCollider[(int)joint].transform.position - grabbable.transform.position);
				movableHits.Add(moveable);
			}
		}

		private void RemoveMovableHits(JointType joint, HandGrabInteractable grabbable)
		{
			int index = movableHits.FindIndex(x => x.grabbable == grabbable);
			if (index != -1)
			{
				MovableHitInfo movable = movableHits[index];
				movable.RemoveJoint(joint);
				if (movable.jointHitInfos.Count == 0)
				{
					movableHits.Remove(movable);
				}
				else
				{
					movableHits[index] = movable;
				}
			}
		}

		private void UpdateImmovableHIts(JointType joint, Collider collider, bool isCollision)
		{
			GrabCollider grabCollider = jointsCollider[(int)joint];
			grabCollider.IsCollision = isCollision;

			if (isCollision && !immovableHits.ContainsKey(grabCollider))
			{
				immovableHits.Add(grabCollider, collider);
			}
			else if (!isCollision && immovableHits.ContainsKey(grabCollider))
			{
				immovableHits.Remove(grabCollider);
			}
		}
		#endregion
	}
}
