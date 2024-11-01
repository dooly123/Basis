using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.Unity.Helpers;
using UnityEngine;
using UnityEngine.Profiling;
namespace FFmpeg.Unity
{
    public class FFUnity : MonoBehaviour
    {
        // Video-related fields
        public bool IsPaused => _paused;
        [SerializeField] public bool _paused;
        private bool _wasPaused = false;
        [SerializeField] public bool CanSeek = true;

        // Time controls
        [SerializeField] public double _offset = 0.0d;
        private double _prevTime = 0.0d;
        private double _timeOffset = 0.0d;
        [SerializeField] public double _videoOffset = -0.0d;
        private Stopwatch _videoWatch;
        private double? _lastPts;
        private int? _lastPts2;
        public double timer;
        public double PlaybackTime => _lastVideoTex?.pts ?? _elapsedOffset;
        public double _elapsedTotalSeconds => _videoWatch?.Elapsed.TotalSeconds ?? 0d;
        public double _elapsedOffsetVideo => _elapsedTotalSeconds + _videoOffset - _timeOffset;
        public double _elapsedOffset => _elapsedTotalSeconds - _timeOffset;
        private double? seekTarget = null;

        // Video buffer controls
        private int _videoBufferCount = 4;
        [SerializeField] public double _videoTimeBuffer = 1d;
        [SerializeField] public double _videoSkipBuffer = 0.25d;

        // Unity assets (video textures)
        private Queue<TexturePool.TexturePoolState> _videoTextures;
        private TexturePool.TexturePoolState _lastVideoTex;
        private TexturePool _texturePool;
        private FFTexData? _lastTexData;
        private MaterialPropertyBlock propertyBlock;
        public Action<Texture2D> OnDisplay = null;

        // Decoders and video processing
        [SerializeField] public AVHWDeviceType _hwType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
        private FFmpegCtx _streamVideoCtx;
        private VideoStreamDecoder _videoDecoder;
        private VideoFrameConverter _videoConverter;
        private Queue<FFTexData> _videoFrameClones;
        private Mutex _videoMutex = new Mutex();
        private Thread _decodeThread;

        // Video frame buffers
        private AVFrame[] _videoFrames;
        private int _videoWriteIndex = 0;
        private double _lastVideoDecodeTime;
        [NonSerialized] public int skippedFrames = 0;
        public double VideoCatchupMultiplier = 5;
        public int LastTotalSize;
        public int synchronizingmaxIterations = 128;
        Stopwatch FillVideoBuffersStopWatch = new Stopwatch();

        // Unity texture generation
        [SerializeField] public FFUnityTextureGeneration unityTextureGeneration = new FFUnityTextureGeneration();

        // Audio processing
        [SerializeField] public FFUnityAudioProcess AudioProcessing = new FFUnityAudioProcess();
        public FFTexDataPool _ffTexDataPool;
        private void OnEnable()
        {
            _ffTexDataPool = new FFTexDataPool();
        _paused = false;
        }
        private void OnDisable()
        {
            _paused = true;
        }
        private void OnDestroy()
        {
            JoinThreads();
            DeInit();
        }
        public void JoinThreads()
        {
            _paused = true;
            _decodeThread?.Join();
        }
        public void DeInit()
        {
            DeInitVideo();
            AudioProcessing.DeInitAudio();
        }
        public void DeInitVideo()
        {
            _texturePool?.Dispose();
            _videoDecoder?.Dispose();
            _streamVideoCtx?.Dispose();
        }
        public void Seek(double seek)
        {
            UnityEngine.Debug.Log(nameof(Seek));
            _paused = true;
            seekTarget = seek;
        }
        // Full seek method divided into audio, video, and other parts
        public void SeekInternal(double seek)
        {
            AudioProcessing.SeekAudio(seek);
            SeekVideo(seek);
            SeekOther(seek);

            seekTarget = null;
            _paused = false;
            StartDecodeThread();  // Assuming you have this method to restart decoding
        }

        // Handles seeking in video
        private void SeekVideo(double seek)
        {
            // Clear video frame clones and textures
            _videoFrameClones.Clear();
            foreach (TexturePool.TexturePoolState tex in _videoTextures)
            {
                _texturePool.Release(tex);
            }
            _videoTextures.Clear();

            // Reset video tracking variables
            _lastVideoTex = null;
            _lastTexData = null;
            _videoWatch.Restart();
            ResetTimers();
            _timeOffset = -seek;
            _prevTime = _offset;
            _lastPts = null;
            _lastPts2 = null;

            // Seek video decoder if seeking is enabled
            if (CanSeek)
            {
                _streamVideoCtx.Seek(_videoDecoder, seek);
            }

            // Explicitly seek video decoder
            _videoDecoder.Seek();
        }

