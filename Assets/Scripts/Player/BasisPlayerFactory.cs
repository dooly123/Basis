using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

public static class BasisPlayerFactory
{
    public static async Task<BasisLocalPlayer> CreateLocalPlayer(InstantiationParameters InstantiationParameters, string RemotePlayerId = "LocalPlayer")
    {
        BasisPlayer Player = await CreatePlayer(RemotePlayerId, InstantiationParameters);
        BasisLocalPlayer CreatedLocalPlayer = (BasisLocalPlayer)Player;
        await CreatedLocalPlayer.LocalInitialize();
        return CreatedLocalPlayer;
    }
    public static async Task<BasisRemotePlayer> CreateRemotePlayer(InstantiationParameters InstantiationParameters, string LocalPlayerId = "RemotePlayer")
    {
        BasisPlayer Player = await CreatePlayer(LocalPlayerId, InstantiationParameters);
        BasisRemotePlayer CreatedRemotePlayer = (BasisRemotePlayer)Player;
        CreatedRemotePlayer.RemoteInitialize();
        return CreatedRemotePlayer;
    }
    public static async Task<BasisPlayer> CreatePlayer(string PlayerAddressableID, InstantiationParameters InstantiationParameters)
    {
        var data = await AddressableResourceProcess.LoadAsGameObjectsAsync(PlayerAddressableID, InstantiationParameters);
        List<GameObject> Gameobjects = data.Item1;
        if (Gameobjects.Count != 0)
        {
            foreach (GameObject gameObject in Gameobjects)
            {
                if (gameObject.TryGetComponent(out BasisPlayer Player))
                {
                    return Player;
                }
            }
        }
        else
        {
            Debug.LogError("Missing ");
        }
        Debug.LogError("Error Missing Player!");
        return null;
    }
}