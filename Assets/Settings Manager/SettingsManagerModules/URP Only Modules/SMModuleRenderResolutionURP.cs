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
    }
    public void SetRenderResolution(float renderScale)
    {

        Asset.renderScale = renderScale;
    }
}
#endif