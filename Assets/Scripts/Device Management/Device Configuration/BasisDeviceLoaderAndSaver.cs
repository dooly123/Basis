using Basis.Scripts.Device_Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BasisDeviceLoaderAndSaver
{
    private const string JsonIdentifier = ".json";
    private static readonly string JsonAllTag = "*" + JsonIdentifier; // Made readonly for optimization

    public static async Task<List<BasisDeviceMatchSettings>> LoadDeviceAsync(string directoryPath)
    {
        var loadedDevices = new List<BasisDeviceMatchSettings>();

        if (Directory.Exists(directoryPath))
        {
            string[] jsonFiles = Directory.GetFiles(directoryPath, JsonAllTag);

            var loadTasks = jsonFiles.Select(jsonFile => LoadDeviceFromFileAsync(jsonFile));
            var results = await Task.WhenAll(loadTasks);
            loadedDevices.AddRange(results.Where(device => device != null));
        }
        else
        {
            Debug.LogError($"Directory '{directoryPath}' does not exist.");
        }

        return loadedDevices;
    }

    private static async Task<BasisDeviceMatchSettings> LoadDeviceFromFileAsync(string jsonFile)
    {
        if (!File.Exists(jsonFile))
        {
            Debug.LogError($"File not found: '{jsonFile}'");
            return null;
        }

        try
        {
            using (var stream = new FileStream(jsonFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096))
            {
                StringBuilder jsonContent = new StringBuilder();
                char[] buffer = new char[4096];
                int bytesRead;

                while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                {
                    jsonContent.Append(buffer, 0, bytesRead);
                }

                // Deserialize the accumulated JSON data
                return JsonUtility.FromJson<BasisDeviceMatchSettings>(jsonContent.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load or parse file '{jsonFile}': {ex.Message}");
            return null;
        }
    }

    public static async Task SaveDevices(string directoryPath, List<BasisDeviceMatchSettings> devices)
    {
        // Safety checks
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));

        if (devices == null || devices.Count == 0)
            throw new ArgumentException("Devices list cannot be null or empty", nameof(devices));

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        // Use a fixed-size array instead of a List to avoid resizing overhead
        Task[] tasks = new Task[devices.Count];
        for (int i = 0; i < devices.Count; i++)
        {
            var device = devices[i];
            if (device == null) continue;

            string filePath = Path.Combine(directoryPath, device.DeviceID + JsonIdentifier);
            tasks[i] = Task.Run(() => SaveDeviceAsync(filePath, device));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred while saving devices: {ex.Message}");
        }
    }

    private static async Task SaveDeviceAsync(string filePath, BasisDeviceMatchSettings device)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string loadedContent;
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    loadedContent = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                var existingDevice = JsonUtility.FromJson<BasisDeviceMatchSettings>(loadedContent);

                if (existingDevice != null && existingDevice.VersionNumber > device.VersionNumber)
                {
                    Debug.Log("Newer version exists; using that instead");
                    return;
                }
                else
                {
                    Debug.Log("Old file detected");
                    File.Delete(filePath); // Delete outdated file
                }
            }

            var jsonContent = JsonUtility.ToJson(device, false); // Use compact JSON format to reduce file size
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(jsonContent).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing file '{filePath}': {ex.Message}");
            if (File.Exists(filePath))
            {
                File.Delete(filePath); // Delete corrupted or incorrect file
            }
            throw; // Rethrow to catch in WhenAll
        }
    }
}