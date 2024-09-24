using FFmpeg.Unity;
using System.Collections.Generic;
using UnityEngine;

public static class FFUnityAudioHelper
{
    public static void SetVolume(List<FFUnityAudio> AudioSources,float Volume)
    {
        foreach (FFUnityAudio FFUnityAudio in AudioSources)
        {
            if (FFUnityAudio.source != null)
            {
                FFUnityAudio.source.volume = Volume;
            }
        }
    }
    public static void StopAll(List<FFUnityAudio> AudioSources)
    {
        foreach (FFUnityAudio FFUnityAudio in AudioSources)
        {
            if (FFUnityAudio.source != null)
            {
                FFUnityAudio.source.Stop();
            }
        }
    }
    public static void PlayAll(List<FFUnityAudio> AudioSources,int Channel,AudioClip Clip)
    {
        foreach (FFUnityAudio FFUnityAudio in AudioSources)
        {
            if (FFUnityAudio.Channel != Channel)
            {
                continue;
            }
            if (FFUnityAudio.source != null)
            {
                FFUnityAudio.source.clip = Clip;
                FFUnityAudio.source.PlayScheduled(AudioSettings.dspTime);
            }
        }
    }
    public static void UnPauseAll(List<FFUnityAudio> AudioSources)
    {
        foreach (FFUnityAudio FFUnityAudio in AudioSources)
        {
            if (FFUnityAudio.source != null)
            {
                FFUnityAudio.source.UnPause();
            }
        }
    }
    public static void PauseAll(List<FFUnityAudio> AudioSources)
    {
        foreach (FFUnityAudio FFUnityAudio in AudioSources)
        {
            if (FFUnityAudio.source != null)
            {
                FFUnityAudio.source.Pause();
            }
        }
    }
}
