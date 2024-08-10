using Basis.Scripts.Device_Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BasisDeviceLoaderAndSaver
{
    private const string JsonIdentifier = ".json";
    private static string JsonAllTag = "*" + JsonIdentifier;
    public static async Task<List<BasisDeviceMatchSettings>> LoadDeviceAsync(string directoryPath)
    {
        List<BasisDeviceMatchSettings> loadedDevices = new List<BasisDeviceMatchSettings>();

        if (Directory.Exists(directoryPath))
        {
            string[] jsonFiles = Directory.GetFiles(directoryPath, JsonAllTag);

            var loadTasks = jsonFiles.Select(async jsonFile =>
            {
                try
                {
                    if (File.Exists(jsonFile))
                    {
                        string jsonContent = await File.ReadAllTextAsync(jsonFile);
                        BasisDeviceMatchSettings deviceData = JsonUtility.FromJson<BasisDeviceMatchSettings>(jsonContent);
                        return deviceData;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load or parse file '{jsonFile}': {ex.Message}");
                }

                return null;
            });

            BasisDeviceMatchSettings[] results = await Task.WhenAll(loadTasks);

            loadedDevices.AddRange(results.Where(device => device != null));
        }
        else
        {
            Debug.LogError($"Directory '{directoryPath}' does not exist.");
        }

        return loadedDevices;
    }
    public static async Task SaveDevices(string directoryPath, List<BasisDeviceMatchSettings> devices)
    {
        // Safety checks
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));
        }

        if (devices == null || devices.Count == 0)
        {
            throw new ArgumentException("Devices list cannot be null or empty", nameof(devices));
        }

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        List<Task> tasks = new List<Task>();

        for (int DeviceIndex = 0; DeviceIndex < devices.Count; DeviceIndex++)
        {
            BasisDeviceMatchSettings device = devices[DeviceIndex];
            if (device == null)
            {
                continue; // Skip null devices
            }

            string filePath = Path.Combine(directoryPath, device.DeviceID + JsonIdentifier); // Assuming DeviceID is a unique identifier
            bool proceed = true;

            if (File.Exists(filePath))
            {
                try
                {
                    string loadedContent = await File.ReadAllTextAsync(filePath);
                    BasisDeviceMatchSettings loadedDevice = JsonUtility.FromJson<BasisDeviceMatchSettings>(loadedContent);

                    if (loadedDevice != null && loadedDevice.VersionNumber > device.VersionNumber)
                    {
                        proceed = false;
                    }
                    else
                    {
                        File.Delete(filePath); // Delete outdated file
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reading or deserializing file '{filePath}': {ex.Message} nuking...");
                    proceed = true; // Skip this device to avoid corrupt data
                    File.Delete(filePath); // Delete corrupted or incorrect file
                }
            }

            if (proceed)
            {
                string jsonContent = JsonUtility.ToJson(device, true);

                // Adding task to list only if proceed is true
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await File.WriteAllTextAsync(filePath, jsonContent);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error writing to file '{filePath}': {ex.Message}");
                        throw; // Rethrow to catch in WhenAll
                    }
                }));
            }
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Debug.LogError("An error occurred while saving devices: " + ex.Message);
            foreach (var t in tasks)
            {
                if (t.IsFaulted)
                {
                    Debug.LogError("Task faulted: " + t.Exception?.Message);
                }
            }
        }
    }
}
