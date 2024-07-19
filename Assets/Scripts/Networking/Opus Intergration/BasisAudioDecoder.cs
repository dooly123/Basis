using UnityEngine;
using System;
using UnityOpus;

public class BasisAudioDecoder : MonoBehaviour
{
    public event Action OnDecoded;
    AudioDecoder decoder;
    public BasisOpusSettings Settings;
    public float[] pcmBuffer;
    public int pcmLength;
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
        Settings = BasisDeviceManagement.Instance.BasisOpusSettings;
        pcmBuffer = new float[AudioDecoder.maximumPacketDuration * (int)Settings.NumChannels];
        decoder = new AudioDecoder(Settings.SamplingFrequency, Settings.NumChannels);
    }
    public void Deinitalize()
    {
        decoder.Dispose();
        decoder = null;
    }

    /// <summary>
    /// decodes data into the pcm buffer
    /// note that the pcm buffer is always going to have more data then submited.
    /// the pcm length is how much was actually encoded.
    /// </summary>
    /// <param name="data"></param>
    public void OnEncoded(byte[] data)
    {
        pcmLength = decoder.Decode(data, data.Length, pcmBuffer);
        OnDecoded?.Invoke();
    }
}