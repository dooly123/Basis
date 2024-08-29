using Basis.Scripts.Addressable_Driver.Loading;
using System.Threading.Tasks;
using UnityEngine;

public static class BasisAssetBundleHashLookup
{
    public const string HashExtension = ".hash";

    public static async Task<string> GetHashOrFallback(string hash, string AvatarAddress)
    {
        if (string.IsNullOrEmpty(hash))
        {
            hash = AddressableManagement.ChangeExtension(AvatarAddress, HashExtension);
            return await AddressableManagement.LoadTextFromURLAsync(hash);
        }
        else if (hash.Contains("https://") || hash.Contains("http://"))
        {
            return await AddressableManagement.LoadTextFromURLAsync(hash);
        }

        return hash;
    }
}
