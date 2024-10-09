using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BasisAssetBundleBuilder : MonoBehaviour
{
    public static void BuildAssetBundle(GameObject prefab, BasisAssetBundleObject BundleAsset)
    {
        LogMessage("Starting AssetBundle build process for prefab.");

        if (!IsValidPrefab(prefab))
        {
            LogError("Invalid prefab. AssetBundle build aborted.");
            return;
        }

        bool wasModified = false;

        try
        {
            LogMessage($"Prefab '{prefab.name}' is valid. Saving to temporary storage...");
            string prefabPath = SavePrefabToTemporaryStorage(prefab, BundleAsset, ref wasModified);

            if (string.IsNullOrEmpty(prefabPath))
            {
                LogError("Failed to save prefab to temporary storage. Aborting build.");
                return;
            }

            LogMessage($"Prefab saved at '{prefabPath}'. Setting asset bundle name...");
            string assetBundleName = SetAssetBundleName(prefab, BundleAsset);

            if (string.IsNullOrEmpty(assetBundleName))
            {
                LogError("Failed to set asset bundle name. Aborting build.");
                return;
            }

            LogMessage($"Asset bundle name set to '{assetBundleName}'. Building AssetBundle...");
            BuildAssetBundle(BundleAsset, assetBundleName);

            LogMessage("AssetBundle built successfully. Cleaning up...");
            ResetAssetBundleName(prefab);
            ClearTemporaryStorage(BundleAsset.ExportDirectory);
            AssetDatabase.Refresh();
            LogMessage("Build process completed and cleaned up successfully.");
        }
        catch (Exception ex)
        {
            HandleBuildError(ex, prefab, wasModified, BundleAsset.ExportDirectory);
        }
    }
    public static void BuildAssetBundle(Scene scene, BasisAssetBundleObject settings)
    {
        LogMessage("Starting AssetBundle build process for scene.");

        if (!IsSceneValid(scene))
        {
            LogError("Invalid scene. AssetBundle build aborted.");
            return;
        }

        string tempScenePath = null;
        try
        {
            LogMessage($"Scene '{scene.name}' is valid. Saving a temporary copy of the scene...");
            tempScenePath = SaveSceneToTemporaryStorage(scene, settings);

            if (string.IsNullOrEmpty(tempScenePath))
            {
                LogError("Failed to save scene to temporary storage. Aborting build.");
                return;
            }

            LogMessage($"Temporary scene saved at '{tempScenePath}'. Preparing build map...");
            string sceneName = Path.GetFileNameWithoutExtension(tempScenePath);

            AssetBundleBuild buildMap = new AssetBundleBuild
            {
                assetBundleName = sceneName + settings.BundleExtension,
                assetNames = new[] { tempScenePath },
            };

            LogMessage($"Building AssetBundle with bundle name '{buildMap.assetBundleName}'...");
            EnsureDirectoryExists(settings.AssetBundleDirectory);

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                settings.AssetBundleDirectory,
                new[] { buildMap },
                settings.BuildAssetBundleOptions,
                settings.BuildTarget
            );

            if (manifest != null)
            {
                LogMessage("AssetBundle built successfully. Saving hashes...");
                BasisAssetBundleHashGeneration.ComputeAndSaveHashes(manifest, settings);
                LogMessage("AssetBundle for scene built successfully.");
            }
            else
            {
                LogError("AssetBundle build failed.");
            }
        }
        catch (Exception ex)
        {
            LogError($"Error while building AssetBundle from scene: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            if (!string.IsNullOrEmpty(tempScenePath))
            {
                ClearTemporaryStorage(tempScenePath);
            }
        }
    }
    private static bool IsValidPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            LogError("Prefab is null.");
            return false;
        }

        if (!PrefabUtility.IsPartOfPrefabInstance(prefab) && !PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            LogWarning($"GameObject '{prefab.name}' is not part of a prefab.");
            return false;
        }

        return true;
    }
    private static string SavePrefabToTemporaryStorage(GameObject prefab, BasisAssetBundleObject BasisAssetBundleObject, ref bool wasModified)
    {
        EnsureDirectoryExists(BasisAssetBundleObject.ExportDirectory);

        string NewName = BasisGenerateUniqueID.GenerateUniqueID();
        string prefabPath = Path.Combine(BasisAssetBundleObject.ExportDirectory, $"{NewName}.prefab");
        prefab = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        LogMessage($"Prefab saved to '{prefabPath}'");
        wasModified = true;

        return prefabPath;
    }
    private static string SaveSceneToTemporaryStorage(Scene scene, BasisAssetBundleObject settings)
    {
        string tempSceneDir = settings.ExportDirectory;
        EnsureDirectoryExists(tempSceneDir);

        string sceneName = Path.GetFileNameWithoutExtension(scene.path);

        string NewName = BasisGenerateUniqueID.GenerateUniqueID();
        string tempScenePath = Path.Combine(tempSceneDir, NewName + ".unity");

        LogMessage($"Saving scene to temporary path: {tempScenePath}");

        if (EditorSceneManager.SaveScene(scene, tempScenePath))
        {
            LogMessage($"Scene successfully saved to {tempScenePath}");
            return tempScenePath;
        }

        LogError("Failed to save temporary scene.");
        return null;
    }
    private static string SetAssetBundleName(GameObject prefab, BasisAssetBundleObject settings)
    {
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        string assetBundleName = $"{prefab.name.ToLower()}{settings.BundleExtension}";

        if (assetImporter != null)
        {
            assetImporter.assetBundleName = assetBundleName;
            LogMessage($"Asset bundle name '{assetBundleName}' set for prefab '{prefab.name}'.");
            return assetBundleName;
        }

        LogError("Failed to set asset bundle name.");
        return null;
    }
    private static void BuildAssetBundle(BasisAssetBundleObject settings, string assetBundleName)
    {
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            settings.AssetBundleDirectory,
            settings.BuildAssetBundleOptions,
            settings.BuildTarget
        );

        if (manifest != null)
        {
            LogMessage($"AssetBundle '{assetBundleName}' built successfully in '{settings.AssetBundleDirectory}'.");
            BasisAssetBundleHashGeneration.ComputeAndSaveHashes(manifest, settings);
        }
        else
        {
            LogError("AssetBundle build failed.");
        }
    }
    private static bool IsSceneValid(Scene scene)
    {
        if (scene.isDirty || string.IsNullOrEmpty(scene.path))
        {
            LogError("The active scene must be saved before building the AssetBundle.");
            return false;
        }
        return true;
    }
    private static void HandleBuildError(Exception ex, GameObject prefab, bool wasModified, string TemporaryStorage)
    {
        LogError($"Error while building AssetBundle from prefab: {ex.Message}\n{ex.StackTrace}");

        if (wasModified)
        {
            ResetAssetBundleName(prefab);
            ClearTemporaryStorage(TemporaryStorage);
            AssetDatabase.Refresh();
            LogMessage("Temporary modifications and storage have been reset.");
        }
    }
    private static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            LogMessage($"Directory created: {directoryPath}");
        }
    }
    private static void ClearTemporaryStorage(string tempStoragePath)
    {
        if (Directory.Exists(tempStoragePath))
        {
            Directory.Delete(tempStoragePath, true);
            LogMessage("Temporary storage has been cleared.");
        }
        else
        {
            LogMessage("Temporary storage does not exist, no cleanup needed.");
        }
    }
    private static void ResetAssetBundleName(GameObject prefab)
    {
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);

        if (assetImporter != null && !string.IsNullOrEmpty(assetImporter.assetBundleName))
        {
            assetImporter.assetBundleName = null;
            LogMessage($"Asset bundle name for '{prefab.name}' has been reset.");
        }
    }
    private static void LogMessage(string message)
    {
        Debug.Log(message);
    }
    private static void LogError(string message)
    {
        Debug.LogError(message);
    }
    private static void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }
}