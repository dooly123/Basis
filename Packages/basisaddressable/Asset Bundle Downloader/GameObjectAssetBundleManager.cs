using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Basis.Scripts.Addressable_Driver.Loading;

public static class GameObjectAssetBundleManager
{
    // Delegate to report progress (value between 0 and 100)
    public delegate void ProgressReport(float progress);

    public static async Task<GameObject> DownloadAndLoadGameObjectAsync(string url, string assetName, string subfolderName, ProgressReport progressCallback)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, subfolderName);
        Directory.CreateDirectory(folderPath);
        string localPath = Path.Combine(folderPath, AddressableManagement.GetFileNameFromUrl(url));

        return await CheckAndLoadGameObjectBundleAsync(url, localPath, assetName, progressCallback);
    }

    private static async Task<GameObject> CheckAndLoadGameObjectBundleAsync(string url, string localPath, string assetName, ProgressReport progressCallback)
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

        return await LoadGameObjectBundleFromDiskAsync(url, localPath, assetName, progressCallback);
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

    private static async Task<GameObject> LoadGameObjectBundleFromDiskAsync(string url, string localPath, string assetName, ProgressReport progressCallback)
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

            GameObject asset = bundle.LoadAsset<GameObject>(assetName);
            if (asset != null)
            {
                progressCallback?.Invoke(100); // Set progress to 100 when done
                return UnityEngine.Object.Instantiate(asset);
            }

            Debug.LogError("Failed to load the specified GameObject from AssetBundle.");
        }
        else
        {
            Debug.LogError("Failed to load AssetBundle from disk.");
        }

        return null;
    }
}