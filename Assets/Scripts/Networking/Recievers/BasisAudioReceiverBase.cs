using System;
using UnityEngine;

public class BasisAudioReceiverBase
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
    public CircularBuffer Buffer;

    public int samplingFrequency;
    public int numChannels;
    public int SampleLength;
    public static int SegmentSize = 480;
    public static int MaximumStored = 150;

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

        AudioClip clip = AudioClip.Create("BufferedAudio", entireBuffer.Length, numChannels, samplingFrequency, false);
        clip.SetData(entireBuffer, 0);
        audioSource.clip = clip;
        audioSource.Play();
    }

    [System.Serializable]
    public class CircularBuffer
    {
        public float[] Buffer;
        public int Head;
        public int Tail;
        public int BufferSize;
        public int SegmentSize;
        public int SegmentCount;
        public int CurrentCount;

        public CircularBuffer(int segmentSize, int segmentCount)
        {
            SegmentSize = segmentSize;
            SegmentCount = segmentCount;
            BufferSize = segmentSize * segmentCount;
            Buffer = new float[BufferSize];
            Head = 0;
            Tail = 0;
            CurrentCount = 0;
        }

        public void Clear()
        {
            Head = 0;
            Tail = 0;
            CurrentCount = 0;
            Array.Clear(Buffer, 0, BufferSize);
        }

        public void Add(float[] data)
        {
            if (data.Length != SegmentSize)
            {
                throw new ArgumentException($"Data length must be {SegmentSize}");
            }
            if (IsFull())
            {
                // Buffer is full, overwrite the oldest data
                Head = (Head + SegmentSize) % BufferSize;
                Debug.Log("Buffer was full old data was overwritten");
            }

            Array.Copy(data, 0, Buffer, Tail, SegmentSize);
            Tail = (Tail + SegmentSize) % BufferSize;
            CurrentCount = IsFull() ? SegmentCount : CurrentCount + 1;
        }

        public float[] GetNextSegment()
        {
            if (IsEmpty())
                return null;

            float[] segment = new float[SegmentSize];
            Array.Copy(Buffer, Head, segment, 0, SegmentSize);
            Head = (Head + SegmentSize) % BufferSize;
            CurrentCount--;
            return segment;
        }

        public bool IsFull()
        {
            return CurrentCount == SegmentCount;
        }

        public bool IsEmpty()
        {
            return CurrentCount == 0;
        }
    }
}