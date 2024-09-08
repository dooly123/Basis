using UnityEngine;
using System;
using System.Linq;
using Basis.Scripts.Device_Management;
using System.Threading;

public class MicrophoneRecorder : MicrophoneRecorderBase
{
    public int head = 0;
    private int bufferLength;
    public bool HasEvents = false;
    public int PacketSize;
    public bool UseDenoiser = false;
    public static Action<bool> OnPausedAction;
    public bool MicrophoneIsStarted = false;
    private Thread processingThread;
    private bool isRunning = true;
    private ManualResetEvent processingEvent = new ManualResetEvent(false);
    private object processingLock = new object();
    public int position;

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
        Debug.Log("Setting Denoiser To " + UseDenoiser);
    }

    public new void OnDestroy()
    {
        if (HasEvents)
        {
            SMDMicrophone.OnMicrophoneChanged -= ResetMicrophones;
            SMDMicrophone.OnMicrophoneVolumeChanged -= ChangeAudio;
            SMDMicrophone.OnMicrophoneUseDenoiserChanged -= ConfigureDenoiser;
            BasisDeviceManagement.Instance.OnBootModeChanged -= OnBootModeChanged;

            HasEvents = false;
        }
        base.OnDestroy();
        StopProcessingThread();  // Stop the processing thread
    }

    private void OnBootModeChanged(string mode)
    {
        ResetMicrophones(SMDMicrophone.SelectedMicrophone);
    }

    public void ResetMicrophones(string newMicrophone)
    {
        if (string.IsNullOrEmpty(newMicrophone))
        {
            Debug.LogError("Microphone was empty or null");
            return;
        }
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No Microphones found!");
            return;
        }
        if (!Microphone.devices.Contains(newMicrophone))
        {
            Debug.LogError("Microphone " + newMicrophone + " not found!");
            return;
        }
        bool isRecording = Microphone.IsRecording(newMicrophone);
        Debug.Log(isRecording ? $"Is Recording {MicrophoneDevice}" : $"Is not Recording {MicrophoneDevice}");
        if (MicrophoneDevice != newMicrophone)
        {
            StopMicrophone();
        }
        if (!isRecording)
        {
            if (!IsPaused)
            {
                Debug.Log("Starting Microphone :" + newMicrophone);
                clip = Microphone.Start(newMicrophone, true, BasisOpusSettings.RecordingFullLength, samplingFrequency);
                MicrophoneIsStarted = true;
            }
            else
            {
                Debug.Log("Microphone Change Stored");
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
        Debug.Log("Stopped Microphone " + MicrophoneDevice);
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

    void LateUpdate()
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
            processingEvent.Set();  // Wake up the thread to stop it
            processingThread.Join();  // Wait for the thread to finish
        }
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
            }
            else
            {
                OnHasSilence?.Invoke();
            }

            head = (head + ProcessBufferLength) % bufferLength;
            dataLength -= ProcessBufferLength;
        }
    }
        public void AdjustVolume()
    {
        if (ProcessedLogVolume == 1)
        {
            // No need to modify
            return;
        }

        for (int Index = 0; Index < ProcessBufferLength; Index++)
        {
            processBufferArray[Index] *= ProcessedLogVolume;
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
        // ProcessedLogVolume = volume;
        // Convert the volume to a logarithmic scale
        float Scaled = 1 + 9 * Volume;
        ProcessedLogVolume = (float)Math.Log10(Scaled); // Logarithmic scaling between 0 and 1
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