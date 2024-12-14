[System.Serializable]
public class BasisLoadableBundle
{
    public string UnlockPassword;
    //encrypted state
    public BasisRemoteEncyptedBundle BasisRemoteBundleEncrypted = new BasisRemoteEncyptedBundle();
    public BasisStoredEncyptedBundle BasisLocalEncryptedBundle= new BasisStoredEncyptedBundle();
    //loaded MetaFile
    public BasisBundleInformation BasisBundleInformation = new BasisBundleInformation();
}