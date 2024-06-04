// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;

namespace VIVE.OpenXR
{
    /// <summary>
    /// This component acts as a world space refernce point of the tracking space origin.
    /// Add this component to the root of your VR camera rig in order to let other features to find the tracking space origin and apply positional and rotational offset when needed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrackingSpaceOrigin : MonoBehaviour
    {
        public static TrackingSpaceOrigin Instance { get { return m_Instance; } }
        private static TrackingSpaceOrigin m_Instance = null;

        private void Awake()
		{
			if (m_Instance != null)
			{
                Destroy(m_Instance);
			}

            m_Instance = this;
		}
    }
}
