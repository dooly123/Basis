using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift.Server.Plugins.Commands;
using System;
using UnityEngine;
public class BasisTestNetworkAvatar : MonoBehaviour
{
    [Header("Assign Ahead Of Time")]
    public BasisAvatar avatar;
    public byte MessageIndexTest;
    public byte[] SubmittingData;
    public ushort[] Recipients = null;
    public void Awake()
    {
        if (BasisNetworkManagement.HasSentOnLocalPlayerJoin == false)
        {
            BasisNetworkManagement.OnLocalPlayerJoined += OnLocalPlayerJoined;
        }
        else
        {
            Setup();
        }
        avatar.OnNetworkMessageReceived += OnNetworkMessageReceived;
    }
    public void OnDestroy()
    {
        BasisNetworkManagement.OnLocalPlayerJoined -= OnLocalPlayerJoined;
    }
    private void OnLocalPlayerJoined(BasisNetworkedPlayer player1, BasisLocalPlayer player2)
    {
        Setup();
    }
    private void Setup()
    {
        if (BasisNetworkManagement.AvatarToPlayer(avatar, out BasisPlayer Player))
        {
            if (Player.IsLocal)
            {
                InvokeRepeating(nameof(LoopSend), 0, 1);//local avatar lets start sending data!
            }
        }
        else
        {
            Debug.LogError("Was Unable to find the owner of this avatar" + this.gameObject.name);
        }
    }
    public void LoopSend()
    {
        avatar.NetworkMessageSend(MessageIndexTest, SubmittingData, DarkRift.DeliveryMethod.Unreliable, Recipients);
    }

    private void OnNetworkMessageReceived(ushort PlayerID, byte MessageIndex, byte[] buffer, ushort[] Recipients = null)
    {
        Debug.Log("OnNetworkMessageReceived from player " + PlayerID + " with message Index " + MessageIndex + " | " + buffer.Length);
    }
}