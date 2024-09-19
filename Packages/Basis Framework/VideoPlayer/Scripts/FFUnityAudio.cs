using UnityEngine;
namespace FFmpeg.Unity
{
    public class FFUnityAudio : MonoBehaviour
    {
        public AudioSource source;
        public AudioClip _audioClip;
        public int Channel;
        public FFUnityAudioProcess _audioProcess;

        public void Initialize(AudioClip audioClip, FFUnityAudioProcess audioProcess)
        {
            _audioClip = audioClip;
            _audioProcess = audioProcess;
            source.clip = _audioClip;
            source.loop = true; // Set to loop if necessary
            Debug.Log("total channels is " + _audioClip.channels);
            this.gameObject.name = "Audio for VideoPlayer [" + Channel + "]";
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (_audioProcess != null)
            {
                _audioProcess.AudioCallback(data, Channel);
            }
        }
    }
}