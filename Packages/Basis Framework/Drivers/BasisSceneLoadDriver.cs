using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Factory;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Basis.Scripts.Drivers
{
public static class BasisSceneLoadDriver
{
    public static async Task LoadScene(string SceneToLoad)
    {
        Debug.Log("Loading Scene " + SceneToLoad);
        Addressable_Driver.AddressableSceneResource Process = new AddressableSceneResource(SceneToLoad, true, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        await AddressableLoadFactory.LoadAddressableResourceAsync<SceneInstance>(Process);
        Debug.Log("Loaded Scene " + SceneToLoad);
    }
}
}