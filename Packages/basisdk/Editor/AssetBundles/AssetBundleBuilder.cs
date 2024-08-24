using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildAssetBundleForPrefab
{
    [MenuItem("Assets/Build AssetBundle From Prefab")]
    public static void BuildAssetBundle()
    {
        // Get the selected object in the project
        Object selectedObject = Selection.activeObject;

        if (selectedObject == null || !(selectedObject is GameObject))
        {
            Debug.LogError("No prefab selected. Please select a prefab in the Project window to build an AssetBundle.");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);

        // Ensure the selected object is a prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            Debug.LogError("Selected object is not a valid prefab.");
            return;
        }

        // Create a folder to store the AssetBundle if it doesn't exist
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        // Set the asset bundle name for the prefab and its dependencies
        string assetBundleName = prefab.name.ToLower() + ".bundle";
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        assetImporter.assetBundleName = assetBundleName;

        // Build the AssetBundle containing the prefab and its dependencies
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        // Clear the AssetBundle name after building (optional)
        assetImporter.assetBundleName = null;

        // Refresh the AssetDatabase to reflect changes
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();

        Debug.Log($"AssetBundle '{assetBundleName}' has been built successfully in '{assetBundleDirectory}'");
    }
}