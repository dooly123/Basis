#if SETTINGS_MANAGER_UNIVERSAL
using BattlePhaze.SettingsManager;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class SMModuleRenderResolutionURP : SettingsManagerOption
{
    public UniversalRenderPipelineAsset Asset;
    public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
    {
        if (NameReturn(0, Option))
        {
            if (Asset == null)
            {
                Asset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
            }
            if (SliderReadOption(Option, Manager, out float Value))
            {
                SetRenderResolution(Value);
            }
        }
        else
        {
            if (NameReturn(1, Option))
            {
                SetUpscaler(Option.SelectedValue);
            }
        }
    }
    public void SetRenderResolution(float renderScale)
    {

        Asset.renderScale = renderScale;
    }
    public void SetUpscaler(string Using)
    {
        switch (Using)
        {
            case "Auto":
                Asset.upscalingFilter = UpscalingFilterSelection.Auto;
                break;
            case "Linear Upscaling":
                Asset.upscalingFilter = UpscalingFilterSelection.Linear;
                break;
            case "Point Upscaling":
                Asset.upscalingFilter = UpscalingFilterSelection.Point;
                break;
            case "FSR Upscaling":
                Asset.upscalingFilter = UpscalingFilterSelection.FSR;
                break;
            case "Spatial Temporal Upscaling":
                Asset.upscalingFilter = UpscalingFilterSelection.STP;
                break;
        }
    }
}
#endif