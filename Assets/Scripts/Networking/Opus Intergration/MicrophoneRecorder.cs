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
        float[] processBufferArray = BasisOpusSettings.CalculateProcessBuffer();
        processBuffer = new ArraySegment<float>(processBufferArray); // Initialize ArraySegment
        ProcessBufferLength = processBufferArray.Length;
        samplingFrequency = BasisOpusSettings.GetSampleFreq();
        float[] microphoneBufferArray = new float[BasisOpusSettings.RecordingFullLength * samplingFrequency];
        microphoneBuffer = new ArraySegment<float>(microphoneBufferArray); // Initialize ArraySegment
        rmsValues = new float[rmsWindowSize];
        bufferLength = microphoneBufferArray.Length;

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
            if (position < 0 || head == position)
            {
                Debug.Log("Bailing out " + position + " | " + head);
                return;
            }

            clip.GetData(microphoneBuffer, 0);

            dataLength = GetDataLength(bufferLength, head, position);
            while (dataLength > ProcessBufferLength)
            {
                remain = bufferLength - head;
                if (remain < ProcessBufferLength)
                {
                    CopyToProcessBuffer(head, remain);
                    CopyToProcessBuffer(0, ProcessBufferLength - remain);
                }
                else
                {
                    CopyToProcessBuffer(head, ProcessBufferLength);
                }

                AdjustVolume(Volume); // Adjust the volume of the audio data

                float rms = GetRMS();
                rmsValues[rmsIndex] = rms;
                rmsIndex = (rmsIndex + 1) % rmsWindowSize;
                rms = rmsValues.Average();

                if (rms < silenceThreshold)
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