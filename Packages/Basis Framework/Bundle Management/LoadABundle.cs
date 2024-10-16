using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class LoadABundle : MonoBehaviour
{
    public BasisProgressReport.ProgressReport Report;
    public CancellationToken CancellationToken = new CancellationToken();
    public BasisLoadableBundle BasisLoadableBundle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        BasisLoadableBundle = await BasisBundleManagement.DownloadAndSaveBundle(BasisLoadableBundle, Report, CancellationToken);
        BasisLoadableBundle.LoadedAssetBundle = await BasisLoadBundle.LoadBasisBundle(BasisLoadableBundle.BasisBundleInformation.BasisStoredDecyptedBundle.LocalBundleFile, BasisLoadableBundle.BasisBundleInformation);
        await LoadAssetFromBundle.BundleToAsset(BasisLoadableBundle);
    }
    public static class LoadAssetFromBundle
    {
        public static async Task BundleToAsset(BasisLoadableBundle BasisLoadableBundle)
        {
            if (BasisLoadableBundle.LoadedAssetBundle != null)
            {
                switch (BasisLoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetMode)
                {
                    case "GameObject":
                        {
                            AssetBundleRequest Request = BasisLoadableBundle.LoadedAssetBundle.LoadAssetAsync<GameObject>(BasisLoadableBundle.BasisBundleInformation.BasisBundleGenerated.AssetToLoadName);
                            await Request;
                            GameObject.Instantiate(Request.asset);
                            break;
                        }

                    case "Scene":
                        {
                            try
                            {
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"Error loading scene: {ex.Message}");
                            }
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

    }
}
