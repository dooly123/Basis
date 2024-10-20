using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasisValidationHandler
{
    public static bool IsValidPrefab(GameObject prefab)
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

    public static bool IsSceneValid(Scene scene)
    {
        if (scene.isDirty || string.IsNullOrEmpty(scene.path))
        {
            Debug.LogError("The active scene must be saved before building the AssetBundle.");
            return false;
        }

        return true;
    }
}