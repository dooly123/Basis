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
        public Image Loadingbar;
        public bool HasActiveLoadingbar = false;
        public Button AdvancedSettings;
        public GameObject AdvancedSettingsPanel;
        [Header("Advanced Settings")]
        public TMP_InputField IPaddress;
        public TMP_InputField Port;
        public TMP_InputField Password;
        public void Start()
        {
            UserNameTMP_InputField.text = BasisDataStore.LoadString(LoadFileName, string.Empty);
            Ready.onClick.AddListener(hasUserName);
            if (AdvancedSettingsPanel != null)
            {
                AdvancedSettings.onClick.AddListener(ToggleAdvancedSettings);
            }
            BasisSceneLoadDriver.progressCallback += ProgresReport;
            BasisNetworkConnector.OnExists += LoadCurrentSettings;
        }
        public void LoadCurrentSettings()
        {
            IPaddress.text = BasisNetworkConnector.Instance.Ip;
            Port.text = BasisNetworkConnector.Instance.Port.ToString();
            Password.text = "basis18072024"; //BasisNetworkConnector.Instance.Client.LiteNetLibConnnection.authenticationKey;
        }
        public void OnDestroy()
        {
            if (AdvancedSettingsPanel != null)
            {
                AdvancedSettings.onClick.RemoveListener(ToggleAdvancedSettings);
            }
            BasisSceneLoadDriver.progressCallback -= ProgresReport;
        }

        private void ProgresReport(float progress)
        {
            if (HasActiveLoadingbar == false)
            {
                StartProgressBar();
                UpdateProgressBar(progress);
            }
            else
            {
                UpdateProgressBar(progress);
                if (progress == 100)
                {
                    StopProgressBar();
                }
            }
        }
        public void StartProgressBar()
        {
            Loadingbar.gameObject.SetActive(true);
            HasActiveLoadingbar = true;
        }
        public void UpdateProgressBar(float progress)
        {
            Loadingbar.rectTransform.localScale = new Vector3(progress / 100, 1f, 1f);
        }
        public void StopProgressBar()
        {
            Loadingbar.gameObject.SetActive(false);
            HasActiveLoadingbar = false;
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
                    BasisNetworkConnector.Instance.Ip = IPaddress.text;
                    ushort.TryParse(Port.text, out BasisNetworkConnector.Instance.Port);
                  //  BasisNetworkConnector.Instance.Client.LiteNetLibConnnection.authenticationKey = Password.text;

                    BasisNetworkConnector.Instance.Connect();
                    Ready.interactable = false;
                    Debug.Log("connecting to default");
                    if (UseAddressables)
                    {
                        await BasisSceneLoadDriver.LoadSceneAddressables(SceneToLoad);
                    }
                    else
                    {
                        await BasisSceneLoadDriver.LoadSceneAssetBundle(SceneToLoad);
                    }
                    BasisUIComponent[] Components = FindObjectsByType<BasisUIComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (BasisUIComponent Component in Components)
                    {
                        Destroy(Component.gameObject);
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
        public void ToggleAdvancedSettings()
        {
            if (AdvancedSettingsPanel != null)
            {
                AdvancedSettingsPanel.SetActive(!AdvancedSettingsPanel.activeSelf);
            }
        }
    }
}