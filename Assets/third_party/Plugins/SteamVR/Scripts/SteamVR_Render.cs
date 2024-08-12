//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Handles rendering of all SteamVR_Cameras
//
//=============================================================================
using UnityEngine;
using System.Collections;
namespace Valve.VR
{
    public class SteamVR_Render : MonoBehaviour
    {
        public static SteamVR_Render instance { get { return SteamVR_Behaviour.instance.steamvr_render; } }
        void OnApplicationQuit()
        {
            SteamVR.SafeDispose();
        }

        public TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        public TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];

        static private bool _pauseRendering;
        static public bool pauseRendering
        {
            get { return _pauseRendering; }
            set
            {
                _pauseRendering = value;

                var compositor = OpenVR.Compositor;
                if (compositor != null)
                    compositor.SuspendRendering(value);
            }
        }

        private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        private IEnumerator RenderLoop()
        {
            while (Application.isPlaying)
            {
                yield return waitForEndOfFrame;

                if (pauseRendering)
                    continue;

                var compositor = OpenVR.Compositor;
                if (compositor != null)
                {
                    if (!compositor.CanRenderScene())
                        continue;

                    compositor.SetTrackingSpace(SteamVR.settings.trackingSpace);
                }

                var overlay = SteamVR_Overlay.instance;
                if (overlay != null)
                    overlay.UpdateOverlay();
            }
        }
        private void OnInputFocus(bool hasFocus)
        {
            if (SteamVR.active == false)
                return;
        }
        private void OnRequestScreenshot(VREvent_t vrEvent)
        {
        }

        private EVRScreenshotType[] screenshotTypes = new EVRScreenshotType[] { EVRScreenshotType.StereoPanorama };

        private void OnEnable()
        {
            StartCoroutine(RenderLoop());
            SteamVR_Events.InputFocus.Listen(OnInputFocus);
            SteamVR_Events.System(EVREventType.VREvent_RequestScreenshot).Listen(OnRequestScreenshot);

            Application.onBeforeRender += OnBeforeRender;

            if (SteamVR.initializedState == SteamVR.InitializedStates.InitializeSuccess)
                OpenVR.Screenshots.HookScreenshot(screenshotTypes);
            else
                SteamVR_Events.Initialized.AddListener(OnSteamVRInitialized);
        }

        private void OnSteamVRInitialized(bool success)
        {
            if (success)
                OpenVR.Screenshots.HookScreenshot(screenshotTypes);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            SteamVR_Events.InputFocus.Remove(OnInputFocus);
            SteamVR_Events.System(EVREventType.VREvent_RequestScreenshot).Remove(OnRequestScreenshot);

            Application.onBeforeRender -= OnBeforeRender;

            if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
                SteamVR_Events.Initialized.RemoveListener(OnSteamVRInitialized);
        }

        public void UpdatePoses()
        {
            var compositor = OpenVR.Compositor;
            if (compositor != null)
            {
                compositor.GetLastPoses(poses, gamePoses);
                SteamVR_Events.NewPoses.Send(poses);
                SteamVR_Events.NewPosesApplied.Send();
            }
        }

        void OnBeforeRender()
        {
            if (SteamVR.active == false)
                return;

            if (SteamVR.settings.IsPoseUpdateMode(SteamVR_UpdateModes.OnPreCull))
            {
                UpdatePoses();
            }
        }

        void Update()
        {
            if (SteamVR.active == false)
                return;

            // Dispatch any OpenVR events.
            var system = OpenVR.System;
            if (system == null)
                return;

            UpdatePoses();

            var vrEvent = new VREvent_t();
            var size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));
            for (int i = 0; i < 64; i++)
            {
                if (!system.PollNextEvent(ref vrEvent, size))
                    break;

                switch ((EVREventType)vrEvent.eventType)
                {
                    case EVREventType.VREvent_InputFocusCaptured: // another app has taken focus (likely dashboard)
                        if (vrEvent.data.process.oldPid == 0)
                        {
                            SteamVR_Events.InputFocus.Send(false);
                        }
                        break;
                    case EVREventType.VREvent_InputFocusReleased: // that app has released input focus
                        if (vrEvent.data.process.pid == 0)
                        {
                            SteamVR_Events.InputFocus.Send(true);
                        }
                        break;
                    case EVREventType.VREvent_ShowRenderModels:
                        SteamVR_Events.HideRenderModels.Send(false);
                        break;
                    case EVREventType.VREvent_HideRenderModels:
                        SteamVR_Events.HideRenderModels.Send(true);
                        break;
                    default:
                        SteamVR_Events.System((EVREventType)vrEvent.eventType).Send(vrEvent);
                        break;
                }
            }
        }
    }
}