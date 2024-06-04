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
using UnityEngine.XR;
using UnityEngine.UI;
using VIVE.OpenXR.CompositionLayer;
using UnityEngine.XR.OpenXR;
using UnityEngine.InputSystem;

public class CompositionLayerTest_Manager : MonoBehaviour
{
    [SerializeField]
    InputActionAsset m_ActionAsset;
    public InputActionAsset actionAsset
    {
        get => m_ActionAsset;
        set => m_ActionAsset = value;
    }


    public GameObject layerAnchorGO, mainCameraGO, playerRigGO;
	public Text maxLayerCountText, currentLayerCountText, fallbackStatusText;
	public GameObject contentLayerTextureGameObjectRef, compositionLayerGameObjectRef;
	protected List<GameObject> contentLayerTextureGameObjects, compositionLayerGameObjects;
	public float furthestDistance = 7f;
	public float nearestDistance = 1f;
	public string mainScenePath = "";

	private CompositionLayerManager compositionLayerManagerInstance = null;
    private ViveCompositionLayer compositionLayerFeature = null;
    //private GameObject LayerContentGOSet = null;
	private float analogDetectionThreshold = 0.7f;
	private Vector3 layerAnchorOriginalPosition, layerAnchorOriginalRotation;
	private bool compositionLayerActive = true;

	private const string LOG_TAG = "CompositionLayerTest";

    static void DEBUG(string msg) { Debug.Log(LOG_TAG + " " + msg); }
    static void WARNING(string msg) { Debug.LogWarning(LOG_TAG + " " + msg); }
    static void ERROR(string msg) { Debug.LogError(LOG_TAG + " " + msg); }

    // Start is called before the first frame update
    void Start()
    {
		layerAnchorOriginalPosition = layerAnchorGO.transform.position;
		layerAnchorOriginalRotation = layerAnchorGO.transform.eulerAngles;

        //AddLayer();
        compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();
    }

    // Update is called once per frame
    void Update()
    {
		if (compositionLayerManagerInstance == null)
		{
			compositionLayerManagerInstance = CompositionLayerManager.GetInstance();
		}

		LayerTranslationCheck();
		LayerRotationCheck();

		UpdateMaxLayerCountText();
		UpdateCurrentLayerCountText();
		UpdateAutoFallbackStatusText();
	}

    private void OnEnable()
    {
        if (m_ActionAsset != null)
        {
            m_ActionAsset.Enable();
        }
    }

    public void ToggleWorldHeadLock(bool headlock)
	{
		if (headlock)
		{
            layerAnchorGO.transform.SetParent(mainCameraGO.transform);
		}
		else
		{
            layerAnchorGO.transform.SetParent(null);
        }
	}

	public void ResetLayer()
	{
		layerAnchorGO.transform.position = layerAnchorOriginalPosition;
		layerAnchorGO.transform.eulerAngles = layerAnchorOriginalRotation;
	}

	public virtual void AddLayer()
	{
		if (contentLayerTextureGameObjects == null)
		{
			contentLayerTextureGameObjects = new List<GameObject>();
		}

		if (compositionLayerGameObjects == null)
		{
			compositionLayerGameObjects = new List<GameObject>();
		}

		if (contentLayerTextureGameObjects != null && compositionLayerGameObjects != null)
		{
			GameObject newContentLayerTextureGameObjectInstance = Instantiate(contentLayerTextureGameObjectRef);
			GameObject newCompositionLayerGameObjectInstance = Instantiate(compositionLayerGameObjectRef);
			newCompositionLayerGameObjectInstance.name = newCompositionLayerGameObjectInstance.name + " " + compositionLayerGameObjects.Count;

			CompositionLayer compositionLayerComponent = newCompositionLayerGameObjectInstance.GetComponentInChildren<CompositionLayer>();
			if (compositionLayerComponent != null)
			{
				compositionLayerComponent.SetRenderPriority((uint)compositionLayerGameObjects.Count);
                compositionLayerComponent.compositionDepth = (uint)compositionLayerGameObjects.Count;
                compositionLayerComponent.trackingOrigin = playerRigGO;
            }

			GameObject newObjectContainer = new GameObject("Layer Content");
			RectTransform newObjectContainerTransform = newObjectContainer.AddComponent<RectTransform>();
			newObjectContainerTransform.sizeDelta = new Vector2(1,0);
			newObjectContainer.transform.SetParent(layerAnchorGO.transform);

			newObjectContainer.transform.localPosition = Vector3.zero;
			newObjectContainer.transform.localRotation = Quaternion.identity;

			newContentLayerTextureGameObjectInstance.transform.SetParent(newObjectContainer.transform);
			newCompositionLayerGameObjectInstance.transform.SetParent(newObjectContainer.transform);

			newContentLayerTextureGameObjectInstance.transform.localPosition = Vector3.zero;
			newContentLayerTextureGameObjectInstance.transform.localRotation = Quaternion.identity;

			newCompositionLayerGameObjectInstance.transform.localPosition = Vector3.zero;
			newCompositionLayerGameObjectInstance.transform.localRotation = Quaternion.identity;

			contentLayerTextureGameObjects.Add(newContentLayerTextureGameObjectInstance);
			compositionLayerGameObjects.Add(newCompositionLayerGameObjectInstance);

			if (compositionLayerActive)
			{
				newCompositionLayerGameObjectInstance.SetActive(true);
				newContentLayerTextureGameObjectInstance.SetActive(false);
			}
			else
			{
				newCompositionLayerGameObjectInstance.SetActive(false);
				newContentLayerTextureGameObjectInstance.SetActive(true);
			}
		}
	}

