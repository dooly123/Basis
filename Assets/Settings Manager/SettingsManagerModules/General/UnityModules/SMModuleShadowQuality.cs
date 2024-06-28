namespace BattlePhaze.SettingsManager.Intergrations
{
    using UnityEngine;
    public class SMModuleShadowQuality : SettingsManagerOption
    {
        public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
        {
            if (NameReturn(0, Option))
            {
                ChangeShadowQuality(Option.SelectedValue);
            }
            if (NameReturn(1, Option))
            {
                ChangeShadowDistance(Option.SelectedValue);
            }
        }
        public int VeryLowShadowCascades = 1;
        public ShadowQuality VeryLowShadowQuality = ShadowQuality.HardOnly;
        public ShadowResolution VeryLowResolution = ShadowResolution.Low;
        public float VeryLowShadowDistance = 3;
        [Space]
        public int LowShadowCascades = 2;
        public ShadowQuality LowShadowQuality = ShadowQuality.HardOnly;
        public ShadowResolution LowResolution = ShadowResolution.Low;
        public float LowShadowDistance = 5;
        [Space]
        public int mediumShadowCascades = 2;
        public ShadowQuality mediumShadowQuality = ShadowQuality.All;
        public ShadowResolution mediumResolution = ShadowResolution.Low;
        public float MediumShadowDistance = 10;
        [Space]
        public int HighShadowCascades = 3;
        public ShadowQuality highShadowQuality = ShadowQuality.All;
        public ShadowResolution highResolution = ShadowResolution.Low;
        public float HighShadowDistance = 20;
        [Space]
        public int UltraShadowCascades = 4;
        public ShadowQuality ultraShadowQuality = ShadowQuality.All;
        public ShadowResolution ultraResolution = ShadowResolution.VeryHigh;
        public float UltraShadowDistance = 30;
        public void ChangeShadowQuality(string Quality)
        {
            switch (Quality)
            {
                case "very low":
                    QualitySettings.shadows = VeryLowShadowQuality;
                    QualitySettings.shadowCascades = VeryLowShadowCascades;
                    QualitySettings.shadowResolution = VeryLowResolution;
                    break;
                case "low":
                    QualitySettings.shadows = LowShadowQuality;
                    QualitySettings.shadowCascades = LowShadowCascades;
                    QualitySettings.shadowResolution = LowResolution;
                    break;
                case "medium":
                    QualitySettings.shadows = mediumShadowQuality;
                    QualitySettings.shadowCascades = mediumShadowCascades;
                    QualitySettings.shadowResolution = mediumResolution;
                    break;
                case "high":
                    QualitySettings.shadows = highShadowQuality;
                    QualitySettings.shadowCascades = HighShadowCascades;
                    QualitySettings.shadowResolution = highResolution;
                    break;
                case "ultra":
                    QualitySettings.shadows = ultraShadowQuality;
                    QualitySettings.shadowCascades = UltraShadowCascades;
                    QualitySettings.shadowResolution = ultraResolution;
                    break;
            }
        }
        /// <summary>
        /// Changes the shadow quality
        /// </summary>
        /// <param name="Quality"></param>
        public void ChangeShadowDistance(string Quality)
        {
            switch (Quality)
            {
                case "very low":
                    QualitySettings.shadowDistance = VeryLowShadowDistance;
                    break;
                case "low":
                    QualitySettings.shadowDistance = LowShadowDistance;
                    break;
                case "medium":
                    QualitySettings.shadowDistance = MediumShadowDistance;
                    break;
                case "high":
                    QualitySettings.shadowDistance = HighShadowDistance;
                    break;
                case "ultra":
                    QualitySettings.shadowDistance = UltraShadowDistance;
                    break;
            }
        }
    }
}