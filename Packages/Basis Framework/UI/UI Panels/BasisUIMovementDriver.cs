using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using UnityEngine;

namespace Basis.Scripts.UI.UI_Panels
{
    public class BasisUIMovementDriver : MonoBehaviour
    {
        public BasisLocalPlayer LocalPlayer;
        public Vector3 WorldOffset = new Vector3(0, 0, 0.5f);
        public bool hasLocalCreationEvent = false;
        public Vector3 Position;
        public Quaternion Rotation;
        public void OnEnable()
        {
            LocalPlayer = BasisLocalPlayer.Instance;
            if (BasisLocalPlayer.Instance != null)
            {
                LocalPlayerGenerated();
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
            BasisLocalPlayer.Instance.OnPlayersHeightChanged += StartWaitAndSetUILocation;
            SetUILocation();
        }
        public void OnDisable()
        {
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
                Debug.Log("StartWaitAndSetUILocation");
                SetUILocation();
            }
        }
        public void SetUILocation()
        {
            BasisLocalCameraDriver.GetPositionAndRotation(out Position, out Rotation);
            Debug.Log("Scale was " + LocalPlayer.RatioPlayerToEyeDefaultScale);
            transform.SetPositionAndRotation(Position + (Rotation * (WorldOffset * LocalPlayer.RatioPlayerToEyeDefaultScale)), Rotation);
        }
    }
}