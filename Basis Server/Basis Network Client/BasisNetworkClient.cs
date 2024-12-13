using Basis.Network.Core;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SerializableBasis;

public static class BasisNetworkClient
{
    public static NetManager client;
    public static EventBasedNetListener listener;
    public static Thread ClientIncomeThread;
    private static CancellationTokenSource cancellationTokenSource;
    private static NetPeer peer;
    private static bool IsInUse;
    /// <summary>
    /// inital data is typically the 
    /// </summary>
    /// <param name="IP"></param>
    /// <param name="port"></param>
    /// <param name="ReadyMessage"></param>
    public static NetPeer StartClient(string IP, int port, ReadyMessage ReadyMessage)
    {
        if (IsInUse == false)
        {
            listener = new EventBasedNetListener();
            client = new NetManager(listener)
            {
                AutoRecycle = true,
                UnconnectedMessagesEnabled = true,
                NatPunchEnabled = true,
                AllowPeerAddressChange = true,
                BroadcastReceiveEnabled = true,
                UseNativeSockets = false,//unity does not work with this
                ChannelsCount = 7,

            };
            client.Start();
            NetDataWriter Writer = new NetDataWriter();
            //this is the only time we dont put key!
            Writer.Put(BasisNetworkVersion.ServerVersion);
            ReadyMessage.Serialize(Writer);
            peer = client.Connect(IP, port, Writer);
            listener.PeerConnectedEvent += (peer) =>
            {
                BNL.Log("Client connected to server - ID: " + peer.Id);
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                if (disconnectInfo.Reason == DisconnectReason.RemoteConnectionClose)
                {
                    if (disconnectInfo.AdditionalData.TryGetString(out string Reason))
                    {
                        BNL.LogError(Reason);
                    }
                }
                BNL.Log($"Client disconnected from server [{peer.Id}] [{disconnectInfo.Reason}]");
            };
            StartWorker();
            return peer;
        }
        else
        {
            BNL.LogError("Call Shutdown First!");
            return null;
        }
    }
    public static void Disconnect()
    {
        IsInUse = false;
        BNL.Log("Client Called Disconnect from server");
        peer?.Disconnect();
        client?.Stop();
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        if (ClientIncomeThread != null && ClientIncomeThread.IsAlive)
        {
            ClientIncomeThread.Join();
        }

        ClientIncomeThread = null;
        cancellationTokenSource = null;

        BNL.Log("Worker thread stopped.");
    }
    public static void StartWorker()
    {
        cancellationTokenSource = new CancellationTokenSource();
        ClientIncomeThread = new Thread(() => WorkerThread(cancellationTokenSource.Token))
        {
            IsBackground = true // Ensure the thread doesn't block application exit
        };
        ClientIncomeThread.Start();
    }
    public static void WorkerThread(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                //works BNL.Log("Running");
                client?.PollEvents();
                Task.Delay(BasisNetworkCommons.NetworkIntervalPoll, token).Wait(token); // Waits but respects cancellation
            }
        }
        catch (Exception ex)
        {
            BNL.LogError($"Worker thread encountered an exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
        }
        finally
        {
            BNL.Log("Worker thread is exiting.");
        }
    }
}