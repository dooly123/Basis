// "VIVE SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the VIVE SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIVE.OpenXR.CompositionLayer;

public class OverlayDemo_Manager : CompositorLayerDemo_Manager
{
	public CompositionLayer cameraRTOverlay, staticOverlay;

	protected override void Start()
    {
		base.Start();
		SetSrcCameraRT(cameraRTOverlay);

        staticOverlay.texture = texture1024;
    }
}
