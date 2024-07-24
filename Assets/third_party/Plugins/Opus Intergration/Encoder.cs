using System;

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
                Library.OpusEncoderSetBitrate(encoder, value);
                bitrate = value;
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
                Library.OpusEncoderSetComplexity(encoder, value);
                complexity = value;
            }
        }

        OpusSignal signal;
        public OpusSignal Signal
        {
            get { return signal; }
            set
            {
                Library.OpusEncoderSetSignal(encoder, value);
                signal = value;
            }
        }

        IntPtr encoder;
        NumChannels channels;

        public Encoder(SamplingFrequency samplingFrequency,NumChannels channels, OpusApplication application)
        {
            this.channels = channels;
            ErrorCode error;
            encoder = Library.OpusEncoderCreate(samplingFrequency,channels,application,out error);
            if (error != ErrorCode.OK)
            {
                UnityEngine.Debug.LogError("[UnityOpus] Failed to init encoder. Error code: " + error.ToString());
                encoder = IntPtr.Zero;
            }
        }

        public int Encode(float[] pcm, byte[] output)
        {
            if (encoder == IntPtr.Zero)
            {
                return 0;
            }
            return Library.OpusEncodeFloat(
                encoder,
                pcm,
                pcm.Length / (int)channels,
                output,
                output.Length
                );
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (encoder == IntPtr.Zero)
                {
                    return;
                }
                Library.OpusEncoderDestroy(encoder);
                encoder = IntPtr.Zero;

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