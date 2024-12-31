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
            BasisDebug.LogError("BasisTrackedBundleWrapper is null.");
            return;
        }

        if (BasisTrackedBundleWrapper.LoadableBundle == null || BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted == null)
        {
            BasisDebug.LogError("LoadableBundle or BasisRemoteBundleEncypted is null.");
            return;
        }

        string metaUrl = BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL;

        if (string.IsNullOrEmpty(metaUrl))
        {
            BasisDebug.LogError("MetaURL is null or empty.");
            return;
        }

        BasisDebug.Log($"Starting download process for {metaUrl}");

        try
        {
            BasisDebug.Log($"Downloading meta file for {metaUrl}");

            string UniqueDownload = BasisGenerateUniqueID.GenerateUniqueID();
            if (string.IsNullOrEmpty(UniqueDownload))
            {
                BasisDebug.LogError("Failed to generate a unique ID.");
                return;
            }

            string UniqueFilePath = BasisIOManagement.GenerateFilePath($"Temp_{UniqueDownload}{EncryptedMetaBasisSuffix}", LockedBundlesFolder);
            if (string.IsNullOrEmpty(UniqueFilePath))
            {
                BasisDebug.LogError("Failed to generate file path for the unique file.");
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
                    BasisDebug.LogError($"Local meta file not found: {metaUrl}");
                    return;
                }

                File.Copy(BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL, UniqueFilePath); // The goal here is just to get the data out
            }
            else
            {
                await BasisIOManagement.DownloadFile(metaUrl, UniqueFilePath, progressCallback, cancellationToken);
            }

            BasisDebug.Log($"Successfully downloaded meta file for {metaUrl} Decrypting meta file...");

            BasisTrackedBundleWrapper.LoadableBundle = await BasisEncryptionToData.GenerateMetaFromFile(BasisTrackedBundleWrapper.LoadableBundle, UniqueFilePath, progressCallback);

            if (BasisTrackedBundleWrapper.LoadableBundle == null)
            {
                BasisDebug.LogError("Failed to decrypt meta file, LoadableBundle is null.");
                return;
            }

            // Step 4: Download the bundle file
            string bundleUrl = BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.BundleURL;

            if (string.IsNullOrEmpty(bundleUrl))
            {
                BasisDebug.LogError("BundleURL is null or empty.");
                return;
            }

            BasisDebug.Log($"Downloading bundle file from {bundleUrl}");

            string FilePathMeta = BasisIOManagement.GenerateFilePath($"{BasisTrackedBundleWrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}{EncryptedMetaBasisSuffix}", AssetBundlesFolder);
            string FilePathBundle = BasisIOManagement.GenerateFilePath($"{BasisTrackedBundleWrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}{EncryptedBundleBasisSuffix}", AssetBundlesFolder);

            if (string.IsNullOrEmpty(FilePathMeta) || string.IsNullOrEmpty(FilePathBundle))
            {
                BasisDebug.LogError("Failed to generate file paths for meta or bundle.");
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
                    BasisDebug.LogError($"Local bundle file not found: {bundleUrl}");
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

            BasisDebug.Log($"Successfully downloaded bundle file for {bundleUrl}");
            BasisTrackedBundleWrapper.LoadableBundle.BasisLocalEncryptedBundle.LocalBundleFile = FilePathBundle;
            BasisTrackedBundleWrapper.LoadableBundle.BasisLocalEncryptedBundle.LocalMetaFile = FilePathMeta;
        }
        catch (Exception ex)
        {
            BasisDebug.LogError($"Error during download and processing of meta: {ex.Message}");
            BasisTrackedBundleWrapper.LoadableBundle.BasisBundleInformation.HasError = true;
        }
    }
    public static async Task DownloadAndSaveMetaFile(BasisTrackedBundleWrapper BasisTrackedBundleWrapper, BasisProgressReport progressCallback, CancellationToken cancellationToken)
    {
        if (BasisTrackedBundleWrapper == null)
        {
            BasisDebug.LogError("BasisTrackedBundleWrapper is null.");
            return;
        }

        if (BasisTrackedBundleWrapper.LoadableBundle == null || BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted == null)
        {
            BasisDebug.LogError("LoadableBundle or BasisRemoteBundleEncrypted is null.");
            return;
        }

        string metaUrl = BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL;

        if (string.IsNullOrEmpty(metaUrl))
        {
            BasisDebug.LogError("MetaURL is null or empty.");
            return;
        }

        BasisDebug.Log($"Starting download process for {metaUrl}");

        try
        {
            BasisDebug.Log($"Downloading meta file for {metaUrl}");

            string UniqueDownload = BasisGenerateUniqueID.GenerateUniqueID();
            if (string.IsNullOrEmpty(UniqueDownload))
            {
                BasisDebug.LogError("Failed to generate a unique ID.");
                return;
            }

            string UniqueFilePath = BasisIOManagement.GenerateFilePath($"{UniqueDownload}{EncryptedMetaBasisSuffix}", LockedBundlesFolder);
            if (string.IsNullOrEmpty(UniqueFilePath))
            {
                BasisDebug.LogError("Failed to generate file path for the unique file.");
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
                    BasisDebug.LogError($"Local meta file not found: {metaUrl}");
                    return;
                }

                File.Copy(BasisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL, UniqueFilePath); // The goal here is just to get the data out
            }
            else
            {
                await BasisIOManagement.DownloadFile(metaUrl, UniqueFilePath, progressCallback, cancellationToken);
            }

            BasisDebug.Log($"Successfully downloaded meta file for {metaUrl}. Decrypting meta file...");

            BasisTrackedBundleWrapper.LoadableBundle = await BasisEncryptionToData.GenerateMetaFromFile(BasisTrackedBundleWrapper.LoadableBundle, UniqueFilePath, progressCallback);

            if (BasisTrackedBundleWrapper.LoadableBundle == null)
            {
                BasisDebug.LogError("Failed to decrypt meta file, LoadableBundle is null.");
                return;
            }

            // Move the meta file to its final destination
            string FilePathMeta = BasisIOManagement.GenerateFilePath($"{BasisTrackedBundleWrapper.LoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName}{EncryptedMetaBasisSuffix}", AssetBundlesFolder);

            if (string.IsNullOrEmpty(FilePathMeta))
            {
                BasisDebug.LogError("Failed to generate file path for the meta file.");
                return;
            }

            if (File.Exists(FilePathMeta))
            {
                File.Delete(FilePathMeta);
            }

            File.Move(UniqueFilePath, FilePathMeta); // Move encrypted file to match new name.

            BasisTrackedBundleWrapper.LoadableBundle.BasisLocalEncryptedBundle.LocalMetaFile = FilePathMeta;

            BasisDebug.Log($"Meta file saved successfully at {FilePathMeta}");
        }
        catch (Exception ex)
        {
            BasisDebug.LogError($"Error during download and processing of meta: {ex.Message}");
            BasisTrackedBundleWrapper.LoadableBundle.BasisBundleInformation.HasError = true;
        }
    }
    public static async Task ProcessOnDiscMetaDataAsync(BasisTrackedBundleWrapper basisTrackedBundleWrapper, BasisStoredEncyptedBundle BasisStoredEncyptedBundle,BasisProgressReport progressCallback,CancellationToken cancellationToken)
    {
        // Log entry point
        BasisDebug.Log("Starting DataOnDiscProcessMeta method...");

        // Parameter validation with detailed logging
        if (BasisStoredEncyptedBundle == null)
        {
            BasisDebug.LogError("BasisTrackedBundleWrapper is null. Exiting method.");
            return;
        }
        // Parameter validation with detailed logging
        if (basisTrackedBundleWrapper == null)
        {
            BasisDebug.LogError("BasisTrackedBundleWrapper is null. Exiting method.");
            return;
        }

        // Validate nested objects in BasisTrackedBundleWrapper
        if (basisTrackedBundleWrapper.LoadableBundle == null)
        {
            BasisDebug.LogError("LoadableBundle inside BasisTrackedBundleWrapper is null. Exiting method.");
            return;
        }

        if (basisTrackedBundleWrapper.LoadableBundle.BasisLocalEncryptedBundle == null)
        {
            BasisDebug.LogError("BasisStoredEncyptedBundle inside LoadableBundle is null. Exiting method.");
            return;
        }

        if (basisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted == null)
        {
            BasisDebug.LogError("BasisRemoteBundleEncypted inside LoadableBundle is null. Exiting method.");
            return;
        }

        // Set local paths
        BasisDebug.Log($"Setting local bundle file: {BasisStoredEncyptedBundle.LocalBundleFile} Setting local meta file: {BasisStoredEncyptedBundle.LocalMetaFile}");

        basisTrackedBundleWrapper.LoadableBundle.BasisLocalEncryptedBundle = BasisStoredEncyptedBundle;

        // Fetching the meta URL
        string metaUrl = basisTrackedBundleWrapper.LoadableBundle.BasisRemoteBundleEncrypted.MetaURL;
        if (string.IsNullOrEmpty(metaUrl))
        {
            BasisDebug.LogError("MetaURL is null or empty. Exiting method.");
            return;
        }

        BasisDebug.Log($"Fetched meta URL: {metaUrl}");

        // Create and assign the download task
        BasisDebug.Log("Creating BasisTrackedBundleWrapper... at path: " + BasisStoredEncyptedBundle.LocalMetaFile);

        var loadableBundle = basisTrackedBundleWrapper.LoadableBundle;
        if (loadableBundle == null)
        {
            BasisDebug.LogError("Failed to retrieve LoadableBundle from BasisTrackedBundleWrapper.");
            return;
        }

        basisTrackedBundleWrapper.LoadableBundle = await BasisEncryptionToData.GenerateMetaFromFile(loadableBundle, BasisStoredEncyptedBundle.LocalMetaFile, progressCallback);

        if (basisTrackedBundleWrapper.LoadableBundle == null)
        {
            BasisDebug.LogError("Failed to generate meta from file.");
        }
        else
        {
            BasisDebug.Log("Successfully processed the meta file.");
        }
    }
}