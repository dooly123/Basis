using Basis.Scripts.Drivers;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public void OnDecoded()
        {
            OnDecoded(decoder.pcmBuffer, decoder.pcmLength);
        }
        public void OnDecoded(float[] pcm, int length)
        {
            if (length != decoder.pcmLength)
            {
                Debug.LogError($"PCM length {length} does not match SegmentSize {decoder.pcmLength}");
                return;
            }
            RingBuffer.Add(pcm, decoder.pcmLength);
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
            //no need to lock unlock as its only used here!
            RingBuffer.BufferedReturn.Enqueue(segment);
        }
    }
    public class RingBuffer
    {
        private readonly float[] buffer;
        private int head; // Points to the next position to write
        private int tail; // Points to the next position to read
        private int size; // Current data size in the buffer
        private readonly object lockObject = new();
        public Queue<float[]> BufferedReturn = new Queue<float[]>();
        public RingBuffer(int capacity)
        {
            if (capacity <= 0) throw new ArgumentException("Capacity must be greater than zero.");
            buffer = new float[capacity];
            head = 0;
            tail = 0;
            size = 0;
        }
        public int Capacity => buffer.Length;
        public void Add(float[] segment, int length)
        {
            if (segment == null || segment.Length == 0)
                throw new ArgumentNullException(nameof(segment));
            if (length <= 0 || length > segment.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be a positive number and less than or equal to the segment's length.");

            lock (lockObject)
            {
                // If the segment is larger than the buffer capacity, we can't fit it
                if (length > Capacity)
                {
                    throw new InvalidOperationException("The segment is too large to fit into the buffer.");
                }

                // If there's not enough space, remove oldest data until there is enough space
                while (length > Capacity - size)
                {
                    // Remove the oldest item to make room for new data
                    tail = (tail + 1) % Capacity;
                    size--;
                }

                // Add data up to the valid 'length'
                for (int i = 0; i < length; i++)
                {
                    buffer[head] = segment[i];
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
                BufferedReturn.TryDequeue(out float[] segment);
                if (segment == null || segment.Length != segmentSize)
                {
                    segment = new float[segmentSize];
                }
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