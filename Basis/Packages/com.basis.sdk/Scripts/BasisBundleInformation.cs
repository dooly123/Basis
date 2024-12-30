[System.Serializable]
public class BasisBundleInformation
{
    public bool HasError;
    public BasisBundleDescription BasisBundleDescription = new BasisBundleDescription();
    public BasisBundleGenerated BasisBundleGenerated = new BasisBundleGenerated();
}
[System.Serializable]
public class BasisBundleDescription
{
    public string AssetBundleName;//user friendly name of this asset.
    public string AssetBundleDescription;//the description of this asset
}
[System.Serializable]
public class BasisBundleGenerated
{
    public string AssetBundleHash;//hash stored seperately
    public string AssetMode;//Scene or Gameobject
    public string AssetToLoadName;// assets name we are using out of the box.
    public uint AssetBundleCRC;//CRC of the assetbundle
}
