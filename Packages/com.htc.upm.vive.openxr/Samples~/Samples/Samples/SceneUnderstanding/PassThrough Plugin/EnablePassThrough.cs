using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HVR
{
    public class EnablePassThrough : MonoBehaviour
    {
        EHVRErrorCode hvrErr;
        EHVRSettingsError settingErr;
        //IHVRSettings_FnTable *vr_settings = (IHVRSettings_FnTable*) HVR_GetGenericInterface(IHVRSettings_Version, &hvrErr);
        public bool enable = true;
        public int blend_mode = 0;
        private int origin_blend_mode = 4;
        // Start is called before the first frame update
        private CHVRSettings vr_settings;

        private void Awake() {
            HVRClass.Init(ref hvrErr);
            vr_settings = HVRClass.Settings;
        }
        void Start()
        {
            if(vr_settings != null)
            {
                origin_blend_mode = vr_settings.GetInt32("camera", "cameraRoomViewMode", ref settingErr);
                Debug.Log("cameraRoomViewMode: " + origin_blend_mode);
                // Change exposure mode
                vr_settings.SetBool("camera", "cameraRoomViewAlwaysOn", enable, ref settingErr);
                if (settingErr == EHVRSettingsError.EVRSettingsError_None)
                {
                    Debug.Log("Enable pass through successfully."); // Enable pass through successfully
                } else
                {
                    Debug.Log("Enable pass through failed: " + settingErr);
                }

                vr_settings.SetInt32("camera", "cameraRoomViewMode", blend_mode, ref settingErr);
                if (settingErr == EHVRSettingsError.EVRSettingsError_None)
                {
                    Debug.Log("Set camera mode successfully."); // Set camera mode successfully
                } else
                {
                    Debug.Log("Set camera mode failed: " + settingErr);
                }
            } else
            {
                Debug.Log("vr_settings is null.");
            }
        }

        private void OnApplicationQuit()
        {
            if(vr_settings != null)
            {
                // Disable pass through before closing the App.
                vr_settings.SetBool("camera", "cameraRoomViewAlwaysOn", !enable, ref settingErr);
                if (settingErr == EHVRSettingsError.EVRSettingsError_None)
                {
                    Debug.Log("Disable pass through.");
                }

                vr_settings.SetInt32("camera", "cameraRoomViewMode", origin_blend_mode, ref settingErr);
                if (settingErr == EHVRSettingsError.EVRSettingsError_None)
                {
                    Debug.Log("Reset cameraRoomViewMode: " + origin_blend_mode);
                }
            }
        }
    }
}