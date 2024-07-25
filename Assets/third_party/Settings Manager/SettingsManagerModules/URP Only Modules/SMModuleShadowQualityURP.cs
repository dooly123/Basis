#if SETTINGS_MANAGER_UNIVERSAL
using BattlePhaze.SettingsManager;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

public class SMModuleShadowQualityURP : SettingsManagerOption
{
    public UniversalRenderPipelineAsset Asset;
    public ShadowResolution VeryLowShadowQualityAdditional = ShadowResolution._256;
    public ShadowResolution VeryLowShadowQuality = ShadowResolution._256;
    public float VeryLowShadowDistance = 150;
    [Space]
    public ShadowResolution LowShadowQualityAdditional = ShadowResolution._512;
    public ShadowResolution LowShadowQuality = ShadowResolution._512;
    public float LowShadowDistance = 250;
    [Space]
    public ShadowResolution MediumShadowQualityAdditional = ShadowResolution._1024;
    public ShadowResolution mediumShadowQuality = ShadowResolution._1024;
    public float MediumShadowDistance = 350;
    [Space]
    public ShadowResolution HighShadowQualityAdditional = ShadowResolution._2048;
    public ShadowResolution highShadowQuality = ShadowResolution._2048;
    public float HighShadowDistance = 500;
    [Space]
    public ShadowResolution ultraShadowQualityAdditional = ShadowResolution._4096;
    public ShadowResolution ultraShadowQuality = ShadowResolution._4096;
    public float UltraShadowDistance = 1000;

    public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
    {
        if (NameReturn(0, Option))
        {
            if (Asset == null)
            {
                Asset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
            }
            ChangeShadowQuality(Option.SelectedValue);
        }
        if (NameReturn(1, Option))
        {
            if (Asset == null)
            {
                Asset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
            }
            ChangeShadowDistance(Option.SelectedValue);
        }
    }
    public void ChangeShadowQuality(string Quality)
    {
        switch (Quality)
        {
            case "very low":
                MainLightShadowResolution = VeryLowShadowQuality;
                AdditionalLightsShadowResolution = VeryLowShadowQualityAdditional;
                break;
            case "low":
                MainLightShadowResolution = LowShadowQuality;
                AdditionalLightsShadowResolution = LowShadowQualityAdditional;
                break;
            case "medium":
                MainLightShadowResolution = mediumShadowQuality;
                AdditionalLightsShadowResolution = MediumShadowQualityAdditional;
                break;
            case "high":
                MainLightShadowResolution = highShadowQuality;
                AdditionalLightsShadowResolution = HighShadowQualityAdditional;
                break;
            case "ultra":
                MainLightShadowResolution = ultraShadowQuality;
                AdditionalLightsShadowResolution = ultraShadowQualityAdditional;
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
                Asset.shadowDistance = VeryLowShadowDistance;
                break;
            case "low":
                Asset.shadowDistance = LowShadowDistance;
                break;
            case "medium":
                Asset.shadowDistance = MediumShadowDistance;
                break;
            case "high":
                Asset.shadowDistance = HighShadowDistance;
                break;
            case "ultra":
                Asset.shadowDistance = UltraShadowDistance;
                break;
        }
    }
    private System.Type universalRenderPipelineAssetType;
    private FieldInfo mainLightShadowmapResolutionFieldInfo;
    private FieldInfo additionalLightsRenderingMode;
    private void InitializeShadowMapFieldInfo()
    {
        universalRenderPipelineAssetType = Asset.GetType();
        mainLightShadowmapResolutionFieldInfo = universalRenderPipelineAssetType.GetField("m_MainLightShadowmapResolution", BindingFlags.Instance | BindingFlags.NonPublic);
        additionalLightsRenderingMode = universalRenderPipelineAssetType.GetField("m_AdditionalLightsShadowmapResolution", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public ShadowResolution MainLightShadowResolution
    {
        get
        {
            if (mainLightShadowmapResolutionFieldInfo == null)
            {
                InitializeShadowMapFieldInfo();
            }
            return (ShadowResolution)mainLightShadowmapResolutionFieldInfo.GetValue(Asset);
        }
        set
        {
            if (mainLightShadowmapResolutionFieldInfo == null)
            {
                InitializeShadowMapFieldInfo();
            }
            mainLightShadowmapResolutionFieldInfo.SetValue(Asset, value);
        }
    }
    public ShadowResolution AdditionalLightsShadowResolution
    {
        get
        {
            if (additionalLightsRenderingMode == null)
            {
                InitializeShadowMapFieldInfo();
            }
            return (ShadowResolution)additionalLightsRenderingMode.GetValue(Asset);
        }
        set
        {
            if (additionalLightsRenderingMode == null)
            {
                InitializeShadowMapFieldInfo();
            }
            additionalLightsRenderingMode.SetValue(Asset, value);
        }
    }
}
#endif