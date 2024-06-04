using System.Collections.Generic;
using UnityEngine;

namespace VIVE.OpenXR.Samples.RealisticHandInteraction
{
	public class ModeManager : MonoBehaviour
	{
		[SerializeField]
		private List<ModeEvent> modeEvents;

		private void OnEnable()
		{
			foreach (ModeEvent modeEvent in modeEvents)
			{
				modeEvent.AddListener(OnListenCollisionEvent);
			}
		}

		private void OnDisable()
		{
			foreach (ModeEvent modeEvent in modeEvents)
			{
				modeEvent.RemoveListener(OnListenCollisionEvent);
			}
		}

		private void OnListenCollisionEvent(ModeEvent.CollisionMode collisionMode)
		{
			switch (collisionMode)
			{
				case ModeEvent.CollisionMode.REALHAND:
					UnityEngine.SceneManagement.SceneManager.LoadScene("HandGrab_RealHand");
					break;
				case ModeEvent.CollisionMode.UNITY:
					UnityEngine.SceneManagement.SceneManager.LoadScene("HandGrab_Unity");
					break;
				case ModeEvent.CollisionMode.VIRTUALHAND:
					UnityEngine.SceneManagement.SceneManager.LoadScene("HandGrab_VirtualHand");
					break;
			}
		}
	}
}
