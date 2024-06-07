using System.Collections.Generic;
using System;
using UnityEngine;
using UnityOpus;
using DarkRift;
using static SerializableDarkRift;
using DarkRift.Server.Plugins.Commands;
[System.Serializable]
public class BasisAudioTransmission
{
    public event Action<byte[]> OnEncoded;
    public Encoder encoder;
    public BasisNetworkedPlayer NetworkedPlayer;
    public BasisNetworkSendBase Base;
    public BasisOpusSettings settings;
    public byte[] outputBuffer;
    public byte[] encodedData;
    public int encodedLength;
    public void OnEnable(BasisNetworkedPlayer networkedPlayer)
    {
        NetworkedPlayer = networkedPlayer;
        Base = networkedPlayer.NetworkSend;
        settings = BasisDeviceManagement.Instance.BasisOpusSettings;
        encoder = new Encoder(settings.SamplingFrequency, settings.NumChannels, settings.OpusApplication)
        {
            Bitrate = settings.BitrateKPS,
            Complexity = settings.Complexity,
            Signal = settings.OpusSignal
        };
        OnEncoded += SendVoiceOverNetwork;
        BasisLocalPlayer Local = (BasisLocalPlayer)networkedPlayer.Player;
        if (Local.AvatarDriver != null && Local.AvatarDriver.MicrophoneRecorder != null)
        {
            Local.AvatarDriver.MicrophoneRecorder.OnHasAudio += OnAudioReady;
            Local.AvatarDriver.MicrophoneRecorder.OnHasSilence += OnAudioSilence;
        }
    }
    public void OnDisable()
    {
        if (BasisLocalPlayer.Instance.AvatarDriver.MicrophoneRecorder != null)
        {
            GameObject.Destroy(BasisLocalPlayer.Instance.AvatarDriver.MicrophoneRecorder.gameObject);
        }
        BasisLocalPlayer.Instance.AvatarDriver.MicrophoneRecorder.OnHasAudio -= OnAudioReady;
        BasisLocalPlayer.Instance.AvatarDriver.MicrophoneRecorder.OnHasSilence -= OnAudioSilence;
        encoder.Dispose();
        encoder = null;
        OnEncoded -= SendVoiceOverNetwork;
    }
    public void OnAudioSilence()
    {
        SendSilenceOverNetwork();
    }
    public void OnAudioReady(float[] data)
    {
        int PacketSize = data.Length * 4;
        if (outputBuffer == null || PacketSize != outputBuffer.Length)
        {
            outputBuffer = new byte[PacketSize];
        }
        encodedLength = encoder.Encode(data, outputBuffer);

        encodedData = new byte[encodedLength];
        Array.Copy(outputBuffer, 0, encodedData, 0, encodedLength);

        OnEncoded?.Invoke(encodedData);
    }
    private void SendVoiceOverNetwork(byte[] VoiceData)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            AudioSegmentData Audio = new AudioSegmentData
            {
                buffer = VoiceData,
            };
            writer.Write(Audio);
            BasisNetworkProfiler.AudioUpdatePacket.Sample(writer.Length);
            using (Message msg = Message.Create(BasisTags.AudioSegmentTag, writer))
            {
                BasisNetworkConnector.Instance.Client.SendMessage(msg, SendMode.Unreliable);
            }
        }
    }
    private void SendSilenceOverNetwork()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            AudioSilentSegmentData Audio = new AudioSilentSegmentData
            {
            };
            writer.Write(Audio);
            BasisNetworkProfiler.AudioUpdatePacket.Sample(writer.Length);
            using (Message msg = Message.Create(BasisTags.AudioSegmentTag, writer))
            {
                BasisNetworkConnector.Instance.Client.SendMessage(msg, SendMode.Unreliable);
            }
        }
    }
}