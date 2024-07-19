using System;
using UnityEngine;

public class BasisAudioReceiverBase : MonoBehaviour
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
    public static int SegmentSize = 5760;
    public static int MaximumStored = 5;

    private void Start()
    {
        // Initialize buffer with SegmentSize and MaximumStored
        Buffer = new CircularBuffer(SegmentSize, MaximumStored);
    }

    public void OnDecoded(float[] pcm, int pcmLength)
    {
        if (pcm.Length != SegmentSize)
        {
            Debug.LogError($"PCM length {pcm.Length} does not match SegmentSize {SegmentSize}");
            return;
        }

        if (!Buffer.IsFull())
        {
            Buffer.Add(pcm);
            Debug.Log("Added PCM segment to buffer");
        }
        else
        {
            Debug.LogError("Buffer is full. Clearing old data.");
            Buffer.Clear();
            Buffer.Add(pcm);
        }
    }

    public void OnDecoded()
    {
        OnDecoded(decoder.pcmBuffer, decoder.pcmLength);
    }

    public void LateUpdate()
    {
        // Example of triggering playback if the buffer has enough data
        if (Buffer.CurrentCount >= MaximumStored)
        {
            PlayEntireBuffer();
        }
    }

    public void PlayEntireBuffer()
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

        Debug.Log($"Playing audio buffer with {entireBuffer.Length} samples");

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
            Debug.Log("Buffer cleared");
        }

        public bool Add(float[] data)
        {
            if (data.Length != SegmentSize)
                throw new ArgumentException($"Data length must be {SegmentSize}");

            if (IsFull())
                return false;

            Array.Copy(data, 0, Buffer, Tail, SegmentSize);
            Tail = (Tail + SegmentSize) % BufferSize;
            CurrentCount++;
            Debug.Log("Data added to buffer");
            return true;
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