namespace BattlePhaze.SettingsManager.Intergrations
{
    using UnityEngine;
    using UnityEngine.Audio;
    public class SMModuleAudio : SettingsManagerOption
    {
        public bool UseAudioListener = true;
        public AudioMixer Mixer;
        public string AudioMixerGroupTwo = "SFX Audio";
        public string AudioMixerGroupThree = "Music Audio";
        public string AudioMixerGroupFour= "Player Audio";
        public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
        {
            if (NameReturn(0, Option))
            {
                if (SliderReadOption(Option, Manager, out float Value))
                {
                    AudioListener.volume = Value;
                }
            }
            if (NameReturn(1, Option))
            {
                if (SliderReadOption(Option, Manager, out float Value))
                {
                    ChangeSFXAudio(Value - 80);
                }
            }
            if (NameReturn(2, Option))
            {
                if (SliderReadOption(Option, Manager, out float Value))
                {
                    ChangeMusicAudio(Value - 80);
                }
            }
            if (NameReturn(3, Option))
            {
                if (SliderReadOption(Option, Manager, out float Value))
                {
                    ChangePlayerAudio(Value - 80);
                }
            }
        }
        public void ChangeSFXAudio(float Volume)
        {
            Mixer.SetFloat(AudioMixerGroupTwo, Volume);
        }
        public void ChangeMusicAudio(float Volume)
        {
            Mixer.SetFloat(AudioMixerGroupThree, Volume);
        }
        public void ChangePlayerAudio(float Volume)
        {
            Mixer.SetFloat(AudioMixerGroupFour, Volume);
        }
    }
}