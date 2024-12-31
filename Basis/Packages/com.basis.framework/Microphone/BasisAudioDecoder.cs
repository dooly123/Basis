using UnityEngine;
using System;
using UnityOpus;
using Basis.Scripts.Device_Management;

public class BasisAudioDecoder
{
    public event Action OnDecoded;
    AudioDecoder decoder;
    public BasisOpusSettings Settings;
    public float[] pcmBuffer;
    public int pcmLength;
    public int FakepcmLength;
    public void Initialize()
    {
        FakepcmLength = 2048;
        pcmLength = 2048;
        Settings = BasisDeviceManagement.Instance.BasisOpusSettings;
        pcmBuffer = new float[FakepcmLength * (int)Settings.NumChannels];//AudioDecoder.maximumPacketDuration now its 2048
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
    public void OnDecode(byte[] data, int length)
    {
       // UnityEngine.BasisDebug.Log("decode size is " + data.Length + " " + length);
        pcmLength = decoder.Decode(data, length, pcmBuffer);
        OnDecoded?.Invoke();
    }
}