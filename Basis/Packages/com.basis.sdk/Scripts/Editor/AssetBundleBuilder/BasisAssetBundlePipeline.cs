using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class BasisAssetBundlePipeline
{
    // Define static delegates
    public delegate void BeforeBuildGameobjectHandler(GameObject prefab, BasisAssetBundleObject settings);
    public delegate void BeforeBuildSceneHandler(Scene prefab, BasisAssetBundleObject settings);
    public delegate void AfterBuildHandler(string assetBundleName);
    public delegate void BuildErrorHandler(Exception ex, GameObject prefab, bool wasModified, string temporaryStorage);

    // Static delegates
    public static BeforeBuildGameobjectHandler OnBeforeBuildPrefab;
    public static AfterBuildHandler OnAfterBuildPrefab;
    public static BuildErrorHandler OnBuildErrorPrefab;

    public static BeforeBuildSceneHandler OnBeforeBuildScene;
    public static AfterBuildHandler OnAfterBuildScene;
    public static BuildErrorHandler OnBuildErrorScene;
    public static void ClearOutExistingSets()
    {
        // Get all asset paths in the project
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

        // Loop through each asset path
        foreach (string assetPath in allAssetPaths)
        {
            // Get the AssetImporter for the asset at this path
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
            {
                // Clear the assetBundleName for the asset
                importer.assetBundleName = string.Empty;

                // Apply the modified asset importer settings
                importer.SaveAndReimport();

                Debug.Log("Cleared AssetBundle for asset: " + assetPath);
            }
        }

        // After clearing all AssetBundle names, optionally refresh the AssetDatabase
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();

        Debug.Log("All AssetBundle names cleared from Importer settings.");
    }
    public static async Task BuildAssetBundle(GameObject originalPrefab, BasisAssetBundleObject settings, BasisBundleInformation BasisBundleInformation, string Password)
    {
        ClearOutExistingSets();
        TemporaryStorageHandler.ClearTemporaryStorage(settings.AssetBundleDirectory);
        TemporaryStorageHandler.EnsureDirectoryExists(settings.AssetBundleDirectory);

        bool wasModified = false;

        GameObject prefab = Object.Instantiate(originalPrefab);
        try
        {
            // Invoke the delegate before building the asset bundle
            OnBeforeBuildPrefab?.Invoke(prefab, settings);

            string prefabPath = TemporaryStorageHandler.SavePrefabToTemporaryStorage(prefab, settings, ref wasModified, out string uniqueID);
            string assetBundleName = AssetBundleBuilder.SetAssetBundleName(prefabPath, uniqueID, settings);


            await AssetBundleBuilder.BuildAssetBundle(settings, assetBundleName, BasisBundleInformation, "GameObject", Password);
            AssetBundleBuilder.ResetAssetBundleName(prefabPath);
            TemporaryStorageHandler.ClearTemporaryStorage(settings.TemporaryStorage);
            AssetDatabase.Refresh();

            // Invoke the delegate after building the asset bundle
            OnAfterBuildPrefab?.Invoke(assetBundleName);
            EditorUtility.DisplayDialog("Completed Build", "successfully built asset bundles for assets, Will be found in ./AssetBundles", "ok");
        }
        catch (Exception ex)
        {
            // Handle the build error
            OnBuildErrorPrefab?.Invoke(ex, prefab, wasModified, settings.TemporaryStorage);
            BasisBundleErrorHandler.HandleBuildError(ex, prefab, wasModified, settings.TemporaryStorage);
            EditorUtility.DisplayDialog("Failed To Build", "please check the console for the full issue, " + ex, "will do");
        }
        finally
        {
            Object.DestroyImmediate(prefab);
        }
    }

    public static async void BuildAssetBundle(Scene scene, BasisAssetBundleObject settings, BasisBundleInformation BasisBundleInformation, string Password)
    {
        ClearOutExistingSets();
        TemporaryStorageHandler.ClearTemporaryStorage(settings.AssetBundleDirectory);
        TemporaryStorageHandler.EnsureDirectoryExists(settings.AssetBundleDirectory);

        if (!BasisValidationHandler.IsSceneValid(scene))
        {
            Debug.LogError("Invalid scene. AssetBundle build aborted.");
            return;
        }

        string tempScenePath = null;

        try
        {
            // Invoke the delegate before building the asset bundle
            OnBeforeBuildScene?.Invoke(scene, settings);

            tempScenePath = TemporaryStorageHandler.SaveSceneToTemporaryStorage(scene, settings, out string uniqueID);
            string assetBundleName = AssetBundleBuilder.SetAssetBundleName(tempScenePath, uniqueID, settings);

            await AssetBundleBuilder.BuildAssetBundle(settings, assetBundleName, BasisBundleInformation, "Scene", Password);
            TemporaryStorageHandler.DeleteTemporaryStorageScene(uniqueID, settings);

            // Invoke the delegate after building the asset bundle
            OnAfterBuildScene?.Invoke(assetBundleName);
        }
        catch (Exception ex)
        {
            // Handle the build error
            OnBuildErrorScene?.Invoke(ex, null, false, settings.TemporaryStorage); // Pass `null` for prefab since this is a scene
            Debug.LogError($"Error while building AssetBundle from scene: {ex.Message}\n{ex.StackTrace}");
        }
    }
}