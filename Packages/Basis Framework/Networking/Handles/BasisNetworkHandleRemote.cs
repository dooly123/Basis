using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Networking.Factorys;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Player;
using DarkRift;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using static SerializableDarkRift;
namespace Basis.Scripts.Networking
{
    public static class BasisNetworkHandleRemote
    {
        public static async Task HandleCreateRemotePlayer(DarkRiftReader reader,Transform Parent)
        {
            reader.Read(out ServerReadyMessage SRM);
            await CreateRemotePlayer(SRM, Parent);
        }
        public static async Task HandleCreateAllRemoteClients(DarkRiftReader reader, Transform Parent)
        {
            reader.Read(out CreateAllRemoteMessage allRemote);
            int RemoteLength = allRemote.serverSidePlayer.Length;

            // Create a list to hold the tasks
            List<Task> tasks = new List<Task>();

            // Start all tasks without awaiting them
            for (int PlayerIndex = 0; PlayerIndex < RemoteLength; PlayerIndex++)
            {
                tasks.Add(CreateRemotePlayer(allRemote.serverSidePlayer[PlayerIndex], Parent));
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
                networkedPlayer.ReInitialize(remote, ServerReadyMessage.playerIdMessage.playerID, ServerReadyMessage.localReadyMessage.localAvatarSyncMessage);

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