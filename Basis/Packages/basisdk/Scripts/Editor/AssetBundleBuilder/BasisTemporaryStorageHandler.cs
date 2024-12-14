using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TemporaryStorageHandler
{
    public static string SavePrefabToTemporaryStorage(GameObject prefab, BasisAssetBundleObject settings, ref bool wasModified, out string uniqueID)
    {
        EnsureDirectoryExists(settings.TemporaryStorage);
        uniqueID = BasisGenerateUniqueID.GenerateUniqueID();
        string prefabPath = Path.Combine(settings.TemporaryStorage, $"{uniqueID}.prefab");
        prefab = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        wasModified = true;

        return prefabPath;
    }

    public static string SaveSceneToTemporaryStorage(Scene scene, BasisAssetBundleObject settings, out string uniqueID)
    {
        EnsureDirectoryExists(settings.TemporaryStorage);
        uniqueID = BasisGenerateUniqueID.GenerateUniqueID();
        string tempScenePath = Path.Combine(settings.TemporaryStorage, uniqueID + ".unity");

        if (EditorSceneManager.SaveScene(scene, tempScenePath,true))
        {
            return tempScenePath;
        }

        return null;
    }

    /// <summary>
    /// Deletes the scene from the temporary storage if it exists.
    /// </summary>
    /// <param name="uniqueID">The unique ID of the scene to delete.</param>
    /// <param name="settings">An object containing temporary storage path settings.</param>
    /// <returns>Returns true if the file was successfully deleted, false if it does not exist or failed to delete.</returns>
    public static bool DeleteTemporaryStorageScene(string uniqueID, BasisAssetBundleObject settings)
    {
        string tempScenePath = Path.Combine(settings.TemporaryStorage, uniqueID + ".unity");

        if (File.Exists(tempScenePath))
        {
            try
            {
                File.Delete(tempScenePath);
                return true;
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to delete scene file: {ex.Message}");
                return false;
            }
        }

        Debug.LogWarning("Scene file not found.");
        return false;
    }

    public static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public static void ClearTemporaryStorage(string tempStoragePath)
    {
        if (Directory.Exists(tempStoragePath))
        {
            Directory.Delete(tempStoragePath, true);
        }
    }
}