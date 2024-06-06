using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
public class BasisDeviceManagement : MonoBehaviour
{
    public bool ForceNoVR = false;
    public BootedMode CurrentMode = BootedMode.None;
    public static BasisDeviceManagement Instance;
    public enum BootedMode
    {
        OpenVR,
        XR,
        Desktop,
        None
    }
    async void Start()
    {
        Instance = this;
        InstantiationParameters Parameters = new InstantiationParameters();
        await BasisPlayerFactory.CreateLocalPLayer(Parameters);
        
        if (ForceNoVR == false && BasisOpenVRManagement.TryStartOpenVR())
        {
            BasisOverrideRotations BasisXRHeadToBodyOverride = BasisHelpers.GetOrAddComponent<BasisOverrideRotations>(this.gameObject);
            BasisXRHeadToBodyOverride.Initialize();
            Debug.Log("OpenVR Started Correctly");
            CurrentMode = BootedMode.OpenVR;
            BasisLocalCameraDriver.Instance.CameraData.allowXRRendering = true;
        }
        else if (ForceNoVR == false && BasisOpenXRManagement.TryStartXR())
        {
            BasisOverrideRotations BasisXRHeadToBodyOverride = BasisHelpers.GetOrAddComponent<BasisOverrideRotations>(this.gameObject);
            BasisXRHeadToBodyOverride.Initialize();
            Debug.Log("XR Started Correctly");
            CurrentMode = BootedMode.XR;
           // BasisLocalCameraDriver.Instance.Camera.stereoTargetEye = StereoTargetEyeMask.Both;
            BasisLocalCameraDriver.Instance.CameraData.allowXRRendering = true;
        }
        else
        {
            Debug.Log("Falling back to Desktop");
            await BasisDeviceManagement.LoadGameobject("Assets/Prefabs/Loadins/Desktop Eye Controller.prefab", new InstantiationParameters());
            CurrentMode = BootedMode.Desktop;
           // BasisLocalCameraDriver.Instance.Camera.stereoTargetEye = StereoTargetEyeMask.None;
            BasisLocalCameraDriver.Instance.CameraData.allowXRRendering = false;
        }
        await LoadGameobject("NetworkManagement",new InstantiationParameters());
    }
    public void OnDestroy()
    {
        BasisOpenXRManagement.StopXR();
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