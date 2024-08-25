using Basis.Scripts.Addressable_Driver.Loading;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static GameObjectAssetBundleManager;

public static class SceneAssetBundleManager
{
    public static async Task DownloadAndLoadSceneAsync(string url, string sceneName, string subfolderName, ProgressReport progressCallback)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, subfolderName);
        Directory.CreateDirectory(folderPath);
        string localPath = Path.Combine(folderPath, GetFileNameFromUrl(url));

        await CheckAndLoadSceneBundleAsync(url, localPath, sceneName, progressCallback);
    }

    private static string GetFileNameFromUrl(string url)
    {
        Uri uri = new Uri(url);
        return Path.GetFileName(uri.LocalPath);
    }

    private static async Task CheckAndLoadSceneBundleAsync(string url, string localPath, string sceneName, ProgressReport progressCallback)
    {
        if (File.Exists(localPath))
        {
            Debug.Log("Local AssetBundle is up-to-date, loading from disk.");
            await LoadSceneBundleFromDiskAsync(url, localPath, sceneName, progressCallback);
        }
        else
        {
            Debug.Log("AssetBundle not found locally, downloading.");
            await DownloadAssetBundleAsync(url, localPath, progressCallback);
            await LoadSceneBundleFromDiskAsync(url, localPath, sceneName, progressCallback);
        }
    }

    private static async Task DownloadAssetBundleAsync(string url, string localPath, ProgressReport progressCallback)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        var asyncOperation = request.SendWebRequest();

        // Track download progress
        while (!asyncOperation.isDone)
        {
            progressCallback?.Invoke(asyncOperation.progress * 50); // Progress from 0 to 50 during download
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download AssetBundle: " + request.error);
            return;
        }

        File.WriteAllBytes(localPath, request.downloadHandler.data);
        Debug.Log("AssetBundle saved to: " + localPath);
    }

    private static async Task LoadSceneBundleFromDiskAsync(string url, string localPath, string sceneName, ProgressReport progressCallback)
    {
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(localPath);

        // Track loading progress
        while (!bundleRequest.isDone)
        {
            progressCallback?.Invoke(50 + bundleRequest.progress * 50); // Progress from 50 to 100 during loading
            await Task.Yield();
        }

        AssetBundle bundle = bundleRequest.assetBundle;

        if (bundle != null)
        {
            BasisLoadedAssets BasisLoadedAssets = new BasisLoadedAssets
            {
                LoadedAssetBundle = bundle,
                Url = url
            };
            AddressableManagement.Instance.LoadedBundles.Add(BasisLoadedAssets);
            Debug.Log("AssetBundle loaded successfully from disk.");

            await LoadSceneFromAssetBundleAsync(bundle, sceneName, progressCallback);
        }
        else
        {
            Debug.LogError("Failed to load AssetBundle from disk.");
        }
    }

    private static async Task LoadSceneFromAssetBundleAsync(AssetBundle bundle, string sceneName, ProgressReport progressCallback)
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