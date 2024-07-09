using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
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
    private Dictionary<BasisInput, Action> TriggerDelegates = new Dictionary<BasisInput, Action>();

    public void PutIntoCalibrationMode()
    {
        BasisBootedMode BasisBootedMode = BasisDeviceManagement.Instance.CurrentMode;
        if (OverrideForceCalibration || BasisBootedMode == BasisBootedMode.OpenVRLoader || BasisBootedMode == BasisBootedMode.OpenXRLoader)
        {
            BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTPose();

            foreach (BasisInput BasisInput in BasisDeviceManagement.Instance.AllInputDevices)
            {
                Action triggerDelegate = () => OnTriggerChanged(BasisInput);
                TriggerDelegates[BasisInput] = triggerDelegate;
                BasisInput.InputState.OnTriggerChanged += triggerDelegate;
            }
        }
    }

    public void OnTriggerChanged(BasisInput FiredOff)
    {
        if (FiredOff.InputState.Trigger >= 0.9f)
        {
            foreach (var entry in TriggerDelegates)
            {
                entry.Key.InputState.OnTriggerChanged -= entry.Value;
            }
            TriggerDelegates.Clear();
            StartCalibration();
        }
    }
    public static void StartCalibration()
    {
        BasisLocalPlayer.Instance.RecalculateMyHeight();
        foreach(BasisInput Input in BasisDeviceManagement.Instance.AllInputDevices)
        {
            Input.UnAssignTracker();
        }
        //disable builder and it will be updated when the animator updates
        BasisLocalPlayer.Instance.AvatarDriver.Builder.enabled = false;
        //now lets grab and apply the height
        BasisLocalPlayer.Instance.LocalBoneDriver.Simulate();
        BasisLocalPlayer.Instance.LocalBoneDriver.ApplyMovement();
        //now that we have latest * scale we can run calibration
        BasisAvatarIKStageCalibration.FullBodyCalibration();
        BasisLocalPlayer.Instance.AvatarDriver.AnimatorDriver.AssignHipsFBTracker();
        BasisLocalPlayer.Instance.AvatarDriver.Builder.enabled = true;
    }
#if UNITY_EDITOR
        [MenuItem("Basis/CalibrateFB")]
    public static void CalibrateEditor()
    {
        StartCalibration();
    }
#endif
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