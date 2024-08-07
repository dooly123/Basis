using Assets.Scripts.Addressable_Driver.Debug;
using Assets.Scripts.Addressable_Driver.Enums;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Addressable_Driver.Scene
{
public static class AddressableSceneSetActive
{
    public static void SetSceneActive(AddressableLoadResourceBase LoadRequest)
    {
        if (LoadRequest.ResourceType == AddressableLoadResourceType.Scene && LoadRequest.Handles.Count != 0)
        {
            SceneInstance Instance = (SceneInstance)LoadRequest.Handles[0].Result;
            AddressableDebug.Log("Setting Active Scene to " + Instance.Scene.name);
            SceneManager.SetActiveScene(Instance.Scene);
            AddressableDebug.Log("Set Active Scene" + Instance.Scene.name);
            return;
        }
    }
}
}