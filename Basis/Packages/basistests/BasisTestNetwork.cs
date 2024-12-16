using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using LiteNetLib;
using System.Linq;
using UnityEngine;

public class BasisTestNetwork : MonoBehaviour
{
    //tests network apis
    public BasisAvatar avatar;
    public DeliveryMethod Method = DeliveryMethod.ReliableSequenced;
    public bool Send = false;
    public void OnEnable()
    {
        avatar = BasisLocalPlayer.Instance.Avatar;

        avatar.OnNetworkMessageReceived += OnNetworkMessageReceived;
        BasisScene.OnNetworkMessageReceived += OnNetworkMessageReceived;
    }
    public void LateUpdate()
    {
        if (Send)
        {
            Send = false;
            Debug.Log("sending stage 1");
            avatar.NetworkMessageSend(1, Method);
            BasisScene.NetworkMessageSend(1, Method);
            Debug.Log("sending stage 2");
            byte[] Bytes = new byte[16];
            avatar.NetworkMessageSend(2, Bytes, Method);
            BasisScene.NetworkMessageSend(2, Bytes, Method);
            Debug.Log("sending stage 3");
            Bytes = new byte[16];
            ushort[] Players = BasisNetworkManagement.Players.Keys.ToArray();
            avatar.NetworkMessageSend(3, Bytes, Method, Players);
            BasisScene.NetworkMessageSend(3, Bytes, Method, Players);
        }
    }
    public void OnDisable()
    {
        avatar.OnNetworkMessageReceived -= OnNetworkMessageReceived;
        BasisScene.OnNetworkMessageReceived -= OnNetworkMessageReceived;
    }
    private void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer, ushort[] Recipients)
    {
        Debug.Log($"Scene: Interpreting {PlayerID} Stage {MessageIndex} {buffer.Length} {Recipients.Length}");
    }

    private void OnNetworkMessageReceived(ushort PlayerID, byte MessageIndex, byte[] buffer, ushort[] Recipients)
    {
        Debug.Log($"Avatar: Interpreting {PlayerID} Stage {MessageIndex} {buffer.Length} {Recipients.Length}");
    }
}