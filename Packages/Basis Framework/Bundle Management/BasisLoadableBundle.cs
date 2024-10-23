[System.Serializable]
public class BasisLoadableBundle
{
    public string UnlockPassword;
    //encrypted state
    public BasisRemoteEncyptedBundle BasisRemoteBundleEncrypted;
    public BasisStoredEncyptedBundle BasisLocalEncryptedBundle;
    //loaded MetaFile
    public BasisBundleInformation BasisBundleInformation;
}