using System;
using UnityEngine;
using UnityOpus;
using DarkRift;
using static SerializableDarkRift;
using DarkRift.Server.Plugins.Commands;
using Basis.Scripts.Networking.NetworkedPlayer;
using Basis.Scripts.Networking.NetworkedAvatar;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management;
using Basis.Scripts.Profiler;

namespace Basis.Scripts.Networking.Transmitters
{
[System.Serializable]
public class BasisAudioTransmission
{
    public event Action OnEncoded;
    public Encoder encoder;
    public BasisNetworkedPlayer NetworkedPlayer;
    public BasisNetworkSendBase Base;
    public BasisOpusSettings settings;
    public byte[] outputBuffer;
    public int encodedLength;
    public BasisLocalPlayer Local;
    public MicrophoneRecorder Recorder;
    public bool IsInitalized = false;
    public AudioSegmentDataMessage AudioSegmentData = new AudioSegmentDataMessage();
    public AudioSilentSegmentDataMessage audioSilentSegmentData = new AudioSilentSegmentDataMessage();
    public bool HasEvents = false;
    public void OnEnable(BasisNetworkedPlayer networkedPlayer)
    {
        if (IsInitalized == false)
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
            Local = (BasisLocalPlayer)networkedPlayer.Player;
            Recorder = Local.MicrophoneRecorder;
            if (HasEvents == false)
            {
                if (Recorder != null)
                {
                    MicrophoneRecorder.OnHasAudio += OnAudioReady;
                    MicrophoneRecorder.OnHasSilence += SendSilenceOverNetwork;
                    OnEncoded += SendVoiceOverNetwork;
                    HasEvents = true;
                }
            }
            IsInitalized = true;
        }
    }
    public void OnDisable()
    {
        if (HasEvents)
        {
            MicrophoneRecorder.OnHasAudio -= OnAudioReady;
            MicrophoneRecorder.OnHasSilence -= SendSilenceOverNetwork;
            OnEncoded -= SendVoiceOverNetwork;
            HasEvents = false;
        }
        if (Recorder != null)
        {
            GameObject.Destroy(Recorder.gameObject);
        }
        encoder.Dispose();
        encoder = null;
    }
    public void OnAudioReady()
    {
        if (outputBuffer == null || Recorder.PacketSize != outputBuffer.Length)
        {
            outputBuffer = new byte[Recorder.PacketSize];
        }
        encodedLength = encoder.Encode(Recorder.processBufferArray, outputBuffer);
        OnEncoded?.Invoke();
    }
    private void SendVoiceOverNetwork()
    {
        if (AudioSegmentData.buffer == null || AudioSegmentData.buffer.Length != encodedLength)
        {
            AudioSegmentData.buffer = new byte[encodedLength];
        }
        Buffer.BlockCopy(outputBuffer, 0, AudioSegmentData.buffer, 0, encodedLength);

        using (DarkRiftWriter writer = DarkRiftWriter.Create(encodedLength))
        {
            writer.Write(AudioSegmentData);
            BasisNetworkProfiler.AudioUpdatePacket.Sample(encodedLength);
            using (Message msg = Message.Create(BasisTags.AudioSegmentTag, writer))
            {
                BasisNetworkConnector.Instance.Client.SendMessage(msg, BasisNetworking.VoiceChannel, DeliveryMethod.Sequenced);
            }
        }
    }
    private void SendSilenceOverNetwork()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(audioSilentSegmentData);
            BasisNetworkProfiler.AudioUpdatePacket.Sample(writer.Length);
            using (Message msg = Message.Create(BasisTags.AudioSegmentTag, writer))
            {
                BasisNetworkConnector.Instance.Client.SendMessage(msg, BasisNetworking.VoiceChannel, DeliveryMethod.Sequenced);
            }
        }
    }
}
}