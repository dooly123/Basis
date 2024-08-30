using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasisSceneAssetBundleBuilder
{
    public static void BuildAssetBundle(Scene scene, BasisBuildSettings settings)
    {
        try
        {
            if (!scene.isDirty && !string.IsNullOrEmpty(scene.path))
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
            else
            {
                Debug.LogError("The active scene must be saved before building the AssetBundle.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error while building AssetBundle from scene: {ex.Message}\n{ex.StackTrace}");
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