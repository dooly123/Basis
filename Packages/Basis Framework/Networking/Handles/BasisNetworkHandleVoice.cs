using Basis.Scripts.Networking;
using Basis.Scripts.Networking.Recievers;
using DarkRift;
using DarkRift.Client;
using System.Threading;
using System;
using System.Threading.Tasks;
using UnityEngine;
using static SerializableDarkRift;

public static class BasisNetworkHandleVoice
{
    private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1); // Ensures single execution
    private const int TimeoutMilliseconds = 100; // 100ms limit per execution

    public static async Task HandleAudioUpdate(MessageReceivedEventArgs e)
    {
        if (!await semaphore.WaitAsync(TimeoutMilliseconds))
        {
            Debug.LogWarning("Skipped HandleAudioUpdate due to execution overlap.");
            return; // Skip this call if the previous one isn't done
        }

        try
        {
            using (Message message = e.GetMessage())
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    reader.Read(out AudioSegmentMessage audioUpdate);

                    if (BasisNetworkManagement.RemotePlayers.TryGetValue(audioUpdate.playerIdMessage.playerID, out BasisNetworkReceiver player))
                    {
                        if (audioUpdate.wasSilentData)
                        {
                            player.ReceiveSilentNetworkAudio(audioUpdate.silentData);
                        }
                        else
                        {
                            player.ReceiveNetworkAudio(audioUpdate);
                        }
                    }
                    else
                    {
                        Debug.Log($"Missing Player For Message {audioUpdate.playerIdMessage.playerID}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in HandleAudioUpdate: {ex.Message}");
        }
        finally
        {
            semaphore.Release();
        }
    }
}