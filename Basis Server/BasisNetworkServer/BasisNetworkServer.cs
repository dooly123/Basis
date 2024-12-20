using Basis.Network.Core;
using Basis.Network.Server;
using Basis.Network.Server.Generic;
using Basis.Network.Server.Ownership;
using Basis.Network.Server.Password;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Basis.Network.Core.Serializable.SerializableBasis;
using static Basis.Network.Server.Generic.BasisSavedState;
using static SerializableBasis;
public static class BasisNetworkServer
{
    public static EventBasedNetListener listener;
    public static NetManager server;
    public static ConcurrentDictionary<ushort, NetPeer> Peers = new ConcurrentDictionary<ushort, NetPeer>();
    public static Configuration Configuration;

    public static void StartServer(Configuration configuration)
    {
        Configuration = configuration;
        BasisServerReductionSystem.Configuration = configuration;

        SetupServer(configuration);
        SetupServerEvents(configuration);

        if (configuration.EnableStatistics)
        {
            BasisStatistics.StartWorkerThread(BasisNetworkServer.server);
        }
        BNL.Log("Server Worker Threads Booted");

    }

    #region Server Setup
    private static void SetupServer(Configuration configuration)
    {
        listener = new EventBasedNetListener();
        server = new NetManager(listener)
        {
            AutoRecycle = false,
            UnconnectedMessagesEnabled = false,
            NatPunchEnabled = configuration.NatPunchEnabled,
            AllowPeerAddressChange = configuration.AllowPeerAddressChange,
            BroadcastReceiveEnabled = false,
            UseNativeSockets = configuration.UseNativeSockets,
            ChannelsCount = 64,
            EnableStatistics = configuration.EnableStatistics,
            IPv6Enabled = configuration.IPv6Enabled,
            UpdateTime = configuration.UpdateTime,
            PingInterval = configuration.PingInterval,
            DisconnectTimeout = configuration.DisconnectTimeout
        };

        StartListening(configuration);
    }

    private static void StartListening(Configuration configuration)
    {
        if (configuration.OverrideAutoDiscoveryOfIpv)
        {
            BNL.Log("Server Wiring up SetPort " + Configuration.SetPort + "IPv6Address " + Configuration.IPv6Address);
            server.Start(Configuration.IPv4Address, Configuration.IPv6Address, Configuration.SetPort);
        }
        else
        {
            BNL.Log("Server Wiring up SetPort " + Configuration.SetPort);
            server.Start(Configuration.SetPort);
        }
    }
    #endregion

    #region Server Events Setup
    private static void SetupServerEvents(Configuration configuration)
    {
        listener.ConnectionRequestEvent += HandleConnectionRequest;
        listener.PeerDisconnectedEvent += HandlePeerDisconnected;
        listener.NetworkReceiveEvent += HandleNetworkReceiveEvent;
    }

    private static void HandleConnectionRequest(ConnectionRequest request)
    {
        BNL.Log("Processing Connection Request");
        int ServerCount = server.ConnectedPeersCount;

        if (ServerCount >= Configuration.PeerLimit)
        {
            RejectWithReason(request, "Server is full! Rejected.");
            return;
        }

        if (!request.Data.TryGetUShort(out ushort ClientVersion))
        {
            RejectWithReason(request, "Invalid client data.");
            return;
        }

        if (ClientVersion < BasisNetworkVersion.ServerVersion)
        {
            RejectWithReason(request, "Outdated client version.");
            return;
        }

        ProcessConnectionApproval(request, ServerCount);
    }

    private static void ProcessConnectionApproval(ConnectionRequest request, int ServerCount)
    {
        AuthenticationMessage authMessage = new AuthenticationMessage();
        authMessage.Deserialize(request.Data);

        if (!BasisPasswordImplementation.CheckPassword(authMessage, Configuration, out string UsedPassword))
        {
            RejectWithReason(request, "Authentication failed Expected " + Configuration.Password + " but got " + UsedPassword);
            return;
        }

        BNL.Log("Player approved. Current count: " + ServerCount);
        ApproveAndInitializeConnection(request);
    }
    private static void ApproveAndInitializeConnection(ConnectionRequest request)
    {
        NetPeer newPeer = request.Accept();
        if (Peers.TryAdd((ushort)newPeer.Id, newPeer))
        {
            BNL.Log($"Peer connected: {newPeer.Id}");
            ReadyMessage readyMessage = new ReadyMessage();
            readyMessage.Deserialize(request.Data);
            SendRemoteSpawnMessage(newPeer, readyMessage);
        }
        else
        {
            RejectWithReason(request, "Peer already exists.");
        }
    }

