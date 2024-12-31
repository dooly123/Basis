using BasisSerializer.OdinSerializer;
using System.Threading.Tasks;
using UnityEngine;

public static class BasisEncryptionToData
{
    public static async Task<AssetBundleCreateRequest> GenerateBundleFromFile(string Password, string FilePath, uint CRC, BasisProgressReport progressCallback)
    {
        // Define the password object for decryption
        var BasisPassword = new BasisEncryptionWrapper.BasisPassword
        {
            VP = Password
        };

        // Decrypt the file asynchronously
        byte[] LoadedBundleData = await BasisEncryptionWrapper.DecryptFileAsync(BasisPassword, FilePath, progressCallback, 8388608);

        // Start the AssetBundle loading process from memory with CRC check
        AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(LoadedBundleData, CRC);

        // Track the last reported progress
        int lastReportedProgress = -1;

        // Periodically check the progress of AssetBundleCreateRequest and report progress
        while (!assetBundleCreateRequest.isDone)
        {
            // Convert the progress to a percentage (0-100)
            int progress = Mathf.RoundToInt(assetBundleCreateRequest.progress * 100);

            // Report progress only if it has changed
            if (progress > lastReportedProgress)
            {
                lastReportedProgress = progress;

                // Call the progress callback with the current progress
                progressCallback.ReportProgress(progress, "loading bundle");
            }

            // Wait a short period before checking again to avoid busy waiting
            await Task.Delay(100); // Adjust delay as needed (e.g., 100ms)
        }

        // Ensure progress reaches 100% after completion
        progressCallback.ReportProgress(100, "loading bundle");

        // Await the request completion
        await assetBundleCreateRequest;

        return assetBundleCreateRequest;
    }
    public static async Task<BasisLoadableBundle> GenerateMetaFromFile(BasisLoadableBundle BasisLoadableBundle, string FilePath, BasisProgressReport progressCallback)
    {
        var BasisPassword = new BasisEncryptionWrapper.BasisPassword
        {
            VP = BasisLoadableBundle.UnlockPassword
        };
        // BasisDebug.Log("BasisLoadableBundle.UnlockPassword" + BasisLoadableBundle.UnlockPassword);
        byte[] LoadedMetaData = await BasisEncryptionWrapper.DecryptFileAsync(BasisPassword, FilePath, progressCallback, 81920);
        BasisDebug.Log("Converting decrypted meta file to BasisBundleInformation...", BasisDebug.LogTag.Event);
        BasisLoadableBundle.BasisBundleInformation = ConvertBytesToJson(LoadedMetaData);
        return BasisLoadableBundle;
    }
    public static BasisBundleInformation ConvertBytesToJson(byte[] loadedlocalmeta)
    {
        if (loadedlocalmeta == null || loadedlocalmeta.Length == 0)
        {
            BasisDebug.LogError($"Data for {nameof(BasisBundleInformation)} is empty or null.", BasisDebug.LogTag.Event);
            return new BasisBundleInformation() { HasError = true };
        }

        // Convert the byte array to a JSON string (assuming UTF-8 encoding)
        BasisDebug.Log($"Converting byte array to JSON string...", BasisDebug.LogTag.Event);
        BasisBundleInformation Information = SerializationUtility.DeserializeValue<BasisBundleInformation>(loadedlocalmeta, DataFormat.JSON);
        return Information;
    }
}
