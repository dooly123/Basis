using BasisSerializer.OdinSerializer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public static class BasisLoadhandler
{
    public static Dictionary<string, BasisTrackedBundleWrapper> QueryableBundles = new Dictionary<string, BasisTrackedBundleWrapper>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static async Task OnRuntimeMethodLoad()
    {
        Debug.Log("After Scene is loaded and game is running");
        await EnsureLoadAllComplete();
    }
    public static async Task<GameObject> LoadGameobjectBundle(BasisLoadableBundle BasisLoadableBundle, bool UseCondom, BasisProgressReport.ProgressReport Report, CancellationToken CancellationToken)
    {
        await EnsureLoadAllComplete();
        if (QueryableBundles.TryGetValue(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL, out BasisTrackedBundleWrapper Wrapper))
        {
            try
            {
                //was previously loaded and already a loaded bundle skip everything and go for the source.
                await Wrapper.WaitForBundleLoadAsync();
                return await LoadAssetFromBundle.BundleToAsset(Wrapper, UseCondom);
            }
            catch (Exception E)
            {
                QueryableBundles.Remove(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL);
                Debug.LogError("Unable to load Loaded content " + E);
                return null;
            }
        }
        else
        {
            Wrapper = new BasisTrackedBundleWrapper()
            {
                AssetBundle = null,
                LoadableBundle = BasisLoadableBundle,
            };
            try
            {
                if (QueryableBundles.TryAdd(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL, Wrapper))
                {

                }
                else
                {
                    Debug.LogError("cant add Wrapper");
                }

                //first time, thats great but before we download the file lets check to see if we already have a saved version of it.
                bool URLDisc = HasURLOnDisc(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL, out OnDiscInformation Info);
                OnDiscInformation OnDiscInformation = new OnDiscInformation();
                if (URLDisc)
                {
                    await BasisBundleManagement.DataOnDiscProcessMetaAsync(Wrapper, Info.StoredMetaLocal, Info.StoredBundleLocal, Report, CancellationToken);
                }
                else
                {
                    await BasisBundleManagement.DownloadAndSaveBundle(Wrapper, Report, CancellationToken);
                }
                AssetBundleCreateRequest output = await BasisEncryptionToData.GenerateBundleFromFile(Wrapper.LoadableBundle.UnlockPassword, Wrapper.LoadableBundle.BasisStoredEncyptedBundle.LocalBundleFile, Wrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetBundleCRC, Report);
                await output;
                if (URLDisc == false)
                {
                    OnDiscInformation = new OnDiscInformation()
                    {
                        StoredMetaURL = Wrapper.LoadableBundle.BasisRemoteBundleEncypted.MetaURL,
                        StoredBundleURL = Wrapper.LoadableBundle.BasisRemoteBundleEncypted.BundleURL,
                        StoredMetaLocal = Wrapper.LoadableBundle.BasisStoredEncyptedBundle.LocalMetaFile,
                        AssetToLoad = Wrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName,
                        StoredBundleLocal = Wrapper.LoadableBundle.BasisStoredEncyptedBundle.LocalBundleFile
                    };
                    await TryAddOnDiscInfo(OnDiscInformation);
                }
                Wrapper.AssetBundle = output.assetBundle;
                GameObject Output = await LoadAssetFromBundle.BundleToAsset(Wrapper, UseCondom);
                return Output;
            }
            catch (Exception e)
            {
                Wrapper.DidErrorOccur = true;
                QueryableBundles.Remove(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL);
                Debug.LogError(e);
                if (File.Exists(BasisLoadableBundle.BasisStoredEncyptedBundle.LocalMetaFile))
                {
                    File.Delete(BasisLoadableBundle.BasisStoredEncyptedBundle.LocalMetaFile);
                }
                if (File.Exists(BasisLoadableBundle.BasisStoredEncyptedBundle.LocalBundleFile))
                {
                    File.Delete(BasisLoadableBundle.BasisStoredEncyptedBundle.LocalBundleFile);
                }
                loadableDiscData.TryRemove(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL,out  OnDiscInformation Value);
                return null;
            }
        }
    }
    public static async Task LoadSceneBundle(bool MakeActiveScene, BasisLoadableBundle BasisLoadableBundle, BasisProgressReport.ProgressReport Report, CancellationToken CancellationToken)
    {
        await EnsureLoadAllComplete();
        if (QueryableBundles.TryGetValue(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL, out BasisTrackedBundleWrapper Wrapper))
        {
            //was previously loaded and already a loaded bundle skip everything and go for the source.
            await Wrapper.WaitForBundleLoadAsync();
            await LoadAssetFromBundle.LoadSceneFromAssetBundleAsync(Wrapper, MakeActiveScene, Report);
            return;
        }
        else
        {
            Wrapper = new BasisTrackedBundleWrapper()
            {
                AssetBundle = null,
                LoadableBundle = BasisLoadableBundle,
            };
            try
            {
                if (QueryableBundles.TryAdd(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL, Wrapper))
                {

                }
                else
                {
                    Debug.LogError("cant add Wrapper");
                }

                //first time, thats great but before we download the file lets check to see if we already have a saved version of it.
                bool URLDisc = HasURLOnDisc(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL, out OnDiscInformation Info);
                OnDiscInformation OnDiscInformation = new OnDiscInformation();
                if (URLDisc)
                {
                    await BasisBundleManagement.DataOnDiscProcessMetaAsync(Wrapper, Info.StoredMetaLocal, Info.StoredBundleLocal, Report, CancellationToken);
                }
                else
                {
                    await BasisBundleManagement.DownloadAndSaveBundle(Wrapper, Report, CancellationToken);
                }
                AssetBundleCreateRequest output = await BasisEncryptionToData.GenerateBundleFromFile(Wrapper.LoadableBundle.UnlockPassword, Wrapper.LoadableBundle.BasisStoredEncyptedBundle.LocalBundleFile, Wrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetBundleCRC, Report);
                await output;
                if (URLDisc == false)
                {
                    OnDiscInformation = new OnDiscInformation()
                    {
                        StoredMetaURL = Wrapper.LoadableBundle.BasisRemoteBundleEncypted.MetaURL,
                        StoredBundleURL = Wrapper.LoadableBundle.BasisRemoteBundleEncypted.BundleURL,
                        StoredMetaLocal = Wrapper.LoadableBundle.BasisStoredEncyptedBundle.LocalMetaFile,
                        AssetToLoad = Wrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName,
                        StoredBundleLocal = Wrapper.LoadableBundle.BasisStoredEncyptedBundle.LocalBundleFile
                    };
                    await TryAddOnDiscInfo(OnDiscInformation);
                }
                Wrapper.AssetBundle = output.assetBundle;
                await LoadAssetFromBundle.LoadSceneFromAssetBundleAsync(Wrapper, MakeActiveScene, Report);
                return;
            }
            catch (Exception e)
            {
                Wrapper.DidErrorOccur = true;
                Debug.LogError(e);
                return;
            }
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
    public static void TryRemoveOnDiscInfo(string Path)
    {
        if (loadableDiscData.TryRemove(Path, out _))
        {
            // Define the file path in the persistent data directory
            string filePath = BasisIOManagement.GenerateFilePath($"{Path}{BasisBundleManagement.MetaBasis}", BasisBundleManagement.AssetBundles);

            // Try to delete the file
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"Deleted OnDiscInformation from {filePath}");
                }
                else
                {
                    Debug.LogWarning($"File not found at {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete OnDiscInformation: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("OnDiscInformation not found or already removed");
        }
    }
}