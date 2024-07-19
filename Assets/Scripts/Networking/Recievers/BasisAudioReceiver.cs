using UnityEngine;

[System.Serializable]
public class BasisAudioReceiver : BasisAudioReceiverBase
{
    public void OnEnable(BasisNetworkedPlayer networkedPlayer, GameObject audioParent)
    {
        // Initialize settings and audio source
        settings = BasisDeviceManagement.Instance.BasisOpusSettings;
        if (audioSource == null)
        {
            var remotePlayer = (BasisRemotePlayer)networkedPlayer.Player;
            audioSource = BasisHelpers.GetOrAddComponent<AudioSource>(remotePlayer.AudioSourceGameobject);
            audioSource.spatialize = true;
            audioSource.spatializePostEffects = false;
            audioSource.spatialBlend = 1.0f; // Fully 3D spatialized
        }

        // Initialize sampling parameters
        samplingFrequency = settings.GetSampleFreq();
        numChannels = settings.GetChannelAsInt();
        SampleLength = samplingFrequency * numChannels;

        // Create AudioClip
        Buffer = new BasisFloatCircularBuffer(SegmentSize, MaximumStored);
        audioSource.clip = AudioClip.Create($"player [{networkedPlayer.NetId}]", Buffer.BufferSize, numChannels, samplingFrequency, false);
        // Ensure decoder is initialized and subscribe to events
        if (decoder == null)
        {
            decoder = BasisHelpers.GetOrAddComponent<BasisAudioDecoder>(audioParent);
        }
        decoder.OnDecoded += OnDecoded;

        // Start audio playback
        audioSource.loop = false;
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
            visemeDriver = BasisHelpers.GetOrAddComponent<BasisVisemeDriver>(audioSource.gameObject);
        }
        visemeDriver.audioLoopback = true;
        visemeDriver.audioSource = audioSource;
        visemeDriver.Initialize(networkedPlayer.Player.Avatar);
    }
}