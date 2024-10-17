using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
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