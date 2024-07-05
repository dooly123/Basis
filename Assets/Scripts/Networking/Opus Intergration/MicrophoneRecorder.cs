using UnityEngine;
using System;
using System.Linq;

public class MicrophoneRecorder : MonoBehaviour
{
    public event Action OnHasAudio;
    public event Action OnHasSilence; // Event triggered when silence is detected
    public AudioClip clip;
    public int head = 0;
    public float[] processBuffer;
    public int ProcessBufferLength;
    public float[] microphoneBuffer;
    public string MicrophoneDevice = null;
    public float silenceThreshold = 0.001f; // RMS threshold for detecting silence
    public BasisOpusSettings BasisOpusSettings;
    public int samplingFrequency;
    public float[] rmsValues;
    public int rmsIndex = 0;
    public int rmsWindowSize = 10; // Size of the moving average window
    public bool useMovingWindowRMS = false; // Flag to enable/disable moving window RMS (needs refinement)

    private int bufferLength;
    private int dataLength;
    private int position;
    private int remain;
    public bool HasMicrophoneSetup = false;
    public void Initialize()
    {
        BasisOpusSettings = BasisDeviceManagement.Instance.BasisOpusSettings;
        processBuffer = BasisOpusSettings.CalculateProcessBuffer();
        ProcessBufferLength = processBuffer.Length;
        samplingFrequency = BasisOpusSettings.GetSampleFreq();
        microphoneBuffer = new float[BasisOpusSettings.RecordingFullLength * samplingFrequency];
        ResetMicrophones(SMDMicrophone.SelectedMicrophone);
        rmsValues = new float[rmsWindowSize];
        bufferLength = microphoneBuffer.Length;
        SMDMicrophone.OnMicrophoneChanged += ResetMicrophones;
        BasisDeviceManagement.Instance.OnBootModeChanged += OnBootModeChanged;
    }
    public void OnDestroy()
    {
        SMDMicrophone.OnMicrophoneChanged -= ResetMicrophones;
        BasisDeviceManagement.Instance.OnBootModeChanged -= OnBootModeChanged;
    }
    private void OnBootModeChanged(BasisBootedMode mode)
    {
        ResetMicrophones(SMDMicrophone.SelectedMicrophone);
    }

    public void DeInitialize()
    {
        Microphone.End(MicrophoneDevice);
    }
    public void ResetMicrophones(string NewMicrophone)
    {
        if (NewMicrophone.ToLower() == "system default")
        {
            NewMicrophone = string.Empty;
        }
        if (MicrophoneDevice != NewMicrophone || HasMicrophoneSetup == false)
        {
            HasMicrophoneSetup = true;
            ForceSetMicrophone(NewMicrophone);
        }
    }
    public void ForceSetMicrophone(string NewMicrophone)
    {
        Debug.Log("firing off" + NewMicrophone);
        if (Microphone.devices.Contains(NewMicrophone) || NewMicrophone == string.Empty)
        {
            Microphone.End(MicrophoneDevice);
            Debug.Log("starting Microphone " + NewMicrophone);
            clip = Microphone.Start(NewMicrophone, true, BasisOpusSettings.RecordingFullLength, samplingFrequency);
            MicrophoneDevice = NewMicrophone;
        }
    }
    void Update()
    {
        position = Microphone.GetPosition(MicrophoneDevice);
        if (position < 0 || head == position)
        {
            return;
        }

        clip.GetData(microphoneBuffer, 0);

        dataLength = GetDataLength(bufferLength, head, position);
        while (dataLength > ProcessBufferLength)
        {
            remain = bufferLength - head;
            if (remain < ProcessBufferLength)
            {
                Array.Copy(microphoneBuffer, head, processBuffer, 0, remain);
                Array.Copy(microphoneBuffer, 0, processBuffer, remain, ProcessBufferLength - remain);
            }
            else
            {
                Array.Copy(microphoneBuffer, head, processBuffer, 0, ProcessBufferLength);
            }

            float rms = GetRMS();
            if (useMovingWindowRMS)
            {
                AddRMSValue(rms);
                rms = GetAverageRMS();
            }

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

    public float GetRMS()
    {
        float sum = 0.0f;
        for (int i = 0; i < processBuffer.Length; i++)
        {
            sum += processBuffer[i] * processBuffer[i];
        }
        return Mathf.Sqrt(sum / processBuffer.Length);
    }

    private void AddRMSValue(float rms)
    {
        rmsValues[rmsIndex] = rms;
        rmsIndex = (rmsIndex + 1) % rmsWindowSize;
    }

    private float GetAverageRMS()
    {
        float sum = 0.0f;
        for (int i = 0; i < rmsValues.Length; i++)
        {
            sum += rmsValues[i];
        }
        return sum / rmsWindowSize;
    }

    static int GetDataLength(int bufferLength, int head, int tail)
    {
        return head <= tail ? tail - head : bufferLength - head + tail;
    }
}