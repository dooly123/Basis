using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using UnityEngine.Networking;

public static class BasisIOManagement
{
    // Define chunk size (in bytes) for reading and writing.
    private static int chunkSize = 81920; // 80 KB
    /// <summary>
    /// downloads a file in chunks
    /// </summary>
    /// <param name="url"></param>
    /// <param name="localFilePath"></param>
    /// <param name="progressCallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task DownloadFile(string url, string localFilePath, BasisProgressReport progressCallback, CancellationToken cancellationToken = default)
    {
        BasisDebug.Log($"Starting file download from {url}");

        // Null or empty URL check
        if (string.IsNullOrWhiteSpace(url))
        {
            BasisDebug.LogError("The provided URL is null or empty.");
            return;
        }

        // Null or empty file path check
        if (string.IsNullOrWhiteSpace(localFilePath))
        {
            BasisDebug.LogError("The provided local file path is null or empty.");
            return;
        }

        // Ensure directory exists
        string directory = Path.GetDirectoryName(localFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Stream the download directly to the file
            request.downloadHandler = new DownloadHandlerFile(localFilePath)
            {
                removeFileOnAbort = true // Ensure incomplete downloads are cleaned up
            };
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            // Track download progress and handle the download
            while (!asyncOperation.isDone)
            {
                // Check if cancellation is requested
                if (cancellationToken.IsCancellationRequested)
                {
                    BasisDebug.Log("Download cancelled.");
                    request.Abort(); // Abort request on cancellation
                    return;
                }

                // Report progress (0% to 100%)
                progressCallback.ReportProgress(asyncOperation.webRequest.downloadProgress * 100, "downloading data");
                // BasisDebug.Log("downloading file " + asyncOperation.webRequest.downloadProgress);
                await Task.Yield();
            }

            // Handle potential download errors
            if (request.result != UnityWebRequest.Result.Success)
            {
                BasisDebug.LogError($"Failed to download file: {request.error} for URL {url}");
                return;
            }

            // Check if the file has been written successfully
            if (!File.Exists(localFilePath))
            {
                BasisDebug.LogError("The file was not created.");
                return;
            }

            BasisDebug.Log($"Successfully downloaded file from {url} to {localFilePath}");
        }
    }
    public static async Task<byte[]> LoadLocalFile(string filePath, BasisProgressReport progressCallback, CancellationToken cancellationToken = default)
    {
        BasisDebug.Log($"Starting file load from {filePath}");

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            BasisDebug.LogError($"File does not exist: {filePath}");
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
                    BasisDebug.Log("Load cancelled.");
                    return null;
                }

                // Report progress (0% to 100%)
                //  progressCallback?.Invoke((float)totalBytesRead / fileSize * 100);
            }
        }

        BasisDebug.Log($"Successfully loaded file from {filePath}");
        return fileData;
    }

    private static async Task WriteToFileAsync(string filePath, byte[] data, BasisProgressReport progressCallback, CancellationToken cancellationToken)
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

    public static async Task<bool> CopyFileAsync(string sourceFilePath, string destinationFilePath, BasisProgressReport Report, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(sourceFilePath))
        {
            BasisDebug.LogError($"Source file not found: {sourceFilePath}");
            return false;
        }

        try
        {
            long totalBytes = new FileInfo(sourceFilePath).Length;
            long totalBytesCopied = 0;

            // Open the source and destination file streams
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] buffer = new byte[chunkSize];
                int bytesRead;

                // Read and write chunks asynchronously
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    totalBytesCopied += bytesRead;

                    // Calculate and report progress
                    float progress = (float)totalBytesCopied / totalBytes;
                    Report.ReportProgress(progress * 100, "copying data");

                    // Allow other tasks to run
                    await Task.Yield();
                }
            }

            // After copying, delete the source file
            BasisDebug.Log($"Successfully copied file from {sourceFilePath} to {destinationFilePath}");
            return true;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("File move operation was canceled.");
            return false;
        }
        catch (Exception ex)
        {
            BasisDebug.LogError($"Error moving file: {ex.Message}");
            return false;
        }
    }
    public static string GenerateFilePath(string fileName, string subFolder)
    {
        BasisDebug.Log($"Generating folder path for {fileName} in subfolder {subFolder}");

        // Create the full folder path
        string folderPath = GenerateFolderPath(subFolder);
        // Create the full file path
        string localPath = Path.Combine(folderPath, fileName);
        BasisDebug.Log($"Generated folder path: {localPath}");

        // Return the local path
        return localPath;
    }
    public static string GenerateFolderPath(string subFolder)
    {
        BasisDebug.Log($"Generating folder path in subfolder {subFolder}");

        // Create the full folder path
        string folderPath = Path.Combine(Application.persistentDataPath, subFolder);

        // Check if the directory exists, and create it if it doesn't
        if (!Directory.Exists(folderPath))
        {
            BasisDebug.Log($"Directory {folderPath} does not exist. Creating directory.");
            Directory.CreateDirectory(folderPath);
        }
        return folderPath;
    }
}
