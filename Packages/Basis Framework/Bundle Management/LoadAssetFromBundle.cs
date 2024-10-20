using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BasisProgressReport;
public static class LoadAssetFromBundle
{
    public static async Task<GameObject> BundleToAsset(BasisTrackedBundleWrapper BasisLoadableBundle,bool UseContentCondum)
    {
        if (BasisLoadableBundle.AssetBundle != null)
        {
            BasisLoadableBundle output = BasisLoadableBundle.LoadableBundle;
            switch (output.BasisBundleInformation.BasisBundleGenerated.AssetMode)
            {
                case "GameObject":
                    {
                        AssetBundleRequest Request = BasisLoadableBundle.AssetBundle.LoadAssetAsync<GameObject>(output.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName);
                        await Request;
                       GameObject Copied = ContentControlCondom((GameObject)Request.asset, UseContentCondum);
                        if(Copied == null)
                        {
                            Debug.LogError("Unable to proceed, null Gameobject");
                            return  null;
                        }
                        return Copied;
                    }
                default:
                    Debug.LogError("Requested type " + output.BasisBundleInformation.BasisBundleGenerated.AssetMode + " has no handler");
                    return null;
            }
        }
        else
        {
            Debug.LogError("Missing Bundle!");
        }
        Debug.LogError("Returning unable to load gameobject!");
        return null;
    }
    /// <summary>
    /// Creates a copy of a GameObject, removes any unapproved MonoBehaviours, and returns the cleaned copy.
    /// </summary>
    /// <param name="SearchAndDestroy">The original GameObject to copy and clean.</param>
    /// <param name="UseContentCondum">Whether to remove unapproved MonoBehaviours or not.</param>
    /// <returns>A copy of the GameObject with unapproved scripts removed.</returns>
    public static GameObject ContentControlCondom(GameObject SearchAndDestroy, bool UseContentCondum = true)
    {
        // Create a copy of the SearchAndDestroy GameObject
        GameObject copy = GameObject.Instantiate(SearchAndDestroy);

        if (UseContentCondum)
        {
            // Create a list to hold all MonoBehaviours in the copy
            List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();
            copy.GetComponentsInChildren(true, monoBehaviours);

            // Iterate through the list of MonoBehaviours and remove unapproved ones
            for (int i = monoBehaviours.Count - 1; i >= 0; i--)
            {
                MonoBehaviour mono = monoBehaviours[i];
                string monoTypeName = mono.GetType().FullName;

                // Check if the type is in the selectedTypes list
                if (!BundledContentHolder.Instance.Selector.selectedTypes.Contains(monoTypeName))
                {
                    Debug.LogError($"MonoBehaviour {monoTypeName} is not approved and will be removed.");
                    GameObject.DestroyImmediate(mono); // Destroy the unapproved MonoBehaviour immediately
                }
            }
        }

        // Return the cleaned copy
        return copy;
    }
    public static async Task LoadSceneFromAssetBundleAsync(BasisTrackedBundleWrapper bundle, bool MakeActiveScene, ProgressReport progressCallback)
    {
        string[] scenePaths = bundle.AssetBundle.GetAllScenePaths();
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