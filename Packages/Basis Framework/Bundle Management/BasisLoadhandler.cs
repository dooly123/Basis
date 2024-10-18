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
                BasisTrackedBundleWrapper = BasisBundleManagement.DataOnDiscProcessMeta(BasisLoadableBundle, Info.StoredMetaLocal, Info.StoredBundleLocal, Report, CancellationToken);
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
                BasisTrackedBundleWrapper = BasisBundleManagement.DataOnDiscProcessMeta(BasisLoadableBundle, Info.StoredMetaLocal, Info.StoredBundleLocal, Report, CancellationToken);
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
    private static Lazy<Task> _loadAllTask = new Lazy<Task>(() => LoadAllInternal());
    private static async Task EnsureLoadAllComplete()
    {
        if (!IsInitalized)
        {
            await _loadAllTask.Value; // Wait for LoadAll to complete if not initialized
        }
    }


    public struct OnDiscInformation
    {
        public string StoredMetaURL;//where we got file from
        public string StoredMetaLocal;//where we stored the file
        public string StoredBundleLocal;//where we stored the file
        public string AssetToLoad;
    }
    public static List<OnDiscInformation> loadableDiscData = new List<OnDiscInformation>();
    public static bool IsInitalized = false;
    // Internal method to load all on-disk information
    private static async Task LoadAllInternal()
    {
        // Define the directory where the files are saved
        string directoryPath = Application.persistentDataPath;

        // Use wildcard * to find all files with the .dat extension (or whatever you use)
        string[] files = Directory.GetFiles(directoryPath, "*" + BasisBundleManagement.MetaBasis);
        int Count = files.Length;
        Debug.Log("Found On Disc Bundles " + Count);
        for (int Index = 0; Index < Count; Index++)
        {
            string file = files[Index];
            try
            {
                // Read the file content as binary
                byte[] fileData = await File.ReadAllBytesAsync(file);

                // Deserialize the binary data back into an OnDiscInformation object
                OnDiscInformation onDiscInformation = SerializationUtility.DeserializeValue<OnDiscInformation>(fileData, DataFormat.Binary);

                // Add to the loadableDiscData if it's not already in the list
                if (!loadableDiscData.Contains(onDiscInformation))
                {
                    loadableDiscData.Add(onDiscInformation);
                    Debug.Log($"Loaded OnDiscInformation from {file}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load OnDiscInformation from {file}: {ex.Message}");
            }
        }
        IsInitalized = true; // Mark the process as complete
    }
    public static bool HasURLOnDisc(string MetaURL,out OnDiscInformation Info)
    {
        foreach (OnDiscInformation OnDiscInformation in loadableDiscData)
        {
            if(OnDiscInformation.StoredMetaURL.Equals(MetaURL))
            {
                Debug.Log("File exists on Disc! " + MetaURL);
                Info = OnDiscInformation;
                return true;
            }
        }
        Info = new OnDiscInformation();
        return false;
    }
    public static async Task TryAddOnDiscInfo(OnDiscInformation onDiscInformation)
    {
        if (!loadableDiscData.Contains(onDiscInformation))
        {
            loadableDiscData.Add(onDiscInformation);

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
    public class BasisTrackedBundleWrapper
    {
        public Task<BasisLoadableBundle> LoadableBundle;
        public Task<AssetBundle> AssetBundle;
        public string metaUrl;
    }
}