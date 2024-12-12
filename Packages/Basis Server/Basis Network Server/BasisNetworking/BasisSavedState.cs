using LiteNetLib;
using System.Collections.Generic;
using System.Threading;
using static SerializableBasis;

namespace Basis.Network.Server.Generic
{
    public static class BasisSavedState
    {
        /// <summary>
        /// Stores the last state of each player on the server side.
        /// </summary>
        private static readonly Dictionary<int, StoredData> serverSideLastState = new Dictionary<int, StoredData>();
        private static readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Removes a player from the store.
        /// </summary>
        /// <param name="client">The client representing the player to be removed.</param>
        public static void RemovePlayer(NetPeer client)
        {
            rwLock.EnterWriteLock();
            try
            {
                serverSideLastState.Remove(client.Id);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static void AddLastData(NetPeer client, LocalAvatarSyncMessage avatarSyncMessage)
        {
            rwLock.EnterWriteLock();
            try
            {
                if (!serverSideLastState.TryGetValue(client.Id, out var existingData))
                {
                    existingData = new StoredData();
                    serverSideLastState[client.Id] = existingData;
                }
                existingData.lastAvatarSyncState = avatarSyncMessage;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static void AddLastData(NetPeer client, LocalAvatarSyncMessage avatarSyncMessage, ClientAvatarChangeMessage avatarChangeMessage)
        {
            rwLock.EnterWriteLock();
            try
            {
                if (!serverSideLastState.TryGetValue(client.Id, out var existingData))
                {
                    existingData = new StoredData();
                    serverSideLastState[client.Id] = existingData;
                }
                existingData.lastAvatarSyncState = avatarSyncMessage;
                existingData.lastAvatarChangeState = avatarChangeMessage;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static void AddLastData(NetPeer client, ReadyMessage readyMessage)
        {
            rwLock.EnterWriteLock();
            try
            {
                if (!serverSideLastState.TryGetValue(client.Id, out var existingData))
                {
                    existingData = new StoredData();
                    serverSideLastState[client.Id] = existingData;
                }
                existingData.lastAvatarSyncState = readyMessage.localAvatarSyncMessage;
                existingData.lastAvatarChangeState = readyMessage.clientAvatarChangeMessage;
                existingData.playerMetaDataMessage = readyMessage.playerMetaDataMessage;

                BNL.Log("Added " + client.Id + " with AvatarID " + readyMessage.clientAvatarChangeMessage.byteArray);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static void AddLastData(NetPeer client, VoiceReceiversMessage voiceReceiversMessage)
        {
            rwLock.EnterWriteLock();
            try
            {
                if (!serverSideLastState.TryGetValue(client.Id, out var existingData))
                {
                    existingData = new StoredData();
                    serverSideLastState[client.Id] = existingData;
                }
                existingData.voiceReceiversMessage = voiceReceiversMessage;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static void AddLastData(NetPeer client, ClientAvatarChangeMessage avatarChangeMessage)
        {
            rwLock.EnterWriteLock();
            try
            {
                if (!serverSideLastState.TryGetValue(client.Id, out var existingData))
                {
                    existingData = new StoredData();
                    serverSideLastState[client.Id] = existingData;
                }
                existingData.lastAvatarChangeState = avatarChangeMessage;
            }
            finally
            {
                rwLock.ExitWriteLock();
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
            rwLock.EnterReadLock();
            try
            {
                return serverSideLastState.TryGetValue(client.Id, out storedData);
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public struct StoredData
        {
            public LocalAvatarSyncMessage lastAvatarSyncState; // pos 1 & 2, rot, scale, muscles
            public ClientAvatarChangeMessage lastAvatarChangeState; // last avatar state
            public PlayerMetaDataMessage playerMetaDataMessage; //who i am meta
            public VoiceReceiversMessage voiceReceiversMessage;//who am i talking to
        }
    }
}