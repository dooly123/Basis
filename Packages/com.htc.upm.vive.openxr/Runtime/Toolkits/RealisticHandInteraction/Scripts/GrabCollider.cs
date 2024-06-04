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
	/// This class is designed to generate appropriately sized colliders for each joint.
	/// </summary>
	public class GrabCollider : MonoBehaviour
	{
		public enum CollisionState
		{
			start = 0,
			keep = 1,
			end = 2,
		}

		private CapsuleCollider m_Collider = null;
		public Collider Collider => m_Collider;

		private bool m_IsCollision = false;
		public bool IsCollision { get { return m_IsCollision; } set { m_IsCollision = value; } }

		private const float k_ColliderRadius = 0.01f;
		private const float k_ColliderHeight = 0.01f;
		private JointType jointType = JointType.Count;

		public delegate void CollisionHandler(JointType joint, Collision collision, CollisionState state);
		private CollisionHandler m_CollisionHandler;

		private void OnEnable()
		{
			m_Collider = transform.GetComponent<CapsuleCollider>();
			if (m_Collider == null)
			{
				m_Collider = transform.gameObject.AddComponent<CapsuleCollider>();
			}
			m_Collider.radius = k_ColliderRadius;
			m_Collider.height = k_ColliderHeight;
			m_Collider.direction = 2;

			Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				rigidbody = transform.gameObject.AddComponent<Rigidbody>();
			}
			rigidbody.useGravity = false;
			rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		}

		/// <summary>
		/// Set the joint id and adjust collider size..
		/// </summary>
		/// <param name="id">JointType of joint.</param>
		public void SetJointId(int id)
		{
			jointType = (JointType)id;
			if (m_Collider)
			{
				// Adjust the size and position of the collider based on jointId.
				switch (jointType)
				{
					case JointType.Thumb_Joint0:
					case JointType.Thumb_Joint1:
						m_Collider.height = 0.03f;
						break;
					case JointType.Index_Joint0:
					case JointType.Middle_Joint0:
					case JointType.Ring_Joint0:
					case JointType.Pinky_Joint0:
						m_Collider.height = 0.08f;
						m_Collider.center = new Vector3(0f, 0f, 0.02f);
						break;
					case JointType.Index_Joint1:
					case JointType.Middle_Joint1:
					case JointType.Ring_Joint1:
					case JointType.Pinky_Joint1:
						m_Collider.height = 0.05f;
						m_Collider.center = new Vector3(0f, 0f, 0.02f);
						break;
					case JointType.Index_Tip:
					case JointType.Middle_Tip:
					case JointType.Ring_Tip:
					case JointType.Pinky_Tip:
						m_Collider.radius = 0.005f;
						break;
				}
			}
		}

		public void AddListener(CollisionHandler handler)
		{
			m_CollisionHandler += handler;
		}

		public void RemoveListener(CollisionHandler handler)
		{
			m_CollisionHandler -= handler;
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!IsGrabCollider(collision.collider) && m_CollisionHandler != null)
			{
				m_CollisionHandler.Invoke(jointType, collision, CollisionState.start);
			}
		}

		private void OnCollisionStay(Collision collision)
		{
			if (!IsGrabCollider(collision.collider) && m_CollisionHandler != null)
			{
				m_CollisionHandler.Invoke(jointType, collision, CollisionState.keep);
			}
		}

		private void OnCollisionExit(Collision collision)
		{
			if (!IsGrabCollider(collision.collider) && m_CollisionHandler != null)
			{
				m_CollisionHandler.Invoke(jointType, collision, CollisionState.end);
			}
		}

		private bool IsGrabCollider(Collider collider)
		{
			GrabCollider grabCollider = collider.gameObject.GetComponent<GrabCollider>();
			return grabCollider != null;
		}
	}
}
