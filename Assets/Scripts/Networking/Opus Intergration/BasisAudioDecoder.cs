using UnityEngine;
using System;
using UnityOpus;

public class BasisAudioDecoder : MonoBehaviour
{
    public event Action<float[], int> OnDecoded;
    AudioDecoder decoder;
    public float[] pcmBuffer;
    public BasisOpusSettings Settings;
    void OnEnable()
    {
        Initialize();
    }

    void OnDisable()
    {
        Deinitalize();
    }
    public void Initialize()
    {
        Settings = BasisNetworkConnector.Instance.BasisOpusSettings;
        pcmBuffer = new float[AudioDecoder.maximumPacketDuration * (int)Settings.NumChannels];
        decoder = new AudioDecoder(Settings.SamplingFrequency, Settings.NumChannels);
    }
    public void Deinitalize()
    {
        decoder.Dispose();
        decoder = null;
    }

    public void OnEncoded(byte[] data, int length)
    {
        var pcmLength = decoder.Decode(data, length, pcmBuffer);
        OnDecoded?.Invoke(pcmBuffer, pcmLength);
    }
}