	public void RemoveLayer()
	{
		if (contentLayerTextureGameObjects.Count > 0 && compositionLayerGameObjects.Count > 0)
		{
			Transform parentTransform = contentLayerTextureGameObjects[contentLayerTextureGameObjects.Count - 1].transform.parent;

			Destroy(contentLayerTextureGameObjects[contentLayerTextureGameObjects.Count - 1]);
			Destroy(compositionLayerGameObjects[compositionLayerGameObjects.Count - 1]);

			Destroy(parentTransform.gameObject);

			contentLayerTextureGameObjects.RemoveAt(contentLayerTextureGameObjects.Count - 1);
			compositionLayerGameObjects.RemoveAt(compositionLayerGameObjects.Count - 1);
		}
	}

	public void SwitchLayer()
	{
		//Toggle content layer and composition layer
		if (!compositionLayerActive)
		{
			DEBUG("Switch to composition layer");
			foreach (GameObject contentLayerTextureGO in contentLayerTextureGameObjects)
			{
				contentLayerTextureGO.SetActive(false);
			}

			foreach (GameObject compositionLayerGO in compositionLayerGameObjects)
			{
				compositionLayerGO.SetActive(true);
			}
			compositionLayerActive = true;
		}
		else
		{
			DEBUG("Switch to content layer");
			foreach (GameObject contentLayerTextureGO in contentLayerTextureGameObjects)
			{
				contentLayerTextureGO.SetActive(true);
			}

			foreach (GameObject compositionLayerGO in compositionLayerGameObjects)
			{
				compositionLayerGO.SetActive(false);
			}
			compositionLayerActive = false;
		}
	}

	private void UpdateMaxLayerCountText()
	{
		if (compositionLayerManagerInstance != null && maxLayerCountText != null)
		{
			maxLayerCountText.text = "Max Layer Count: " + compositionLayerManagerInstance.MaxLayerCount();
		}
	}

	private void UpdateCurrentLayerCountText()
	{
		if (compositionLayerManagerInstance != null && currentLayerCountText != null)
		{
			currentLayerCountText.text = "Current Layer Count: " + compositionLayerManagerInstance.CurrentLayerCount();
		}
	}

	private void UpdateAutoFallbackStatusText()
	{
		if (compositionLayerManagerInstance != null && fallbackStatusText != null)
		{
            compositionLayerFeature = OpenXRSettings.Instance.GetFeature<ViveCompositionLayer>();
            fallbackStatusText.text = "Autofall back enabled: " + compositionLayerFeature.enableAutoFallback.ToString();
		}
	}

	void LayerTranslationCheck()
	{
        float L_TS_Y_State = KeyAxis2D(kControllerLeftCharacteristics, UnityEngine.XR.CommonUsages.secondary2DAxis).y;
		float R_TS_Y_State = KeyAxis2D(kControllerRightCharacteristics, UnityEngine.XR.CommonUsages.secondary2DAxis).y;
        float L_TP_Y_State = KeyAxis2D(kControllerLeftCharacteristics, UnityEngine.XR.CommonUsages.primary2DAxis).y;
        float R_TP_Y_State = KeyAxis2D(kControllerRightCharacteristics, UnityEngine.XR.CommonUsages.primary2DAxis).y;

        //DEBUG("L_TS_Y_State: " + L_TS_Y_State + " R_TS_Y_State: " + R_TS_Y_State + " L_TP_Y_State: " + L_TP_Y_State + " R_TP_Y_State: " + R_TP_Y_State);

        if (L_TS_Y_State > analogDetectionThreshold ||
			R_TS_Y_State > analogDetectionThreshold ||
			L_TP_Y_State > analogDetectionThreshold ||
			R_TP_Y_State > analogDetectionThreshold)
		{
			//DEBUG("Button Axis: Y Positive");
			if (layerAnchorGO.transform.position.z < furthestDistance)
			{
				//DEBUG("Layer translation: Further");
				Vector3 targetLayerPosition = new Vector3(layerAnchorGO.transform.position.x, layerAnchorGO.transform.position.y, Mathf.Min(layerAnchorGO.transform.position.z + 0.025f, furthestDistance));
				layerAnchorGO.transform.position = targetLayerPosition;
			}
		}
		else if (L_TS_Y_State < -analogDetectionThreshold ||
				 R_TS_Y_State < -analogDetectionThreshold ||
				 L_TP_Y_State < -analogDetectionThreshold ||
				 R_TP_Y_State < -analogDetectionThreshold)
		{
			//DEBUG("Button Axis: Y Negative");
			if (layerAnchorGO.transform.position.z > nearestDistance)
			{
				//DEBUG("Layer translation: Nearer");
				Vector3 targetLayerPosition = new Vector3(layerAnchorGO.transform.position.x, layerAnchorGO.transform.position.y, Mathf.Max(layerAnchorGO.transform.position.z - 0.025f, nearestDistance));
				layerAnchorGO.transform.position = targetLayerPosition;
			}
		}
	}

