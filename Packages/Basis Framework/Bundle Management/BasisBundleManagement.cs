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
    public static string MetaBasis = ".MetaBasis";
    public static string AssetBundles = "AssetBundles";
    public static string LockedBundles = "LockedBundles";

    // Dictionary to track ongoing downloads keyed by MetaURL
    public static BasisProgressReport.ProgressReport FindAllBundlesReport;
    /// <summary>
    /// AssetToLoad,Password
    /// </summary>
    public static ConcurrentDictionary<string, string> UnlockedVisibleBundles = new ConcurrentDictionary<string, string>();
    public static async Task DownloadAndSaveBundle(BasisTrackedBundleWrapper BasisTrackedBundleWrapper, BasisProgressReport.ProgressReport progressCallback, CancellationToken cancellationToken)
    {
        string metaUrl = BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncypted.MetaURL;
        Debug.Log($"Starting download process for {metaUrl}");

        Debug.Log($"No ongoing download for {metaUrl}, starting a new one.");
        BasisTrackedBundleWrapper downloadTask = new BasisTrackedBundleWrapper
        {
            LoadableBundle = await DownloadAndProcessMeta(BasisTrackedBundleWrapper.LoadableBundle, progressCallback, cancellationToken),
            metaUrl = metaUrl
        };
    }
    public static async Task DataOnDiscProcessMetaAsync(BasisTrackedBundleWrapper BasisTrackedBundleWrapper, string metaFilepath, string localBundleFile, BasisProgressReport.ProgressReport progressCallback, CancellationToken cancellationToken)
    {
        // Log entry point
        Debug.Log("Starting DataOnDiscProcessMeta method...");

        if (BasisTrackedBundleWrapper == null || string.IsNullOrEmpty(metaFilepath) || string.IsNullOrEmpty(localBundleFile))
        {
            Debug.LogError("Invalid parameters passed to DataOnDiscProcessMeta");
            return; // or handle accordingly
        }
        // Set local paths
        Debug.Log($"Setting local bundle file: {localBundleFile}");
        BasisTrackedBundleWrapper.LoadableBundle.BasisStoredEncyptedBundle.LocalBundleFile = localBundleFile;

        Debug.Log($"Setting local meta file: {metaFilepath}");
        BasisTrackedBundleWrapper.LoadableBundle.BasisStoredEncyptedBundle.LocalMetaFile = metaFilepath;

        // Fetching the meta URL
        string metaUrl = BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncypted.MetaURL;

        Debug.Log($"Fetched meta URL: {metaUrl}");
        // Create and assign the download task
        Debug.Log("Creating BasisTrackedBundleWrapper...");
        BasisTrackedBundleWrapper.LoadableBundle = await BasisEncryptionToData.GenerateMetaFromFile(BasisTrackedBundleWrapper.LoadableBundle, metaFilepath, progressCallback);
        BasisTrackedBundleWrapper.metaUrl = BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncypted.MetaURL;
    }
    /// <summar>
    /// generates unique id for new data 
    /// copy just the meta data to new location
    /// decrypt it. rename file to match new 
    /// </summary>
    /// <param name="BasisLoadedBundle"></param>
    /// <param name="progressCallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<BasisLoadableBundle> DownloadAndProcessMeta(BasisLoadableBundle BasisLoadedBundle, BasisProgressReport.ProgressReport progressCallback, CancellationToken cancellationToken)
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
            BasisLoadedBundle = await BasisEncryptionToData.GenerateMetaFromFile(BasisLoadedBundle, UniqueFilePath, progressCallback);


            // Step 4: Download the bundle file
            Debug.Log($"Downloading bundle file from {BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL}");

            string FilePathMeta = BasisIOManagement.GenerateFilePath($"{BasisLoadedBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}{DecryptedMetaBasis}", AssetBundles);
            string FilePathBundle = BasisIOManagement.GenerateFilePath($"{BasisLoadedBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}{DecryptedBundleBasis}", AssetBundles);

            if (File.Exists(FilePathMeta))
            {
                File.Delete(FilePathMeta);
            }
            File.Move(UniqueFilePath, FilePathMeta);//move encrypted to match new name.

            if (BasisLoadedBundle.BasisRemoteBundleEncypted.IsLocal)
            {
                if (File.Exists(FilePathBundle))
                {
                    File.Delete(FilePathBundle);
                }
                File.Copy(BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL, FilePathBundle);//the goal here is just to get the data out
            }
            else
            {
                await BasisIOManagement.DownloadFile(BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL, FilePathBundle, progressCallback, cancellationToken);
            }
            Debug.Log($"Successfully downloaded bundle file for {BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL}");
            Debug.Log("Meta and bundle files written to disk successfully.");

            BasisLoadedBundle.BasisStoredEncyptedBundle.LocalBundleFile = FilePathBundle;
            BasisLoadedBundle.BasisStoredEncyptedBundle.LocalMetaFile = FilePathMeta;
            return BasisLoadedBundle;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during download and processing of meta: {ex.Message}");
        }
        BasisLoadedBundle.BasisBundleInformation.HasError = true;
        return BasisLoadedBundle;
    }
}