#if SETTINGS_MANAGER_UNIVERSAL
using BattlePhaze.SettingsManager;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SMModuleHDRURP : SettingsManagerOption
{
    public UniversalRenderPipelineAsset Asset;
    public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
    {
        if (NameReturn(0, Option))
        {
            SetHDR(Option.SelectedValue);
        }
    }
    public void SetHDR(string SelectedValue)
    {
        if (Asset == null)
        {
            Asset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
        }
        Asset.supportsHDR = CheckIsOn(SelectedValue);
    }
}
#endif