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
    public event Action OnEncodedAndNetworkSent;
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
            Recorder = Local.AvatarDriver.MicrophoneRecorder;
            if (HasEvents == false)
            {
                if (Recorder != null)
                {
                    Recorder.OnHasAudio += EncodeAndSend;
                    Recorder.OnHasSilence += SendSilenceOverNetwork;
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
            Recorder.OnHasAudio -= EncodeAndSend;
            Recorder.OnHasSilence -= SendSilenceOverNetwork;
            HasEvents = false;
        }
        if (Recorder != null)
        {
            GameObject.Destroy(Recorder.gameObject);
        }
        encoder.Dispose();
        encoder = null;
    }
    public void EncodeAndSend()
    {
        if (outputBuffer == null || Recorder.PacketSize != outputBuffer.Length)
        {
            outputBuffer = new byte[Recorder.PacketSize];
        }

        encodedLength = encoder.Encode(Recorder.processBufferArray, outputBuffer);

        if (AudioSegmentData.buffer == null || AudioSegmentData.buffer.Length != encodedLength)
        {
            AudioSegmentData.buffer = new byte[encodedLength];
        }
        Buffer.BlockCopy(outputBuffer, 0, AudioSegmentData.buffer, 0, encodedLength);
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(AudioSegmentData);
            BasisNetworkProfiler.AudioUpdatePacket.Sample(writer.Length);

            using (Message msg = Message.Create(BasisTags.AudioSegmentTag, writer))
            {
                BasisNetworkConnector.Instance.Client.SendMessage(msg, BasisNetworking.VoiceChannel, DeliveryMethod.Sequenced);
            }
        }

        OnEncodedAndNetworkSent?.Invoke();
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