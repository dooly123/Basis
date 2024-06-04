using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
public static class BasisPlayerFactoryNetworked
{
    public static async Task<BasisNetworkedPlayer> CreateNetworkedPlayer(InstantiationParameters InstantiationParameters, string PlayerAddressableID = "NetworkedPlayer")
    {
        Debug.Log("creating NetworkedPlayer Player");
        List<GameObject> Gameobjects = await AddressableResourceProcess.LoadAsGameObjectsAsync(PlayerAddressableID, InstantiationParameters);
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