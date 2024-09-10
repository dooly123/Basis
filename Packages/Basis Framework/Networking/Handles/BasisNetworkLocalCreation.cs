using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.Factorys;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using DarkRift;
using DarkRift.Server.Plugins.Commands;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using static SerializableDarkRift;

namespace Basis.Scripts.Networking
{
    public static class BasisNetworkLocalCreation
    {
        public static async Task HandleAuthSuccess(Transform Parent)
        {
            BasisNetworkedPlayer player = await BasisPlayerFactoryNetworked.CreateNetworkedPlayer(new InstantiationParameters(Parent.position, Parent.rotation, Parent));
            ushort playerID = BasisNetworkManagement.Instance.Client.ID;
            BasisLocalPlayer BasisLocalPlayer = BasisLocalPlayer.Instance;
            player.ReInitialize(BasisLocalPlayer.Instance, playerID);
            if (BasisNetworkManagement.Instance.Players.TryAdd(playerID, player))
            {

                Debug.Log("added local Player " + BasisNetworkManagement.Instance.Client.ID);
            }
            else
            {
                Debug.LogError("Cant add " + playerID);
            }
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                BasisNetworkAvatarCompressor.CompressIntoSendBase(player.NetworkSend, BasisLocalPlayer.Avatar.Animator);
                BasisNetworkManagement.Instance.readyMessage.localAvatarSyncMessage = player.NetworkSend.LASM;
                BasisNetworkManagement.Instance.readyMessage.clientAvatarChangeMessage = new ClientAvatarChangeMessage
                {
                    avatarID = BasisLocalPlayer.AvatarUrl,
                    loadMode = BasisLocalPlayer.AvatarLoadMode,
                };
                BasisNetworkManagement.Instance.readyMessage.playerMetaDataMessage = new PlayerMetaDataMessage
                {
                    playerUUID = BasisLocalPlayer.UUID,
                    playerDisplayName = BasisLocalPlayer.DisplayName
                };
                writer.Write(BasisNetworkManagement.Instance.readyMessage);
                Message ReadyMessage = Message.Create(BasisTags.ReadyStateTag, writer);
                BasisNetworkManagement.Instance.Client.SendMessage(ReadyMessage, BasisNetworking.EventsChannel, DeliveryMethod.ReliableOrdered);
                BasisNetworkManagement.OnLocalPlayerJoined?.Invoke(player, BasisLocalPlayer);
                BasisNetworkManagement.HasSentOnLocalPlayerJoin = true;
            }
        }
    }
}