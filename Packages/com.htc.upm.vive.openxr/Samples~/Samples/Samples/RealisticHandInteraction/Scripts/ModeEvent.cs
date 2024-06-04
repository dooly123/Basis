using UnityEngine;

namespace VIVE.OpenXR.Samples.RealisticHandInteraction
{
	public class ModeEvent : MonoBehaviour
	{
		public enum CollisionMode
		{
			REALHAND,
			UNITY,
			VIRTUALHAND
		}

		[SerializeField]
		private CollisionMode collisionMode;

		public delegate void CollisionHandler(CollisionMode collisionMode);
		private CollisionHandler m_CollisionHandler;

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
			if (m_CollisionHandler != null)
			{
				m_CollisionHandler.Invoke(collisionMode);
			}
		}
	}
}
