using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Basis.Scripts.Addressable_Driver.Loading;
public static class GameObjectAssetBundleManager
{
    public static async Task<GameObject> DownloadAndLoadGameObjectAsync(string url, string assetName, string subfolderName)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, subfolderName);
        Directory.CreateDirectory(folderPath);
        string localPath = Path.Combine(folderPath, AddressableManagement.GetFileNameFromUrl(url));

        return await CheckAndLoadGameObjectBundleAsync(url, localPath, assetName);
    }
    private static async Task<GameObject> CheckAndLoadGameObjectBundleAsync(string url, string localPath, string assetName)
    {
        if (File.Exists(localPath))
        {
         //   File.Delete(localPath);
            Debug.Log("Local AssetBundle is up-to-date, loading from disk.");
        }
        else
        {
            Debug.Log("AssetBundle not found locally, downloading.");
            await DownloadAssetBundleAsync(url, localPath);
        }

        return await LoadGameObjectBundleFromDiskAsync(url, localPath, assetName);
    }
    private static async Task DownloadAssetBundleAsync(string url, string localPath)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        var asyncOperation = request.SendWebRequest();
        while (!asyncOperation.isDone)
        {
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
    private static async Task<GameObject> LoadGameObjectBundleFromDiskAsync(string url, string localPath, string assetName)
    {
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(localPath);
        while (!bundleRequest.isDone)
        {
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