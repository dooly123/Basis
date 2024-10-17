using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static BasisProgressReport;   
using static BasisLoadhandler;
using UnityEngine.SceneManagement;

public static class BasisLoadBundle
{
    /// <summary>
    /// Loads an AssetBundle asynchronously from a specified file location. Uses caching to prevent reloading the same bundle.
    /// Returns the cached AssetBundle immediately if it's already loaded.
    /// </summary>
    /// <param name="EncyptedfileLocation">The file location of the AssetBundle.</param>
    /// <param name="basisBundleInformation">Information related to the AssetBundle, including hash and CRC values.</param>
    /// <returns>The loaded AssetBundle if successful, otherwise null.</returns>
    public static async Task<AssetBundle> LoadBasisBundle(BasisTrackedBundleWrapper BasisTrackedBundleWrapper, ProgressReport ProgressReport)
    {
        // Ensure the provided file location is valid
       BasisLoadableBundle Bundle = BasisTrackedBundleWrapper.LoadableBundle.Result;
        if (Bundle != null && string.IsNullOrEmpty(Bundle.BasisStoredEncyptedBundle.LocalBundleFile))
        {
            Debug.LogError("Invalid file location provided was null or empty.");
            return null;
        }
        BasisTrackedBundleWrapper.AssetBundle = LoadAssetBundleAsync(Bundle.BasisStoredEncyptedBundle.LocalBundleFile, Bundle.BasisBundleInformation, Bundle.UnlockPassword, ProgressReport);

        // Return the loaded AssetBundle
        return await BasisTrackedBundleWrapper.AssetBundle;
    }

    private static async Task<AssetBundle> LoadAssetBundleAsync(string fileLocation, BasisBundleInformation basisBundleInformation, string Password, ProgressReport ProgressReport)
    {
        try
        {
            // Load the AssetBundle from the file location asynchronously using CRC
            Task<AssetBundle> Bundle = BasisEncryptionToData.GenerateBundleFromFile(Password, fileLocation, basisBundleInformation.BasisBundleGenerated.AssetBundleCRC, ProgressReport);
            return await Bundle;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred while loading AssetBundle: {ex.Message}");
            return null;
        }
    }
    public static async Task LoadSceneFromAssetBundleAsync(AssetBundle bundle, bool MakeActiveScene, ProgressReport progressCallback)
    {
        string[] scenePaths = bundle.GetAllScenePaths();
        if (scenePaths.Length == 0)
        {
            Debug.LogError("No scenes found in AssetBundle.");
            return;
        }

        if (!string.IsNullOrEmpty(scenePaths[0]))
        {
            // Load the scene asynchronously
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePaths[0], LoadSceneMode.Additive);

            // Track scene loading progress
            while (!asyncLoad.isDone)
            {
                progressCallback?.Invoke(50 + asyncLoad.progress * 50); // Progress from 50 to 100 during scene load
                await Task.Yield();
            }

            Debug.Log("Scene loaded successfully from AssetBundle.");
            Scene loadedScene = SceneManager.GetSceneByPath(scenePaths[0]);

            // Set the loaded scene as the active scene
            if (loadedScene.IsValid())
            {
                if (MakeActiveScene)
                {
                    SceneManager.SetActiveScene(loadedScene);
                }
                Debug.Log("Scene set as active: " + loadedScene.name);
                progressCallback?.Invoke(100); // Set progress to 100 when done
            }
            else
            {
                Debug.LogError("Failed to get loaded scene.");
            }
        }
        else
        {
            Debug.LogError("Scene not found in AssetBundle.");
        }
    }
    /// <summary>
    /// pew pew
    /// </summary>
    /// <param name="SearchAndDestroy"></param>
    public static void ContentControlCondom(GameObject SearchAndDestroy)
    {
        List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();
        SearchAndDestroy.GetComponentsInChildren(true, monoBehaviours);

        int count = monoBehaviours.Count;
        for (int Index = 0; Index < count; Index++)
        {
            MonoBehaviour mono = monoBehaviours[Index];
            // Get the full name of the MonoBehaviour's type
            string monoTypeName = mono.GetType().FullName;

            // Check if the type is in the selectedTypes list
            if (BundledContentHolder.Instance.Selector.selectedTypes.Contains(monoTypeName))
            {
                // Debug.Log($"MonoBehaviour {monoTypeName} is approved.");
                // Do something if the MonoBehaviour type is approved
            }
            else
            {
                Debug.LogError($"MonoBehaviour {monoTypeName} is not approved.");
                GameObject.Destroy(mono);
                // Do something if the MonoBehaviour type is not approved
            }
        }
    }
}