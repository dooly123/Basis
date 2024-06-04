// "VIVE SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the VIVE SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using UnityEngine;
using VIVE.OpenXR;

public class MyFoveatedTest : MonoBehaviour
{
	private float FOVLarge = 57;
    private float FOVSmall = 19;
    private float FOVMiddle = 38;
	
    public static XrFoveationConfigurationHTC config_left, config_right;
    public static XrFoveationConfigurationHTC[] configs = { config_left, config_right };

    MyFoveatedTest()
    {
        configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
        configs[0].clearFovDegree = FOVLarge;
        configs[0].focalCenterOffset.x = 0.0f;
        configs[0].focalCenterOffset.y = 0.0f;
        configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
        configs[1].clearFovDegree = FOVLarge;
        configs[1].focalCenterOffset.x = 0.0f;
        configs[1].focalCenterOffset.y = 0.0f;
    }

    public void FoveationIsDisable()
	{
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_DISABLE_HTC, 0, null);
    }

	public void FoveationIsEnable()
	{
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_FIXED_HTC, 0, null);
    }

    public void FoveationIsDynamic()
    {
        UInt64 flags = ViveFoveation.XR_FOVEATION_DYNAMIC_CLEAR_FOV_ENABLED_BIT_HTC |
            ViveFoveation.XR_FOVEATION_DYNAMIC_FOCAL_CENTER_OFFSET_ENABLED_BIT_HTC |
            ViveFoveation.XR_FOVEATION_DYNAMIC_LEVEL_ENABLED_BIT_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_DYNAMIC_HTC, 0, null, flags);
    }

    public void LeftClearVisionFOVHigh()
	{
        configs[0].clearFovDegree = FOVLarge;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void LeftClearVisionFOVLow()
	{
        configs[0].clearFovDegree = FOVSmall;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void LeftClearVisionFOVMiddle()
	{
        configs[0].clearFovDegree = FOVMiddle;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void LeftEyePeripheralQualityHigh()
	{
        configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void LeftEyePeripheralQualityLow()
	{
        configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_LOW_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void LeftEyePeripheralQualityMiddle()
	{
        configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_MEDIUM_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightClearVisionFOVHigh()
	{
        configs[1].clearFovDegree = FOVLarge;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightClearVisionFOVLow()
	{
        configs[1].clearFovDegree = FOVSmall;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightClearVisionFOVMiddle()
	{
        configs[1].clearFovDegree = FOVMiddle;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightEyePeripheralQualityHigh()
	{
        configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightEyePeripheralQualityLow()
	{
        configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_LOW_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightEyePeripheralQualityMedium()
	{
        configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_MEDIUM_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }
}
