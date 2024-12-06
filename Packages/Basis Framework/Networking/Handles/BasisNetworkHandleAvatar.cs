using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.Recievers;
using DarkRift;
using DarkRift.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static SerializableDarkRift;
public static class BasisNetworkHandleAvatar
{
    private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1); // Ensures single execution
    private static CancellationTokenSource avatarUpdateCancellationTokenSource = new CancellationTokenSource();
    private const int TimeoutMilliseconds = 100; // 100ms limit per execution
    public static ConcurrentQueue<ServerSideSyncPlayerMessage> Message = new ConcurrentQueue<ServerSideSyncPlayerMessage>();
    public static async Task HandleAvatarUpdate(MessageReceivedEventArgs e)
    {
        // Cancel any ongoing task
        avatarUpdateCancellationTokenSource.Cancel();
        avatarUpdateCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = avatarUpdateCancellationTokenSource.Token;

        try
        {
            await semaphore.WaitAsync(TimeoutMilliseconds);

            try
            {
                using (Message message = e.GetMessage())
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        if (Message.TryDequeue(out ServerSideSyncPlayerMessage SSM) == false)
                        {
                            SSM = new ServerSideSyncPlayerMessage();
                        }

                        SSM.Deserialize(reader.deserializeEventSingleton);
                        if (BasisNetworkManagement.RemotePlayers.TryGetValue(SSM.playerIdMessage.playerID, out BasisNetworkReceiver player))
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                Debug.Log("HandleAvatarUpdate operation canceled.");
                                return; // Exit early if a cancellation is requested
                            }

                            player.ReceiveNetworkAvatarData(SSM);
                        }
                        else
                        {
                            Debug.Log($"Missing Player For Avatar Update {SSM.playerIdMessage.playerID}");
                        }
                        Message.Enqueue(SSM);
                        while (Message.Count > 250)
                        {
                            Message.TryDequeue(out ServerSideSyncPlayerMessage seg);
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Debug.LogError($"Error in HandleAvatarUpdate: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("HandleAvatarUpdate task canceled.");
        }
    }
    public static void HandleAvatarChangeMessage(DarkRiftReader reader)
    {
        reader.Read(out ServerAvatarChangeMessage ServerAvatarChangeMessage);
        ushort PlayerID = ServerAvatarChangeMessage.uShortPlayerId.playerID;
        if (BasisNetworkManagement.Players.TryGetValue(PlayerID, out BasisNetworkedPlayer Player))
        {
            BasisNetworkReceiver networkReceiver = (BasisNetworkReceiver)Player.NetworkSend;
            networkReceiver.ReceiveAvatarChangeRequest(ServerAvatarChangeMessage);
        }
        else
        {
            Debug.Log("Missing Player For Message " + ServerAvatarChangeMessage.uShortPlayerId.playerID);
        }
    }
}