// "VIVE SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the VIVE SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using UnityEngine.InputSystem;

namespace VIVE.OpenXR.Samples
{
    public class ActionAssetActivator : MonoBehaviour
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
    }
}
