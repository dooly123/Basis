using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BasisAssetBundleBuilder : MonoBehaviour
{
    public static void BuildAssetBundle(GameObject prefab, BasisAssetBundleObject settings)
    {
        ClearTemporaryStorage(settings.AssetBundleDirectory);
        EnsureDirectoryExists(settings.AssetBundleDirectory);
        Debug.Log("Starting AssetBundle build process for prefab.");

        if (!IsValidPrefab(prefab))
        {
            Debug.LogError("Invalid prefab. AssetBundle build aborted.");
            return;
        }

        bool wasModified = false;

        try
        {
            Debug.Log($"Prefab '{prefab.name}' is valid. Saving to temporary storage...");
            string prefabPath = SavePrefabToTemporaryStorage(prefab, settings, ref wasModified,out string UniqueId);

            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("Failed to save prefab to temporary storage. Aborting build.");
                return;
            }

            Debug.Log($"Prefab saved at '{prefabPath}'. Setting asset bundle name...");
            string assetBundleName = SetAssetBundleName(prefabPath, UniqueId, settings);

            if (string.IsNullOrEmpty(assetBundleName))
            {
                Debug.LogError("Failed to set asset bundle name. Aborting build.");
                return;
            }

            Debug.Log($"Asset bundle name set to '{assetBundleName}'. Building AssetBundle...");
            BuildAssetBundle(settings, assetBundleName);

            Debug.Log("AssetBundle built successfully. Cleaning up...");
            ResetAssetBundleName(prefabPath);
            ClearTemporaryStorage(settings.TemporaryStorage);
            AssetDatabase.Refresh();
            Debug.Log("Build process completed and cleaned up successfully.");
        }
        catch (Exception ex)
        {
            HandleBuildError(ex, prefab, wasModified, settings.TemporaryStorage);
        }
    }
    public static void BuildAssetBundle(Scene scene, BasisAssetBundleObject settings)
    {
        ClearTemporaryStorage(settings.AssetBundleDirectory);
        EnsureDirectoryExists(settings.AssetBundleDirectory);
        Debug.Log("Starting AssetBundle build process for scene.");

        if (!IsSceneValid(scene))
        {
            Debug.LogError("Invalid scene. AssetBundle build aborted.");
            return;
        }

        string tempScenePath = null;
        try
        {
            Debug.Log($"Scene '{scene.name}' is valid. Saving a temporary copy of the scene...");
            tempScenePath = SaveSceneToTemporaryStorage(scene, settings, out string UniqueId);

            if (string.IsNullOrEmpty(tempScenePath))
            {
                Debug.LogError("Failed to save scene to temporary storage. Aborting build.");
                return;
            }

            Debug.Log($"Temporary scene saved at '{tempScenePath}'. Preparing build map...");
            string sceneName = Path.GetFileNameWithoutExtension(tempScenePath);

            AssetBundleBuild buildMap = new AssetBundleBuild
            {
                assetBundleName = UniqueId + settings.BundleExtension,
                assetNames = new[] { tempScenePath },
            };

            Debug.Log($"Building AssetBundle with bundle name '{buildMap.assetBundleName}'...");
            EnsureDirectoryExists(settings.AssetBundleDirectory);

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                settings.AssetBundleDirectory,
                new[] { buildMap },
                settings.BuildAssetBundleOptions,
                settings.BuildTarget
            );

            if (manifest != null)
            {
                Debug.Log("AssetBundle built successfully. Saving hashes...");
                BasisAssetBundleHashGeneration.ComputeAndSaveHashes(manifest, settings);
                Debug.Log("AssetBundle for scene built successfully.");
            }
            else
            {
                Debug.LogError("AssetBundle build failed.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while building AssetBundle from scene: {ex.Message}\n{ex.StackTrace}");
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
            Debug.LogError("Prefab is null.");
            return false;
        }

        if (!PrefabUtility.IsPartOfPrefabInstance(prefab) && !PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            Debug.LogWarning($"GameObject '{prefab.name}' is not part of a prefab.");
            return false;
        }

        return true;
    }
    private static string SavePrefabToTemporaryStorage(GameObject prefab, BasisAssetBundleObject BasisAssetBundleObject, ref bool wasModified,out string UniqueID)
    {
        EnsureDirectoryExists(BasisAssetBundleObject.TemporaryStorage);
        UniqueID = BasisGenerateUniqueID.GenerateUniqueID();
        string prefabPath = Path.Combine(BasisAssetBundleObject.TemporaryStorage, $"{UniqueID}.prefab");
        prefab = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        Debug.Log($"Prefab saved to '{prefabPath}'");
        wasModified = true;

        return prefabPath;
    }
    private static string SaveSceneToTemporaryStorage(Scene scene, BasisAssetBundleObject settings,out string UniqueID)
    {
        string tempSceneDir = settings.TemporaryStorage;
        EnsureDirectoryExists(tempSceneDir);

        string sceneName = Path.GetFileNameWithoutExtension(scene.path);

        UniqueID = BasisGenerateUniqueID.GenerateUniqueID();
        string tempScenePath = Path.Combine(tempSceneDir, UniqueID + ".unity");

        Debug.Log($"Saving scene to temporary path: {tempScenePath}");

        if (EditorSceneManager.SaveScene(scene, tempScenePath))
        {
            Debug.Log($"Scene successfully saved to {tempScenePath}");
            return tempScenePath;
        }

        Debug.LogError("Failed to save temporary scene.");
        return null;
    }
    private static string SetAssetBundleName(string NewCopy,string AssetBundleNewName, BasisAssetBundleObject settings)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(NewCopy);
        string assetBundleName = $"{AssetBundleNewName}{settings.BundleExtension}";

        if (assetImporter != null)
        {
            assetImporter.assetBundleName = assetBundleName;
            Debug.Log($"Asset bundle name '{assetBundleName}'");
            return assetBundleName;
        }

        Debug.LogError("Failed to set asset bundle name.");
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
            Debug.Log($"AssetBundle '{assetBundleName}' built successfully in '{settings.AssetBundleDirectory}'.");
            BasisAssetBundleHashGeneration.ComputeAndSaveHashes(manifest, settings);
        }
        else
        {
            Debug.LogError("AssetBundle build failed.");
        }
    }
    private static bool IsSceneValid(Scene scene)
    {
        if (scene.isDirty || string.IsNullOrEmpty(scene.path))
        {
            Debug.LogError("The active scene must be saved before building the AssetBundle.");
            return false;
        }
        return true;
    }
    private static void HandleBuildError(Exception ex, GameObject prefab, bool wasModified, string TemporaryStorage)
    {
        Debug.LogError($"Error while building AssetBundle from prefab: {ex.Message}\n{ex.StackTrace}");

        if (wasModified)
        {
            ResetAssetBundleName(TemporaryStorage);
            ClearTemporaryStorage(TemporaryStorage);
            AssetDatabase.Refresh();
            Debug.LogError("Temporary modifications and storage have been reset.");
        }
    }
    private static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Debug.Log($"Directory created: {directoryPath}");
        }
    }
    private static void ClearTemporaryStorage(string tempStoragePath)
    {
        if (Directory.Exists(tempStoragePath))
        {
            Directory.Delete(tempStoragePath, true);
            Debug.Log("Temporary storage has been cleared.");
        }
        else
        {
            Debug.Log("Temporary storage does not exist, no cleanup needed.");
        }
    }
    private static void ResetAssetBundleName(string TempPath)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(TempPath);

        if (assetImporter != null && !string.IsNullOrEmpty(assetImporter.assetBundleName))
        {
            assetImporter.assetBundleName = null;
            Debug.Log($"Asset bundle name for '{TempPath}' has been reset.");
        }
    }
}