using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "NewBasisAssetBundleObject", menuName = "Basis/ScriptableObjects/BasisAssetBundleObject", order = 1)]
public class BasisAssetBundleObject : ScriptableObject
{
    public static string AssetBundleObject = "Assets/Settings/AssetBundleBuildSettings.asset";
    public string TemporaryStorage = "Packages/com.basis.basisdk/TemporaryStorage";
    public string BundleExtension = ".bundle";
    public string hashExtension = ".hash";
    public string BasisMetaExtension = ".BasisMeta";
    public string BasisBundleEncyptedExtension = ".BasisEncyptedBundle";
    public string BasisMetaEncyptedExtension = ".BasisEncyptedMeta";
    public bool useCompression = true;
    public bool GenerateImage = true;
    public BuildTarget BuildTarget = BuildTarget.StandaloneWindows;
    public BuildAssetBundleOptions BuildAssetBundleOptions;
    public string AssetBundleDirectory = "./AssetBundles";
}