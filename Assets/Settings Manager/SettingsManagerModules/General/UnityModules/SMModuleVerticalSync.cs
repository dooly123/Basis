using UnityEngine;

namespace BattlePhaze.SettingsManager.Integrations
{
    public class SMModuleVerticalSync : SettingsManagerOption
    {
        public int targetFrameRateWhenOff = 500;

        public override void ReceiveOption(SettingsMenuInput option, SettingsManager manager)
        {
            if (NameReturn(0, option))
            {
                ChangeVerticalSync(option.SelectedValue);
            }
        }

        public void ChangeVerticalSync(string quality)
        {
            switch (quality)
            {
                case "on":
                    QualitySettings.vSyncCount = 1;
                    Application.targetFrameRate = Screen.currentResolution.refreshRate;
                    break;
                case "half":
                    QualitySettings.vSyncCount = 2;
                    Application.targetFrameRate = Screen.currentResolution.refreshRate / 2;
                    break;
                case "off":
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = targetFrameRateWhenOff;
                    break;
            }
        }
    }
}