using System.Collections.Generic;
using UnityEngine;
namespace FFmpeg.Unity
{
    public class FFUnityAudio : MonoBehaviour
    {
        public AudioSource source;
        public AudioClip _audioClip;
        public int Channel;
        public FFUnityAudioProcess _audioProcess;
        public Queue<float> _audioStream;
        public void Initialize(AudioClip audioClip, FFUnityAudioProcess audioProcess)
        {
            _audioClip = audioClip;
            _audioProcess = audioProcess;
            source.clip = _audioClip;
            source.loop = true; // Set to loop if necessary
            Debug.Log("total channels is " + _audioClip.channels);
            this.gameObject.name = "Audio for VideoPlayer [" + Channel + "]";
            //create audioclip here and callback here aswell.
            //queue should also be here
        }
    }
}