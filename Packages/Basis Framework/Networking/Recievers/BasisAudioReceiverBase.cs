using Basis.Scripts.Drivers;
using System;
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
        public Queue<float[]> Buffer = new Queue<float[]>();
        public int samplingFrequency;
        public int numChannels;
        public int SampleLength;
        public float[] LatestBuffer;
        public void OnDecoded()
        {
            if (LatestBuffer == null || LatestBuffer.Length != decoder.pcmLength)
            {
                LatestBuffer = new float[decoder.pcmLength];
            }
            Array.Copy(decoder.pcmBuffer, 0, LatestBuffer, 0, decoder.pcmLength);
            OnDecoded(LatestBuffer);
        }

        public void OnDecoded(float[] pcm)
        {
            if (pcm.Length != decoder.pcmLength)
            {
                Debug.LogError($"PCM length {pcm.Length} does not match SegmentSize {decoder.pcmLength}");
                return;
            }
            Buffer.Enqueue(pcm);
        }
        /// <summary>
        /// processed in array size 2048 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="channels"></param>
        public void ProcessAudioSamples(float[] data, int channels)
        {
            Debug.Log("Processing " + data.Length);
            if (Buffer.TryDequeue(out float[] segment))
            {
                Array.Copy(segment, data, 2048);
            }
            else
            {
                Array.Clear(data, 0, 2048);
            }
        }
    }
}