using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BasisProgressReport;
public static class LoadAssetFromBundle
{
    public static async Task BundleToAsset(BasisLoadableBundle BasisLoadableBundle,bool UseContentCondum)
    {
        if (BasisLoadableBundle.LoadedAssetBundle != null)
        {
            switch (BasisLoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetMode)
            {
                case "GameObject":
                    {
                        AssetBundleRequest Request = BasisLoadableBundle.LoadedAssetBundle.LoadAssetAsync<GameObject>(BasisLoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName);
                        await Request;
                       GameObject Copied = ContentControlCondom((GameObject)Request.asset, UseContentCondum);
                        break;
                    }
                default:
                    Debug.LogError("Requested type " + BasisLoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetMode + " has no handler");
                    break;
            }
        }
        else
        {
            Debug.LogError("Missing Bundle!");
        }
    }
    /// <summary>
    /// pew pew
    /// </summary>
    /// <param name="SearchAndDestroy"></param>
    public static GameObject ContentControlCondom(GameObject SearchAndDestroy, bool UseContentCondum = true)
    {
        GameObject Disabled = new GameObject("Disabled");
        Disabled.SetActive(false);
        // Create a copy of the SearchAndDestroy GameObject
        GameObject copy = GameObject.Instantiate(SearchAndDestroy, Disabled.transform);
        if (UseContentCondum)
        {
            // Create a list to hold all MonoBehaviours in the copy
            List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();
            copy.GetComponentsInChildren(true, monoBehaviours);

            // Iterate through the list of MonoBehaviours and remove unapproved ones
            int count = monoBehaviours.Count;
            for (int Index = 0; Index < count; Index++)
            {
                MonoBehaviour mono = monoBehaviours[Index];
                // Get the full name of the MonoBehaviour's type
                string monoTypeName = mono.GetType().FullName;

                // Check if the type is in the selectedTypes list
                if (BundledContentHolder.Instance.Selector.selectedTypes.Contains(monoTypeName))
                {
                    // Debug.Log($"MonoBehaviour {monoTypeName} is approved.");
                    // Do something if the MonoBehaviour type is approved
                }
                else
                {
                    Debug.LogError($"MonoBehaviour {monoTypeName} is not approved.");
                    GameObject.DestroyImmediate(mono); // Destroy the unapproved MonoBehaviour immediately
                }
            }
        }
        copy.transform.parent = null;
        GameObject.DestroyImmediate(Disabled);
        // Return the modified copy of the GameObject
        return copy;
    }
    public static async Task LoadSceneFromAssetBundleAsync(BasisLoadableBundle bundle, bool MakeActiveScene, ProgressReport progressCallback)
    {
        string[] scenePaths = bundle.LoadedAssetBundle.GetAllScenePaths();
        if (scenePaths.Length == 0)
        {
            Debug.LogError("No scenes found in AssetBundle.");
            return;
        }

        if (!string.IsNullOrEmpty(scenePaths[0]))
        {
            // Load the scene asynchronously
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePaths[0], LoadSceneMode.Additive);

            // Track scene loading progress
            while (!asyncLoad.isDone)
            {
                progressCallback?.Invoke(50 + asyncLoad.progress * 50); // Progress from 50 to 100 during scene load
                await Task.Yield();
            }

            Debug.Log("Scene loaded successfully from AssetBundle.");
            Scene loadedScene = SceneManager.GetSceneByPath(scenePaths[0]);

            // Set the loaded scene as the active scene
            if (loadedScene.IsValid())
            {
                if (MakeActiveScene)
                {
                    SceneManager.SetActiveScene(loadedScene);
                }
                Debug.Log("Scene set as active: " + loadedScene.name);
                progressCallback?.Invoke(100); // Set progress to 100 when done
            }
            else
            {
                Debug.LogError("Failed to get loaded scene.");
            }
        }
        else
        {
            Debug.LogError("Scene not found in AssetBundle.");
        }
    }

}