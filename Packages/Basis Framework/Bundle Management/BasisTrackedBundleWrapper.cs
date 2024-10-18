using System.Threading.Tasks;
using UnityEngine;

public class BasisTrackedBundleWrapper
{
    public Task<BasisLoadableBundle> LoadableBundle;
    public Task<AssetBundle> AssetBundle;
    public string metaUrl;
}