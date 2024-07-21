#if SETTINGS_MANAGER_UNIVERSAL
using BattlePhaze.SettingsManager;
using UnityEngine.Rendering.Universal;
public class SMModuleAmbientOcclusionURP : SettingsManagerOption
{
    public ScriptableRendererFeature AmbientOcclusion;
    public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
    {
        if (NameReturn(0, Option))
        {
            ComputeState(Option.SelectedValue);
        }
    }
    public void ComputeState(string Quality)
    {
        if (AmbientOcclusion != null)
        {
            switch (Quality)
            {
                case "very low":
                    AmbientOcclusion.SetActive(false);
                    break;
                case "low":
                    AmbientOcclusion.SetActive(true);
                    break;
                case "medium":
                    AmbientOcclusion.SetActive(true);
                    break;
                case "high":
                    AmbientOcclusion.SetActive(true);
                    break;
                case "ultra":
                    AmbientOcclusion.SetActive(true);
                    break;
            }
        }
    }
}
#endif