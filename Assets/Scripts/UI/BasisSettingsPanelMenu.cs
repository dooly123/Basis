using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasisSettingsPanelMenu : BasisUIBase
{
    public static string SettingsPanel = "SettingsPanel";
    public Button Connect;
    public TMP_InputField IP;
    public TMP_InputField Port;
    public TMP_InputField Password;
    public BasisNetworkConnector NetworkConnector;
    public string IPDefault = "localhost";
    public ushort PortDefault = 4296;
    public string PassWordDefault = "Default";
    public ushort SelectedPort = 4296;
    public void OnEnable()
    {
        IP.contentType = TMP_InputField.ContentType.Standard;
        Port.contentType = TMP_InputField.ContentType.IntegerNumber;
        Password.contentType = TMP_InputField.ContentType.Password;

        NetworkConnector = BasisNetworkConnector.Instance;

        IP.text = IPDefault;
        Port.text = PortDefault.ToString();
        Password.text = PassWordDefault;

        Connect.onClick.AddListener(ConnectTOServer);
    }
    public void ConnectTOServer()
    {
        if (Port != null)
        {
            if (ushort.TryParse(Port.text, out SelectedPort))
            {

            }
            else
            {
                Debug.LogError("Submitted Port was not parsable until type Ushort");
            }
        }
        if (Password != null)
        {
            NetworkConnector.authenticationCode = Password.text;
        }
        if (IP != null)
        {
            NetworkConnector.Connect(SelectedPort, IP.text);
        }
    }
}