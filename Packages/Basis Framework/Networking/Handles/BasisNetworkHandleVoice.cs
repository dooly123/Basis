using Basis.Scripts.Networking;
using Basis.Scripts.Networking.Recievers;
using DarkRift;
using UnityEngine;
using static SerializableDarkRift;

public static class BasisNetworkHandleVoice
{
    public static void HandleAudioUpdate(DarkRiftReader reader)
    {
        reader.Read(out AudioSegmentMessage AudioUpdate);
        BasisNetworkManagement Instance = BasisNetworkManagement.Instance;
        if (Instance.Players.TryGetValue(AudioUpdate.playerIdMessage.playerID, out var player))
        {
            BasisNetworkReceiver networkReceiver = (BasisNetworkReceiver)player.NetworkSend;
            if (AudioUpdate.wasSilentData)
            {
                networkReceiver.ReceiveSilentNetworkAudio(AudioUpdate.silentData);
            }
            else
            {
                networkReceiver.ReceiveNetworkAudio(AudioUpdate);
            }
        }
        else
        {
            Debug.Log("Missing Player For Message " + AudioUpdate.playerIdMessage.playerID);
        }
    }
}
