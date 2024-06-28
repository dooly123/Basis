#if SETTINGS_MANAGER_UNIVERSAL
using BattlePhaze.SettingsManager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class SMModuleBrightnessURP : SettingsManagerOption
{
    public bool UsePostExposure;
    public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
    {
        if (NameReturn(0, Option))
        {
            if (SliderReadOption(Option, Manager, out float Value))
            {
                if (UsePostExposure)
                {
                    URPBrightness(Value);
                }
                else
                {
                    RenderSettings.ambientLight = new Color(Value, Value, Value, 1.0f);
                }
            }
            }
        }
    public void URPBrightness(float Brightness)
    {
        if (FindObjectOfType<Volume>())
        {
            FindObjectOfType<Volume>().sharedProfile.TryGet(out ColorAdjustments adjustment);
            if (adjustment != null)
            {
                adjustment.postExposure.value = Brightness;
            }
        }
    }
}
#endif