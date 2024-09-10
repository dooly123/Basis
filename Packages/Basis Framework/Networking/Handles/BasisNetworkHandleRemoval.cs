using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift;
using UnityEngine;

public static class BasisNetworkHandleRemoval
{
    public static void HandleDisconnection(DarkRiftReader reader)
    {
        ushort DisconnectValue = reader.ReadUInt16();
        if (BasisNetworkManagement.Instance.Players.TryGetValue(DisconnectValue, out BasisNetworkedPlayer player))
        {
            GameObject.Destroy(player.gameObject);
        }
        BasisNetworkManagement.Instance.Players.Remove(DisconnectValue);
    }
}