        // Handles other tasks related to seeking (like resetting timers)
        private void SeekOther(double seek)
        {
            // This method is used to reset timers or any non-audio, non-video states
            ResetTimers(); // Assuming you have this method to reset the timing controls
        }
        public async Task PlayAsync(Stream video, Stream audio)
        {
            await Task.Run(() =>
            {
                JoinThreads();
            });
            DeInit();
            // Initialize video context
            _streamVideoCtx = new FFmpegCtx(video);
            AudioProcessing._streamAudioCtx = new FFmpegCtx(audio);
            Init();
        }
        public async Task PlayAsync(string urlV, string urlA)
        {
            await Task.Run(() =>
            {
                JoinThreads();
            });
            DeInit();
            // Initialize video context
            _streamVideoCtx = new FFmpegCtx(urlV);
            AudioProcessing._streamAudioCtx = new FFmpegCtx(urlA);
            Init();
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
            _videoWriteIndex = 0;
            AudioProcessing._audioWriteIndex = 0;
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

            ResetTimers();

            unityTextureGeneration.InitializeTexture();

            InitVideo();
            AudioProcessing.CanSeek = CanSeek;
            AudioProcessing.InitAudio(nameof(this.gameObject));

            UnityEngine.Debug.Log(nameof(PlayAsync));
            Seek(0d);
        }
        private void InitVideo()
        {
            // pre-allocate buffers, prevent the C# GC from using CPU
            _texturePool = new TexturePool(_videoBufferCount);
            _videoTextures = new Queue<TexturePool.TexturePoolState>(_videoBufferCount);
            _lastVideoTex = null;
            _lastTexData = null;
            _videoFrames = new AVFrame[_videoBufferCount];
            _videoFrameClones = new Queue<FFTexData>(_videoBufferCount);
            // init decoders
#if  UNITY_EDITOR
            _videoMutex = new Mutex(false, "Video Mutex");
#else
_videoMutex = new Mutex(false); // Or another synchronization method
#endif
            _videoDecoder = new VideoStreamDecoder(_streamVideoCtx, AVMediaType.AVMEDIA_TYPE_VIDEO, _hwType);
        }
        private void Update()
        {
            if (!IsInitialized())
            {
                return;
            }
            HandleEndOfStream();
            if (HandleSeeking())
            {
                return;
            }
            UpdatePlaybackState();
            if (!_paused)
            {
                StartOrResumeDecodeThread();
                UpdateVideoDisplay();
            }
            _prevTime = _offset;
            _wasPaused = _paused;
        }
        /// <summary>
        /// Checks if the stream and video context are initialized.
        /// </summary>
        private bool IsInitialized()
        {
            if (_videoWatch == null || _streamVideoCtx == null)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Handles the end of the video stream and pauses if necessary.
        /// </summary>
        private void HandleEndOfStream()
        {
            if (CanSeek && IsStreamAtEnd() && !_paused)
            {
                Pause();
            }
        }
        /// <summary>
        /// Determines whether the video and audio streams have reached their end.
        /// </summary>
        /// <returns>True if both streams are at the end, false otherwise.</returns>
        private bool IsStreamAtEnd()
        {
            return _offset >= _streamVideoCtx.GetLength()
                || (_streamVideoCtx.EndReached &&
                    (AudioProcessing._audioDecoder == null || AudioProcessing._streamAudioCtx.EndReached)
                    && _videoTextures.Count == 0
                    && (AudioProcessing._audioDecoder == null || AudioProcessing._audioStreams.Count == 0));
        }
        /// <summary>
        /// Handles seeking by performing an internal seek if necessary.
        /// </summary>
        /// <returns>True if a seek is in progress and has been handled, false otherwise.</returns>
        private bool HandleSeeking()
        {
            if (seekTarget.HasValue && (_decodeThread == null || !_decodeThread.IsAlive))
            {
                SeekInternal(seekTarget.Value);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Updates the playback state, handling pause and resume logic.
        /// </summary>
        private void UpdatePlaybackState()
        {
            if (!_paused)
            {
                _offset = _elapsedOffset;

                if (!_videoWatch.IsRunning)
                {
                    _videoWatch.Start();
                    FFUnityAudioHelper.UnPauseAll(AudioProcessing.AudioOutput);
                }
            }
            else
            {
                if (_videoWatch.IsRunning)
                {
                    _videoWatch.Stop();
                    FFUnityAudioHelper.PauseAll(AudioProcessing.AudioOutput);
                }
            }
        }
        /// <summary>
        /// Starts the decode thread if necessary.
        /// </summary>
        private void StartOrResumeDecodeThread()
        {
            if (_decodeThread == null || !_decodeThread.IsAlive)
            {
                StartDecodeThread();
            }
        }
        /// <summary>
        /// Updates the video display, synchronizing with the audio playback.
        /// </summary>
        private void UpdateVideoDisplay()
        {
            int iterations = 0;

            while (ShouldUpdateVideo() && iterations < synchronizingmaxIterations && !IsPaused)
            {
                iterations++;
                if (_videoMutex.WaitOne(0))
                {
                    UpdateVideoFromClones();
                    _videoMutex.ReleaseMutex();
                }
                Present();
            }
        }
        /// <summary>
        /// Determines whether the video display should be updated.
        /// </summary>
        /// <returns>True if the video display needs updating, false otherwise.</returns>
        private bool ShouldUpdateVideo()
        {
            return Math.Abs(_elapsedOffsetVideo - (PlaybackTime + _videoOffset)) >= 0.25d || _lastVideoTex == null;
        }
        private void UpdateThread()
        {
            UnityEngine.Debug.Log("AV Thread started.");

            double targetFps = GetVideoFps();
            double frameTimeMs = GetFrameTimeMs(targetFps);
            double frameInterval = 1d / targetFps;

            // Continuously update video frames while not paused
            while (!_paused)
            {
                try
                {
                    long elapsedMs = FillVideoBuffers(frameInterval, frameTimeMs);

                    // Calculate the sleep duration, ensuring a minimum of 5ms sleep to avoid tight loops
                    int sleepDurationMs = (int)Math.Max(5, frameTimeMs - elapsedMs);
                    Thread.Sleep(sleepDurationMs);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"Error in video update thread: {e}");
                }
            }

            // Log and finalize thread operation
            UnityEngine.Debug.Log("AV Thread stopped.");
            _videoWatch.Stop();
            _paused = true;
        }
        /// <summary>
        /// Gets the frame rate of the video, defaulting to 30fps if not available.
        /// </summary>
        private double GetVideoFps()
        {
            if (!_streamVideoCtx.TryGetFps(_videoDecoder, out double fps))
            {
                fps = 30d;
            }
            return fps;
        }
        /// <summary>
        /// Calculates the time per frame in milliseconds based on the FPS.
        /// </summary>
        private double GetFrameTimeMs(double fps)
        {
            return (1d / fps) * 1000;
        }
        private void StartDecodeThread()
        {
            _decodeThread = new Thread(() => UpdateThread());
            _decodeThread.Name = $"AV Decode Thread {name}";
            _decodeThread.Start();
        }
        /// <summary>
        /// Generates an Image
        /// </summary>
        /// <returns></returns>
        private bool Present()
        {
            if (!_lastTexData.HasValue)
                return false; // Early exit if no texture data

            _lastVideoTex = new TexturePool.TexturePoolState()
            {
                pts = _lastTexData.Value.time,
            };

            unityTextureGeneration.UpdateTexture(_lastTexData.Value);
            // Invoke the display callback
            OnDisplay?.Invoke(unityTextureGeneration.texture);

            _lastTexData = null; // Clear after processing
            return true;
        }
        private long FillVideoBuffers(double invFps, double fpsMs)
        {
            if (!IsInitialized())
            {
                return 0;
            }
            FillVideoBuffersStopWatch.Restart();

            // Main loop that runs as long as we are within the target frame time
            while (FillVideoBuffersStopWatch.ElapsedMilliseconds <= fpsMs && !IsPaused)
            {
                // Initialize state variables
                double time = default;
                bool decodeV = ShouldDecodeVideo();
                bool decodeA = AudioProcessing._audioDecoder != null && AudioProcessing.ShouldDecodeAudio(this);

                // Process video frames
                if (decodeV && TryProcessVideoFrame(ref time))
                {
                    continue;
                }

                // Process audio frames
                if (decodeA && AudioProcessing.TryProcessAudioFrame(ref time, this))
                {
                    continue;
                }
            }

            return FillVideoBuffersStopWatch.ElapsedMilliseconds;
        }
        /// <summary>
        /// Determines whether the video frame should be decoded.
        /// </summary>
        private bool ShouldDecodeVideo()
        {
            if (_lastVideoTex != null)
            {
                // Adjust the time offset if playback and video are out of sync
                if (Math.Abs(_elapsedOffsetVideo - PlaybackTime) > _videoTimeBuffer * VideoCatchupMultiplier && !CanSeek)
                {
                    _timeOffset = -PlaybackTime;
                }

                // Check if the current video decoder can decode and is in sync
                if (_videoDecoder.CanDecode() && _streamVideoCtx.TryGetTime(_videoDecoder, out var time))
                {
                    if (_elapsedOffsetVideo + _videoTimeBuffer < time) return false;

                    if (_elapsedOffsetVideo > time + _videoSkipBuffer && CanSeek)
                    {
                        _streamVideoCtx.NextFrame(out _);
                        skippedFrames++;
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Processes the video frame, decodes it, and manages the buffer.
        /// </summary>
        private bool TryProcessVideoFrame(ref double time)
        {
            int vid = -1;
            AVFrame vFrame = default;

            // Attempt to decode the next video frame
            _streamVideoCtx.NextFrame(out _);
            vid = _videoDecoder.Decode(out vFrame);

            if (vid == 0)
            {
                if (_streamVideoCtx.TryGetTime(_videoDecoder, vFrame, out time) && _elapsedOffsetVideo > time + _videoSkipBuffer && CanSeek)
                    return false;

                if (_streamVideoCtx.TryGetTime(_videoDecoder, vFrame, out time) && time != 0)
                    _lastVideoDecodeTime = time;

                // Store the video frame in the buffer
                _videoFrames[_videoWriteIndex % _videoFrames.Length] = vFrame;

                EnqueueVideoFrame(vFrame, time);
                _videoWriteIndex++;
                return true;
            }

            return false;
        }
        private void EnqueueVideoFrame(AVFrame vFrame, double time)
        {
            if (_videoMutex.WaitOne(1))
            {
                // Retrieve a frame from the pool
                FFTexData frameData = _ffTexDataPool.Get(vFrame.width, vFrame.height);
                // Clone the frame data
                if (!FFUnityFrameHelper.SaveFrame(vFrame, vFrame.width, vFrame.height, frameData.data, _videoDecoder.HWPixelFormat))
                {
                    UnityEngine.Debug.LogError("Could not save frame");
                    _videoWriteIndex--;
                    _ffTexDataPool.Return(frameData); // Return it to the pool on failure
                }
                else
                {
                    _streamVideoCtx.TryGetTime(_videoDecoder, vFrame, out time);
                    _lastPts = time;

                    frameData.time = time;
                    _videoFrameClones.Enqueue(frameData); // Enqueue the frame data for processing
                }
                _videoMutex.ReleaseMutex();
            }
        }
        private unsafe bool UpdateVideoFromClones()
        {
            Profiler.BeginSample(nameof(UpdateVideoFromClones), this);
            if (_videoFrameClones.Count == 0)
            {
                Profiler.EndSample();
                return false;
            }

            // Dequeue the frame data for updating the video
            FFTexData videoFrame = _videoFrameClones.Dequeue();
            _lastTexData = videoFrame;

            // Once done, return it to the pool
            _ffTexDataPool.Return(videoFrame);

            Profiler.EndSample();
            return true;
        }
        public class FFTexDataPool
        {
            private readonly ConcurrentQueue<FFTexData> _pool = new ConcurrentQueue<FFTexData>();

            private FFTexData CreateNewFFTexData(int FrameWidth, int FrameHeight)
            {
                return new FFTexData
                {
                    data = new byte[FrameWidth * FrameHeight * 3], // Assuming 3 bytes per pixel (RGB)
                     height = FrameHeight,
                      width = FrameWidth,
                };
            }

            public FFTexData Get(int FrameWidth,int FrameHeight)
            {
                if (_pool.TryDequeue(out FFTexData item))
                {
                    int Length = FrameWidth * FrameHeight * 3;
                    if (item.data == null)
                    {
                        item = new FFTexData
                        {
                            data = new byte[Length], // Assuming 3 bytes per pixel (RGB)
                            height = FrameHeight,
                            width = FrameWidth,
                        };
                    }
                    else
                    {
                        if (item.data.Length != Length)
                        {
                            item.data = new byte[Length];
                            item.height = FrameHeight;
                            item.width = FrameWidth;
                        }
                    }
                    return item;
                }
                else
                {
                    // Pool is empty, create a new instance
                    return CreateNewFFTexData(FrameWidth,FrameHeight);
                }
            }

            public void Return(FFTexData item)
            {
                // Reset the reusable data object
                item.time = 0;
                _pool.Enqueue(item);
            }
        }
    }
}