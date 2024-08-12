using System.Collections;
using UnityEngine;
using UnityEngine.XR;

namespace Valve.VR
{
    public class SteamVR_Behaviour : MonoBehaviour
    {
        private const string openVRDeviceName = "OpenVR";
        public static bool forcingInitialization = false;
        [HideInInspector]
        public SteamVR_Render steamvr_render;

        internal static bool isPlaying = false;

        private static bool initializing = false;
        public static void Initialize(SteamVR_Render renderInstance, SteamVR_Behaviour behaviourInstance, bool forceUnityVRToOpenVR = false)
        {
            if (initializing == false)
            {
                initializing = true;
                if (forceUnityVRToOpenVR)
                    forcingInitialization = true;

                behaviourInstance.steamvr_render = renderInstance;
                initializing = false;
            }
        }

        protected void Awake()
        {
            isPlaying = true;
            if (forcingInitialization == false)
                InitializeSteamVR(steamvr_render, this);
        }

        public void InitializeSteamVR(SteamVR_Render renderInstance, SteamVR_Behaviour behaviourInstance, bool forceUnityVRToOpenVR = false)
        {
            if (forceUnityVRToOpenVR)
            {
                forcingInitialization = true;

                if (initializeCoroutine != null)
                    StopCoroutine(initializeCoroutine);

                if (XRSettings.loadedDeviceName == openVRDeviceName)
                    EnableOpenVR();
                else
                    initializeCoroutine = StartCoroutine(DoInitializeSteamVR(forceUnityVRToOpenVR));
            }
            else
            {
                SteamVR.Initialize(renderInstance, behaviourInstance, false);
            }
        }

        private Coroutine initializeCoroutine;
        private bool loadedOpenVRDeviceSuccess = false;
        private IEnumerator DoInitializeSteamVR(bool forceUnityVRToOpenVR = false)
        {
            XRDevice.deviceLoaded += XRDevice_deviceLoaded;
            XRSettings.LoadDeviceByName(new string[1] { openVRDeviceName });
            while (loadedOpenVRDeviceSuccess == false)
            {
                yield return null;
            }
            XRDevice.deviceLoaded -= XRDevice_deviceLoaded;
            EnableOpenVR();
        }

        private void XRDevice_deviceLoaded(string deviceName)
        {
            if (deviceName == openVRDeviceName)
            {
                loadedOpenVRDeviceSuccess = true;
            }
            else
            {
                Debug.LogError("<b>[SteamVR]</b> Tried to async load: " + openVRDeviceName + ". Loaded: " + deviceName, this);
                loadedOpenVRDeviceSuccess = true; //try anyway
            }
        }

        private void EnableOpenVR()
        {
            SteamVR.Initialize(steamvr_render, this, false);
            initializeCoroutine = null;
            forcingInitialization = false;
        }

#if UNITY_EDITOR
        //only stop playing if the unity editor is running
        private void OnDestroy()
        {
            isPlaying = false;
        }
#endif
        protected void OnEnable()
        {
		    Application.onBeforeRender += OnBeforeRender;
            SteamVR_Events.System(EVREventType.VREvent_Quit).Listen(OnQuit);
        }
        protected void OnDisable()
        {
		    Application.onBeforeRender -= OnBeforeRender;
            SteamVR_Events.System(EVREventType.VREvent_Quit).Remove(OnQuit);
        }
	    protected void OnBeforeRender()
        {
            PreCull();
        }

        protected static int lastFrameCount = -1;
        protected void PreCull()
        {
            if (OpenVR.Input != null)
            {
                // Only update poses on the first camera per frame.
                if (Time.frameCount != lastFrameCount)
                {
                    lastFrameCount = Time.frameCount;

                    SteamVR_Input.OnPreCull();
                }
            }
        }

        protected void FixedUpdate()
        {
            if (OpenVR.Input != null)
            {
                SteamVR_Input.FixedUpdate();
            }
        }

        protected void LateUpdate()
        {
            if (OpenVR.Input != null)
            {
                SteamVR_Input.LateUpdate();
            }
        }

        protected void Update()
        {
            if (OpenVR.Input != null)
            {
                SteamVR_Input.Update();
            }
        }

        protected void OnQuit(VREvent_t vrEvent)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		    Application.Quit();
#endif
        }
    }
}
