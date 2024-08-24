using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Basis.Scripts.Drivers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Common;
using Basis.Scripts.Networking;

namespace Basis.Scripts.UI.UI_Panels
{
    public class BasisSetUserName : MonoBehaviour
    {
        public TMP_InputField UserNameTMP_InputField;
        public Button Ready;
        public static string LoadFileName = "CachedUserName.BAS";
        public string SceneToLoad;
        public bool UseAddressables;
        public void Start()
        {
            UserNameTMP_InputField.text = BasisDataStore.LoadString(LoadFileName, string.Empty);
            Ready.onClick.AddListener(hasUserName);
        }

        public async void hasUserName()
        {
            // Set button to non-interactable immediately after clicking
            Ready.interactable = false;

            if (!string.IsNullOrEmpty(UserNameTMP_InputField.text))
            {
                BasisLocalPlayer.Instance.DisplayName = UserNameTMP_InputField.text;
                BasisDataStore.SaveString(BasisLocalPlayer.Instance.DisplayName, LoadFileName);
                if (BasisNetworkConnector.Instance != null)
                {
                    BasisNetworkConnector.Instance.Connect();

                    BasisUIComponent[] Components = FindObjectsByType<BasisUIComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (BasisUIComponent Component in Components)
                    {
                        Destroy(Component.gameObject);
                    }
                    Debug.Log("connecting to default");
                    if (UseAddressables)
                    {
                        await BasisSceneLoadDriver.LoadSceneAssetBundle(SceneToLoad);
                    }
                    else
                    {
                        await BasisSceneLoadDriver.LoadSceneAssetBundle(SceneToLoad);
                    }
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
}