using System.IO;
using UnityEditor;
using UnityEngine;

public class BasisPrefabAssetBundleBuilder
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

            string assetPath = AssetDatabase.GetAssetPath(prefab);
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
            string assetBundleName = prefab.name.ToLower() + settings.BundleExtension;
            assetImporter.assetBundleName = assetBundleName;

            EnsureDirectoryExists(settings.AssetBundleDirectory);

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

            assetImporter.assetBundleName = null;
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error while building AssetBundle from prefab: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
