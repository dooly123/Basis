using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static BasisProgressReport;
using static BasisLoadhandler;

public static class BasisLoadBundle
{
    /// <summary>
    /// Loads an AssetBundle asynchronously from a specified file location. Uses caching to prevent reloading the same bundle.
    /// Returns the cached AssetBundle immediately if it's already loaded.
    /// </summary>
    /// <param name="EncyptedfileLocation">The file location of the AssetBundle.</param>
    /// <param name="basisBundleInformation">Information related to the AssetBundle, including hash and CRC values.</param>
    /// <returns>The loaded AssetBundle if successful, otherwise null.</returns>
    public static async Task<AssetBundle> LoadBasisBundle(BasisTrackedBundleWrapper BasisTrackedBundleWrapper, ProgressReport ProgressReport)
    {
        // Ensure the provided file location is valid
       BasisLoadableBundle Bundle = BasisTrackedBundleWrapper.LoadableBundle.Result;
        if (Bundle != null && string.IsNullOrEmpty(Bundle.BasisStoredEncyptedBundle.LocalBundleFile))
        {
            Debug.LogError("Invalid file location provided was null or empty.");
            return null;
        }
        BasisTrackedBundleWrapper.AssetBundle = LoadAssetBundleAsync(Bundle.BasisStoredEncyptedBundle.LocalBundleFile, Bundle.BasisBundleInformation, Bundle.UnlockPassword, ProgressReport);

        // Return the loaded AssetBundle
        return await BasisTrackedBundleWrapper.AssetBundle;
    }

    private static async Task<AssetBundle> LoadAssetBundleAsync(string fileLocation, BasisBundleInformation basisBundleInformation, string Password, ProgressReport ProgressReport)
    {
        try
        {
            // Load the AssetBundle from the file location asynchronously using CRC
            Task<AssetBundle> Bundle = BasisEncryptionToData.GenerateBundleFromFile(Password, fileLocation, basisBundleInformation.BasisBundleGenerated.AssetBundleCRC, ProgressReport);
            return await Bundle;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred while loading AssetBundle: {ex.Message}");
            return null;
        }
    }
}