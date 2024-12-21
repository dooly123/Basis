using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management;
using Basis.Scripts.Drivers;
using Basis.Scripts.Networking.NetworkedPlayer;
using System;
using UnityEngine;

namespace Basis.Scripts.Networking.Recievers
{
    [System.Serializable]
    public class BasisAudioReceiver : BasisAudioReceiverBase
    {
        public BasisRemoteAudioDriver BasisRemoteVisemeAudioDriver;
        public void OnEnable(BasisNetworkedPlayer networkedPlayer)
        {
            // Initialize settings and audio source
            settings = BasisDeviceManagement.Instance.BasisOpusSettings;
            if (audioSource == null)
            {
                BasisRemotePlayer remotePlayer = (BasisRemotePlayer)networkedPlayer.Player;
                audioSource = BasisHelpers.GetOrAddComponent<AudioSource>(remotePlayer.AudioSourceTransform.gameObject);
            }
            audioSource.spatialize = true;
            audioSource.spatializePostEffects = true; //revist later!
            audioSource.spatialBlend = 1.0f;
            audioSource.dopplerLevel = 0;
            audioSource.volume = 1.0f;
            audioSource.loop = true;
            // Initialize sampling parameters
            samplingFrequency = settings.GetSampleFreq();
            numChannels = 1;
            SampleLength = samplingFrequency * numChannels;
            RingBuffer = new RingBuffer(4096*2);
            // Create AudioClip
            audioSource.clip = AudioClip.Create($"player [{networkedPlayer.NetId}]", 4096, numChannels, samplingFrequency, false, (buf) => 
            {
                Array.Fill(buf, 1.0f);
            });
            // Ensure decoder is initialized and subscribe to events
            if (decoder == null)
            {
                decoder = new BasisAudioDecoder();
                decoder.Initialize();
            }
            decoder.OnDecoded += OnDecoded;
            StartAudio();

            // Perform calibration
            OnCalibration(networkedPlayer);
        }
        public void OnDestroy()
        {
            // Unsubscribe from events on destroy
            if (decoder != null)
            {
                decoder.OnDecoded -= OnDecoded;
                decoder.Deinitalize();
            }
            if (audioSource != null)
            {
                audioSource.Stop();
                GameObject.Destroy(audioSource);
            }

            if (visemeDriver != null)
            {
                GameObject.Destroy(visemeDriver);
            }
        }
        public void OnCalibration(BasisNetworkedPlayer networkedPlayer)
        {
            // Ensure viseme driver is initialized for audio processing
            if (visemeDriver == null)
            {
                visemeDriver = BasisHelpers.GetOrAddComponent<BasisAudioAndVisemeDriver>(audioSource.gameObject);
            }
            visemeDriver.TryInitialize(networkedPlayer.Player);
            if (BasisRemoteVisemeAudioDriver == null)
            {
                BasisRemoteVisemeAudioDriver = BasisHelpers.GetOrAddComponent<BasisRemoteAudioDriver>(audioSource.gameObject);
                BasisRemoteVisemeAudioDriver.BasisAudioReceiver = this;
            }
            BasisRemoteVisemeAudioDriver.Initalize(visemeDriver);
        }
    }
}
