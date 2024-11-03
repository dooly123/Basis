using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BattlePhaze.SettingsManager.Intergrations
{
    public class SMModuleAudio : SettingsManagerOption
    {
        public bool UseAudioListener = true;
        public AudioMixer Mixer;
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
                    ChangeVolume(Value - 80, Option.Name);
                }
            }
            if (NameReturn(2, Option))
            {
                if (SliderReadOption(Option, Manager, out float Value))
                {
                    ChangeVolume(Value - 80, Option.Name);
                }
            }
            if (NameReturn(3, Option))
            {
                if (SliderReadOption(Option, Manager, out float Value))
                {
                    ChangeVolume(Value - 80, Option.Name);
                }
            }
        }
        public void ChangeVolume(float Value, string Name)
        {
            Mixer.SetFloat(Name, Value);
        }
    }
}