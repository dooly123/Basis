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
        public ContentPoliceSelector ContentPoliceSelector;
[SerializeField]
        public List<BasisLoadedAssets> LoadedBundles = new List<BasisLoadedAssets>();
        public static List<AddressableLoadResourceBase> AsyncHandles = new List<AddressableLoadResourceBase>();
        public static List<Task> LoadingTasks = new List<Task>();
        public static AddressableManagement Instance;
        // Delegate to report progress (value between 0 and 100)
        public delegate void ProgressReport(float progress);
        public enum Status
        {
            False,
            True,
            HasNewHash
        }
        public static string ChangeExtension(string url, string newExtension)
        {
            // Get the file path without the extension
            string urlWithoutExtension = Path.ChangeExtension(url, null);

            // Add the new extension
            return urlWithoutExtension + newExtension;
        }
        // Method to load text asynchronously using UnityWebRequest
        public static async Task<string> LoadTextFromURLAsync(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // Send the request and wait for the response asynchronously
                UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    // Optionally, you can add a loading indicator or progress display here
                    await Task.Yield(); // This allows other tasks to run during the wait
                }

                // Check for errors
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error loading text: {request.error} for url " + url);
                    return string.Empty;
                }
                else
                {
                    // Return the downloaded text
                    return request.downloadHandler.text;
                }
            }
        }
        public Status LoadedBundlesContains(string Url,string Hash, out BasisLoadedAssets BasisLoadedAssets)
        {
            if (string.IsNullOrEmpty(Url))
            {
                BasisLoadedAssets = new BasisLoadedAssets();
                return  Status.False;
            }

            for (int Index = 0; Index < LoadedBundles.Count; Index++)
            {
                BasisLoadedAssets LoadedBundle = LoadedBundles[Index];
                if (LoadedBundle.Url == Url)
                {
                    if (LoadedBundle.Hash == Hash)
                    {
                        BasisLoadedAssets = LoadedBundle;
                        return Status.True;
                    }
                    else
                    {
                        BasisLoadedAssets = LoadedBundle;
                        return Status.HasNewHash;
                    }
                }
            }
            BasisLoadedAssets = new BasisLoadedAssets();
            return Status.False;
        }
        public Status LoadedBundlesContains(AssetBundle AssetBundle,string Hash, out BasisLoadedAssets BasisLoadedAssets)
        {
            if (AssetBundle == null)
            {
                BasisLoadedAssets = new BasisLoadedAssets();
                return Status.False;
            }
            for (int Index = 0; Index < LoadedBundles.Count; Index++)
            {
                BasisLoadedAssets LoadedBundle = LoadedBundles[Index];
                if (LoadedBundle.Bundle == AssetBundle)
                {
                    if (LoadedBundle.Hash == Hash)
                    {
                        BasisLoadedAssets = LoadedBundle;
                        return Status.True;
                    }
                    else
                    {
                        BasisLoadedAssets = LoadedBundle;
                        return Status.HasNewHash;
                    }
                }
            }
            BasisLoadedAssets = new BasisLoadedAssets();
            return Status.False;
        }
        public bool AddBundle(string Url, string localPath, string hash, AssetBundle AssetBundle, out BasisLoadedAssets BasisLoadedAssets)
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
            if (string.IsNullOrEmpty(hash))
            {
                Debug.LogError("Missing Hash was null or empty");
                BasisLoadedAssets = new BasisLoadedAssets();
                return false;
            }
            Status Status = LoadedBundlesContains(Url, hash, out BasisLoadedAssets A);
            Status StatusB = LoadedBundlesContains(AssetBundle, hash, out BasisLoadedAssets B);
            if (Status == Status.False && StatusB == Status.False)
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
        public static async Task<BasisLoadedAssets> LoadBundle(string url, string Hash, string localPath, ProgressReport progressCallback)
        {
            Hash = await BasisAssetBundleHashLookup.GetHashOrFallback(Hash, url);
            Status output = Instance.LoadedBundlesContains(url, Hash, out BasisLoadedAssets loadedAssets);

            switch (output)
            {
                case Status.True:
                    Debug.Log("Found Bundle already loaded");
                    return await TrackLoadingProgress(loadedAssets, progressCallback);

                case Status.False:
                case Status.HasNewHash:
                    if (output == Status.HasNewHash)
                    {
                        Instance.UnloadAssetBundle(url);
                    }

                    Debug.Log("Loading from disc and adding bundle");
                    if (Instance.AddBundle(url, localPath, Hash, null, out loadedAssets))
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

        private static async Task<BasisLoadedAssets> TrackLoadingProgress(BasisLoadedAssets loadedAssets, ProgressReport progressCallback)
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
                Debug.Log("Failed to download AssetBundle: " + request.error + " for url " + url + " with localpath " + localPath);
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
                    LoadedBundles.RemoveAt(Index);
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