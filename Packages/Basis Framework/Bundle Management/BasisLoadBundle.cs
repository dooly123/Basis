using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static BasisProgressReport;

public static class BasisLoadBundle
{
    // Cache to store already loaded AssetBundles
    private static ConcurrentDictionary<string, Task<AssetBundle>> bundleCache = new ConcurrentDictionary<string, Task<AssetBundle>>();

    /// <summary>
    /// Loads an AssetBundle asynchronously from a specified file location. Uses caching to prevent reloading the same bundle.
    /// Returns the cached AssetBundle immediately if it's already loaded.
    /// </summary>
    /// <param name="EncyptedfileLocation">The file location of the AssetBundle.</param>
    /// <param name="basisBundleInformation">Information related to the AssetBundle, including hash and CRC values.</param>
    /// <returns>The loaded AssetBundle if successful, otherwise null.</returns>
    public static async Task<AssetBundle> LoadBasisBundle(string EncyptedfileLocation, BasisBundleInformation basisBundleInformation,string Password, ProgressReport ProgressReport)
    {
        // Ensure the provided file location is valid
        if (string.IsNullOrEmpty(EncyptedfileLocation))
        {
            Debug.LogError("Invalid file location provided was null or empty.");
            return null;
        }

        // Check if the bundle is already loaded and return it immediately
        if (bundleCache.TryGetValue(EncyptedfileLocation, out var cachedTask))
        {
            // If the bundle is already loaded and available, return it immediately
            if (cachedTask.IsCompletedSuccessfully && cachedTask.Result != null)
            {
                Debug.Log("AssetBundle already loaded, returning cached version.");
                return cachedTask.Result;
            }

            // If the task is still in progress, await it
            Debug.Log("AssetBundle is already being loaded, waiting for the task to complete.");
            return await cachedTask;
        }

        // Create a new task to load the AssetBundle on the main thread
        Task<AssetBundle> loadTask = LoadAssetBundleAsync(EncyptedfileLocation, basisBundleInformation, Password, ProgressReport);

        // Add the task to the cache so subsequent requests will use the same task
        if (!bundleCache.TryAdd(EncyptedfileLocation, loadTask))
        {
            // If another thread added the same task simultaneously, return the cached one
            return await bundleCache[EncyptedfileLocation];
        }

        // Return the loaded AssetBundle
        return await loadTask;
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
    /// <summary>
    /// Unloads the AssetBundle to free up memory and removes it from the cache.
    /// </summary>
    /// <param name="fileLocation">The file location used as the key in the cache.</param>
    /// <param name="unloadAllLoadedObjects">Whether to unload all loaded objects.</param>
    public static void UnloadAssetBundle(string fileLocation, bool unloadAllLoadedObjects = false)
    {
        if (bundleCache.TryRemove(fileLocation, out var bundleTask) && bundleTask.IsCompletedSuccessfully)
        {
            AssetBundle assetBundle = bundleTask.Result;
            if (assetBundle != null)
            {
                assetBundle.Unload(unloadAllLoadedObjects);
                Debug.Log("AssetBundle unloaded and removed from cache.");
            }
        }
        else
        {
            Debug.LogWarning("AssetBundle is either not loaded or already removed from cache.");
        }
    }
}