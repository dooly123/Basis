using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static Basis.Scripts.Addressable_Driver.Loading.AddressableManagement;
public static class BasisBundleManagement
{
    public static ConcurrentDictionary<string, BasisLoadedBundle> LoadableBundles = new ConcurrentDictionary<string, BasisLoadedBundle>();
    // Dictionary to track ongoing downloads keyed by MetaURL
    private static ConcurrentDictionary<string, Task<BasisBundleInformation>> OnGoingDownloads = new ConcurrentDictionary<string, Task<BasisBundleInformation>>();
    public static bool FindBundle(BasisBundleInformation BasisBundleInformation)
    {
        if (LoadableBundles.ContainsKey(BasisBundleInformation.BasisBundleGenerated.AssetToLoadName))
        {
            return true;
        }
        return false;
    }
    public static void LoadAllOnDisc()
    {

    }
    public static async Task DownloadMetaToDisc(BasisLoadedBundle BasisLoadedBundle, ProgressReport progressCallback, CancellationToken cancellationToken)
    {
        string metaUrl = BasisLoadedBundle.BasisRemoteBundleEncypted.MetaURL;

        // Check if there's an ongoing or completed download for this MetaURL
        if (!OnGoingDownloads.TryGetValue(metaUrl, out var downloadTask))
        {
            // No ongoing download, so start a new one
            downloadTask = DownloadAndProcessMeta(BasisLoadedBundle, progressCallback, cancellationToken);
            OnGoingDownloads.TryAdd(metaUrl, downloadTask);
        }

        // Await the task (this will wait for the first download if it's still in progress)
        BasisBundleInformation bundleInfo = await downloadTask;

        // Handle the result or failure (if bundleInfo is null, there was an error)
        if (bundleInfo.HasError)
        {
            Debug.LogError($"Failed to download and process meta for {metaUrl}");
            return;
        }

        // Update the LoadableBundles dictionary after download completes
        LoadableBundles.TryAdd(bundleInfo.BasisBundleGenerated.AssetToLoadName, BasisLoadedBundle);
    }

    private static async Task<BasisBundleInformation> DownloadAndProcessMeta(BasisLoadedBundle BasisLoadedBundle, ProgressReport progressCallback, CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Download the meta file
            byte[] LoadedLocalMetaData = await DownloadFile(BasisLoadedBundle.BasisLocalBundleEncypted.LocalMetaFile, progressCallback, cancellationToken);
            if (LoadedLocalMetaData == null)
            {
                Debug.LogError("Unable to download meta for " + BasisLoadedBundle.BasisLocalBundleEncypted.LocalMetaFile);
                return new BasisBundleInformation() {HasError = true};
            }

            // Step 2: Decrypt the meta file
            byte[] loadedlocalmeta = await BasisEncryptionWrapper.DecryptBytesAsync(LoadedLocalMetaData, BasisLoadedBundle.UnlockPassword);
            if (loadedlocalmeta == null)
            {
                Debug.LogError("Unable to load meta for " + BasisLoadedBundle.BasisLocalBundleEncypted.LocalMetaFile);
                return new BasisBundleInformation() { HasError = true };
            }

            // Step 3: Convert the decrypted meta to a BasisBundleInformation object
            BasisBundleInformation BasisBundleInformation = ConvertBytesToJson(loadedlocalmeta);

            // Step 4: Download the bundle file
            byte[] LoadedBundleData = await DownloadFile(BasisLoadedBundle.BasisLocalBundleEncypted.LocalBundleFile, progressCallback, cancellationToken);

            // Step 5: Write meta and bundle to disk
            string FilePathMeta = GenerateFolderPath(BasisBundleInformation.BasisBundleGenerated.AssetToLoadName + ".DecryptedMetaBasis", "/AssetBundles");
            string FilePathBundle = GenerateFolderPath(BasisBundleInformation.BasisBundleGenerated.AssetToLoadName + ".EncyptedBundleBasis", "/AssetBundles");

            await WriteToFileAsync(FilePathMeta, loadedlocalmeta, progressCallback, cancellationToken);
            await WriteToFileAsync(FilePathBundle, LoadedBundleData, progressCallback, cancellationToken);

            // Return the processed bundle information
            return BasisBundleInformation;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during download and process of meta: {ex.Message}");
            return new BasisBundleInformation() { HasError = true };
        }
    }

    public static string GenerateFolderPath(string FileName, string SubFolder)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, SubFolder);
        if (Directory.Exists(folderPath) == false)
        {
            Directory.CreateDirectory(folderPath);
        }
        string localPath = Path.Combine(folderPath, FileName);
        return localPath;
    }

    public static BasisBundleInformation ConvertBytesToJson(byte[] loadedlocalmeta)
    {
        // Convert the byte array to a JSON string (assuming UTF-8 encoding)
        string jsonString = Encoding.UTF8.GetString(loadedlocalmeta);

        // Deserialize the JSON string into a BasisBundleInformation object
        BasisBundleInformation bundleInfo = JsonUtility.FromJson<BasisBundleInformation>(jsonString);
        return bundleInfo;
    }

    public static async Task<byte[]> DownloadFile(string url, ProgressReport progressCallback, CancellationToken cancellationToken = default)
    {
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
                Debug.LogError($"Failed to download file: {request.error} for url {url}");
                return null;
            }

            try
            {
                // Asynchronously write the downloaded file to disk
                byte[] fileData = request.downloadHandler.data;
                return fileData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving file to disk: {ex.Message}");
                return null;
            }
        }
    }

    private static async Task WriteToFileAsync(string filePath, byte[] data, ProgressReport progressCallback, CancellationToken cancellationToken)
    {
        // Ensure the directory exists
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Write the file data asynchronously
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        {
            int totalBytes = data.Length;
            int bytesWritten = 0;

            // Write data in chunks for better progress reporting
            const int chunkSize = 8192; // 8KB per chunk
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
                progressCallback?.Invoke(progress);
            }
        }
    }
}