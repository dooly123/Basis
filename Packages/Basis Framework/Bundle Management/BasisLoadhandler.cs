using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public static class BasisLoadhandler
{
    public static ConcurrentDictionary<string, BasisTrackedBundleWrapper> QueryableBundles = new ConcurrentDictionary<string, BasisTrackedBundleWrapper>();
    public static async Task LoadGameobjectBundle(BasisLoadableBundle BasisLoadableBundle,bool UseCondom, BasisProgressReport.ProgressReport Report, CancellationToken CancellationToken)
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
                        await LoadAssetFromBundle.BundleToAsset(Wrapper.LoadableBundle.Result, UseCondom);
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
                            await LoadAssetFromBundle.BundleToAsset(Bundle, UseCondom);
                        }
                        else
                        {
                            await Wrapper.AssetBundle;
                            await LoadAssetFromBundle.BundleToAsset(Bundle, UseCondom);
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
            BasisLoadableBundle.LoadedAssetBundle = await BasisLoadBundle.LoadBasisBundle(BasisTrackedBundleWrapper, Report);
            await LoadAssetFromBundle.BundleToAsset(BasisLoadableBundle, UseCondom);
        }
    }
    public static async Task LoadSceneBundle(bool MakeActiveScene, BasisLoadableBundle BasisLoadableBundle, BasisProgressReport.ProgressReport Report, CancellationToken CancellationToken)
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
                        await LoadAssetFromBundle.LoadSceneFromAssetBundleAsync(Wrapper.LoadableBundle.Result,MakeActiveScene, Report);
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
                            await LoadAssetFromBundle.LoadSceneFromAssetBundleAsync(Bundle, MakeActiveScene, Report);
                        }
                        else
                        {
                            await Wrapper.AssetBundle;
                            await LoadAssetFromBundle.LoadSceneFromAssetBundleAsync(Bundle, MakeActiveScene, Report);
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
            BasisLoadableBundle.LoadedAssetBundle = await BasisLoadBundle.LoadBasisBundle(BasisTrackedBundleWrapper, Report);
            await LoadAssetFromBundle.LoadSceneFromAssetBundleAsync(BasisLoadableBundle, MakeActiveScene, Report);
        }
    }
    public class BasisTrackedBundleWrapper
    {
        public Task<BasisLoadableBundle> LoadableBundle;
        public Task<AssetBundle> AssetBundle;
        public string metaUrl;
    }
}