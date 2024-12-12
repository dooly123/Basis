using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Basis.Network.Core;
using Basis.Network.Core.Compression;
using Basis.Scripts.Networking.Compression;
using LiteNetLib;
using LiteNetLib.Utils;
using static SerializableBasis;
public class BasisServerReductionSystem
{
    // Default interval in milliseconds for the timer
    public static int MillisecondDefaultInterval = 50;
    public static float BaseMultiplier = 1f; // Starting multiplier.
    public static float IncreaseRate = 0.005f; // Rate of increase per unit distance.
    private static readonly Dictionary<NetPeer, SyncedToPlayerPulse> PlayerSync = new Dictionary<NetPeer, SyncedToPlayerPulse>();
    private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

    public static void AddOrUpdatePlayer(NetPeer playerID, SyncedToPlayerPulse data)
    {
        Lock.EnterWriteLock();
        try
        {
            PlayerSync[playerID] = data;
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }
    /// <summary>
    /// Retrieves all SyncedToPlayerPulse instances from the dictionary.
    /// </summary>
    /// <returns>A list containing all SyncedToPlayerPulse instances.</returns>
    public static List<SyncedToPlayerPulse> GetAllSyncedToPlayerPulses()
    {
        Lock.EnterReadLock();
        try
        {
            // Return a copy of the values to ensure thread safety
            return PlayerSync.Values.ToList();
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }
    public static bool TryGetPlayer(NetPeer playerID, out SyncedToPlayerPulse data)
    {
        Lock.EnterReadLock();
        try
        {
            return PlayerSync.TryGetValue(playerID, out data);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }
    public static bool RemovePlayer(NetPeer playerID,out SyncedToPlayerPulse output)
    {
        Lock.EnterWriteLock();
        try
        {
            output =  PlayerSync[playerID];
            return PlayerSync.Remove(playerID);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }
    public struct ClientPayload
    {
        public NetPeer localClient;
        public NetPeer dataCameFromThisUser;
    }
    /// <summary>
    /// Structure representing a player's server-side data that can be reduced.
    /// </summary>
    public struct ServerSideReducablePlayer
    {
        public System.Threading.Timer timer;//create a new timer
        public bool newDataExists;
        public ServerSideSyncPlayerMessage serverSideSyncPlayerMessage;
    }
    /// <summary>
    /// add the new client
    /// then update all existing clients arrays
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="playerToUpdate"></param>
    /// <param name="serverSideSyncPlayer"></param>
    public static void AddOrUpdatePlayer(NetPeer playerID, ServerSideSyncPlayerMessage playerToUpdate, NetPeer serverSideSyncPlayer)
    {
        //stage 1 lets update whoever send us this datas last player information
        if (TryGetPlayer(serverSideSyncPlayer, out SyncedToPlayerPulse playerData))
        {
            playerData.lastPlayerInformation = playerToUpdate;
        }
        //ok now we can try to schedule sending out this data!
        if (TryGetPlayer(playerID, out playerData))
        {
            // Update the player's message
            playerData.SupplyNewData(playerID, playerToUpdate, serverSideSyncPlayer);
        }
        else
        {
            //first time request create said data!
            playerData = new SyncedToPlayerPulse
            {
                playerID = playerID,
              //  queuedPlayerMessages = new Dictionary<NetPeer, ServerSideReducablePlayer>()
                lastPlayerInformation = playerToUpdate,
            };
            //ok now we can try to schedule sending out this data!
            AddOrUpdatePlayer(playerID, playerData);
            playerData.SupplyNewData(playerID, playerToUpdate, serverSideSyncPlayer);
        }
    }
    public static void RemovePlayer(NetPeer playerID)
    {
        if (RemovePlayer(playerID, out SyncedToPlayerPulse pulse))
        {
            foreach (ServerSideReducablePlayer player in pulse.GetAllSyncedToqueued())
            {
                player.timer.Dispose();
            }
        }
        foreach (SyncedToPlayerPulse player in GetAllSyncedToPlayerPulses())
        {
            if (player.Removequeued(playerID, out ServerSideReducablePlayer reduceablePlayer))
            {
                reduceablePlayer.timer.Dispose();
            }
        }
    }
    /// <summary>
    /// Structure to synchronize data with a specific player.
    /// </summary>
    public class SyncedToPlayerPulse
    {
        // The player ID to which the data is being sent
        public NetPeer playerID;
        public ServerSideSyncPlayerMessage lastPlayerInformation;
        /// <summary>
        /// Dictionary to hold queued messages for each player.
        /// Key: Player ID, Value: Server-side player data
        /// </summary>
        public readonly Dictionary<NetPeer, ServerSideReducablePlayer> queuedPlayerMessages = new Dictionary<NetPeer, ServerSideReducablePlayer>();
        public readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        public void AddOrUpdatequeued(NetPeer playerID, ServerSideReducablePlayer data)
        {
            Lock.EnterWriteLock();
            try
            {
                queuedPlayerMessages[playerID] = data;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
        /// <summary>
        /// Retrieves all SyncedToPlayerPulse instances from the dictionary.
        /// </summary>
        /// <returns>A list containing all SyncedToPlayerPulse instances.</returns>
        public List<ServerSideReducablePlayer> GetAllSyncedToqueued()
        {
            Lock.EnterReadLock();
            try
            {
                // Return a copy of the values to ensure thread safety
                return queuedPlayerMessages.Values.ToList();
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }
        public bool TryGetqueued(NetPeer playerID, out ServerSideReducablePlayer data)
        {
            Lock.EnterReadLock();
            try
            {
                return queuedPlayerMessages.TryGetValue(playerID, out data);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }
        public bool Removequeued(NetPeer playerID, out ServerSideReducablePlayer output)
        {
            Lock.EnterWriteLock();
            try
            {
                output = queuedPlayerMessages[playerID];
                return queuedPlayerMessages.Remove(playerID);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
        /// <summary>
        /// Supply new data to a specific player.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <param name="serverSideSyncPlayerMessage">The message to be synced</param>
        /// <param name="serverSidePlayer"></param>
        public void SupplyNewData(NetPeer playerID, ServerSideSyncPlayerMessage serverSideSyncPlayerMessage, NetPeer serverSidePlayer)
        {
            if (TryGetqueued(serverSidePlayer, out ServerSideReducablePlayer playerData))
            {
                // Update the player's message
                playerData.serverSideSyncPlayerMessage = serverSideSyncPlayerMessage;
                playerData.newDataExists = true;
                AddOrUpdatequeued(serverSidePlayer, playerData);
            }
            else
            {
                // If the player doesn't exist, add them with default settings
                AddPlayer(playerID, serverSideSyncPlayerMessage, serverSidePlayer);
            }
        }

        /// <summary>
        /// Adds a new player to the queue with a default timer and settings.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <param name="serverSideSyncPlayerMessage">The initial message to be sent</param>
        /// <param name="serverSidePlayer"></param>
        public void AddPlayer(NetPeer playerID, ServerSideSyncPlayerMessage serverSideSyncPlayerMessage, NetPeer serverSidePlayer)
        {
            ClientPayload clientPayload = new ClientPayload
            {
                localClient = playerID,
                dataCameFromThisUser = serverSidePlayer
            };
            ServerSideReducablePlayer newPlayer = new ServerSideReducablePlayer
            {
                serverSideSyncPlayerMessage = serverSideSyncPlayerMessage,
                newDataExists = true,
                timer = new System.Threading.Timer(SendPlayerData, clientPayload, MillisecondDefaultInterval, MillisecondDefaultInterval)
            };
            AddOrUpdatequeued(serverSidePlayer, newPlayer);
        }

        /// <summary>
        /// Removes a player from the queue and disposes of their timer.
        /// </summary>
        /// <param name="playerID">The ID of the player to remove</param>
        public void RemovePlayer(NetPeer playerID)
        {
            if (Removequeued(playerID, out ServerSideReducablePlayer playerData))
            {
                // Dispose of the timer to free resources
                playerData.timer.Dispose();
            }
        }
        /// <summary>
        /// Callback function to send player data at regular intervals.
        /// </summary>
        /// <param name="state">The player ID (passed from the timer)</param>
        private void SendPlayerData(object state)
        {
            if (state is ClientPayload playerID && TryGetqueued(playerID.dataCameFromThisUser, out ServerSideReducablePlayer playerData))
            {
                if (playerData.newDataExists)
                {
                    if (TryGetPlayer(playerID.localClient, out SyncedToPlayerPulse pulse))
                    {
                        Vector3 from = BasisNetworkCompressionExtensions.DecompressAndProcessAvatar(pulse.lastPlayerInformation);
                        Vector3 to = BasisNetworkCompressionExtensions.DecompressAndProcessAvatar(playerData.serverSideSyncPlayerMessage);
                        // Calculate the distance between the two points
                        float activeDistance = Distance(from, to);
                        // Adjust the timer interval based on the new syncRateMultiplier
                        int adjustedInterval = (int)(MillisecondDefaultInterval * (BaseMultiplier + (activeDistance * IncreaseRate)));
                        if (adjustedInterval > byte.MaxValue)
                        {
                            adjustedInterval = byte.MaxValue;
                        }
                        //   BNL.Log("Adjusted Interval is" + adjustedInterval);
                        playerData.timer.Change(adjustedInterval, adjustedInterval);
                        //how long does this data need to last for
                        playerData.serverSideSyncPlayerMessage.interval = (byte)adjustedInterval;
                    }
                    else
                    {
                        BNL.Log("Unable to find Pulse for LocalClient Wont Do Interval Adjust");
                    }
                    if (playerData.serverSideSyncPlayerMessage.avatarSerialization.array == null || playerData.serverSideSyncPlayerMessage.avatarSerialization.array.Length == 0)
                    {
                        BNL.Log("Unable to send out Avatar Data Was null or Empty!");
                    }
                    else
                    {
                        NetDataWriter Writer = new NetDataWriter();
                        playerData.serverSideSyncPlayerMessage.Serialize(Writer);
                        playerID.localClient.Send(Writer, BasisNetworkCommons.MovementChannel, DeliveryMethod.Sequenced);
                        playerData.newDataExists = false;
                    }
                }
            }
        }
        public static float Distance(Vector3 pointA, Vector3 pointB)
        {
            Vector3 difference = pointB - pointA;
            return difference.SquaredMagnitude();
        }
    }
}