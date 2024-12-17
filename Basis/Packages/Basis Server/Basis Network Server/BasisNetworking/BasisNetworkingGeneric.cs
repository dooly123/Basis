using Basis.Network.Core;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Concurrent;
using static SerializableBasis;

namespace Basis.Network.Server.Generic
{
    public static class BasisNetworkingGeneric
    {
        public static void HandleSceneDataMessage_Recipients_Payload(NetPacketReader Reader, DeliveryMethod DeliveryMethod, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> clients)
        {
            SceneDataMessage SceneDataMessage = new SceneDataMessage();
            SceneDataMessage.Deserialize(Reader);
            HandleSceneServer_Recipients_Payload(SceneDataMessage, BasisNetworkCommons.SceneChannel, DeliveryMethod, sender, clients);
        }
        public static void HandleAvatarDataMessage_Recipients_Payload(NetPacketReader Reader, DeliveryMethod DeliveryMethod, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> clients)
        {
            AvatarDataMessage avatarDataMessage = new AvatarDataMessage();
            avatarDataMessage.Deserialize(Reader);
            HandleAvatarServer_Recipients_Payload(avatarDataMessage, BasisNetworkCommons.AvatarChannel, DeliveryMethod, sender, clients);
        }

        private static void HandleSceneServer_Recipients_Payload(SceneDataMessage sceneDataMessage, byte channel,  DeliveryMethod DeliveryMethod, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {

            ServerSceneDataMessage serverSceneDataMessage = new ServerSceneDataMessage
            {
                sceneDataMessage = sceneDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id,
                }
            };

            NetDataWriter Writer = new NetDataWriter();
            if (serverSceneDataMessage.sceneDataMessage.recipients != null && serverSceneDataMessage.sceneDataMessage.recipients.Length > 0)
            {
                var targetedClients = new ConcurrentDictionary<ushort, NetPeer>();

                foreach (ushort recipientId in serverSceneDataMessage.sceneDataMessage.recipients)
                {
                    if (allClients.TryGetValue(recipientId, out NetPeer client))
                    {
                        targetedClients.TryAdd((ushort)client.Id, client);
                    }
                }

                if (targetedClients.Count > 0)
                {
                    BasisNetworkServer.BroadcastMessageToClients(Writer, channel, targetedClients, DeliveryMethod);
                }
            }
            else
            {
                BasisNetworkServer.BroadcastMessageToClients(Writer, channel, sender, allClients, DeliveryMethod);
            }
        }
        private static void HandleAvatarServer_Recipients_Payload(AvatarDataMessage avatarDataMessage, byte channel, DeliveryMethod method, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {
            ServerAvatarDataMessage serverAvatarDataMessage = new ServerAvatarDataMessage
            {
                avatarDataMessage = avatarDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id
                }
            };

            NetDataWriter Writer = new NetDataWriter();
            serverAvatarDataMessage.Serialize(Writer);
            if (avatarDataMessage.recipients != null && avatarDataMessage.recipients.Length > 0)
            {
                var targetedClients = new ConcurrentDictionary<ushort, NetPeer>();

                int recipientsLength = avatarDataMessage.recipients.Length;
                for (int index = 0; index < recipientsLength; index++)
                {
                    if (allClients.TryGetValue(avatarDataMessage.recipients[index], out NetPeer client))
                    {
                        targetedClients.TryAdd((ushort)client.Id, client);
                    }
                }

                if (targetedClients.Count > 0)
                {
                    BasisNetworkServer.BroadcastMessageToClients(Writer, channel, targetedClients, method);
                }
            }
            else
            {
                BasisNetworkServer.BroadcastMessageToClients(Writer, channel, sender, allClients, method);
            }
        }
    }
}