	void LayerRotationCheck()
	{
		float L_TS_X_State = KeyAxis2D(kControllerLeftCharacteristics, UnityEngine.XR.CommonUsages.secondary2DAxis).x;
        float R_TS_X_State = KeyAxis2D(kControllerRightCharacteristics, UnityEngine.XR.CommonUsages.secondary2DAxis).x;
        float L_TP_X_State = KeyAxis2D(kControllerLeftCharacteristics, UnityEngine.XR.CommonUsages.primary2DAxis).x;
        float R_TP_X_State = KeyAxis2D(kControllerRightCharacteristics, UnityEngine.XR.CommonUsages.primary2DAxis).x;

        //DEBUG("L_TS_X_State: " + L_TS_X_State + " R_TS_X_State: " + R_TS_X_State + " L_TP_X_State: " + L_TP_X_State + " R_TP_X_State: " + R_TP_X_State);

        if (L_TS_X_State > analogDetectionThreshold ||
			R_TS_X_State > analogDetectionThreshold ||
			L_TP_X_State > analogDetectionThreshold ||
			R_TP_X_State > analogDetectionThreshold)
		{
			//DEBUG("Button Axis: X Positive");
			Vector3 targetLayerRotation = new Vector3(layerAnchorGO.transform.rotation.eulerAngles.x, layerAnchorGO.transform.rotation.eulerAngles.y + 2.5f, layerAnchorGO.transform.rotation.eulerAngles.z);
			layerAnchorGO.transform.eulerAngles = targetLayerRotation;
		}
		else if (L_TS_X_State < -analogDetectionThreshold ||
				 R_TS_X_State < -analogDetectionThreshold ||
				 L_TP_X_State < -analogDetectionThreshold ||
				 R_TP_X_State < -analogDetectionThreshold)
		{
			//DEBUG("Button Axis: X Negative");
			Vector3 targetLayerRotation = new Vector3(layerAnchorGO.transform.rotation.eulerAngles.x, layerAnchorGO.transform.rotation.eulerAngles.y - 2.5f, layerAnchorGO.transform.rotation.eulerAngles.z);
			layerAnchorGO.transform.eulerAngles = targetLayerRotation;
		}
	}

	public void PrevScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
	}

    #region Unity XR Buttons
    internal static List<UnityEngine.XR.InputDevice> m_InputDevices = new List<UnityEngine.XR.InputDevice>();
    private static Vector2 KeyAxis2D(InputDeviceCharacteristics device, InputFeatureUsage<Vector2> button)
    {
        Vector2 axis2d = Vector2.zero;

        InputDevices.GetDevices(m_InputDevices);
        foreach (UnityEngine.XR.InputDevice id in m_InputDevices)
        {
            // The device is connected.
            if (id.characteristics.Equals(device))
            {
                if (id.TryGetFeatureValue(button, out Vector2 value))
                {
                    axis2d = value;
                }
            }
        }

        return axis2d;
    }

    /// <summary> VIVE Left Controller Characteristics </summary>
    private const InputDeviceCharacteristics kControllerLeftCharacteristics = (
        InputDeviceCharacteristics.Left |
        InputDeviceCharacteristics.TrackedDevice |
        InputDeviceCharacteristics.Controller |
        InputDeviceCharacteristics.HeldInHand
    );
    /// <summary> VIVE Right Controller Characteristics </summary>
    private const InputDeviceCharacteristics kControllerRightCharacteristics = (
        InputDeviceCharacteristics.Right |
        InputDeviceCharacteristics.TrackedDevice |
        InputDeviceCharacteristics.Controller |
        InputDeviceCharacteristics.HeldInHand
    );
    #endregion
}
