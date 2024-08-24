using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Factory;
using Basis.Scripts.Addressable_Driver.Loading;
using System.IO;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Basis.Scripts.Drivers
{
    public static class BasisSceneLoadDriver
    {
        public const string World = "World";
        public static async Task LoadSceneAddressables(string SceneToLoad)
        {
            Debug.Log("Loading Scene " + SceneToLoad);
            Addressable_Driver.AddressableSceneResource Process = new AddressableSceneResource(SceneToLoad, true, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            await AddressableLoadFactory.LoadAddressableResourceAsync<SceneInstance>(Process);
            Debug.Log("Loaded Scene " + SceneToLoad);
        }
        public static async Task LoadSceneAssetBundle(string SceneToLoad)
        {
            AddressableManagement.Instance.UnloadAssetBundle(SceneToLoad);
            await SceneAssetBundleManager.DownloadAndLoadSceneAsync(SceneToLoad, GetFileNameFromUrl(SceneToLoad), World);
            Debug.Log("Loading Scene " + SceneToLoad);
        }

        private static string GetFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            return Path.GetFileName(uri.LocalPath);
        }
    }
}