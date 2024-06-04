using System.Threading.Tasks;
using UnityEngine.UI;
public class BasisHamburgerMenu : BasisUIBase
{
    public Button Settings;
    public Button AvatarButton;
    public Button CloseUI;
    public static string AddressableID = "MainMenu";
    public static BasisHamburgerMenu Instance;
    public void Initialize()
    {
        Instance = this;
        Settings.onClick.AddListener(SettingsPanel);
        AvatarButton.onClick.AddListener(AvatarButtonPanel);
        CloseUI.onClick.AddListener(CloseThisMenu);
        OnCloseMenu.AddListener(CallbackCloseThisMenu);
    }

    private void AvatarButtonPanel()
    {
        CloseThisMenu();
    }

    public async void SettingsPanel()
    {
        await BasisSettingsPanelMenu.OpenThisMenu(BasisSettingsPanelMenu.SettingsPanel);
        CloseThisMenu();
    }
    public void CallbackCloseThisMenu()
    {
        if (BasisAvatarEyeInput.Instance != null)
        {
            BasisAvatarEyeInput.Instance.HandleEscape();

        }
    }
    public async static void OpenMenu()
    {
        await BasisHamburgerMenu.OpenHamburgerMenu();
    }
    public static async Task OpenHamburgerMenu()
    {
        await OpenThisMenu(AddressableID);
    }
}