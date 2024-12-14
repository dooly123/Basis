using Basis.Network.Core;
using Basis.Network.Server.Generic;
using Basis.Network.Server.Ownership;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Basis.Network.Server.Generic.BasisSavedState;
using static SerializableBasis;
public static class BasisNetworkServer
{
    public static int PeerLimit = 1024;
    public static ushort SetPort = 4296;
    public static Thread serverIncomeThread;
    public static EventBasedNetListener listener;
    public static NetManager server;
    public static bool UseNativeSockets = false;
    private static CancellationTokenSource cancellationTokenSource;
    public static ConcurrentDictionary<ushort, NetPeer> Peers = new ConcurrentDictionary<ushort, NetPeer>();
    public static void StartServer()
    {
        listener = new EventBasedNetListener();
        server = new NetManager(listener)
        {
            AutoRecycle = false,//we do this ourselves
            UnconnectedMessagesEnabled = true,
            NatPunchEnabled = true,
            AllowPeerAddressChange = true,
            BroadcastReceiveEnabled = true,
            UseNativeSockets = UseNativeSockets,
            ChannelsCount = 7,
        };
        server.Start(SetPort);
        BNL.Log("Server Wiring up " + SetPort);
 
        listener.ConnectionRequestEvent += request =>
        {
            BNL.Log("Processing Connection Request");
            int ServerCount = server.ConnectedPeersCount;
            if (ServerCount < PeerLimit)
            {
                if (request.Data.TryGetUShort(out ushort ClientVersion))
                {
                    if (ClientVersion >= BasisNetworkVersion.ServerVersion)
                    {
                        BNL.Log("Player Is Approved Total Count is: " + ServerCount);
                        NetPeer ReadyToRoll = request.Accept();
                        if (Peers.TryAdd((ushort)ReadyToRoll.Id, ReadyToRoll))
                        {
                            BNL.Log("Length is " + request.Data.AvailableBytes);
                            ReadyMessage ReadyMessage = new ReadyMessage();
                            ReadyMessage.Deserialize(request.Data);
                            BNL.Log($"Peer added and connected: {ReadyToRoll.Id}");
                            SendRemoteSpawnMessage(ReadyToRoll, ReadyMessage, BasisNetworkCommons.EventsChannel);
                        }
                        else
                        {
                            RejectWithReason(request, $"Peer was unable to be added already exists! {ReadyToRoll.Id}");
                        }
                    }
                    else
                    {
                        RejectWithReason(request, "Client is trying to connect to a newer server then itself");
                    }
                }
                else
                {
                    RejectWithReason(request, "Rejecting player submitted no byte array");
                }
            }
            else
            {
                RejectWithReason(request, "Server has " + ServerCount + " no room for more! Rejected");
            }
        };

        listener.PeerDisconnectedEvent += (peer, info) =>
        {
            ushort Id = (ushort)peer.Id;
            if (Peers.TryRemove(Id, out peer))
            {
                BNL.Log($"Peer removed and Disconnected: {peer.Id}");
            }
            else
            {
                BNL.LogError($"Peer was unable to be removed! {peer.Id}");
            }
            BasisNetworkOwnership.RemovePlayerOwnership(Id);
            BasisSavedState.RemovePlayer(peer);
            BasisServerReductionSystem.RemovePlayer(peer);
            ClientDisconnect(Id, BasisNetworkCommons.EventsChannel, Peers);
        };
        listener.NetworkReceiveEvent += NetworkReceiveEvent;
        BNL.Log("Server Worker Threads booting");
        StartWorker();
        BNL.Log("Server Worker Threads Booted");
    }
    public static void ClientDisconnect(ushort Leaving, byte channel, ConcurrentDictionary<ushort, NetPeer> authenticatedClients)
    {
        NetDataWriter Writer = new NetDataWriter();
        Writer.Put(BasisNetworkTag.Disconnection);
        Writer.Put(Leaving);
        foreach (NetPeer client in authenticatedClients.Values)
        {
            client.Send(Writer, channel, DeliveryMethod.ReliableOrdered);
        }
    }
    public static void RejectWithReason(ConnectionRequest Request, string Reason)
    {
        NetDataWriter Writer = new NetDataWriter();
        Writer.Put(Reason);
        Request.Reject(Writer);
        BNL.LogError(Request.RemoteEndPoint.ToString() + " " + Reason);
    }
    public static void StartWorker()
    {
        cancellationTokenSource = new CancellationTokenSource();
        serverIncomeThread = new Thread(() => WorkerThread(cancellationTokenSource.Token))
        {
            IsBackground = true // Ensure the thread doesn't block application exit
        };
        serverIncomeThread.Start();
    }
    public static void WorkerThread(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Main worker logic
                    while (!token.IsCancellationRequested)
                    {
                        server?.PollEvents();
                        Task.Delay(BasisNetworkCommons.NetworkIntervalPoll, token).Wait(token); // Waits but respects cancellation
                    }
                }
                catch (OperationCanceledException)
                {
                    // This is expected when the token is canceled; simply exit gracefully
                    BNL.Log("Worker thread cancellation requested.");
                    break;
                }
                catch (Exception ex)
                {
                    // Log the exception and continue looping
                    BNL.LogError($"Worker thread encountered an exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    // Optional: add a delay to avoid rapid re-execution in case of persistent failures
                    Task.Delay(BasisNetworkCommons.NetworkIntervalPoll).Wait();
                }
            }
        }
        finally
        {
            BNL.Log("Worker thread is exiting.");
        }
    }
    public static void StopWorker()
    {
        // Signal cancellation
        cancellationTokenSource.Cancel();

        // Optionally wait for the thread to finish
        if (serverIncomeThread.IsAlive)
        {
            serverIncomeThread.Join();
        }
        server?.Stop();
    }
    private static void NetworkReceiveEventTag(NetPeer peer, NetPacketReader Reader, BasisMessageReceivedEventArgs e)
    {
        switch (e.Tag)
        {
            case BasisNetworkTag.AvatarChangeMessage:
                SendAvatarMessageToClients(Reader, peer, e);
                break;
            case BasisNetworkTag.SceneGenericMessage_Recipients_NoPayload:
                BasisNetworkingGeneric.HandleSceneDataMessage_Recipients_NoPayload(Reader, e, peer, Peers);
                break;
            case BasisNetworkTag.AvatarGenericMessage_Recipients_NoPayload:
            case BasisNetworkTag.AvatarGenericMessage:
                BasisNetworkingGeneric.HandleAvatarDataMessage_Recipients_Payload(Reader, e, peer, Peers);
                break;
            case BasisNetworkTag.AvatarGenericMessage_NoRecipients:
                BasisNetworkingGeneric.HandleAvatarDataMessage_NoRecipients(Reader, e, peer, Peers);
                break;
            case BasisNetworkTag.AvatarGenericMessage_NoRecipients_NoPayload:
                BasisNetworkingGeneric.HandleAvatarDataMessage_NoRecipients_NoPayload(Reader, e, peer, Peers);
                break;
            case BasisNetworkTag.SceneGenericMessage:
            case BasisNetworkTag.SceneGenericMessage_NoRecipients:
                BasisNetworkingGeneric.HandleSceneDataMessage_NoRecipients(Reader, e, peer, Peers);
                break;
            case BasisNetworkTag.SceneGenericMessage_NoRecipients_NoPayload:
                BasisNetworkingGeneric.HandleSceneDataMessage_NoRecipients_NoPayload(Reader, e, peer, Peers);
                break;
            case BasisNetworkTag.AudioRecipients:
                UpdateVoiceReceivers(Reader, peer);
                break;
            case BasisNetworkTag.OwnershipResponse:
                BNL.Log("OwnershipResponse");
                BasisNetworkOwnership.OwnershipResponse(Reader, peer, e, Peers);
                break;
            case BasisNetworkTag.OwnershipTransfer:
                BNL.Log("OwnershipTransfer");
                BasisNetworkOwnership.OwnershipTransfer(Reader, peer, e, Peers);
                break;
            default:
                BNL.LogError("Message Index " + e.Tag + " does not exist on the server!");
                break;
        }
    }
    private static void NetworkReceiveEvent(NetPeer peer, NetPacketReader Reader, byte channel, LiteNetLib.DeliveryMethod deliveryMethod)
    {
        BasisMessageReceivedEventArgs e = new BasisMessageReceivedEventArgs
        {
            Tag = Reader.GetByte(),
            SendMode = deliveryMethod,
            ClientId = (ushort)peer.Id
        };
        switch (channel)
        {
            case BasisNetworkCommons.EventsChannel:
                NetworkReceiveEventTag(peer, Reader, e);
                break;
            case BasisNetworkCommons.VoiceChannel:
                HandleVoiceMessage(Reader, peer, e);
                break;
            case BasisNetworkCommons.MovementChannel:
                HandleAvatarMovement(Reader, peer, e);
                break;
            case BasisNetworkCommons.SceneChannel:
                NetworkReceiveEventTag(peer, Reader, e);
                break;
            case BasisNetworkCommons.AvatarChannel:
                NetworkReceiveEventTag(peer, Reader, e);
                break;
            default:
                BNL.LogError($"this Channel was not been implemented {channel}");
                break;
        }
        Reader.Recycle();
    }
    private static void SendAvatarMessageToClients(NetPacketReader Reader, NetPeer Peer, BasisMessageReceivedEventArgs e)
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
        BroadcastMessageToClients(Writer, BasisNetworkCommons.EventsChannel, Peer, Peers);
    }
    private static void UpdateVoiceReceivers(NetPacketReader Reader, NetPeer Peer)
    {
        VoiceReceiversMessage VoiceReceiversMessage = new VoiceReceiversMessage();
        VoiceReceiversMessage.Deserialize(Reader);
        BasisSavedState.AddLastData(Peer, VoiceReceiversMessage);
    }
    private static void HandleVoiceMessage(NetPacketReader Reader, NetPeer peer, BasisMessageReceivedEventArgs e)
    {
        AudioSegmentMessage audioSegment = new AudioSegmentMessage();
        if (Reader.EndOfData)
        {
            audioSegment.wasSilentData = true;
            AudioSilentSegmentDataMessage AudioSilentSegmentDataMessage = new AudioSilentSegmentDataMessage();
            AudioSilentSegmentDataMessage.Deserialize(Reader);
            audioSegment.silentData = AudioSilentSegmentDataMessage;
        }
        else
        {
            audioSegment.wasSilentData = false;
            audioSegment.Deserialize(Reader);
        }
        SendVoiceMessageToClients(audioSegment, BasisNetworkCommons.VoiceChannel, peer);
    }
    private static void SendVoiceMessageToClients(AudioSegmentMessage audioSegment, byte channel, NetPeer sender)
    {
        if (BasisSavedState.GetLastData(sender, out StoredData data))
        {
            if (data.voiceReceiversMessage.users == null || data.voiceReceiversMessage.users.Length == 0)
            {
                return;
            }
            int count = data.voiceReceiversMessage.users.Length;
            List<NetPeer> endPoints = new List<NetPeer>(count);
            foreach (ushort user in data.voiceReceiversMessage.users)
            {
                if (Peers.TryGetValue(user, out NetPeer client))
                {
                    endPoints.Add(client);
                }
            }

            if (endPoints.Count == 0) return;

            audioSegment.playerIdMessage.playerID = (ushort)sender.Id;
            NetDataWriter NetDataWriter = new NetDataWriter();
            audioSegment.Serialize(NetDataWriter);
            BroadcastMessageToClients(NetDataWriter, channel, endPoints, DeliveryMethod.Sequenced);
        }
        else
        {
            //   Console.WriteLine("Error unable to find " + sender.ID + " in the data store!");
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
    private static void HandleAvatarMovement(NetPacketReader Reader, NetPeer Peer, BasisMessageReceivedEventArgs e)
    {
        LocalAvatarSyncMessage LocalAvatarSyncMessage = new LocalAvatarSyncMessage();
        LocalAvatarSyncMessage.Deserialize(Reader);
        BasisSavedState.AddLastData(Peer, LocalAvatarSyncMessage);
        foreach (NetPeer client in Peers.Values)
        {
            if (client.Id == e.ClientId)
            {
                continue;
            }
            ServerSideSyncPlayerMessage ssspm = CreateServerSideSyncPlayerMessage(LocalAvatarSyncMessage, e.ClientId);
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
        SendClientListToNewClient(authClient, BasisNetworkCommons.EventsChannel);
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

        foreach (NetPeer client in clientsToNotify)
        {
            BNL.Log($"Sent Remote Spawn request to {client.Id}");
            client.Send(Writer, channel, DeliveryMethod.ReliableOrdered);
        }
    }
    private static void SendClientListToNewClient(NetPeer authClient, byte channel)
    {
        if (Peers.Count > ushort.MaxValue)
        {
            BNL.Log($"authenticatedClients count exceeds {ushort.MaxValue}");
            return;
        }

        List<ServerReadyMessage> copied = new List<ServerReadyMessage>();

        var clientsToNotify = Peers.Values.Where(client => client != authClient);

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
                    localAvatarSyncMessage = new LocalAvatarSyncMessage() { array = new byte[390] },
                    clientAvatarChangeMessage = new ClientAvatarChangeMessage() { byteArray = new byte[] { }, byteLength = 0 },
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