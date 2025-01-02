using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.Recievers;
using Basis.Scripts.Player;

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using static SerializableBasis;

namespace Basis.Scripts.Networking
{
    public static class BasisNetworkHandleRemote
    {
        public static async Task HandleCreateRemotePlayer(LiteNetLib.NetPacketReader reader,Transform Parent)
        {
            BasisDebug.Log("Handling Create Remote Player!");
            ServerReadyMessage ServerReadyMessage = new ServerReadyMessage();
            ServerReadyMessage.Deserialize(reader);
            await CreateRemotePlayer(ServerReadyMessage, Parent);
        }
        public static async Task HandleCreateAllRemoteClients(LiteNetLib.NetPacketReader reader, Transform Parent)
        {
            CreateAllRemoteMessage createAllRemoteMessage = new CreateAllRemoteMessage();
            createAllRemoteMessage.Deserialize(reader);
            int RemoteLength = createAllRemoteMessage.serverSidePlayer.Length;
            BasisDebug.Log("Handling Create All Remote Players! Total Connections To Create " + RemoteLength);
            // Create a list to hold the tasks
            List<Task> tasks = new List<Task>();

            // Start all tasks without awaiting them
            for (int PlayerIndex = 0; PlayerIndex < RemoteLength; PlayerIndex++)
            {
                tasks.Add(CreateRemotePlayer(createAllRemoteMessage.serverSidePlayer[PlayerIndex], Parent));
            }

            // Await all tasks at once
            await Task.WhenAll(tasks);
        }
        public static async Task<BasisNetworkedPlayer> CreateRemotePlayer(ServerReadyMessage ServerReadyMessage, InstantiationParameters instantiationParameters)
        {

            ClientAvatarChangeMessage avatarID = ServerReadyMessage.localReadyMessage.clientAvatarChangeMessage;

            if (avatarID.byteArray != null)
            {
                BasisNetworkManagement.JoiningPlayers.Add(ServerReadyMessage.playerIdMessage.playerID);

                // Start both tasks simultaneously
                Task<BasisRemotePlayer> createRemotePlayerTask = BasisPlayerFactory.CreateRemotePlayer(instantiationParameters, avatarID, ServerReadyMessage.localReadyMessage.playerMetaDataMessage);
                BasisNetworkedPlayer BasisNetworkedPlayer = new BasisNetworkedPlayer();

                BasisNetworkedPlayer.ProvideNetworkKey(ServerReadyMessage.playerIdMessage.playerID);
                // Retrieve the results
                BasisRemotePlayer remote = await createRemotePlayerTask;
                // Continue with the rest of the code
                BasisNetworkedPlayer.RemoteInitalization(remote);
                if (BasisNetworkManagement.AddPlayer(BasisNetworkedPlayer))
                {
                    BasisDebug.Log("Added Player AT " + BasisNetworkedPlayer.NetId);
                }
                else
                {
                    BasisDebug.LogError("Critical issue could not add player to data");
                    return null;
                }
                BasisNetworkedPlayer.InitalizeNetwork();//fires events and makes us network compatible
                BasisDebug.Log("Added Player " + ServerReadyMessage.playerIdMessage.playerID);
                BasisNetworkReceiver Rec =(BasisNetworkReceiver)BasisNetworkedPlayer.NetworkSend;
                BasisNetworkAvatarDecompressor.DecompressAndProcessAvatar(Rec, ServerReadyMessage.localReadyMessage.localAvatarSyncMessage);
                BasisNetworkManagement.OnRemotePlayerJoined?.Invoke(BasisNetworkedPlayer, remote);

                BasisNetworkManagement.JoiningPlayers.Remove(ServerReadyMessage.playerIdMessage.playerID);
                await remote.LoadAvatarFromInital(avatarID);

                return BasisNetworkedPlayer;
            }
            else
            {
                BasisDebug.LogError("Empty Avatar ID for Player fatal error! " + ServerReadyMessage.playerIdMessage.playerID);
                return null;
            }
        }
        public static async Task<BasisNetworkedPlayer> CreateRemotePlayer(ServerReadyMessage ServerReadyMessage,Transform Parent)
        {
            InstantiationParameters instantiationParameters = new InstantiationParameters(Vector3.zero, Quaternion.identity, Parent);
            return await CreateRemotePlayer(ServerReadyMessage, instantiationParameters);
        }
    }
}
