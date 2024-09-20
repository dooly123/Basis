using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FFmpeg.Unity.Helpers
{
    public sealed unsafe class FFmpegCtx : IDisposable
    {
        internal readonly AVFormatContext* _pFormatContext;
        internal readonly AVIOContext* _pIOContext;
        internal readonly Stream _stream;
        internal readonly avio_alloc_context_read_packet read;
        internal readonly avio_alloc_context_seek seek;
        internal readonly GCHandle streamHandle;
        internal byte* bufferPtr = null;
        internal readonly AVPacket* _pPacket;
        internal readonly int _videoIndex = 0;
        public bool EndReached { get; private set; } = false;
        public bool IsValid { get; private set; } = false;

        public FFmpegCtx(Stream stream, uint bufferSize = 16_000_000)
        {
            _stream = stream;
            bufferPtr = (byte*)ffmpeg.av_malloc(bufferSize);
            read = ReadPacketCallback;
            if (stream.CanSeek)
                seek = SeekPacketCallback;
            streamHandle = GCHandle.Alloc(_stream, GCHandleType.Normal);
            _pIOContext = ffmpeg.avio_alloc_context(bufferPtr, (int)bufferSize, 0, GCHandle.ToIntPtr(streamHandle).ToPointer(), read, null, seek);

            _pFormatContext = ffmpeg.avformat_alloc_context();
            _pFormatContext->pb = _pIOContext;
            _pFormatContext->flags |= ffmpeg.AVFMT_FLAG_CUSTOM_IO | ffmpeg.AVFMT_NOFILE;
            var pFormatContext = _pFormatContext;
            var url = "some_dummy_filename";
            ffmpeg.avformat_open_input(&pFormatContext, url, null, null).ThrowExceptionIfError();
            ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();

            _videoIndex = ffmpeg.av_find_best_stream(_pFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, null, 0).ThrowExceptionIfError();
            // UnityEngine.Debug.Log(_videoIndex);

            // UnityEngine.Debug.Log($"{AVRationalToString(_pFormatContext->streams[_videoIndex]->avg_frame_rate)} {AVRationalToString(_pFormatContext->streams[_videoIndex]->r_frame_rate)} {AVRationalToString(_pFormatContext->streams[_videoIndex]->time_base)}");

            _pPacket = ffmpeg.av_packet_alloc();
            IsValid = true;
        }

        public FFmpegCtx(string url)
        {
            _pFormatContext = ffmpeg.avformat_alloc_context();
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_open_input(&pFormatContext, url, null, null).ThrowExceptionIfError();
            ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();

            _videoIndex = ffmpeg.av_find_best_stream(_pFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, null, 0).ThrowExceptionIfError();

            _pPacket = ffmpeg.av_packet_alloc();
            IsValid = true;
        }

        public static string AVRationalToString(AVRational av)
        {
            return $"{av.num}/{av.den}";
        }

        public double GetLength()
        {
            double time_base = (double)_pFormatContext->streams[_videoIndex]->time_base.num / _pFormatContext->streams[_videoIndex]->time_base.den;
            return _pFormatContext->streams[_videoIndex]->duration * time_base;
        }

        public bool TryGetFps(out double fps)
        {
            fps = default;
            int _streamIndex = _pPacket->stream_index;
            _streamIndex = _videoIndex;
            if (_streamIndex < 0 || _streamIndex > _pFormatContext->nb_streams)
                return false;
            fps = (double)_pFormatContext->streams[_streamIndex]->avg_frame_rate.num / _pFormatContext->streams[_streamIndex]->avg_frame_rate.den;
            return true;
        }

        public bool TryGetFps(VideoStreamDecoder decoder, out double fps)
        {
            fps = default;
            // int _streamIndex = _pPacket->stream_index;
            // _streamIndex = _videoIndex;
            int _streamIndex = decoder._streamIndex;
            if (_streamIndex < 0 || _streamIndex > _pFormatContext->nb_streams)
                return false;
            fps = (double)_pFormatContext->streams[_streamIndex]->avg_frame_rate.num / _pFormatContext->streams[_streamIndex]->avg_frame_rate.den;
            return true;
        }

        public bool TryGetPts(out double fps)
        {
            fps = default;
            int _streamIndex = _pPacket->stream_index;
            _streamIndex = _videoIndex;
            if (_streamIndex < 0 || _streamIndex > _pFormatContext->nb_streams)
                return false;
            fps = (double)_pFormatContext->streams[_streamIndex]->time_base.den / ((double)_pFormatContext->streams[_streamIndex]->avg_frame_rate.num / _pFormatContext->streams[_streamIndex]->avg_frame_rate.den);
            return true;
        }

        public bool TryGetPts(VideoStreamDecoder decoder, out double fps)
        {
            fps = default;
            int _streamIndex = decoder._streamIndex;
            if (_streamIndex < 0 || _streamIndex > _pFormatContext->nb_streams)
                return false;
            fps = (double)_pFormatContext->streams[_streamIndex]->time_base.den / ((double)_pFormatContext->streams[_streamIndex]->avg_frame_rate.num / _pFormatContext->streams[_streamIndex]->avg_frame_rate.den);
            return true;
        }

        public bool TryGetTimeBase(out double timebase)
        {
            timebase = default;
            int _streamIndex = _pPacket->stream_index;
            _streamIndex = _videoIndex;
            if (_streamIndex < 0 || _streamIndex > _pFormatContext->nb_streams)
                return false;
            timebase = (double)_pFormatContext->streams[_streamIndex]->time_base.num / _pFormatContext->streams[_streamIndex]->time_base.den;
            return true;
        }

        public bool TryGetStart(out double start)
        {
            start = default;
            int _streamIndex = _pPacket->stream_index;
            _streamIndex = _videoIndex;
            if (_streamIndex < 0 || _streamIndex > _pFormatContext->nb_streams)
                return false;
            double timebase = (double)_pFormatContext->streams[_streamIndex]->time_base.num / _pFormatContext->streams[_streamIndex]->time_base.den;
            start = _pFormatContext->streams[_streamIndex]->start_time * timebase;
            return start != ffmpeg.AV_NOPTS_VALUE;
        }

        public bool TryGetTime(out double time)
        {
            time = default;
            if (_pPacket == null)
                return false;
            int _streamIndex = _pPacket->stream_index;
            _streamIndex = _videoIndex;
            if (_streamIndex < 0 || _streamIndex > _pFormatContext->nb_streams)
                return false;
            double timebase = (double)_pFormatContext->streams[_streamIndex]->time_base.num / _pFormatContext->streams[_streamIndex]->time_base.den;
            time = _pPacket->pts * timebase;
            return true;
        }

        public bool TryGetTime(VideoStreamDecoder decoder, out double time)
        {
            time = default;
            int _streamIndex = decoder._streamIndex;
            if (_streamIndex < 0 || _streamIndex > _pFormatContext->nb_streams)
                return false;
            double timebase = (double)_pFormatContext->streams[_streamIndex]->time_base.num / _pFormatContext->streams[_streamIndex]->time_base.den;
            time = _pPacket->pts * timebase;
            return true;
        }

        public bool TryGetTime(VideoStreamDecoder decoder, AVFrame frame, out double time)
        {
            time = default;
            int _streamIndex = decoder._streamIndex;
            if (_streamIndex < 0 || _streamIndex > _pFormatContext->nb_streams)
                return false;
            double timebase = (double)_pFormatContext->streams[_streamIndex]->time_base.num / _pFormatContext->streams[_streamIndex]->time_base.den;
            time = frame.pts * timebase;
            return true;
        }

        internal static int ReadPacketCallback(void* @opaque, byte* @buf, int @buf_size)
        {
            int ret = ffmpeg.AVERROR_EOF;
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (!handle.IsAllocated)
            {
                return ret;
            }
            var stream = (Stream)handle.Target;
            if (buf == null)
            {
                return ret;
            }
            // if (buf_size == 0)
                // return 0;
            var span = new Span<byte>(buf, buf_size);
            if (stream == null || !stream.CanRead)
            {
                return ret;
            }
            // UnityEngine.Debug.Log(stream);
            // UnityEngine.Debug.Log(stream.GetType().FullName);
            int count = stream.Read(span);
            // UnityEngine.Debug.Assert(count > 0);
            return count == 0 ? ret : count;
        }

        internal static long SeekPacketCallback(void* @opaque, long @offset, int @whence)
        {
            int ret = ffmpeg.AVERROR_EOF;
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (!handle.IsAllocated)
            {
                return ret;
            }
            var stream = (Stream)handle.Target;
            if (stream == null || !stream.CanSeek)
            {
                return ret;
            }
            long idk = stream.Seek(offset, SeekOrigin.Begin);
            return idk;
        }

        public bool NextFrame(out AVPacket packet)
        {
            if (!IsValid)
            {
                packet = default;
                return false;
            }
            /*if (EndReached)
            {
                frame = default;
                // frame = *_pFrame;
                return false;
            }*/
            int error;
            do
            {
                ffmpeg.av_packet_unref(_pPacket);
                error = ffmpeg.av_read_frame(_pFormatContext, _pPacket);
                if (error == ffmpeg.AVERROR_EOF)
                {
                    EndReached = true;
                    packet = default;
                    return false;
                }
                error.ThrowExceptionIfError();
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));
            packet = *_pPacket;
            EndReached = false;
            return true;
        }

        public void Seek(int index, long offset)
        {
            if (!IsValid)
                return;
            // AVRational base_q = ffmpeg.av_get_time_base_q();
            // long target = ffmpeg.av_rescale_q(offset, base_q, _ctx._pFormatContext->streams[_streamIndex]->time_base);
            // ffmpeg.avformat_seek_file(_pFormatContext, -1, long.MinValue, offset, long.MaxValue, ffmpeg.AVSEEK_FLAG_ANY).ThrowExceptionIfError();
            int flags = ffmpeg.AVSEEK_FLAG_BACKWARD;
            ffmpeg.av_seek_frame(_pFormatContext, index, offset, flags).ThrowExceptionIfError();
            // ffmpeg.avformat_seek_file(_pFormatContext, index, 0, offset, offset, flags).ThrowExceptionIfError();
        }

        public void Seek(VideoStreamDecoder decoder, double offset)
        {
            if (!IsValid)
                return;
            int _streamIndex = decoder._streamIndex;
            // if (TryGetPts(out double pts))
            AVRational base_q = ffmpeg.av_get_time_base_q();
            base_q = _pFormatContext->streams[_streamIndex]->time_base;
            double pts = (double)base_q.num / base_q.den;
            long frame = ffmpeg.av_rescale((long)(offset * 1000d), base_q.den, base_q.num);
            frame /= 1000;
            Seek(_streamIndex, Math.Max(0, frame));
        }

        public void Dispose()
        {
            IsValid = false;
            var pPacket = _pPacket;
            ffmpeg.av_packet_free(&pPacket);

            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
            // ffmpeg.avformat_free_context(_pFormatContext);

            var pIOContext = _pIOContext;
            if (pIOContext != null)
                ffmpeg.avio_context_free(&pIOContext);
            if (streamHandle.IsAllocated)
                streamHandle.Free();
            if (bufferPtr != null)
                ffmpeg.av_free(bufferPtr);
        }
    }
}
