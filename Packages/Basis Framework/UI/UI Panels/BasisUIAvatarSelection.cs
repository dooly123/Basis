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
        public void Start()
        {
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
        public void Initialize()
        {
            for (int Index = 0; Index < AvatarUrls.Count; Index++)
            {
                AvatarLoadRequest url = AvatarUrls[Index];
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
            }
        }
        public async void OnButtonPressed(AvatarLoadRequest AvatarLoadRequest)
        {
            if (BasisLocalPlayer.Instance != null)
            {
                await BasisLocalPlayer.Instance.CreateAvatar(AvatarLoadRequest.AvatarAddress, AvatarLoadRequest.IsLocalLoad, string.Empty);
            }
        }

        public override void DestroyEvent()
        {
            BasisCursorManagement.LockCursor(nameof(BasisUIAvatarSelection));
        }
    }
}