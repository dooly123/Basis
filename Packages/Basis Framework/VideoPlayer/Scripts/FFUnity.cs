using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using FFmpeg.Unity.Helpers;
using UnityEngine;
using UnityEngine.Profiling;
namespace FFmpeg.Unity
{
    public class FFUnity : MonoBehaviour
    {
        public List<FFUnityAudio> AudioOutput = new List<FFUnityAudio>();
        public bool IsPaused => _paused;
        public int materialIndex = -1;
        [SerializeField]
        public bool _paused;
        private bool _wasPaused = false;
        [SerializeField]
        public bool CanSeek = true;
        // time controls
        [SerializeField]
        public double _offset = 0.0d;
        private double _prevTime = 0.0d;
        private double _timeOffset = 0.0d;
        [SerializeField]
        public double _videoOffset = -0.0d;
        private Stopwatch _videoWatch;
        private double? _lastPts;
        private int? _lastPts2;
        public double timer;
        public double PlaybackTime => _lastVideoTex?.pts ?? _elapsedOffset;
        public double _elapsedTotalSeconds => _videoWatch?.Elapsed.TotalSeconds ?? 0d;
        public double _elapsedOffsetVideo => _elapsedTotalSeconds + _videoOffset - _timeOffset;
        public double _elapsedOffset => _elapsedTotalSeconds - _timeOffset;
        private double? seekTarget = null;
        // buffer controls
        private int _videoBufferCount = 4;
        private int _audioBufferCount = 1;
        [SerializeField]
        public double _videoTimeBuffer = 1d;
        [SerializeField]
        public double _videoSkipBuffer = 0.25d;
        [SerializeField]
        public double _audioTimeBuffer = 1d;
        [SerializeField]
        public double _audioSkipBuffer = 0.25d;
        private int _audioBufferSize = 128;
        // unity assets
        private Queue<TexturePool.TexturePoolState> _videoTextures;
        private TexturePool.TexturePoolState _lastVideoTex;
        private TexturePool _texturePool;
        private FFTexData? _lastTexData;
        private Texture2D image;
        private AudioClip _audioClip;
        private MaterialPropertyBlock propertyBlock;
        public Action<Texture2D> OnDisplay = null;
        // decoders
        [SerializeField]
        public AVHWDeviceType _hwType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
        private FFmpegCtx _streamVideoCtx;
        private FFmpegCtx _streamAudioCtx;
        private VideoStreamDecoder _videoDecoder;
        private VideoStreamDecoder _audioDecoder;
        private VideoFrameConverter _videoConverter;
        private Queue<FFTexData> _videoFrameClones;
        private Mutex _videoMutex = new Mutex();
        private Thread _decodeThread;
        private Mutex _audioLocker = new Mutex();
        private Queue<float> _audioStream;
        private MemoryStream _audioMemStream;
        // buffers
        private AVFrame[] _videoFrames;
        private AVFrame[] _audioFrames;
        private int _videoDisplayIndex = 0;
        private int _audioDisplayIndex = 0;
        private int _videoWriteIndex = 0;
        private int _audioWriteIndex = 0;
        // logging
        public Action<object> Log = UnityEngine.Debug.Log;
        public Action<object> LogWarning = UnityEngine.Debug.LogWarning;
        public Action<object> LogError = UnityEngine.Debug.LogError;
        private bool _lastAudioRead = false;
        private int _audioMissCount = 0;
        private double _lastVideoDecodeTime;
        private double _lastAudioDecodeTime;
        [NonSerialized]
        public int skippedFrames = 0;
        public double VideoCatchupMultiplier = 5;
        public float[] vals;
        public int LastTotalSize;
        private void OnEnable()
        {
            _paused = true;
        }
        private void OnDisable()
        {
            _paused = true;
        }
        private void OnDestroy()
        {
            _paused = true;
            _decodeThread?.Abort();
            _decodeThread?.Join();
            _texturePool?.Dispose();
            _videoDecoder?.Dispose();
            _audioDecoder?.Dispose();
            _streamVideoCtx?.Dispose();
            _streamAudioCtx?.Dispose();
        }
        public void Seek(double seek)
        {
            Log(nameof(Seek));
            _paused = true;
            seekTarget = seek;
        }
        public void SeekInternal(double seek)
        {
            FFUnityAudioHelper.StopAll(AudioOutput);

            if (_audioLocker.WaitOne())
            {
                _audioMemStream.Position = 0;
                _audioStream.Clear();
                _audioLocker.ReleaseMutex();
            }
            _videoFrameClones.Clear();
            foreach (TexturePool.TexturePoolState tex in _videoTextures)
            {
                _texturePool.Release(tex);
            }
            _videoTextures.Clear();
            _lastVideoTex = null;
            _lastTexData = null;
            _videoWatch.Restart();
            ResetTimers();
            _timeOffset = -seek;
            _prevTime = _offset;
            _lastPts = null;
            _lastPts2 = null;
            if (CanSeek)
            {
                _streamVideoCtx.Seek(_videoDecoder, seek);
                _streamAudioCtx.Seek(_audioDecoder, seek);
            }
            _videoDecoder.Seek();
            _audioDecoder?.Seek();
            FFUnityAudioHelper.PlayAll(AudioOutput, _audioClip);
            seekTarget = null;
            _paused = false;
            StartDecodeThread();
        }
        public void Play(Stream video, Stream audio)
        {
            DeInit();
            _streamVideoCtx = new FFmpegCtx(video);
            _streamAudioCtx = new FFmpegCtx(audio);
            Init();
        }
        public void Play(string urlV, string urlA)
        {
            DeInit();
            _streamVideoCtx = new FFmpegCtx(urlV);
            _streamAudioCtx = new FFmpegCtx(urlA);
            Init();
        }
        public void DeInit()
        {
            _paused = true;
            _decodeThread?.Abort();
            _decodeThread?.Join();
            _texturePool?.Dispose();
            _videoDecoder?.Dispose();
            _audioDecoder?.Dispose();
            _streamVideoCtx?.Dispose();
            _streamAudioCtx?.Dispose();
        }
        public void Resume()
        {
            if (!CanSeek)
            {
                Init();
            }
            _paused = false;
        }
        public void Pause()
        {
            _paused = true;
        }
        private void ResetTimers()
        {
            // reset index counters and timers
            _videoDisplayIndex = 0;
            _audioDisplayIndex = 0;
            _videoWriteIndex = 0;
            _audioWriteIndex = 0;
            _lastPts = null;
            _lastPts2 = null;
            _offset = 0d;
            _prevTime = 0d;
            _timeOffset = 0d;
            timer = 0d;
        }
        private void Init()
        {
            _paused = true;

            // Stopwatches are more accurate than Time.timeAsDouble(?)
            _videoWatch = new Stopwatch();
            // pre-allocate buffers, prevent the C# GC from using CPU
            _texturePool = new TexturePool(_videoBufferCount);
            _videoTextures = new Queue<TexturePool.TexturePoolState>(_videoBufferCount);
            _audioClip = null; // don't create audio clip yet, we have nothing to play.
            _videoFrames = new AVFrame[_videoBufferCount];
            _videoFrameClones = new Queue<FFTexData>(_videoBufferCount);
            _audioFrames = new AVFrame[_audioBufferCount];
            _audioMemStream = new MemoryStream();
            _audioStream = new Queue<float>(_audioBufferSize * 4);

            ResetTimers();
            _lastVideoTex = null;
            _lastTexData = null;

            // init decoders
            _videoMutex = new Mutex(false, "Video Mutex");
            _videoDecoder = new VideoStreamDecoder(_streamVideoCtx, AVMediaType.AVMEDIA_TYPE_VIDEO, _hwType);
            _audioLocker = new Mutex(false, "Audio Mutex");
            _audioDecoder = new VideoStreamDecoder(_streamAudioCtx, AVMediaType.AVMEDIA_TYPE_AUDIO);
            _audioClip = AudioClip.Create($"{name}-AudioClip", _audioBufferSize * _audioDecoder.Channels, _audioDecoder.Channels, _audioDecoder.SampleRate, true, AudioCallback);
            FFUnityAudioHelper.PlayAll(AudioOutput, _audioClip);
            Log(nameof(Play));
            Seek(0d);
        }
        private void Update()
        {
            if (_videoWatch == null || _streamVideoCtx == null)
            {
                return;
            }

            if (CanSeek && (_offset >= _streamVideoCtx.GetLength() || (_streamVideoCtx.EndReached && (_audioDecoder == null || _streamAudioCtx.EndReached) && _videoTextures.Count == 0 && (_audioDecoder == null || _audioStream.Count == 0))) && !_paused)
            {
                Pause();
            }
            if (seekTarget.HasValue && (_decodeThread == null || !_decodeThread.IsAlive))
            {
                SeekInternal(seekTarget.Value);
            }

            if (!_paused)
            {
                _offset = _elapsedOffset;
                if (!_videoWatch.IsRunning)
                {
                    _videoWatch.Start();
                    FFUnityAudioHelper.UnPauseAll(AudioOutput);
                }
            }
            else
            {
                if (_videoWatch.IsRunning)
                {
                    _videoWatch.Stop();
                    FFUnityAudioHelper.PauseAll(AudioOutput);
                }
            }

            if (!_paused)
            {
                if (_decodeThread == null || !_decodeThread.IsAlive)
                    StartDecodeThread();

                int idx = _videoDisplayIndex;
                int j = 0;
                while (Math.Abs(_elapsedOffsetVideo - (PlaybackTime + _videoOffset)) >= 0.25d || _lastVideoTex == null)
                {
                    j++;
                    if (j >= 128)
                        break;
                    if (_videoMutex.WaitOne())
                    {
                        bool failed = !UpdateVideoFromClones(idx);
                        _videoMutex.ReleaseMutex();
                        // if (failed)
                        //     break;
                    }
                    Present(idx, true);
                }
            }
            _prevTime = _offset;
            _wasPaused = _paused;
        }
        private void Update_Thread()
        {
            Log("AV Thread started.");
            double fps;
            if (!_streamVideoCtx.TryGetFps(_videoDecoder, out fps))
                fps = 30d;
            double fpsMs = 1d / fps * 1000;
            fps = 1d / fps;
            while (!_paused)
            {
                try
                {
                    long ms = FillVideoBuffers(false, fps, fpsMs);
                    Thread.Sleep((int)Math.Max(5, fpsMs - ms));
                }
                catch (Exception e)
                {
                    LogError(e);
                }
            }
            Log("AV Thread stopped.");
            _videoWatch.Stop();
            _paused = true;
        }
        private void StartDecodeThread()
        {
            _decodeThread = new Thread(() => Update_Thread());
            _decodeThread.Name = $"AV Decode Thread {name}";
            _decodeThread.Start();
        }
        private bool Present(int idx, bool display)
        {
            if (_lastTexData.HasValue)
            {
                _lastVideoTex = new TexturePool.TexturePoolState()
                {
                    pts = _lastTexData.Value.time,
                };
                if (display)
                {
                    if (image == null)
                    {
                        image = new Texture2D(16, 16, TextureFormat.RGB24, false);
                    }
                    if (image.width != _lastTexData.Value.w || image.height != _lastTexData.Value.h)
                    {
                        image.Reinitialize(_lastTexData.Value.w, _lastTexData.Value.h);
                    }
                    image.SetPixelData(_lastTexData.Value.data, 0);
                    image.Apply(false);

                    OnDisplay?.Invoke(image);
                }
                _lastTexData = null;
                return true;
            }
            return false;
        }
        Stopwatch FillVideoBuffersStopWatch = new Stopwatch();
        private long FillVideoBuffers(bool mainThread, double invFps, double fpsMs)
        {
            if (_streamVideoCtx == null || _streamAudioCtx == null)
            {
                return 0;
            }
            FillVideoBuffersStopWatch.Restart();
            while (FillVideoBuffersStopWatch.ElapsedMilliseconds <= fpsMs)
            {
                double time = default;
                bool decodeV = true;
                bool decodeA = _audioDecoder != null;
                if (_lastVideoTex != null)
                {
                    if (Math.Abs(_elapsedOffsetVideo - PlaybackTime) > _videoTimeBuffer * VideoCatchupMultiplier && !CanSeek)
                    {
                        _timeOffset = -PlaybackTime;
                    }
                }
                if (_lastVideoTex != null && _videoDecoder.CanDecode() && _streamVideoCtx.TryGetTime(_videoDecoder, out time))
                {
                    if (_elapsedOffsetVideo + _videoTimeBuffer < time)
                        decodeV = false;
                    if (_elapsedOffsetVideo > time + _videoSkipBuffer && CanSeek)
                    {
                        _streamVideoCtx.NextFrame(out _);
                        skippedFrames++;
                        decodeV = false;
                    }
                }
                if (_lastVideoTex != null && _audioDecoder != null && _audioDecoder.CanDecode() && _streamAudioCtx.TryGetTime(_audioDecoder, out time))
                {
                    if (_elapsedOffset + _audioTimeBuffer < time)
                        decodeA = false;
                    if (_elapsedOffset > time + _audioSkipBuffer && CanSeek)
                    {
                        _streamAudioCtx.NextFrame(out _);
                        skippedFrames++;
                        decodeA = false;
                    }
                }
                {
                    int vid = -1;
                    int aud = -1;
                    AVFrame vFrame = default;
                    AVFrame aFrame = default;
                    if (decodeV)
                    {
                        _streamVideoCtx.NextFrame(out _);
                        vid = _videoDecoder.Decode(out vFrame);
                    }
                    if (decodeA)
                    {
                        _streamAudioCtx.NextFrame(out _);
                        aud = _audioDecoder.Decode(out aFrame);
                    }
                    switch (vid)
                    {
                        case 0:
                            if (_streamVideoCtx.TryGetTime(_videoDecoder, vFrame, out time) && _elapsedOffsetVideo > time + _videoSkipBuffer && CanSeek)
                                break;
                            if (_streamVideoCtx.TryGetTime(_videoDecoder, vFrame, out time) && time != 0)
                                _lastVideoDecodeTime = time;
                            _videoFrames[_videoWriteIndex % _videoFrames.Length] = vFrame;
                            if (mainThread)
                            {
                                UpdateVideo(_videoWriteIndex % _videoFrames.Length);
                            }
                            else
                            {
                                {
                                    if (_videoMutex.WaitOne(1))
                                    {
                                        byte[] frameClone = new byte[vFrame.width * vFrame.height * 3];
                                        if (!FFUnityFrameHelper.SaveFrame(vFrame, vFrame.width, vFrame.height, frameClone, _videoDecoder.HWPixelFormat))
                                        {
                                            LogError("Could not save frame");
                                            _videoWriteIndex--;
                                        }
                                        else
                                        {
                                            _streamVideoCtx.TryGetTime(_videoDecoder, vFrame, out time);
                                            _lastPts = time;
                                            _videoFrameClones.Enqueue(new FFTexData()
                                            {
                                                time = time,
                                                data = frameClone,
                                                w = vFrame.width,
                                                h = vFrame.height,
                                            });
                                        }
                                        _videoMutex.ReleaseMutex();
                                    }
                                }
                            }
                            _videoWriteIndex++;
                            break;
                        case 1:
                            break;
                    }
                    switch (aud)
                    {
                        case 0:
                            if (_streamAudioCtx.TryGetTime(_audioDecoder, aFrame, out time) && _elapsedOffset > time + _audioSkipBuffer && CanSeek)
                                break;
                            if (_streamAudioCtx.TryGetTime(_audioDecoder, aFrame, out time) && time != 0)
                                _lastAudioDecodeTime = time;
                            _audioFrames[_audioWriteIndex % _audioFrames.Length] = aFrame;
                            UpdateAudio(_audioWriteIndex % _audioFrames.Length);
                            _audioWriteIndex++;
                            break;
                        case 1:
                            break;
                    }
                }
            }
            return FillVideoBuffersStopWatch.ElapsedMilliseconds;
        }
        private unsafe Texture2D UpdateVideo(int idx)
        {
            Profiler.BeginSample(nameof(UpdateVideo), this);
            AVFrame videoFrame;
            videoFrame = _videoFrames[idx];
            if (videoFrame.data[0] == null)
            {
                Profiler.EndSample();
                return null;
            }
            var tex = _texturePool.Get();
            if (tex.texture == null)
            {
                tex.texture = FFUnityFrameHelper.SaveFrame(videoFrame, videoFrame.width, videoFrame.height, _videoDecoder.HWPixelFormat);
            }
            else
            {
                FFUnityFrameHelper.SaveFrame(videoFrame, videoFrame.width, videoFrame.height, tex.texture, _videoDecoder.HWPixelFormat);
            }
            tex.texture.name = $"{name}-Texture2D-{idx}";
            _videoTextures.Enqueue(tex);
            Profiler.EndSample();
            return tex.texture;
        }
        private unsafe bool UpdateVideoFromClones(int idx)
        {
            Profiler.BeginSample(nameof(UpdateVideoFromClones), this);
            if (_videoFrameClones.Count == 0)
            {
                Profiler.EndSample();
                return false;
            }
            FFTexData videoFrame = _videoFrameClones.Dequeue();
            _lastTexData = videoFrame;
            Profiler.EndSample();
            return true;
        }
        private unsafe void AudioCallback(float[] data)
        {
            if (_audioLocker.WaitOne(0))
            {
                if (_audioStream.Count < data.Length)
                {
                    _lastAudioRead = false;
                }
                for (int i = 0; i < data.Length; i++)
                {
                    if (_audioStream.Count > 0)
                        data[i] = _audioStream.Dequeue();
                    else
                        data[i] = 0;
                }
                _audioLocker.ReleaseMutex();
            }
        }
        private unsafe void UpdateAudio(int idx)
        {
            Profiler.BeginSample(nameof(UpdateAudio), this);
            var audioFrame = _audioFrames[idx];
            if (audioFrame.data[0] == null)
            {
                Profiler.EndSample();
                return;
            }
            List<float> vals = new List<float>();
            for (uint ch = 0; ch < _audioDecoder.Channels; ch++)
            {
                int size = ffmpeg.av_samples_get_buffer_size(null, 1, audioFrame.nb_samples, _audioDecoder.SampleFormat, 1);
                if (size < 0)
                {
                    LogError("audio buffer size is less than zero");
                    Profiler.EndSample();
                    return;
                }
                byte[] backBuffer2 = new byte[size];
                float[] backBuffer3 = new float[size / sizeof(float)];
                Marshal.Copy((IntPtr)audioFrame.data[ch], backBuffer2, 0, size);
                Buffer.BlockCopy(backBuffer2, 0, backBuffer3, 0, backBuffer2.Length);
                for (int i = 0; i < backBuffer3.Length; i++)
                {
                    vals.Add(backBuffer3[i]);
                }
            }
            if (_audioLocker.WaitOne(0))
            {
                int c = vals.Count / _audioDecoder.Channels;
                for (int valueIndex = 0; valueIndex < c; valueIndex++)
                {
                    for (int ChannelIndex = 0; ChannelIndex < _audioDecoder.Channels; ChannelIndex++)
                    {
                        _audioStream.Enqueue(vals[valueIndex + c * ChannelIndex]);
                    }
                }
                _audioLocker.ReleaseMutex();
            }
            Profiler.EndSample();
        }
    }
    }