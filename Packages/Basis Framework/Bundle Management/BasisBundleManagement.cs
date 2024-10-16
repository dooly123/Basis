using BasisSerializer.OdinSerializer;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class BasisBundleManagement
{
    public static ConcurrentDictionary<string, BasisLoadableBundle> LoadableBundles = new ConcurrentDictionary<string, BasisLoadableBundle>();

    // Dictionary to track ongoing downloads keyed by MetaURL
    private static ConcurrentDictionary<string, Task<BasisBundleInformation>> OnGoingDownloads = new ConcurrentDictionary<string, Task<BasisBundleInformation>>();
    // Write data in chunks for better progress reporting
    const int chunkSize = 16 * 1024 * 1024; // 16 MB per chunk
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

    private static async Task<BasisBundleInformation> DownloadAndProcessMeta(BasisLoadableBundle BasisLoadedBundle, BasisProgressReport.ProgressReport progressCallback, CancellationToken cancellationToken)
    {
        try
        {
            Debug.Log($"Downloading meta file for {BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL}");
            byte[] LoadedLocalMetaData;
            // Step 1: Download the meta file
            if (BasisLoadedBundle.BasisRemoteBundleEncypted.IsLocal == false)
            {
                LoadedLocalMetaData = await DownloadFile(BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL, progressCallback, cancellationToken);
            }
            else
            {
                LoadedLocalMetaData = await LoadLocalFile(BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL, progressCallback, cancellationToken);
            }
            if (LoadedLocalMetaData == null)
            {
                Debug.LogError($"Unable to download meta for {BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL}");
                return new BasisBundleInformation() { HasError = true };
            }
            Debug.Log($"Successfully downloaded meta file for {BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL}" + " Size " + LoadedLocalMetaData.Length);

            // Step 2: Decrypt the meta file
            Debug.Log("Decrypting meta file...");
            byte[] loadedlocalmeta = await BasisEncryptionWrapper.DecryptDataAsync(LoadedLocalMetaData, BasisLoadedBundle.UnlockPassword);
            if (loadedlocalmeta == null)
            {
                Debug.LogError($"Unable to decrypt meta for {BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL}");
                return new BasisBundleInformation() { HasError = true };
            }
            // Step 3: Convert the decrypted meta to a BasisBundleInformation object
            Debug.Log("Converting decrypted meta file to BasisBundleInformation...");
            BasisBundleInformation BasisBundleInformation = ConvertBytesToJson(loadedlocalmeta);

            // Step 4: Download the bundle file
            Debug.Log($"Downloading bundle file from {BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL}");

            byte[] LoadedBundleData;
            if (BasisLoadedBundle.BasisRemoteBundleEncypted.IsLocal == false)
            {
                LoadedBundleData = await DownloadFile(BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL, progressCallback, cancellationToken);
            }
            else
            {
                LoadedBundleData = await LoadLocalFile(BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL, progressCallback, cancellationToken);
            }
            if (LoadedBundleData == null)
            {
                Debug.LogError($"Unable to download bundle file for {BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL}");
                return new BasisBundleInformation() { HasError = true };
            }
            Debug.Log($"Successfully downloaded bundle file for {BasisLoadedBundle.BasisRemoteBundleEncypted.BundleURL}");

            // Step 5: Write meta and bundle to disk
            Debug.Log("Writing meta and bundle files to disk...");

            string FilePathMeta = GenerateFolderPath(BasisBundleInformation.BasisBundleGenerated.AssetToLoadName + ".DecryptedMetaBasis", "AssetBundles");
            string FilePathBundle = GenerateFolderPath(BasisBundleInformation.BasisBundleGenerated.AssetToLoadName + ".DecryptedBundleBasis", "AssetBundles");

            await WriteToFileAsync(FilePathMeta, loadedlocalmeta, progressCallback, cancellationToken);
            await WriteToFileAsync(FilePathBundle, LoadedBundleData, progressCallback, cancellationToken);
            BasisBundleInformation.BasisStoredDecyptedBundle = new BasisStoredDecyptedBundle
            {
                LocalMetaFile = FilePathMeta,
                LocalBundleFile = FilePathBundle
            };
            Debug.Log("Meta and bundle files written to disk successfully.");
            return BasisBundleInformation;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during download and processing of meta: {ex.Message}");
        }
        return new BasisBundleInformation() { HasError = true };
    }

    public static string GenerateFolderPath(string fileName, string subFolder)
    {
        Debug.Log($"Generating folder path for {fileName} in subfolder {subFolder}");

        // Create the full folder path
        string folderPath = Path.Combine(Application.persistentDataPath, subFolder);

        // Check if the directory exists, and create it if it doesn't
        if (!Directory.Exists(folderPath))
        {
            Debug.Log($"Directory {folderPath} does not exist. Creating directory.");
            Directory.CreateDirectory(folderPath);
        }

        // Create the full file path
        string localPath = Path.Combine(folderPath, fileName);
        Debug.Log($"Generated folder path: {localPath}");

        // Return the local path
        return localPath;
    }

    public static BasisBundleInformation ConvertBytesToJson(byte[] loadedlocalmeta)
    {
        if (loadedlocalmeta == null || loadedlocalmeta.Length == 0)
        {
            Debug.LogError($"Data for {nameof(BasisBundleInformation)} is empty or null.");
            return new BasisBundleInformation() { HasError = true };
        }

        // Convert the byte array to a JSON string (assuming UTF-8 encoding)
        Debug.Log($"Converting byte array to JSON string...");
        BasisBundleInformation Information = SerializationUtility.DeserializeValue<BasisBundleInformation>(loadedlocalmeta, DataFormat.JSON);
        return Information;
    }

    public static async Task<byte[]> DownloadFile(string url, BasisProgressReport.ProgressReport progressCallback, CancellationToken cancellationToken = default)
    {
        Debug.Log($"Starting file download from {url}");
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            var asyncOperation = request.SendWebRequest();

            // Track download progress (0% to 50% during download)
            while (!asyncOperation.isDone)
            {
                // Check if cancellation is requested
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Download cancelled.");
                    request.Abort(); // Abort request on cancellation
                    return null;
                }

                progressCallback?.Invoke(asyncOperation.progress * 50); // Progress from 0 to 50
                await Task.Yield();
            }

            // Handle potential download errors
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download file: {request.error} for URL {url}");
                return null;
            }

            Debug.Log($"Successfully downloaded file from {url}");
            return request.downloadHandler.data;
        }
    }
    public static async Task<byte[]> LoadLocalFile(string filePath, BasisProgressReport.ProgressReport progressCallback, CancellationToken cancellationToken = default)
    {
        Debug.Log($"Starting file load from {filePath}");

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File does not exist: {filePath}");
            return null;
        }

        long fileSize = new FileInfo(filePath).Length;
        byte[] fileData = new byte[fileSize];

        // Open the file stream for reading
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
        {
            long totalBytesRead = 0;
            int bytesRead;

            // Read the file in chunks
            while ((bytesRead = await fileStream.ReadAsync(fileData, (int)totalBytesRead, (int)(fileSize - totalBytesRead), cancellationToken)) > 0)
            {
                totalBytesRead += bytesRead;

                // Check if cancellation is requested
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Load cancelled.");
                    return null;
                }

                // Report progress (0% to 100%)
              //  progressCallback?.Invoke((float)totalBytesRead / fileSize * 100);
            }
        }

        Debug.Log($"Successfully loaded file from {filePath}");
        return fileData;
    }

    private static async Task WriteToFileAsync(string filePath, byte[] data, BasisProgressReport.ProgressReport progressCallback, CancellationToken cancellationToken)
    {
        // Ensure the directory exists
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        // Write the file data asynchronously on a separate thread
        await Task.Run(async () =>
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                int totalBytes = data.Length;
                int bytesWritten = 0;

                while (bytesWritten < totalBytes)
                {
                    // Check for cancellation
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Debug.LogWarning("File write operation cancelled.");
                        return;
                    }

                    int bytesToWrite = Math.Min(chunkSize, totalBytes - bytesWritten);
                    await fileStream.WriteAsync(data, bytesWritten, bytesToWrite, cancellationToken);
                    bytesWritten += bytesToWrite;

                    // Report file write progress (from 50% to 100%)
                    float progress = 50 + ((float)bytesWritten / totalBytes) * 50;
                }
            }
        });
    }
}