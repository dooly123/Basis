using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

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
        // Set button to non-interactable immediately after clicking
        Ready.interactable = false;

        if (!string.IsNullOrEmpty(UserNameTMP_InputField.text))
        {
            BasisLocalPlayer.Instance.DisplayName = UserNameTMP_InputField.text;

            if (BasisNetworkConnector.Instance != null)
            {
                BasisNetworkConnector.Instance.Connect();

                BasisUIComponent[] Components = FindObjectsByType<BasisUIComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (BasisUIComponent Component in Components)
                {
                    Destroy(Component.gameObject);
                }
                Debug.Log("connecting to default");
                await BasisSceneLoadDriver.LoadScene("GardenScene");
            }
        }
        else
        {
            Debug.LogError("Name was empty, bailing");
            // Re-enable button interaction if username is empty
            Ready.interactable = true;
        }
    }
}