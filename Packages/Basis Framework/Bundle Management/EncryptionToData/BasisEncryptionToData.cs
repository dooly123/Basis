using BasisSerializer.OdinSerializer;
using System.Threading.Tasks;
using UnityEngine;

public static class BasisEncryptionToData
{
    public static async Task<AssetBundle> GenerateBundleFromFile(string Password, string FilePath, uint CRC, BasisProgressReport.ProgressReport progressCallback)
    {
        byte[] LoadedBundleData = await BasisEncryptionWrapper.DecryptFileAsync(Password, FilePath, progressCallback);
        AssetBundle AssetBundle = AssetBundle.LoadFromMemory(LoadedBundleData, CRC);
        return AssetBundle;
    }
    public static async Task<BasisBundleInformation> GenerateMetaFromFile(string Password, string FilePath, BasisProgressReport.ProgressReport progressCallback)
    {
        byte[] LoadedMetaData = await BasisEncryptionWrapper.DecryptFileAsync(Password, FilePath, progressCallback);
        Debug.Log("Converting decrypted meta file to BasisBundleInformation...");
        BasisBundleInformation BasisBundleInformation = ConvertBytesToJson(LoadedMetaData);
        return BasisBundleInformation;
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