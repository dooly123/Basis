using UnityEngine;

public static class BasisBasisBundleInformationHandler
{
    public static void CreateInformation(ref BasisBundleInformation BasisBundleInformation, AssetBundleManifest AssetBundleManifest, string AssetBundleName, string AssetMode)
    {
        Hash128 Hash = AssetBundleManifest.GetAssetBundleHash(AssetBundleName);
        BasisBundleInformation.BasisBundleGenerated = new BasisBundleGenerated();
        BasisBundleInformation.BasisBundleGenerated.AssetBundleHash = Hash.ToString();
        BasisBundleInformation.BasisBundleGenerated.AssetToLoadName = AssetBundleName;//saved asset bundle is the same name internal at this stage
        BasisBundleInformation.BasisBundleGenerated.AssetMode = AssetMode;
    }
}