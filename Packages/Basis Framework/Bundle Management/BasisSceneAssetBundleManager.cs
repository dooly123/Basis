using System.Threading;
using System.Threading.Tasks;
using static BasisProgressReport;
public static class BasisSceneAssetBundleManager
{
    public static async Task DownloadAndLoadSceneAsync(bool MakeSceneActiveScene, BasisLoadableBundle BasisLoadableBundle, ProgressReport progressCallback)
    {
        await BasisLoadhandler.LoadBundle(BasisLoadableBundle, progressCallback, new CancellationToken());
        await BasisLoadBundle.LoadSceneFromAssetBundleAsync(BasisLoadableBundle.LoadedAssetBundle, MakeSceneActiveScene, progressCallback);
    }
}