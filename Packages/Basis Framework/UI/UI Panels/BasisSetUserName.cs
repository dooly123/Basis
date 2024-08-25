using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Basis.Scripts.Drivers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Common;
using Basis.Scripts.Networking;
using System;

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
        public void Start()
        {
            UserNameTMP_InputField.text = BasisDataStore.LoadString(LoadFileName, string.Empty);
            Ready.onClick.AddListener(hasUserName);
            BasisSceneLoadDriver.progressCallback += ProgresReport;
        }
        public void OnDestroy()
        {
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
    }
}