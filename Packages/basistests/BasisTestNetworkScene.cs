using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift.Server.Plugins.Commands;
using System;
using UnityEngine;

public class BasisTestNetworkScene : MonoBehaviour
{
    public BasisScene scene;
    public byte[] SendingData;
    public void Awake()
    {
        BasisNetworkManagement.OnLocalPlayerJoined += OnLocalPlayerJoined;
        BasisNetworkManagement.OnRemotePlayerJoined += OnRemotePlayerJoined;
    }

    private void OnRemotePlayerJoined(BasisNetworkedPlayer player1, BasisRemotePlayer player2)
    {

    }

    public void OnLocalPlayerJoined(BasisNetworkedPlayer player1, BasisLocalPlayer player2)
    {
        scene.OnNetworkMessageReceived += OnNetworkMessageReceived;
        scene.OnNetworkMessageSend.Invoke(1, SendingData, DarkRift.DeliveryMethod.ReliableOrdered);
    }
    private void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer)
    {

    }
}
