using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;
public class BasisHamburgerMenu : BasisUIBase
{
    public Button Settings;
    public Button AvatarButton;
    public Button CloseUI;
    public Button FullBody;
    public static string MainMenuAddressableID = "MainMenu";
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
    private Dictionary<BasisInput, Action> triggerDelegates = new Dictionary<BasisInput, Action>();

    public void PutIntoCalibrationMode()
    {
        BasisBootedMode BasisBootedMode = BasisDeviceManagement.Instance.CurrentMode;
        if (OverrideForceCalibration || BasisBootedMode == BasisBootedMode.OpenVRLoader || BasisBootedMode == BasisBootedMode.OpenXRLoader)
        {
            BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTpose();

            foreach (BasisInput BasisInput in BasisDeviceManagement.Instance.AllInputDevices)
            {
                Action triggerDelegate = () => OnTriggerChanged(BasisInput);
                triggerDelegates[BasisInput] = triggerDelegate;
                BasisInput.InputState.OnTriggerChanged += triggerDelegate;
            }
        }
    }

    public void OnTriggerChanged(BasisInput FiredOff)
    {
        if (FiredOff.InputState.Trigger >= 0.9f)
        {
            foreach (var entry in triggerDelegates)
            {
                entry.Key.InputState.OnTriggerChanged -= entry.Value;
            }
            triggerDelegates.Clear();

            BasisLocalPlayer.Instance.RecalculateMyHeight();
            BasisAvatarIKStageCalibration.FullBodyCalibration();
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
        AddressableGenericResource resource = new AddressableGenericResource(MainMenuAddressableID, AddressableExpectedResult.SingleItem);
        await OpenThisMenu(resource);
    }
    public static void OpenHamburgerMenuNow()
    {
        AddressableGenericResource resource = new AddressableGenericResource(MainMenuAddressableID, AddressableExpectedResult.SingleItem);
        OpenMenuNow(resource);
    }
}