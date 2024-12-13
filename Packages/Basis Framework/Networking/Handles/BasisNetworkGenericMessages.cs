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
        ushort avatarLinkID = serverAvatarDataMessage.avatarDataMessage.playerIdMessage.playerID; // destination
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
                if (output.payload == null)
                {
                    Debug.LogError("Missing Payload");
                }
                player.Player.Avatar.OnNetworkMessageReceived?.Invoke(serverAvatarDataMessage.playerIdMessage.playerID, output.messageIndex, output.payload, output.recipients);
            }
        }
        else
        {
            Debug.Log("Missing Player For Message " + serverAvatarDataMessage.playerIdMessage.playerID);
        }
    }
    // Handler for server scene data messages with no recipients
    public static void HandleServerSceneDataMessage_NoRecipients(LiteNetLib.NetPacketReader reader)
    {

        ServerSceneDataMessage_NoRecipients serverSceneDataMessage_NoRecipients = new ServerSceneDataMessage_NoRecipients();
        serverSceneDataMessage_NoRecipients.Deserialize(reader);
        ushort playerID = serverSceneDataMessage_NoRecipients.playerIdMessage.playerID;
        var sceneDataMessage = serverSceneDataMessage_NoRecipients.sceneDataMessage;
        BasisScene.OnNetworkMessageReceived?.Invoke(playerID, sceneDataMessage.messageIndex, sceneDataMessage.payload);
    }

    // Handler for server scene data messages with no recipients and no payload
    public static void HandleServerSceneDataMessage_NoRecipients_NoPayload(LiteNetLib.NetPacketReader reader)
    {
        ServerSceneDataMessage_NoRecipients_NoPayload serverSceneDataMessage_NoRecipients_NoPayload = new ServerSceneDataMessage_NoRecipients_NoPayload();
        serverSceneDataMessage_NoRecipients_NoPayload.Deserialize(reader);


        ushort playerID = serverSceneDataMessage_NoRecipients_NoPayload.playerIdMessage.playerID;
        var sceneDataMessage = serverSceneDataMessage_NoRecipients_NoPayload.sceneDataMessage;
        BasisScene.OnNetworkMessageReceived?.Invoke(playerID, sceneDataMessage.messageIndex, null); // Pass null payload
    }

    // Handler for server avatar data messages with no recipients
    public static void HandleServerAvatarDataMessage_NoRecipients(LiteNetLib.NetPacketReader reader)
    {
        ServerAvatarDataMessage_NoRecipients serverAvatarDataMessage_NoRecipients = new ServerAvatarDataMessage_NoRecipients();
        serverAvatarDataMessage_NoRecipients.Deserialize(reader);

        ushort avatarLinkID = serverAvatarDataMessage_NoRecipients.avatarDataMessage.playerIdMessage.playerID; // destination
        if (BasisNetworkManagement.Players.TryGetValue(avatarLinkID, out BasisNetworkedPlayer player))
        {
            if (player.Player == null)
            {
                Debug.LogError("Missing Player! " + avatarLinkID);
                return;
            }
            if (player.Player.Avatar != null)
            {
                var output = serverAvatarDataMessage_NoRecipients.avatarDataMessage;
                if (output.payload == null)
                {
                    Debug.LogError("Missing Payload");
                }
                player.Player.Avatar.OnNetworkMessageReceived?.Invoke(serverAvatarDataMessage_NoRecipients.playerIdMessage.playerID, output.messageIndex, output.payload, null);
            }
        }
        else
        {
            if (BasisNetworkManagement.JoiningPlayers.Contains(serverAvatarDataMessage_NoRecipients.playerIdMessage.playerID) == false)
            {
                Debug.Log("Missing Player For Message " + serverAvatarDataMessage_NoRecipients.playerIdMessage.playerID);
            }
        }
    }

    // Handler for server avatar data messages with no recipients and no payload
    public static void HandleServerAvatarDataMessage_NoRecipients_NoPayload(LiteNetLib.NetPacketReader reader)
    {
        ServerAvatarDataMessage_NoRecipients_NoPayload serverAvatarDataMessage_NoRecipients_NoPayload = new ServerAvatarDataMessage_NoRecipients_NoPayload();
        serverAvatarDataMessage_NoRecipients_NoPayload.Deserialize(reader);
        ushort avatarLinkID = serverAvatarDataMessage_NoRecipients_NoPayload.avatarDataMessage.playerIdMessage.playerID; // destination
        if (BasisNetworkManagement.Players.TryGetValue(avatarLinkID, out BasisNetworkedPlayer player))
        {
            if (player.Player == null)
            {
                Debug.LogError("Missing Player! " + avatarLinkID);
                return;
            }
            if (player.Player.Avatar != null)
            {
                var output = serverAvatarDataMessage_NoRecipients_NoPayload.avatarDataMessage;
                player.Player.Avatar.OnNetworkMessageReceived?.Invoke(serverAvatarDataMessage_NoRecipients_NoPayload.playerIdMessage.playerID, output.messageIndex, null, null); // Pass null payload
            }
            else
            {
                Debug.LogError("cant accept payload no avatar!");
            }
        }
        else
        {
            Debug.Log("Missing Player For Message " + serverAvatarDataMessage_NoRecipients_NoPayload.playerIdMessage.playerID);
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
        // Check if there are no recipients and no payload
        if (recipients == null && buffer == null)
        {
            // Debug.Log("Sending with no Recipients or buffer");
            // No recipients, no payload case
            SceneDataMessage_NoRecipients_NoPayload sceneDataMessage = new SceneDataMessage_NoRecipients_NoPayload
            {
                messageIndex = messageIndex
            };
            Writer.Put(BasisNetworkTag.SceneGenericMessage_NoRecipients_NoPayload);
            sceneDataMessage.Serialize(Writer);
            BasisNetworkManagement.LocalPlayerPeer.Send(Writer, BasisNetworkCommons.SceneChannel, deliveryMethod);
        }
        // Check if there are no recipients but there is a payload
        else if (recipients == null)
        {
            // Debug.Log("Sending with no Recipients");
            // No recipients but has payload
            SceneDataMessage_NoRecipients sceneDataMessage = new SceneDataMessage_NoRecipients
            {
                messageIndex = messageIndex,
                payload = buffer
            };
            Writer.Put(BasisNetworkTag.SceneGenericMessage_NoRecipients);
            sceneDataMessage.Serialize(Writer);
            BasisNetworkManagement.LocalPlayerPeer.Send(Writer, BasisNetworkCommons.SceneChannel, deliveryMethod);
        }
        // Case where there are recipients (payload could be null or not)
        else
        {
            //Debug.Log("Sending with Recipients and buffer");
            SceneDataMessage sceneDataMessage = new SceneDataMessage
            {
                messageIndex = messageIndex,
                payload = buffer,
                recipients = recipients
            };
            Writer.Put(BasisNetworkTag.SceneGenericMessage);
            sceneDataMessage.Serialize(Writer);
            BasisNetworkManagement.LocalPlayerPeer.Send(Writer, BasisNetworkCommons.SceneChannel, deliveryMethod);
        }
    }

    // Handler for server avatar data messages with recipients but no payload
    public static void HandleServerAvatarDataMessage_Recipients_NoPayload(LiteNetLib.NetPacketReader reader)
    {
        ServerAvatarDataMessage_NoRecipients_NoPayload ServerAvatarDataMessage_NoRecipients_NoPayload = new ServerAvatarDataMessage_NoRecipients_NoPayload();
        ServerAvatarDataMessage_NoRecipients_NoPayload.Deserialize(reader);
        ushort avatarLinkID = ServerAvatarDataMessage_NoRecipients_NoPayload.avatarDataMessage.playerIdMessage.playerID; // destination
        if (BasisNetworkManagement.Players.TryGetValue(avatarLinkID, out BasisNetworkedPlayer player))
        {
            if (player.Player == null)
            {
                Debug.LogError("Missing Player! " + avatarLinkID);
                return;
            }
            if (player.Player.Avatar != null)
            {
                AvatarDataMessage_NoRecipients_NoPayload output = ServerAvatarDataMessage_NoRecipients_NoPayload.avatarDataMessage;
                player.Player.Avatar.OnNetworkMessageReceived?.Invoke(ServerAvatarDataMessage_NoRecipients_NoPayload.playerIdMessage.playerID, output.messageIndex, null, null);
            }
        }
        else
        {
            Debug.Log("Missing Player For Message " + ServerAvatarDataMessage_NoRecipients_NoPayload.playerIdMessage.playerID);
        }
    }
    // Handler for server scene data messages with recipients but no payload
    public static void HandleServerSceneDataMessage_Recipients_NoPayload(LiteNetLib.NetPacketReader reader)
    {
        ServerSceneDataMessage_NoRecipients_NoPayload serverSceneDataMessage_NoPayload = new ServerSceneDataMessage_NoRecipients_NoPayload();
        serverSceneDataMessage_NoPayload.Deserialize(reader);
        ushort playerID = serverSceneDataMessage_NoPayload.playerIdMessage.playerID;
        SceneDataMessage_NoRecipients_NoPayload sceneDataMessage = serverSceneDataMessage_NoPayload.sceneDataMessage;
        BasisScene.OnNetworkMessageReceived?.Invoke(playerID, sceneDataMessage.messageIndex, null);
    }
}