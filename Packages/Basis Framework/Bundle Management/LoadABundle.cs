using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LoadABundle : MonoBehaviour
{
    public BasisProgressReport.ProgressReport Report;
    public CancellationToken CancellationToken= new CancellationToken();
    public BasisLoadableBundle BasisLoadableBundle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        BasisLoadableBundle = await BasisBundleManagement.DownloadAndSaveBundle(BasisLoadableBundle, Report, CancellationToken);
        BasisLoadableBundle.LoadedAssetBundle = await BasisLoadBundle.LoadBasisBundle(BasisLoadableBundle.BasisBundleInformation.BasisStoredDecyptedBundle.LocalBundleFile, BasisLoadableBundle.BasisBundleInformation);
    }
}
public static class BasisLoadBundle
{
    // Cache to store already loaded AssetBundles
    private static ConcurrentDictionary<string, Task<AssetBundle>> bundleCache = new ConcurrentDictionary<string, Task<AssetBundle>>();

    /// <summary>
    /// Loads an AssetBundle asynchronously from a specified file location. Uses caching to prevent reloading the same bundle.
    /// Returns the cached AssetBundle immediately if it's already loaded.
    /// </summary>
    /// <param name="fileLocation">The file location of the AssetBundle.</param>
    /// <param name="basisBundleInformation">Information related to the AssetBundle, including hash and CRC values.</param>
    /// <returns>The loaded AssetBundle if successful, otherwise null.</returns>
    public static async Task<AssetBundle> LoadBasisBundle(string fileLocation, BasisBundleInformation basisBundleInformation)
    {
        // Ensure the provided file location is valid
        if (string.IsNullOrEmpty(fileLocation))
        {
            Debug.LogError("Invalid file location provided was null or empty.");
            return null;
        }

        // Check if the bundle is already loaded and return it immediately
        if (bundleCache.TryGetValue(fileLocation, out var cachedTask))
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
        Task<AssetBundle> loadTask = LoadAssetBundleAsync(fileLocation, basisBundleInformation);

        // Add the task to the cache so subsequent requests will use the same task
        if (!bundleCache.TryAdd(fileLocation, loadTask))
        {
            // If another thread added the same task simultaneously, return the cached one
            return await bundleCache[fileLocation];
        }

        // Return the loaded AssetBundle
        return await loadTask;
    }

    private static async Task<AssetBundle> LoadAssetBundleAsync(string fileLocation, BasisBundleInformation basisBundleInformation)
    {
        try
        {
            // Load the AssetBundle from the file location asynchronously using CRC
            AssetBundleCreateRequest assetBundleRequest = AssetBundle.LoadFromFileAsync(fileLocation, basisBundleInformation.BasisBundleGenerated.AssetBundleCRC);

            // Wait for the AssetBundle to fully load asynchronously
            while (!assetBundleRequest.isDone)
            {
                Debug.Log($"Loading AssetBundle... {assetBundleRequest.progress * 100}% completed.");
                await Task.Yield(); // Yield control back to the main thread while waiting
            }

            // Check if loading succeeded
            if (assetBundleRequest.assetBundle != null)
            {
                Debug.Log("AssetBundle loaded successfully.");
                return assetBundleRequest.assetBundle;
            }
            else
            {
                Debug.LogError("AssetBundle loaded but is null. Possible corruption or incorrect file.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred while loading AssetBundle: {ex.Message}");
            return null;
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