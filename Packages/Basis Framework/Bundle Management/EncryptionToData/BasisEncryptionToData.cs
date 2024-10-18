using BasisSerializer.OdinSerializer;
using System.Threading.Tasks;
using UnityEngine;

public static class BasisEncryptionToData
{
    public static async Task<AssetBundleCreateRequest> GenerateBundleFromFile(string Password, string FilePath, uint CRC, BasisProgressReport.ProgressReport progressCallback)
    {
        byte[] LoadedBundleData = await BasisEncryptionWrapper.DecryptFileAsync(Password, FilePath, progressCallback);
        AssetBundleCreateRequest AssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(LoadedBundleData, CRC);
        await AssetBundleCreateRequest;
        return AssetBundleCreateRequest;
    }
    public static async Task<BasisLoadableBundle> GenerateMetaFromFile(BasisLoadableBundle BasisLoadableBundle, string FilePath, BasisProgressReport.ProgressReport progressCallback)
    {
        byte[] LoadedMetaData = await BasisEncryptionWrapper.DecryptFileAsync(BasisLoadableBundle.UnlockPassword, FilePath, progressCallback);
        Debug.Log("Converting decrypted meta file to BasisBundleInformation...");
        BasisLoadableBundle.BasisBundleInformation = ConvertBytesToJson(LoadedMetaData);
        return BasisLoadableBundle;
    }
    public static BasisBundleInformation ConvertBytesToJson(byte[] loadedlocalmeta)
    {
        if (loadedlocalmeta == null || loadedlocalmeta.Length == 0)
        {
            Debug.LogError($"Data for {nameof(BasisBundleInformation)} is empty or null.");
            return new BasisBundleInformation() { HasError = true };
        }

        // Convert the byte array to a JSON string (assuming UTF-8 encoding)
        Debug.Log($"Converting byte array to JSON string...");
        BasisBundleInformation Information = SerializationUtility.DeserializeValue<BasisBundleInformation>(loadedlocalmeta, DataFormat.JSON);
        return Information;
    }
}