using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.Factorys;
using Basis.Scripts.Networking.NetworkedPlayer;
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
            Debug.Log("Handling Create Remote Player!");
            ServerReadyMessage ServerReadyMessage = new ServerReadyMessage();
            ServerReadyMessage.Deserialize(reader);
            await CreateRemotePlayer(ServerReadyMessage, Parent);
        }
        public static async Task HandleCreateAllRemoteClients(LiteNetLib.NetPacketReader reader, Transform Parent)
        {
            CreateAllRemoteMessage createAllRemoteMessage = new CreateAllRemoteMessage();
            createAllRemoteMessage.Deserialize(reader);
            int RemoteLength = createAllRemoteMessage.serverSidePlayer.Length;
            Debug.Log("Handling Create All Remote Players! Total Connections To Create " + RemoteLength);
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
                Task<BasisNetworkedPlayer> createNetworkedPlayerTask = BasisPlayerFactoryNetworked.CreateNetworkedPlayer(instantiationParameters);

                // Wait for both tasks to complete
                await Task.WhenAll(createRemotePlayerTask, createNetworkedPlayerTask);

                // Retrieve the results
                BasisRemotePlayer remote = await createRemotePlayerTask;
                BasisNetworkedPlayer networkedPlayer = await createNetworkedPlayerTask;

                // Continue with the rest of the code
                networkedPlayer.RemoteInitalization(remote, ServerReadyMessage.playerIdMessage.playerID);

                if (BasisNetworkManagement.AddPlayer(networkedPlayer))
                {
                    Debug.Log("Added Player " + ServerReadyMessage.playerIdMessage.playerID);
                    BasisNetworkManagement.OnRemotePlayerJoined?.Invoke(networkedPlayer, remote);
                }

                BasisNetworkManagement.JoiningPlayers.Remove(ServerReadyMessage.playerIdMessage.playerID);
                await remote.LoadAvatarFromInital(avatarID);

                return networkedPlayer;
            }
            else
            {
                Debug.LogError("Empty Avatar ID for Player fatal error! " + ServerReadyMessage.playerIdMessage.playerID);
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