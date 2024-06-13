using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// Factory class for loading addressable assets asynchronously.
/// </summary>
public static class AddressableLoadFactory
{
    /// <summary>
    /// Asynchronously loads an addressable asset.
    /// </summary>
    /// <param name="LoadRequest">The load request parameters.</param>
    public static async Task<bool> LoadAddressableResourceAsync<T>(AddressableLoadResourceBase LoadRequest)
    {
        LoadRequest.IsLoading = true;
        try
        {
            if (AddressableValidation.ValidateLoadResource(LoadRequest))
            {
                // For safety, we never assume people are going to do the right thing.
                if (ReleaseResource(LoadRequest))
                {
                    AddressableDebug.DebugError("Trying to load an addressable asset that was already loaded, resolving...", LoadRequest);
                }

                LoadRequest.ResourceLocationHandles = Addressables.LoadResourceLocationsAsync(LoadRequest.Key, typeof(T));
                await LoadRequest.ResourceLocationHandles.Task;
                if (LoadRequest.ResourceLocationHandles.Result != null)
                {
                    if (LoadRequest.ResourceLocationHandles.Result.Count == 0)
                    {
                        AddressableDebug.DebugError("Resource handles were empty... [" + LoadRequest.Key + "]", LoadRequest);
                    }
                    else
                    {
                        return await AddressableLoadProcess.LoadAssetAsync<T>(LoadRequest);
                    }
                }
                else
                {
                    AddressableDebug.DebugError("Resource handles result was null", LoadRequest);
                }
            }
            LoadRequest.IsLoading = false;
        }
        catch (Exception E)
        {
            AddressableDebug.DebugError(E.StackTrace + " " + E.Message, LoadRequest);
        }
        LoadRequest.IsLoading = false;
        return false;
    }
    /// <summary>
    /// Unloads the previously loaded addressable asset.
    /// </summary>
    /// <param name="LoadRequest">The load request parameters.</param>
    /// <returns>True if the resource was unloaded, false otherwise.</returns>
    public static bool ReleaseResource(AddressableLoadResourceBase loadRequest)
    {
        if (loadRequest == null || (loadRequest.Handles?.Count == 0 && !loadRequest.ResourceLocationHandles.IsValid()))
        {
            return false;
        }

        if (loadRequest.Handles != null)
        {
            int HandleCount = loadRequest.Handles.Count;
            for (int HandleIndex = 0; HandleIndex < HandleCount; HandleIndex++)
            {
                UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle handle = loadRequest.Handles[HandleIndex];
                Addressables.Release(handle);
            }
            loadRequest.Handles.Clear();
        }

        if (loadRequest.ResourceLocationHandles.IsValid())
        {
            Addressables.Release(loadRequest.ResourceLocationHandles);
        }

        return true;
    }
}