using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Management;

[System.Serializable]
public class BasisXRManagement
{
    public XRManagerSettings xRManagerSettings;
    public XRGeneralSettings xRGeneralSettings;
    public XRLoader targetLoader;
    public enum LoaderType
    {
        OpenVRLoader,
        OpenXRLoader,
        SuccessButUnknown,
        NotVR
    }

    // Define the event
    public event System.Action<LoaderType> CheckForPass;

    // Store the initial list of loaders
    [SerializeField]
    public List<XRLoader> initialLoaders = new List<XRLoader>();

    public void Initalize()
    {
        // Debug.Log("Begin Load of XR");
        xRGeneralSettings = XRGeneralSettings.Instance;
        xRManagerSettings = xRGeneralSettings.Manager;
    }
    public void BeginLoad(BasisXRManagement.LoaderType loaderType)
    {
        Debug.Log("Starting LoadXR");
        BasisDeviceManagement.Instance.StartCoroutine(LoadXR(loaderType));
    }

    public IEnumerator LoadXR(LoaderType tryingToLoad)
    {
        Debug.Log("LoadXR Load");

        Debug.Log("trying to load " + tryingToLoad);
        LoadByName(tryingToLoad.ToString());

        // Initialize the XR loader
        yield return xRManagerSettings.InitializeLoader();
        LoaderType result = LoaderType.NotVR;
        // Check the result
        if (xRManagerSettings.activeLoader != null)
        {
            Debug.Log("InitializeLoader");
            // Start the subsystems
            xRManagerSettings.StartSubsystems();
            result = GetLoaderType(xRManagerSettings.activeLoader?.name);
            if (result.ToString() != tryingToLoad.ToString())
            {
                Debug.LogError("was unable to load requested " + tryingToLoad);
            }
        }
        Debug.Log("Found Loader " + result);
        CheckForPass?.Invoke(result);
    }

    public void LoadByName(string tryingToLoad)
    {
        targetLoader = initialLoaders.FirstOrDefault(loader => loader.name == tryingToLoad);
        Debug.Log("Going through Initial Loaders" + initialLoaders.Count);
        // Remove all current loaders
        foreach (XRLoader loader in initialLoaders)
        {
            if (xRManagerSettings.TryRemoveLoader(loader))
            {
                Debug.Log("Removed loader: " + loader.name);
            }
        }

        if (targetLoader != null)
        {
            if(xRManagerSettings.activeLoaders.Count > 0)
            {
                Debug.LogError("we removed the loaders but there still not zero?");
            }
            // Add the target loader at the first position
            if (xRManagerSettings.TryAddLoader(targetLoader, -1))
            {
                Debug.Log("Added target loader: " + targetLoader.name);
            }
            else
            {
                Debug.LogError("Failed to add the target loader.");
            }
        }
        else
        {
            for (int Index = 0; Index < initialLoaders.Count; Index++)
            {
                XRLoader loader = initialLoaders[Index];
                Debug.LogError("could use " + loader.name);
            }
            Debug.LogError("Loader was not found! Unable to find(" + tryingToLoad + ")");
        }
    }

    public LoaderType GetLoaderType(string loaderName)
    {
        if (loaderName == LoaderType.OpenVRLoader.ToString()) return LoaderType.OpenVRLoader;
        if (loaderName == LoaderType.OpenXRLoader.ToString()) return LoaderType.OpenXRLoader;
        return LoaderType.SuccessButUnknown;
    }

    public void StopXR()
    {
        if (xRManagerSettings != null)
        {
            xRManagerSettings.StopSubsystems();
            xRManagerSettings.DeinitializeLoader();
        }
        CheckForPass?.Invoke(LoaderType.NotVR);
    }
}
