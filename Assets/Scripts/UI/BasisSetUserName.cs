using DarkRift.Server.Plugins.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasisSetUserName : MonoBehaviour
{
    public TMP_InputField UserNameTMP_InputField;
    public Button Ready;
    public void Start()
    {
        Ready.onClick.AddListener(hasUserName);
    }
    public void hasUserName()
    {
        if (string.IsNullOrEmpty(UserNameTMP_InputField.text) == false)
        {
            BasisLocalPlayer.Instance.DisplayName = UserNameTMP_InputField.text;
            if (BasisNetworking.Instance != null)
            {
                BasisNetworkConnector.Instance.Connect();
            }
            GameObject.Destroy(this.gameObject);
        }
    }
}
