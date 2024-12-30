using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using UnityEngine;
public static class BasisNetworkHandleRemoval
{
    public static void HandleDisconnection(LiteNetLib.NetPacketReader reader)
    {
        if (reader.TryGetUShort(out ushort DisconnectValue))
        {
            Debug.Log($"trying to remove Networked Player {DisconnectValue}");
            if (BasisNetworkManagement.Players.TryGetValue(DisconnectValue, out BasisNetworkedPlayer NetworkedPlayer))
            {
                if (NetworkedPlayer.Player.IsLocal == false)
                {
                    BasisNetworkManagement.RemovePlayer(DisconnectValue);//detach from sequences
                    BasisNetworkManagement.MainThreadContext.Post(_ =>
                    {
                        BasisNetworkManagement.OnRemotePlayerLeft?.Invoke(NetworkedPlayer, (Basis.Scripts.BasisSdk.Players.BasisRemotePlayer)NetworkedPlayer.Player);//tell scripts delete time
                        NetworkedPlayer.NetworkSend.DeInitialize();//shutdown the networking
                        if (NetworkedPlayer.Player.Avatar != null)//nuke avatar first
                        {
                            GameObject.Destroy(NetworkedPlayer.Player.Avatar.gameObject);
                        }
                        if (NetworkedPlayer.Player != null)//remove the player
                        {
                            GameObject.Destroy(NetworkedPlayer.Player.gameObject);
                        }

                    }, null);
                }
                else
                {
                    Debug.LogError("network used wrong api to remove local player!");
                }
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
