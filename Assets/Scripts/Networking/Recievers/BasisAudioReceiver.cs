using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BasisAudioReceiver
{
    [SerializeField]
    public BasisAudioDecoder decoder;
    [SerializeField]
    public AudioSource audioSource;
    [SerializeField]
    public BasisVisemeDriver visemeDriver;
    [SerializeField]
    public BasisOpusSettings settings;
    public int samplingFrequency;
    public int numChannels;
    public int SampleLength;

    public float[] ringBuffer;
    public int head = 0;
    public float lastDataReceivedTime;
    public float dataTimeout = 0.1f; // Timeout period in seconds
    public float[] SilentData;
    public int DataSize;
    public void Update()
    {
        if (Time.time - lastDataReceivedTime > dataTimeout)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                ClearRingBuffer();
            }
        }
    }
    // API to clear the ring buffer and reset the head
    public void ClearRingBuffer()
    {
        Array.Clear(ringBuffer, 0, SampleLength);
        head = 0;
    }
    public void OnDecoded(float[] pcm, int pcmLength)
    {
        // Write incoming PCM data into the ring buffer
        for (int Index = 0; Index < pcmLength; Index++)
        {
            ringBuffer[(head + Index) % SampleLength] = pcm[Index];
        }

        head = (head + pcmLength) % SampleLength;

        // Update the AudioClip with the new data
        audioSource.clip.SetData(ringBuffer, 0);

        //do we have enough data
        if (!audioSource.isPlaying && head > SampleLength / 2)
        {
            audioSource.Play();
        }

        // Update the timestamp of the last received data
        lastDataReceivedTime = Time.time;
    }

    public void OnDecodedSilence(float[] pcm, int pcmLength)
    {
      //  Debug.Log(" OnDecodedSilence");
        OnDecoded(pcm, pcmLength);
    }
    public void OnEnable(BasisNetworkedPlayer networkedPlayer, GameObject audioParent)
    {
        settings = BasisNetworkConnector.Instance.BasisOpusSettings;
        if (audioSource == null)
        {
            var remotePlayer = (BasisRemotePlayer)networkedPlayer.Player;
            audioSource = BasisHelpers.GetOrAddComponent<AudioSource>(remotePlayer.AudioSourceGameobject);
            audioSource.spatialize = true;
            audioSource.spatializePostEffects = false;
            audioSource.spatialBlend = 1.0f; // Ensure audio is fully 3D spatialized
        }
        samplingFrequency = settings.GetSampleFreq();
        numChannels = settings.GetChannelAsInt();
        SampleLength = samplingFrequency * numChannels;
        ringBuffer = new float[SampleLength];
        audioSource.clip = AudioClip.Create($"player [{networkedPlayer.NetId}]", SampleLength, numChannels, samplingFrequency, false);
        if (decoder == null)
        {
            decoder = BasisHelpers.GetOrAddComponent<BasisAudioDecoder>(audioParent);
        }
        decoder.OnDecoded += OnDecoded;
        audioSource.loop = false;
        audioSource.Play();
        OnCalibration(networkedPlayer);
    }

    public void OnDisable()
    {
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
        if (visemeDriver == null)
        {
            visemeDriver = BasisHelpers.GetOrAddComponent<BasisVisemeDriver>(audioSource.gameObject);
        }
        visemeDriver.audioLoopback = true;
        visemeDriver.audioSource = audioSource;
        visemeDriver.Initialize(networkedPlayer.Player.Avatar);
    }
}