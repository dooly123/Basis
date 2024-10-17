using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public static class BasisBundleManagement
{
    public static string DecryptedMetaBasis = ".DecryptedMetaBasis";
    public static string DecryptedBundleBasis = ".DecryptedBundleBasis";
    public static string EncryptedMetaBasis = ".EncryptedMetaBasis";
    public static string EncryptedBundleBasis = ".EncryptedBundleBasis";

    public static string AssetBundles = "AssetBundles";
    public static string LockedBundles = "LockedBundles";

    public static ConcurrentDictionary<string, BasisBundleInformation> UnLoadedBundles = new ConcurrentDictionary<string, BasisBundleInformation>();

    public static ConcurrentDictionary<string, BasisLoadableBundle> LoadableBundles = new ConcurrentDictionary<string, BasisLoadableBundle>();

    // Dictionary to track ongoing downloads keyed by MetaURL
    private static ConcurrentDictionary<string, Task<BasisBundleInformation>> OnGoingDownloads = new ConcurrentDictionary<string, Task<BasisBundleInformation>>();
    public static BasisProgressReport.ProgressReport FindAllBundlesReport;

    /// <summary>
    /// AssetToLoad,Password
    /// </summary>
    public static ConcurrentDictionary<string, string> UnlockedVisibleBundles = new ConcurrentDictionary<string, string>();
    public static bool FindBundle(BasisBundleInformation BasisBundleInformation)
    {
        Debug.Log($"Checking if bundle exists for {BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}");
        if (LoadableBundles.ContainsKey(BasisBundleInformation.BasisBundleGenerated.AssetToLoadName))
        {
            Debug.Log("Bundle found in LoadableBundles.");
            return true;
        }
        Debug.Log("Bundle not found in LoadableBundles.");
        return false;
    }
    public static async Task FindAllBundles()
    {
        /*
        string FolderPath = GenerateFolderPath(AssetBundles);
        string[] Files = System.IO.Directory.GetFiles(FolderPath, $"*{DecryptedMetaBasis}");
        //i want to do this over multiple frames
        foreach (string File in Files)
        {
            BasisBundleInformation BasisBundleInformation = BasisEncryptionToData.GenerateMetaFromFile(,File);
            string AssetToLoadName = BasisBundleInformation.BasisBundleGenerated.AssetToLoadName;
            if (UnLoadedBundles.TryAdd(AssetToLoadName, BasisBundleInformation))
            {

            }
            else
            {
                Debug.LogError("There was a Duplicate Asset with " + AssetToLoadName);
            }
        }
        */
    }
    public static async Task<BasisLoadableBundle> DownloadAndSaveBundle(BasisLoadableBundle BasisLoadedBundle, BasisProgressReport.ProgressReport progressCallback, CancellationToken cancellationToken)
    {
        string metaUrl = BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL;
        Debug.Log($"Starting download process for {metaUrl}");

        // Check if there's an ongoing or completed download for this MetaURL
        if (!OnGoingDownloads.TryGetValue(metaUrl, out var downloadTask))
        {
            Debug.Log($"No ongoing download for {metaUrl}, starting a new one.");
            downloadTask = DownloadAndProcessMeta(BasisLoadedBundle, progressCallback, cancellationToken);
            OnGoingDownloads.TryAdd(metaUrl, downloadTask);
        }
        else
        {
            Debug.Log($"Found an ongoing download for {metaUrl}, awaiting completion.");
        }

        // Await the task (this will wait for the first download if it's still in progress)
        BasisBundleInformation bundleInfo = await downloadTask;

        // Handle the result or failure (if bundleInfo is null, there was an error)
        if (bundleInfo.HasError)
        {
            Debug.LogError($"Failed to download and process meta for {metaUrl}");
            BasisLoadedBundle.BasisBundleInformation = bundleInfo;
            return BasisLoadedBundle;
        }

        // Update the LoadableBundles dictionary after download completes
        LoadableBundles.TryAdd(bundleInfo.BasisBundleGenerated.AssetToLoadName, BasisLoadedBundle);
        Debug.Log($"Download and processing for {metaUrl} completed successfully.");
        BasisLoadedBundle.BasisBundleInformation = bundleInfo;
        return BasisLoadedBundle;
    }
    /// <summary>
    /// generates unique id for new data 
    /// copy just the meta data to new location
    /// decrypt it. rename file to match new 
    /// </summary>
    /// <param name="BasisLoadedBundle"></param>
    /// <param name="progressCallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<BasisBundleInformation> DownloadAndProcessMeta(BasisLoadableBundle BasisLoadedBundle, BasisProgressReport.ProgressReport progressCallback, CancellationToken cancellationToken)
    {
        try
        {
            Debug.Log($"Downloading meta file for {BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL}");
            // this downloads in chunks the file, using a UniqueID that is unnassocated with the final bundle file
            string UniqueDownload = BasisGenerateUniqueID.GenerateUniqueID();
            string UniqueFilePath = BasisIOManagement.GenerateFilePath($"{UniqueDownload}{DecryptedMetaBasis}", LockedBundles);

            if (File.Exists(UniqueFilePath))
            {
                File.Delete(UniqueFilePath);
            }
            if (BasisLoadedBundle.BasisRemoteBundleEncypted.IsLocal)
            {
                File.Copy(BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL, UniqueFilePath);//the goal here is just to get the data out
            }
            else
            {
                await BasisIOManagement.DownloadFile(BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL, UniqueFilePath, progressCallback, cancellationToken);
            }
            Debug.Log($"Successfully downloaded meta file for {BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL}");

            // Step 2: Decrypt the meta file
            Debug.Log("Decrypting meta file...");
            BasisBundleInformation BasisBundleInformation = await BasisEncryptionToData.GenerateMetaFromFile(BasisLoadedBundle.UnlockPassword, UniqueFilePath, progressCallback);


            // Step 4: Download the bundle file
            Debug.Log($"Downloading bundle file from {BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL}");

            string FilePathMeta = BasisIOManagement.GenerateFilePath($"{BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}{DecryptedMetaBasis}", AssetBundles);
            string FilePathBundle = BasisIOManagement.GenerateFilePath($"{BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}{DecryptedBundleBasis}", AssetBundles);

            File.Move(UniqueFilePath, FilePathMeta);//move encrypted to match new name.

            if (BasisLoadedBundle.BasisRemoteBundleEncypted.IsLocal)
            {
                File.Copy(BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL, FilePathBundle);//the goal here is just to get the data out
            }
            else
            {
                await BasisIOManagement.DownloadFile(BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL, FilePathBundle, progressCallback, cancellationToken);
            }

            Debug.Log($"Successfully downloaded bundle file for {BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL}");
            Debug.Log("Meta and bundle files written to disk successfully.");
            return BasisBundleInformation;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during download and processing of meta: {ex.Message}");
        }
        return new BasisBundleInformation() { HasError = true };
    }
}