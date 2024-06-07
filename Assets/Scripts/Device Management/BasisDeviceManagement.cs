using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
public partial class BasisDeviceManagement : MonoBehaviour
{
    public BasisBootedMode CurrentMode = BasisBootedMode.Desktop;
    public static BasisDeviceManagement Instance;
    public BasisAvatarEyeInput BasisAvatarEyeInput;
    void Start()
    {
        if (BasisHelpers.CheckInstance<BasisDeviceManagement>(Instance))
        {
            Instance = this;
        }
        Initalize();
    }
    public async void Initalize()
    {
        InstantiationParameters Parameters = new InstantiationParameters();
        await BasisPlayerFactory.CreateLocalPLayer(Parameters);
        BasisOverrideRotations BasisXRHeadToBodyOverride = BasisHelpers.GetOrAddComponent<BasisOverrideRotations>(this.gameObject);
        BasisXRHeadToBodyOverride.Initialize();
        SwitchMode(CurrentMode);
        await LoadGameobject("NetworkManagement", new InstantiationParameters());
    }
#if UNITY_EDITOR
    [MenuItem("Basis/ForceSetOpenXR")]
    public static void ForceSetOpenXR()
    {
     Instance.SwitchMode(BasisBootedMode.OpenXR);
    }
    [MenuItem("Basis/ForceSetOpenVR")]
    public static void ForceSetOpenVR()
    {
        Instance.SwitchMode(BasisBootedMode.OpenVR);
    }
    [MenuItem("Basis/ForceSetDesktop")]
    public static void ForceSetDesktop() 
    {
        Instance.SwitchMode(BasisBootedMode.Desktop);
    }
#else
    public static void ForceSetOpenXR()
    {
     Instance.SwitchMode(BasisBootedMode.OpenXR);
    }
    public static void ForceSetOpenVR()
    {
        Instance.SwitchMode(BasisBootedMode.OpenVR);
    }

    public static void ForceSetDesktop() 
    {
        Instance.SwitchMode(BasisBootedMode.Desktop);
    }
#endif
    public void SwitchMode(BasisBootedMode BasisBootedMode)
    {
        ShutDownLastMode();
        switch (BasisBootedMode)
        {
            case BasisBootedMode.OpenVR:
                if (BasisOpenVRManagement.TryStartOpenVR())
                {
                    SetCameraRenderState(true);
                }
                else
                {
                    UseFallBack();
                }
                break;
            case BasisBootedMode.OpenXR:
                if (BasisOpenXRManagement.TryStartOpenXR())
                {
                    SetCameraRenderState(true);
                }
                else
                {
                    UseFallBack();
                }
                break;
            case BasisBootedMode.Desktop:
                UseFallBack();
                break;
            default:
                Debug.LogError("this should not occur (default)");
                UseFallBack();
                break;
        }
    }
    public void SetCameraRenderState(bool state)
    {
        BasisLocalCameraDriver.Instance.CameraData.allowXRRendering = state;
    }
    public void UseFallBack()
    {
        SetCameraRenderState(false);
        BasisAvatarEyeInput = BasisHelpers.GetOrAddComponent<BasisAvatarEyeInput>(this.gameObject);
        CurrentMode = BasisBootedMode.Desktop;
        BasisLocalCameraDriver.Instance.CameraData.allowXRRendering = false;
    }
    public void OnDestroy()
    {
        ShutDownLastMode();
    }
    public void ShutDownLastMode()
    {
        BasisOpenVRManagement.StopXRSDK();
        BasisOpenXRManagement.StopXR();
        StopDesktop();
        switch (CurrentMode)
        {
            case BasisBootedMode.OpenVR:
                break;
            case BasisBootedMode.OpenXR:
                break;
            case BasisBootedMode.Desktop:
                break;
        }
    }
    public void StopDesktop()
    {
        if (BasisAvatarEyeInput != null)
        {
            GameObject.Destroy(BasisAvatarEyeInput);
        }
    }
    public static async Task<BasisPlayer> LoadGameobject(string PlayerAddressableID, InstantiationParameters InstantiationParameters)
    {
        List<GameObject> Gameobjects = await AddressableResourceProcess.LoadAsGameObjectsAsync(PlayerAddressableID, InstantiationParameters);
        if (Gameobjects.Count != 0)
        {
        }
        else
        {
            Debug.LogError("Missing ");
        }
        return null;
    }
}