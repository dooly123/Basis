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
    public async static void OpenMenu()
    {
        await BasisHamburgerMenu.OpenHamburgerMenu();
    }
    public static async Task OpenHamburgerMenu()
    {
        await OpenThisMenu(AddressableID);
    }
}