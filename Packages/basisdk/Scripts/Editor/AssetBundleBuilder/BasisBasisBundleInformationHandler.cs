using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static AssetBundleBuilder;

public static class BasisBasisBundleInformationHandler
{
    public static async Task<BasisBundleInformation> CreateInformation(BasisAssetBundleObject BuildSettings, BasisBundleInformation BasisBundleInformation, InformationHash InformationHash, string AssetMode, string AssetBundlePath, string ExportFilePath, string Password)
    {
        BasisBundleInformation.BasisBundleGenerated = new BasisBundleGenerated
        {
            AssetBundleHash = InformationHash.bundleHash.ToString(),
            AssetToLoadName = InformationHash.File, // Asset bundle name
            AssetMode = AssetMode // Provided asset mode
        };
        // Form the meta file path using the provided extension from build settings
        string hashFilePath = Path.ChangeExtension(AssetBundlePath, BuildSettings.BasisMetaExtension);
        string filePath = Path.Combine(ExportFilePath, hashFilePath);

        // If the file exists, delete it
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        // Serialize and save the BasisBundleInformation to disk
        await SaveBasisBundleInformation(BasisBundleInformation, filePath, BuildSettings, Password);
        return BasisBundleInformation;
    }

    // Function to serialize and save BasisBundleInformation to disk
    private static async Task SaveBasisBundleInformation(BasisBundleInformation basisBundleInfo, string filePath, BasisAssetBundleObject BuildSettings, string password)
    {
        // Example of serializing the BasisBundleInformation to JSON (you can use other formats if needed)
        string json = JsonUtility.ToJson(basisBundleInfo, false);

        try
        {
            // Write JSON data to the file
            File.WriteAllText(filePath, json);
            Debug.Log($"BasisBundleInformation saved to {filePath}");
            string EncryptedPath = Path.ChangeExtension(filePath, BuildSettings.BasisMetaEncyptedExtension);
            byte[] buffer = new byte[BasisEncryptionWrapper.BufferSize];
            await BasisEncryptionWrapper.EncryptFileAsync(filePath, EncryptedPath, password, buffer);

            // Delete the bundle file if it exists
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException ioEx)
        {
            Debug.LogError($"Failed to save BasisBundleInformation to {filePath}: {ioEx.Message}");
        }
    }
}