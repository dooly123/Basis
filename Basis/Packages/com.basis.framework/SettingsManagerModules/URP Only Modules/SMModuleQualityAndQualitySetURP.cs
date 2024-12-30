using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Analytics.IAnalytic;
namespace BattlePhaze.SettingsManager.Intergrations
{
    public class SMModuleQualityAndQualitySetURP : SettingsManagerOption
    {
        public UniversalAdditionalCameraData Data;
        public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
        {
            if (NameReturn(0, Option))
            {
                ChangeQualityLevel(Option.SelectedValue);
            }
        }
        public AnisotropicFiltering VeryLow = AnisotropicFiltering.Disable;
        public AnisotropicFiltering low = AnisotropicFiltering.Disable;
        public AnisotropicFiltering medium = AnisotropicFiltering.Enable;
        public AnisotropicFiltering high = AnisotropicFiltering.Enable;
        public AnisotropicFiltering ultra = AnisotropicFiltering.Enable;
        public Camera Camera;
        public void ChangeQualityLevel(string Quality)
        {
            if (Camera == null)
            {
                Camera = Camera.main;
                Data = Camera.GetComponent<UniversalAdditionalCameraData>();
            }
            switch (Quality)
            {
                case "very low":
                    QualitySettings.anisotropicFiltering = VeryLow;
                    QualitySettings.SetQualityLevel(2);
                    QualitySettings.realtimeReflectionProbes = false;
                    QualitySettings.softParticles = false;
                    QualitySettings.particleRaycastBudget = 256;
                    if (Data != null)
                    {
                        Data.renderPostProcessing = false;
                        Data.requiresColorOption = CameraOverrideOption.Off;
                        Data.requiresDepthOption = CameraOverrideOption.Off;
                        Data.renderShadows = false;
                        Data.stopNaN = false;
                    }
                    break;
                case "low":
                    QualitySettings.anisotropicFiltering = low;
                    QualitySettings.SetQualityLevel(2);
                    QualitySettings.realtimeReflectionProbes = true;
                    QualitySettings.softParticles = true;
                    QualitySettings.particleRaycastBudget = 512;
                    if (Data != null)
                    {
                        Data.renderPostProcessing = true;
                        Data.requiresColorOption = CameraOverrideOption.UsePipelineSettings;
                        Data.requiresDepthOption = CameraOverrideOption.UsePipelineSettings;
                        Data.renderShadows = true;
                        Data.stopNaN = true;
                    }
                    break;
                case "medium":
                    QualitySettings.anisotropicFiltering = medium;
                    QualitySettings.SetQualityLevel(1);
                    QualitySettings.realtimeReflectionProbes = true;
                    QualitySettings.softParticles = true;
                    QualitySettings.particleRaycastBudget = 1024;
                    if (Data != null)
                    {
                        Data.renderPostProcessing = true;
                        Data.requiresColorOption = CameraOverrideOption.UsePipelineSettings;
                        Data.requiresDepthOption = CameraOverrideOption.UsePipelineSettings;
                        Data.renderShadows = true;
                        Data.stopNaN = true;
                    }
                    break;
                case "high":
                    QualitySettings.anisotropicFiltering = high;
                    QualitySettings.SetQualityLevel(0);
                    QualitySettings.realtimeReflectionProbes = true;
                    QualitySettings.softParticles = true;
                    QualitySettings.particleRaycastBudget = 2048;
                    if (Data != null)
                    {
                        Data.renderPostProcessing = true;
                        Data.requiresColorOption = CameraOverrideOption.UsePipelineSettings;
                        Data.requiresDepthOption = CameraOverrideOption.UsePipelineSettings;
                        Data.renderShadows = true;
                        Data.stopNaN = true;
                    }
                    break;
                case "ultra":
                    QualitySettings.anisotropicFiltering = ultra;
                    QualitySettings.SetQualityLevel(0);
                    QualitySettings.realtimeReflectionProbes = true;
                    QualitySettings.softParticles = true;
                    QualitySettings.particleRaycastBudget = 4096;
                    if (Data != null)
                    {
                        Data.renderPostProcessing = true;
                        Data.requiresColorOption = CameraOverrideOption.UsePipelineSettings;
                        Data.requiresDepthOption = CameraOverrideOption.UsePipelineSettings;
                        Data.renderShadows = true;
                        Data.stopNaN = true;
                    }
                    break;
            }
        }
    }
}