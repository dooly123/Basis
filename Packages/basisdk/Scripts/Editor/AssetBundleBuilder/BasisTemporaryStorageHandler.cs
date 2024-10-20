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

        if (EditorSceneManager.SaveScene(scene, tempScenePath))
        {
            return tempScenePath;
        }

        return null;
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