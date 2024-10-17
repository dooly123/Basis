using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public static class BasisLoadhandler
{
    public static ConcurrentDictionary<string, BasisTrackedBundleWrapper> QueryableBundles = new ConcurrentDictionary<string, BasisTrackedBundleWrapper>();
    public static async Task LoadBundle(BasisLoadableBundle BasisLoadableBundle, BasisProgressReport.ProgressReport Report, CancellationToken CancellationToken)
    {
        if (QueryableBundles.TryGetValue(BasisLoadableBundle.BasisRemoteBundleEncypted.MetaURL, out BasisTrackedBundleWrapper Wrapper))
        {
            //was previously loaded and already a loaded bundle skip everything and go for the source.
            if (Wrapper.LoadableBundle != null)
            {
                if (Wrapper.LoadableBundle.IsCompletedSuccessfully)
                {
                    if (Wrapper.LoadableBundle.Result.LoadedAssetBundle != null)
                    {
                        await LoadAssetFromBundle.BundleToAsset(Wrapper.LoadableBundle.Result);
                    }
                    else
                    {
                        Debug.LogError("LoadedAssetBundle was missing");
                    }
                }
                else
                {
                    BasisLoadableBundle Bundle = await Wrapper.LoadableBundle;
                    if (Bundle.BasisBundleInformation.HasError == false)
                    {
                        if (Wrapper.AssetBundle.IsCompletedSuccessfully)
                        {
                            await LoadAssetFromBundle.BundleToAsset(Bundle);
                        }
                        else
                        {
                            await Wrapper.AssetBundle;
                            await LoadAssetFromBundle.BundleToAsset(Bundle);
                        }
                    }
                    else
                    {
                        Debug.LogError("Error During the import for this tie in Loading bundle");
                    }
                }
            }
            else
            {
                Debug.LogError("Loaded Bundle was Null");
            }
        }
        else
        {
            BasisTrackedBundleWrapper BasisTrackedBundleWrapper = await BasisBundleManagement.DownloadAndSaveBundle(BasisLoadableBundle, Report, CancellationToken);
            BasisLoadableBundle.LoadedAssetBundle = await BasisLoadBundle.LoadBasisBundle(BasisTrackedBundleWrapper,  Report);
            await LoadAssetFromBundle.BundleToAsset(BasisLoadableBundle);
        }
    }
    public class BasisTrackedBundleWrapper
    {
        public Task<BasisLoadableBundle> LoadableBundle;
        public Task<AssetBundle> AssetBundle;
        public string metaUrl;
    }
}