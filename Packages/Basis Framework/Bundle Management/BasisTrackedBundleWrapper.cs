using System.Threading.Tasks;
using UnityEngine;
[System.Serializable]
public class BasisTrackedBundleWrapper
{
    public BasisLoadableBundle LoadableBundle;
    [SerializeField]
    public AssetBundle AssetBundle;
    public bool DidErrorOccur = false;
    // Method to await the completion of the bundle loading
    public async Task WaitForBundleLoadAsync()
    {
        // Simulating the bundle loading process - this can be replaced by your actual loading logic
        while (!IsBundleCompleteAndLoaded())
        {
            if(DidErrorOccur)
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
}