using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace UnityOpus
{
    public class Encoder : IDisposable
    {
        int bitrate;

        public int Bitrate
        {
            get { return bitrate; }
            set
            {
                lock (encoderLock)
                {
                    Library.OpusEncoderSetBitrate(encoder, value);
                    bitrate = value;
                }
            }
        }

        int complexity;
        public int Complexity
        {
            get
            {
                return complexity;
            }
            set
            {
                lock (encoderLock)
                {
                    Library.OpusEncoderSetComplexity(encoder, value);
                    complexity = value;
                }
            }
        }

        OpusSignal signal;
        public OpusSignal Signal
        {
            get { return signal; }
            set
            {
                lock (encoderLock)
                {
                    Library.OpusEncoderSetSignal(encoder, value);
                    signal = value;
                }
            }
        }

        IntPtr encoder;
        NumChannels channels;
        public readonly object encoderLock = new object(); // For thread safety
        public Encoder(SamplingFrequency samplingFrequency, NumChannels channels, OpusApplication application)
        {
            this.channels = channels;
            ErrorCode error;
            encoder = Library.OpusEncoderCreate(samplingFrequency, channels, application, out error);
            if (error != ErrorCode.OK)
            {
                Debug.Log("[UnityOpus] Failed to init encoder. Error code: " + error.ToString());
                encoder = IntPtr.Zero;
            }
        }

        public int Encode(float[] pcm, byte[] output)
        {
            if (encoder == IntPtr.Zero)
            {
                return 0;
            }

            lock (encoderLock)
            {
                return Library.OpusEncodeFloat(
                    encoder,
                    pcm,
                    pcm.Length / (int)channels,
                    output,
                    output.Length
                );
            }
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (encoder == IntPtr.Zero)
                {
                    return;
                }

                lock (encoderLock)
                {
                    Library.OpusEncoderDestroy(encoder);
                    encoder = IntPtr.Zero;
                }

                disposedValue = true;
            }
        }

        ~Encoder()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
