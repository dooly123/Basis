using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VIVE.OpenXR.Samples.RealisticHandInteraction
{
	public class BackEvent : MonoBehaviour
	{
		private DateTime lastEventTime = DateTime.MinValue;
		private int intervalTime = 1;

		private void OnEnable()
		{
			lastEventTime = DateTime.Now;
		}

		private void OnCollisionEnter(Collision collision)
		{
			TimeSpan timeSinceLastEvent = DateTime.Now - lastEventTime;
			if (timeSinceLastEvent.TotalSeconds > intervalTime)
			{
				lastEventTime = DateTime.Now;
				SceneManager.LoadScene("HandGrab_Main");
			}
		}

		public void OnClick()
		{
			TimeSpan timeSinceLastEvent = DateTime.Now - lastEventTime;
			if (timeSinceLastEvent.TotalSeconds > intervalTime)
			{
				lastEventTime = DateTime.Now;
				SceneManager.LoadScene("HandGrab_Main");
			}
		}
	}
}
