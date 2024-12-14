using Basis.Scripts.Addressable_Driver.Resource;
using Basis.Scripts.Networking.NetworkedPlayer;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Basis.Scripts.Networking.Factorys
{
public static class BasisPlayerFactoryNetworked
{
    public static async Task<BasisNetworkedPlayer> CreateNetworkedPlayer(InstantiationParameters InstantiationParameters, string PlayerAddressableID = "NetworkedPlayer")
    {
        Debug.Log("creating NetworkedPlayer Player");
            ChecksRequired Required = new ChecksRequired
            {
                UseContentRemoval = false,
                DisableAnimatorEvents = false
            };
            var data = await AddressableResourceProcess.LoadAsGameObjectsAsync(PlayerAddressableID, InstantiationParameters, Required);
        List<GameObject> Gameobjects = data.Item1;
        if (Gameobjects.Count != 0)
        {
            foreach (GameObject gameObject in Gameobjects)
            {
                if (gameObject.TryGetComponent(out BasisNetworkedPlayer NetworkedPlayer))
                {
                    return NetworkedPlayer;
                }
            }
        }
        Debug.LogError("Error Missing Player!");
        return null;
    }
}
}