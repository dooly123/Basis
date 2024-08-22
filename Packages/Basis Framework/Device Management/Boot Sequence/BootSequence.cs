using Basis.Scripts.Addressable_Driver.Resource;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Basis.Scripts.Boot_Sequence
{
public static class BootSequence
{
    public static GameObject LoadedBootManager;
    public static string BootManager = "BootManager";
    public static bool HasEvents = false;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        AsyncOperationHandle<IResourceLocator> Address = Addressables.InitializeAsync(false);
        Address.Completed += OnAddressablesInitializationComplete;
        HasEvents = true;
    }
    private static async void OnAddressablesInitializationComplete(AsyncOperationHandle<IResourceLocator> obj)
    {
      var data = await AddressableResourceProcess.LoadAsGameObjectsAsync(BootManager, new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters());
        List<GameObject> Gameobjects = data.Item1;
        if (Gameobjects.Count != 0)
        {
            foreach (GameObject gameObject in Gameobjects)
            {
                gameObject.name = BootManager;
            }
        }
        else
        {
            Debug.LogError("Missing " + BootManager);
        }
    }
}
}