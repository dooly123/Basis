using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Basis.Scripts.Addressable_Driver.Loading;
using static Basis.Scripts.Addressable_Driver.Loading.AddressableManagement;
using System.Collections.Generic;
public static class BasisGameObjectAssetBundleManager
{
    public static async Task<GameObject> DownloadAndLoadGameObjectAsync(string url, BasisBundleInformation Hash, string assetName, string subfolderName, Vector3 Position, Quaternion Rotation, ProgressReport progressCallback)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, subfolderName);
        if (Directory.Exists(folderPath) == false)
        {
            Directory.CreateDirectory(folderPath);
        }
        string localPath = Path.Combine(folderPath, AddressableManagement.GetFileNameFromUrl(url));
        GameObject LoadRequest = await CheckAndLoadGameObjectBundleAsync(url, Hash, localPath, assetName, Position, Rotation, progressCallback);
        return LoadRequest;
    }

    private static async Task<GameObject> CheckAndLoadGameObjectBundleAsync(string url, BasisBundleInformation Hash, string localPath, string assetName, Vector3 Position, Quaternion Rotation, ProgressReport progressCallback)
    {
        if (File.Exists(localPath) == false)
        {
            Debug.Log("AssetBundle not found locally, downloading.");
            await AddressableManagement.Instance.AssetBundleManagement.DownloadAssetBundleAsync(url, localPath, progressCallback);
        }
        return await LoadGameObjectBundleFromDiskAsync(url, Hash, localPath, assetName, Position,Rotation, progressCallback);
    }
    private static async Task<GameObject> LoadGameObjectBundleFromDiskAsync(string url, BasisBundleInformation Hash, string localPath, string assetName,Vector3 Position,Quaternion Rotation, ProgressReport progressCallback)
    {
        Debug.Log("Loading Bundle");
        BasisLoadedAssets BasisLoadedAssets = await AddressableManagement.Instance.AssetBundleManagement.LoadBundle(url, Hash, localPath, progressCallback);
        if(BasisLoadedAssets == null)
        {
            progressCallback?.Invoke(100); // Set progress to 100 when done
            return null;
        }
        if (BasisLoadedAssets.Bundle != null)
        {
            Debug.Log("AssetBundle loaded successfully from disk.");

            GameObject asset = BasisLoadedAssets.Bundle.LoadAsset<GameObject>(assetName);
            if (asset != null)
            {
                progressCallback?.Invoke(100); // Set progress to 100 when done
                ContentControl(asset);
                return UnityEngine.Object.Instantiate(asset, Position,Rotation);
            }
            else
            {
                Debug.LogError("cant find Gameobject with name " + assetName);
            }
            Debug.LogError("Failed to load the specified GameObject from AssetBundle.");
        }
        else
        {
            if(BasisLoadedAssets.IsBundleLoaded)
            {
                Debug.LogError("Failed to load AssetBundle from disk but its marked as loaded with loaded Amount " + BasisLoadedAssets.ProgressReportAvatarLoad);
            }
            else
            {
                Debug.LogError("Failed to load AssetBundle from disk. was marked as bundle not loaded");
            }
        }
        progressCallback?.Invoke(100); // Set progress to 100 when done
        return null;
    }

    public static void ContentControl(GameObject SearchAndDestroy)
    {
        List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();
        SearchAndDestroy.GetComponentsInChildren(true, monoBehaviours);

        int count = monoBehaviours.Count;
        for (int Index = 0; Index < count; Index++)
        {
            MonoBehaviour mono = monoBehaviours[Index];
            // Get the full name of the MonoBehaviour's type
            string monoTypeName = mono.GetType().FullName;

            // Check if the type is in the selectedTypes list
            if (Instance.ContentPoliceSelector.selectedTypes.Contains(monoTypeName))
            {
               // Debug.Log($"MonoBehaviour {monoTypeName} is approved.");
                // Do something if the MonoBehaviour type is approved
            }
            else
            {
                Debug.LogError($"MonoBehaviour {monoTypeName} is not approved.");
                GameObject.Destroy(mono);
                // Do something if the MonoBehaviour type is not approved
            }
        }
    }
}