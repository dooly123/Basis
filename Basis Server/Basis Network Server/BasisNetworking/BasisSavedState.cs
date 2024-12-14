using LiteNetLib;
using System.Collections.Concurrent;
using static SerializableBasis;

namespace Basis.Network.Server.Generic
{
    public static class BasisSavedState
    {
        /// <summary>
        /// Stores the last state of each player on the server side.
        /// </summary>
        private static readonly ConcurrentDictionary<int, StoredData> serverSideLastState = new ConcurrentDictionary<int, StoredData>();

        /// <summary>
        /// Removes a player from the store.
        /// </summary>
        /// <param name="client">The client representing the player to be removed.</param>
        public static void RemovePlayer(NetPeer client)
        {
            serverSideLastState.TryRemove(client.Id, out _);
        }

        public static void AddLastData(NetPeer client, LocalAvatarSyncMessage avatarSyncMessage)
        {
            if (serverSideLastState.TryGetValue(client.Id, out var existingData))
            {
                existingData.lastAvatarSyncState = avatarSyncMessage;
                serverSideLastState[client.Id] = existingData;
            }
            else
            {
                serverSideLastState[client.Id] = new StoredData
                {
                    lastAvatarSyncState = avatarSyncMessage
                };
            }
        }

        public static void AddLastData(NetPeer client, ReadyMessage readyMessage)
        {
            if (serverSideLastState.TryGetValue(client.Id, out var existingData))
            {
                existingData.lastAvatarSyncState = readyMessage.localAvatarSyncMessage;
                existingData.lastAvatarChangeState = readyMessage.clientAvatarChangeMessage;
                existingData.playerMetaDataMessage = readyMessage.playerMetaDataMessage;
                serverSideLastState[client.Id] = existingData;

                BNL.Log("Updated " + client.Id + " with AvatarID " + readyMessage.clientAvatarChangeMessage.byteArray.Length);
            }
            else
            {
                serverSideLastState[client.Id] = new StoredData
                {
                    lastAvatarSyncState = readyMessage.localAvatarSyncMessage,
                    lastAvatarChangeState = readyMessage.clientAvatarChangeMessage,
                    playerMetaDataMessage = readyMessage.playerMetaDataMessage
                };

                BNL.Log("Added " + client.Id + " with AvatarID " + readyMessage.clientAvatarChangeMessage.byteArray.Length);
            }
        }

        public static void AddLastData(NetPeer client, VoiceReceiversMessage voiceReceiversMessage)
        {
            if (serverSideLastState.TryGetValue(client.Id, out var existingData))
            {
                existingData.voiceReceiversMessage = voiceReceiversMessage;
                serverSideLastState[client.Id] = existingData;
            }
            else
            {
                serverSideLastState[client.Id] = new StoredData
                {
                    voiceReceiversMessage = voiceReceiversMessage
                };
            }
        }

        public static void AddLastData(NetPeer client, ClientAvatarChangeMessage avatarChangeMessage)
        {
            if (serverSideLastState.TryGetValue(client.Id, out var existingData))
            {
                existingData.lastAvatarChangeState = avatarChangeMessage;
                serverSideLastState[client.Id] = existingData;
            }
            else
            {
                serverSideLastState[client.Id] = new StoredData
                {
                    lastAvatarChangeState = avatarChangeMessage
                };
            }
        }

        /// <summary>
        /// Retrieves the last data message for a player.
        /// </summary>
        /// <param name="client">The client representing the player.</param>
        /// <param name="storedData">The last stored data of the player.</param>
        /// <returns>True if the player is found, otherwise false.</returns>
        public static bool GetLastData(NetPeer client, out StoredData storedData)
        {
            return serverSideLastState.TryGetValue(client.Id, out storedData);
        }

        public struct StoredData
        {
            public LocalAvatarSyncMessage lastAvatarSyncState; // pos 1 & 2, rot, scale, muscles
            public ClientAvatarChangeMessage lastAvatarChangeState; // last avatar state
            public PlayerMetaDataMessage playerMetaDataMessage; //who i am meta
            public VoiceReceiversMessage voiceReceiversMessage; //who am i talking to
        }
    }
}