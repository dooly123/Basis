using UnityEngine;
using System;
public abstract class MicrophoneRecorderBase : MonoBehaviour
{
    public Action OnHasAudio;
    public Action OnHasSilence; // Event triggered when silence is detected
    public AudioClip clip;
    public BasisOpusSettings BasisOpusSettings;
    public bool IsInitialize = false;
    public string MicrophoneDevice = null;
    public float silenceThreshold = 0.0007f; // RMS threshold for detecting silence
    public int samplingFrequency;
    public int ProcessBufferLength;
    public float Volume = 0.2f; // Volume adjustment factor, default to 1 (no adjustment)
    public float[] microphoneBufferArray;
    public float[] processBufferArray;
    public void AdjustVolume(float volume)
    {
        if(volume == 1)
        {
            //no need to modify
            return;
        }
        for (int i = 0; i < ProcessBufferLength; i++)
        {
            processBufferArray[i] *= volume;
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
    }
}