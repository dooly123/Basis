using Basis.Scripts.Addressable_Driver.DebugError;
using Basis.Scripts.Addressable_Driver.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Basis.Scripts.Addressable_Driver.Loading
{
    public class AddressableManagement : MonoBehaviour
    {
        public Coroutine CheckTaskStatus;
[SerializeField]
        public List<BasisLoadedAssets> LoadedBundles = new List<BasisLoadedAssets>();
        public static List<AddressableLoadResourceBase> AsyncHandles = new List<AddressableLoadResourceBase>();
        public static List<Task> LoadingTasks = new List<Task>();
        public static AddressableManagement Instance;
        // Delegate to report progress (value between 0 and 100)
        public delegate void ProgressReport(float progress);

        public bool LoadedBundlesContains(string Url, out BasisLoadedAssets BasisLoadedAssets)
        {
            if (string.IsNullOrEmpty(Url))
            {
                BasisLoadedAssets = new BasisLoadedAssets();
                return false;
            }

            for (int Index = 0; Index < LoadedBundles.Count; Index++)
            {
                BasisLoadedAssets bundle = LoadedBundles[Index];
                if (bundle.Url == Url)
                {
                    BasisLoadedAssets = bundle;
                    return true;
                }
            }
            BasisLoadedAssets = new BasisLoadedAssets();
            return false;
        }
        public bool LoadedBundlesContains(AssetBundle AssetBundle, out BasisLoadedAssets BasisLoadedAssets)
        {
            if (AssetBundle == null)
            {
                BasisLoadedAssets = new BasisLoadedAssets();
                return false;
            }
            for (int Index = 0; Index < LoadedBundles.Count; Index++)
            {
                BasisLoadedAssets bundle = LoadedBundles[Index];
                if (bundle.Bundle == AssetBundle)
                {
                    BasisLoadedAssets = bundle;
                    return true;
                }
            }
            BasisLoadedAssets = new BasisLoadedAssets();
            return false;
        }
        public bool AddBundle(string Url, string localPath, string hash, AssetBundle AssetBundle, out BasisLoadedAssets BasisLoadedAssets)
        {
            if (string.IsNullOrEmpty(Url))
            {
                BasisLoadedAssets = new BasisLoadedAssets();
                return false;
            }
            if (string.IsNullOrEmpty(localPath))
            {
                BasisLoadedAssets = new BasisLoadedAssets();
                return false;
            }
            /*
             *             if (string.IsNullOrEmpty(hash))
            {

            }
             */
            if (LoadedBundlesContains(Url, out BasisLoadedAssets A) == false && LoadedBundlesContains(AssetBundle, out BasisLoadedAssets B) == false)
            {
                BasisLoadedAssets = new BasisLoadedAssets
                {
                    Bundle = AssetBundle,
                    Url = Url,
                    localPath = localPath,
                    Hash = hash,
                    IsBundleLoaded = false,
                    OverallLoadPercentage = 0,
                };
                LoadedBundles.Add(BasisLoadedAssets);
                return true;
            }
            BasisLoadedAssets = new BasisLoadedAssets();
            return false;
        }
        public static async Task<BasisLoadedAssets> LoadBundle(string url, string localPath, ProgressReport progressCallback)
        {
            if (Instance.LoadedBundlesContains(url, out BasisLoadedAssets Loaded))
            {
                Debug.Log("Found Bundle already loaded");
                while (!Loaded.bundleRequest.isDone)
                {
                    progressCallback?.Invoke(50 + Loaded.bundleRequest.progress * 50); // Progress from 50 to 100 during loading
                    await Task.Yield();
                }
                return Loaded;
            }
            else
            {
                Debug.Log("loading from disc and adding bundle");
                Instance.AddBundle(url, localPath, string.Empty, null, out BasisLoadedAssets LoadedAssets);
                LoadedAssets.bundleRequest = AssetBundle.LoadFromFileAsync(localPath);
                LoadedAssets.ProgressReportAvatarLoad = progressCallback;
                // Track loading progress
                while (!LoadedAssets.bundleRequest.isDone)
                {
                    progressCallback?.Invoke(50 + LoadedAssets.bundleRequest.progress * 50); // Progress from 50 to 100 during loading
                    await Task.Yield();
                }
                LoadedAssets.Bundle = LoadedAssets.bundleRequest.assetBundle;
                LoadedAssets.IsBundleLoaded = true;
                if (LoadedAssets.Bundle == null)
                {
                    Debug.LogError("missing Bundle!");
                }
                Debug.Log("bundle loaded and ready");
                return LoadedAssets;
            }
        }
        public static async Task DownloadAssetBundleAsync(string url, string localPath, ProgressReport progressCallback)
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
                Debug.Log("Failed to download AssetBundle: " + request.error);
                return;
            }

            File.WriteAllBytes(localPath, request.downloadHandler.data);
            Debug.Log("AssetBundle saved to: " + localPath);
        }

        public void OnEnable()
        {
            Instance = this;
            CheckTaskStatus = StartCoroutine(LoopCheckTask());
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
        public void UnloadAssetBundle(string url)
        {
            for (int Index = 0; Index < LoadedBundles.Count; Index++)
            {
                BasisLoadedAssets asset = LoadedBundles[Index];
                if (asset.Url.Equals(url) && asset.Bundle != null)
                {
                    asset.Bundle.Unload(false);
                    return;
                }
            }
        }
        public static string GetFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            return Path.GetFileName(uri.LocalPath);
        }
        public static string GetFileNameFromUrlWithoutExtension(string url)
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath); // Get the file name with extension
            return Path.GetFileNameWithoutExtension(fileName); // Remove the extension and return
        }
        private IEnumerator LoopCheckTask()
        {
            while (true)
            {
                if (AsyncHandles.Count != 0)
                {
                    AsyncHandles.RemoveAll(item => item == null);
                    for (int Index = 0; Index < AsyncHandles.Count; Index++)
                    {
                        AddressableLoadResourceBase loadRequest = AsyncHandles[Index];
                        // Calculate and update the overall progress
                        float totalProgress = 0f;
                        foreach (var handle in loadRequest.Handles)
                        {
                            totalProgress += handle.PercentComplete;
                        }
                        totalProgress /= loadRequest.Handles.Count;

                        // Update progress or perform other actions based on the progress
                        AddressableLoadProcess.UpdateProgress(loadRequest, totalProgress);
                    }
                }
                yield return new WaitForSeconds(1f); // Wait for 1 second
            }
        }
        public static void RemoveFromLoadingList(AddressableLoadResourceBase RemoveMe)
        {
            if (RemoveMe != null)
            {
                AddressableLoadProcess.UpdateProgress(RemoveMe, 1);
                AsyncHandles.Remove(RemoveMe);
            }
        }
        public static async Task<bool> AwaitLoading(AddressableLoadResourceBase loadRequest)
        {
            try
            {
                loadRequest.OnLoaded.AddListener(RemoveFromLoadingList);
                foreach (AsyncOperationHandle asyncOperationHandle in loadRequest.Handles)
                {
                    if (!asyncOperationHandle.IsValid())
                    {
                        Debug.Log($"Invalid Async Handle In Key {loadRequest.Key}");
                        continue;
                    }
                    LoadingTasks.Add(asyncOperationHandle.Task);
                }
                AsyncHandles.Add(loadRequest);
                // Wait for all loading tasks to complete
                await Task.WhenAll(LoadingTasks);

                // Stop the progress check task after all loading tasks are completed
                AddressableLoadProcess.StopProgressCheck(loadRequest);

                AddressableLoadProcess.OnCompleteLoad(loadRequest);
                return true;
            }
            catch (Exception e)
            {
                AddressableDebug.DebugError($"{e.StackTrace} {e.Message}", loadRequest);
                return false;
            }
        }
    }
}