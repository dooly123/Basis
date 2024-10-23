using Basis.Scripts.BasisSdk.Players;
using BasisSerializer.OdinSerializer;

public static class BasisBundleConversionNetwork
{
    // Converts AvatarNetworkLoadInformation to BasisLoadableBundle
    public static BasisLoadableBundle ConvertFromNetwork(AvatarNetworkLoadInformation AvatarNetworkLoadInformation)
    {
        BasisLoadableBundle BasisLoadableBundle = new BasisLoadableBundle
        {
            BasisRemoteBundleEncrypted = new BasisRemoteEncyptedBundle
            {
                MetaURL = AvatarNetworkLoadInformation.AvatarMetaUrl,
                BundleURL = AvatarNetworkLoadInformation.AvatarBundleUrl
            },
            BasisBundleInformation = new BasisBundleInformation(),
            BasisLocalEncryptedBundle = new BasisStoredEncyptedBundle(),
            UnlockPassword = AvatarNetworkLoadInformation.UnlockPassword
        };

        return BasisLoadableBundle;
    }

    // Converts BasisLoadableBundle to AvatarNetworkLoadInformation
    public static AvatarNetworkLoadInformation ConvertToNetwork(BasisLoadableBundle BasisLoadableBundle)
    {
        AvatarNetworkLoadInformation AvatarNetworkLoadInformation = new AvatarNetworkLoadInformation
        {
            AvatarMetaUrl = BasisLoadableBundle.BasisRemoteBundleEncrypted.MetaURL,
            AvatarBundleUrl = BasisLoadableBundle.BasisRemoteBundleEncrypted.BundleURL,
            UnlockPassword = BasisLoadableBundle.UnlockPassword
        };
        return AvatarNetworkLoadInformation;
    }

    // Converts byte array (serialized AvatarNetworkLoadInformation) to AvatarNetworkLoadInformation
    public static AvatarNetworkLoadInformation ConvertToNetwork(byte[] BasisLoadableBundle)
    {
        return SerializationUtility.DeserializeValue<AvatarNetworkLoadInformation>(BasisLoadableBundle, DataFormat.Binary);
    }

    // Converts byte array (serialized AvatarNetworkLoadInformation) to BasisLoadableBundle
    public static BasisLoadableBundle ConvertNetworkBytesToBasisLoadableBundle(byte[] BasisLoadableBundle)
    {
        AvatarNetworkLoadInformation AvatarNetworkLoadInformation = SerializationUtility.DeserializeValue<AvatarNetworkLoadInformation>(BasisLoadableBundle, DataFormat.Binary);
        return ConvertFromNetwork(AvatarNetworkLoadInformation);
    }

    // Converts AvatarNetworkLoadInformation to byte array (serialization)
    public static byte[] ConvertNetworkToByte(AvatarNetworkLoadInformation AvatarNetworkLoadInformation)
    {
        return SerializationUtility.SerializeValue<AvatarNetworkLoadInformation>(AvatarNetworkLoadInformation, DataFormat.Binary);
    }

    // Converts BasisLoadableBundle to byte array (serialization)
    public static byte[] ConvertBasisLoadableBundleToBytes(BasisLoadableBundle BasisLoadableBundle)
    {
        AvatarNetworkLoadInformation AvatarNetworkLoadInformation = ConvertToNetwork(BasisLoadableBundle);
        return SerializationUtility.SerializeValue<AvatarNetworkLoadInformation>(AvatarNetworkLoadInformation, DataFormat.Binary);
    }

    // Converts byte array (serialized BasisLoadableBundle) to BasisLoadableBundle
    public static BasisLoadableBundle ConvertBytesToBasisLoadableBundle(byte[] BasisLoadableBundleBytes)
    {
        AvatarNetworkLoadInformation AvatarNetworkLoadInformation = SerializationUtility.DeserializeValue<AvatarNetworkLoadInformation>(BasisLoadableBundleBytes, DataFormat.Binary);
        return ConvertFromNetwork(AvatarNetworkLoadInformation);
    }
}