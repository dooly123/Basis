using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using LiteNetLib;
using System.Linq;
using UnityEngine;

public class BasisTestNetwork : MonoBehaviour
{
    //tests network apis
    public BasisAvatar avatar;
    public DeliveryMethod Method = DeliveryMethod.ReliableSequenced;
    public bool Send = false;
    public ushort[] Players;
    public void OnEnable()
    {
        avatar = BasisLocalPlayer.Instance.BasisAvatar;

        foreach (BasisNetworkedPlayer Player in BasisNetworkManagement.Players.Values)
        {
            if (Player != null && Player.Player != null & Player.Player.BasisAvatar != null)
            {
                Player.Player.BasisAvatar.OnNetworkMessageReceived += OnNetworkMessageReceived;
            }
            else
            {
                Debug.LogError("This should not have occured!");
            }
        }
        BasisScene.OnNetworkMessageReceived += OnNetworkMessageReceived;
    }
    public void OnDisable()
    {
        avatar = BasisLocalPlayer.Instance.BasisAvatar;

        foreach (BasisNetworkedPlayer Player in BasisNetworkManagement.Players.Values)
        {
            if (Player != null && Player.Player != null & Player.Player.BasisAvatar != null)
            {
                Player.Player.BasisAvatar.OnNetworkMessageReceived -= OnNetworkMessageReceived;
            }
            else
            {
                Debug.LogError("This should not have occured!");
            }
        }
        BasisScene.OnNetworkMessageReceived -= OnNetworkMessageReceived;
    }
    public void LateUpdate()
    {
        if (Send)
        {
            //   ushort[] Players = BasisNetworkManagement.RemotePlayers.Keys.ToArray();
            byte[] Bytes = new byte[16];
            Debug.Log("sending Avatar");

           // avatar.NetworkMessageSend(1, Method);
           // avatar.NetworkMessageSend(2, null, Method);
            avatar.NetworkMessageSend(3, null, Method, Players);

          //  avatar.NetworkMessageSend(4, null, Method, null);

           // avatar.NetworkMessageSend(5, Bytes, Method);
           // avatar.NetworkMessageSend(6, Bytes, Method, Players);
          //  avatar.NetworkMessageSend(7, Bytes, Method, null);
         //   Debug.Log("sent Avatar");
          //  Debug.Log("Sending Scene");
         //   BasisScene.NetworkMessageSend(8, Method);
         //   BasisScene.NetworkMessageSend(9, null, Method);
         //   BasisScene.NetworkMessageSend(10, null, Method, Players);
         //
          //  BasisScene.NetworkMessageSend(11, null, Method, null);

          ///  BasisScene.NetworkMessageSend(12, Bytes, Method);
         //   BasisScene.NetworkMessageSend(13, Bytes, Method, Players);
          //  BasisScene.NetworkMessageSend(14, Bytes, Method, null);
          //  Debug.Log("sent Scene");
            Send = false;
        }
    }
    private void OnNetworkMessageReceived(ushort PlayerID, ushort MessageIndex, byte[] buffer, DeliveryMethod Method = DeliveryMethod.ReliableSequenced)
    {
        string bufferLength = buffer != null ? buffer.Length.ToString() : "null";
        Debug.Log($"Scene: Interpreting {PlayerID} Stage {MessageIndex} BufferLength: {bufferLength} ReliableSequenced: {Method}");
    }

    private void OnNetworkMessageReceived(ushort PlayerID, byte MessageIndex, byte[] buffer, DeliveryMethod Method = DeliveryMethod.ReliableSequenced)
    {
        string bufferLength = buffer != null ? buffer.Length.ToString() : "null";
        Debug.Log($"Avatar: Interpreting {PlayerID} Stage {MessageIndex} BufferLength: {bufferLength} ReliableSequenced: {Method}");
    }
}
