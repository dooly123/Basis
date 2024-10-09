using System.IO;
using UnityEditor;
using UnityEngine;

public class BasisAssetBundleHashGeneration
{
    public static void ComputeAndSaveHashes(AssetBundleManifest manifest, BasisAssetBundleObject BuildSettings)
    {
        try
        {
            string[] bundles = manifest.GetAllAssetBundles();
            if (bundles.Length == 0)
            {
                Debug.LogError("No asset bundles found in manifest.");
                return;
            }

            foreach (string bundle in bundles)
            {
                Hash128 bundleHash = manifest.GetAssetBundleHash(bundle);
                string hashFilePath = Path.ChangeExtension(bundle, BuildSettings.hashExtension);
                string filePath = Path.Combine(BuildSettings.AssetBundleDirectory, hashFilePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.WriteAllText(filePath, bundleHash.ToString());
            }

            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error while computing and saving hashes: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
