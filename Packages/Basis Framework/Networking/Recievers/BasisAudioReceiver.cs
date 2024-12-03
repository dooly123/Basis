using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management;
using Basis.Scripts.Drivers;
using Basis.Scripts.Networking.NetworkedPlayer;
using UnityEngine;

namespace Basis.Scripts.Networking.Recievers
{
    [System.Serializable]
    public class BasisAudioReceiver : BasisAudioReceiverBase
    {
        public BasisRemoteAudioDriver BasisRemoteVisemeAudioDriver;
        public void OnEnable(BasisNetworkedPlayer networkedPlayer, GameObject audioParent)
        {
            // Initialize settings and audio source
            settings = BasisDeviceManagement.Instance.BasisOpusSettings;
            if (audioSource == null)
            {
                var remotePlayer = (BasisRemotePlayer)networkedPlayer.Player;
                audioSource = BasisHelpers.GetOrAddComponent<AudioSource>(remotePlayer.AudioSourceGameobject);
            }
            audioSource.spatialize = true;
            audioSource.spatializePostEffects = false;
            audioSource.spatialBlend = 1.0f;
            audioSource.dopplerLevel = 0;
            audioSource.volume = 1.0f;
            audioSource.loop = true;
            // Initialize sampling parameters
            samplingFrequency = settings.GetSampleFreq();
            numChannels = settings.GetChannelAsInt();
            SampleLength = samplingFrequency * numChannels;

            // Create AudioClip
            audioSource.clip = AudioClip.Create($"player [{networkedPlayer.NetId}]", 2048*2, numChannels, samplingFrequency, false);
            // Ensure decoder is initialized and subscribe to events
            if (decoder == null)
            {
                decoder = BasisHelpers.GetOrAddComponent<BasisAudioDecoder>(audioParent);
            }
            decoder.OnDecoded += OnDecoded;
            audioSource.Play();

            // Perform calibration
            OnCalibration(networkedPlayer);
        }
        public void OnDestroy()
        {
            // Unsubscribe from events on destroy
            if (decoder != null)
            {
                decoder.OnDecoded -= OnDecoded;
            }
        }
        public void OnDisable()
        {
            // Clean up audio and related components
            if (decoder != null)
            {
                decoder.OnDecoded -= OnDecoded;
                GameObject.Destroy(decoder.gameObject);
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