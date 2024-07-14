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
    public ArraySegment<float> processBuffer; // Changed to ArraySegment<float>
    public int ProcessBufferLength;
    public ArraySegment<float> microphoneBuffer; // Changed to ArraySegment<float>
    public float Volume = 0.5f; // Volume adjustment factor, default to 1 (no adjustment)
    public void AdjustVolume(float volume)
    {
        for (int Index = 0; Index < ProcessBufferLength; Index++)
        {
            processBuffer.Array[processBuffer.Offset + Index] *= volume;
        }
    }
    public float GetRMS()
    {
        // Use a double for the sum to avoid overflow and precision issues
        double sum = 0.0;

        for (int Index = 0; Index < ProcessBufferLength; Index++)
        {
            float value = processBuffer[Index];
            sum += value * value;
        }

        return Mathf.Sqrt((float)(sum / ProcessBufferLength));
    }

    public void CopyToProcessBuffer(int sourceIndex, int length)
    {
        Array.Copy(microphoneBuffer.Array, sourceIndex + microphoneBuffer.Offset, processBuffer.Array, processBuffer.Offset, length);
    }

    public static int GetDataLength(int bufferLength, int head, int tail)
    {
        return head <= tail ? tail - head : bufferLength - head + tail;
    }
    public void ChangeAudio(float volume)
    {
        Volume = volume;
    }
}