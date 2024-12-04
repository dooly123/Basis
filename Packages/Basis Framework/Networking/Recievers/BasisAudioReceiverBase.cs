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
            int length = data.Length;
            int frames = length / channels; // Number of audio frames
            if (RingBuffer.IsEmpty)
            {
              //  Debug.Log("no voice data clearing out existing");
                Array.Clear(data, 0, length);
                return;
            }
            RingBuffer.Remove(frames, out float[] segment);

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
        public bool IsEmpty
        {
            get
            {
                lock (lockObject)
                {
                    return size == 0;
                }
            }
        }

        public void Add(float[] segment, int length)
        {
            if (segment == null || segment.Length == 0)
                throw new ArgumentNullException(nameof(segment));
            if (length <= 0 || length > segment.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be a positive number and less than or equal to the segment's length.");

            lock (lockObject)
            {
                if (length > Capacity)
                    throw new InvalidOperationException("The segment is too large to fit into the buffer.");

                // Remove old data to make room for new data
                int availableSpace = Capacity - size;
                if (length > availableSpace)
                {
                    int itemsToRemove = length - availableSpace;
                    tail = (tail + itemsToRemove) % Capacity;
                    size -= itemsToRemove;
                }

                // Add the new segment to the buffer
                int firstPart = Math.Min(length, Capacity - head); // Space till the end of the buffer
                Array.Copy(segment, 0, buffer, head, firstPart);   // Copy first part
                int remaining = length - firstPart;
                if (remaining > 0)
                {
                    Array.Copy(segment, firstPart, buffer, 0, remaining); // Copy wrap-around part
                }

                head = (head + length) % Capacity; // Update head
                size += length;                    // Update size
            }
        }
        public void Remove(int segmentSize, out float[] segment)
        {
            if (segmentSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(segmentSize));

            lock (lockObject)
            {
                // Try to reuse an array from the buffer pool
                if (!BufferedReturn.TryDequeue(out segment) || segment.Length != segmentSize)
                {
                    segment = new float[segmentSize];
                }

                // Calculate the actual number of items to remove
                int itemsToRemove = Math.Min(segmentSize, size);

                // Remove items in bulk
                int firstPart = Math.Min(itemsToRemove, Capacity - tail); // Items till the end of the buffer
                Array.Copy(buffer, tail, segment, 0, firstPart);          // Copy first part
                int remaining = itemsToRemove - firstPart;
                if (remaining > 0)
                {
                    Array.Copy(buffer, 0, segment, firstPart, remaining); // Copy wrap-around part
                }

                // Update tail and size
                tail = (tail + itemsToRemove) % Capacity;
                size -= itemsToRemove;

                // Warn if fewer items were available than requested
                if (itemsToRemove < segmentSize)
                {
                    Console.WriteLine($"Warning: Requested {segmentSize} items, but only {itemsToRemove} are available.");
                }
            }
        }
    }
}