using UnityEngine;
using System;
using System.Linq;

public class MicrophoneRecorder : MicrophoneRecorderBase
{
    public int head = 0;
    public float[] rmsValues;
    public int rmsIndex = 0;
    public int rmsWindowSize = 10; // Size of the moving average window
    private int bufferLength;
    private int dataLength;
    private int position;
    private int remain;
    public bool HasEvents = false;
    public int PacketSize;

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
            BasisDeviceManagement.Instance.OnBootModeChanged += OnBootModeChanged;
            HasEvents = true;
        }

        ChangeAudio(SMDMicrophone.SelectedVolumeMicrophone);
        ResetMicrophones(SMDMicrophone.SelectedMicrophone);
    }

    public void OnDestroy()
    {
        if (HasEvents)
        {
            SMDMicrophone.OnMicrophoneChanged -= ResetMicrophones;
            SMDMicrophone.OnMicrophoneVolumeChanged -= ChangeAudio;
            BasisDeviceManagement.Instance.OnBootModeChanged -= OnBootModeChanged;
            HasEvents = false;
        }
    }

    private void OnBootModeChanged(BasisBootedMode mode)
    {
        ResetMicrophones(SMDMicrophone.SelectedMicrophone);
    }

    public void ResetMicrophones(string newMicrophone)
    {
        if (Microphone.devices.Length != 0)
        {
            if (Microphone.IsRecording(MicrophoneDevice))
            {
                Debug.Log("Is Recording " + MicrophoneDevice);
                if (MicrophoneDevice != newMicrophone)
                {
                    ForceSetMicrophone(newMicrophone);
                }
            }
            else
            {
                Debug.Log("Is not Recording " + MicrophoneDevice);
                ForceSetMicrophone(newMicrophone);
            }
        }
        else
        {
            Debug.LogError("No Microphones found!");
        }
    }

    public void ForceSetMicrophone(string newMicrophone)
    {
        if (newMicrophone == null)
        {
            Debug.LogError("Microphone was null!");
            return;
        }

        if (Microphone.devices.Contains(newMicrophone) || newMicrophone == string.Empty)
        {
            StopMicrophone();
            Debug.Log("Starting Microphone :" + newMicrophone);
            clip = Microphone.Start(newMicrophone, true, BasisOpusSettings.RecordingFullLength, samplingFrequency);
            MicrophoneDevice = newMicrophone;
        }
        else
        {
            Debug.LogError("Microphone device not found: " + newMicrophone);
        }
    }

    private void StopMicrophone()
    {
        if (MicrophoneDevice != null)
        {
            Microphone.End(MicrophoneDevice);
            Debug.Log("Stopped Microphone " + MicrophoneDevice);
            MicrophoneDevice = null;
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

                ApplyNoiseGate(); // Apply noise gate before processing the audio

                AdjustVolume(); // Adjust the volume of the audio data

                float rms = GetRMS();
                rmsValues[rmsIndex] = rms;
                rmsIndex = (rmsIndex + 1) % rmsWindowSize;
                float averageRms = rmsValues.Average();

                if (averageRms < silenceThreshold)
                {
                    OnHasSilence?.Invoke();
                }
                else
                {
                    OnHasAudio?.Invoke();
                }

                head = (head + ProcessBufferLength) % bufferLength;
                dataLength -= ProcessBufferLength;
            }
        }
    }
}