using UnityEngine;
using System;
using System.Linq;
using Basis.Scripts.Device_Management;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

public class MicrophoneRecorder : MicrophoneRecorderBase
{
    private int head = 0;
    private int bufferLength;
    public bool HasEvents = false;
    public int PacketSize;
    public bool UseDenoiser = false;
    public static Action<bool> OnPausedAction;
    private bool MicrophoneIsStarted = false;
    private Thread processingThread;
    public bool isRunning = true;
    private ManualResetEvent processingEvent = new ManualResetEvent(false);
    private object processingLock = new object();
    private int position;
    private NativeArray<float> PBA;
    private LogarithmicVolumeAdjustmentJob VAJ;
    private JobHandle handle;
    public bool TryInitialize()
    {
        if (!IsInitialize)
        {
            Initialize();
            IsInitialize = true;
            return true;
        }
        return false;
    }
    public void Initialize()
    {
        BasisOpusSettings = BasisDeviceManagement.Instance.BasisOpusSettings;
        processBufferArray = BasisOpusSettings.CalculateProcessBuffer();
        PBA = new NativeArray<float>(processBufferArray, Allocator.Persistent);
        VAJ = new LogarithmicVolumeAdjustmentJob
        {
            processBufferArray = PBA,
            Volume = Volume
        };
        ProcessBufferLength = processBufferArray.Length;
        samplingFrequency = BasisOpusSettings.GetSampleFreq();
        microphoneBufferArray = new float[BasisOpusSettings.RecordingFullLength * samplingFrequency];
        rmsValues = new float[rmsWindowSize];
        bufferLength = microphoneBufferArray.Length;
        PacketSize = ProcessBufferLength * 4;
        if (!HasEvents)
        {
            SMDMicrophone.OnMicrophoneChanged += ResetMicrophones;
            SMDMicrophone.OnMicrophoneVolumeChanged += ChangeAudio;
            SMDMicrophone.OnMicrophoneUseDenoiserChanged += ConfigureDenoiser;
            BasisDeviceManagement.Instance.OnBootModeChanged += OnBootModeChanged;
            HasEvents = true;
        }
        ChangeAudio(SMDMicrophone.SelectedVolumeMicrophone);
        ResetMicrophones(SMDMicrophone.SelectedMicrophone);
        ConfigureDenoiser(SMDMicrophone.SelectedDenoiserMicrophone);

        StartProcessingThread();  // Start the processing thread once
    }

    private void ConfigureDenoiser(bool useDenoiser)
    {
        UseDenoiser = useDenoiser;
        BasisDebug.Log("Setting Denoiser To " + UseDenoiser);
    }

    public new void OnDestroy()
    {
        StopProcessingThread();  // Stop the processing thread
        if (HasEvents)
        {
            SMDMicrophone.OnMicrophoneChanged -= ResetMicrophones;
            SMDMicrophone.OnMicrophoneVolumeChanged -= ChangeAudio;
            SMDMicrophone.OnMicrophoneUseDenoiserChanged -= ConfigureDenoiser;
            BasisDeviceManagement.Instance.OnBootModeChanged -= OnBootModeChanged;

            HasEvents = false;
        }
        // Dispose the NativeArray when done to avoid memory leaks
        if (VAJ.processBufferArray.IsCreated)
        {
            VAJ.processBufferArray.Dispose();
        }
        base.OnDestroy();
    }

    private void OnBootModeChanged(string mode)
    {
        ResetMicrophones(SMDMicrophone.SelectedMicrophone);
    }

    public void ResetMicrophones(string newMicrophone)
    {
        if (string.IsNullOrEmpty(newMicrophone))
        {
            BasisDebug.LogError("Microphone was empty or null");
            return;
        }
        if (Microphone.devices.Length == 0)
        {
            BasisDebug.LogError("No Microphones found!");
            return;
        }
        if (!Microphone.devices.Contains(newMicrophone))
        {
            BasisDebug.LogError("Microphone " + newMicrophone + " not found!");
            return;
        }
        bool isRecording = Microphone.IsRecording(newMicrophone);
        BasisDebug.Log(isRecording ? $"Is Recording {MicrophoneDevice}" : $"Is not Recording {MicrophoneDevice}");
        if (MicrophoneDevice != newMicrophone)
        {
            StopMicrophone();
        }
        if (!isRecording)
        {
            if (!IsPaused)
            {
                BasisDebug.Log("Starting Microphone :" + newMicrophone);
                clip = Microphone.Start(newMicrophone, true, BasisOpusSettings.RecordingFullLength, samplingFrequency);
                MicrophoneIsStarted = true;
            }
            else
            {
                BasisDebug.Log("Microphone Change Stored");
            }
            MicrophoneDevice = newMicrophone;
        }
    }

    private void StopMicrophone()
    {
        if (string.IsNullOrEmpty(MicrophoneDevice))
        {
            return;
        }
        Microphone.End(MicrophoneDevice);
        BasisDebug.Log("Stopped Microphone " + MicrophoneDevice);
        MicrophoneDevice = null;
        MicrophoneIsStarted = false;
    }

    public void ToggleIsPaused()
    {
        IsPaused = !IsPaused;
    }

    public void SetPauseState(bool isPaused)
    {
        IsPaused = isPaused;
    }

    public bool GetPausedState()
    {
        return IsPaused;
    }

