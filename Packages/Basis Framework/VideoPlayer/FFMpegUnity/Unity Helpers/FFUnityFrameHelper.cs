using System.Runtime.InteropServices;
using System;
using UnityEngine;
using Unity.Profiling;
using System.Diagnostics;

public static class FFUnityFrameHelper
{
    [ThreadStatic]
    private static byte[] line;
    // Categories
    public const string FrameGenerationCategory = "FrameGeneration";

    // Profiler counter names
    public const string SaveFrameTexture2DGeneration = "Save Frame Texture2D Generation";
    public const string SaveFrameByteArray = "Save Frame Byte Array";
    public const string SaveMain = "Save Main";
    public const string ConvertFrame = "Convert Frame";
    public const string LoadTexture = "Load Texture";

    public static readonly ProfilerCategory Category = new ProfilerCategory("FrameGeneration");

    // Define Profiler Counters
    public static readonly ProfilerCounter<double> pSaveFrameTexture2DGeneration = new ProfilerCounter<double>(Category, "Save Frame Texture2D Generation", ProfilerMarkerDataUnit.TimeNanoseconds);
    public static readonly ProfilerCounter<double> pSaveFrameByteArray = new ProfilerCounter<double>(Category, "Save Frame Byte Array", ProfilerMarkerDataUnit.TimeNanoseconds);
    public static readonly ProfilerCounter<double> pSaveMain = new ProfilerCounter<double>(Category, "Save Main", ProfilerMarkerDataUnit.TimeNanoseconds);
    public static readonly ProfilerCounter<double> pConvertFrame = new ProfilerCounter<double>(Category, "Convert Frame", ProfilerMarkerDataUnit.TimeNanoseconds);
    public static readonly ProfilerCounter<double> pLoadTexture = new ProfilerCounter<double>(Category, "Load Texture", ProfilerMarkerDataUnit.TimeNanoseconds);

    public static Stopwatch SaveFrameStopWatch = new Stopwatch();

    public static Stopwatch SaveFrameByteStopWatch = new Stopwatch();

    public static Stopwatch SaveFrameMainStopWatch = new Stopwatch();
    public static Texture2D SaveFrame(AVFrame frame, int width, int height, AVPixelFormat format)
    {
        SaveFrameStopWatch.Start();
        ///take a snapshot of time to do this and then submit it to the profiler 
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        SaveFrame(frame, width, height, texture, format);
        SaveFrameStopWatch.Stop();
        pSaveFrameTexture2DGeneration.Sample(SaveFrameStopWatch.Elapsed.TotalMilliseconds * 1000000);
        return texture;
    }

    public unsafe static bool SaveFrame(AVFrame frame, int width, int height, byte[] texture, AVPixelFormat format)
    {
        SaveFrameByteStopWatch.Start();
        if (line == null)
        {
            line = new byte[4096 * 4096 * 6];
        }

        if (frame.data[0] == null || frame.format == -1 || texture == null)
        {
            return false;
        }

        using var converter = new VideoFrameConverter(new System.Drawing.Size(frame.width, frame.height), (AVPixelFormat)frame.format, new System.Drawing.Size(width, height), AVPixelFormat.AV_PIX_FMT_RGB24);
        var convFrame = converter.Convert(frame);
        Marshal.Copy((IntPtr)convFrame.data[0], line, 0, width * height * 3);
        Array.Copy(line, 0, texture, 0, width * height * 3);
        SaveFrameByteStopWatch.Stop();
        pSaveFrameByteArray.Sample(SaveFrameByteStopWatch.Elapsed.TotalMilliseconds * 1000000);
        return true;
    }

    public unsafe static void SaveFrame(AVFrame frame, int width, int height, Texture2D texture, AVPixelFormat format)
    {
        if (line == null)
        {
            line = new byte[4096 * 4096 * 6];
        }
        if (frame.data[0] == null || frame.format == -1)
        {
            return;
        }

        using var converter = new VideoFrameConverter(new System.Drawing.Size(width, height), (AVPixelFormat)frame.format, new System.Drawing.Size(width, height), AVPixelFormat.AV_PIX_FMT_RGB24);
        var convFrame = converter.Convert(frame);
        if (texture.width != width || texture.height != height)
        {
            texture.Reinitialize(width, height);
        }

        Marshal.Copy((IntPtr)frame.data[0], line, 0, width * height * 3);
        texture.LoadRawTextureData((IntPtr)frame.data[0], width * height * 3);
        texture.Apply(false);
    }
}