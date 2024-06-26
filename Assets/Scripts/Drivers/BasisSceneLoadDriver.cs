using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

public static class BasisSceneLoadDriver
{
    public static async Task LoadScene(string SceneToLoad)
    {
        Debug.Log("Loading Scene " + SceneToLoad);
        AddressableSceneResource Process = new AddressableSceneResource(SceneToLoad, true, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        await AddressableLoadFactory.LoadAddressableResourceAsync<SceneInstance>(Process);
        Debug.Log("Loaded Scene " + SceneToLoad);
    }
}