using UnityEngine;
using System;
using System.Linq;

public class MicrophoneRecorder : MonoBehaviour
{
    public event Action OnHasAudio;
    public event Action OnHasSilence; // Event triggered when silence is detected
    public AudioClip clip;
    public int head = 0;
    public ArraySegment<float> processBuffer; // Changed to ArraySegment<float>
    public int ProcessBufferLength;
    public ArraySegment<float> microphoneBuffer; // Changed to ArraySegment<float>
    public string MicrophoneDevice = null;
    public float silenceThreshold = 0.0001f; // RMS threshold for detecting silence
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

    public bool IsInitialize = false;

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

        SMDMicrophone.OnMicrophoneChanged += ResetMicrophones;
        BasisDeviceManagement.Instance.OnBootModeChanged += OnBootModeChanged;
        ResetMicrophones(SMDMicrophone.SelectedMicrophone);
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

    public void GetData()
    {
        clip.GetData(microphoneBuffer, 0);
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

            GetData();

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
        else
        {
            Debug.Log("No microphone found " + MicrophoneDevice);
        }
    }

    public float GetRMS()
    {
        float sum = 0.0f;
        for (int i = 0; i < processBuffer.Count; i++)
        {
            sum += processBuffer.Array[i + processBuffer.Offset] * processBuffer.Array[i + processBuffer.Offset];
        }
        return Mathf.Sqrt(sum / processBuffer.Count);
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

    private void CopyToProcessBuffer(int sourceIndex, int length)
    {
        Array.Copy(microphoneBuffer.Array, sourceIndex + microphoneBuffer.Offset, processBuffer.Array, processBuffer.Offset, length);
    }

    static int GetDataLength(int bufferLength, int head, int tail)
    {
        return head <= tail ? tail - head : bufferLength - head + tail;
    }
}