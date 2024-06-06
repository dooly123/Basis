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
    public MicrophoneRecorder MicrophoneRecorder;
    public event Action<byte[], int> OnEncoded;
    public Encoder encoder;
    public AudioSource SelfOutput;
    public BasisVisemeDriver VisemeDriver;
    public BasisNetworkedPlayer NetworkedPlayer;
    public BasisNetworkSendBase Base;
    public BasisOpusSettings settings;
    public byte[] outputBuffer;
    public void OnEnable(BasisNetworkedPlayer networkedPlayer, GameObject MicrophoneGameobject)
    {
        NetworkedPlayer = networkedPlayer;
        Base = networkedPlayer.NetworkSend;
        settings = BasisNetworkConnector.Instance.BasisOpusSettings;
        if (MicrophoneRecorder == null)
        {
            MicrophoneRecorder = BasisHelpers.GetOrAddComponent<MicrophoneRecorder>(MicrophoneGameobject);
        }
        else
        {
            MicrophoneRecorder.DeInitialize();
        }
        MicrophoneRecorder.Initialize();
        OnCalibration(NetworkedPlayer, MicrophoneGameobject);
        encoder = new Encoder(settings.SamplingFrequency, settings.NumChannels, settings.OpusApplication)
        {
            Bitrate = settings.BitrateKPS,
            Complexity = settings.Complexity,
            Signal = settings.OpusSignal
        };
        OnEncoded += SendVoiceOverNetwork;
        MicrophoneRecorder.OnHasAudio += OnAudioReady;
        MicrophoneRecorder.OnHasSilence += OnAudioSilence;
    }
    public void OnCalibration(BasisNetworkedPlayer NetworkedPlayer, GameObject MicrophoneGameobject)
    {

        if (SelfOutput == null)
        {
            SelfOutput = BasisHelpers.GetOrAddComponent<AudioSource>(MicrophoneGameobject);
        }
        if (VisemeDriver == null)
        {
            VisemeDriver = BasisHelpers.GetOrAddComponent<BasisVisemeDriver>(MicrophoneGameobject);
        }
        SelfOutput.loop = true;     // Set the AudioClip to loop
        SelfOutput.mute = false;
        SelfOutput.clip = MicrophoneRecorder.clip;
        SelfOutput.Play();
        VisemeDriver.audioSource = SelfOutput;
        VisemeDriver.Initialize(NetworkedPlayer.Player.Avatar);
    }
    public void OnDisable()
    {
        if (MicrophoneRecorder != null)
        {
            GameObject.Destroy(MicrophoneRecorder.gameObject);
        }
        if (SelfOutput != null)
        {
            GameObject.Destroy(SelfOutput);
        }
        if (VisemeDriver != null)
        {
            GameObject.Destroy(VisemeDriver);
        }
        MicrophoneRecorder.OnHasAudio -= OnAudioReady;
        MicrophoneRecorder.OnHasSilence -= OnAudioSilence;
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
        var encodedLength = encoder.Encode(data, outputBuffer);
        OnEncoded?.Invoke(outputBuffer, encodedLength);
    }
    private void SendVoiceOverNetwork(byte[] VoiceData, int Length)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            if (Length > ushort.MaxValue)
            {
                Debug.LogError("Length was " + Length + " and is larger then " + ushort.MaxValue);
                return;
            }
            AudioSegmentData Audio = new AudioSegmentData
            {
                encodedLength = (ushort)Length,
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