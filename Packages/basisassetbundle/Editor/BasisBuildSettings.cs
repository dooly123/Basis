using UnityEditor;

public class BasisBuildSettings
{
    public string AssetBundleDirectory { get; set; }
    public BuildTarget BuildTarget { get; set; }
    public string BundleExtension = ".bundle";
    public string hashExtension = ".hash";
    public BuildAssetBundleOptions BuildAssetBundleOptions = BuildAssetBundleOptions.None;
    public BasisBuildSettings(string assetBundleDirectory, BuildTarget buildTarget)
    {
        AssetBundleDirectory = assetBundleDirectory;
        BuildTarget = buildTarget;
    }

    public static BasisBuildSettings Default()
    {
        return new BasisBuildSettings("Assets/AssetBundles", BuildTarget.StandaloneWindows64);
    }
}
