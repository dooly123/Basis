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
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        List<Task> tasks = new List<Task>();

        foreach (BasisDeviceMatchSettings device in devices)
        {
            string jsonContent = JsonUtility.ToJson(device, true);
            string filePath = Path.Combine(directoryPath, device.DeviceID + JsonIdentifier); // Assuming DeviceID is a unique identifier

            Task writeTask = File.WriteAllTextAsync(filePath, jsonContent);
            tasks.Add(writeTask);
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
