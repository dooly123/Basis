using Basis.Scripts.Addressable_Driver.Loading;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Basis.Scripts.Addressable_Driver.Loading.AddressableManagement;
public static class SceneAssetBundleManager
{
    public static async Task DownloadAndLoadSceneAsync(string url, string subfolderName, ProgressReport progressCallback)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, subfolderName);
        Directory.CreateDirectory(folderPath);
        string localPath = Path.Combine(folderPath, GetFileNameFromUrl(url));

        await CheckAndLoadSceneBundleAsync(url, localPath, progressCallback);
    }
    private static async Task CheckAndLoadSceneBundleAsync(string url, string localPath, ProgressReport progressCallback)
    {
        if (File.Exists(localPath))
        {
            Debug.Log("Local AssetBundle is up-to-date, loading from disk.");
        }
        else
        {
            Debug.Log("AssetBundle not found locally, downloading.");
            await DownloadAssetBundleAsync(url, localPath, progressCallback);
        }
        await LoadSceneBundleFromDiskAsync(url, localPath, progressCallback);
    }
    private static async Task LoadSceneBundleFromDiskAsync(string url, string localPath, ProgressReport progressCallback)
    {
        BasisLoadedAssets BasisLoadedAssets = new BasisLoadedAssets();
        try
        {
            BasisLoadedAssets = await LoadBundle(url, localPath, progressCallback);
        }
        catch (Exception E)
        {
            Debug.LogError("Unable to Loadin asset bundle " + E.Message);
        }

        if (BasisLoadedAssets.Bundle != null)
        {
            await LoadSceneFromAssetBundleAsync(BasisLoadedAssets.Bundle, progressCallback);
        }
        else
        {
            Debug.LogError("Failed to load AssetBundle from disk.");
        }
    }

    private static async Task LoadSceneFromAssetBundleAsync(AssetBundle bundle, ProgressReport progressCallback)
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
                SceneManager.SetActiveScene(loadedScene);
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
}