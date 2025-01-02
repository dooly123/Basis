using System;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
[System.Serializable]
public class BasisTrackedBundleWrapper
{
    [SerializeField]
    public BasisLoadableBundle LoadableBundle;
    [SerializeField]
    public AssetBundle AssetBundle;
    public int RequestedTimes = 0;
    public bool DidErrorOccur = false;
    /// <summary>
    /// for example this is the scene path. we can use this to see 
    /// if this scene is unloaded so we can remove the memory.
    /// </summary>
    public string MetaLink;
    // Method to await the completion of the bundle loading
    public async Task WaitForBundleLoadAsync()
    {
        // Simulating the bundle loading process - this can be replaced by your actual loading logic
        while (!IsBundleCompleteAndLoaded())
        {
            if (DidErrorOccur)
            {
                return;
            }
            await Task.Yield(); // Yield to avoid blocking the main thread
        }
    }

    // Method to check if the bundle is fully loaded
    private bool IsBundleCompleteAndLoaded()
    {
        // You can implement your actual logic to check if the bundle is loaded here
        return AssetBundle != null; // Assuming AssetBundle being non-null means it's loaded
    }
    /// <summary>
    /// in the future make this async, 
    /// i dont want to introduce await conditional issues
    /// </summary>
    /// <returns></returns>
    public async Task<bool> UnloadIfReady()
    {
        if (RequestedTimes <= 0 && AssetBundle != null)
        {
            await Task.Delay(TimeSpan.FromSeconds(BasisLoadHandler.TimeUntilMemoryRemoval));
            if (RequestedTimes <= 0 && AssetBundle != null)
            {
                BasisDebug.Log("Unloading Bundle " + AssetBundle.name);
                AssetBundle.Unload(true);
            }
            else
            {
                return false;
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool Increment()
    {
        RequestedTimes++;
        return true;
    }
    public bool DeIncrement()
    {
        if (RequestedTimes > 0)
        {
            RequestedTimes--;
        }
        return true;
    }
}