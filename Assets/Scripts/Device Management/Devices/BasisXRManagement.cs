using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Basis.Scripts.Device_Management.Devices
{
    [System.Serializable]
    public class BasisXRManagement
    {
        public XRManagerSettings xRManagerSettings;
        public XRGeneralSettings xRGeneralSettings;
        // Define the event
        public event System.Action<BasisBootedMode> CheckForPass;

        // Store the initial list of loaders
        [SerializeField]
        public List<XRLoader> initialLoaders = new List<XRLoader>();
        public void ReInitalizeCheck()
        {
            if (XRGeneralSettings.Instance != null)
            {
                xRGeneralSettings = XRGeneralSettings.Instance;
                if (xRGeneralSettings.Manager != null)
                {
                    xRManagerSettings = xRGeneralSettings.Manager;
                }
            }
        }
        public void BeginLoad()
        {
            Debug.Log("Starting LoadXR");
            // Debug.Log("Begin Load of XR");
            ReInitalizeCheck();
            BasisDeviceManagement.Instance.StartCoroutine(LoadXR());
        }
        public void DisableDeviceManagerSolution(BasisBootedMode BasisBootedMode)
        {
            ReInitalizeCheck();
            IReadOnlyList<XRLoader> Loaders = xRManagerSettings.activeLoaders;
            foreach (XRLoader loader in Loaders)
            {
                if (loader?.name == BasisBootedMode.ToString())
                {
                    xRManagerSettings.TryRemoveLoader(loader);
                    return;
                }
            }
        }
        public IEnumerator LoadXR()
        {
            // Initialize the XR loader
            yield return xRManagerSettings.InitializeLoader();
            BasisBootedMode result = BasisBootedMode.Desktop;
            // Check the result
            if (xRManagerSettings.activeLoader != null)
            {
                xRManagerSettings.StartSubsystems();
                result = GetLoaderType(xRManagerSettings.activeLoader?.name);
            }
            Debug.Log("Found Loader " + result);
            CheckForPass?.Invoke(result);
        }
        public BasisBootedMode GetLoaderType(string loaderName)
        {
            if (loaderName == BasisBootedMode.OpenVRLoader.ToString()) return BasisBootedMode.OpenVRLoader;
            if (loaderName == BasisBootedMode.OpenXRLoader.ToString()) return BasisBootedMode.OpenXRLoader;
            return BasisBootedMode.SuccessButUnknown;
        }

        public void StopXR(bool IsExiting)
        {
            if (xRManagerSettings != null)
            {
                if (xRManagerSettings.isInitializationComplete)
                {
                    xRManagerSettings.DeinitializeLoader();
                }
            }
            if (IsExiting)
            {
                CheckForPass?.Invoke(BasisBootedMode.Exiting);
            }
            else
            {
                CheckForPass?.Invoke(BasisBootedMode.Desktop);
            }
        }
    }
}