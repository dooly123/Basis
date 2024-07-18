using System;
using UnityEngine;

[System.Serializable]
public class BasisAudioReceiver
{
    [SerializeField] public BasisAudioDecoder decoder;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public BasisVisemeDriver visemeDriver;
    [SerializeField] public BasisOpusSettings settings;

    public int samplingFrequency;
    public int numChannels;
    public int SampleLength;
    public float[] ringBuffer;
    public int head = 0;
    public float lastDataReceivedTime;
    public float dataTimeout = 0.1f; // Timeout period in seconds

    // Constructor
    public BasisAudioReceiver()
    {
        // Initialize settings or other variables if needed
    }

    public void LateUpdate()
    {
        // Check for data timeout
        if (Time.time - lastDataReceivedTime > dataTimeout)
        {
            if (audioSource.isPlaying)
            {
                ClearRingBuffer();
            }
        }
    }

    public void ClearRingBuffer()
    {
        Array.Clear(ringBuffer, 0, SampleLength);
        head = 0;
    }

    public void OnDecoded(float[] pcm, int pcmLength)
    {
        // Ensure incoming data fits within buffer
        if (pcmLength > SampleLength)
        {
            Debug.LogWarning("Received PCM data longer than buffer size. Dropping old data.");
            head = 0; // Reset head position
        }
        else if (head + pcmLength > SampleLength)
        {
            // Handle wrap-around if data doesn't fit at the current head position
            int remainingSpace = SampleLength - head;
            Array.Copy(pcm, 0, ringBuffer, head, remainingSpace);
            Array.Copy(pcm, remainingSpace, ringBuffer, 0, pcmLength - remainingSpace);
        }
        else
        {
            // Data fits normally in the buffer
            Array.Copy(pcm, 0, ringBuffer, head, pcmLength);
        }

        head = (head + pcmLength) % SampleLength;

        // Update AudioClip with new data
        audioSource.clip.SetData(ringBuffer, 0);

        // Start playback if stopped and buffer is sufficiently filled
        if (!audioSource.isPlaying && head > SampleLength / 2)
        {
            audioSource.Play();
        }

        // Update last received data time
        lastDataReceivedTime = Time.time;
    }

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
        ringBuffer = new float[SampleLength];

        // Create AudioClip
        audioSource.clip = AudioClip.Create($"player [{networkedPlayer.NetId}]", SampleLength, numChannels, samplingFrequency, false);

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

    private void OnDecoded()
    {
        OnDecoded(decoder.pcmBuffer, decoder.pcmLength);
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