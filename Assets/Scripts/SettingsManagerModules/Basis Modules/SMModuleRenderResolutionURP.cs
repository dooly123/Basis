#if SETTINGS_MANAGER_UNIVERSAL
using Basis.Scripts.Device_Management;
using BattlePhaze.SettingsManager;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
public class SMModuleRenderResolutionURP : SettingsManagerOption
{
    public UniversalRenderPipelineAsset Asset;
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
            Debug.Log("Render Resolution");
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
    public float RenderScale = 1;
    public void SetRenderResolution(float renderScale)
    {
        RenderScale = renderScale;
        if (BasisDeviceManagement.Instance.CurrentMode == BasisDeviceManagement.Desktop)
        {
            Asset.renderScale = RenderScale;
        }
        else
        {
            XRSettings.eyeTextureResolutionScale = RenderScale;
            XRSettings.useOcclusionMesh = true;
            Asset.renderScale = 1;
        }
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