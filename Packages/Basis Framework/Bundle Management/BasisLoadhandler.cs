using BasisSerializer.OdinSerializer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public static class BasisLoadhandler
{
    public static ConcurrentDictionary<string, BasisTrackedBundleWrapper> QueryableBundles = new ConcurrentDictionary<string, BasisTrackedBundleWrapper>();
    public static async Task LoadGameobjectBundle(BasisLoadableBundle BasisLoadableBundle, bool UseCondom, BasisProgressReport.ProgressReport Report, CancellationToken CancellationToken)
    {
        await EnsureLoadAllComplete();
        if (QueryableBundles.TryGetValue(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL, out BasisTrackedBundleWrapper Wrapper))
        {
            //was previously loaded and already a loaded bundle skip everything and go for the source.
            if (Wrapper.LoadableBundle != null)
            {
                if (Wrapper.LoadableBundle.IsCompletedSuccessfully)
                {
                    if (Wrapper.LoadableBundle.Result.LoadedAssetBundle != null)
                    {
                        await LoadAssetFromBundle.BundleToAsset(Wrapper.LoadableBundle.Result, UseCondom);
                    }
                    else
                    {
                        Debug.LogError("LoadedAssetBundle was missing");
                    }
                }
                else
                {
                    BasisLoadableBundle Bundle = await Wrapper.LoadableBundle;
                    if (Bundle.BasisBundleInformation.HasError == false)
                    {
                        if (Wrapper.AssetBundle == null)
                        {
                            Debug.LogError("Missing Bundle Task! " + Wrapper.metaUrl);
                        }
                        else
                        {
                            if (Wrapper.AssetBundle.IsCompletedSuccessfully)
                            {
                                await LoadAssetFromBundle.BundleToAsset(Bundle, UseCondom);
                            }
                            else
                            {
                                await Wrapper.AssetBundle;
                                await LoadAssetFromBundle.BundleToAsset(Bundle, UseCondom);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Error During the import for this tie in Loading bundle");
                    }
                }
            }
            else
            {
                Debug.LogError("Loaded Bundle was Null");
            }
        }
        else
        {
            //first time, thats great but before we download the file lets check to see if we already have a saved version of it.
            bool URLDisc = HasURLOnDisc(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL,out OnDiscInformation Info);
            BasisTrackedBundleWrapper BasisTrackedBundleWrapper;
            OnDiscInformation OnDiscInformation = new OnDiscInformation();
            if (URLDisc)
            {
               BasisTrackedBundleWrapper = await BasisBundleManagement.DataOnDiscProcessMetaAsync(BasisLoadableBundle, Info.StoredMetaLocal, Info.StoredBundleLocal, Report, CancellationToken);
            }
            else
            {
                BasisTrackedBundleWrapper = await BasisBundleManagement.DownloadAndSaveBundle(BasisLoadableBundle, Report, CancellationToken);
                OnDiscInformation = new OnDiscInformation()
                {
                    StoredMetaURL = BasisTrackedBundleWrapper.LoadableBundle.Result.BasisRemoteBundleEncypted.MetaURL,
                    StoredMetaLocal = BasisTrackedBundleWrapper.LoadableBundle.Result.BasisStoredEncyptedBundle.LocalMetaFile,
                    AssetToLoad = BasisTrackedBundleWrapper.LoadableBundle.Result.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName,
                    StoredBundleLocal = BasisTrackedBundleWrapper.LoadableBundle.Result.BasisStoredEncyptedBundle.LocalBundleFile
                };
            }

            BasisLoadableBundle.LoadedAssetBundle = await BasisLoadBundle.LoadBasisBundle(BasisTrackedBundleWrapper, Report);
            if (URLDisc == false)
            {
                await TryAddOnDiscInfo(OnDiscInformation);
            }
            await LoadAssetFromBundle.BundleToAsset(BasisLoadableBundle, UseCondom);
        }
    }
    public static async Task LoadSceneBundle(bool MakeActiveScene, BasisLoadableBundle BasisLoadableBundle, BasisProgressReport.ProgressReport Report, CancellationToken CancellationToken)
    {
        await EnsureLoadAllComplete();
        if (QueryableBundles.TryGetValue(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL, out BasisTrackedBundleWrapper Wrapper))
        {
            //was previously loaded and already a loaded bundle skip everything and go for the source.
            if (Wrapper.LoadableBundle != null)
            {
                if (Wrapper.LoadableBundle.IsCompletedSuccessfully)
                {
                    if (Wrapper.LoadableBundle.Result.LoadedAssetBundle != null)
                    {
                        await LoadAssetFromBundle.LoadSceneFromAssetBundleAsync(Wrapper.LoadableBundle.Result, MakeActiveScene, Report);
                    }
                    else
                    {
                        Debug.LogError("LoadedAssetBundle was missing");
                    }
                }
                else
                {
                    BasisLoadableBundle Bundle = await Wrapper.LoadableBundle;
                    if (Bundle.BasisBundleInformation.HasError == false)
                    {
                        if (Wrapper.AssetBundle.IsCompletedSuccessfully)
                        {
                            await LoadAssetFromBundle.LoadSceneFromAssetBundleAsync(Bundle, MakeActiveScene, Report);
                        }
                        else
                        {
                            await Wrapper.AssetBundle;
                            await LoadAssetFromBundle.LoadSceneFromAssetBundleAsync(Bundle, MakeActiveScene, Report);
                        }
                    }
                    else
                    {
                        Debug.LogError("Error During the import for this tie in Loading bundle");
                    }
                }
            }
            else
            {
                Debug.LogError("Loaded Bundle was Null");
            }
        }
        else
        {
            //first time, thats great but before we download the file lets check to see if we already have a saved version of it.
            bool URLDisc = HasURLOnDisc(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL, out OnDiscInformation Info);
            BasisTrackedBundleWrapper BasisTrackedBundleWrapper;
            OnDiscInformation OnDiscInformation = new OnDiscInformation();
            if (URLDisc)
            {
                  BasisTrackedBundleWrapper = await BasisBundleManagement.DataOnDiscProcessMetaAsync(BasisLoadableBundle, Info.StoredMetaLocal, Info.StoredBundleLocal, Report, CancellationToken);
            }
            else
            {
                BasisTrackedBundleWrapper = await BasisBundleManagement.DownloadAndSaveBundle(BasisLoadableBundle, Report, CancellationToken);
                OnDiscInformation = new OnDiscInformation()
                {
                    StoredMetaURL = BasisTrackedBundleWrapper.LoadableBundle.Result.BasisRemoteBundleEncypted.MetaURL,
                    StoredMetaLocal = BasisTrackedBundleWrapper.LoadableBundle.Result.BasisStoredEncyptedBundle.LocalMetaFile,
                    AssetToLoad = BasisTrackedBundleWrapper.LoadableBundle.Result.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName,
                     StoredBundleLocal = BasisTrackedBundleWrapper.LoadableBundle.Result.BasisStoredEncyptedBundle.LocalBundleFile
                };
            }

            BasisLoadableBundle.LoadedAssetBundle = await BasisLoadBundle.LoadBasisBundle(BasisTrackedBundleWrapper, Report);
            if (URLDisc == false)
            {
                await TryAddOnDiscInfo(OnDiscInformation);
            }
            await LoadAssetFromBundle.LoadSceneFromAssetBundleAsync(BasisLoadableBundle, MakeActiveScene, Report);
        }
    }
    public static ConcurrentDictionary<string, OnDiscInformation> loadableDiscData = new ConcurrentDictionary<string, OnDiscInformation>();
    public static bool IsInitalized = false;
    // Internal method to load all on-disk information
    private static Task _loadAllTask;
    private static object _initLock = new object();

    private static SemaphoreSlim _initSemaphore = new SemaphoreSlim(1, 1); // To control concurrent access

    private static async Task EnsureLoadAllComplete()
    {
        if (!IsInitalized)
        {
            await _initSemaphore.WaitAsync(); // Awaiting the semaphore
            try
            {
                if (!IsInitalized) // Double-check if still not initialized
                {
                    await LoadAllInternal(); // Only initialize once
                    IsInitalized = true;
                }
            }
            finally
            {
                _initSemaphore.Release();
            }
        }
    }
    private static async Task LoadAllInternal()
    {
        // Initial logging
        Debug.Log("Starting to load all OnDisc information...");

        string path = BasisIOManagement.GenerateFolderPath(BasisBundleManagement.AssetBundles);
        string[] files = Directory.GetFiles(path, $"*{BasisBundleManagement.MetaBasis}");
        int count = files.Length;
        Debug.Log($"Found {count} On Disc Bundles.");

        for (int index = 0; index < count; index++)
        {
            string file = files[index];
            Debug.Log($"Loading file: {file}");
            try
            {
                byte[] fileData = await File.ReadAllBytesAsync(file);
                OnDiscInformation onDiscInformation = SerializationUtility.DeserializeValue<OnDiscInformation>(fileData, DataFormat.Binary);
                if (loadableDiscData.TryAdd(onDiscInformation.StoredMetaURL, onDiscInformation))
                {
                    Debug.Log($"Successfully added OnDiscInformation for {onDiscInformation.StoredMetaURL}");
                }
                else
                {
                    Debug.LogWarning($"OnDiscInformation for {onDiscInformation.StoredMetaURL} already exists.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load OnDiscInformation from {file}: {ex.Message}");
            }
        }
        IsInitalized = true; // Mark initialization complete.
        Debug.Log("Completed loading all OnDisc information.");
    }
    private static readonly object _discInfoLock = new object();

    public static bool HasURLOnDisc(string MetaURL, out OnDiscInformation Info)
    {
        lock (_discInfoLock)
        {
            foreach (OnDiscInformation OnDiscInformation in loadableDiscData.Values)
            {
                if (OnDiscInformation.StoredMetaURL == MetaURL)
                {
                    Debug.Log("File exists on Disc! " + MetaURL);
                    Info = OnDiscInformation;
                    return true;
                }
            }
            Info = new OnDiscInformation();
            return false;
        }
    }
    public static async Task TryAddOnDiscInfo(OnDiscInformation onDiscInformation)
    {
        if (loadableDiscData.TryAdd(onDiscInformation.StoredMetaURL, onDiscInformation))
        {
            // Serialize the OnDiscInformation object to a binary format
            byte[] onDiscInfo = SerializationUtility.SerializeValue<OnDiscInformation>(onDiscInformation, DataFormat.Binary);

            // Define a file path in the persistent data directory
            string filePath = BasisIOManagement.GenerateFilePath($"{onDiscInformation.AssetToLoad}{BasisBundleManagement.MetaBasis}", BasisBundleManagement.AssetBundles);

            // Save the binary data to the file
            try
            {
                await File.WriteAllBytesAsync(filePath, onDiscInfo);
                Debug.Log($"Saved OnDiscInformation to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save OnDiscInformation: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Already has Disc Info");
        }
    }
}