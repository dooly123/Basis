using UnityEngine;

public struct BasisLoadedBundle
{
    public string UnlockPassword;
    //encrypted state
    public BasisRemoteEncyptedBundle BasisRemoteBundleEncypted;
    public BasisLocalEncyptedBundle BasisLocalBundleEncypted;
    //unencrypted state
    public BasisLocalDecyptedBundle BasisLocalDeEncyptedBundle;
    //loaded MetaFile
    public BasisBundleInformation BasisBundleInformation;
    public AssetBundle LoadedAssetBundle;
}