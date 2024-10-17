using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LoadABundle : MonoBehaviour
{
    public BasisProgressReport.ProgressReport Report;
    public CancellationToken CancellationToken = new CancellationToken();
    public BasisLoadableBundle BasisLoadableBundle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        BasisLoadableBundle = await BasisBundleManagement.DownloadAndSaveBundle(BasisLoadableBundle, Report, CancellationToken);
        BasisLoadableBundle.LoadedAssetBundle = await BasisLoadBundle.LoadBasisBundle(BasisLoadableBundle.BasisStoredEncyptedBundle.LocalBundleFile, BasisLoadableBundle.BasisBundleInformation, BasisLoadableBundle.UnlockPassword, Report);
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
