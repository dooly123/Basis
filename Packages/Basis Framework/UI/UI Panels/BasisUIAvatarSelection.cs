using Basis.Scripts.BasisSdk.Players;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Basis.Scripts.UI.UI_Panels
{
    public class BasisUIAvatarSelection : BasisUIBase
    {
        [SerializeField]
        public List<AvatarLoadRequest> AvatarUrls = new List<AvatarLoadRequest>();
        public RectTransform ParentedAvatarButtons;
        public GameObject ButtonPrefab; // Prefab for the button
        public const string AvatarSelection = "BasisUIAvatarSelection";
        public TMP_InputField AddAvatarInputField;
        public Button AddAvatarApply;
        private List<AvatarLoadRequest> AvatarUrlsRuntime = new List<AvatarLoadRequest>();
        public void Start()
        {
            AddAvatarApply.onClick.AddListener(AddAvatar);
            Initialize();
        }
        public override void InitalizeEvent()
        {
            BasisCursorManagement.UnlockCursor(nameof(BasisUIAvatarSelection));
        }
        [System.Serializable]
        public struct AvatarLoadRequest
        {
            public string AvatarAddress;
            //LoadModeNetworkDownloadable = 0; LoadModeLocal = 1;
            public byte IsLocalLoad;
        }
        public void AddAvatar()
        {
            if(string.IsNullOrEmpty(AddAvatarInputField.text) == false)
            {
                BasisUIAvatarRequest.Save(AddAvatarInputField.text);
                Initialize();
            }
            else
            {
                Debug.LogError("tried to add a empty or null avatar!");
            }
        }
        public List<GameObject> CreatedCopies = new List<GameObject>();
        public void Initialize()
        {
            foreach (GameObject go in CreatedCopies)
            {
               GameObject.Destroy(go);
            }
            AvatarUrlsRuntime.Clear();
            CreatedCopies.Clear();
            AvatarUrlsRuntime.AddRange(AvatarUrls);
            BasisUIAvatarRequest.LoadAllAvatars();
            int AvatarUrlsCount = BasisUIAvatarRequest.LocallyStoredAvatarUrls.Count;
            for (int Index = 0; Index < AvatarUrlsCount; Index++)
            {
                string Url = BasisUIAvatarRequest.LocallyStoredAvatarUrls[Index];
                AvatarLoadRequest AvatarLoadRequest = new AvatarLoadRequest
                {
                    AvatarAddress = Url,
                    IsLocalLoad = 0
                };
                AvatarUrlsRuntime.Add(AvatarLoadRequest);
            }
            for (int Index = 0; Index < AvatarUrlsRuntime.Count; Index++)
            {
                AvatarLoadRequest url = AvatarUrlsRuntime[Index];
                // Create a new button from the prefab
                GameObject buttonObject = Instantiate(ButtonPrefab);
                buttonObject.transform.SetParent(ParentedAvatarButtons, false);
                buttonObject.SetActive(true);
                // Get the Button component and set its onClick listener
                if (buttonObject.TryGetComponent<Button>(out Button button))
                {
                    string avatarUrl = url.AvatarAddress; // Capture the url in the local variable for the lambda
                    button.onClick.AddListener(() => OnButtonPressed(url));

                    // Optionally set the button's label to something meaningful, like the URL or a part of it
                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = Path.GetFileNameWithoutExtension(avatarUrl); // or some other meaningful name
                    }
                }
                CreatedCopies.Add(buttonObject);
            }
        }
        public async void OnButtonPressed(AvatarLoadRequest AvatarLoadRequest)
        {
            if (BasisLocalPlayer.Instance != null)
            {
                await BasisLocalPlayer.Instance.CreateAvatar(AvatarLoadRequest.AvatarAddress, AvatarLoadRequest.IsLocalLoad, new BasisBundleInformation());
            }
        }

        public override void DestroyEvent()
        {
            BasisCursorManagement.LockCursor(nameof(BasisUIAvatarSelection));
        }
    }
}