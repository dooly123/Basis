using Assets.Scripts.BasisSdk.Players;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.UI_Panels
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

    public void Initialize()
    {
        foreach (string url in AvatarUrls)
        {
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
                    buttonText.text = avatarUrl; // or some other meaningful name
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
}
}