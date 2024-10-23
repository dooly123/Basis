using BasisSerializer.OdinSerializer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class BasisLoadHandler
{
    public static Dictionary<string, BasisTrackedBundleWrapper> LoadedBundles = new Dictionary<string, BasisTrackedBundleWrapper>();
    public static ConcurrentDictionary<string, OnDiscInformation> OnDiscData = new ConcurrentDictionary<string, OnDiscInformation>();
    public static bool IsInitialized = false;

    private static readonly object _discInfoLock = new object();
    private static SemaphoreSlim _initSemaphore = new SemaphoreSlim(1, 1);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static async Task OnGameStart()
    {
        Debug.Log("Game has started after scene load.");
        await EnsureInitializationComplete();
    }

    public static async Task<GameObject> LoadGameObjectBundle(BasisLoadableBundle loadableBundle, bool useContentRemoval, BasisProgressReport.ProgressReport report, CancellationToken cancellationToken)
    {
        await EnsureInitializationComplete();

        if (LoadedBundles.TryGetValue(loadableBundle.BasisRemoteBundleEncrypted.MetaURL, out BasisTrackedBundleWrapper wrapper))
        {
            try
            {
                await wrapper.WaitForBundleLoadAsync();
                return await LoadAssetFromBundle.LoadFromWrapper(wrapper, useContentRemoval);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load content: {ex}");
                LoadedBundles.Remove(loadableBundle.BasisRemoteBundleEncrypted.MetaURL);
                return null;
            }
        }

        return await HandleFirstBundleLoad(loadableBundle, useContentRemoval, report, cancellationToken);
    }

    public static async Task LoadSceneBundle(bool makeActiveScene, BasisLoadableBundle loadableBundle, BasisProgressReport.ProgressReport report, CancellationToken cancellationToken)
    {
        await EnsureInitializationComplete();

        if (LoadedBundles.TryGetValue(loadableBundle.BasisRemoteBundleEncrypted.MetaURL, out BasisTrackedBundleWrapper wrapper))
        {
            await wrapper.WaitForBundleLoadAsync();
            await LoadAssetFromBundle.LoadSceneFromBundleAsync(wrapper, makeActiveScene, report);
            return;
        }

        await HandleFirstSceneLoad(loadableBundle, makeActiveScene, report, cancellationToken);
    }

    private static async Task HandleFirstSceneLoad(BasisLoadableBundle loadableBundle, bool makeActiveScene, BasisProgressReport.ProgressReport report, CancellationToken cancellationToken)
    {
        var wrapper = new BasisTrackedBundleWrapper { AssetBundle = null, LoadableBundle = loadableBundle };

        if (!LoadedBundles.TryAdd(loadableBundle.BasisRemoteBundleEncrypted.MetaURL, wrapper))
        {
            Debug.LogError("Unable to add bundle wrapper.");
            return;
        }

        await HandleBundleLoading(wrapper, report, cancellationToken);
        await LoadAssetFromBundle.LoadSceneFromBundleAsync(wrapper, makeActiveScene, report);
    }

    private static async Task<GameObject> HandleFirstBundleLoad(BasisLoadableBundle loadableBundle, bool useContentRemoval, BasisProgressReport.ProgressReport report, CancellationToken cancellationToken)
    {
        var wrapper = new BasisTrackedBundleWrapper { AssetBundle = null, LoadableBundle = loadableBundle };

        if (!LoadedBundles.TryAdd(loadableBundle.BasisRemoteBundleEncrypted.MetaURL, wrapper))
        {
            Debug.LogError("Unable to add bundle wrapper.");
            return null;
        }

        try
        {
            await HandleBundleLoading(wrapper, report, cancellationToken);
            return await LoadAssetFromBundle.LoadFromWrapper(wrapper, useContentRemoval);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            LoadedBundles.Remove(loadableBundle.BasisRemoteBundleEncrypted.MetaURL);
            CleanupFiles(loadableBundle.BasisStoredEncryptedBundle);
            OnDiscData.TryRemove(loadableBundle.BasisRemoteBundleEncrypted.MetaURL, out _);
            return null;
        }
    }

    private static async Task HandleBundleLoading(BasisTrackedBundleWrapper wrapper, BasisProgressReport.ProgressReport report, CancellationToken cancellationToken)
    {
        bool isOnDisc = IsBundleOnDisc(wrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL, out OnDiscInformation discInfo);

        if (isOnDisc)
        {
            await BasisBundleManagement.ProcessOnDiscMetaDataAsync(wrapper, discInfo.StoredMetaLocal, discInfo.StoredBundleLocal, report, cancellationToken);
        }
        else
        {
            await BasisBundleManagement.DownloadAndSaveBundle(wrapper, report, cancellationToken);
        }

        AssetBundleCreateRequest bundleRequest = await BasisEncryptionToData.GenerateBundleFromFile(
            wrapper.LoadableBundle.UnlockPassword,
            wrapper.LoadableBundle.BasisStoredEncryptedBundle.LocalBundleFile,
            wrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetBundleCRC,
            report
        );

        wrapper.AssetBundle = bundleRequest.assetBundle;

        if (!isOnDisc)
        {
            OnDiscInformation newDiscInfo = new OnDiscInformation
            {
                StoredMetaURL = wrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL,
                StoredBundleURL = wrapper.LoadableBundle.BasisRemoteBundleEncrypted.BundleURL,
                StoredMetaLocal = wrapper.LoadableBundle.BasisStoredEncryptedBundle.LocalMetaFile,
                AssetToLoad = wrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName,
                StoredBundleLocal = wrapper.LoadableBundle.BasisStoredEncryptedBundle.LocalBundleFile
            };

            await AddDiscInfo(newDiscInfo);
        }
    }

    public static bool IsBundleOnDisc(string metaUrl, out OnDiscInformation info)
    {
        lock (_discInfoLock)
        {
            foreach (var discInfo in OnDiscData.Values)
            {
                if (discInfo.StoredMetaURL == metaUrl)
                {
                    info = discInfo;
                    return true;
                }
            }

            info = new OnDiscInformation();
            return false;
        }
    }

    public static async Task AddDiscInfo(OnDiscInformation discInfo)
    {
        if (OnDiscData.TryAdd(discInfo.StoredMetaURL, discInfo))
        {
            string filePath = BasisIOManagement.GenerateFilePath($"{discInfo.AssetToLoad}{BasisBundleManagement.MetaSuffix}", BasisBundleManagement.AssetBundles);
            byte[] serializedData = SerializationUtility.SerializeValue(discInfo, DataFormat.Binary);

            try
            {
                await File.WriteAllBytesAsync(filePath, serializedData);
                Debug.Log($"Disc info saved to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save disc info: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Disc info already exists.");
        }
    }

    public static void RemoveDiscInfo(string metaUrl)
    {
        if (OnDiscData.TryRemove(metaUrl, out _))
        {
            string filePath = BasisIOManagement.GenerateFilePath($"{metaUrl}{BasisBundleManagement.MetaSuffix}", BasisBundleManagement.AssetBundles);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"Deleted disc info from {filePath}");
            }
            else
            {
                Debug.LogWarning($"File not found at {filePath}");
            }
        }
        else
        {
            Debug.LogError("Disc info not found or already removed.");
        }
    }

    private static async Task EnsureInitializationComplete()
    {
        if (!IsInitialized)
        {
            await _initSemaphore.WaitAsync();
            try
            {
                if (!IsInitialized)
                {
                    await LoadAllDiscData();
                    IsInitialized = true;
                }
            }
            finally
            {
                _initSemaphore.Release();
            }
        }
    }

    private static async Task LoadAllDiscData()
    {
        Debug.Log("Loading all disc data...");
        string path = BasisIOManagement.GenerateFolderPath(BasisBundleManagement.AssetBundles);
        string[] files = Directory.GetFiles(path, $"*{BasisBundleManagement.MetaSuffix}");

        foreach (var file in files)
        {
            Debug.Log($"Loading file: {file}");
            try
            {
                byte[] fileData = await File.ReadAllBytesAsync(file);
                OnDiscInformation discInfo = SerializationUtility.DeserializeValue<OnDiscInformation>(fileData, DataFormat.Binary);
                OnDiscData.TryAdd(discInfo.StoredMetaURL, discInfo);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load disc info from {file}: {ex.Message}");
            }
        }

        Debug.Log("Completed loading all disc data.");
    }

    private static void CleanupFiles(BasisStoredEncyptedBundle bundle)
    {
        if (File.Exists(bundle.LocalMetaFile))
        {
            File.Delete(bundle.LocalMetaFile);
        }

        if (File.Exists(bundle.LocalBundleFile))
        {
            File.Delete(bundle.LocalBundleFile);
        }
    }
}