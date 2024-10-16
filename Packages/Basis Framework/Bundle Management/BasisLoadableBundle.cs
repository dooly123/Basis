using UnityEngine;

[System.Serializable]
public struct BasisLoadableBundle
{
    public string UnlockPassword;
    //encrypted state
    public BasisRemoteEncyptedBundle BasisRemoteBundleEncypted;
    public BasisStoredEncyptedBundle BasisStoredEncyptedBundle;
    //loaded MetaFile
    public BasisBundleInformation BasisBundleInformation;
    public AssetBundle LoadedAssetBundle;
}