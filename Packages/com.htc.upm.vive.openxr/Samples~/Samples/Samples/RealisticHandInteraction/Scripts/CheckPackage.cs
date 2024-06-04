using UnityEngine;
using VIVE.OpenXR.Toolkits.RealisticHandInteraction;

namespace VIVE.OpenXR.Samples.RealisticHandInteraction
{
	public class CheckPackage : MonoBehaviour
	{
		private void OnEnable()
		{
#if UNITY_XR_HANDS
			if (DataWrapper.Validate())
			{
				gameObject.SetActive(false);
			}
#endif
		}
	}
}
