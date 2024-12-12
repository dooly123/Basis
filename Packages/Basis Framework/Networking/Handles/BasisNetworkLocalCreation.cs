using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.Factorys;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using LiteNetLib;
using LiteNetLib.Utils;
using static SerializableBasis;
namespace Basis.Scripts.Networking
{
    public static class BasisNetworkLocalCreation
    {
        public static async Task HandleAuthSuccess(Transform Parent)
        {
            BasisNetworkedPlayer NetworkedPlayer = await BasisPlayerFactoryNetworked.CreateNetworkedPlayer(new InstantiationParameters(Parent.position, Parent.rotation, Parent));
            ushort playerID = BasisNetworkManagement.Instance.Client.ID;
            BasisLocalPlayer BasisLocalPlayer = BasisLocalPlayer.Instance;
            NetworkedPlayer.ReInitialize(BasisLocalPlayer, playerID);
            if (BasisNetworkManagement.AddPlayer(NetworkedPlayer))
            {

                Debug.Log("added local Player " + playerID);
            }
            else
            {
                Debug.LogError("Cant add " + playerID);
            }
            byte[] Information = BasisBundleConversionNetwork.ConvertBasisLoadableBundleToBytes(BasisLocalPlayer.AvatarMetaData);
            Transmitters.BasisNetworkTransmitter Transmitter = (Transmitters.BasisNetworkTransmitter)NetworkedPlayer.NetworkSend;
            BasisNetworkAvatarCompressor.CompressAvatarData(Transmitter, BasisLocalPlayer.Avatar.Animator);

            BasisNetworkManagement.Instance.readyMessage.localAvatarSyncMessage = Transmitter.LASM;
            BasisNetworkManagement.Instance.readyMessage.clientAvatarChangeMessage = new ClientAvatarChangeMessage
            {
                byteArray = Information,
                loadMode = BasisLocalPlayer.AvatarLoadMode,
            };
            BasisNetworkManagement.Instance.readyMessage.playerMetaDataMessage = new PlayerMetaDataMessage
            {
                playerUUID = BasisLocalPlayer.UUID,
                playerDisplayName = BasisLocalPlayer.DisplayName
            };
            NetDataWriter  netDataWriter = new NetDataWriter();
            BasisNetworkManagement.Instance.readyMessage.Serialize(netDataWriter);

            //here will be send out
            BasisNetworkManagement.OnLocalPlayerJoined?.Invoke(NetworkedPlayer, BasisLocalPlayer);
            BasisNetworkManagement.HasSentOnLocalPlayerJoin = true;
        }
    }
}