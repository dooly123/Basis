namespace BattlePhaze.SettingsManager.Intergrations
{
    using UnityEngine;
    using UnityEngine.Audio;
    public class SMModuleAudio : SettingsManagerOption
    {
        public bool UseAudioListener = true;
        public AudioMixer Mixer;
        public string AudioMixerGroupOne = "Master";
        public string AudioMixerGroupTwo = "SFX Audio";
        public string AudioMixerGroupThree = "Music Audio";
        public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
        {
            if (NameReturn(0, Option))
            {
                if (SliderReadOption(Option, Manager, out float Value))
                {
                    if (UseAudioListener)
                    {
                        AudioListener.volume = Value;
                    }
                    else
                    {
                        ChangeMainAudio(Value);
                    }
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
        }
        public void ChangeMainAudio(float Volume)
        {
            Mixer.SetFloat(AudioMixerGroupOne, Volume);
        }
        public void ChangeSFXAudio(float Volume)
        {
            Mixer.SetFloat(AudioMixerGroupTwo, Volume);
        }
        public void ChangeMusicAudio(float Volume)
        {
            Mixer.SetFloat(AudioMixerGroupThree, Volume);
        }
    }
}