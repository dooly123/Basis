using Basis.Scripts.BasisSdk;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasisBuildAssetBundleMenu
{
    [MenuItem("Basis/Build AssetBundle From Prefab")]
    public static void BuildAssetBundleFromPrefab()
    {
        GameObject selectedObject = Selection.activeObject as GameObject;

        if (selectedObject == null)
        {
            Debug.LogError("No prefab selected. Please select a prefab in the Project window to build an AssetBundle.");
            return;
        }
        BasisAssetBundleObject BasisAssetBundleObject = AssetDatabase.LoadAssetAtPath<BasisAssetBundleObject>(BasisAssetBundleObject.AssetBundleObject);
        BasisAssetBundlePipeline.BuildAssetBundle(selectedObject, BasisAssetBundleObject);
    }
    [MenuItem("Basis/Build All AssetBundles With BasisAvatar Script")]
    public static void BuildAllAssetBundlesWithBasisAvatar()
    {
        // Find all prefabs in the project
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
        List<GameObject> prefabsWithBasisAvatar = new List<GameObject>();

        // Iterate through all found prefabs
        foreach (string guid in allPrefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // Check if the prefab has the BasisAvatar component
            if (prefab != null && prefab.GetComponent<BasisAvatar>() != null)
            {
                prefabsWithBasisAvatar.Add(prefab);
            }
        }

        // If no prefabs found with BasisAvatar component, log an error and return
        if (prefabsWithBasisAvatar.Count == 0)
        {
            Debug.LogError("No prefabs with the BasisAvatar script found in the project.");
            return;
        }
        BasisAssetBundleObject BasisAssetBundleObject = AssetDatabase.LoadAssetAtPath<BasisAssetBundleObject>(BasisAssetBundleObject.AssetBundleObject);
        // Iterate over all prefabs with BasisAvatar component and build AssetBundles
        foreach (GameObject prefab in prefabsWithBasisAvatar)
        {
            Debug.Log("Building AssetBundle for: " + prefab.name);
            BasisAssetBundlePipeline.BuildAssetBundle(prefab, BasisAssetBundleObject);
        }
    }
    [MenuItem("Basis/Build AssetBundle from Scene")]
    public static void BuildAssetBundleFromScene()
    {
        BasisAssetBundleObject BasisAssetBundleObject = AssetDatabase.LoadAssetAtPath<BasisAssetBundleObject>(BasisAssetBundleObject.AssetBundleObject);
        Scene activeScene = SceneManager.GetActiveScene();
        BasisAssetBundlePipeline.BuildAssetBundle(activeScene, BasisAssetBundleObject);
    }
}