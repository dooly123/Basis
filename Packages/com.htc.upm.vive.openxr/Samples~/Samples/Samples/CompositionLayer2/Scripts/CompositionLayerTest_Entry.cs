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
using UnityEngine.InputSystem;

public class CompositionLayerTest_Entry : MonoBehaviour
{
    [SerializeField]
    InputActionAsset m_ActionAsset;
    public InputActionAsset actionAsset
    {
        get => m_ActionAsset;
        set => m_ActionAsset = value;
    }
    private void OnEnable()
    {
        if (m_ActionAsset != null)
        {
            m_ActionAsset.Enable();
        }
    }

    public void LaunchQuadOverlayScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(3);
    }

	public void LaunchQuadUnderlayScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(4);
	}

	public void LaunchCylinderOverlayScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(1);
	}

	public void LaunchCylinderUnderlayScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(2);
	}

	public void LaunchCanvasOverlayScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(5);
	}

	public void LaunchCanvasUnderlayScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(6);
	}

    public void LaunchColorScaleBiasScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(7);
    }

    public void LaunchTrackingOriginScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(8);
    }
}
