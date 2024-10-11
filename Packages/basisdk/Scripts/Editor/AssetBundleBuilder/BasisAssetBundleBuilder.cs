using System.Collections.Generic;

using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
public static class AssetBundleBuilder
{
    public static async Task<List<BasisBundleInformation>> BuildAssetBundle(BasisAssetBundleObject settings, string assetBundleName, BasisBundleInformation BasisBundleInformation, string Mode, string Password)
    {
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            settings.AssetBundleDirectory,
            settings.BuildAssetBundleOptions,
            settings.BuildTarget
        );
        List<BasisBundleInformation> basisBundleInformation = new List<BasisBundleInformation>();
        if (manifest != null)
        {
            string[] Files = manifest.GetAllAssetBundles();
            for (int Index = 0; Index < Files.Length; Index++)
            {
                string FileOutput = Files[Index];
                Hash128 bundleHash = manifest.GetAssetBundleHash(FileOutput);
                InformationHash informationHash = new InformationHash
                {
                    File = FileOutput,
                    bundleHash = bundleHash
                };
                string actualFilePath = Path.Combine(settings.AssetBundleDirectory, informationHash.File);
                // Create bundle information and add it to the list
                BasisBundleInformation Output = await BasisBasisBundleInformationHandler.CreateInformation(settings, BasisBundleInformation, informationHash, Mode, assetBundleName, settings.AssetBundleDirectory, Password);
                basisBundleInformation.Add(Output);

                // Encrypt the bundle asynchronously
                await EncryptBundle(Password, actualFilePath, settings, manifest);

                // Delete the bundle file if it exists
                if (File.Exists(actualFilePath))
                {
                    File.Delete(actualFilePath);
                }
                OpenRelativePath(actualFilePath);
            }
            // Find and delete all .manifest files in the AssetBundle directory
            string[] manifestFiles = Directory.GetFiles(settings.AssetBundleDirectory, "*.manifest");
            foreach (string manifestFile in manifestFiles)
            {
                if (File.Exists(manifestFile))
                {
                    File.Delete(manifestFile);
                    Debug.Log("Deleted manifest file: " + manifestFile);
                }
            }
            string[] AssetFiles = Directory.GetFiles(settings.AssetBundleDirectory);
            foreach (string manifestFile in AssetFiles)
            {
                // Get the file name without its extension
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(manifestFile);

                // Check if the file name is exactly "AssetBundles"
                if (fileNameWithoutExtension == "AssetBundles")
                {
                    // Delete the file
                    File.Delete(manifestFile);
                    Debug.Log("Deleted manifest file: " + manifestFile);
                }
            }
        }
        else
        {
            Debug.LogError("AssetBundle build failed.");
        }
        return basisBundleInformation;
    }
    // Convert a Unity path to a Windows-compatible path and open it in File Explorer
    public static void OpenFolderInExplorer(string folderPath)
    {
        // Convert Unity-style file path (forward slashes) to Windows-style (backslashes)
        string windowsPath = folderPath.Replace("/", "\\");

        // Check if the path exists
        if (Directory.Exists(windowsPath) || File.Exists(windowsPath))
        {
            // On Windows, use 'explorer' to open the folder or highlight the file
            System.Diagnostics.Process.Start("explorer.exe", windowsPath);
        }
        else
        {
            Debug.LogError("Path does not exist: " + windowsPath);
        }
    }

    // Example usage to convert relative paths like './AssetBundles/...'
    public static void OpenRelativePath(string relativePath)
    {
        // Get the root path of the project (up to the Assets folder)
        string projectRoot = Application.dataPath.Replace("/Assets", "");

        // If the relative path starts with './', remove it
        if (relativePath.StartsWith("./"))
        {
            relativePath = relativePath.Substring(2); // Remove './'
        }

        // Combine the root with the relative path
        string fullPath = Path.Combine(projectRoot, relativePath);

        // Open the folder or file in explorer
        OpenFolderInExplorer(fullPath);
    }
    public struct InformationHash
    {
        public string File;
        public Hash128 bundleHash;
    }
    // Method to encrypt a file using a password
    public static async Task EncryptBundle(string password, string actualFilePath, BasisAssetBundleObject buildSettings, AssetBundleManifest assetBundleManifest)
    {
        System.Diagnostics.Stopwatch encryptionTimer = System.Diagnostics.Stopwatch.StartNew();

        // Get all asset bundles from the manifest
        string[] bundles = assetBundleManifest.GetAllAssetBundles();
        if (bundles.Length == 0)
        {
            Debug.LogError("No asset bundles found in manifest.");
            return;
        }

        // Reuse a single buffer for encryption to reduce memory pressure
        byte[] buffer = new byte[BasisBundleEncryption.BufferSize];

        string extendedFilePath = Path.ChangeExtension(actualFilePath, buildSettings.BasisBundleEncyptedExtension);

        // Delete existing encrypted file if present
        if (File.Exists(extendedFilePath))
        {
            File.Delete(extendedFilePath);
        }

        Debug.Log("Encrypting " + actualFilePath);
        await BasisBundleEncryption.EncryptFileAsync(actualFilePath, extendedFilePath, password, buffer);

        encryptionTimer.Stop();
        Debug.Log("Encryption took " + encryptionTimer.ElapsedMilliseconds + " ms for " + extendedFilePath);
    }
    public static string SetAssetBundleName(string assetPath, string uniqueID, BasisAssetBundleObject settings)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        string assetBundleName = $"{uniqueID}{settings.BundleExtension}";

        if (assetImporter != null)
        {
            assetImporter.assetBundleName = assetBundleName;
            return assetBundleName;
        }

        return null;
    }

    public static void ResetAssetBundleName(string assetPath)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        if (assetImporter != null && !string.IsNullOrEmpty(assetImporter.assetBundleName))
        {
            assetImporter.assetBundleName = null;
        }
    }
}