    private static void HandlePeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        ushort id = (ushort)peer.Id;
        ClientDisconnect(id,  Peers);

        if (Peers.TryRemove(id, out _))
        {
            BNL.Log($"Peer removed: {id}");
        }
        else
        {
            BNL.LogError($"Failed to remove peer: {id}");
        }
        CleanupPlayerData(id, peer);
    }

    private static void CleanupPlayerData(ushort id, NetPeer peer)
    {
        BasisNetworkOwnership.RemovePlayerOwnership(id);
        BasisSavedState.RemovePlayer(peer);
        BasisServerReductionSystem.RemovePlayer(peer);
    }
    #endregion

    #region Worker Thread

    public static void StopWorker()
    {
        server.Stop();
    }

    #endregion

    #region Network Receive Handlers
    private static void HandleNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        switch (channel)
        {
            case BasisNetworkCommons.FallChannel:
                if (deliveryMethod == DeliveryMethod.Unreliable)
                {
                    if (reader.TryGetByte(out byte Byte))
                    {
                      //  BNL.Log($"Found Channel {Byte} {reader.AvailableBytes}");
                        HandleNetworkReceiveEvent(peer, reader, Byte, deliveryMethod);
                    }
                    else
                    {
                        BNL.LogError($"Unknown channel no data remains: {channel} " + reader.AvailableBytes);
                        reader.Recycle();
                    }
                }
                else
                {
                    BNL.LogError($"Unknown channel: {channel} " + reader.AvailableBytes);
                    reader.Recycle();
                }
                break;
            case BasisNetworkCommons.MovementChannel:
                HandleAvatarMovement(reader, peer);
                reader.Recycle();
                break;
            case BasisNetworkCommons.VoiceChannel:
                HandleVoiceMessage(reader, peer);
                reader.Recycle();
                break;
            case BasisNetworkCommons.AvatarChannel:
                BasisNetworkingGeneric.HandleAvatar(reader, deliveryMethod, peer);
                reader.Recycle();
                break;
            case BasisNetworkCommons.SceneChannel:
                BasisNetworkingGeneric.HandleScene(reader, deliveryMethod, peer, Peers);
                reader.Recycle();
                break;
            case BasisNetworkCommons.AvatarChangeMessage:
                SendAvatarMessageToClients(reader, peer);
                reader.Recycle();
                break;
            case BasisNetworkCommons.OwnershipTransfer:
                BasisNetworkOwnership.OwnershipTransfer(reader, peer, Peers);
                reader.Recycle();
                break;
            case BasisNetworkCommons.AudioRecipients:
                UpdateVoiceReceivers(reader, peer);
                reader.Recycle();
                break;
            default:
                BNL.LogError($"Unknown channel: {channel} " + reader.AvailableBytes);
                reader.Recycle();
                break;
        }
    }

    #endregion

    #region Utility Methods
    private static void RejectWithReason(ConnectionRequest request, string reason)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(reason);
        request.Reject(writer);
        BNL.LogError($"Rejected: {reason}");
    }

    public static void ClientDisconnect(ushort leaving, ConcurrentDictionary<ushort, NetPeer> authenticatedClients)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(leaving);

        foreach (var client in authenticatedClients.Values)
        {
            if (client.Id != leaving)
                client.Send(writer, BasisNetworkCommons.Disconnection, DeliveryMethod.ReliableOrdered);
        }
    }
    #endregion
    private static void SendAvatarMessageToClients(NetPacketReader Reader, NetPeer Peer)
    {
        ClientAvatarChangeMessage ClientAvatarChangeMessage = new ClientAvatarChangeMessage();
        ClientAvatarChangeMessage.Deserialize(Reader);
        ServerAvatarChangeMessage serverAvatarChangeMessage = new ServerAvatarChangeMessage
        {
            clientAvatarChangeMessage = ClientAvatarChangeMessage,
            uShortPlayerId = new PlayerIdMessage
            {
                playerID = (ushort)Peer.Id
            }
        };
        BasisSavedState.AddLastData(Peer, ClientAvatarChangeMessage);
        NetDataWriter Writer = new NetDataWriter();
        serverAvatarChangeMessage.Serialize(Writer);
        BroadcastMessageToClients(Writer, BasisNetworkCommons.AvatarChangeMessage, Peer, Peers);
    }
    private static void UpdateVoiceReceivers(NetPacketReader Reader, NetPeer Peer)
    {
        VoiceReceiversMessage VoiceReceiversMessage = new VoiceReceiversMessage();
        VoiceReceiversMessage.Deserialize(Reader);
        BasisSavedState.AddLastData(Peer, VoiceReceiversMessage);
    }
    private static void HandleVoiceMessage(NetPacketReader Reader, NetPeer peer)
    {
        AudioSegmentDataMessage audioSegment = new AudioSegmentDataMessage();
        audioSegment.Deserialize(Reader);
        ServerAudioSegmentMessage ServerAudio = new ServerAudioSegmentMessage
        {
            audioSegmentData = audioSegment
        };
        SendVoiceMessageToClients(ServerAudio, BasisNetworkCommons.VoiceChannel, peer);
    }
    private static void SendVoiceMessageToClients(ServerAudioSegmentMessage audioSegment, byte channel, NetPeer sender)
    {
        if (BasisSavedState.GetLastData(sender, out StoredData data))
        {
            if (data.voiceReceiversMessage.users == null)
            {
                // BNL.Log("No Users!");
                return;
            }

            int count = data.voiceReceiversMessage.users.Length;
            if (count == 0)
            {
                //  BNL.Log("No Count!");
                return;
            }
            List<NetPeer> endPoints = new List<NetPeer>(count);
            foreach (ushort user in data.voiceReceiversMessage.users)
            {
                if (Peers.TryGetValue(user, out NetPeer client))
                {
                    endPoints.Add(client);
                }
            }

            if (endPoints.Count == 0)
            {
                //  BNL.Log("No Viable");
                return;
            }

            audioSegment.playerIdMessage = new PlayerIdMessage
            {
                playerID = (ushort)sender.Id
            };
            NetDataWriter NetDataWriter = new NetDataWriter();
            audioSegment.Serialize(NetDataWriter);
          //  BNL.Log("Sending Voice Data To Clients");
            BroadcastMessageToClients(NetDataWriter, channel, endPoints, DeliveryMethod.Sequenced);
        }
        else
        {
            BNL.Log("Error unable to find " + sender.Id + " in the data store!");
        }
    }
    public static void BroadcastMessageToClients(NetDataWriter Reader, byte channel, NetPeer sender, ConcurrentDictionary<ushort, NetPeer> authenticatedClients, DeliveryMethod deliveryMethod = DeliveryMethod.Sequenced)
    {
        IEnumerable<KeyValuePair<ushort, NetPeer>> clientsExceptSender = authenticatedClients.Where(client => client.Value.Id != sender.Id);

        foreach (KeyValuePair<ushort, NetPeer> client in clientsExceptSender)
        {
            client.Value.Send(Reader, channel, deliveryMethod);
        }
    }
    public static void BroadcastMessageToClients(NetDataWriter Reader, byte channel, List<NetPeer> authenticatedClients, DeliveryMethod deliveryMethod = DeliveryMethod.Sequenced)
    {
        int count = authenticatedClients.Count;
        for (int index = 0; index < count; index++)
        {
            authenticatedClients[index].Send(Reader, channel, deliveryMethod);
        }
    }
    public static void BroadcastMessageToClients(NetDataWriter Reader, byte channel, ConcurrentDictionary<ushort, NetPeer> authenticatedClients, DeliveryMethod deliveryMethod = DeliveryMethod.Sequenced)
    {
        foreach (KeyValuePair<ushort, NetPeer> client in authenticatedClients)
        {
            client.Value.Send(Reader, channel, deliveryMethod);
        }
    }
    private static void HandleAvatarMovement(NetPacketReader Reader, NetPeer Peer)
    {
        LocalAvatarSyncMessage LocalAvatarSyncMessage = new LocalAvatarSyncMessage();
        LocalAvatarSyncMessage.Deserialize(Reader);
        BasisSavedState.AddLastData(Peer, LocalAvatarSyncMessage);
        foreach (NetPeer client in Peers.Values)
        {
            if (client.Id == Peer.Id)
            {
                continue;
            }
            ServerSideSyncPlayerMessage ssspm = CreateServerSideSyncPlayerMessage(LocalAvatarSyncMessage, (ushort)Peer.Id);
            BasisServerReductionSystem.AddOrUpdatePlayer(client, ssspm, Peer);
        }
    }
    private static ServerSideSyncPlayerMessage CreateServerSideSyncPlayerMessage(LocalAvatarSyncMessage local, ushort clientId)
    {
        return new ServerSideSyncPlayerMessage
        {
            playerIdMessage = new PlayerIdMessage { playerID = clientId },
            avatarSerialization = local
        };
    }
    public static void SendRemoteSpawnMessage(NetPeer authClient, ReadyMessage readyMessage)
    {
        ServerReadyMessage serverReadyMessage = LoadInitialState(authClient, readyMessage);
        NotifyExistingClients(serverReadyMessage, authClient);
        SendClientListToNewClient(authClient);
    }
    public static ServerReadyMessage LoadInitialState(NetPeer authClient, ReadyMessage readyMessage)
    {
        ServerReadyMessage serverReadyMessage = new ServerReadyMessage
        {
            localReadyMessage = readyMessage,
            playerIdMessage = new PlayerIdMessage() { playerID = (ushort)authClient.Id },
        };
        BasisSavedState.AddLastData(authClient, readyMessage);
        return serverReadyMessage;
    }
    private static void NotifyExistingClients(ServerReadyMessage serverSideSyncPlayerMessage, NetPeer authClient)
    {
        NetDataWriter Writer = new NetDataWriter();
        serverSideSyncPlayerMessage.Serialize(Writer);
        IEnumerable<NetPeer> clientsToNotify = Peers.Values.Where(client => client != authClient);

        string ClientIds = string.Empty;
        foreach (NetPeer client in clientsToNotify)
        {
            ClientIds += $" | {client.Id}";
            client.Send(Writer, BasisNetworkCommons.CreateRemotePlayer, DeliveryMethod.ReliableOrdered);
        }
        BNL.Log($"Sent Remote Spawn request to {ClientIds}");
    }
    private static void SendClientListToNewClient(NetPeer authClient)
    {
        if (Peers.Count > ushort.MaxValue)
        {
            BNL.Log($"authenticatedClients count exceeds {ushort.MaxValue}");
            return;
        }

        List<ServerReadyMessage> copied = new List<ServerReadyMessage>();

        IEnumerable<NetPeer> clientsToNotify = Peers.Values.Where(client => client != authClient);
        BNL.Log("Notifing Newly Connected Client about "+ clientsToNotify.Count());
        foreach (NetPeer client in clientsToNotify)
        {
            ServerReadyMessage serverReadyMessage = new ServerReadyMessage();

            if (BasisSavedState.GetLastData(client, out StoredData sspm))
            {
                serverReadyMessage.localReadyMessage = new ReadyMessage
                {
                    localAvatarSyncMessage = sspm.lastAvatarSyncState,
                    clientAvatarChangeMessage = sspm.lastAvatarChangeState,
                    playerMetaDataMessage = sspm.playerMetaDataMessage,
                };
                serverReadyMessage.playerIdMessage = new PlayerIdMessage() { playerID = (ushort)client.Id };
            }
            else
            {
                BNL.Log("Unable to get last Data Creating Fake");
                serverReadyMessage.playerIdMessage = new PlayerIdMessage { playerID = (ushort)client.Id };
                serverReadyMessage.localReadyMessage = new ReadyMessage
                {
                    localAvatarSyncMessage = new LocalAvatarSyncMessage() { array = new byte[386] },
                    clientAvatarChangeMessage = new ClientAvatarChangeMessage() { byteArray = new byte[] { }, },
                    playerMetaDataMessage = new PlayerMetaDataMessage() { playerDisplayName = "Error", playerUUID = string.Empty },
                };
            }

            copied.Add(serverReadyMessage);
        }

        CreateAllRemoteMessage remoteMessages = new CreateAllRemoteMessage
        {
            serverSidePlayer = copied.ToArray(),
        };
        NetDataWriter Writer = new NetDataWriter();
        remoteMessages.Serialize(Writer);
        BNL.Log($"Sending list of clients to {authClient.Id}");
        authClient.Send(Writer, BasisNetworkCommons.CreateRemotePlayers, DeliveryMethod.ReliableOrdered);
    }
}
