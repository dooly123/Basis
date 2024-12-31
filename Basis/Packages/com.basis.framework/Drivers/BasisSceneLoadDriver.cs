using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Factory;
using Basis.Scripts.BasisSdk.Players;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
namespace Basis.Scripts.Drivers
{
    public static class BasisSceneLoadDriver
    {
        public static BasisProgressReport progressCallback = new BasisProgressReport();
        /// <summary>
        /// local but can be used remote
        /// </summary>
        /// <param name="SceneToLoad"></param>
        /// <returns></returns>
        public static async Task LoadSceneAddressables(string SceneToLoad, bool SpawnPlayerOnSceneLoad = true, UnityEngine.SceneManagement.LoadSceneMode Mode = UnityEngine.SceneManagement.LoadSceneMode.Additive)
        {
            SetIfPlayerShouldSpawnOnSceneLoad(SpawnPlayerOnSceneLoad);
            Debug.Log("Loading Scene " + SceneToLoad);
            AddressableSceneResource Process = new AddressableSceneResource(SceneToLoad, true, Mode);
            await AddressableLoadFactory.LoadAddressableResourceAsync<SceneInstance>(Process);
            Debug.Log("Loaded Scene " + SceneToLoad);
        }

        /// <summary>
        /// remote but can be used local.
        /// </summary>
        /// <returns></returns>
        public static async Task LoadSceneAssetBundle(BasisLoadableBundle BasisLoadableBundle, bool SpawnPlayerOnSceneLoad = true, bool MakeSceneActiveScene = true)
        {
            SetIfPlayerShouldSpawnOnSceneLoad(SpawnPlayerOnSceneLoad);
            Debug.Log("Loading Scene ");
            await BasisLoadHandler.LoadSceneBundle(MakeSceneActiveScene, BasisLoadableBundle, progressCallback, new CancellationToken());
            Debug.Log("Loaded Scene ");
        }
        /// <summary>
        /// turning this off for loading in additional levels is recommended. :) 
        /// so first run its on after that off. unless your handling it yourself.
        /// </summary>
        public static void SetIfPlayerShouldSpawnOnSceneLoad(bool SpawnPlayerOnSceneLoad)
        {
            if (BasisLocalPlayer.Instance != null)
            {
                BasisLocalPlayer.Instance.SpawnPlayerOnSceneLoad = SpawnPlayerOnSceneLoad;
            }
        }
    }
}