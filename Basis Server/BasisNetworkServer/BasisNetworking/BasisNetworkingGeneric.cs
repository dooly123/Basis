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

            NetDataWriter Writer = new NetDataWriter();
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
                    BasisNetworkServer.BroadcastMessageToClients(Writer, BasisNetworkCommons.SceneChannel, targetedClients, DeliveryMethod);
                }
            }
            else
            {
                BasisNetworkServer.BroadcastMessageToClients(Writer, BasisNetworkCommons.SceneChannel, sender, allClients, DeliveryMethod);
            }
        }
        public static void HandleAvatar(NetPacketReader Reader, DeliveryMethod DeliveryMethod, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> allClients)
        {
            AvatarDataMessage avatarDataMessage = new AvatarDataMessage();
            avatarDataMessage.Deserialize(Reader);
            var Rec = avatarDataMessage.recipients;
            avatarDataMessage.recipients = null;
            avatarDataMessage.recipientsSize = 0;
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
            if (Rec != null && Rec.Length > 0)
            {
                var targetedClients = new ConcurrentDictionary<ushort, NetPeer>();

                int recipientsLength = Rec.Length;
                for (int index = 0; index < recipientsLength; index++)
                {
                    if (allClients.TryGetValue(Rec[index], out NetPeer client))
                    {
                        targetedClients.TryAdd((ushort)client.Id, client);
                    }
                }

                if (targetedClients.Count > 0)
                {
                    BasisNetworkServer.BroadcastMessageToClients(Writer, BasisNetworkCommons.AvatarChannel, targetedClients, DeliveryMethod);
                }
            }
            else
            {
                BasisNetworkServer.BroadcastMessageToClients(Writer, BasisNetworkCommons.AvatarChannel, sender, allClients, DeliveryMethod);
            }
        }
    }
}