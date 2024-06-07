using UnityEngine;
using System;

public class MicrophoneRecorder : MonoBehaviour
{
    public event Action<float[]> OnHasAudio;
    public event Action OnHasSilence; // Event triggered when silence is detected
    public AudioClip clip;
    public int head = 0;
    public float[] processBuffer;
    public float[] microphoneBuffer;
    public string MicrophoneDevice = null;
    public float silenceThreshold = 0.001f; // RMS threshold for detecting silence
    public BasisOpusSettings BasisOpusSettings;
    public int samplingFrequency;
    private float[] rmsValues;
    private int rmsIndex = 0;
    private int rmsWindowSize = 10; // Size of the moving average window
    public void Initialize()
    {
        BasisOpusSettings = BasisDeviceManagement.Instance.BasisOpusSettings;
        processBuffer = BasisOpusSettings.CalculateProcessBuffer();
        samplingFrequency = BasisOpusSettings.GetSampleFreq();
        microphoneBuffer = new float[BasisOpusSettings.RecordingFullLength * samplingFrequency];
        clip = Microphone.Start(MicrophoneDevice, true, BasisOpusSettings.RecordingFullLength, samplingFrequency);
        rmsValues = new float[rmsWindowSize];
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

        while (GetDataLength(microphoneBuffer.Length, head, position) > processBuffer.Length)
        {
            int remain = microphoneBuffer.Length - head;
            if (remain < processBuffer.Length)
            {
                Array.Copy(microphoneBuffer, head, processBuffer, 0, remain);
                Array.Copy(microphoneBuffer, 0, processBuffer, remain, processBuffer.Length - remain);
            }
            else
            {
                Array.Copy(microphoneBuffer, head, processBuffer, 0, processBuffer.Length);
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
                OnHasAudio?.Invoke(processBuffer);
            }

            head += processBuffer.Length;
            if (head >= microphoneBuffer.Length)
            {
                head -= microphoneBuffer.Length;
            }
        }
    }

    public float GetRMS()
    {
        float sum = 0.0f;
        foreach (var sample in processBuffer)
        {
            sum += sample * sample;
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
        foreach (var value in rmsValues)
        {
            sum += value;
        }
        return sum / rmsWindowSize;
    }
    static int GetDataLength(int bufferLength, int head, int tail)
    {
        if (head < tail)
        {
            return tail - head;
        }
        else
        {
            return bufferLength - head + tail;
        }
    }
}