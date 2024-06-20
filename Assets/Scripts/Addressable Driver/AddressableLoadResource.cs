using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

[System.Serializable]
public class AddressableLoadResourceBase
{
    public string Key;
    public AddressableLoadResourceType ResourceType = AddressableLoadResourceType.Generic;
    public AddressableExpectedResult ExpectedResult = AddressableExpectedResult.SingleItem;
    public AsyncOperationHandle<IList<IResourceLocation>> ResourceLocationHandles = new AsyncOperationHandle<IList<IResourceLocation>>();
    public IList<AsyncOperationHandle> Handles = new List<AsyncOperationHandle>();
    public UnityEvent<string> OnLoadFailure = new UnityEvent<string>();
    public UnityEvent<AddressableLoadResourceBase> OnLoaded = new UnityEvent<AddressableLoadResourceBase>();
    public float LoadPercentage = 0;
    public bool IsLoading = false;
    public AddressableLoadResourceBase(string key, AddressableExpectedResult expectedResult)
    {
        Key = key;
        ExpectedResult = expectedResult;
    }
}
public class AddressableGenericResource : AddressableLoadResourceBase
{
    public InstantiationParameters InstantiationParameters; // Moved from base class to here

    public AddressableGenericResource(string key, AddressableExpectedResult expectedResult) : base(key, expectedResult)
    {
        ResourceType = AddressableLoadResourceType.Generic;
    }

    public AddressableGenericResource(AssetReference assetReference, AddressableExpectedResult expectedResult, InstantiationParameters instantiationParameters) : base(assetReference.RuntimeKeyIsValid() ? assetReference.RuntimeKey.ToString() : "Invalid", expectedResult)
    {
        if (!assetReference.RuntimeKeyIsValid())
            AddressableDebug.Log("Construct Type is Other but Preset == Scene or Gameobject");

        InstantiationParameters = instantiationParameters; // Assign instantiation parameters
    }

    public AddressableGenericResource(string key, AddressableLoadResourceType resourceType, AddressableExpectedResult expectedResult) : base(key, expectedResult)
    {
        ResourceType = resourceType;
    }
}

public class AddressableSceneResource : AddressableLoadResourceBase
{
    public bool SetAsActiveScene = true;
    public bool HasActiveSceneCallback = false;
    public LoadSceneMode LoadSceneMode = LoadSceneMode.Additive;

    public AddressableSceneResource(string key, bool setAsActiveScene, LoadSceneMode loadSceneMode) : base(key, AddressableExpectedResult.SingleItem)
    {
        ResourceType = AddressableLoadResourceType.Scene;
        SetAsActiveScene = setAsActiveScene;
        LoadSceneMode = loadSceneMode;
    }
}