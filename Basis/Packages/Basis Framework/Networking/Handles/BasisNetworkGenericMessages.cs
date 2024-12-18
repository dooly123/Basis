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
    public delegate void OnNetworkMessageReceiveOwnershipTransfer(string UniqueEntityID, ushort NetIdNewOwner, bool IsOwner);
    public static void HandleOwnershipTransfer(LiteNetLib.NetPacketReader reader)
    {
        OwnershipTransferMessage OwnershipTransferMessage = new OwnershipTransferMessage();
        OwnershipTransferMessage.Deserialize(reader);
        HandleOwnership(OwnershipTransferMessage);
    }
    public static void HandleOwnershipResponse(LiteNetLib.NetPacketReader reader)
    {
        OwnershipTransferMessage ownershipTransferMessage = new OwnershipTransferMessage();
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
        if (BasisNetworkManagement.Players.TryGetValue(serverAvatarDataMessage.playerIdMessage.playerID, out BasisNetworkedPlayer player))
        {
            if (player.Player == null)
            {
                Debug.LogError("Missing Player! " + serverAvatarDataMessage.playerIdMessage.playerID);
                return;
            }
            if (player.Player.Avatar != null)
            {
                AvatarDataMessage output = serverAvatarDataMessage.avatarDataMessage;
                if (player.Player.Avatar.OnNetworkMessageReceived == null)
                {
                    Debug.LogError("Message was Queued But nothing was there to Rec it.");
                }
                else
                {
                    player.Player.Avatar.OnNetworkMessageReceived?.Invoke(serverAvatarDataMessage.playerIdMessage.playerID, output.messageIndex, output.payload, output.recipients);
                }
            }
            else
            {
                Debug.LogError("Missing Avatar For Message " + serverAvatarDataMessage.playerIdMessage.playerID);
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
        NetDataWriter netDataWriter = new NetDataWriter();
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
        if (deliveryMethod == DeliveryMethod.Unreliable)
        {
            netDataWriter.Put(BasisNetworkCommons.SceneChannel);
            sceneDataMessage.Serialize(netDataWriter);
            BasisNetworkManagement.LocalPlayerPeer.Send(netDataWriter, BasisNetworkCommons.FallChannel, deliveryMethod);
        }
        else
        {
            sceneDataMessage.Serialize(netDataWriter);
            BasisNetworkManagement.LocalPlayerPeer.Send(netDataWriter, BasisNetworkCommons.SceneChannel, deliveryMethod);
        }
    }
}