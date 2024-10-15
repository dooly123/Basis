using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static Basis.Scripts.Addressable_Driver.Loading.AddressableManagement;
using static BasisProgressReport;
namespace Basis.Scripts.Addressable_Driver.Loading
{
    public class AssetBundleManagement
    {
        [SerializeField]
        public List<BasisLoadedAssets> LoadedBundles = new List<BasisLoadedAssets>();
        public enum BasisAssetStatus
        {
            False,
            True,
            HasNewHash
        }
        public string ChangeExtension(string url, string newExtension)
        {
            // Get the file path without the extension
            string urlWithoutExtension = Path.ChangeExtension(url, null);

            // Add the new extension
            return urlWithoutExtension + newExtension;
        }
        public void UnloadAssetBundle(string url)
        {
            for (int Index = 0; Index < LoadedBundles.Count; Index++)
            {
                BasisLoadedAssets asset = LoadedBundles[Index];
                if (asset.Url.Equals(url) && asset.Bundle != null)
                {
                    asset.Bundle.Unload(false);
                    LoadedBundles.RemoveAt(Index);
                    return;
                }
            }
        }
        public void OnDestroy()
        {
            for (int Index = 0; Index < LoadedBundles.Count; Index++)
            {
                BasisLoadedAssets asset = LoadedBundles[Index];
                if (asset.Bundle != null)
                {
                    asset.Bundle.Unload(false);
                }
            }
        }
        public BasisAssetStatus LoadedBundlesContains(string Url, BasisBundleInformation Hash, out BasisLoadedAssets BasisLoadedAssets)
        {
            if (string.IsNullOrEmpty(Url))
            {
                BasisLoadedAssets = new BasisLoadedAssets();
                return BasisAssetStatus.False;
            }

            for (int Index = 0; Index < LoadedBundles.Count; Index++)
            {
                BasisLoadedAssets LoadedBundle = LoadedBundles[Index];
                if (LoadedBundle.Url == Url)
                {
                    if (LoadedBundle.Hash.BasisBundleGenerated.AssetBundleHash == Hash.BasisBundleGenerated.AssetBundleHash)
                    {
                        BasisLoadedAssets = LoadedBundle;
                        return BasisAssetStatus.True;
                    }
                    else
                    {
                        BasisLoadedAssets = LoadedBundle;
                        return BasisAssetStatus.HasNewHash;
                    }
                }
            }
            BasisLoadedAssets = new BasisLoadedAssets();
            return BasisAssetStatus.False;
        }
        public BasisAssetStatus LoadedBundlesContains(AssetBundle AssetBundle, BasisBundleInformation Hash, out BasisLoadedAssets BasisLoadedAssets)
        {
            if (AssetBundle == null)
            {
                BasisLoadedAssets = new BasisLoadedAssets();
                return BasisAssetStatus.False;
            }
            for (int Index = 0; Index < LoadedBundles.Count; Index++)
            {
                BasisLoadedAssets LoadedBundle = LoadedBundles[Index];
                if (LoadedBundle.Bundle == AssetBundle)
                {
                    if (LoadedBundle.Hash.BasisBundleGenerated.AssetBundleHash == Hash.BasisBundleGenerated.AssetBundleHash)
                    {
                        BasisLoadedAssets = LoadedBundle;
                        return BasisAssetStatus.True;
                    }
                    else
                    {
                        BasisLoadedAssets = LoadedBundle;
                        return BasisAssetStatus.HasNewHash;
                    }
                }
            }
            BasisLoadedAssets = new BasisLoadedAssets();
            return BasisAssetStatus.False;
        }
        public bool AddBundle(string Url, string localPath, BasisBundleInformation hash, AssetBundle AssetBundle, out BasisLoadedAssets BasisLoadedAssets)
        {
            if (string.IsNullOrEmpty(Url))
            {
                Debug.LogError("Missing Url was null or empty");
                BasisLoadedAssets = new BasisLoadedAssets();
                return false;
            }
            if (string.IsNullOrEmpty(localPath))
            {
                Debug.LogError("Missing localPath was null or empty");
                BasisLoadedAssets = new BasisLoadedAssets();
                return false;
            }
            BasisAssetStatus Status = LoadedBundlesContains(Url, hash, out BasisLoadedAssets A);
            BasisAssetStatus StatusB = LoadedBundlesContains(AssetBundle, hash, out BasisLoadedAssets B);
            if (Status == BasisAssetStatus.False && StatusB == BasisAssetStatus.False)
            {
                BasisLoadedAssets = new BasisLoadedAssets
                {
                    Bundle = AssetBundle,
                    Url = Url,
                    localPath = localPath,
                    Hash = hash,
                    IsBundleLoaded = false,
                };
                LoadedBundles.Add(BasisLoadedAssets);
                return true;
            }
            else
            {
                Debug.LogError("A Status was true A was " + Status + " | B was " + StatusB);
            }
            BasisLoadedAssets = new BasisLoadedAssets();
            return false;
        }
        public async Task<BasisLoadedAssets> LoadBundle(string url, BasisBundleInformation Hash, string localPath, ProgressReport progressCallback)
        {
            BasisAssetStatus output = LoadedBundlesContains(url, Hash, out BasisLoadedAssets loadedAssets);

            switch (output)
            {
                case BasisAssetStatus.True:
                    Debug.Log("Found Bundle already loaded");
                    return await TrackLoadingProgress(loadedAssets, progressCallback);

                case BasisAssetStatus.False:
                case BasisAssetStatus.HasNewHash:
                    if (output == BasisAssetStatus.HasNewHash)
                    {
                        UnloadAssetBundle(url);
                    }

                    Debug.Log("Loading from disc and adding bundle");
                    if (AddBundle(url, localPath, Hash, null, out loadedAssets))
                    {
                        loadedAssets.bundleRequest = AssetBundle.LoadFromFileAsync(localPath);
                        loadedAssets.ProgressReportAvatarLoad = progressCallback;
                        return await TrackLoadingProgress(loadedAssets, progressCallback);
                    }
                    else
                    {
                        Debug.LogError("Unable to add bundle!");
                        return null;
                    }
            }
            Debug.LogError("This should never run, update this api returning null status for bundle!");
            return null;
        }
        private async Task<BasisLoadedAssets> TrackLoadingProgress(BasisLoadedAssets loadedAssets, ProgressReport progressCallback)
        {
            while (!loadedAssets.bundleRequest.isDone)
            {
                progressCallback?.Invoke(50 + loadedAssets.bundleRequest.progress * 50); // Progress from 50 to 100 during loading
                await Task.Yield();
            }

            loadedAssets.Bundle = loadedAssets.bundleRequest.assetBundle;
            loadedAssets.IsBundleLoaded = true;

            if (loadedAssets.Bundle == null)
            {
                Debug.LogError("Missing Bundle!");
                return null;
            }

            Debug.Log("Bundle loaded and ready");
            return loadedAssets;
        }
        public async Task DownloadAssetBundleAsync(string url, string localPath, ProgressReport progressCallback)
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
                Debug.Log("Failed to download AssetBundle: " + request.error + " for url " + url + " with localpath " + localPath);
                return;
            }

            File.WriteAllBytes(localPath, request.downloadHandler.data);
            Debug.Log("AssetBundle saved to: " + localPath);
        }

    }
}