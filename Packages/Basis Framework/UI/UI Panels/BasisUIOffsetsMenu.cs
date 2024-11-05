using Basis.Scripts.Device_Management;
using Basis.Scripts.Device_Management.Devices;
using Basis.Scripts.UI.UI_Panels;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasisUIOffsetsMenu : BasisUIBase
{
    public TMP_InputField XPosRayCast;
    public TMP_InputField YPosRayCast;
    public TMP_InputField ZPosRayCast;
    public TMP_InputField XRotRayCast;
    public TMP_InputField YRotRayCast;
    public TMP_InputField ZRotRayCast;

    public TMP_InputField XPosAvatar;
    public TMP_InputField YPosAvatar;
    public TMP_InputField ZPosAvatar;
    public TMP_InputField XRotAvatar;
    public TMP_InputField YRotAvatar;
    public TMP_InputField ZRotAvatar;

    public Button XPosRayCastAddButton;
    public Button XPosRayCastSubtractButton;
    public Button YPosRayCastAddButton;
    public Button YPosRayCastSubtractButton;
    public Button ZPosRayCastAddButton;
    public Button ZPosRayCastSubtractButton;
    public Button XRotRayCastAddButton;
    public Button XRotRayCastSubtractButton;
    public Button YRotRayCastAddButton;
    public Button YRotRayCastSubtractButton;
    public Button ZRotRayCastAddButton;
    public Button ZRotRayCastSubtractButton;

    public Button XPosAvatarAddButton;
    public Button XPosAvatarSubtractButton;
    public Button YPosAvatarAddButton;
    public Button YPosAvatarSubtractButton;
    public Button ZPosAvatarAddButton;
    public Button ZPosAvatarSubtractButton;
    public Button XRotAvatarAddButton;
    public Button XRotAvatarSubtractButton;
    public Button YRotAvatarAddButton;
    public Button YRotAvatarSubtractButton;
    public Button ZRotAvatarAddButton;
    public Button ZRotAvatarSubtractButton;

    public Button Apply;
    public TMP_Dropdown SelectableConfigs;
    private const float IncrementValue = 0.01f;
    private BasisDeviceNameMatcher Matched;
    private int currentIndex = 0;

    public override void DestroyEvent()
    {
    }

    public override void InitalizeEvent()
    {
        // Set up callback events for each button
        XPosRayCastAddButton.onClick.AddListener(() => IncrementValueInField(XPosRayCast));
        XPosRayCastSubtractButton.onClick.AddListener(() => DecrementValueInField(XPosRayCast));
        YPosRayCastAddButton.onClick.AddListener(() => IncrementValueInField(YPosRayCast));
        YPosRayCastSubtractButton.onClick.AddListener(() => DecrementValueInField(YPosRayCast));
        ZPosRayCastAddButton.onClick.AddListener(() => IncrementValueInField(ZPosRayCast));
        ZPosRayCastSubtractButton.onClick.AddListener(() => DecrementValueInField(ZPosRayCast));
        XRotRayCastAddButton.onClick.AddListener(() => IncrementValueInField(XRotRayCast));
        XRotRayCastSubtractButton.onClick.AddListener(() => DecrementValueInField(XRotRayCast));
        YRotRayCastAddButton.onClick.AddListener(() => IncrementValueInField(YRotRayCast));
        YRotRayCastSubtractButton.onClick.AddListener(() => DecrementValueInField(YRotRayCast));
        ZRotRayCastAddButton.onClick.AddListener(() => IncrementValueInField(ZRotRayCast));
        ZRotRayCastSubtractButton.onClick.AddListener(() => DecrementValueInField(ZRotRayCast));

        XPosAvatarAddButton.onClick.AddListener(() => IncrementValueInField(XPosAvatar));
        XPosAvatarSubtractButton.onClick.AddListener(() => DecrementValueInField(XPosAvatar));
        YPosAvatarAddButton.onClick.AddListener(() => IncrementValueInField(YPosAvatar));
        YPosAvatarSubtractButton.onClick.AddListener(() => DecrementValueInField(YPosAvatar));
        ZPosAvatarAddButton.onClick.AddListener(() => IncrementValueInField(ZPosAvatar));
        ZPosAvatarSubtractButton.onClick.AddListener(() => DecrementValueInField(ZPosAvatar));
        XRotAvatarAddButton.onClick.AddListener(() => IncrementValueInField(XRotAvatar));
        XRotAvatarSubtractButton.onClick.AddListener(() => DecrementValueInField(XRotAvatar));
        YRotAvatarAddButton.onClick.AddListener(() => IncrementValueInField(YRotAvatar));
        YRotAvatarSubtractButton.onClick.AddListener(() => DecrementValueInField(YRotAvatar));
        ZRotAvatarAddButton.onClick.AddListener(() => IncrementValueInField(ZRotAvatar));
        ZRotAvatarSubtractButton.onClick.AddListener(() => DecrementValueInField(ZRotAvatar));

        // Initialize dropdown options and set default
        SelectableConfigs.ClearOptions();
        List<string> Options = new List<string>();
        Matched = BasisDeviceManagement.Instance.BasisDeviceNameMatcher;

        foreach (BasisDeviceMatchSettings Settings in Matched.BasisDevice)
        {
            Options.Add(Settings.DeviceID);
        }

        SelectableConfigs.AddOptions(Options);
        SelectableConfigs.onValueChanged.AddListener(onValueChangedDropdown);
        Apply.onClick.AddListener(ApplyBack);

        // Display the first device config by default
        if (Options.Count > 0)
        {
            onValueChangedDropdown(0);
        }
    }

    public void ApplyBack()
    {
        if (currentIndex >= 0 && currentIndex < Matched.BasisDevice.Count)
        {
            BasisDeviceMatchSettings currentSettings = Matched.BasisDevice[currentIndex];

            // Parse and apply the updated values from input fields
            currentSettings.AvatarPositionOffset = new Vector3(
                float.Parse(XPosRayCast.text),
                float.Parse(YPosRayCast.text),
                float.Parse(ZPosRayCast.text)
            );

            currentSettings.AvatarRotationOffset = new Vector3(
                float.Parse(XRotRayCast.text),
                float.Parse(YRotRayCast.text),
                float.Parse(ZRotRayCast.text)
            );

            currentSettings.PositionRayCastOffset = new Vector3(
                float.Parse(XPosAvatar.text),
                float.Parse(YPosAvatar.text),
                float.Parse(ZPosAvatar.text)
            );

            currentSettings.RotationRaycastOffset = new Vector3(
                float.Parse(XRotAvatar.text),
                float.Parse(YRotAvatar.text),
                float.Parse(ZRotAvatar.text)
            );
            /*
            BasisDeviceMatchSettings Match = BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedDeviceMatchableNamesNoCreate(CommonDeviceIdentifier,);
            foreach (BasisInput Input in BasisDeviceManagement.Instance.AllInputDevices)
            {
                if (Input.CommonDeviceIdentifier == currentSettings.DeviceID)
                {
                    Input.AvatarPositionOffset = currentSettings.AvatarPositionOffset;
                    Input.AvatarRotationOffset = Quaternion.Euler(currentSettings.AvatarRotationOffset);
                }
            }
            */
        }
    }

    private void onValueChangedDropdown(int arg0)
    {
        currentIndex = arg0;

        // Fetch settings for the selected device
        BasisDeviceMatchSettings settings = Matched.BasisDevice[arg0];

        // Populate the input fields with current settings
        XPosRayCast.text = settings.AvatarPositionOffset.x.ToString();
        YPosRayCast.text = settings.AvatarPositionOffset.y.ToString();
        ZPosRayCast.text = settings.AvatarPositionOffset.z.ToString();

        XRotRayCast.text = settings.AvatarRotationOffset.x.ToString();
        YRotRayCast.text = settings.AvatarRotationOffset.y.ToString();
        ZRotRayCast.text = settings.AvatarRotationOffset.z.ToString();

        XPosAvatar.text = settings.PositionRayCastOffset.x.ToString();
        YPosAvatar.text = settings.PositionRayCastOffset.y.ToString();
        ZPosAvatar.text = settings.PositionRayCastOffset.z.ToString();

        XRotAvatar.text = settings.RotationRaycastOffset.x.ToString();
        YRotAvatar.text = settings.RotationRaycastOffset.y.ToString();
        ZRotAvatar.text = settings.RotationRaycastOffset.z.ToString();
    }

    private void IncrementValueInField(TMP_InputField inputField)
    {
        if (float.TryParse(inputField.text, out float currentValue))
        {
            inputField.text = (currentValue + IncrementValue).ToString();
        }
    }

    private void DecrementValueInField(TMP_InputField inputField)
    {
        if (float.TryParse(inputField.text, out float currentValue))
        {
            inputField.text = (currentValue - IncrementValue).ToString();
        }
    }
}