using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;
// using FFmpeg.AutoGen.Abstractions;

// namespace FFmpeg.AutoGen.Example;

public sealed unsafe class VideoFrameConverter : IDisposable
{
    private readonly Size _destinationSize;
    private readonly AVPixelFormat _destinationPixelFormat;
    private readonly SwsContext* _pConvertContext;
    private readonly List<byte_ptr4> ptrs = new List<byte_ptr4>();
    private readonly List<IntPtr> ptrs2 = new List<IntPtr>();

    public VideoFrameConverter(Size sourceSize, AVPixelFormat sourcePixelFormat,
        Size destinationSize, AVPixelFormat destinationPixelFormat)
    {
        _destinationSize = destinationSize;
        _destinationPixelFormat = destinationPixelFormat;

        _pConvertContext = ffmpeg.sws_getContext(sourceSize.Width,
            sourceSize.Height,
            sourcePixelFormat,
            destinationSize.Width,
            destinationSize.Height,
            destinationPixelFormat,
            // ffmpeg.SWS_POINT,
            ffmpeg.SWS_FAST_BILINEAR,
            null,
            null,
            null);
        if (_pConvertContext == null)
            throw new ApplicationException("Could not initialize the conversion context.");
    }

    public void Dispose()
    {
        foreach (var _dstData in ptrs)
            fixed (void* p = &_dstData.ToArray()[0])
                ffmpeg.av_freep(p);
        foreach (var _dstData in ptrs2)
            Marshal.FreeHGlobal(_dstData);
        ffmpeg.sws_freeContext(_pConvertContext);
    }

    public AVFrame Convert(AVFrame sourceFrame, int align = -1)
    {
        int j = 1;
        if (align < 0)
        {
            for (uint i = 1; i <= 64; i*=2)
            {
                if (Mathf.Abs(sourceFrame.linesize[0*i]) % i == 0 &&
                Mathf.Abs(sourceFrame.linesize[1*i]) % i == 0 &&
                Mathf.Abs(sourceFrame.linesize[2*i]) % i == 0)
                {
                }
                else
                {
                    j = (int)i;
                    break;
                }
            }
        }
        else
        {
            j = align;
        }
        byte_ptr4 _dstData = new byte_ptr4();
        int4 _dstLinesize = new int4();
        ffmpeg.av_image_alloc(ref _dstData, ref _dstLinesize, _destinationSize.Width, _destinationSize.Height, _destinationPixelFormat, j).ThrowExceptionIfError();
        _dstLinesize[0] = _destinationSize.Width * 3;
        _dstLinesize[1] = 0;
        _dstLinesize[2] = 0;
        _dstLinesize[3] = 0;
        int ret = ffmpeg.sws_scale(_pConvertContext,
            sourceFrame.data,
            sourceFrame.linesize,
            0,
            sourceFrame.height,
            _dstData,
            _dstLinesize);
        if (ret < 0)
        {
            fixed (void* p = &_dstData.ToArray()[0])
            {
                ffmpeg.av_freep(p);
            }
            ret.ThrowExceptionIfError();
            throw new ApplicationException();
        }

        var data = new byte_ptr8();
        data.UpdateFrom(_dstData);
        var linesize = new int8();
        linesize.UpdateFrom(_dstLinesize);

        ptrs.Add(_dstData);

        // fixed (void* p = &_dstData.ToArray()[0])
        {
            // ffmpeg.av_freep(p);
        }

        return new AVFrame
        {
            data = data,
            linesize = linesize,
            width = _destinationSize.Width,
            height = _destinationSize.Height,
        };
    }
}
