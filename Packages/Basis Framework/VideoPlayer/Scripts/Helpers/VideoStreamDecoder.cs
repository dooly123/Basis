using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace FFmpeg.Unity.Helpers
{
    public sealed unsafe class VideoStreamDecoder : IDisposable
    {
        private readonly FFmpegCtx _ctx;
        private readonly AVCodecContext* _pCodecContext;
        private readonly AVFrame* _pFrame;
        private AVFrame* _receivedFrame;
        internal readonly int _streamIndex;
        private readonly AVCodecContext_get_format get_hw_fmt;
        private readonly AVBufferRef* hw_device_ctx;
        public AVPixelFormat HWPixelFormat { get; } = AVPixelFormat.AV_PIX_FMT_NONE;
        public string CodecName { get; }
        public Size FrameSize { get; }
        public AVPixelFormat PixelFormat { get; }
        public int Channels { get; }
        public AVSampleFormat SampleFormat { get; }
        public int SampleRate { get; }

        public VideoStreamDecoder(FFmpegCtx ctx, AVMediaType type, AVHWDeviceType HWDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        {
            _ctx = ctx;
            AVCodec* codec = null;
            _streamIndex = ffmpeg
                .av_find_best_stream(_ctx._pFormatContext, type, -1, -1, &codec, 0)
                .ThrowExceptionIfError();

            if (HWDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                for (int i = 0; ; i++)
                {
                    var codecHWConfig = ffmpeg.avcodec_get_hw_config(codec, i);
                    // UnityEngine.Debug.Log($"HW at index {i} ({codecHWConfig->device_type}) {codecHWConfig->methods}");
                    if (codecHWConfig == null)
                    {
                        UnityEngine.Debug.LogError("No HW decoder found.");
                        HWDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
                        break;
                    }
                    else if ((codecHWConfig->methods & 1) == 0 || codecHWConfig->device_type != HWDeviceType)
                    {
                        UnityEngine.Debug.LogWarning($"HW at index {i} ({codecHWConfig->device_type}) not support/selected.");
                        continue;
                    }
                    else
                    {
                        HWPixelFormat = codecHWConfig->pix_fmt;
                        UnityEngine.Debug.Log($"HW at index {i} ({codecHWConfig->device_type}) format {HWPixelFormat} selected.");
                        break;
                    }
                }
            }

            _pCodecContext = ffmpeg.avcodec_alloc_context3(codec);
            _pCodecContext->thread_count = 0;

            if (HWDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                get_hw_fmt = GetHWFormat;
                _pCodecContext->get_format = get_hw_fmt;
            }

            ffmpeg.avcodec_parameters_to_context(_pCodecContext, _ctx._pFormatContext->streams[_streamIndex]->codecpar)
                .ThrowExceptionIfError();

            if (HWDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                // var hw_device_ctx = this.hw_device_ctx;
                fixed (AVBufferRef** hw_device_ctx = &this.hw_device_ctx)
                {
                    ffmpeg.av_hwdevice_ctx_create(hw_device_ctx, HWDeviceType, null, null, 0)
                        .ThrowExceptionIfError();
                }
                _pCodecContext->hw_device_ctx = ffmpeg.av_buffer_ref(hw_device_ctx);
            }
            ffmpeg.avcodec_open2(_pCodecContext, codec, null).ThrowExceptionIfError();

            CodecName = ffmpeg.avcodec_get_name(codec->id);
            FrameSize = new Size(_pCodecContext->width, _pCodecContext->height);
            PixelFormat = _pCodecContext->pix_fmt;
            Channels = _pCodecContext->ch_layout.nb_channels;
            SampleFormat = _pCodecContext->sample_fmt;
            SampleRate = _pCodecContext->sample_rate;

            _pFrame = ffmpeg.av_frame_alloc();
            _receivedFrame = ffmpeg.av_frame_alloc();
        }

        private AVPixelFormat GetHWFormat(AVCodecContext* @s, AVPixelFormat* @fmt)
        {
            int* p;

            for (p = (int*)fmt; *p != -1; p++)
            {
                if (*p == (int)HWPixelFormat)
                    return (AVPixelFormat)(*p);
            }

            return AVPixelFormat.AV_PIX_FMT_NONE;
        }

        private AVPixelFormat GetSWFormat(AVCodecContext* @s, AVPixelFormat* @fmt)
        {
            int* p;

            for (p = (int*)fmt; *p != -1; p++)
            {
                // if (*p == (int)HWPixelFormat)
                    return (AVPixelFormat)(*p);
            }

            return AVPixelFormat.AV_PIX_FMT_NONE;
        }

        public void Dispose()
        {
            var pFrame = _pFrame;
            if (_pFrame != null)
                ffmpeg.av_frame_free(&pFrame);
            var receivedFrame = _receivedFrame;
            if (_receivedFrame != null)
                ffmpeg.av_frame_free(&receivedFrame);

            if (_pCodecContext != null)
                ffmpeg.avcodec_close(_pCodecContext);
        }

        public bool CanDecode()
        {
            if (_ctx.EndReached || _ctx._pPacket->stream_index != _streamIndex)
            {
                return false;
            }
            return true;
        }

        public int Decode(out AVFrame frame)
        {
            if (_ctx.EndReached || _ctx._pPacket->stream_index != _streamIndex)
            {
                frame = new AVFrame()
                {
                    format = -1
                };
                return -1;
            }
            ffmpeg.av_frame_unref(_pFrame);
            ffmpeg.av_frame_unref(_receivedFrame);
            ffmpeg.avcodec_send_packet(_pCodecContext, _ctx._pPacket).ThrowExceptionIfError();
            int error = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);
            if (error == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                frame = new AVFrame()
                {
                    format = -1
                };
                return 1;
            }
            error.ThrowExceptionIfError();

            if (_pCodecContext->hw_device_ctx != null)
            {
                _receivedFrame->hw_frames_ctx = _pCodecContext->hw_device_ctx;
                ffmpeg.av_hwframe_transfer_data(_receivedFrame, _pFrame, 0).ThrowExceptionIfError();
                frame = *_receivedFrame;
                // /*
                frame.pts = _pFrame->pts;
                frame.duration = _pFrame->duration;
                frame.time_base = _pFrame->time_base;
                // */
            }
            else
                frame = *_pFrame;
            // fixed (int* w = &frame.width)
            // fixed (int* h = &frame.height)
            {
                // ffmpeg.avcodec_align_dimensions(_pCodecContext, w, h);
            }
            return 0;
        }

        public void Seek()
        {
            ffmpeg.avcodec_flush_buffers(_pCodecContext);
            return;
            ffmpeg.av_frame_unref(_pFrame);
            ffmpeg.av_frame_unref(_receivedFrame);
            ffmpeg.avcodec_flush_buffers(_pCodecContext);
            // ffmpeg.avcodec_send_packet(_pCodecContext, null).ThrowExceptionIfError();
        }

        public void Seek(long offset)
        {
            AVRational base_q = ffmpeg.av_get_time_base_q();
            long target = ffmpeg.av_rescale_q(offset, base_q, _ctx._pFormatContext->streams[_streamIndex]->time_base);
            // ffmpeg.avformat_seek_file(_pFormatContext, _streamIndex, 0, target, long.MaxValue, ffmpeg.AVFMT_SEEK_TO_PTS).ThrowExceptionIfError();
            ffmpeg.av_seek_frame(_ctx._pFormatContext, _streamIndex, offset, ffmpeg.AVSEEK_FLAG_FRAME).ThrowExceptionIfError();
        }

        public void Seek(double offset)
        {
            double fps = ((double)Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->time_base.den) / Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->time_base.num));
            Seek((long)Math.Floor(offset * fps));
        }

        public long GetTimeStamp(AVFrame frame)
        {
            return frame.pts;
        }

        public double GetTime(AVFrame frame)
        {
            double fps = ((double)Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->time_base.den) / Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->time_base.num));
            // double fps = _ctx._pFormatContext->streams[_streamIndex]->time_base.den * GetFPS();
            // double fps = ((double)Math.Max(1, _pFormatContext->streams[_streamIndex]->r_frame_rate.den) / Math.Max(1, _pFormatContext->streams[_streamIndex]->r_frame_rate.num));
            // double fps = ((double)Math.Max(1, frame.time_base.num) / Math.Max(1, frame.time_base.den));
            return GetTimeStamp(frame) / fps;
        }

        public static double GetFrameTime(AVFrame frame)
        {
            double fps = (double)Math.Max(1, frame.time_base.num) / Math.Max(1, frame.time_base.den);
            return frame.pts * fps;
        }

        public double GetEndTime(AVFrame frame)
        {
            double fps = ((double)Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->time_base.den) / Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->time_base.num));
            return (frame.pts + frame.duration) / fps;
        }

        public double GetDuration(AVFrame frame)
        {
            double fps = ((double)Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->time_base.den) / Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->time_base.num));
            return frame.duration / fps;
        }

        public double GetFPS()
        {
            // double fps = ((double)Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->r_frame_rate.den) / Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->r_frame_rate.num));
            double fps = ((double)Math.Max(0, _ctx._pFormatContext->streams[_streamIndex]->avg_frame_rate.den) / Math.Max(0, _ctx._pFormatContext->streams[_streamIndex]->avg_frame_rate.num));
            return fps;
        }

        public double GetTimeBase()
        {
            double fps = ((double)Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->time_base.den) / Math.Max(1, _ctx._pFormatContext->streams[_streamIndex]->time_base.num));
            return fps;
        }

        public double GetTimeBaseQ()
        {
            AVRational base_q = ffmpeg.av_get_time_base_q();
            double fps_base = ((double)Math.Max(1, base_q.den) / Math.Max(1, base_q.num));
            return fps_base;
        }

        public IReadOnlyDictionary<string, string> GetContextInfo()
        {
            AVDictionaryEntry* tag = null;
            var result = new Dictionary<string, string>();

            while ((tag = ffmpeg.av_dict_get(_ctx._pFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);
                result.Add(key, value);
            }

            return result;
        }
    }
}