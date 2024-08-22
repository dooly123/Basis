
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;

public class BasisAddressableData
{
    public AddressablesPlayerBuildResult Result;
    public AddressableAssetProfileSettings ProfileSettings;//assigned at runtime
    public AddressableAssetSettings AssetSettings;//assigned at runtime
    public AddressableAssetGroup Group;//assigned at runtime
    public List<string> LocationFiles = new List<string>();///assigned at runtime
    public string UniqueIdentifier;//very unqiue good
    public string RemoteBuildLocation;
    public string RemoteLoadLocation;
    public string BuildScript;
    public string DefaultProfileName = "Default";
    public string RemoteBuildPath = "RemoteBuildPath";
    public string RemoteLoadPath = "RemoteLoadPath";
}