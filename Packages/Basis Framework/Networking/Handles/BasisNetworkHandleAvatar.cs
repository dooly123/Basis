using Basis.Scripts.Networking;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.Recievers;
using DarkRift;
using DarkRift.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static SerializableDarkRift;
public static class BasisNetworkHandleAvatar
{
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
    private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1); // Ensures single execution
    private const int TimeoutMilliseconds = 100; // 100ms limit per execution

    public static async Task HandleAvatarUpdate(MessageReceivedEventArgs e)
    {
        if (!await semaphore.WaitAsync(TimeoutMilliseconds))
        {
            Debug.LogWarning("Skipped HandleAvatarUpdate due to execution overlap.");
            return; // Skip this call if the previous one isn't done
        }

        try
        {
            using (Message message = e.GetMessage())
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    reader.Read(out ServerSideSyncPlayerMessage serverSideSyncPlayerMessage);

                    // Perform thread-safe lookups
                    if (BasisNetworkManagement.RemotePlayers.TryGetValue(serverSideSyncPlayerMessage.playerIdMessage.playerID, out BasisNetworkReceiver player))
                    {
                        player.ReceiveNetworkAvatarData(serverSideSyncPlayerMessage);
                    }
                    else
                    {
                        Debug.Log($"Missing Player For Avatar Update {serverSideSyncPlayerMessage.playerIdMessage.playerID}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in HandleAvatarUpdate: {ex.Message}");
        }
        finally
        {
            semaphore.Release();
        }
    }
}