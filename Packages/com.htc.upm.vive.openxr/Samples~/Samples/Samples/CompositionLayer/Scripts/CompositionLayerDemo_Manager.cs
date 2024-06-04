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

public class CompositorLayerDemo_Manager : MonoBehaviour
{
	public Camera hmd, srcRTCamera;
	public Texture texture1024;
	public Material RTContentMaterial;
	public RenderTexture srcRTCameraRenderTexture;
	public int srcCameraRTDimension = 1024;
	public Transform layerContentAnchor;

	protected const string LOG_TAG = "CompositorLayerDemo";

	// Start is called before the first frame update
	protected virtual void Start()
    {
        if (hmd == null)
		{
			hmd = Camera.main;
		}
	}

	public void SetSrcCameraRT(CompositionLayer RTLayer)
	{
		if (srcRTCamera.targetTexture != null) srcRTCamera.targetTexture.Release();

		srcRTCamera.orthographicSize = 0.5f; //Scale 1/2
		srcRTCamera.aspect = 1f; //1:1 Aspect Ratio

		if (srcRTCamera.targetTexture == null)
		{
			srcRTCameraRenderTexture = new RenderTexture(srcCameraRTDimension, srcCameraRTDimension, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			srcRTCameraRenderTexture.hideFlags = HideFlags.DontSave;
			srcRTCameraRenderTexture.useMipMap = false;
			srcRTCameraRenderTexture.filterMode = FilterMode.Trilinear;
			srcRTCameraRenderTexture.anisoLevel = 4;
			srcRTCameraRenderTexture.autoGenerateMips = false;

			srcRTCameraRenderTexture.Create();

			srcRTCamera.targetTexture = srcRTCameraRenderTexture;
		}

		RTLayer.texture = srcRTCameraRenderTexture;

		RTContentMaterial.mainTexture = srcRTCameraRenderTexture;
	}
}
