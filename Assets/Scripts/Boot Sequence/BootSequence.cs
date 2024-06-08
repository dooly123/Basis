 using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class BootSequence
{
    public static GameObject LoadedBootManager;
    public static string BootManager = "BootManager";
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        AsyncOperationHandle<IResourceLocator> Address = Addressables.InitializeAsync(false);
        Address.Completed += OnAddressablesInitializationComplete;
    }
    private static async void OnAddressablesInitializationComplete(AsyncOperationHandle<IResourceLocator> obj)
    {
        List<GameObject> Gameobjects = await AddressableResourceProcess.LoadAsGameObjectsAsync(BootManager, new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters());
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