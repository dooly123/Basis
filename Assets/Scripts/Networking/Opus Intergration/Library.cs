using System;
using System.Runtime.InteropServices;

namespace UnityOpus {
    public enum SamplingFrequency : int {
        Frequency_8000 = 8000,
        Frequency_12000 = 12000,
        Frequency_16000 = 16000,
        Frequency_24000 = 24000,
        Frequency_48000 = 48000,
    }

    public enum NumChannels : int {
        Mono = 1,
        Stereo = 2,
    }

    public enum OpusApplication : int {
        VoIP = 2048,
        Audio = 2049,
        RestrictedLowDelay = 2051,
    }

    public enum OpusSignal : int {
        Auto = -1000,
        Voice = 3001,
        Music = 3002
    }

    public enum ErrorCode {
        OK = 0,
        BadArg = -1,
        BufferTooSmall = -2,
        InternalError = -3,
        InvalidPacket = -4,
        Unimplemented = -5,
        InvalidState = -6,
        AllocFail = -7,
    }

    public class Library {
        public const int maximumPacketDuration = 5760;

#if UNITY_ANDROID
        const string dllName = "unityopus";
#else
        const string dllName = "UnityOpus";
#endif

        [DllImport(dllName)]
        public static extern IntPtr OpusEncoderCreate(
            SamplingFrequency samplingFrequency,
            NumChannels channels,
            OpusApplication application,
            out ErrorCode error);

        [DllImport(dllName)]
        public static extern int OpusEncode(
            IntPtr encoder,
            short[] pcm,
            int frameSize,
            byte[] data,
            int maxDataBytes);

        [DllImport(dllName)]
        public static extern int OpusEncodeFloat(
            IntPtr encoder,
            float[] pcm,
            int frameSize,
            byte[] data,
            int maxDataBytes);

        [DllImport(dllName)]
        public static extern void OpusEncoderDestroy(
            IntPtr encoder);

        [DllImport(dllName)]
        public static extern int OpusEncoderSetBitrate(
            IntPtr encoder,
            int bitrate);

        [DllImport(dllName)]
        public static extern int OpusEncoderSetComplexity(
            IntPtr encoder,
            int complexity);

        [DllImport(dllName)]
        public static extern int OpusEncoderSetSignal(
            IntPtr encoder,
            OpusSignal signal);

        [DllImport(dllName)]
        public static extern IntPtr OpusDecoderCreate(
            SamplingFrequency samplingFrequency,
            NumChannels channels,
            out ErrorCode error);

        [DllImport(dllName)]
        public static extern int OpusDecode(
            IntPtr decoder,
            byte[] data,
            int len,
            short[] pcm,
            int frameSize,
            int decodeFec);

        [DllImport(dllName)]
        public static extern int OpusDecodeFloat(
            IntPtr decoder,
            byte[] data,
            int len,
            float[] pcm,
            int frameSize,
            int decodeFec);

        [DllImport(dllName)]
        public static extern void OpusDecoderDestroy(
            IntPtr decoder);

        [DllImport(dllName)]
        public static extern void OpusPcmSoftClip(
            float[] pcm,
            int frameSize,
            NumChannels channels,
            float[] softclipMem);
    }
}
