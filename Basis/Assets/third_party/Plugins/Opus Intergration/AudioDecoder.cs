using UnityEngine;
using System;

namespace UnityOpus
{
    public class AudioDecoder : IDisposable {
        public const int maximumPacketDuration = Library.maximumPacketDuration;

        IntPtr decoder;
        readonly NumChannels channels;
        readonly float[] softclipMem;

        public AudioDecoder(SamplingFrequency samplingFrequency,NumChannels channels) {ErrorCode error;this.channels = channels;
            decoder = Library.OpusDecoderCreate(samplingFrequency,channels,out error);
            if (error != ErrorCode.OK)
            {
                Debug.LogError("[UnityOpus] Failed to create Decoder. Error code is " + error.ToString());
                decoder = IntPtr.Zero;
            }
            softclipMem = new float[(int)channels];
        }

        public int Decode(byte[] data,int dataLength,float[] pcm, int decodeFec = 0) {
            if (decoder == IntPtr.Zero) {
                return 0;
            }
            var decodedLength = Library.OpusDecodeFloat( decoder,data,dataLength, pcm, pcm.Length / (int)channels,decodeFec);
            Library.OpusPcmSoftClip(pcm, decodedLength / (int)channels, channels,softclipMem);
            return decodedLength;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (decoder == IntPtr.Zero) {
                    return;
                }
                Library.OpusDecoderDestroy(decoder);
                decoder = IntPtr.Zero;

                disposedValue = true;
            }
        }

        ~AudioDecoder() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
