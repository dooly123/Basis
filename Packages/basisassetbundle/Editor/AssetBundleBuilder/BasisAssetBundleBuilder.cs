using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasisAssetBundleBuilder : MonoBehaviour
{
    public static void BuildAssetBundle(GameObject prefab, BasisBuildSettings settings)
    {
        if (!IsValidPrefab(prefab)) return;

        bool wasModified = false;

        try
        {
            string prefabPath = SavePrefabToTemporaryStorage(prefab, ref wasModified);
            if (string.IsNullOrEmpty(prefabPath)) return;

            string assetBundleName = SetAssetBundleName(prefab, settings);
            if (string.IsNullOrEmpty(assetBundleName)) return;

            BuildAssetBundle(settings, assetBundleName);

            // Reset asset bundle name and clean up
            ResetAssetBundleName(prefab);
            ClearTemporaryStorage();
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            HandleBuildError(ex, prefab, wasModified);
        }
    }

    public static void BuildAssetBundle(Scene scene, BasisBuildSettings settings)
    {
        if (!IsSceneValid(scene)) return;

        try
        {
            string sceneName = Path.GetFileNameWithoutExtension(scene.path);
            AssetBundleBuild buildMap = new AssetBundleBuild
            {
                assetBundleName = sceneName + settings.BundleExtension,
                assetNames = new[] { scene.path }
            };

            EnsureDirectoryExists(settings.AssetBundleDirectory);

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                settings.AssetBundleDirectory,
                new[] { buildMap },
                settings.BuildAssetBundleOptions,
                settings.BuildTarget
            );

            if (manifest != null)
            {
                BasisAssetBundleUtility.ComputeAndSaveHashes(manifest, settings);
                Debug.Log("AssetBundle for active scene built successfully!");
            }
            else
            {
                Debug.LogError("AssetBundle build for active scene failed.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error while building AssetBundle from scene: {ex.Message}\n{ex.StackTrace}");
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

    private static string SavePrefabToTemporaryStorage(GameObject prefab, BasisAssetBundleObject BasisAssetBundleObject, ref bool wasModified)
    {
        EnsureDirectoryExists(BasisAssetBundleObject.ExportDirectory);

        string prefabPath = Path.Combine(BasisAssetBundleObject.ExportDirectory, $"{prefab.name}.prefab");
        prefab = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        Debug.Log($"Prefab saved to '{prefabPath}'");
        wasModified = true;

        return prefabPath;
    }

    private static string SetAssetBundleName(GameObject prefab, BasisBuildSettings settings)
    {
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        string assetBundleName = $"{prefab.name.ToLower()}{settings.BundleExtension}";

        if (assetImporter != null)
        {
            assetImporter.assetBundleName = assetBundleName;
            return assetBundleName;
        }

        Debug.LogError("Failed to set asset bundle name.");
        return null;
    }

    private static void BuildAssetBundle(BasisBuildSettings settings, string assetBundleName, BasisAssetBundleObject BasisAssetBundleObject)
    {
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            settings.AssetBundleDirectory,
            settings.BuildAssetBundleOptions,
            settings.BuildTarget
        );

        if (manifest != null)
        {
            BasisAssetBundleUtility.ComputeAndSaveHashes(manifest, settings);
            Debug.Log($"AssetBundle '{assetBundleName}' built successfully in '{settings.AssetBundleDirectory}'");
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

    private static void HandleBuildError(System.Exception ex, GameObject prefab, bool wasModified)
    {
        Debug.LogError($"Error while building AssetBundle from prefab: {ex.Message}\n{ex.StackTrace}");

        if (wasModified)
        {
            ResetAssetBundleName(prefab);
            ClearTemporaryStorage();
            AssetDatabase.Refresh(); // Ensure the AssetDatabase is refreshed after changes
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

    private static void ClearTemporaryStorage()
    {
        string tempStoragePath = "Packages/com.basis.basisdk/TemporaryStorage";

        if (Directory.Exists(tempStoragePath))
        {
            Directory.Delete(tempStoragePath, true);
            Debug.Log("TemporaryStorage has been cleared.");
        }
        else
        {
            Debug.Log("TemporaryStorage does not exist, no cleanup needed.");
        }
    }

    private static void ResetAssetBundleName(GameObject prefab)
    {
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);

        if (assetImporter != null && !string.IsNullOrEmpty(assetImporter.assetBundleName))
        {
            assetImporter.assetBundleName = null;
            Debug.Log($"Asset bundle name for '{prefab.name}' has been reset.");
        }
    }
}