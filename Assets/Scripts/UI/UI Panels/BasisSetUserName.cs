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
    public async void hasUserName()
    {
        if (string.IsNullOrEmpty(UserNameTMP_InputField.text) == false)
        {
            Ready.interactable = false;
            BasisLocalPlayer.Instance.DisplayName = UserNameTMP_InputField.text;
            if (BasisNetworkConnector.Instance != null)
            {
                Debug.Log("connecting to default");
                await BasisSceneLoadDriver.LoadScene("GardenScene");
                BasisNetworkConnector.Instance.Connect();
                BasisUIComponent[] Components = FindObjectsByType<BasisUIComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (BasisUIComponent Component in Components)
                {
                    GameObject.Destroy(Component.gameObject);
                }
            }
        }
        else
        {
            Debug.LogError("Name was empty bailing");
        }
    }
}