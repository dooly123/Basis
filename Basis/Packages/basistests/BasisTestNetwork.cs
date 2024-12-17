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
    public void OnDisable()
    {
        avatar = BasisLocalPlayer.Instance.Avatar;

        avatar.OnNetworkMessageReceived -= OnNetworkMessageReceived;
        BasisScene.OnNetworkMessageReceived -= OnNetworkMessageReceived;
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
            Debug.Log("syncing out " + Players.Length);
            avatar.NetworkMessageSend(3, Bytes, Method, Players);
            BasisScene.NetworkMessageSend(3, Bytes, Method, Players);

            Debug.Log("sending stage 4");
            Bytes = new byte[16];
            avatar.NetworkMessageSend(4, null, Method);
            BasisScene.NetworkMessageSend(4, null, Method);
            Debug.Log("sending stage 5");
            Bytes = new byte[16];
            avatar.NetworkMessageSend(5, null, Method, Players);
            BasisScene.NetworkMessageSend(5, null, Method, Players);
        }
    }
    private void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer, ushort[] Recipients)
    {
        string bufferLength = buffer != null ? buffer.Length.ToString() : "null";
        string recipientsLength = Recipients != null ? Recipients.Length.ToString() : "null";

        Debug.Log($"Scene: Interpreting {PlayerID} Stage {MessageIndex} BufferLength: {bufferLength} RecipientsLength: {recipientsLength}");
    }

    private void OnNetworkMessageReceived(ushort PlayerID, byte MessageIndex, byte[] buffer, ushort[] Recipients)
    {
        string bufferLength = buffer != null ? buffer.Length.ToString() : "null";
        string recipientsLength = Recipients != null ? Recipients.Length.ToString() : "null";

        Debug.Log($"Avatar: Interpreting {PlayerID} Stage {MessageIndex} BufferLength: {bufferLength} RecipientsLength: {recipientsLength}");
    }
}