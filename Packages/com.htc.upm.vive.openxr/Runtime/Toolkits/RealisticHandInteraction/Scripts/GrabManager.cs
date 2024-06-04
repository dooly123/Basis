using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
	/// <summary>
	/// This class is designed to manage all Grabbers and Grabbables.
	/// </summary>
	public static class GrabManager
	{
		private static List<IGrabber> m_GrabberRegistry = new List<IGrabber>();
		public static IReadOnlyList<HandGrabInteractor> handGrabbers => m_GrabberRegistry.OfType<HandGrabInteractor>().ToList().AsReadOnly();

		private static List<IGrabbable> m_GrabbableRegistry = new List<IGrabbable>();
		public static IReadOnlyList<HandGrabInteractable> handGrabbables => m_GrabbableRegistry.OfType<HandGrabInteractable>().ToList().AsReadOnly();

		#region IGrabber
		/// <summary>
		/// Register the grabber in the grabber registry.
		/// </summary>
		/// <param name="grabber">The grabber to register.</param>
		/// <returns>True if the grabber is successfully registered; otherwise, false.</returns>
		public static bool RegisterGrabber(IGrabber grabber)
		{
			if (!m_GrabberRegistry.Contains(grabber))
			{
				m_GrabberRegistry.Add(grabber);
			}
			return m_GrabberRegistry.Contains(grabber);
		}

		/// <summary>
		/// Remove the grabber from the grabber registry.
		/// </summary>
		/// <param name="grabber">The grabber to remove.</param>
		/// <returns>True if the grabber is successfully removed; otherwise, false.</returns>
		public static bool UnregisterGrabber(IGrabber grabber)
		{
			if (m_GrabberRegistry.Contains(grabber))
			{
				m_GrabberRegistry.Remove(grabber);
			}
			return !m_GrabberRegistry.Contains(grabber);
		}

		/// <summary>
		/// Get the first hand grabber component found in the child hierarchy of the GameObject.
		/// </summary>
		/// <param name="target">The target whose child hierarchy to search.</param>
		/// <param name="grabber">The output parameter to store the first hand grabber component found.</param>
		/// <returns>True if a hand grabber component is found; otherwise, false.</returns>
		public static bool GetFirstHandGrabberFromChild(GameObject target, out HandGrabInteractor grabber)
		{
			grabber = TopDownFind<HandGrabInteractor>(target.transform);
			return grabber != null;
		}

		/// <summary>
		/// Get the first hand grabber component found in the parent hierarchy of the GameObject.
		/// </summary>
		/// <param name="target">The target whose parent hierarchy to search.</param>
		/// <param name="grabber">The output parameter to store the first hand grabber component found.</param>
		/// <returns>True if a hand grabber component is found; otherwise, false.</returns>
		public static bool GetFirstHandGrabberFromParent(GameObject target, out HandGrabInteractor grabber)
		{
			grabber = BottomUpFind<HandGrabInteractor>(target.transform);
			return grabber != null;
		}
		#endregion

		#region GrabInteractable
		/// <summary>
		/// Register the grabbable in the grabbable registry.
		/// </summary>
		/// <param name="grabbable">The grabbable to register.</param>
		/// <returns>True if the grabbable is successfully registered; otherwise, false.</returns>
		public static bool RegisterGrabbable(IGrabbable grabbable)
		{
			if (!m_GrabbableRegistry.Contains(grabbable))
			{
				m_GrabbableRegistry.Add(grabbable);
			}
			return m_GrabbableRegistry.Contains(grabbable);
		}

		/// <summary>
		/// Remove the grabbable from the grabbable registry.
		/// </summary>
		/// <param name="grabbable">The grabbable to remove.</param>
		/// <returns>True if the grabbable is successfully removed; otherwise, false.</returns>
		public static bool UnregisterGrabbable(IGrabbable grabbable)
		{
			if (m_GrabbableRegistry.Contains(grabbable))
			{
				m_GrabbableRegistry.Remove(grabbable);
			}
			return !m_GrabbableRegistry.Contains(grabbable);
		}

		/// <summary>
		/// Get the first hand grabbable component found in the child hierarchy of the GameObject.
		/// </summary>
		/// <param name="target">The target whose child hierarchy to search.</param>
		/// <param name="grabbable">The output parameter to store the first hand grabbable component found.</param>
		/// <returns>True if a hand grabbable component is found; otherwise, false.</returns>
		public static bool GetFirstHandGrabbableFromChild(GameObject target, out HandGrabInteractable grabbable)
		{
			grabbable = TopDownFind<HandGrabInteractable>(target.transform);
			return grabbable != null;
		}

		/// <summary>
		/// Get the first hand grabbable component found in the parent hierarchy of the GameObject.
		/// </summary>
		/// <param name="target">The target whose parent hierarchy to search.</param>
		/// <param name="grabbable">The output parameter to store the first hand grabbable component found.</param>
		/// <returns>True if a hand grabbable component is found; otherwise, false.</returns>
		public static bool GetFirstHandGrabbableFromParent(GameObject target, out HandGrabInteractable grabbable)
		{
			grabbable = BottomUpFind<HandGrabInteractable>(target.transform);
			return grabbable != null;
		}
		#endregion

		/// <summary>
		/// Find available components from self to children nodes.
		/// </summary>
		/// <param name="transform">The transform of the gameobject.</param>
		/// <returns>Value for available component.</returns>
		private static T TopDownFind<T>(Transform transform) where T : Component
		{
			T component = transform.GetComponent<T>();
			if (component != null)
			{
				return component;
			}

			if (transform.childCount > 0)
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					T childComponent = TopDownFind<T>(transform.GetChild(i));
					if (childComponent != null)
					{
						return childComponent;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Find available components from self to parent node.
		/// </summary>
		/// <param name="transform">The transform of the gameobject.</param>
		/// <returns>Value for available component.</returns>
		private static T BottomUpFind<T>(Transform transform) where T : Component
		{
			T component = transform.GetComponent<T>();
			if (component != null)
			{
				return component;
			}

			if (transform.parent != null)
			{
				return BottomUpFind<T>(transform.parent);
			}
			return null;
		}
	}
}
