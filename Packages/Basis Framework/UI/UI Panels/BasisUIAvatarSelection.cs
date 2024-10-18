using Basis.Scripts.BasisSdk.Players;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Basis.Scripts.UI.UI_Panels
{
    public class BasisUIAvatarSelection : BasisUIBase
    {
        [SerializeField]
        public List<BasisLoadableBundle> preLoadedBundles = new List<BasisLoadableBundle>();
        public RectTransform ParentedAvatarButtons;
        public GameObject ButtonPrefab; // Prefab for the button
        public const string AvatarSelection = "BasisUIAvatarSelection";

        public TMP_InputField MetaField;
        public TMP_InputField BundleField;
        public TMP_InputField PasswordField;

        public Button AddAvatarApply;
        private List<BasisLoadableBundle> AvatarUrlsRuntime = new List<BasisLoadableBundle>();
        public async void Start()
        {
            BasisDataStoreAvatarKeys.DisplayKeys();
            AddAvatarApply.onClick.AddListener(AddAvatar);
            await Initialize();
        }
        public override void InitalizeEvent()
        {
            BasisCursorManagement.UnlockCursor(nameof(BasisUIAvatarSelection));
        }
        public async void AddAvatar()
        {
            bool MetaFieldState = string.IsNullOrEmpty(MetaField.text);
            bool BundleFieldState = string.IsNullOrEmpty(BundleField.text);
            bool PasswordFieldState = string.IsNullOrEmpty(PasswordField.text);
            if (MetaFieldState)
            {
                Debug.LogError("Meta Field was Empty");
                return;
            }
            if (BundleFieldState)
            {
                Debug.LogError("Bundle Field was Empty");
                return;
            }
            if (PasswordFieldState)
            {
                Debug.LogError("Password Field was Empty");
                return;
            }
            BasisLoadableBundle BasisLoadableBundle = new BasisLoadableBundle
            {
                UnlockPassword = PasswordField.text,
                BasisRemoteBundleEncypted = new BasisRemoteEncyptedBundle
                {
                    BundleURL = BundleField.text,
                    MetaURL = MetaField.text
                },
                BasisBundleInformation = new BasisBundleInformation() { BasisBundleDescription = new BasisBundleDescription(), BasisBundleGenerated = new BasisBundleGenerated() },
                BasisStoredEncyptedBundle = new BasisStoredEncyptedBundle(),
            };
            await BasisLocalPlayer.Instance.CreateAvatar(0, BasisLoadableBundle);
            BasisDataStoreAvatarKeys.AvatarKey AvatarKey = new BasisDataStoreAvatarKeys.AvatarKey
            {
                Url = MetaField.text,

                Pass = PasswordField.text
            };
            await BasisDataStoreAvatarKeys.AddNewKey(AvatarKey);

            await Initialize();
        }
        public List<GameObject> CreatedCopies = new List<GameObject>();
        public async Task Initialize()
        {
            foreach (GameObject go in CreatedCopies)
            {
                GameObject.Destroy(go);
            }
            AvatarUrlsRuntime.Clear();
            CreatedCopies.Clear();
            AvatarUrlsRuntime.AddRange(preLoadedBundles);
           await BasisDataStoreAvatarKeys.LoadKeys();
           List<BasisDataStoreAvatarKeys.AvatarKey> ActiveKeys = BasisDataStoreAvatarKeys.DisplayKeys();
           int KeysCount = ActiveKeys.Count;
            for (int Index = 0; Index < KeysCount; Index++)
            {
                BasisLoadhandler.HasURLOnDisc(ActiveKeys[Index].Url,out OnDiscInformation Information);
                BasisLoadableBundle Bundle = new BasisLoadableBundle
                {
                    BasisRemoteBundleEncypted = new BasisRemoteEncyptedBundle
                    {
                        BundleURL = Information.StoredBundleURL,
                        MetaURL = Information.StoredMetaURL
                    },
                     BasisBundleInformation = new BasisBundleInformation() { BasisBundleDescription = new BasisBundleDescription(), BasisBundleGenerated = new BasisBundleGenerated() },

                      BasisStoredEncyptedBundle = new BasisStoredEncyptedBundle() { },

                    UnlockPassword = ActiveKeys[Index].Pass
                };
                AvatarUrlsRuntime.Add(Bundle);
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
                    button.onClick.AddListener(() => OnButtonPressed(url));

                    // Optionally set the button's label to something meaningful, like the URL or a part of it
                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = url.BasisBundleInformation.BasisBundleDescription.AssetBundleName; // or some other meaningful name
                    }
                }
                CreatedCopies.Add(buttonObject);
            }
        }
        public async void OnButtonPressed(BasisLoadableBundle AvatarLoadRequest)
        {
            if (BasisLocalPlayer.Instance != null)
            {
                if (AvatarLoadRequest.BasisRemoteBundleEncypted.IsLocal)
                {
                    await BasisLocalPlayer.Instance.CreateAvatar(0, AvatarLoadRequest);
                }
                else
                {
                    await BasisLocalPlayer.Instance.CreateAvatar(0, AvatarLoadRequest);
                }
            }
        }

        public override void DestroyEvent()
        {
            BasisCursorManagement.LockCursor(nameof(BasisUIAvatarSelection));
        }
    }
}