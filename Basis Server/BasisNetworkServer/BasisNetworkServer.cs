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
    public static Thread serverIncomeThread;
    public static EventBasedNetListener listener;
    public static NetManager server;
    private static CancellationTokenSource cancellationTokenSource;
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

        StartWorkerThread();
        BNL.Log("Server Worker Threads Booted");
    }

    #region Server Setup
    private static void SetupServer(Configuration configuration)
    {
        listener = new EventBasedNetListener();
        server = new NetManager(listener)
        {
            AutoRecycle = false,
            UnconnectedMessagesEnabled = configuration.UnconnectedMessagesEnabled,
            NatPunchEnabled = configuration.NatPunchEnabled,
            AllowPeerAddressChange = configuration.AllowPeerAddressChange,
            BroadcastReceiveEnabled = configuration.BroadcastReceiveEnabled,
            UseNativeSockets = configuration.UseNativeSockets,
            ChannelsCount = 7,
            EnableStatistics = configuration.EnableStatistics,
            IPv6Enabled = configuration.IPv6Enabled,
            UpdateTime = configuration.QueueEvents,
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
            SendRemoteSpawnMessage(newPeer, readyMessage, BasisNetworkCommons.BasisChannel);
        }
        else
        {
            RejectWithReason(request, "Peer already exists.");
        }
    }

    private static void HandlePeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        ushort id = (ushort)peer.Id;
        ClientDisconnect(id, BasisNetworkCommons.BasisChannel, Peers);

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
    private static void StartWorkerThread()
    {
        cancellationTokenSource = new CancellationTokenSource();
        serverIncomeThread = new Thread(() => WorkerThread(cancellationTokenSource.Token))
        {
            IsBackground = true
        };
        serverIncomeThread.Start();
    }

    public static void StopWorker()
    {
        cancellationTokenSource.Cancel();
        serverIncomeThread.Join();
        server.Stop();
    }

    private static void WorkerThread(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                server.PollEvents();
                Task.Delay(BasisNetworkCommons.NetworkIntervalPoll, token).Wait(token);
            }
            catch (OperationCanceledException)
            {
                BNL.Log("Worker thread cancelled.");
                break;
            }
            catch (Exception ex)
            {
                BNL.LogError($"Worker exception: {ex.Message}");
            }
        }
    }
    #endregion

    #region Network Receive Handlers
    private static void HandleNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        switch (channel)
        {
            case BasisNetworkCommons.DefaultChannel:
                BNL.Log("Rec unknown data " + reader.AvailableBytes);
                break;
            case BasisNetworkCommons.BasisChannel:
                HandleEventMessage(peer, reader, deliveryMethod);
                break;
            case BasisNetworkCommons.MovementChannel:
                HandleAvatarMovement(reader, peer);
                break;
            case BasisNetworkCommons.VoiceChannel:
                HandleVoiceMessage(reader, peer);
                break;
            case BasisNetworkCommons.AvatarChannel:
                BasisNetworkingGeneric.HandleAvatar(reader, deliveryMethod, peer, Peers);
                break;
            case BasisNetworkCommons.SceneChannel:
                BasisNetworkingGeneric.HandleScene(reader, deliveryMethod, peer, Peers);
                break;
            default:
                BNL.LogError($"Unknown channel: {channel}");
                break;
        }
        reader.Recycle();
    }

    private static void HandleEventMessage(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
      byte Tag = reader.GetByte();
        switch (Tag)
        {
            case BasisNetworkTag.AvatarChangeMessage:

                SendAvatarMessageToClients(reader, peer);
                break;
            case BasisNetworkTag.OwnershipTransfer:
                BasisNetworkOwnership.OwnershipTransfer(reader, peer, Peers);
                break;
            case BasisNetworkTag.AudioRecipients:
                UpdateVoiceReceivers(reader, peer);
                break;
            default:
                BNL.LogError($"Unhandled tag: {Tag}");
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

    public static void ClientDisconnect(ushort leaving, byte channel, ConcurrentDictionary<ushort, NetPeer> authenticatedClients)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(BasisNetworkTag.Disconnection);
        writer.Put(leaving);

        foreach (var client in authenticatedClients.Values)
        {
            if (client.Id != leaving)
                client.Send(writer, channel, DeliveryMethod.ReliableOrdered);
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
        Writer.Put(BasisNetworkTag.AvatarChangeMessage);
        serverAvatarChangeMessage.Serialize(Writer);
        BroadcastMessageToClients(Writer, BasisNetworkCommons.BasisChannel, Peer, Peers);
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
        ServerAudioSegmentMessage ServerAudio = new ServerAudioSegmentMessage();
        ServerAudio.audioSegmentData = audioSegment;
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
    public static void SendRemoteSpawnMessage(NetPeer authClient, ReadyMessage readyMessage, byte channel)
    {
        ServerReadyMessage serverReadyMessage = LoadInitialState(authClient, readyMessage);
        NotifyExistingClients(serverReadyMessage, channel, authClient);
        SendClientListToNewClient(authClient, BasisNetworkCommons.BasisChannel);
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
    private static void NotifyExistingClients(ServerReadyMessage serverSideSyncPlayerMessage, byte channel, NetPeer authClient)
    {
        NetDataWriter Writer = new NetDataWriter();
        Writer.Put(BasisNetworkTag.CreateRemotePlayer);
        serverSideSyncPlayerMessage.Serialize(Writer);
        IEnumerable<NetPeer> clientsToNotify = Peers.Values.Where(client => client != authClient);

        string ClientIds = string.Empty;
        foreach (NetPeer client in clientsToNotify)
        {
            ClientIds += $" | {client.Id}";
            client.Send(Writer, channel, DeliveryMethod.ReliableOrdered);
        }
        BNL.Log($"Sent Remote Spawn request to {ClientIds}");
    }
    private static void SendClientListToNewClient(NetPeer authClient, byte channel)
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
        Writer.Put(BasisNetworkTag.CreateRemotePlayers);
        remoteMessages.Serialize(Writer);
        BNL.Log($"Sending list of clients to {authClient.Id}");
        authClient.Send(Writer, channel, DeliveryMethod.ReliableOrdered);
    }
}