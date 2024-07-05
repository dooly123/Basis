using UnityEngine;

public class SwitchActiveMode : MonoBehaviour
{
    public UnityEngine.UI.Button VRButton;
    public UnityEngine.UI.Button DesktopButton;
    public void Start()
    {
        VRButton.onClick.AddListener(OnVRButton);
        DesktopButton.onClick.AddListener(OnDesktopButton);
    }
    public void OnDesktopButton()
    {
        BasisDeviceManagement.Instance.SwitchMode(BasisBootedMode.Desktop);
    }
    public void OnVRButton()
    {
        BasisDeviceManagement.Instance.SwitchMode(BasisBootedMode.OpenVRLoader);
    }
}
