using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.Factorys;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Player;
using DarkRift;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using static SerializableDarkRift;
namespace Basis.Scripts.Networking
{
    public static class BasisNetworkCreateRemote
    {
        public static async Task HandleCreateRemotePlayer(DarkRiftReader reader,Transform Parent)
        {
            reader.Read(out ServerReadyMessage SRM);
            await BasisNetworkCreateRemote.CreateRemotePlayer(SRM, Parent);
        }
        public static async Task HandleCreateAllRemoteClients(DarkRiftReader reader, Transform Parent)
        {
            reader.Read(out CreateAllRemoteMessage allRemote);
            int RemoteLength = allRemote.serverSidePlayer.Length;
            for (int PlayerIndex = 0; PlayerIndex < RemoteLength; PlayerIndex++)
            {
                await BasisNetworkCreateRemote.CreateRemotePlayer(allRemote.serverSidePlayer[PlayerIndex], Parent);
            }
        }
        public static async Task<BasisNetworkedPlayer> CreateRemotePlayer(ServerReadyMessage ServerReadyMessage, InstantiationParameters instantiationParameters)
        {
            string avatarID = ServerReadyMessage.localReadyMessage.clientAvatarChangeMessage.avatarID;

            if (!string.IsNullOrEmpty(avatarID))
            {
                BasisRemotePlayer remote = await BasisPlayerFactory.CreateRemotePlayer(instantiationParameters, avatarID, ServerReadyMessage.localReadyMessage.playerMetaDataMessage);
                BasisNetworkedPlayer networkedPlayer = await BasisPlayerFactoryNetworked.CreateNetworkedPlayer(instantiationParameters);

                networkedPlayer.ReInitialize(remote, ServerReadyMessage.playerIdMessage.playerID, ServerReadyMessage.localReadyMessage.localAvatarSyncMessage);

                if (BasisNetworkManagement.Instance.Players.TryAdd(ServerReadyMessage.playerIdMessage.playerID, networkedPlayer))
                {
                    Debug.Log("Added Player " + ServerReadyMessage.playerIdMessage.playerID);
                }

                return networkedPlayer;
            }
            else
            {
                Debug.LogError("Empty Avatar ID for Player " + ServerReadyMessage.playerIdMessage.playerID);
                return null;
            }
        }
        public static async Task<BasisNetworkedPlayer> CreateRemotePlayer(ServerReadyMessage ServerReadyMessage, Transform Parent)
        {
            InstantiationParameters instantiationParameters = new InstantiationParameters(Vector3.zero, Quaternion.identity, Parent);
            return await CreateRemotePlayer(ServerReadyMessage, instantiationParameters);
        }
    }
}