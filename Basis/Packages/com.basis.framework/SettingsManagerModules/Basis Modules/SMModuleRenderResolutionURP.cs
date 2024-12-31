#if SETTINGS_MANAGER_UNIVERSAL
using Basis.Scripts.Device_Management;
using BattlePhaze.SettingsManager;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
public class SMModuleRenderResolutionURP : SettingsManagerOption
{
    public void Start()
    {
        BasisDeviceManagement.Instance.OnBootModeChanged += OnBootModeChanged;
    }

    private void OnBootModeChanged(string obj)
    {
        SetRenderResolution(RenderScale);
    }

    public void OnDestroy()
    {
        BasisDeviceManagement.Instance.OnBootModeChanged -= OnBootModeChanged;
    }
    public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
    {
        if (NameReturn(0, Option))
        {
            BasisDebug.Log("Render Resolution");
            UniversalRenderPipelineAsset Asset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
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
    public float RenderScale = 1;
    public void SetRenderResolution(float renderScale)
    {
        RenderScale = renderScale;
        if (BasisDeviceManagement.Instance.CurrentMode == BasisDeviceManagement.Desktop)
        {
            UniversalRenderPipelineAsset Asset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
            if (Asset.renderScale != RenderScale)
            {
                Asset.renderScale = RenderScale;
            }
        }
        else
        {
            if (XRSettings.useOcclusionMesh == false)
            {
                XRSettings.useOcclusionMesh = true;
            }
            UniversalRenderPipelineAsset Asset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
            if (Asset.renderScale != 1)
            {
                Asset.renderScale = 1;
            }
            if (XRSettings.eyeTextureResolutionScale != renderScale)
            {
                XRSettings.eyeTextureResolutionScale = RenderScale;
            }
            if (XRSettings.renderViewportScale != 1)
            {
                XRSettings.renderViewportScale = 1;
            }
        }
    }
    public void SetUpscaler(string Using)
    {
        UniversalRenderPipelineAsset Asset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
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