#if SETTINGS_MANAGER_UNIVERSAL
using BattlePhaze.SettingsManager;
using UnityEngine.Rendering.Universal;
public class SMModuleAmbientOcclusionURP : SettingsManagerOption
{
    public UniversalRendererData rendererData;
    public string AmbientOcclusionRenderData = "SSAO";
    public ScriptableRendererFeature AmbientOcclusion;
    public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
    {
        if (NameReturn(0, Option))
        {
            if (AmbientOcclusion == null)
            {
                FindAmbientOcculusion();
            }
            ComputeState(Option.SelectedValue);
        }
    }
    public void FindAmbientOcculusion()
    {
        for (int RenderFeatureIndex = 0; RenderFeatureIndex < rendererData.rendererFeatures.Count; RenderFeatureIndex++)
        {
            if (rendererData.rendererFeatures[RenderFeatureIndex].name == AmbientOcclusionRenderData)
            {
                AmbientOcclusion = rendererData.rendererFeatures[RenderFeatureIndex];
                return;
            }
        }
    }
    public void ComputeState(string Quality)
    {
        if (AmbientOcclusion != null)
        {
            bool State = CheckIsOn(Quality);
            AmbientOcclusion.SetActive(State);
        }
    }
}
#endif