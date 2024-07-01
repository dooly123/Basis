using System.Threading.Tasks;
using UnityEngine.UI;
public class BasisHamburgerMenu : BasisUIBase
{
    public Button Settings;
    public Button AvatarButton;
    public Button CloseUI;
    public Button FullBody;
    public static string AddressableID = "MainMenu";
    public static BasisHamburgerMenu Instance;
    public bool OverrideForceCalibration;
    public void Initialize()
    {
        Instance = this;
        Settings.onClick.AddListener(SettingsPanel);
        AvatarButton.onClick.AddListener(AvatarButtonPanel);
        CloseUI.onClick.AddListener(CloseThisMenu);
        FullBody.onClick.AddListener(PutIntoCalibrationMode);
    }
    public void PutIntoCalibrationMode()
    {
        BasisBootedMode BasisBootedMode = BasisDeviceManagement.Instance.CurrentMode;
        if (OverrideForceCalibration || BasisBootedMode == BasisBootedMode.OpenVRLoader || BasisBootedMode == BasisBootedMode.OpenXRLoader)
        {
            BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTpose();
        }
    }
    private static void AvatarButtonPanel()
    {
        BasisHamburgerMenu.Instance.CloseThisMenu();
        AddressableGenericResource resource = new AddressableGenericResource(BasisUIAvatarSelection.AvatarSelection, AddressableExpectedResult.SingleItem);
        BasisSettingsPanelMenu.OpenMenuNow(resource);
    }

    public static void SettingsPanel()
    {
        BasisHamburgerMenu.Instance.CloseThisMenu();
        AddressableGenericResource resource = new AddressableGenericResource(BasisSettingsPanelMenu.SettingsPanel, AddressableExpectedResult.SingleItem);
        BasisSettingsPanelMenu.OpenMenuNow(resource);
    }
    public static async Task OpenHamburgerMenu()
    {
        AddressableGenericResource resource = new AddressableGenericResource(AddressableID, AddressableExpectedResult.SingleItem);
        await OpenThisMenu(resource);
    }
    public static void OpenHamburgerMenuNow()
    {
        AddressableGenericResource resource = new AddressableGenericResource(AddressableID, AddressableExpectedResult.SingleItem);
        OpenMenuNow(resource);
    }
}