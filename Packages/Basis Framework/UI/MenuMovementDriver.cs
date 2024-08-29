using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Device_Management;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using System.Collections;
using UnityEngine;

namespace Basis.Scripts.UI.UI_Panels
{
    public class MenuMovementDriver : MonoBehaviour
    {
        [SerializeField] private BasisUIBase UIBase;
        private BasisLocalPlayer LocalPlayer;
        private BasisLocalCameraDriver CameraDriver;

        [SerializeField] private Vector3 menuPosOffset;
        [SerializeField] private Vector3 menuRotOffset;
        public float FallBackMultiplier = 0.35f;
        private enum MenuType
        {
            Hand,
            Floating
        }

        [SerializeField, Tooltip("Controls whether menu will be attached to hand or float in front of player")] 
        private MenuType menuType = MenuType.Floating;

        #region Menu Generation Events

        private bool hasGeneratedAction = false;
        
        private void OnEnable()
        {
            LocalPlayer = BasisLocalPlayer.Instance;
            CameraDriver = BasisLocalCameraDriver.Instance;

            if (BasisLocalPlayer.Instance != null)
            {
                OnLocalPlayerGenerated();
                StartCoroutine(DelaySetUI());
            }
            else
            {
                if (hasGeneratedAction == false)
                {
                    BasisLocalPlayer.OnLocalPlayerCreated += OnLocalPlayerGenerated;
                    hasGeneratedAction = true;
                }
            }

            LocalPlayer.LocalBoneDriver.ReadyToRead += UpdateUI;
        }

        private void OnDisable()
        {
            if (hasGeneratedAction)
            {
                BasisLocalPlayer.OnLocalPlayerCreated -= OnLocalPlayerGenerated;
                hasGeneratedAction = false;
            }
            
            BasisLocalPlayer.Instance.OnLocalAvatarChanged -= OnLocalAvatarChanged;
            BasisLocalPlayer.Instance.OnPlayersHeightChanged -= OnPlayersHeightChanged;
            
            LocalPlayer.LocalBoneDriver.ReadyToRead -= UpdateUI;
        }

        #endregion

        #region Player Change Callbacks
        public BasisBoneControl hand;
        private void OnLocalPlayerGenerated()
        {
            BasisLocalPlayer.Instance.OnLocalAvatarChanged += OnLocalAvatarChanged;
            BasisLocalPlayer.Instance.OnPlayersHeightChanged += OnPlayersHeightChanged;
            if (LocalPlayer.LocalBoneDriver.FindBone(out  hand, BasisBoneTrackedRole.LeftHand))
            {
            }
        }

        private void OnPlayersHeightChanged()
        {
            StartCoroutine(DelaySetUI());
        }

        private void OnLocalAvatarChanged()
        {
            StartCoroutine(DelaySetUI());
        }

        #endregion

        #region UI Position Updating

        private IEnumerator DelaySetUI() // Waits until end of frame to set position, to ensure all other data has been updated
        {
            yield return null;
            SetUIPositionAndRotation();
        }

        private void UpdateUI()
        {
            if (menuType != MenuType.Hand || BasisDeviceManagement.IsUserInDesktop())
            {
                return;
            }
            
            SetUIPositionAndRotation();
        }

        public void SetUIPositionAndRotation()
        {
            switch (menuType)
            {
                case MenuType.Floating:

                    Vector3 newPos = CameraDriver.Camera.transform.forward; // Get Cam Forward Disr

                    Vector3 projectedPos = Vector3.ProjectOnPlane(newPos, LocalPlayer.transform.up).normalized; // Project that along the player's local up dir

                    newPos = LocalPlayer.transform.position + (projectedPos * (0.5f * LocalPlayer.RatioPlayerToEyeDefaultScale)) + menuPosOffset; // Adjust for player scale and add offset

                    transform.position = newPos; // Assign

                    transform.rotation = Quaternion.LookRotation(projectedPos) * Quaternion.Euler(menuRotOffset); // Use projected pos and multiply by euler rot offset
                    break;
                case MenuType.Hand:
                    
                    if (BasisDeviceManagement.IsUserInDesktop() == false)
                    {
                        transform.position = hand.BoneTransform.position + (menuPosOffset * LocalPlayer.RatioPlayerToEyeDefaultScale);

                        transform.rotation = hand.BoneTransform.rotation * Quaternion.Euler(menuRotOffset);
                    }
                    else
                    {
                        transform.position = CameraDriver.Camera.transform.position + (CameraDriver.Camera.transform.forward * FallBackMultiplier);
                        transform.rotation = CameraDriver.Camera.transform.rotation;
                    }
                    break;
            }
        }

        #endregion
    }
}
