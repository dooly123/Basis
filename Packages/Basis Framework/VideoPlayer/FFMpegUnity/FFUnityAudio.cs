using UnityEngine;
namespace FFmpeg.Unity
{
    public class FFUnityAudio : MonoBehaviour
    {
        public AudioSource source;
        public AudioClip _audioClip;
        public int Channel;
        public void Initialize(AudioClip audioClip)
        {
            _audioClip = audioClip;
            source.clip = _audioClip;
            source.loop = true; // Set to loop if necessary
            Debug.Log("total channels is " + _audioClip.channels);
            this.gameObject.name = "Audio for VideoPlayer [" + Channel + "]";
            source.dopplerLevel = 0;
            //create audioclip here and callback here aswell.
            //queue should also be here
        }
    }
}