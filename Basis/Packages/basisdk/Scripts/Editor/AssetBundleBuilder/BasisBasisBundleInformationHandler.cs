using BasisSerializer.OdinSerializer;
using System;
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
            AssetMode = AssetMode, // Provided asset mode
            AssetBundleCRC = InformationHash.CRC
        };
        // Form the meta file path using the provided extension from build settings
        string hashFilePath = Path.ChangeExtension(AssetBundlePath, BuildSettings.BasisMetaExtension);
        string filePath = Path.Combine(ExportFilePath, hashFilePath);

        // If the file exists, delete it
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        ValidateBasisBundleInformation(ref BasisBundleInformation);
        if(BasisBundleInformation.HasError)
        {
            new Exception("BasisBundleInformation Had Error!");
            return null;
        }
        // Serialize and save the BasisBundleInformation to disk
        await SaveBasisBundleInformation(BasisBundleInformation, filePath, BuildSettings, Password);
        return BasisBundleInformation;
    }
    // Function to validate BasisBundleInformation
    private static void ValidateBasisBundleInformation(ref BasisBundleInformation basisBundleInfo)
    {
        basisBundleInfo.HasError = false; // Reset the error flag

        // Check BasisBundleDescription
        if (string.IsNullOrEmpty(basisBundleInfo.BasisBundleDescription.AssetBundleName))
        {
            basisBundleInfo.HasError = true;
            Debug.LogError("AssetBundleName is not assigned.");
        }
        if (string.IsNullOrEmpty(basisBundleInfo.BasisBundleDescription.AssetBundleDescription))
        {
            basisBundleInfo.HasError = true;
            Debug.LogError("AssetBundleDescription is not assigned.");
        }

        // Check BasisBundleGenerated
        if (string.IsNullOrEmpty(basisBundleInfo.BasisBundleGenerated.AssetBundleHash))
        {
            basisBundleInfo.HasError = true;
            Debug.LogError("AssetBundleHash is not assigned.");
        }
        if (string.IsNullOrEmpty(basisBundleInfo.BasisBundleGenerated.AssetMode))
        {
            basisBundleInfo.HasError = true;
            Debug.LogError("AssetMode is not assigned.");
        }
        if (string.IsNullOrEmpty(basisBundleInfo.BasisBundleGenerated.AssetToLoadName))
        {
            basisBundleInfo.HasError = true;
            Debug.LogError("AssetToLoadName is not assigned.");
        }
    }
    private static BasisProgressReport Report = new BasisProgressReport();
    // Function to serialize and save BasisBundleInformation to disk
    private static async Task SaveBasisBundleInformation(BasisBundleInformation basisBundleInfo, string filePath, BasisAssetBundleObject BuildSettings, string password)
    {
        byte[] Information = SerializationUtility.SerializeValue<BasisBundleInformation>(basisBundleInfo, DataFormat.JSON);
        try
        {
            Debug.Log("Saving Json " + Information.Length);
            // Write JSON data to the file
           await File.WriteAllBytesAsync(filePath, Information);
            Debug.Log($"BasisBundleInformation saved to {filePath}");
            string EncryptedPath = Path.ChangeExtension(filePath, BuildSettings.BasisMetaEncyptedExtension);
            var BasisPassword = new BasisEncryptionWrapper.BasisPassword
            {
                VP = password
            };
            await BasisEncryptionWrapper.EncryptFileAsync(BasisPassword, filePath, EncryptedPath, Report);

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