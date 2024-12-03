using UnityEngine;
using UnityOpus;
[CreateAssetMenu(fileName = "newBasisOpusSettings", menuName = "Opus Data")]
public class BasisOpusSettings : ScriptableObject
{
    public int RecordingFullLength = 1;
    public int BitrateKPS = 128000; // 128 kbps
    /// <summary>
    /// where 0 is the fastest on the cpu
    /// and 10 is the most performance hoggy
    /// recommend 10 as network performance is better.
    /// </summary>
    public int Complexity = 10;
    public SamplingFrequency SamplingFrequency = SamplingFrequency.Frequency_48000;
    public NumChannels NumChannels = NumChannels.Mono;
    public OpusApplication OpusApplication = OpusApplication.Audio;
    public OpusSignal OpusSignal = OpusSignal.Auto;
    public float DesiredDurationInSeconds = 0.02f;
    public int GetSampleFreq()
    {
        return SampleFreqToInt(SamplingFrequency);
    }
    public int CalculateDesiredTime()
    {
        return Mathf.CeilToInt(DesiredDurationInSeconds * GetSampleFreq());
    }
    public float[] CalculateProcessBuffer()
    {
        return new float[CalculateDesiredTime()];
    }
    public int GetChannelAsInt()
    {
        return GetChannelAsInt(NumChannels);
    }
    public static int GetChannelAsInt(NumChannels SamplingFrequency)
    {
        switch (SamplingFrequency)
        {
            case NumChannels.Mono:
                return 1;
            case NumChannels.Stereo:
                return 2;
            default:
                return 1;
        }
    }
    public static int SampleFreqToInt(SamplingFrequency SamplingFrequency)
    {
        switch (SamplingFrequency)
        {
            case SamplingFrequency.Frequency_48000:
                return 48000;
            case SamplingFrequency.Frequency_12000:
                return 12000;
            case SamplingFrequency.Frequency_8000:
                return 8000;
            case SamplingFrequency.Frequency_16000:
                return 16000;
            case SamplingFrequency.Frequency_24000:
                return 24000;
            default:
                return 48000;
        }
    }
}