    public static bool isPaused = false;
    private bool IsPaused
    {
        get
        {
            return isPaused;
        }
        set
        {
            isPaused = value;
            if (isPaused)
            {
                StopMicrophone();
            }
            else
            {
                ResetMicrophones(SMDMicrophone.SelectedMicrophone);
            }
            OnPausedAction?.Invoke(isPaused);
        }
    }
    private bool ScheduleMainHasAudio;
    private bool ScheduleMainHasSilence;
    public static Action MainThreadOnHasAudio;
    public static Action MainThreadOnHasSilence; // Event triggered when silence is detected
    private readonly object _lock = new object();
    public void LateUpdate()
    {
        if (MicrophoneIsStarted)
        {
            position = Microphone.GetPosition(MicrophoneDevice);
            if (position == head)
            {
                // No new data has been recorded since the last update
                return;
            }

            clip.GetData(microphoneBufferArray, 0);

            // Signal the processing thread to start processing the audio data
            processingEvent.Set();
            lock (_lock)
            {
                if (ScheduleMainHasAudio)
                {
                    MainThreadOnHasAudio?.Invoke();
                    ScheduleMainHasAudio = false;
                }
                else
                {
                    if (ScheduleMainHasSilence)
                    {
                        MainThreadOnHasSilence?.Invoke();
                        ScheduleMainHasSilence = false;
                    }
                }
            }
        }
    }

    void StartProcessingThread()
    {
        processingThread = new Thread(() =>
        {
            while (isRunning)
            {
                processingEvent.WaitOne();  // Wait until there's data to process
                lock (processingLock)
                {
                    if (!isRunning) break;  // Exit if the thread should stop
                    ProcessAudioData(position);
                }
                processingEvent.Reset();  // Reset the event to wait for new data
            }
        });
        processingThread.Start();
    }

    void StopProcessingThread()
    {
        lock (processingLock)
        {
            isRunning = false;

            // Safely trigger the event if the thread is waiting on it
            processingEvent?.Set();

            // Check if the thread is still alive before attempting to join it
            if (processingThread != null && processingThread.IsAlive)
            {
                // Wait for the thread to finish, with a timeout to prevent hanging
                bool terminated = processingThread.Join(1000); // 1 second timeout
            }
        }
    }
    void OnApplicationQuit()
    {
        StopProcessingThread();
    }
    public void ProcessAudioData(int position)
    {
        int dataLength = GetDataLength(bufferLength, head, position);

        while (dataLength >= ProcessBufferLength)
        {
            int remain = bufferLength - head;
            if (remain < ProcessBufferLength)
            {
                Array.Copy(microphoneBufferArray, head, processBufferArray, 0, remain);
                Array.Copy(microphoneBufferArray, 0, processBufferArray, remain, ProcessBufferLength - remain);
            }
            else
            {
                Array.Copy(microphoneBufferArray, head, processBufferArray, 0, ProcessBufferLength);
            }

            AdjustVolume();  // Adjust the volume of the audio data

            if (UseDenoiser)
            {
                ApplyDeNoise();  // Apply noise gate before processing the audio
            }

            RollingRMS();

            if (IsTransmitWorthy())
            {
                OnHasAudio?.Invoke();
                lock (_lock)
                {
                    ScheduleMainHasAudio = true;
                }
             }
            else
            {
                OnHasSilence?.Invoke();
                lock (_lock)
                {
                    ScheduleMainHasSilence = true;
                }
            }

            head = (head + ProcessBufferLength) % bufferLength;
            dataLength -= ProcessBufferLength;
        }
    }
    public void AdjustVolume()
    {
        VAJ.processBufferArray.CopyFrom(processBufferArray);

        handle = VAJ.Schedule(processBufferArray.Length, 64);
        handle.Complete();

        VAJ.processBufferArray.CopyTo(processBufferArray);
    }
    [BurstCompile]
    public struct LogarithmicVolumeAdjustmentJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<float> processBufferArray;
        public float Volume; // Recommended range: 0.0f to 1.0f

        public void Execute(int index)
        {
            float sample = processBufferArray[index];
            float sign = math.sign(sample);
            sample = math.abs(sample);
            sample = math.log(1.0f + Volume * sample) / math.log(1.0f + Volume); // Normalize to keep the scale consistent
            processBufferArray[index] = sign * sample;
        }
    }
    public float GetRMS()
    {
        // Use a double for the sum to avoid overflow and precision issues
        double sum = 0.0;

        for (int Index = 0; Index < ProcessBufferLength; Index++)
        {
            float value = processBufferArray[Index];
            sum += value * value;
        }

        return Mathf.Sqrt((float)(sum / ProcessBufferLength));
    }
    public int GetDataLength(int bufferLength, int head, int position)
    {
        if (position < head)
        {
            return bufferLength - head + position;
        }
        else
        {
            return position - head;
        }
    }
    public void ChangeAudio(float volume)
    {
        Volume = volume;
        // Create the job
        VAJ.Volume = Volume;
        BasisDebug.Log("Set Microphone Volume To "+ Volume);
    }
    public void ApplyDeNoise()
    {
        Denoiser.Denoise(processBufferArray);
    }
    public void RollingRMS()
    {
        float rms = GetRMS();
        rmsValues[rmsIndex] = rms;
        rmsIndex = (rmsIndex + 1) % rmsWindowSize;
        averageRms = rmsValues.Average();
    }
    public bool IsTransmitWorthy()
    {
        return averageRms > silenceThreshold;
    }
}