using UnityEngine;
using static Basis.Scripts.Addressable_Driver.Loading.AddressableManagement;

namespace Basis.Scripts.Addressable_Driver.Loading
{
    [System.Serializable]
    public class BasisLoadedAssets
    {
        public AssetBundleCreateRequest bundleRequest;
        public ProgressReport ProgressReportAvatarLoad;
        public AssetBundle Bundle;
        public string Url;
        public string localPath;
        public BasisBundleInformation Hash;
        public bool IsBundleLoaded;
    }
}