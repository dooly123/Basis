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
        public event Action OnEncodedThreaded;
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
            if (!IsInitalized)
            {
                // Assign the networked player and base network send functionality
                NetworkedPlayer = networkedPlayer;
                Base = networkedPlayer.NetworkSend;

                // Retrieve the Opus settings from the singleton instance
                settings = BasisDeviceManagement.Instance.BasisOpusSettings;

                // Initialize the Opus encoder with the retrieved settings
                encoder = new Encoder(settings.SamplingFrequency, settings.NumChannels, settings.OpusApplication)
                {
                    Bitrate = settings.BitrateKPS,
                    Complexity = settings.Complexity,
                    Signal = settings.OpusSignal
                };

                // Cast the networked player to a local player to access the microphone recorder
                Local = (BasisLocalPlayer)networkedPlayer.Player;
                Recorder = Local.MicrophoneRecorder;

                // If there are no events hooked up yet, attach them
                if (!HasEvents)
                {
                    if (Recorder != null)
                    {
                        // Hook up the event handlers
                        MicrophoneRecorder.OnHasAudio += OnAudioReady;
                        MicrophoneRecorder.OnHasSilence += SendSilenceOverNetwork;
                        OnEncodedThreaded += SendVoiceOverNetwork;

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
                OnEncodedThreaded -= SendVoiceOverNetwork;
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
            // Ensure the output buffer is properly initialized and matches the packet size
            if (outputBuffer == null || Recorder.PacketSize != outputBuffer.Length)
            {
                outputBuffer = new byte[Recorder.PacketSize];
            }

            // Locking to ensure thread safety during encoding
            lock (encoder.encoderLock)
            {
                // Encode the audio data from the microphone recorder's buffer
                encodedLength = encoder.Encode(Recorder.processBufferArray, outputBuffer);
            }

            // Invoke the OnEncoded event to handle the encoded data (e.g., sending over the network)
            OnEncodedThreaded?.Invoke();
        }
        private void SendVoiceOverNetwork()
        {
            if (Base.HasReasonToSendAudio)
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
                        BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.VoiceChannel, DeliveryMethod.Sequenced);
                    }
                }
                Local.AudioReceived?.Invoke(true);
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
                    BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.VoiceChannel, DeliveryMethod.Sequenced);
                }
            }
            Local.AudioReceived?.Invoke(false);
        }
    }
}