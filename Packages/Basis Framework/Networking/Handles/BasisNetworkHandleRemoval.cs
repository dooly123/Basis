using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;

using UnityEngine;

public static class BasisNetworkHandleRemoval
{
    public static void HandleDisconnection(LiteNetLib.NetPacketReader reader)
    {
        if (reader.TryGetUShort(out ushort DisconnectValue))
        {
            if (BasisNetworkManagement.Players.TryGetValue(DisconnectValue, out BasisNetworkedPlayer NetworkedPlayer))
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