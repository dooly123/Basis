using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using System.Threading.Tasks;
using UnityEngine;

public static class BasisNetworkHandleRemoval
{
    public static async Task HandleDisconnection(LiteNetLib.NetPacketReader reader)
    {
        if (reader.TryGetUShort(out ushort DisconnectValue))
        {
            if (BasisNetworkManagement.Players.TryGetValue(DisconnectValue, out BasisNetworkedPlayer NetworkedPlayer))
            {
                await Task.Run(() =>
                {
                    BasisNetworkManagement.MainThreadContext.Post(async _ =>
                    {
                        BasisNetworkManagement.RemovePlayer(DisconnectValue);

                        if (NetworkedPlayer.Player.IsLocal)
                        {
                            BasisNetworkManagement.OnLocalPlayerLeft?.Invoke(NetworkedPlayer, (Basis.Scripts.BasisSdk.Players.BasisLocalPlayer)NetworkedPlayer.Player);
                        }
                        else
                        {
                            BasisNetworkManagement.OnRemotePlayerLeft?.Invoke(NetworkedPlayer, (Basis.Scripts.BasisSdk.Players.BasisRemotePlayer)NetworkedPlayer.Player);
                        }
                        GameObject.Destroy(NetworkedPlayer.gameObject);

                    }, null);
                });
            }
            else
            {
                Debug.LogError("Removal Requested but no one was found with id " + DisconnectValue);
            }
        }
        else
        {
            Debug.LogError("Tried To Read Disconnect Message Missing Data!");
        }
    }
}