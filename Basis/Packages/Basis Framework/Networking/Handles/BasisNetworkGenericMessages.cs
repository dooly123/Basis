using Basis.Network.Core;
using Basis.Scripts.BasisSdk;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;

using DarkRift.Basis_Common.Serializable;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using static SerializableBasis;


public static class BasisNetworkGenericMessages
{
    // Handler for server scene data messages
    public static void HandleServerSceneDataMessage(LiteNetLib.NetPacketReader reader)
    {
        ServerSceneDataMessage ServerSceneDataMessage = new ServerSceneDataMessage();
        ServerSceneDataMessage.Deserialize(reader);
        ushort playerID = ServerSceneDataMessage.playerIdMessage.playerID;
        SceneDataMessage sceneDataMessage = ServerSceneDataMessage.sceneDataMessage;
        BasisScene.OnNetworkMessageReceived?.Invoke(playerID, sceneDataMessage.messageIndex, sceneDataMessage.payload);
    }
    public delegate void OnNetworkMessageReceiveOwnershipTransfer(string UniqueEntityID,ushort NetIdNewOwner,bool IsOwner);
    public static void HandleOwnershipTransfer(LiteNetLib.NetPacketReader reader)
    {
        OwnershipTransferMessage OwnershipTransferMessage = new OwnershipTransferMessage();
        OwnershipTransferMessage.Deserialize(reader);
        HandleOwnership(OwnershipTransferMessage);
    }
    public static void HandleOwnershipResponse(LiteNetLib.NetPacketReader reader)
    {
        OwnershipTransferMessage  ownershipTransferMessage = new OwnershipTransferMessage();
        ownershipTransferMessage.Deserialize(reader);
        HandleOwnership(ownershipTransferMessage);
    }
    public static void HandleOwnership(OwnershipTransferMessage OwnershipTransferMessage)
    {
        if (BasisNetworkManagement.Instance.OwnershipPairing.ContainsKey(OwnershipTransferMessage.ownershipID))
        {
            BasisNetworkManagement.Instance.OwnershipPairing[OwnershipTransferMessage.ownershipID] = OwnershipTransferMessage.playerIdMessage.playerID;
        }
        else
        {
            BasisNetworkManagement.Instance.OwnershipPairing.TryAdd(OwnershipTransferMessage.ownershipID, OwnershipTransferMessage.playerIdMessage.playerID);
        }
        if (BasisNetworkManagement.TryGetLocalPlayerID(out ushort Id))
        {
            bool isLocalOwner = OwnershipTransferMessage.playerIdMessage.playerID == Id;

            BasisNetworkManagement.OnOwnershipTransfer?.Invoke(OwnershipTransferMessage.ownershipID, OwnershipTransferMessage.playerIdMessage.playerID, isLocalOwner);
        }
    }
    // Handler for server avatar data messages
    public static void HandleServerAvatarDataMessage(LiteNetLib.NetPacketReader reader)
    {
        ServerAvatarDataMessage serverAvatarDataMessage = new ServerAvatarDataMessage();
        serverAvatarDataMessage.Deserialize(reader);
      //  Debug.Log("running " + nameof(HandleServerAvatarDataMessage));
        ushort avatarLinkID = serverAvatarDataMessage.avatarDataMessage.PlayerIdMessage.playerID; // destination
        if (BasisNetworkManagement.Players.TryGetValue(avatarLinkID, out BasisNetworkedPlayer player))
        {
            if (player.Player == null)
            {
                Debug.LogError("Missing Player! " + avatarLinkID);
                return;
            }
            if (player.Player.Avatar != null)
            {
                AvatarDataMessage output = serverAvatarDataMessage.avatarDataMessage;
                player.Player.Avatar.OnNetworkMessageReceived?.Invoke(serverAvatarDataMessage.playerIdMessage.playerID, output.messageIndex, output.payload, output.recipients);
            }
        }
        else
        {
            Debug.Log("Missing Player For Message " + serverAvatarDataMessage.playerIdMessage.playerID);
        }
    }
    // Sending message with different conditions
    public static void OnNetworkMessageSend(ushort messageIndex, byte[] buffer = null, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable, ushort[] recipients = null)
    {
        // Check if Recipients array is valid or not
        if (recipients != null && recipients.Length == 0)
        {
            recipients = null;
        }
        NetDataWriter Writer = new NetDataWriter();
        // Check if buffer is valid or not
        if (buffer != null && buffer.Length == 0)
        {
            buffer = null;
        }
        //Debug.Log("Sending with Recipients and buffer");
        SceneDataMessage sceneDataMessage = new SceneDataMessage
        {
            messageIndex = messageIndex,
            payload = buffer,
            recipients = recipients
        };
        sceneDataMessage.Serialize(Writer);
        BasisNetworkManagement.LocalPlayerPeer.Send(Writer, BasisNetworkCommons.SceneChannel, deliveryMethod);
    }
}