using UnityEngine;
using System;
using System.Linq;
public abstract class MicrophoneRecorderBase : MonoBehaviour
{
    public static Action OnHasAudio;
    public static Action OnHasSilence; // Event triggered when silence is detected
    public AudioClip clip;
    public BasisOpusSettings BasisOpusSettings;
    public bool IsInitialize = false;
    public string MicrophoneDevice = null;
    public float silenceThreshold = 0.0007f; // RMS threshold for detecting silence
    public int samplingFrequency;
    public int ProcessBufferLength;
    public float Volume = 1; // Volume adjustment factor, default to 1 (no adjustment)
    public float[] microphoneBufferArray;
    public float[] processBufferArray;
    public float noiseGateThreshold = 0.01f; // Threshold for the noise gate
    public int Channels = 1;
    public float ProcessedLogVolume;
    public float[] rmsValues;
    public int rmsIndex = 0;
    public int rmsWindowSize = 10; // Size of the moving average window
    public float averageRms;
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
    public void ApplyNoiseGate()
    {

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