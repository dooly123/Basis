using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "NewBasisAssetBundleObject", menuName = "Basis/ScriptableObjects/BasisAssetBundleObject", order = 1)]
public class BasisAssetBundleObject : ScriptableObject
{
    public static string AssetBundleObject = "Assets/Settings/AssetBundleBuildSettings.asset";
    public string ExportDirectory = "Packages/com.basis.basisdk/TemporaryStorage";
    public string BundleExtension = ".bundle";
    public string hashExtension = ".hash";
    public bool useCompression = true;
    public bool GenerateImage = true;
    public BuildTarget BuildTarget = BuildTarget.StandaloneWindows;
    public BuildAssetBundleOptions BuildAssetBundleOptions;
    public string AssetBundleDirectory = "Assets/AssetBundles";
}