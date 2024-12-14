using Basis.Network.Core;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Concurrent;
using static BasisNetworkServer;
using static SerializableBasis;

namespace Basis.Network.Server.Generic
{
    public static class BasisNetworkingGeneric
    {
        // Original SceneDataMessage handler
        public static void HandleSceneDataMessage_Recipients_Payload(NetPacketReader Reader, BasisMessageReceivedEventArgs e, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> clients)
        {
            SceneDataMessage SceneDataMessage = new SceneDataMessage();
            SceneDataMessage.Deserialize(Reader);
            HandleSceneServer_Recipients_Payload(SceneDataMessage, BasisNetworkCommons.SceneChannel, e, sender, clients);
        }
        private static void HandleSceneServer_Recipients_Payload(SceneDataMessage sceneDataMessage, byte channel, BasisMessageReceivedEventArgs e, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {

            ServerSceneDataMessage serverSceneDataMessage = new ServerSceneDataMessage
            {
                sceneDataMessage = sceneDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = e.ClientId,
                }
            };

            NetDataWriter Writer = new NetDataWriter();
            Writer.Put(BasisNetworkTag.SceneGenericMessage);
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
                    BasisNetworkServer.BroadcastMessageToClients(Writer, channel, targetedClients, e.SendMode);
                }
            }
            else
            {
                BasisNetworkServer.BroadcastMessageToClients(Writer, channel, sender, allClients, e.SendMode);
            }
        }
        // Original AvatarDataMessage handler
        public static void HandleAvatarDataMessage_Recipients_Payload(NetPacketReader Reader, BasisMessageReceivedEventArgs e, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> clients)
        {
            AvatarDataMessage avatarDataMessage = new AvatarDataMessage();
            avatarDataMessage.Deserialize(Reader);
            HandleAvatarServer_Recipients_Payload(avatarDataMessage, BasisNetworkCommons.AvatarChannel, e.SendMode, sender, clients);
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
            Writer.Put(BasisNetworkTag.AvatarGenericMessage);
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

        // New handler for SceneDataMessage without recipients
        public static void HandleSceneDataMessage_NoRecipients(NetPacketReader Reader, BasisMessageReceivedEventArgs e, NetPeer Peer, ConcurrentDictionary<ushort, NetPeer> clients)
        {
            SceneDataMessage_NoRecipients sceneDataMessage = new SceneDataMessage_NoRecipients();
            sceneDataMessage.Deserialize(Reader);
            HandleSceneServer_NoRecipients(sceneDataMessage, BasisNetworkCommons.SceneChannel, e.SendMode, Peer, clients);
        }

        // New handler for AvatarDataMessage without recipients
        public static void HandleAvatarDataMessage_NoRecipients(NetPacketReader Reader, BasisMessageReceivedEventArgs e, NetPeer Peer, ConcurrentDictionary<ushort, NetPeer> clients)
        {
            AvatarDataMessage_NoRecipients sceneDataMessage = new AvatarDataMessage_NoRecipients();
            sceneDataMessage.Deserialize(Reader);
            HandleAvatarServer_NoRecipients(sceneDataMessage, BasisNetworkCommons.AvatarChannel, e.SendMode, Peer, clients);
        }

        // New handler for SceneDataMessage without recipients and payload
        public static void HandleSceneDataMessage_NoRecipients_NoPayload(NetPacketReader Reader, BasisMessageReceivedEventArgs e, NetPeer Peer, ConcurrentDictionary<ushort, NetPeer> clients)
        {
            SceneDataMessage_NoRecipients_NoPayload sceneDataMessage = new SceneDataMessage_NoRecipients_NoPayload();
            sceneDataMessage.Deserialize(Reader);
            HandleSceneServer_NoRecipients_NoPayload(sceneDataMessage, BasisNetworkCommons.SceneChannel, e.SendMode, Peer, clients);
        }

        // New handler for AvatarDataMessage without recipients and payload
        public static void HandleAvatarDataMessage_NoRecipients_NoPayload(NetPacketReader Reader, BasisMessageReceivedEventArgs e, NetPeer Peer, ConcurrentDictionary<ushort, NetPeer> clients)
        {
            AvatarDataMessage_NoRecipients_NoPayload sceneDataMessage = new AvatarDataMessage_NoRecipients_NoPayload();
            sceneDataMessage.Deserialize(Reader);
            HandleAvatarServer_NoRecipients_NoPayload(sceneDataMessage, BasisNetworkCommons.AvatarChannel, e.SendMode, Peer, clients);
        }

        // Server logic for SceneDataMessage without recipients
        private static void HandleSceneServer_NoRecipients(SceneDataMessage_NoRecipients sceneDataMessage, byte channel, DeliveryMethod method, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {
            ServerSceneDataMessage_NoRecipients serverSceneDataMessage = new ServerSceneDataMessage_NoRecipients
            {
                sceneDataMessage = sceneDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id
                }
            };


            NetDataWriter Writer = new NetDataWriter();
            Writer.Put(BasisNetworkTag.SceneGenericMessage_NoRecipients);
            serverSceneDataMessage.Serialize(Writer);
            BasisNetworkServer.BroadcastMessageToClients(Writer, channel, sender, allClients, method);
        }

        // Server logic for AvatarDataMessage without recipients
        private static void HandleAvatarServer_NoRecipients(AvatarDataMessage_NoRecipients avatarDataMessage, byte channel, DeliveryMethod method, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {
            ServerAvatarDataMessage_NoRecipients serverAvatarDataMessage = new ServerAvatarDataMessage_NoRecipients
            {
                avatarDataMessage = avatarDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id
                }
            };

            NetDataWriter Writer = new NetDataWriter();
            Writer.Put(BasisNetworkTag.AvatarGenericMessage_NoRecipients);
            serverAvatarDataMessage.Serialize(Writer);
            BasisNetworkServer.BroadcastMessageToClients(Writer, channel, sender, allClients, method);
        }

        // Server logic for SceneDataMessage without recipients and payload
        private static void HandleSceneServer_NoRecipients_NoPayload(SceneDataMessage_NoRecipients_NoPayload sceneDataMessage, byte channel, DeliveryMethod method, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {
            ServerSceneDataMessage_NoRecipients_NoPayload serverSceneDataMessage = new ServerSceneDataMessage_NoRecipients_NoPayload
            {
                sceneDataMessage = sceneDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id
                }
            };
            NetDataWriter Writer = new NetDataWriter();
            Writer.Put(BasisNetworkTag.SceneGenericMessage_NoRecipients_NoPayload);
            serverSceneDataMessage.Serialize(Writer);
            BasisNetworkServer.BroadcastMessageToClients(Writer, channel, sender, allClients, method);
        }

        // Server logic for AvatarDataMessage without recipients and payload
        private static void HandleAvatarServer_NoRecipients_NoPayload(AvatarDataMessage_NoRecipients_NoPayload avatarDataMessage, byte channel, DeliveryMethod method, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {

            ServerAvatarDataMessage_NoRecipients_NoPayload serverAvatarDataMessage = new ServerAvatarDataMessage_NoRecipients_NoPayload
            {
                avatarDataMessage = avatarDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id
                }
            };

            NetDataWriter Writer = new NetDataWriter();
            Writer.Put(BasisNetworkTag.AvatarGenericMessage_NoRecipients_NoPayload);
            serverAvatarDataMessage.Serialize(Writer);
            BasisNetworkServer.BroadcastMessageToClients(Writer, channel, sender, allClients, method);
        }
        // New handler for SceneDataMessage with Recipients but no Payload
        public static void HandleSceneDataMessage_Recipients_NoPayload(NetPacketReader Reader, BasisMessageReceivedEventArgs e, NetPeer Sender, ConcurrentDictionary<ushort, NetPeer> clients)
        {
            SceneDataMessage_Recipients_NoPayload sceneDataMessage = new SceneDataMessage_Recipients_NoPayload();
            sceneDataMessage.Deserialize(Reader);
            HandleSceneServer_Recipients_NoPayload(sceneDataMessage, BasisNetworkCommons.SceneChannel, e.SendMode, Sender, clients);
        }
        // New handler for AvatarDataMessage with Recipients but no Payload
        public static void HandleAvatarDataMessage_Recipients_NoPayload(NetPacketReader Reader, BasisMessageReceivedEventArgs e, NetPeer Sender, ConcurrentDictionary<ushort, NetPeer> clients)
        {
            AvatarDataMessage_Recipients_NoPayload avatarDataMessage = new AvatarDataMessage_Recipients_NoPayload();
            avatarDataMessage.Deserialize(Reader);

            HandleAvatarServer_Recipients_NoPayload(avatarDataMessage, BasisNetworkCommons.AvatarChannel, e.SendMode, Sender, clients);
        }

        // Server logic for SceneDataMessage with Recipients but no Payload
        private static void HandleSceneServer_Recipients_NoPayload(SceneDataMessage_Recipients_NoPayload sceneDataMessage, byte channel, DeliveryMethod method, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {
            ServerSceneDataMessage_Recipients_NoPayload serverSceneDataMessage = new ServerSceneDataMessage_Recipients_NoPayload
            {
                sceneDataMessage = sceneDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id
                }
            };

            NetDataWriter Writer = new NetDataWriter();
            Writer.Put(BasisNetworkTag.SceneGenericMessage_Recipients_NoPayload);
            serverSceneDataMessage.Serialize(Writer);
            var targetedClients = new ConcurrentDictionary<ushort, NetPeer>();

            foreach (ushort recipientId in sceneDataMessage.recipients)
            {
                if (allClients.TryGetValue(recipientId, out NetPeer client))
                {
                    targetedClients.TryAdd((ushort)client.Id, client);
                }
            }

            if (targetedClients.Count > 0)
            {
                BasisNetworkServer.BroadcastMessageToClients(Writer, channel, targetedClients, method);
            }
        }
        // Server logic for AvatarDataMessage with Recipients but no Payload
        private static void HandleAvatarServer_Recipients_NoPayload(AvatarDataMessage_Recipients_NoPayload avatarDataMessage, byte channel, DeliveryMethod method, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {
            ServerAvatarDataMessage_Recipients_NoPayload serverAvatarDataMessage = new ServerAvatarDataMessage_Recipients_NoPayload
            {
                avatarDataMessage = avatarDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id
                }
            };

            NetDataWriter Writer = new NetDataWriter();
            Writer.Put(BasisNetworkTag.AvatarGenericMessage_Recipients_NoPayload);
            serverAvatarDataMessage.Serialize(Writer);
            var targetedClients = new ConcurrentDictionary<ushort, NetPeer>();

            foreach (ushort recipientId in avatarDataMessage.recipients)
            {
                if (allClients.TryGetValue(recipientId, out NetPeer client))
                {
                    targetedClients.TryAdd((ushort)client.Id, client);
                }
            }

            if (targetedClients.Count > 0)
            {
                BasisNetworkServer.BroadcastMessageToClients(Writer, channel, targetedClients, method);
            }
        }
    }
}