using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using System.Collections;
using UnityEngine;

namespace Basis.Scripts.UI.UI_Panels
{
    public class BasisUIMovementDriver : MonoBehaviour
    {
        public BasisLocalPlayer LocalPlayer;
        public Vector3 WorldOffset = new Vector3(0, 0, 0.5f);
        public bool hasLocalCreationEvent = false;
        public void OnEnable()
        {
            LocalPlayer = BasisLocalPlayer.Instance;
            if (BasisLocalPlayer.Instance != null)
            {
                LocalPlayerGenerated();
                StartCoroutine(WaitAndSetUILocation());
            }
            else
            {
                if (hasLocalCreationEvent == false)
                {
                    BasisLocalPlayer.OnLocalPlayerCreated += LocalPlayerGenerated;
                    hasLocalCreationEvent = true;
                }
            }
        }
        public void LocalPlayerGenerated()
        {
            BasisLocalPlayer.Instance.OnLocalAvatarChanged += StartWaitAndSetUILocation;
            BasisLocalPlayer.Instance.OnPlayersHeightChanged += StartWaitAndSetUILocation;
        }
        public void OnDisable()
        {
            BasisLocalPlayer.Instance.OnLocalAvatarChanged -= StartWaitAndSetUILocation;
            BasisLocalPlayer.Instance.OnPlayersHeightChanged -= StartWaitAndSetUILocation;
            if (hasLocalCreationEvent)
            {
                BasisLocalPlayer.OnLocalPlayerCreated -= LocalPlayerGenerated;
                hasLocalCreationEvent = false;
            }
        }
        public void StartWaitAndSetUILocation(bool Final)
        {
            if (Final)
            {
                StartWaitAndSetUILocation();
            }
        }
        private void StartWaitAndSetUILocation()
        {
            StartCoroutine(WaitAndSetUILocation());
        }
        private IEnumerator WaitAndSetUILocation()
        {
            yield return null;
            SetUILocation();
        }
        public Vector3 Position;
        public Quaternion Rotation;
        public void SetUILocation()
        {
            BasisLocalCameraDriver.GetPositionAndRotation(out Position, out Rotation);
            Vector3 VRotation = Rotation.eulerAngles;
            //  VRotation = new Vector3(VRotation.x, VRotation.y, 0);
            Quaternion Rot = Quaternion.Euler(VRotation);
            transform.SetPositionAndRotation(Position + Rot * (WorldOffset * LocalPlayer.RatioPlayerToEyeDefaultScale), Rot);
        }
    }
}