using Basis.Scripts.BasisSdk.Players;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Basis.Scripts.UI.UI_Panels
{
    public class BasisUIAvatarSelection : BasisUIBase
    {
        [SerializeField] public List<BasisLoadableBundle> preLoadedBundles = new List<BasisLoadableBundle>();
        [SerializeField] public RectTransform ParentedAvatarButtons;
        [SerializeField] public GameObject ButtonPrefab;

        public const string AvatarSelection = "BasisUIAvatarSelection";

        [SerializeField] public TMP_InputField MetaField;
        [SerializeField] public TMP_InputField BundleField;
        [SerializeField] public TMP_InputField PasswordField;

        [SerializeField] public Button AddAvatarApply;
        [SerializeField] public BasisProgressReport.ProgressReport Report;
        [SerializeField]
        public List<BasisLoadableBundle> avatarUrlsRuntime = new List<BasisLoadableBundle>();
        [SerializeField]
        public List<GameObject> createdCopies = new List<GameObject>();

        private async void Start()
        {
            BasisDataStoreAvatarKeys.DisplayKeys();
            AddAvatarApply.onClick.AddListener(AddAvatar);
            await Initialize();
        }

        public override void InitalizeEvent()
        {
            BasisCursorManagement.UnlockCursor(AvatarSelection);
        }

        private async void AddAvatar()
        {
            if (string.IsNullOrEmpty(MetaField.text) || string.IsNullOrEmpty(BundleField.text) || string.IsNullOrEmpty(PasswordField.text))
            {
                Debug.LogError("All fields must be filled.");
                return;
            }

            // Check if the avatar key already exists to prevent duplicates
            List<BasisDataStoreAvatarKeys.AvatarKey> activeKeys = BasisDataStoreAvatarKeys.DisplayKeys();
            bool keyExists = activeKeys.Exists(key => key.Url == MetaField.text && key.Pass == PasswordField.text);

            if (keyExists)
            {
                Debug.LogWarning("The avatar key with the same URL and Password already exists. No duplicate will be added.");
                return;
            }

            BasisLoadableBundle loadableBundle = new BasisLoadableBundle
            {
                UnlockPassword = PasswordField.text,
                BasisRemoteBundleEncypted = new BasisRemoteEncyptedBundle
                {
                    BundleURL = BundleField.text,
                    MetaURL = MetaField.text
                },
                BasisBundleInformation = new BasisBundleInformation
                {
                    BasisBundleDescription = new BasisBundleDescription(),
                    BasisBundleGenerated = new BasisBundleGenerated()
                },
                BasisStoredEncyptedBundle = new BasisStoredEncyptedBundle()
            };

            await BasisLocalPlayer.Instance.CreateAvatar(0, loadableBundle);

            var avatarKey = new BasisDataStoreAvatarKeys.AvatarKey
            {
                Url = MetaField.text,
                Pass = PasswordField.text
            };

            await BasisDataStoreAvatarKeys.AddNewKey(avatarKey);
            await Initialize();
        }

        private async Task Initialize()
        {
            ClearCreatedCopies();
            avatarUrlsRuntime.Clear();
            avatarUrlsRuntime.AddRange(preLoadedBundles);

            await BasisDataStoreAvatarKeys.LoadKeys();
            List<BasisDataStoreAvatarKeys.AvatarKey> activeKeys = BasisDataStoreAvatarKeys.DisplayKeys();
            int Count = activeKeys.Count;
            for (int Index = 0; Index < Count; Index++)
            {
                if (BasisLoadhandler.HasURLOnDisc(activeKeys[Index].Url, out var info) == false)
                {
                    Debug.LogError("Missing File on Disc For " + activeKeys[Index].Url);
                    continue;
                }

                BasisLoadableBundle bundle = new BasisLoadableBundle
                {
                    BasisRemoteBundleEncypted = new BasisRemoteEncyptedBundle
                    {
                        BundleURL = info.StoredBundleURL,
                        MetaURL = info.StoredMetaURL
                    },
                    BasisBundleInformation = new BasisBundleInformation
                    {
                        BasisBundleDescription = new BasisBundleDescription(),
                        BasisBundleGenerated = new BasisBundleGenerated()
                    },
                    BasisStoredEncyptedBundle = new BasisStoredEncyptedBundle(),
                    UnlockPassword = activeKeys[Index].Pass
                };
                Debug.Log("Adding Button");
                avatarUrlsRuntime.Add(bundle);
            }
            Debug.Log("CreateAvatarButtons");
            await CreateAvatarButtons(activeKeys);
        }

        private async Task CreateAvatarButtons(List<BasisDataStoreAvatarKeys.AvatarKey> activeKeys)
        {
            for (int index = 0; index < avatarUrlsRuntime.Count; index++)
            {
                Debug.Log("CreateAvatarButton " + index);
                var bundle = avatarUrlsRuntime[index];
                var buttonObject = Instantiate(ButtonPrefab, ParentedAvatarButtons);
                buttonObject.SetActive(true);

                if (buttonObject.TryGetComponent<Button>(out var button))
                {
                    button.onClick.AddListener(() => OnButtonPressed(bundle));

                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        var wrapper = new BasisTrackedBundleWrapper
                        {
                            LoadableBundle = bundle
                        };

                        BasisLoadhandler.HasURLOnDisc(activeKeys[index].Url, out var info);
                        try
                        {
                            await BasisBundleManagement.DataOnDiscProcessMetaAsync(wrapper, info.StoredMetaLocal, info.StoredBundleLocal, Report, new CancellationToken());
                            buttonText.text = wrapper.LoadableBundle.BasisBundleInformation.BasisBundleDescription.AssetBundleName;
                        }
                        catch (Exception E)
                        {
                            Debug.LogError(E);
                            BasisLoadhandler.TryRemoveOnDiscInfo(activeKeys[index].Url);

                            continue;
                        }
                    }
                }

                createdCopies.Add(buttonObject);
            }
        }
        private void ClearCreatedCopies()
        {
            foreach (var copy in createdCopies)
            {
                Destroy(copy);
            }
            createdCopies.Clear();
        }

        private async void OnButtonPressed(BasisLoadableBundle avatarLoadRequest)
        {
            if (BasisLocalPlayer.Instance != null)
            {
                await BasisLocalPlayer.Instance.CreateAvatar(0, avatarLoadRequest);
            }
        }

        public override void DestroyEvent()
        {
            BasisCursorManagement.LockCursor(AvatarSelection);
        }
    }
}