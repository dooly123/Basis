using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Factory;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using static Basis.Scripts.Addressable_Driver.Loading.AddressableManagement;
namespace Basis.Scripts.Drivers
{
    public static class BasisSceneLoadDriver
    {
        public const string World = "World";
        public static ProgressReport progressCallback;
        public static async Task LoadSceneAddressables(string SceneToLoad)
        {
            Debug.Log("Loading Scene " + SceneToLoad);
            AddressableSceneResource Process = new AddressableSceneResource(SceneToLoad, true, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            await AddressableLoadFactory.LoadAddressableResourceAsync<SceneInstance>(Process);
            Debug.Log("Loaded Scene " + SceneToLoad);
        }
        public static async Task LoadSceneAssetBundle(string SceneToLoad,string HashUrl)
        {
            Debug.Log("Loading Scene " + SceneToLoad);
            await SceneAssetBundleManager.DownloadAndLoadSceneAsync(SceneToLoad, HashUrl, World, progressCallback);
            Debug.Log("Loaded Scene " + SceneToLoad);
        }
    }
}