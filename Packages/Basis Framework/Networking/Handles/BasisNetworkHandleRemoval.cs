using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift;
using UnityEngine;

public static class BasisNetworkHandleRemoval
{
    public static void HandleDisconnection(DarkRiftReader reader)
    {
        ushort DisconnectValue = reader.ReadUInt16();
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
    }
}