using Basis.Network.Core;
using Basis.Scripts.BasisSdk;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Profiler;
using DarkRift.Basis_Common.Serializable;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using static SerializableBasis;


public static class BasisNetworkGenericMessages
{
    // Handler for server scene data messages
    public static void HandleServerSceneDataMessage(LiteNetLib.NetPacketReader reader, LiteNetLib.DeliveryMethod deliveryMethod)
    {
        ServerSceneDataMessage ServerSceneDataMessage = new ServerSceneDataMessage();
        ServerSceneDataMessage.Deserialize(reader);
        ushort playerID = ServerSceneDataMessage.playerIdMessage.playerID;
        RemoteSceneDataMessage sceneDataMessage = ServerSceneDataMessage.sceneDataMessage;
        BasisScene.OnNetworkMessageReceived?.Invoke(playerID, sceneDataMessage.messageIndex, sceneDataMessage.payload,deliveryMethod);
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
    public static void HandleServerAvatarDataMessage(LiteNetLib.NetPacketReader reader, LiteNetLib.DeliveryMethod Method)
    {
        BasisNetworkProfiler.ServerAvatarDataMessageCounter.Sample(reader.AvailableBytes);
        ServerAvatarDataMessage serverAvatarDataMessage = new ServerAvatarDataMessage();
        serverAvatarDataMessage.Deserialize(reader);
        ushort avatarLinkID = serverAvatarDataMessage.avatarDataMessage.PlayerIdMessage.playerID; // destination
        if (BasisNetworkManagement.Players.TryGetValue(avatarLinkID, out BasisNetworkedPlayer player))
        {
            if (player.Player == null)
            {
                BasisDebug.LogError("Missing Player! " + avatarLinkID);
                return;
            }
            if (player.Player.BasisAvatar != null)
            {
                RemoteAvatarDataMessage output = serverAvatarDataMessage.avatarDataMessage;
                if (player.Player.BasisAvatar.OnNetworkMessageReceived == null)
                {
                    Debug.LogWarning(player.Player.DisplayName + " Message was Queued But nothing was there to Rec it " + output.messageIndex);
                }
                else
                {
                    player.Player.BasisAvatar.OnNetworkMessageReceived.Invoke(serverAvatarDataMessage.playerIdMessage.playerID, output.messageIndex, output.payload, Method);
                }
            }
            else
            {
                BasisDebug.LogError("Missing Avatar For Message " + serverAvatarDataMessage.playerIdMessage.playerID);
            }
        }
        else
        {
            BasisDebug.Log("Missing Player For Message " + serverAvatarDataMessage.playerIdMessage.playerID);
        }
    }
    // Sending message with different conditions
    public static void OnNetworkMessageSend(ushort messageIndex, byte[] buffer = null, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable, ushort[] recipients = null)
    {
        NetDataWriter netDataWriter = new NetDataWriter();
        //BasisDebug.Log("Sending with Recipients and buffer");
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
        BasisNetworkProfiler.SceneDataMessageCounter.Sample(netDataWriter.Length);
    }
}
