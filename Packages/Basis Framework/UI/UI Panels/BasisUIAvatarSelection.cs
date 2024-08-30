using Basis.Scripts.Addressable_Driver.Loading;
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
        public List<string> AvatarUrls = new List<string>();
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
        public void Initialize()
        {
            for (int Index = 0; Index < AvatarUrls.Count; Index++)
            {
                string url = AvatarUrls[Index];
                // Create a new button from the prefab
                GameObject buttonObject = Instantiate(ButtonPrefab);
                buttonObject.transform.SetParent(ParentedAvatarButtons, false);
                buttonObject.SetActive(true);
                // Get the Button component and set its onClick listener
                if (buttonObject.TryGetComponent<Button>(out Button button))
                {
                    string avatarUrl = url; // Capture the url in the local variable for the lambda
                    button.onClick.AddListener(() => OnButtonPressed(avatarUrl));

                    // Optionally set the button's label to something meaningful, like the URL or a part of it
                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = Path.GetFileNameWithoutExtension(avatarUrl); // or some other meaningful name
                    }
                }
            }
        }

        public async void OnButtonPressed(string Url)
        {
            if (BasisLocalPlayer.Instance != null)
            {
                await BasisLocalPlayer.Instance.CreateAvatar(Url);
            }
        }

        public override void DestroyEvent()
        {
            BasisCursorManagement.LockCursor(nameof(BasisUIAvatarSelection));
        }
    }
}