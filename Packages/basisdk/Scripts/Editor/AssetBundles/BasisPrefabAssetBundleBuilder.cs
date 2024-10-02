using System.IO;
using UnityEditor;
using UnityEngine;

public static class BasisPrefabAssetBundleBuilder
{
    public static void BuildAssetBundle(GameObject prefab, BasisBuildSettings settings)
    {
        try
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab is null.");
                return;
            }

            // Check if the GameObject is in the scene
            if (PrefabUtility.IsPartOfPrefabInstance(prefab) || PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                Debug.Log($"GameObject '{prefab.name}' exists in the scene.");

                // Save the prefab to disk under Packages/com.basis.basisdk/TemporaryStorage
                string tempStoragePath = "Packages/com.basis.basisdk/TemporaryStorage";
                EnsureDirectoryExists(tempStoragePath);

                string prefabPath = Path.Combine(tempStoragePath, prefab.name + ".prefab");

                // Override the prefab if it already exists
                if (File.Exists(prefabPath))
                {
                    Debug.Log($"Prefab '{prefab.name}' already exists in TemporaryStorage. Overriding it.");
                }

                prefab = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                Debug.Log($"Prefab saved to '{prefabPath}'");
            }
            else
            {
                Debug.LogWarning($"GameObject '{prefab.name}' is not part of the scene.");
            }

            // Set asset bundle name
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
            string assetBundleName = prefab.name.ToLower() + settings.BundleExtension;
            assetImporter.assetBundleName = assetBundleName;

            // Ensure asset bundle directory exists
            EnsureDirectoryExists(settings.AssetBundleDirectory);

            // Build the AssetBundle
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                settings.AssetBundleDirectory,
                settings.BuildAssetBundleOptions,
                settings.BuildTarget
            );

            if (manifest != null)
            {
                BasisAssetBundleUtility.ComputeAndSaveHashes(manifest, settings);
                Debug.Log($"AssetBundle '{assetBundleName}' has been built successfully in '{settings.AssetBundleDirectory}'");
            }
            else
            {
                Debug.LogError("AssetBundle build failed.");
            }

            // Clear the asset bundle name after build
            assetImporter.assetBundleName = null;
            AssetDatabase.RemoveUnusedAssetBundleNames();

            // Clean up TemporaryStorage after AssetBundle build
            ClearTemporaryStorage();

            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error while building AssetBundle from prefab: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
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
    }
}