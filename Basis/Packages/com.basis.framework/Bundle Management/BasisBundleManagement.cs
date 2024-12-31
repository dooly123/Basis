using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public static class BasisBundleManagement
{
    public static string MetaLinkBasisSuffix =  ".MetaLinkBasis";
    public static string EncryptedMetaBasisSuffix = ".EncryptedMetaBasis";
    public static string EncryptedBundleBasisSuffix = ".EncryptedBundleBasis";
    public static string AssetBundlesFolder = "AssetBundles";
    public static string LockedBundlesFolder = "LockedBundles";

    // Dictionary to track ongoing downloads keyed by MetaURL
    public static BasisProgressReport FindAllBundlesReport = new BasisProgressReport();
    public static async Task DownloadStoreMetaAndBundle(BasisTrackedBundleWrapper BasisTrackedBundleWrapper, BasisProgressReport progressCallback, CancellationToken cancellationToken)
    {
        if (BasisTrackedBundleWrapper == null)
        {
            Debug.LogError("BasisTrackedBundleWrapper is null.");
            return;
        }

        if (BasisTrackedBundleWrapper.LoadableBundle == null || BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted == null)
        {
            Debug.LogError("LoadableBundle or BasisRemoteBundleEncypted is null.");
            return;
        }

        string metaUrl = BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL;

        if (string.IsNullOrEmpty(metaUrl))
        {
            Debug.LogError("MetaURL is null or empty.");
            return;
        }

        Debug.Log($"Starting download process for {metaUrl}");

        try
        {
            Debug.Log($"Downloading meta file for {metaUrl}");

            string UniqueDownload = BasisGenerateUniqueID.GenerateUniqueID();
            if (string.IsNullOrEmpty(UniqueDownload))
            {
                Debug.LogError("Failed to generate a unique ID.");
                return;
            }

            string UniqueFilePath = BasisIOManagement.GenerateFilePath($"Temp_{UniqueDownload}{EncryptedMetaBasisSuffix}", LockedBundlesFolder);
            if (string.IsNullOrEmpty(UniqueFilePath))
            {
                Debug.LogError("Failed to generate file path for the unique file.");
                return;
            }

            if (File.Exists(UniqueFilePath))
            {
                File.Delete(UniqueFilePath);
            }

            if (BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.IsLocal)
            {
                if (!File.Exists(BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL))
                {
                    Debug.LogError($"Local meta file not found: {metaUrl}");
                    return;
                }

                File.Copy(BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL, UniqueFilePath); // The goal here is just to get the data out
            }
            else
            {
                await BasisIOManagement.DownloadFile(metaUrl, UniqueFilePath, progressCallback, cancellationToken);
            }

            Debug.Log($"Successfully downloaded meta file for {metaUrl} Decrypting meta file...");

            BasisTrackedBundleWrapper.LoadableBundle = await BasisEncryptionToData.GenerateMetaFromFile(BasisTrackedBundleWrapper.LoadableBundle, UniqueFilePath, progressCallback);

            if (BasisTrackedBundleWrapper.LoadableBundle == null)
            {
                Debug.LogError("Failed to decrypt meta file, LoadableBundle is null.");
                return;
            }

            // Step 4: Download the bundle file
            string bundleUrl = BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.BundleURL;

            if (string.IsNullOrEmpty(bundleUrl))
            {
                Debug.LogError("BundleURL is null or empty.");
                return;
            }

            Debug.Log($"Downloading bundle file from {bundleUrl}");

            string FilePathMeta = BasisIOManagement.GenerateFilePath($"{BasisTrackedBundleWrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}{EncryptedMetaBasisSuffix}", AssetBundlesFolder);
            string FilePathBundle = BasisIOManagement.GenerateFilePath($"{BasisTrackedBundleWrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}{EncryptedBundleBasisSuffix}", AssetBundlesFolder);

            if (string.IsNullOrEmpty(FilePathMeta) || string.IsNullOrEmpty(FilePathBundle))
            {
                Debug.LogError("Failed to generate file paths for meta or bundle.");
                return;
            }

            if (File.Exists(FilePathMeta))
            {
                File.Delete(FilePathMeta);
            }

            File.Move(UniqueFilePath, FilePathMeta); // Move encrypted file to match new name.

            if (BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.IsLocal)
            {
                if (!File.Exists(bundleUrl))
                {
                    Debug.LogError($"Local bundle file not found: {bundleUrl}");
                    return;
                }

                if (File.Exists(FilePathBundle))
                {
                    File.Delete(FilePathBundle);
                }

                File.Copy(bundleUrl, FilePathBundle); // The goal here is just to get the data out
            }
            else
            {
                await BasisIOManagement.DownloadFile(bundleUrl, FilePathBundle, progressCallback, cancellationToken);
            }

            Debug.Log($"Successfully downloaded bundle file for {bundleUrl}");
            BasisTrackedBundleWrapper.LoadableBundle.BasisLocalEncryptedBundle.LocalBundleFile = FilePathBundle;
            BasisTrackedBundleWrapper.LoadableBundle.BasisLocalEncryptedBundle.LocalMetaFile = FilePathMeta;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during download and processing of meta: {ex.Message}");
            BasisTrackedBundleWrapper.LoadableBundle.BasisBundleInformation.HasError = true;
        }
    }
    public static async Task DownloadAndSaveMetaFile(BasisTrackedBundleWrapper BasisTrackedBundleWrapper, BasisProgressReport progressCallback, CancellationToken cancellationToken)
    {
        if (BasisTrackedBundleWrapper == null)
        {
            Debug.LogError("BasisTrackedBundleWrapper is null.");
            return;
        }

        if (BasisTrackedBundleWrapper.LoadableBundle == null || BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted == null)
        {
            Debug.LogError("LoadableBundle or BasisRemoteBundleEncrypted is null.");
            return;
        }

        string metaUrl = BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL;

        if (string.IsNullOrEmpty(metaUrl))
        {
            Debug.LogError("MetaURL is null or empty.");
            return;
        }

        Debug.Log($"Starting download process for {metaUrl}");

        try
        {
            Debug.Log($"Downloading meta file for {metaUrl}");

            string UniqueDownload = BasisGenerateUniqueID.GenerateUniqueID();
            if (string.IsNullOrEmpty(UniqueDownload))
            {
                Debug.LogError("Failed to generate a unique ID.");
                return;
            }

            string UniqueFilePath = BasisIOManagement.GenerateFilePath($"{UniqueDownload}{EncryptedMetaBasisSuffix}", LockedBundlesFolder);
            if (string.IsNullOrEmpty(UniqueFilePath))
            {
                Debug.LogError("Failed to generate file path for the unique file.");
                return;
            }

            if (File.Exists(UniqueFilePath))
            {
                File.Delete(UniqueFilePath);
            }

            if (BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.IsLocal)
            {
                if (!File.Exists(BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL))
                {
                    Debug.LogError($"Local meta file not found: {metaUrl}");
                    return;
                }

                File.Copy(BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL, UniqueFilePath); // The goal here is just to get the data out
            }
            else
            {
                await BasisIOManagement.DownloadFile(metaUrl, UniqueFilePath, progressCallback, cancellationToken);
            }

            Debug.Log($"Successfully downloaded meta file for {metaUrl}. Decrypting meta file...");

            BasisTrackedBundleWrapper.LoadableBundle = await BasisEncryptionToData.GenerateMetaFromFile(BasisTrackedBundleWrapper.LoadableBundle, UniqueFilePath, progressCallback);

            if (BasisTrackedBundleWrapper.LoadableBundle == null)
            {
                Debug.LogError("Failed to decrypt meta file, LoadableBundle is null.");
                return;
            }

            // Move the meta file to its final destination
            string FilePathMeta = BasisIOManagement.GenerateFilePath($"{BasisTrackedBundleWrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}{EncryptedMetaBasisSuffix}", AssetBundlesFolder);

            if (string.IsNullOrEmpty(FilePathMeta))
            {
                Debug.LogError("Failed to generate file path for the meta file.");
                return;
            }

            if (File.Exists(FilePathMeta))
            {
                File.Delete(FilePathMeta);
            }

            File.Move(UniqueFilePath, FilePathMeta); // Move encrypted file to match new name.

            BasisTrackedBundleWrapper.LoadableBundle.BasisLocalEncryptedBundle.LocalMetaFile = FilePathMeta;

            Debug.Log($"Meta file saved successfully at {FilePathMeta}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during download and processing of meta: {ex.Message}");
            BasisTrackedBundleWrapper.LoadableBundle.BasisBundleInformation.HasError = true;
        }
    }
    public static async Task ProcessOnDiscMetaDataAsync(BasisTrackedBundleWrapper basisTrackedBundleWrapper, BasisStoredEncyptedBundle BasisStoredEncyptedBundle,BasisProgressReport progressCallback,CancellationToken cancellationToken)
    {
        // Log entry point
        Debug.Log("Starting DataOnDiscProcessMeta method...");

        // Parameter validation with detailed logging
        if (BasisStoredEncyptedBundle == null)
        {
            Debug.LogError("BasisTrackedBundleWrapper is null. Exiting method.");
            return;
        }
        // Parameter validation with detailed logging
        if (basisTrackedBundleWrapper == null)
        {
            Debug.LogError("BasisTrackedBundleWrapper is null. Exiting method.");
            return;
        }

        // Validate nested objects in BasisTrackedBundleWrapper
        if (basisTrackedBundleWrapper.LoadableBundle == null)
        {
            Debug.LogError("LoadableBundle inside BasisTrackedBundleWrapper is null. Exiting method.");
            return;
        }

        if (basisTrackedBundleWrapper.LoadableBundle.BasisLocalEncryptedBundle == null)
        {
            Debug.LogError("BasisStoredEncyptedBundle inside LoadableBundle is null. Exiting method.");
            return;
        }

        if (basisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted == null)
        {
            Debug.LogError("BasisRemoteBundleEncypted inside LoadableBundle is null. Exiting method.");
            return;
        }

        // Set local paths
        Debug.Log($"Setting local bundle file: {BasisStoredEncyptedBundle.LocalBundleFile} Setting local meta file: {BasisStoredEncyptedBundle.LocalMetaFile}");

        basisTrackedBundleWrapper.LoadableBundle.BasisLocalEncryptedBundle = BasisStoredEncyptedBundle;

        // Fetching the meta URL
        string metaUrl = basisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL;
        if (string.IsNullOrEmpty(metaUrl))
        {
            Debug.LogError("MetaURL is null or empty. Exiting method.");
            return;
        }

        Debug.Log($"Fetched meta URL: {metaUrl}");

        // Create and assign the download task
        Debug.Log("Creating BasisTrackedBundleWrapper... at path: " + BasisStoredEncyptedBundle.LocalMetaFile);

        var loadableBundle = basisTrackedBundleWrapper.LoadableBundle;
        if (loadableBundle == null)
        {
            Debug.LogError("Failed to retrieve LoadableBundle from BasisTrackedBundleWrapper.");
            return;
        }

        basisTrackedBundleWrapper.LoadableBundle = await BasisEncryptionToData.GenerateMetaFromFile(loadableBundle, BasisStoredEncyptedBundle.LocalMetaFile, progressCallback);

        if (basisTrackedBundleWrapper.LoadableBundle == null)
        {
            Debug.LogError("Failed to generate meta from file.");
        }
        else
        {
            Debug.Log("Successfully processed the meta file.");
        }
    }
}