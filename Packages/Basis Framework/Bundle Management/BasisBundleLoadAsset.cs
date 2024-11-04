using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class BasisBundleLoadAsset
{
    public static async Task<GameObject> LoadFromWrapper(BasisTrackedBundleWrapper BasisLoadableBundle, bool UseContentRemoval, Vector3 Position, Quaternion Rotation, Transform Parent = null)
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
                        GameObject loadedObject = Request.asset as GameObject;
                        if (loadedObject == null)
                        {
                            Debug.LogError("Unable to proceed, null Gameobject");
                            BasisLoadableBundle.DidErrorOccur = true;
                            await BasisLoadableBundle.AssetBundle.UnloadAsync(true);
                            return null;
                        }
                        return ContentControlCondom(loadedObject, UseContentRemoval,Vector3.positiveInfinity,Rotation,Parent);
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
    /// <param name="UseContentRemoval">Whether to remove unapproved MonoBehaviours or not.</param>
    /// <returns>A copy of the GameObject with unapproved scripts removed.</returns>
    public static GameObject ContentControlCondom(GameObject SearchAndDestroy, bool UseContentRemoval, Vector3 Position, Quaternion Rotation, Transform Parent = null)
    {

        if (UseContentRemoval)
        {
            // Create a list to hold all MonoBehaviours in the copy
            List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();
            SearchAndDestroy.GetComponentsInChildren(true, monoBehaviours);

            // Iterate through the list of MonoBehaviours and remove unapproved ones
            for (int Index = monoBehaviours.Count - 1; Index >= 0; Index--)
            {
                MonoBehaviour mono = monoBehaviours[Index];
                if (mono != null)
                {
                    string monoTypeName = mono.GetType().FullName;

                    // Check if the type is in the selectedTypes list
                    if (!BundledContentHolder.Instance.Selector.selectedTypes.Contains(monoTypeName))
                    {
                        Debug.LogError($"MonoBehaviour {monoTypeName} is not approved and will be removed.");
                        GameObject.DestroyImmediate(mono); // Destroy the unapproved MonoBehaviour immediately
                    }
                }
            }
        }
        if (Parent == null)
        {
            return GameObject.Instantiate(SearchAndDestroy, Position, Rotation);
        }
        else
        {
            // Create a copy of the SearchAndDestroy GameObject
            return GameObject.Instantiate(SearchAndDestroy, Position, Rotation, Parent);
        }
    }
    public static async Task LoadSceneFromBundleAsync(BasisTrackedBundleWrapper bundle, bool MakeActiveScene, BasisProgressReport progressCallback)
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
                progressCallback.ReportProgress(50 + asyncLoad.progress * 50, "loading scene"); // Progress from 50 to 100 during scene load
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
                progressCallback.ReportProgress(100, "loading scene"); // Set progress to 100 when done
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