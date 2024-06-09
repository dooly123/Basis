using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Management;
[System.Serializable]
public class BasisXRManagement
{
    public XRManagerSettings xRManagerSettings;
    public XRGeneralSettings xRGeneralSettings;

    public LoaderType AttemptedDefaultLoad = LoaderType.OpenVRLoader;

    public enum LoaderType
    {
        OpenVRLoader,
        OpenXRLoader,
        SuccessButUnknown,
        Failure,
        Shutdown
    }

    // Define the event
    public event System.Action<LoaderType> CheckForPass;

    public void BeginLoad()
    {
        // Debug.Log("Begin Load of XR");
        xRGeneralSettings = XRGeneralSettings.Instance;
        BasisDeviceManagement.Instance.StartCoroutine(LoadXR(AttemptedDefaultLoad));
    }

    private IEnumerator LoadXR(LoaderType tryingToLoad)
    {
        //   Debug.Log("LoadXR Load");
        xRManagerSettings = xRGeneralSettings.Manager;

        // Initialize the XR loader
        yield return xRManagerSettings.InitializeLoader();
        //   Debug.Log("InitializeLoader");
        // Wait until the initialization is complete
        while (!xRManagerSettings.isInitializationComplete)
        {
            // Debug.Log("isInitializationComplete = false");
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("trying to load  " + tryingToLoad);
        ReorderLoaders(tryingToLoad.ToString());

        // Start the subsystems
        xRManagerSettings.StartSubsystems();
        //  Debug.Log("StartSubsystems");
        // Check the result
        LoaderType result = GetLoaderType(xRManagerSettings.activeLoader?.name);
        if (result.ToString() != tryingToLoad.ToString())
        {
            Debug.LogError("was unable to load requested " + tryingToLoad);
        }
        xRManagerSettings.StartSubsystems();
        Debug.Log("Found Loader " + result);
        CheckForPass?.Invoke(result);

        if (result == LoaderType.Failure)
        {
            //   Debug.Log("Falling Back was Failure");
            StopXR();
        }
    }
    void ReorderLoaders(string tryingToLoad)
    {
        List<XRLoader> loaders = xRManagerSettings.activeLoaders.ToList();
        XRLoader targetLoader = loaders.FirstOrDefault(loader => loader.name == tryingToLoad);

        if (targetLoader != null)
        {
            Debug.Log("Setting loader as: " + targetLoader.name);

            // Remove the target loader from its current position
            if (xRManagerSettings.TryRemoveLoader(targetLoader))
            {
                Debug.Log("Removed target loader from its current position.");
            }
            else
            {
                Debug.LogError("Failed to remove the target loader.");
            }

            // Insert the target loader at the first position
            if (xRManagerSettings.TryAddLoader(targetLoader, 0))
            {
                Debug.Log("Reordered loader complete.");
            }
            else
            {
                Debug.LogError("Failed to add the target loader to the first position.");
            }
        }
        else
        {
            Debug.Log("Loader was not found! Unable to find(" + tryingToLoad + ")");
            foreach (XRLoader loader in loaders)
            {
                Debug.Log("Available loader: " + loader.name);
            }
        }
    }
    private LoaderType GetLoaderType(string loaderName)
    {
        if (loaderName == LoaderType.OpenVRLoader.ToString()) return LoaderType.OpenVRLoader;
        if (loaderName == LoaderType.OpenXRLoader.ToString()) return LoaderType.OpenXRLoader;
        return LoaderType.SuccessButUnknown;
    }

    /// <summary>
    /// Stops XR
    /// </summary>
    public void StopXR()
    {
        if (xRManagerSettings != null)
        {
            xRManagerSettings.StopSubsystems();
            if (xRManagerSettings.isInitializationComplete)
            {
                xRManagerSettings.DeinitializeLoader();
            }
        }
        CheckForPass?.Invoke(LoaderType.Shutdown);
    }
}