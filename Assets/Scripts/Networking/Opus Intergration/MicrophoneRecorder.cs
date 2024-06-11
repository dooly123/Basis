using UnityEngine;
using System;

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

    private int bufferLength;

    public void Initialize()
    {
        BasisOpusSettings = BasisDeviceManagement.Instance.BasisOpusSettings;
        processBuffer = BasisOpusSettings.CalculateProcessBuffer();
        ProcessBufferLength = processBuffer.Length;
        samplingFrequency = BasisOpusSettings.GetSampleFreq();
        microphoneBuffer = new float[BasisOpusSettings.RecordingFullLength * samplingFrequency];
        clip = Microphone.Start(MicrophoneDevice, true, BasisOpusSettings.RecordingFullLength, samplingFrequency);
        rmsValues = new float[rmsWindowSize];
        bufferLength = microphoneBuffer.Length;
    }

    public void DeInitialize()
    {
        Microphone.End(MicrophoneDevice);
    }

    void Update()
    {
        int position = Microphone.GetPosition(MicrophoneDevice);
        if (position < 0 || head == position)
        {
            return;
        }

        clip.GetData(microphoneBuffer, 0);

        int dataLength = GetDataLength(bufferLength, head, position);
        while (dataLength > ProcessBufferLength)
        {
            int remain = bufferLength - head;
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
            AddRMSValue(rms);
            float averageRMS = GetAverageRMS();

            if (averageRMS < silenceThreshold)
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