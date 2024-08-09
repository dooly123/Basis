using UnityEngine;
namespace BattlePhaze.SettingsManager.Intergrations
{
    public class SMModuleAnisotropicFiltering : SettingsManagerOption
    {
        public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
        {
            if (NameReturn(0, Option))
            {
                ChangeAniotropicFiltering(Option.SelectedValue);
            }
        }
        public AnisotropicFiltering VeryLow = AnisotropicFiltering.Disable;
        public AnisotropicFiltering low = AnisotropicFiltering.Disable;
        public AnisotropicFiltering medium = AnisotropicFiltering.Enable;
        public AnisotropicFiltering high = AnisotropicFiltering.Enable;
        public AnisotropicFiltering ultra = AnisotropicFiltering.Enable;
        public void ChangeAniotropicFiltering(string Quality)
        {
            switch (Quality)
            {
                case "very low":
                    QualitySettings.anisotropicFiltering = VeryLow;
                    break;
                case "low":
                    QualitySettings.anisotropicFiltering = low;
                    break;
                case "medium":
                    QualitySettings.anisotropicFiltering = medium;
                    break;
                case "high":
                    QualitySettings.anisotropicFiltering = high;
                    break;
                case "ultra":
                    QualitySettings.anisotropicFiltering = ultra;
                    break;
            }
        }
    }
}