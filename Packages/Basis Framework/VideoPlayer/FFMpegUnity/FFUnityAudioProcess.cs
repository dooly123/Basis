using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using FFmpeg.Unity.Helpers;
using UnityEngine;
namespace FFmpeg.Unity
{
    [System.Serializable]
    public class FFUnityAudioProcess
    {
        public List<FFUnityAudio> AudioOutput = new List<FFUnityAudio>();
        public bool IsPaused => _paused;
        [SerializeField] public bool _paused;
        private bool _wasPaused = false;
        [SerializeField] public bool CanSeek = true;
        [SerializeField] public double _audioTimeBuffer = 1d;
        [SerializeField] public double _audioSkipBuffer = 0.25d;
        public int _audioBufferCount = 1;
        public int _audioBufferSize = 128;

        // Circular buffer for each channel's audio stream
        public List<Queue<float>> _audioStreams;

        public MemoryStream _audioMemStream;
        public Mutex _audioLocker = new Mutex();
        public int _audioWriteIndex = 0;
        public bool _lastAudioRead = false;
        public int _audioMissCount = 0;
        public double _lastAudioDecodeTime;
        public VideoStreamDecoder _audioDecoder;
        public FFmpegCtx _streamAudioCtx;
        public AudioClip[] _audioClips; // One for each channel
        public AVFrame[] _audioFrames;

        public bool HasAnyAudioLeft()
        {
            foreach (Queue<float> buffer in _audioStreams)
            {
                if (buffer.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void SeekAudio(double seek)
        {
            // Stop all audio output
            FFUnityAudioHelper.StopAll(AudioOutput);

            // Lock audio and reset stream
            if (_audioLocker.WaitOne())
            {
                _audioMemStream.Position = 0;
                foreach (var stream in _audioStreams)
                {
                    stream.Clear();
                }
                _audioLocker.ReleaseMutex();
            }

            if (CanSeek)
            {
                _streamAudioCtx.Seek(_audioDecoder, seek);
            }

            _audioDecoder?.Seek();

            for (int channelIndex = 0; channelIndex < _audioClips.Length; channelIndex++)
            {
                FFUnityAudioHelper.PlayAll(AudioOutput, channelIndex, _audioClips[channelIndex]);
            }
        }

        public void DeInitAudio()
        {
            _audioDecoder?.Dispose();
            _streamAudioCtx?.Dispose();
        }

        public void InitAudio(string name)
        {
            _audioFrames = new AVFrame[_audioBufferCount];
            _audioMemStream = new MemoryStream();
            _audioStreams = new List<Queue<float>>();

            _audioLocker = new Mutex(false, "Audio Mutex");
            _audioDecoder = new VideoStreamDecoder(_streamAudioCtx, AVMediaType.AVMEDIA_TYPE_AUDIO);

            _audioClips = new AudioClip[_audioDecoder.Channels];
            for (int channel = 0; channel < _audioDecoder.Channels; channel++)
            {
                _audioStreams.Add(new Queue<float>(_audioBufferSize * 4));
                var duplicate = channel;
                _audioClips[channel] = AudioClip.Create($"{name}-AudioClip-{channel}", _audioBufferSize, 1, _audioDecoder.SampleRate, true, (data) => AudioCallback(data, duplicate));

                FFUnityAudioHelper.PlayAll(AudioOutput, channel, _audioClips[channel]);
            }
        }

        public unsafe void AudioCallback(float[] data, int channel)
        {
            var audioStream = _audioStreams[channel];
            int availableSamples = audioStream.Count;

            _lastAudioRead = availableSamples >= data.Length;

            int samplesToDequeue = Math.Min(availableSamples, data.Length);

            if (_audioLocker.WaitOne(0))
            {
                try
                {
                    for (int sampleIndex = 0; sampleIndex < samplesToDequeue; sampleIndex++)
                    {
                        if (audioStream.TryDequeue(out float sample))
                        {
                            data[sampleIndex] = sample;
                        }
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"AudioCallback error: {ex.Message}");
                }
                finally
                {
                    _audioLocker.ReleaseMutex();
                }
            }
            if (samplesToDequeue < data.Length)
            {
                Array.Clear(data, samplesToDequeue, data.Length - samplesToDequeue);
            }
        }

        public unsafe void UpdateAudio(int idx)
        {
            var audioFrame = _audioFrames[idx];
            if (audioFrame.data[0] == null)
            {
                return;
            }

            for (uint ch = 0; ch < _audioDecoder.Channels; ch++)
            {
                int size = ffmpeg.av_samples_get_buffer_size(null, 1, audioFrame.nb_samples, _audioDecoder.SampleFormat, 1);
                if (size < 0)
                {
                    UnityEngine.Debug.LogError("audio buffer size is less than zero");
                    return;
                }
                byte[] backBuffer2 = new byte[size];
                float[] backBuffer3 = new float[size / sizeof(float)];
                Marshal.Copy((IntPtr)audioFrame.data[ch], backBuffer2, 0, size);
                Buffer.BlockCopy(backBuffer2, 0, backBuffer3, 0, backBuffer2.Length);
                if (_audioLocker.WaitOne(0))
                {
                    foreach (float sample in backBuffer3)
                    {
                        _audioStreams[(int)ch].Enqueue(sample);
                    }
                    _audioLocker.ReleaseMutex();
                }
            }
        }

        public bool ShouldDecodeAudio(FFUnity Unity)
        {
            if (_audioDecoder != null && _audioDecoder.CanDecode() && _streamAudioCtx.TryGetTime(_audioDecoder, out var time))
            {
                if (Unity._elapsedOffset + _audioTimeBuffer < time) return false;

                if (Unity._elapsedOffset > time + _audioSkipBuffer && CanSeek)
                {
                    _streamAudioCtx.NextFrame(out _);
                    Unity.skippedFrames++;
                    return false;
                }
            }
            return true;
        }

        public bool TryProcessAudioFrame(ref double time, FFUnity Unity)
        {
            int aud = -1;
            AVFrame aFrame = default;

            _streamAudioCtx.NextFrame(out _);
            aud = _audioDecoder.Decode(out aFrame);

            if (aud == 0)
            {
                if (_streamAudioCtx.TryGetTime(_audioDecoder, aFrame, out time) && Unity._elapsedOffset > time + _audioSkipBuffer && CanSeek)
                    return false;

                if (_streamAudioCtx.TryGetTime(_audioDecoder, aFrame, out time) && time != 0)
                    _lastAudioDecodeTime = time;

                _audioFrames[_audioWriteIndex % _audioFrames.Length] = aFrame;
                UpdateAudio(_audioWriteIndex % _audioFrames.Length);

                _audioWriteIndex++;
                return true;
            }

            return false;
        }
    }
}