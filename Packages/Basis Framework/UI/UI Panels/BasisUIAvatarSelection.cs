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
        public List<BasisLoadableBundle> AvatarUrls = new List<BasisLoadableBundle>();
        public RectTransform ParentedAvatarButtons;
        public GameObject ButtonPrefab; // Prefab for the button
        public const string AvatarSelection = "BasisUIAvatarSelection";
        public TMP_InputField AddAvatarInputField;
        public Button AddAvatarApply;
        private List<BasisLoadableBundle> AvatarUrlsRuntime = new List<BasisLoadableBundle>();
        public void Start()
        {
            AddAvatarApply.onClick.AddListener(AddAvatar);
            Initialize();
        }
        public override void InitalizeEvent()
        {
            BasisCursorManagement.UnlockCursor(nameof(BasisUIAvatarSelection));
        }
        public void AddAvatar()
        {
            if (string.IsNullOrEmpty(AddAvatarInputField.text) == false)
            {
                //HERE LD  BasisUIAvatarRequest.Save(AddAvatarInputField.text);
                Initialize();
            }
            else
            {
                Debug.LogError("tried to add a empty or null avatar!");
            }
        }
        public List<GameObject> CreatedCopies = new List<GameObject>();
        public async void Initialize()
        {
            foreach (GameObject go in CreatedCopies)
            {
                GameObject.Destroy(go);
            }
            AvatarUrlsRuntime.Clear();
            CreatedCopies.Clear();
            AvatarUrlsRuntime.AddRange(AvatarUrls);
            await BasisBundleManagement.FigureOutExistingContent();
         //   int AvatarUrlsCount = BasisBundleManagement.UnLoadedBundles.Count;
          //  for (int Index = 0; Index < AvatarUrlsCount; Index++)
            {
                // string AvatarBundleAddress = BasisUIAvatarRequest.LocallyStoredAvatarUrls[Index];
                // BasisLoadableBundle AvatarLoadRequest = new BasisLoadableBundle
                // {
                //      AvatarBundleAddress = AvatarBundleAddress,
                //    IsLocalLoad = 0
                //};
                // AvatarUrlsRuntime.Add(AvatarLoadRequest);
            }
            for (int Index = 0; Index < AvatarUrlsRuntime.Count; Index++)
            {
                BasisLoadableBundle url = AvatarUrlsRuntime[Index];
                // Create a new button from the prefab
                GameObject buttonObject = Instantiate(ButtonPrefab);
                buttonObject.transform.SetParent(ParentedAvatarButtons, false);
                buttonObject.SetActive(true);
                // Get the Button component and set its onClick listener
                if (buttonObject.TryGetComponent<Button>(out Button button))
                {
                    // string avatarUrl = url.AvatarAddress; // Capture the url in the local variable for the lambda
                    //  button.onClick.AddListener(() => OnButtonPressed(url));

                    // Optionally set the button's label to something meaningful, like the URL or a part of it
                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        //  buttonText.text = Path.GetFileNameWithoutExtension(avatarUrl); // or some other meaningful name
                    }
                }
                CreatedCopies.Add(buttonObject);
            }
        }
        public async void OnButtonPressed(BasisBundleInformation AvatarLoadRequest)
        {
            if (BasisLocalPlayer.Instance != null)
            {
                //    await BasisLocalPlayer.Instance.CreateAvatar(AvatarLoadRequest.AvatarAddress, AvatarLoadRequest.IsLocalLoad, new BasisBundleInformation());
            }
        }

        public override void DestroyEvent()
        {
            BasisCursorManagement.LockCursor(nameof(BasisUIAvatarSelection));
        }
    }
}