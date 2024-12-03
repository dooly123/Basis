using Basis.Scripts.Drivers;
using System;
using UnityEngine;

namespace Basis.Scripts.Networking.Recievers
{
    public partial class BasisAudioReceiverBase
    {
        [SerializeField]
        public BasisAudioDecoder decoder;
        [SerializeField]
        public AudioSource audioSource;
        [SerializeField]
        public BasisAudioAndVisemeDriver visemeDriver;
        [SerializeField]
        public BasisOpusSettings settings;
        [SerializeField]
        public RingBuffer RingBuffer;
        public int samplingFrequency;
        public int numChannels;
        public int SampleLength;
        public int MaxQueueSize = 4;
        public void OnDecoded()
        {
            OnDecoded(decoder.pcmBuffer);
        }
        public void OnDecoded(float[] pcm)
        {
            if (pcm.Length != decoder.pcmLength)
            {
                Debug.LogError($"PCM length {pcm.Length} does not match SegmentSize {decoder.pcmLength}");
                return;
            }
            RingBuffer.Add(pcm, pcm.Length);
        }
        /// <summary>
        /// processed in array size 2048 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="channels"></param>
        public void ProcessAudioSamples(float[] data, int channels)
        {
            // Ensure you have enough data for the required samples
            int frames = data.Length / channels; // Number of audio frames
            float[] segment = RingBuffer.Remove(frames); // Get only as many frames as needed

            // Populate the interleaved data array
            for (int i = 0; i < frames; i++)
            {
                float sample = segment[i]; // Get the sample from your single-channel RingBuffer

                for (int c = 0; c < channels; c++)
                {
                    data[i * channels + c] = sample; // Copy the sample to all channels
                }
            }
        }
    }
    public class RingBuffer
    {
        private readonly float[] buffer;
        private int head; // Points to the next position to write
        private int tail; // Points to the next position to read
        private int size; // Current data size in the buffer
        private readonly object lockObject = new();

        public RingBuffer(int capacity)
        {
            if (capacity <= 0) throw new ArgumentException("Capacity must be greater than zero.");
            buffer = new float[capacity];
            head = 0;
            tail = 0;
            size = 0;
        }

        public int Capacity => buffer.Length;

        public int Count
        {
            get
            {
                lock (lockObject)
                {
                    return size;
                }
            }
        }

        public bool IsFull => Count == Capacity;

        public bool IsEmpty => Count == 0;

        public void Add(float[] segment, int length)
        {
            if (segment == null || segment.Length == 0)
                throw new ArgumentNullException(nameof(segment));
            if (length <= 0 || length > segment.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be a positive number and less than or equal to the segment's length.");

            lock (lockObject)
            {
                // Check if there's enough space in the buffer for the valid data (length)
                if (length > Capacity - size)
                {
                    throw new InvalidOperationException("Not enough space to add the segment.");
                }

                // Add data up to the valid 'length'
                for (int i = 0; i < length; i++)
                {
                    float b = segment[i];
                    buffer[head] = b;
                    head = (head + 1) % Capacity;
                    size++;
                }
            }
        }
        public float[] Remove(int segmentSize)
        {
            if (segmentSize <= 0) throw new ArgumentOutOfRangeException(nameof(segmentSize));

            lock (lockObject)
            {
                float[] segment = new float[segmentSize];
                int bytesToRemove = Math.Min(segmentSize, size);

                if (bytesToRemove < segmentSize)
                {
                    Console.WriteLine($"Warning: Requested {segmentSize} items, but only {bytesToRemove} are available.");
                }

                for (int i = 0; i < bytesToRemove; i++)
                {
                    segment[i] = buffer[tail];
                    tail = (tail + 1) % Capacity;
                    size--;
                }

                // If not enough data, remaining part of segment is already zeroed
                return segment;
            }
        }
    }
}