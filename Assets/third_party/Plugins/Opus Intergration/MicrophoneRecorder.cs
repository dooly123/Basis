using UnityEngine;
using System;
using System.Linq;
public class MicrophoneRecorder : MicrophoneRecorderBase
{
    public int head = 0;
    private int bufferLength;
    private int dataLength;
    private int position;
    private int remain;
    public bool HasEvents = false;
    public int PacketSize;
    public bool UseDenoiser = false;
    public static Action<bool> OnPausedAction;
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
        if (HasEvents == false)
        {
            SMDMicrophone.OnMicrophoneChanged += ResetMicrophones;
            SMDMicrophone.OnMicrophoneVolumeChanged += ChangeAudio;
            SMDMicrophone.OnMicrophoneUseDenoiserChanged += ConfigureDenioser;
            BasisDeviceManagement.Instance.OnBootModeChanged += OnBootModeChanged;
            HasEvents = true;
        }
        ChangeAudio(SMDMicrophone.SelectedVolumeMicrophone);
        ResetMicrophones(SMDMicrophone.SelectedMicrophone);
        ConfigureDenioser(SMDMicrophone.SelectedDenoiserMicrophone);
    }

    private void ConfigureDenioser(bool useDenoiser)
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
            SMDMicrophone.OnMicrophoneUseDenoiserChanged -= ConfigureDenioser;
            BasisDeviceManagement.Instance.OnBootModeChanged -= OnBootModeChanged;

            HasEvents = false;
        }
        base.OnDestroy();
    }
    private void OnBootModeChanged(BasisBootedMode mode)
    {
        ResetMicrophones(SMDMicrophone.SelectedMicrophone);
    }
    public void ResetMicrophones(string newMicrophone)
    {
        if (string.IsNullOrEmpty(newMicrophone))
        {
            Debug.LogError("microphone was empty or null");
            return;
        }
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No Microphones found!");
            return;
        }
        if (Microphone.devices.Contains(newMicrophone) == false)
        {
            Debug.LogError("Microphone " + newMicrophone);
            return;
        }
        bool isRecording = Microphone.IsRecording(MicrophoneDevice);
        Debug.Log(isRecording ? $"Is Recording {MicrophoneDevice}" : $"Is not Recording {MicrophoneDevice}");
        bool MicrophoneNotMatchCheck = MicrophoneDevice != newMicrophone;
        if (MicrophoneNotMatchCheck)
        {
            StopMicrophone();
        }
        //if we are not recording or the current microphone does not match the new one.
        if (!isRecording || MicrophoneNotMatchCheck)
        {
            if (IsPaused == false)
            {
                Debug.Log("Starting Microphone :" + newMicrophone);
                clip = Microphone.Start(newMicrophone, true, BasisOpusSettings.RecordingFullLength, samplingFrequency);
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
    void Update()
    {
        if (Microphone.IsRecording(MicrophoneDevice))
        {
            position = Microphone.GetPosition(MicrophoneDevice);
            if (position == head)
            {
                // No new data has been recorded since the last update
                return;
            }
            clip.GetData(microphoneBufferArray, 0);
            dataLength = GetDataLength(bufferLength, head, position);
            while (dataLength >= ProcessBufferLength)
            {
                remain = bufferLength - head;
                if (remain < ProcessBufferLength)
                {
                    Array.Copy(microphoneBufferArray, head, processBufferArray, 0, remain);
                    Array.Copy(microphoneBufferArray, 0, processBufferArray, remain, ProcessBufferLength - remain);
                }
                else
                {
                    Array.Copy(microphoneBufferArray, head, processBufferArray, 0, ProcessBufferLength);
                }
                AdjustVolume(); // Adjust the volume of the audio data
                if (UseDenoiser)
                {
                    ApplyDeNoise(); // Apply noise gate before processing the audio
                }
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
    }
}