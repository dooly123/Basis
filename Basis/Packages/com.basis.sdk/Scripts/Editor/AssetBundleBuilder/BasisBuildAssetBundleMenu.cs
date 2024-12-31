using Basis.Scripts.BasisSdk;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasisBuildAssetBundleMenu
{
    /*
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

        if (selectedObject.TryGetComponent(out BasisContentBase basisContentBase))
        {
            BasisBundleInformation basisBundleInformation = new BasisBundleInformation
            {
                BasisBundleDescription = basisContentBase.BasisBundleDescription
            };
            BasisAssetBundlePipeline.BuildAssetBundle(selectedObject, BasisAssetBundleObject, basisBundleInformation, "Test");
        }
        else
        {
            Debug.LogError("Missing the BasisContentBase");
        }
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
        foreach (GameObject selectedObject in prefabsWithBasisAvatar)
        {
            Debug.Log("Building AssetBundle for: " + selectedObject.name);
            if (selectedObject.TryGetComponent(out BasisContentBase basisContentBase))
            {
                BasisBundleInformation basisBundleInformation = new BasisBundleInformation
                {
                    BasisBundleDescription = basisContentBase.BasisBundleDescription
                };
                BasisAssetBundlePipeline.BuildAssetBundle(selectedObject, BasisAssetBundleObject, basisBundleInformation, "Test");
            }
            else
            {
                Debug.LogError("Missing the BasisContentBase");
            }
        }
    }
    */
    [MenuItem("Basis/Build AssetBundle from Scene")]
    public static void BuildAssetBundleFromScene()
    {
        BasisAssetBundleObject BasisAssetBundleObject = AssetDatabase.LoadAssetAtPath<BasisAssetBundleObject>(BasisAssetBundleObject.AssetBundleObject);
        Scene activeScene = SceneManager.GetActiveScene();
        BasisScene BasisContentBase = GameObject.FindAnyObjectByType<BasisScene>(FindObjectsInactive.Exclude);
        BasisBundleInformation basisBundleInformation = new BasisBundleInformation
        {
            BasisBundleDescription = BasisContentBase.BasisBundleDescription
        };
        if (CheckIfIL2CPPIsInstalled())
        {
            BasisAssetBundlePipeline.BuildAssetBundle(activeScene, BasisAssetBundleObject, basisBundleInformation, "Scene");
        }
        else
        {
            Debug.LogError("Missing il2cpp please install from unity hub!");
        }
    }
    private static bool CheckIfIL2CPPIsInstalled()
    {
        var playbackEndingDirectory = BuildPipeline.GetPlaybackEngineDirectory(EditorUserBuildSettings.activeBuildTarget, BuildOptions.None, false);
        return !string.IsNullOrEmpty(playbackEndingDirectory)
               && Directory.Exists(Path.Combine(playbackEndingDirectory, "Variations", "il2cpp"));
    }

}
