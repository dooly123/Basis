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
    public Button Reset;

    public TMP_Dropdown JsonDump;
    public override void DestroyEvent() { }

    public override void InitalizeEvent()
    {
        XPosRayCast.contentType = TMP_InputField.ContentType.DecimalNumber;
        YPosRayCast.contentType = TMP_InputField.ContentType.DecimalNumber;
        ZPosRayCast.contentType = TMP_InputField.ContentType.DecimalNumber;
        XRotRayCast.contentType = TMP_InputField.ContentType.DecimalNumber;
        YRotRayCast.contentType = TMP_InputField.ContentType.DecimalNumber;
        ZRotRayCast.contentType = TMP_InputField.ContentType.DecimalNumber;

        XPosAvatar.contentType = TMP_InputField.ContentType.DecimalNumber;
        YPosAvatar.contentType = TMP_InputField.ContentType.DecimalNumber;
        ZPosAvatar.contentType = TMP_InputField.ContentType.DecimalNumber;
        XRotAvatar.contentType = TMP_InputField.ContentType.DecimalNumber;
        YRotAvatar.contentType = TMP_InputField.ContentType.DecimalNumber;
        ZRotAvatar.contentType = TMP_InputField.ContentType.DecimalNumber;

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

        if (Options.Count > 0)
        {
            onValueChangedDropdown(0);
        }
        Reset.onClick.AddListener(NukeConfigAndReload);
    }

    public void ApplyBack()
    {
        if (currentIndex >= 0 && currentIndex < Matched.BasisDevice.Count)
        {
            BasisDeviceMatchSettings BasisDeviceMatchableNames = Matched.BasisDevice[currentIndex];

            BasisDeviceMatchableNames.PositionRayCastOffset = new Vector3(
                ParseOrZero(XPosRayCast.text),
                ParseOrZero(YPosRayCast.text),
                ParseOrZero(ZPosRayCast.text)
            );

            BasisDeviceMatchableNames.RotationRaycastOffset = new Vector3(
                ParseOrZero(XRotRayCast.text),
                ParseOrZero(YRotRayCast.text),
                ParseOrZero(ZRotRayCast.text)
            );

            BasisDeviceMatchableNames.AvatarPositionOffset= new Vector3(
                ParseOrZero(XPosAvatar.text),
                ParseOrZero(YPosAvatar.text),
                ParseOrZero(ZPosAvatar.text)
            );

            BasisDeviceMatchableNames.AvatarRotationOffset = new Vector3(
                ParseOrZero(XRotAvatar.text),
                ParseOrZero(YRotAvatar.text),
                ParseOrZero(ZRotAvatar.text)
            );

            foreach (BasisInput Input in BasisDeviceManagement.Instance.AllInputDevices)
            {
                if (Input.BasisDeviceMatchableNames.DeviceID == BasisDeviceMatchableNames.DeviceID)
                {
                    Input.AvatarPositionOffset = BasisDeviceMatchableNames.AvatarPositionOffset;
                    Input.AvatarRotationOffset = BasisDeviceMatchableNames.AvatarRotationOffset;
                    BasisDebug.Log("loaded for " + Input.UniqueDeviceIdentifier);
                }
            }
            BasisDeviceMatchableNames.VersionNumber = 2;//we always just say 2.
            //1 = default
            //2 = this
            //3 and up is manual config
            BasisDeviceManagement.Instance.LoadAndOrSaveDefaultDeviceConfigs();
            onValueChangedDropdown(currentIndex);
        }
    }
    public void NukeConfigAndReload()
    {
        // Clear and reload BasisDevice list with deep copies
        BasisDeviceManagement.Instance.BasisDeviceNameMatcher.BasisDevice.Clear();
        foreach (var device in BasisDeviceManagement.Instance.BasisDeviceNameMatcher.BackedUpDevices)
        {
            BasisDeviceManagement.Instance.BasisDeviceNameMatcher.BasisDevice.Add(device.Clone());
        }

        // Update all input devices with matched settings using deep copies
        foreach (BasisDeviceMatchSettings match in BasisDeviceManagement.Instance.BasisDeviceNameMatcher.BasisDevice)
        {
            foreach (BasisInput input in BasisDeviceManagement.Instance.AllInputDevices)
            {
                if (input.BasisDeviceMatchableNames.DeviceID == match.DeviceID)
                {
                    input.AvatarPositionOffset = match.AvatarPositionOffset;
                    input.AvatarRotationOffset = match.AvatarRotationOffset;
                    BasisDebug.Log("Loaded for " + input.UniqueDeviceIdentifier);
                    input.BasisDeviceMatchableNames = match; // Use deep clone here
                }
            }
        }
        onValueChangedDropdown(currentIndex);
    }
    private void onValueChangedDropdown(int arg0)
    {
        currentIndex = arg0;

        BasisDeviceMatchSettings settings = Matched.BasisDevice[arg0];

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

    private float ParseOrZero(string text)
    {
        return float.TryParse(string.IsNullOrEmpty(text) ? "0" : text, out float result) ? result : 0f;
    }
}