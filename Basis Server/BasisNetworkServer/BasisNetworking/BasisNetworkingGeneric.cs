using Basis.Network.Core;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Concurrent;
using static SerializableBasis;

namespace Basis.Network.Server.Generic
{
    public static class BasisNetworkingGeneric
    {
        public static void HandleScene(NetPacketReader Reader, DeliveryMethod DeliveryMethod, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {
            SceneDataMessage SceneDataMessage = new SceneDataMessage();
            SceneDataMessage.Deserialize(Reader);

            ushort[] Rec = SceneDataMessage.recipients;
            SceneDataMessage.recipients = null;
            SceneDataMessage.recipientsSize = 0;
            ServerSceneDataMessage serverSceneDataMessage = new ServerSceneDataMessage
            {
                sceneDataMessage = SceneDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id,
                }
            };
            byte Channel = BasisNetworkCommons.SceneChannel;
            NetDataWriter Writer = new NetDataWriter();
            if (DeliveryMethod == DeliveryMethod.Unreliable)
            {
                Writer.Put(Channel);
                Channel = BasisNetworkCommons.FallChannel;
            }
            serverSceneDataMessage.Serialize(Writer);
            if (Rec != null && Rec.Length > 0)
            {
                var targetedClients = new ConcurrentDictionary<ushort, NetPeer>();

                for (int Index = 0; Index < Rec.Length; Index++)
                {
                    ushort recipientId = Rec[Index];
                    if (allClients.TryGetValue(recipientId, out NetPeer client))
                    {
                        targetedClients.TryAdd((ushort)client.Id, client);
                    }
                }

                if (targetedClients.Count > 0)
                {
                    BasisNetworkServer.BroadcastMessageToClients(Writer, Channel, targetedClients, DeliveryMethod);
                }
            }
            else
            {
                BasisNetworkServer.BroadcastMessageToClients(Writer, Channel, sender, allClients, DeliveryMethod);
            }
        }
        public static void HandleAvatar(NetPacketReader Reader, DeliveryMethod DeliveryMethod, NetPeer sender)
        {
            AvatarDataMessage avatarDataMessage = new AvatarDataMessage();
            avatarDataMessage.Deserialize(Reader);
            ServerAvatarDataMessage serverAvatarDataMessage = new ServerAvatarDataMessage
            {
                avatarDataMessage = new RemoteAvatarDataMessage()
                {
                    messageIndex = avatarDataMessage.messageIndex,
                    payload = avatarDataMessage.payload,
                    PlayerIdMessage = avatarDataMessage.PlayerIdMessage
                },
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id
                }
            };
            byte Channel = BasisNetworkCommons.AvatarChannel;
            NetDataWriter Writer = new NetDataWriter();
            if (DeliveryMethod == DeliveryMethod.Unreliable)
            {
                Writer.Put(Channel);
                Channel = BasisNetworkCommons.FallChannel;
            }
            serverAvatarDataMessage.Serialize(Writer);
            if (avatarDataMessage.recipientsSize != 0)
            {
                ConcurrentDictionary<ushort, NetPeer> targetedClients = new ConcurrentDictionary<ushort, NetPeer>();

                int recipientsLength = avatarDataMessage.recipientsSize;
                BNL.Log("Query Recipients " + recipientsLength);
                for (int index = 0; index < recipientsLength; index++)
                {
                    if (BasisNetworkServer.Peers.TryGetValue(avatarDataMessage.recipients[index], out NetPeer client))
                    {
                        BNL.Log("Found Peer! " + avatarDataMessage.recipients[index]);
                        targetedClients.TryAdd((ushort)client.Id, client);
                    }
                    else
                    {
                        BNL.Log("Missing Peer! " + avatarDataMessage.recipients[index]);
                    }
                }

                if (targetedClients.Count > 0)
                {
                    BNL.Log("Sending out Target Clients " + targetedClients.Count);
                    BasisNetworkServer.BroadcastMessageToClients(Writer, Channel, targetedClients, DeliveryMethod);
                }
            }
            else
            {
                BasisNetworkServer.BroadcastMessageToClients(Writer, Channel, sender, BasisNetworkServer.Peers, DeliveryMethod);
            }
        }
    }
}
