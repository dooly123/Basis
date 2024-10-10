using System.IO;
using UnityEditor;
using UnityEngine;

public static class AssetBundleBuilder
{
    public static void BuildAssetBundle(BasisAssetBundleObject settings, string assetBundleName,ref BasisBundleInformation BasisBundleInformation,string Mode)
    {
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            settings.AssetBundleDirectory,
            settings.BuildAssetBundleOptions,
            settings.BuildTarget
        );

        if (manifest != null)
        {
            BasisAssetBundleHashGeneration.ComputeAndSaveHashes(manifest, settings);
            BasisBasisBundleInformationHandler.CreateInformation(ref BasisBundleInformation, manifest,assetBundleName, Mode);
        }
        else
        {
            Debug.LogError("AssetBundle build failed.");
        }
    }

    public static string SetAssetBundleName(string assetPath, string uniqueID, BasisAssetBundleObject settings)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        string assetBundleName = $"{uniqueID}{settings.BundleExtension}";

        if (assetImporter != null)
        {
            assetImporter.assetBundleName = assetBundleName;
            return assetBundleName;
        }

        return null;
    }

    public static void ResetAssetBundleName(string assetPath)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        if (assetImporter != null && !string.IsNullOrEmpty(assetImporter.assetBundleName))
        {
            assetImporter.assetBundleName = null;
        }
    }
}