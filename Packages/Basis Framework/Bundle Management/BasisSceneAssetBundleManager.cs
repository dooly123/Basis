using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BasisProgressReport;
public static class BasisSceneAssetBundleManager
{
    public static async Task DownloadAndLoadSceneAsync(bool MakeSceneActiveScene, BasisLoadableBundle BasisLoadableBundle, ProgressReport progressCallback)
    {
        BasisLoadableBundle = await BasisBundleManagement.DownloadAndSaveBundle(BasisLoadableBundle, progressCallback, new CancellationToken());
        BasisLoadableBundle.LoadedAssetBundle = await BasisLoadBundle.LoadBasisBundle(BasisLoadableBundle.BasisStoredEncyptedBundle.LocalBundleFile, BasisLoadableBundle.BasisBundleInformation, BasisLoadableBundle.UnlockPassword, progressCallback);
        await LoadSceneFromAssetBundleAsync(BasisLoadableBundle.LoadedAssetBundle, MakeSceneActiveScene, progressCallback);
    }
    private static async Task LoadSceneFromAssetBundleAsync(AssetBundle bundle,bool MakeActiveScene, ProgressReport progressCallback)
    {
        string[] scenePaths = bundle.GetAllScenePaths();
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