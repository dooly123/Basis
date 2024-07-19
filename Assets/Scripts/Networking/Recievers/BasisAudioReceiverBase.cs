using System;
using UnityEngine;

public partial class BasisAudioReceiverBase
{
    [SerializeField]
    public BasisAudioDecoder decoder;
    [SerializeField]
    public AudioSource audioSource;
    [SerializeField]
    public BasisVisemeDriver visemeDriver;
    [SerializeField]
    public BasisOpusSettings settings;
    [SerializeField]
    public BasisFloatCircularBuffer Buffer;

    public int samplingFrequency;
    public int numChannels;
    public int SampleLength;
    public static int SegmentSize = 480;
    public static int MaximumStored = 50;

    public void OnDecoded()
    {
        float[] newBuffer = new float[decoder.pcmLength];
        Array.Copy(decoder.pcmBuffer, 0, newBuffer, 0, decoder.pcmLength);
        OnDecoded(newBuffer);
    }

    public void OnDecoded(float[] pcm)
    {
        if (pcm.Length != SegmentSize)
        {
            Debug.LogError($"PCM length {pcm.Length} does not match SegmentSize {SegmentSize}");
            return;
        }
        Buffer.Add(pcm);
    }

    public void LateUpdate()
    {
        if (Buffer.CurrentCount >= MaximumStored)
        {
            PlayEntireBuffer();
        }
    }

    private void PlayEntireBuffer()
    {
        int totalSegments = Buffer.CurrentCount;
        float[] entireBuffer = new float[totalSegments * SegmentSize];
        int position = 0;

        while (!Buffer.IsEmpty())
        {
            float[] segment = Buffer.GetNextSegment();
            if (segment != null)
            {
                Array.Copy(segment, 0, entireBuffer, position, segment.Length);
                position += segment.Length;
            }
        }
        audioSource.clip.SetData(entireBuffer, 0);
        audioSource.Play();
    }
}