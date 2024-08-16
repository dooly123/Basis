using Basis.Scripts.Device_Management;
using UnityEngine;

namespace Basis.Scripts.UI.UI_Panels
{
public class SwitchActiveMode : MonoBehaviour
{
    public UnityEngine.UI.Button VRButton;
    public UnityEngine.UI.Button DesktopButton;
    public void Start()
    {
        VRButton.onClick.AddListener(OnVRButton);
        DesktopButton.onClick.AddListener(OnDesktopButton);
    }
    public async void OnDesktopButton()
    {
      await  BasisDeviceManagement.Instance.SwitchMode("Desktop");
    }
    public async void OnVRButton()
    {
      await  BasisDeviceManagement.Instance.SwitchMode("OpenVRLoader");
    }
}
}