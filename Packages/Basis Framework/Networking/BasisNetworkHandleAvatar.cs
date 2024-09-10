using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.Recievers;
using DarkRift;
using UnityEngine;
using static SerializableDarkRift;
public static class BasisNetworkHandleAvatar
{
    public static void HandleAvatarChangeMessage(DarkRiftReader reader)
    {
        reader.Read(out ServerAvatarChangeMessage ServerAvatarChangeMessage);
        ushort PlayerID = ServerAvatarChangeMessage.uShortPlayerId.playerID;
        if (BasisNetworkManagement.Instance.Players.TryGetValue(PlayerID, out BasisNetworkedPlayer Player))
        {
            BasisNetworkReceiver networkReceiver = (BasisNetworkReceiver)Player.NetworkSend;
            networkReceiver.ReceiveAvatarChangeRequest(ServerAvatarChangeMessage);
        }
        else
        {
            Debug.Log("Missing Player For Message " + ServerAvatarChangeMessage.uShortPlayerId.playerID);
        }
    }
    public static void HandleAvatarUpdate(DarkRiftReader reader)
    {
        reader.Read(out ServerSideSyncPlayerMessage ServerSideSyncPlayerMessage);
        if (BasisNetworkManagement.Instance.Players.TryGetValue(ServerSideSyncPlayerMessage.playerIdMessage.playerID, out BasisNetworkedPlayer player))
        {
            BasisNetworkReceiver networkReceiver = (BasisNetworkReceiver)player.NetworkSend;
            networkReceiver.ReceiveNetworkAvatarData(ServerSideSyncPlayerMessage);
        }
        else
        {
            Debug.Log("Missing Player For Message " + ServerSideSyncPlayerMessage.playerIdMessage.playerID);
        }
    }
}